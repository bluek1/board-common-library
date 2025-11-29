using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 북마크 서비스 구현
/// </summary>
public class BookmarkService : IBookmarkService
{
    private readonly BoardDbContext _context;
    
    public BookmarkService(BoardDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<bool> AddBookmarkAsync(long postId, long userId)
    {
        // 게시물 존재 확인
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new InvalidOperationException($"게시물(ID: {postId})을 찾을 수 없습니다.");
        }
        
        // 중복 북마크 확인
        var existingBookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);
        
        if (existingBookmark != null)
        {
            throw new InvalidOperationException("이미 북마크한 게시물입니다.");
        }
        
        // 북마크 추가
        var bookmark = new Bookmark
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> RemoveBookmarkAsync(long postId, long userId)
    {
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);
        
        if (bookmark == null)
        {
            return false;
        }
        
        _context.Bookmarks.Remove(bookmark);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<PagedResponse<BookmarkResponse>> GetUserBookmarksAsync(long userId, BookmarkQueryParameters parameters)
    {
        var query = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Post)
            .Where(b => b.Post != null && !b.Post.IsDeleted)
            .AsQueryable();
        
        // 전체 개수
        var totalCount = await query.CountAsync();
        
        // 정렬
        IOrderedQueryable<Bookmark> orderedQuery = parameters.SortOrder?.ToLower() == "asc"
            ? query.OrderBy(b => b.CreatedAt)
            : query.OrderByDescending(b => b.CreatedAt);
        
        // 페이징
        var bookmarks = await orderedQuery
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        var responses = bookmarks.Select(b => new BookmarkResponse
        {
            Id = b.Id,
            PostId = b.PostId,
            Post = MapToPostSummary(b.Post!),
            CreatedAt = b.CreatedAt
        }).ToList();
        
        return PagedResponse<BookmarkResponse>.Create(responses, parameters.Page, parameters.PageSize, totalCount);
    }
    
    /// <inheritdoc />
    public async Task<bool> HasUserBookmarkedAsync(long postId, long userId)
    {
        return await _context.Bookmarks
            .AnyAsync(b => b.UserId == userId && b.PostId == postId);
    }
    
    private static PostSummaryResponse MapToPostSummary(Post post)
    {
        return new PostSummaryResponse
        {
            Id = post.Id,
            Title = post.Title,
            ContentPreview = post.Content.Length > 200 ? post.Content.Substring(0, 200) + "..." : post.Content,
            Category = post.Category,
            Tags = post.Tags,
            AuthorId = post.AuthorId,
            AuthorName = post.AuthorName,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsPinned = post.IsPinned,
            CreatedAt = post.CreatedAt
        };
    }
}
