using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardDemo.Api.Services;

/// <summary>
/// PostService를 상속받아 커스터마이징한 서비스
/// 게시물 생성/수정/삭제 시 로깅과 추가 기능을 제공합니다.
/// </summary>
public class CustomPostService : PostService
{
    private readonly ILogger<CustomPostService> _logger;
    
    public CustomPostService(
        BoardDbContext context,
        ILogger<CustomPostService> logger) 
        : base(context)
    {
        _logger = logger;
    }
    
    #region Hook 메서드 오버라이드
    
    /// <summary>
    /// 게시물 생성 후 호출 - 로깅 및 추가 처리
    /// </summary>
    protected override async Task OnPostCreatedAsync(Post post)
    {
        await base.OnPostCreatedAsync(post);
        
        // 커스텀 로직: 게시물 생성 로깅
        _logger.LogInformation(
            "📝 [CustomPostService] 새 게시물 생성: ID={PostId}, 제목='{Title}', 작성자ID={AuthorId}", 
            post.Id, post.Title, post.AuthorId);
        
        // 확장 속성에 생성 시간 기록 (예시)
        post.ExtendedProperties ??= new Dictionary<string, object>();
        post.ExtendedProperties["customCreatedLog"] = $"CustomPostService에서 생성됨: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        
        await Context.SaveChangesAsync();
    }
    
    /// <summary>
    /// 게시물 수정 후 호출 - 로깅 및 수정 이력 기록
    /// </summary>
    protected override async Task OnPostUpdatedAsync(Post post)
    {
        await base.OnPostUpdatedAsync(post);
        
        // 커스텀 로직: 게시물 수정 로깅
        _logger.LogInformation(
            "✏️ [CustomPostService] 게시물 수정: ID={PostId}, 제목='{Title}'", 
            post.Id, post.Title);
        
        // 수정 횟수 카운트 (확장 속성 활용)
        post.ExtendedProperties ??= new Dictionary<string, object>();
        var editCount = post.ExtendedProperties.TryGetValue("editCount", out var count) 
            ? Convert.ToInt32(count) + 1 
            : 1;
        post.ExtendedProperties["editCount"] = editCount;
        post.ExtendedProperties["lastEditTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        
        await Context.SaveChangesAsync();
    }
    
    /// <summary>
    /// 게시물 삭제 후 호출 - 로깅
    /// </summary>
    protected override async Task OnPostDeletedAsync(Post post)
    {
        await base.OnPostDeletedAsync(post);
        
        // 커스텀 로직: 게시물 삭제 로깅
        _logger.LogWarning(
            "🗑️ [CustomPostService] 게시물 삭제: ID={PostId}, 제목='{Title}', 작성자ID={AuthorId}", 
            post.Id, post.Title, post.AuthorId);
    }
    
    #endregion
    
    #region 쿼리 메서드 오버라이드
    
    /// <summary>
    /// 필터 적용 - 기본 필터 + 커스텀 필터 추가
    /// </summary>
    protected override IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostQueryParameters parameters)
    {
        // 기본 필터 적용
        query = base.ApplyFilters(query, parameters);
        
        // 커스텀 필터: 로깅
        _logger.LogDebug("🔍 [CustomPostService] 필터 적용 - 카테고리: {Category}, 검색어: {Search}", 
            parameters.Category, parameters.Search);
        
        return query;
    }
    
    #endregion
    
    #region 추가 커스텀 메서드
    
    /// <summary>
    /// 인기 게시물 조회 (커스텀 메서드)
    /// </summary>
    public async Task<List<PostSummaryResponse>> GetPopularPostsAsync(int count = 10)
    {
        _logger.LogInformation("🔥 [CustomPostService] 인기 게시물 조회: 요청 개수={Count}", count);
        
        var posts = await Context.Posts
            .Where(p => !p.IsDraft && !p.IsDeleted)
            .OrderByDescending(p => p.LikeCount)
            .ThenByDescending(p => p.ViewCount)
            .Take(count)
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
        
        return posts;
    }
    
    /// <summary>
    /// 최근 활동 게시물 조회 (최근 댓글이 달린 게시물)
    /// </summary>
    public async Task<List<PostSummaryResponse>> GetRecentlyActivePostsAsync(int count = 10)
    {
        _logger.LogInformation("⏰ [CustomPostService] 최근 활동 게시물 조회: 요청 개수={Count}", count);
        
        var posts = await Context.Posts
            .Where(p => !p.IsDraft && !p.IsDeleted)
            .Where(p => p.CommentCount > 0)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(count)
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
        
        return posts;
    }
    
    /// <summary>
    /// 통계 정보 조회
    /// </summary>
    public async Task<PostStatistics> GetStatisticsAsync()
    {
        _logger.LogInformation("📊 [CustomPostService] 통계 조회");
        
        var totalPosts = await Context.Posts.CountAsync(p => !p.IsDraft && !p.IsDeleted);
        var totalViews = await Context.Posts.Where(p => !p.IsDraft && !p.IsDeleted).SumAsync(p => p.ViewCount);
        var totalLikes = await Context.Posts.Where(p => !p.IsDraft && !p.IsDeleted).SumAsync(p => p.LikeCount);
        var totalComments = await Context.Posts.Where(p => !p.IsDraft && !p.IsDeleted).SumAsync(p => p.CommentCount);
        var pinnedCount = await Context.Posts.CountAsync(p => p.IsPinned && !p.IsDraft && !p.IsDeleted);
        
        var categoryCounts = await Context.Posts
            .Where(p => !p.IsDraft && !p.IsDeleted && p.Category != null)
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category!, x => x.Count);
        
        return new PostStatistics
        {
            TotalPosts = totalPosts,
            TotalViews = totalViews,
            TotalLikes = totalLikes,
            TotalComments = totalComments,
            PinnedCount = pinnedCount,
            CategoryCounts = categoryCounts,
            GeneratedAt = DateTime.UtcNow
        };
    }
    
    #endregion
}

/// <summary>
/// 게시물 통계 DTO
/// </summary>
public class PostStatistics
{
    public int TotalPosts { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public int PinnedCount { get; set; }
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
