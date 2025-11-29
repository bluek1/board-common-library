using System.Diagnostics;
using System.Text.RegularExpressions;
using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 검색 서비스 구현
/// </summary>
public class SearchService : ISearchService
{
    private readonly BoardDbContext _context;
    private readonly ILogger<SearchService> _logger;
    
    /// <summary>
    /// 검색 서비스 생성자
    /// </summary>
    public SearchService(BoardDbContext context, ILogger<SearchService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<PagedSearchResult<PostSearchResult>> SearchPostsAsync(PostSearchParameters parameters)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = _context.Posts.AsQueryable();
        
        // 검색어 적용 (대소문자 구분 없이)
        if (!string.IsNullOrWhiteSpace(parameters.Query))
        {
            var searchQuery = parameters.Query.Trim();
            var searchIn = (parameters.SearchIn ?? "all").ToLowerInvariant();
            
            query = searchIn switch
            {
                "title" => query.Where(p => EF.Functions.Like(p.Title, $"%{searchQuery}%")),
                "content" => query.Where(p => EF.Functions.Like(p.Content, $"%{searchQuery}%")),
                _ => query.Where(p => EF.Functions.Like(p.Title, $"%{searchQuery}%") ||
                                     EF.Functions.Like(p.Content, $"%{searchQuery}%"))
            };
        }
        
        // 필터 적용
        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            query = query.Where(p => p.Category == parameters.Category);
        }
        
        if (!string.IsNullOrWhiteSpace(parameters.Tags))
        {
            // 태그 필터링 - 모든 태그가 포함된 게시물 검색
            var tags = parameters.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();
            
            // EF Core에서 LIKE 쿼리 동적 생성을 위해 각 태그별로 필터 적용
            foreach (var tag in tags)
            {
                // 로컬 변수로 복사하여 클로저 문제 방지
                var currentTag = tag;
                query = query.Where(p => p.Tags != null && p.Tags.Contains(currentTag));
            }
        }
        
        if (parameters.AuthorId.HasValue)
        {
            query = query.Where(p => p.AuthorId == parameters.AuthorId);
        }
        
