using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 검색 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    
    /// <summary>
    /// 검색 컨트롤러 생성자
    /// </summary>
    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }
    
    /// <summary>
    /// 통합 검색
    /// </summary>
    /// <param name="q">검색어</param>
    /// <param name="type">검색 타입 (posts, tags, authors, all)</param>
    /// <param name="page">페이지 번호</param>
    /// <param name="pageSize">페이지 크기</param>
    /// <param name="category">카테고리 필터</param>
    /// <param name="tags">태그 필터</param>
    /// <param name="fromDate">시작 날짜</param>
    /// <param name="toDate">종료 날짜</param>
    /// <returns>통합 검색 결과</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UnifiedSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] string type = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? tags = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "검색어는 필수입니다." });
        }
        
        if (q.Length < 2)
        {
            return BadRequest(new { message = "검색어는 최소 2자 이상이어야 합니다." });
        }
        
        var parameters = new UnifiedSearchParameters
        {
            Query = q,
            SearchType = type,
            Page = page,
            PageSize = pageSize,
            Category = category,
            Tags = tags,
            FromDate = fromDate,
            ToDate = toDate
        };
        
        var result = await _searchService.SearchAllAsync(parameters);
        
        // 검색 기록 저장
        var userId = GetCurrentUserId();
        await _searchService.RecordSearchAsync(q, userId);
        
        return Ok(result);
    }
    
    /// <summary>
    /// 게시물 검색
    /// </summary>
    /// <param name="parameters">검색 파라미터</param>
    /// <returns>게시물 검색 결과</returns>
    [HttpGet("posts")]
    [ProducesResponseType(typeof(PagedSearchResult<PostSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPosts([FromQuery] PostSearchParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.Query))
        {
            return BadRequest(new { message = "검색어는 필수입니다." });
        }
        
        if (parameters.Query.Length < 2)
        {
            return BadRequest(new { message = "검색어는 최소 2자 이상이어야 합니다." });
        }
        
        var result = await _searchService.SearchPostsAsync(parameters);
        
        // 검색 기록 저장
        var userId = GetCurrentUserId();
        await _searchService.RecordSearchAsync(parameters.Query, userId);
        
        return Ok(result);
    }
    
    /// <summary>
    /// 태그 검색
    /// </summary>
    /// <param name="q">검색어</param>
    /// <param name="limit">결과 수 제한</param>
    /// <param name="minUsageCount">최소 사용 횟수</param>
    /// <returns>태그 검색 결과</returns>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<TagSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchTags(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        [FromQuery] int? minUsageCount = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "검색어는 필수입니다." });
        }
        
        var parameters = new TagSearchParameters
        {
            Query = q,
            Limit = limit,
            MinUsageCount = minUsageCount
        };
        
        var result = await _searchService.SearchTagsAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 작성자 검색
    /// </summary>
    /// <param name="q">검색어</param>
    /// <param name="limit">결과 수 제한</param>
    /// <param name="minPostCount">최소 게시물 수</param>
    /// <returns>작성자 검색 결과</returns>
    [HttpGet("authors")]
    [ProducesResponseType(typeof(List<AuthorSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAuthors(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        [FromQuery] int? minPostCount = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "검색어는 필수입니다." });
        }
        
        if (q.Length < 2)
        {
            return BadRequest(new { message = "검색어는 최소 2자 이상이어야 합니다." });
        }
        
        var parameters = new AuthorSearchParameters
        {
            Query = q,
            Limit = limit,
            MinPostCount = minPostCount
        };
        
        var result = await _searchService.SearchAuthorsAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 검색 제안 (자동완성)
    /// </summary>
    /// <param name="q">검색어</param>
    /// <param name="limit">제안 수</param>
    /// <returns>검색 제안 목록</returns>
    [HttpGet("suggest")]
    [ProducesResponseType(typeof(List<SearchSuggestion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(new List<SearchSuggestion>());
        }
        
        var result = await _searchService.GetSuggestionsAsync(q, limit);
        return Ok(result);
    }
    
    /// <summary>
    /// 인기 검색어 조회
    /// </summary>
    /// <param name="limit">조회 개수</param>
    /// <returns>인기 검색어 목록</returns>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<PopularSearchResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularSearches([FromQuery] int limit = 10)
    {
        var result = await _searchService.GetPopularSearchesAsync(limit);
        return Ok(result);
    }
    
    /// <summary>
    /// 현재 사용자 ID 가져오기
    /// </summary>
    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        
        return null;
    }
}
