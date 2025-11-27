using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 게시물 서비스 구현
/// </summary>
public class PostService : IPostService
{
    private readonly BoardDbContext _context;
    
    public PostService(BoardDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<PagedResponse<PostSummaryResponse>> GetAllAsync(PostQueryParameters parameters)
    {
        var query = _context.Posts
            .Where(p => !p.IsDraft) // 임시저장 제외
            .AsQueryable();
        
        // 카테고리 필터
        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            query = query.Where(p => p.Category == parameters.Category);
        }
        
        // 작성자 필터
        if (parameters.AuthorId.HasValue)
        {
            query = query.Where(p => p.AuthorId == parameters.AuthorId.Value);
        }
        
        // 검색어 필터
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchTerm) || 
                p.Content.ToLower().Contains(searchTerm));
        }
        
        // 전체 개수
        var totalCount = await query.CountAsync();
        
        // 정렬 (상단고정 우선)
        var orderedQuery = query.OrderByDescending(p => p.IsPinned);
        
        // 추가 정렬
        IOrderedQueryable<Post> finalQuery = parameters.SortBy?.ToLower() switch
        {
            "viewcount" => parameters.SortOrder?.ToLower() == "asc" 
                ? orderedQuery.ThenBy(p => p.ViewCount) 
                : orderedQuery.ThenByDescending(p => p.ViewCount),
            "title" => parameters.SortOrder?.ToLower() == "asc" 
                ? orderedQuery.ThenBy(p => p.Title) 
                : orderedQuery.ThenByDescending(p => p.Title),
            "likecount" => parameters.SortOrder?.ToLower() == "asc"
                ? orderedQuery.ThenBy(p => p.LikeCount)
                : orderedQuery.ThenByDescending(p => p.LikeCount),
            _ => parameters.SortOrder?.ToLower() == "asc" 
                ? orderedQuery.ThenBy(p => p.CreatedAt) 
                : orderedQuery.ThenByDescending(p => p.CreatedAt)
        };
        
        // 페이징
        var posts = await finalQuery
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new PostSummaryResponse
            {
                Id = p.Id,
                Title = p.Title,
                ContentPreview = p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
                Category = p.Category,
                Tags = p.Tags,
                AuthorId = p.AuthorId,
                AuthorName = p.AuthorName,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        
        return PagedResponse<PostSummaryResponse>.Create(posts, parameters.Page, parameters.PageSize, totalCount);
    }
    
    /// <inheritdoc />
    public async Task<PostResponse?> GetByIdAsync(long id)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public async Task<PostResponse> CreateAsync(CreatePostRequest request, long authorId, string? authorName = null)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Category = request.Category,
            Tags = request.Tags ?? new List<string>(),
            AuthorId = authorId,
            AuthorName = authorName,
            Status = PostStatus.Published,
            IsDraft = false,
            PublishedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public async Task<PostResponse?> UpdateAsync(long id, UpdatePostRequest request, long userId, bool isAdmin = false)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        // 권한 검증 (작성자 또는 관리자만 수정 가능)
        if (post.AuthorId != userId && !isAdmin)
            return null;
        
        // 업데이트
        if (!string.IsNullOrWhiteSpace(request.Title))
            post.Title = request.Title;
        
        if (!string.IsNullOrWhiteSpace(request.Content))
            post.Content = request.Content;
        
        if (request.Category != null)
            post.Category = request.Category;
        
        if (request.Tags != null)
            post.Tags = request.Tags;
        
        await _context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, long userId, bool isAdmin = false)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null)
            return false;
        
        // 권한 검증
        if (post.AuthorId != userId && !isAdmin)
            return false;
        
        // 소프트 삭제
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.Status = PostStatus.Deleted;
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<PostResponse?> PinAsync(long id)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        post.IsPinned = true;
        await _context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public async Task<PostResponse?> UnpinAsync(long id)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        post.IsPinned = false;
        await _context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public async Task<DraftPostResponse> SaveDraftAsync(DraftPostRequest request, long authorId, string? authorName = null)
    {
        Post draft;
        
        // 기존 임시저장 덮어쓰기
        if (request.ExistingDraftId.HasValue)
        {
            draft = await _context.Posts.FindAsync(request.ExistingDraftId.Value) 
                    ?? throw new InvalidOperationException("임시저장을 찾을 수 없습니다.");
            
            if (draft.AuthorId != authorId)
                throw new UnauthorizedAccessException("권한이 없습니다.");
            
            draft.Title = request.Title ?? draft.Title;
            draft.Content = request.Content ?? draft.Content;
            draft.Category = request.Category ?? draft.Category;
            draft.Tags = request.Tags ?? draft.Tags;
        }
        else
        {
            // 새 임시저장 생성
            draft = new Post
            {
                Title = request.Title ?? string.Empty,
                Content = request.Content ?? string.Empty,
                Category = request.Category,
                Tags = request.Tags ?? new List<string>(),
                AuthorId = authorId,
                AuthorName = authorName,
                Status = PostStatus.Draft,
                IsDraft = true
            };
            
            _context.Posts.Add(draft);
        }
        
        await _context.SaveChangesAsync();
        
        return new DraftPostResponse
        {
            Id = draft.Id,
            Title = draft.Title,
            ContentPreview = draft.Content.Length > 200 ? draft.Content.Substring(0, 200) + "..." : draft.Content,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt
        };
    }
    
    /// <inheritdoc />
    public async Task<PagedResponse<DraftPostResponse>> GetDraftsAsync(long authorId, PagedRequest parameters)
    {
        var query = _context.Posts
            .IgnoreQueryFilters() // 소프트 삭제 필터 무시 (임시저장은 별도 관리)
            .Where(p => p.IsDraft && p.AuthorId == authorId && !p.IsDeleted)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt);
        
        var totalCount = await query.CountAsync();
        
        var drafts = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new DraftPostResponse
            {
                Id = p.Id,
                Title = p.Title,
                ContentPreview = p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
        
        return PagedResponse<DraftPostResponse>.Create(drafts, parameters.Page, parameters.PageSize, totalCount);
    }
    
    /// <inheritdoc />
    public async Task<PostResponse?> PublishAsync(long draftId, long userId)
    {
        var draft = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == draftId && p.IsDraft);
        
        if (draft == null)
            return null;
        
        if (draft.AuthorId != userId)
            return null;
        
        draft.IsDraft = false;
        draft.Status = PostStatus.Published;
        draft.PublishedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return MapToResponse(draft);
    }
    
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Posts.AnyAsync(p => p.Id == id);
    }
    
    /// <inheritdoc />
    public async Task<bool> IsAuthorAsync(long postId, long userId)
    {
        return await _context.Posts.AnyAsync(p => p.Id == postId && p.AuthorId == userId);
    }
    
    private static PostResponse MapToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Category = post.Category,
            Tags = post.Tags,
            AuthorId = post.AuthorId,
            AuthorName = post.AuthorName,
            Status = post.Status,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsPinned = post.IsPinned,
            IsDraft = post.IsDraft,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt
        };
    }
}