        if (!string.IsNullOrWhiteSpace(parameters.AuthorName))
        {
            var authorName = parameters.AuthorName;
            query = query.Where(p => EF.Functions.Like(p.AuthorName, $"%{authorName}%"));
        }
        
        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            if (Enum.TryParse<Entities.PostStatus>(parameters.Status, true, out var status))
            {
                query = query.Where(p => p.Status == status);
            }
        }
        
        if (parameters.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= parameters.FromDate.Value);
        }
        
        if (parameters.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= parameters.ToDate.Value);
        }
        
        if (parameters.MinViewCount.HasValue)
        {
            query = query.Where(p => p.ViewCount >= parameters.MinViewCount);
        }
        
        if (parameters.MinLikeCount.HasValue)
        {
            query = query.Where(p => p.LikeCount >= parameters.MinLikeCount);
        }
        
        // 정렬 적용
        var sortField = (parameters.Sort ?? "createdat").ToLowerInvariant();
        var sortOrder = (parameters.Order ?? "desc").ToLowerInvariant();
        
        query = sortField switch
        {
            "createdat" => sortOrder == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            "viewcount" => sortOrder == "asc"
                ? query.OrderBy(p => p.ViewCount)
                : query.OrderByDescending(p => p.ViewCount),
            "likecount" => sortOrder == "asc"
                ? query.OrderBy(p => p.LikeCount)
                : query.OrderByDescending(p => p.LikeCount),
            _ => parameters.IncludePinned
                ? query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);
        
        var posts = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        stopwatch.Stop();
        
        var searchQueryText = parameters.Query ?? "";
        
        var results = posts.Select(p => new PostSearchResult
        {
            Id = p.Id,
            Title = p.Title,
            TitleHighlighted = HighlightSearchTerms(p.Title, searchQueryText),
            ContentPreview = GenerateContentPreview(p.Content, searchQueryText),
            ContentPreviewHighlighted = HighlightSearchTerms(
                GenerateContentPreview(p.Content, searchQueryText), 
                searchQueryText),
            AuthorId = p.AuthorId,
            AuthorName = p.AuthorName,
            Category = p.Category,
            Tags = p.Tags ?? new List<string>(),
            Status = p.Status.ToString(),
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            IsPinned = p.IsPinned,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
        
        return new PagedSearchResult<PostSearchResult>
        {
            Items = results,
            Query = searchQueryText,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = parameters.Page,
            PageSize = parameters.PageSize,
            SearchTimeMs = stopwatch.ElapsedMilliseconds
        };
    }
    
    /// <inheritdoc/>
    public async Task<List<TagSearchResult>> SearchTagsAsync(TagSearchParameters parameters)
    {
        var searchQuery = parameters.Query.Trim().ToLower();
        
        // 모든 게시물에서 태그 추출 - 메모리로 가져와서 처리
        var posts = await _context.Posts
            .Where(p => p.Tags != null && p.Tags.Count > 0)
            .Select(p => new { p.Tags, p.CreatedAt })
            .ToListAsync();
        
        // 태그별 사용 횟수 집계
        var tagCounts = posts
            .Where(p => p.Tags != null)
            .SelectMany(p => p.Tags.Select(t => new { Tag = t.Trim(), p.CreatedAt }))
            .Where(x => !string.IsNullOrWhiteSpace(x.Tag) && x.Tag.ToLower().Contains(searchQuery))
            .GroupBy(x => x.Tag, StringComparer.OrdinalIgnoreCase)
            .Select(g => new TagSearchResult
            {
                Name = g.Key,
                NameHighlighted = HighlightSearchTerms(g.Key, parameters.Query),
                UsageCount = g.Count(),
                LastUsedAt = g.Max(x => x.CreatedAt)
            })
            .OrderByDescending(t => t.UsageCount)
            .Take(parameters.Limit)
            .ToList();
        
        if (parameters.MinUsageCount.HasValue)
        {
            tagCounts = tagCounts.Where(t => t.UsageCount >= parameters.MinUsageCount.Value).ToList();
        }
        
        return tagCounts;
    }
    
    /// <inheritdoc/>
    public async Task<List<AuthorSearchResult>> SearchAuthorsAsync(AuthorSearchParameters parameters)
    {
        var searchQuery = parameters.Query.Trim().ToLowerInvariant();
        
        // 게시물 작성자별 집계 - 클라이언트 측에서 필터링
        var allAuthors = await _context.Posts
            .GroupBy(p => new { p.AuthorId, p.AuthorName })
            .Select(g => new
            {
                AuthorId = g.Key.AuthorId,
                AuthorName = g.Key.AuthorName,
                PostCount = g.Count()
            })
            .ToListAsync();
        
        var authors = allAuthors
            .Where(a => a.AuthorName.ToLowerInvariant().Contains(searchQuery))
            .Select(a => new AuthorSearchResult
            {
                Id = a.AuthorId,
                Name = a.AuthorName,
                PostCount = a.PostCount
            })
            .ToList();
        
        // 댓글 작성 수 추가
        var allCommentCounts = await _context.Comments
            .GroupBy(c => c.AuthorId)
            .Select(g => new { AuthorId = g.Key, CommentCount = g.Count() })
            .ToDictionaryAsync(x => x.AuthorId, x => x.CommentCount);
        
        foreach (var author in authors)
        {
            author.NameHighlighted = HighlightSearchTerms(author.Name, parameters.Query);
            if (allCommentCounts.TryGetValue(author.Id, out var count))
            {
                author.CommentCount = count;
            }
        }
        
        var results = authors
            .OrderByDescending(a => a.PostCount)
            .Take(parameters.Limit)
            .ToList();
        
        if (parameters.MinPostCount.HasValue)
        {
            results = results.Where(a => a.PostCount >= parameters.MinPostCount.Value).ToList();
        }
        
        return results;
    }
    
    /// <inheritdoc/>
    public async Task<UnifiedSearchResult> SearchAllAsync(UnifiedSearchParameters parameters)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var result = new UnifiedSearchResult
        {
            Query = parameters.Query
        };
        
        var searchType = (parameters.SearchType ?? "all").ToLowerInvariant();
        
        // 게시물 검색
        if (searchType == "all" || searchType == "posts")
        {
            var postParams = new PostSearchParameters
            {
                Query = parameters.Query,
                Page = parameters.Page,
                PageSize = Math.Min(parameters.PageSize, 10),
                SearchIn = parameters.SearchIn ?? "all",
                Category = parameters.Category,
                Tags = parameters.Tags,
                FromDate = parameters.FromDate,
                ToDate = parameters.ToDate
            };
            
            var postResults = await SearchPostsAsync(postParams);
            result.Posts = new SearchResultSection<PostSearchResult>
            {
                Items = postResults.Items,
                TotalCount = postResults.TotalCount,
                HasMore = postResults.TotalCount > postResults.Items.Count
            };
        }
        
        // 태그 검색
        if (searchType == "all" || searchType == "tags")
        {
            var tagParams = new TagSearchParameters
            {
                Query = parameters.Query,
                Limit = 10
            };
            
            var tagResults = await SearchTagsAsync(tagParams);
            result.Tags = new SearchResultSection<TagSearchResult>
            {
                Items = tagResults,
                TotalCount = tagResults.Count,
                HasMore = false
            };
        }
        
        // 작성자 검색
        if (searchType == "all" || searchType == "authors")
        {
            var authorParams = new AuthorSearchParameters
            {
                Query = parameters.Query,
                Limit = 10
            };
            
            var authorResults = await SearchAuthorsAsync(authorParams);
            result.Authors = new SearchResultSection<AuthorSearchResult>
            {
                Items = authorResults,
                TotalCount = authorResults.Count,
                HasMore = false
            };
        }
        
        stopwatch.Stop();
        result.SearchTimeMs = stopwatch.ElapsedMilliseconds;
        
        return result;
    }
    
    /// <inheritdoc/>
    public Task<List<PopularSearchResult>> GetPopularSearchesAsync(int limit = 10)
    {
        // 실제 구현에서는 검색어 저장 테이블에서 조회
        // 현재는 빈 목록 반환 (추후 SearchLog 테이블 추가 시 구현)
        return Task.FromResult(new List<PopularSearchResult>());
    }
    
    /// <inheritdoc/>
    public async Task<List<SearchSuggestion>> GetSuggestionsAsync(string query, int limit = 5)
    {
        var suggestions = new List<SearchSuggestion>();
        var searchQuery = query.Trim().ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(searchQuery) || searchQuery.Length < 2)
        {
            return suggestions;
        }
        
        // 제목에서 제안 - 클라이언트 측 필터링
        var allTitles = await _context.Posts
            .Select(p => p.Title)
            .Distinct()
            .ToListAsync();
        
        var titleSuggestions = allTitles
            .Where(t => t.ToLower().Contains(searchQuery))
            .Take(limit)
            .ToList();
        
        suggestions.AddRange(titleSuggestions.Select(t => new SearchSuggestion
        {
            Text = t,
            Type = "post",
            Count = 1
        }));
        
        // 태그에서 제안
        var allTags = await _context.Posts
            .Where(p => p.Tags != null && p.Tags.Count > 0)
            .Select(p => p.Tags)
            .ToListAsync();
        
        var uniqueTags = allTags
            .Where(t => t != null)
            .SelectMany(t => t!)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t) && t.ToLower().Contains(searchQuery))
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .ToList();
        
        suggestions.AddRange(uniqueTags.Select(g => new SearchSuggestion
        {
            Text = g.Key,
            Type = "tag",
            Count = g.Count()
        }));
        
        return suggestions
            .OrderByDescending(s => s.Count)
            .Take(limit)
            .ToList();
    }
    
    /// <inheritdoc/>
    public Task RecordSearchAsync(string query, long? userId = null)
    {
        // 실제 구현에서는 SearchLog 테이블에 저장
        _logger.LogInformation("검색 기록: Query={Query}, UserId={UserId}", query, userId);
        return Task.CompletedTask;
    }
    
    /// <inheritdoc/>
    public string HighlightSearchTerms(string text, string query, string highlightTag = "mark")
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
        {
            return text;
        }
        
        var terms = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var term in terms)
        {
            var pattern = Regex.Escape(term);
            text = Regex.Replace(
                text,
                pattern,
                m => $"<{highlightTag}>{m.Value}</{highlightTag}>",
                RegexOptions.IgnoreCase);
        }
        
        return text;
    }
    
    /// <inheritdoc/>
    public string GenerateContentPreview(string content, string query, int previewLength = 150)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }
        
        // HTML 태그 제거
        var plainText = Regex.Replace(content, "<[^>]*>", " ");
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();
        
        if (string.IsNullOrWhiteSpace(query))
        {
            return plainText.Length <= previewLength
                ? plainText
                : plainText.Substring(0, previewLength) + "...";
        }
        
        // 검색어가 포함된 위치 찾기
        var queryLower = query.ToLowerInvariant();
        var textLower = plainText.ToLowerInvariant();
        var index = textLower.IndexOf(queryLower, StringComparison.Ordinal);
        
        if (index < 0)
        {
            return plainText.Length <= previewLength
                ? plainText
                : plainText.Substring(0, previewLength) + "...";
        }
        
        // 검색어 주변 텍스트 추출
        var start = Math.Max(0, index - previewLength / 3);
        var end = Math.Min(plainText.Length, start + previewLength);
        
        var preview = plainText.Substring(start, end - start);
        
        if (start > 0)
        {
            preview = "..." + preview;
        }
        
        if (end < plainText.Length)
        {
            preview = preview + "...";
        }
        
        return preview;
    }
    
    /// <inheritdoc/>
    public Task RebuildSearchIndexAsync()
    {
        // Full-text search 인덱스 재구축 (현재는 LIKE 쿼리 사용으로 불필요)
        _logger.LogInformation("검색 인덱스 재구축 요청됨");
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// 태그 문자열을 리스트로 파싱
    /// </summary>
    private static List<string> ParseTagsToList(string? tags)
    {
        if (string.IsNullOrEmpty(tags))
        {
            return new List<string>();
        }
        
        return tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }
    
    /// <summary>
    /// 태그 문자열을 배열로 분리
    /// </summary>
    private static string[] SplitTagsToArray(string tags)
    {
        if (string.IsNullOrEmpty(tags))
        {
            return Array.Empty<string>();
        }
        
        return tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
