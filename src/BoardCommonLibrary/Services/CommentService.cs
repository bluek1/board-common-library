using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 댓글 서비스 구현
/// </summary>
public class CommentService : ICommentService
{
    private readonly BoardDbContext _context;
    
    public CommentService(BoardDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<CommentResponse> CreateAsync(long postId, CreateCommentRequest request, long authorId, string? authorName = null)
    {
        // 게시물 존재 확인
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new InvalidOperationException($"게시물(ID: {postId})을 찾을 수 없습니다.");
        }
        
        var comment = new Comment
        {
            Content = request.Content,
            PostId = postId,
            AuthorId = authorId,
            AuthorName = authorName,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Comments.Add(comment);
        
        // 게시물 댓글 수 증가
        post.CommentCount++;
        
        await _context.SaveChangesAsync();
        
        return MapToResponse(comment);
    }
    
    /// <inheritdoc />
    public async Task<PagedResponse<CommentResponse>> GetByPostIdAsync(long postId, CommentQueryParameters parameters)
    {
        // 부모 댓글만 조회 (ParentId == null)
        var query = _context.Comments
            .Where(c => c.PostId == postId && c.ParentId == null)
            .AsQueryable();
        
        // 전체 개수
        var totalCount = await query.CountAsync();
        
        // 정렬
        IOrderedQueryable<Comment> orderedQuery = parameters.SortBy?.ToLower() switch
        {
            "likecount" => parameters.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.LikeCount)
                : query.OrderBy(c => c.LikeCount),
            _ => parameters.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt)
        };
        
        // 페이징
        var comments = await orderedQuery
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        // 대댓글 포함
        var responses = new List<CommentResponse>();
        foreach (var comment in comments)
        {
            var response = MapToResponse(comment);
            
            if (parameters.IncludeReplies)
            {
                var replies = await _context.Comments
                    .Where(c => c.ParentId == comment.Id)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();
                
                response.Replies = replies.Select(MapToResponse).ToList();
            }
            
            responses.Add(response);
        }
        
        return PagedResponse<CommentResponse>.Create(responses, parameters.Page, parameters.PageSize, totalCount);
    }
    
    /// <inheritdoc />
    public async Task<CommentResponse?> GetByIdAsync(long id)
    {
        var comment = await _context.Comments.FindAsync(id);
        return comment == null ? null : MapToResponse(comment);
    }
    
    /// <inheritdoc />
    public async Task<CommentResponse?> UpdateAsync(long id, UpdateCommentRequest request, long currentUserId)
    {
        var comment = await _context.Comments.FindAsync(id);
        
        if (comment == null)
            return null;
        
        // 권한 확인
        if (comment.AuthorId != currentUserId)
            throw new UnauthorizedAccessException("댓글을 수정할 권한이 없습니다.");
        
        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return MapToResponse(comment);
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, long currentUserId, bool isAdmin = false)
    {
        var comment = await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (comment == null)
            return false;
        
        // 권한 확인
        if (comment.AuthorId != currentUserId && !isAdmin)
            throw new UnauthorizedAccessException("댓글을 삭제할 권한이 없습니다.");
        
        // 대댓글이 있는 경우 내용만 변경
        if (comment.Replies.Any(r => !r.IsDeleted))
        {
            comment.Content = "삭제된 댓글입니다.";
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            // 대댓글이 없는 경우 소프트 삭제
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
        }
        
        // 게시물 댓글 수 감소
        var post = await _context.Posts.FindAsync(comment.PostId);
        if (post != null)
        {
            post.CommentCount = Math.Max(0, post.CommentCount - 1);
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<CommentResponse?> CreateReplyAsync(long parentId, CreateCommentRequest request, long authorId, string? authorName = null)
    {
        var parentComment = await _context.Comments.FindAsync(parentId);
        
        if (parentComment == null)
            return null;
        
        // 대댓글 깊이 제한 (2단계)
        if (parentComment.ParentId != null)
        {
            throw new InvalidOperationException("대댓글에는 답글을 달 수 없습니다. (최대 2단계)");
        }
        
        var reply = new Comment
        {
            Content = request.Content,
            PostId = parentComment.PostId,
            AuthorId = authorId,
            AuthorName = authorName,
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Comments.Add(reply);
        
        // 게시물 댓글 수 증가
        var post = await _context.Posts.FindAsync(parentComment.PostId);
        if (post != null)
        {
            post.CommentCount++;
        }
        
        await _context.SaveChangesAsync();
        
        return MapToResponse(reply);
    }
    
    /// <inheritdoc />
    public async Task<bool> IsAuthorAsync(long commentId, long userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        return comment != null && comment.AuthorId == userId;
    }
    
    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.IsDeleted ? "삭제된 댓글입니다." : comment.Content,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.AuthorName,
            ParentId = comment.ParentId,
            LikeCount = comment.LikeCount,
            IsBlinded = comment.IsBlinded,
            IsDeleted = comment.IsDeleted,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
