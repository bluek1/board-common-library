using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 게시물 서비스 구현
/// 상속하여 커스터마이징 가능합니다.
/// </summary>
public class PostService : IPostService
{
    protected readonly BoardDbContext Context;
    
    public PostService(BoardDbContext context)
    {
        Context = context;
    }
    
    /// <inheritdoc />
    public virtual async Task<PagedResponse<PostSummaryResponse>> GetAllAsync(PostQueryParameters parameters)
    {
        var query = BuildBaseQuery();
        query = ApplyFilters(query, parameters);
        
        // 전체 개수
        var totalCount = await query.CountAsync();
        
        // 정렬 및 페이징
        var orderedQuery = ApplySorting(query, parameters);
        var posts = await ApplyPagingAndProject(orderedQuery, parameters);
        
        return PagedResponse<PostSummaryResponse>.Create(posts, parameters.Page, parameters.PageSize, totalCount);
    }
    
    /// <summary>
    /// 기본 쿼리 생성 (오버라이드하여 커스텀 필터 추가 가능)
    /// </summary>
    protected virtual IQueryable<Post> BuildBaseQuery()
    {
        return Context.Posts
            .Where(p => !p.IsDraft) // 임시저장 제외
            .AsQueryable();
    }
    
    /// <summary>
    /// 필터 적용 (오버라이드하여 커스텀 필터 추가 가능)
    /// </summary>
    protected virtual IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostQueryParameters parameters)
    {
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
        
        return query;
    }
    
    /// <summary>
    /// 정렬 적용 (오버라이드하여 커스텀 정렬 추가 가능)
    /// </summary>
    protected virtual IOrderedQueryable<Post> ApplySorting(IQueryable<Post> query, PostQueryParameters parameters)
    {
        // 상단고정 우선
        var orderedQuery = query.OrderByDescending(p => p.IsPinned);
        
        // 추가 정렬
        return parameters.SortBy?.ToLower() switch
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
    }
    
    /// <summary>
    /// 페이징 및 프로젝션 적용 (오버라이드하여 커스텀 DTO 매핑 가능)
    /// </summary>
    protected virtual async Task<List<PostSummaryResponse>> ApplyPagingAndProject(
        IOrderedQueryable<Post> query, 
        PostQueryParameters parameters)
    {
        return await query
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
    }
    
    /// <inheritdoc />
    public virtual async Task<PostResponse?> GetByIdAsync(long id)
    {
        var post = await Context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public virtual async Task<PostResponse> CreateAsync(CreatePostRequest request, long authorId, string? authorName = null)
    {
        var post = CreatePostEntity(request, authorId, authorName);
        
        Context.Posts.Add(post);
        await Context.SaveChangesAsync();
        
        await OnPostCreatedAsync(post);
        
        return MapToResponse(post);
    }
    
    /// <summary>
    /// Post 엔티티 생성 (오버라이드하여 커스텀 필드 설정 가능)
    /// </summary>
    protected virtual Post CreatePostEntity(CreatePostRequest request, long authorId, string? authorName)
    {
        return new Post
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
    }
    
    /// <summary>
    /// 게시물 생성 후 훅 (오버라이드하여 커스텀 로직 추가 가능)
    /// </summary>
    protected virtual Task OnPostCreatedAsync(Post post)
    {
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public virtual async Task<PostResponse?> UpdateAsync(long id, UpdatePostRequest request, long userId, bool isAdmin = false)
    {
        var post = await Context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        // 권한 검증 (작성자 또는 관리자만 수정 가능)
        if (!CanModifyPost(post, userId, isAdmin))
            return null;
        
        ApplyUpdate(post, request);
        
        await Context.SaveChangesAsync();
        
        await OnPostUpdatedAsync(post);
        
        return MapToResponse(post);
    }
    
    /// <summary>
    /// 게시물 수정 권한 확인 (오버라이드하여 커스텀 권한 로직 추가 가능)
    /// </summary>
    protected virtual bool CanModifyPost(Post post, long userId, bool isAdmin)
    {
        return post.AuthorId == userId || isAdmin;
    }
    
    /// <summary>
    /// 업데이트 내용 적용 (오버라이드하여 커스텀 필드 업데이트 가능)
    /// </summary>
    protected virtual void ApplyUpdate(Post post, UpdatePostRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Title))
            post.Title = request.Title;
        
        if (!string.IsNullOrWhiteSpace(request.Content))
            post.Content = request.Content;
        
        if (request.Category != null)
            post.Category = request.Category;
        
        if (request.Tags != null)
            post.Tags = request.Tags;
    }
    
    /// <summary>
    /// 게시물 수정 후 훅 (오버라이드하여 커스텀 로직 추가 가능)
    /// </summary>
    protected virtual Task OnPostUpdatedAsync(Post post)
    {
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(long id, long userId, bool isAdmin = false)
    {
        var post = await Context.Posts.FindAsync(id);
        
        if (post == null)
            return false;
        
        // 권한 검증
        if (!CanModifyPost(post, userId, isAdmin))
            return false;
        
        // 소프트 삭제
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.Status = PostStatus.Deleted;
        
        await Context.SaveChangesAsync();
        
        await OnPostDeletedAsync(post);
        
        return true;
    }
    
    /// <summary>
    /// 게시물 삭제 후 훅 (오버라이드하여 커스텀 로직 추가 가능)
    /// </summary>
    protected virtual Task OnPostDeletedAsync(Post post)
    {
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public virtual async Task<PostResponse?> PinAsync(long id)
    {
        var post = await Context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        post.IsPinned = true;
        await Context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public virtual async Task<PostResponse?> UnpinAsync(long id)
    {
        var post = await Context.Posts.FindAsync(id);
        
        if (post == null)
            return null;
        
        post.IsPinned = false;
        await Context.SaveChangesAsync();
        
        return MapToResponse(post);
    }
    
    /// <inheritdoc />
    public virtual async Task<DraftPostResponse> SaveDraftAsync(DraftPostRequest request, long authorId, string? authorName = null)
    {
        Post draft;
        
        // 기존 임시저장 덮어쓰기
        if (request.ExistingDraftId.HasValue)
        {
            draft = await Context.Posts.FindAsync(request.ExistingDraftId.Value) 
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
            
            Context.Posts.Add(draft);
        }
        
        await Context.SaveChangesAsync();
        
        return MapToDraftResponse(draft);
    }
    
    /// <inheritdoc />
    public virtual async Task<PagedResponse<DraftPostResponse>> GetDraftsAsync(long authorId, PagedRequest parameters)
    {
        var query = Context.Posts
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
    public virtual async Task<PostResponse?> PublishAsync(long draftId, long userId)
    {
        var draft = await Context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == draftId && p.IsDraft);
        
        if (draft == null)
            return null;
        
        if (draft.AuthorId != userId)
            return null;
        
        draft.IsDraft = false;
        draft.Status = PostStatus.Published;
        draft.PublishedAt = DateTime.UtcNow;
        
        await Context.SaveChangesAsync();
        
        return MapToResponse(draft);
    }
    
    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(long id)
    {
        return await Context.Posts.AnyAsync(p => p.Id == id);
    }
    
    /// <inheritdoc />
    public virtual async Task<bool> IsAuthorAsync(long postId, long userId)
    {
        return await Context.Posts.AnyAsync(p => p.Id == postId && p.AuthorId == userId);
    }
    
    /// <summary>
    /// Post를 PostResponse로 매핑 (오버라이드하여 커스텀 매핑 가능)
    /// </summary>
    protected virtual PostResponse MapToResponse(Post post)
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
    
    /// <summary>
    /// Post를 DraftPostResponse로 매핑 (오버라이드하여 커스텀 매핑 가능)
    /// </summary>
    protected virtual DraftPostResponse MapToDraftResponse(Post draft)
    {
        return new DraftPostResponse
        {
            Id = draft.Id,
            Title = draft.Title,
            ContentPreview = draft.Content.Length > 200 ? draft.Content.Substring(0, 200) + "..." : draft.Content,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt
        };
    }
}
