using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 좋아요 서비스 구현
/// </summary>
public class LikeService : ILikeService
{
    private readonly BoardDbContext _context;
    
    public LikeService(BoardDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<LikeResponse> LikePostAsync(long postId, long userId)
    {
        // 게시물 존재 확인
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new InvalidOperationException($"게시물(ID: {postId})을 찾을 수 없습니다.");
        }
        
        // 중복 좋아요 확인
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        
        if (existingLike != null)
        {
            throw new InvalidOperationException("이미 좋아요한 게시물입니다.");
        }
        
        // 좋아요 추가
        var like = new Like
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Likes.Add(like);
        
        // 게시물 좋아요 수 증가
        post.LikeCount++;
        
        await _context.SaveChangesAsync();
        
        return new LikeResponse
        {
            IsLiked = true,
            TotalLikeCount = post.LikeCount
        };
    }
    
    /// <inheritdoc />
    public async Task<LikeResponse?> UnlikePostAsync(long postId, long userId)
    {
        // 게시물 존재 확인
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return null;
        }
        
        // 좋아요 확인
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        
        if (like == null)
        {
            return null;
        }
        
        // 좋아요 삭제
        _context.Likes.Remove(like);
        
        // 게시물 좋아요 수 감소
        post.LikeCount = Math.Max(0, post.LikeCount - 1);
        
        await _context.SaveChangesAsync();
        
        return new LikeResponse
        {
            IsLiked = false,
            TotalLikeCount = post.LikeCount
        };
    }
    
    /// <inheritdoc />
    public async Task<LikeResponse> LikeCommentAsync(long commentId, long userId)
    {
        // 댓글 존재 확인
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException($"댓글(ID: {commentId})을 찾을 수 없습니다.");
        }
        
        // 중복 좋아요 확인
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);
        
        if (existingLike != null)
        {
            throw new InvalidOperationException("이미 좋아요한 댓글입니다.");
        }
        
        // 좋아요 추가
        var like = new Like
        {
            UserId = userId,
            CommentId = commentId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Likes.Add(like);
        
        // 댓글 좋아요 수 증가
        comment.LikeCount++;
        
        await _context.SaveChangesAsync();
        
        return new LikeResponse
        {
            IsLiked = true,
            TotalLikeCount = comment.LikeCount
        };
    }
    
    /// <inheritdoc />
    public async Task<LikeResponse?> UnlikeCommentAsync(long commentId, long userId)
    {
        // 댓글 존재 확인
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return null;
        }
        
        // 좋아요 확인
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);
        
        if (like == null)
        {
            return null;
        }
        
        // 좋아요 삭제
        _context.Likes.Remove(like);
        
        // 댓글 좋아요 수 감소
        comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
        
        await _context.SaveChangesAsync();
        
        return new LikeResponse
        {
            IsLiked = false,
            TotalLikeCount = comment.LikeCount
        };
    }
    
    /// <inheritdoc />
    public async Task<bool> HasUserLikedPostAsync(long postId, long userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.UserId == userId && l.PostId == postId);
    }
    
    /// <inheritdoc />
    public async Task<bool> HasUserLikedCommentAsync(long commentId, long userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.UserId == userId && l.CommentId == commentId);
    }
}
