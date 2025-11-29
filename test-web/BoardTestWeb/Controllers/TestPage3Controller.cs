using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 3 테스트 컨트롤러 - 파일/검색
/// 실제 BoardCommonLibrary 서비스를 호출하여 테스트합니다.
/// </summary>
[ApiController]
[Route("api/page3")]
public class TestPage3Controller : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ISearchService _searchService;

    public TestPage3Controller(
        IFileService fileService,
        IFileValidationService fileValidationService,
        IThumbnailService thumbnailService,
        ISearchService searchService)
    {
        _fileService = fileService;
        _fileValidationService = fileValidationService;
        _thumbnailService = thumbnailService;
        _searchService = searchService;
    }

    #region File Upload Tests

    /// <summary>
    /// 파일 업로드 테스트 (T3-001, T3-002, T3-003)
    /// </summary>
    [HttpPost("files/upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiErrorResponse.Create("FILE_REQUIRED", "파일이 없습니다."));
        }

        // 파일 검증
        var validationResult = await _fileValidationService.ValidateAsync(file);
        if (!validationResult.IsValid)
        {
            if (validationResult.Errors.Any(e => e.Contains("크기") || e.Contains("size")))
            {
                return StatusCode(413, ApiErrorResponse.Create("FILE_TOO_LARGE", validationResult.Errors.First()));
            }
            if (validationResult.Errors.Any(e => e.Contains("형식") || e.Contains("확장자") || e.Contains("MIME")))
            {
                return StatusCode(415, ApiErrorResponse.Create("UNSUPPORTED_MEDIA_TYPE", validationResult.Errors.First()));
            }
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", validationResult.Errors.First()));
        }

        // 파일 업로드
        var userId = GetUserId();
        var userName = GetUserName();
        var postId = GetPostIdFromQuery();

        var uploaded = await _fileService.UploadAsync(file, userId, userName, postId);
        
        return Created($"/api/page3/files/{uploaded.Id}", ApiResponse<FileInfoResponse>.Ok(uploaded));
    }

    /// <summary>
    /// 다중 파일 업로드 테스트 (T3-004)
    /// </summary>
    [HttpPost("files/upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(ApiErrorResponse.Create("FILES_REQUIRED", "파일이 없습니다."));
        }

        var userId = GetUserId();
        var userName = GetUserName();
        var postId = GetPostIdFromQuery();

        var result = await _fileService.UploadMultipleAsync(files, userId, userName, postId);

        return Created("/api/page3/files", ApiResponse<MultipleFileUploadResponse>.Ok(result));
    }

    #endregion

    #region File Download/Management Tests

    /// <summary>
    /// 파일 다운로드 테스트 (T3-005, T3-006)
    /// </summary>
    [HttpGet("files/{id:long}/download")]
    public async Task<IActionResult> DownloadFile(long id)
    {
        var downloadResult = await _fileService.DownloadAsync(id);
        
        if (downloadResult == null)
        {
            return NotFound(ApiErrorResponse.Create("FILE_NOT_FOUND", "파일을 찾을 수 없습니다."));
        }

        return File(downloadResult.FileStream, downloadResult.ContentType, downloadResult.FileName);
    }

    /// <summary>
    /// 파일 정보 조회 테스트
    /// </summary>
    [HttpGet("files/{id:long}")]
    public async Task<IActionResult> GetFile(long id)
    {
        var file = await _fileService.GetByIdAsync(id);
        
        if (file == null)
        {
            return NotFound(ApiErrorResponse.Create("FILE_NOT_FOUND", "파일을 찾을 수 없습니다."));
        }

        return Ok(ApiResponse<FileInfoResponse>.Ok(file));
    }

    /// <summary>
    /// 파일 삭제 테스트 (T3-007)
    /// </summary>
    [HttpDelete("files/{id:long}")]
    public async Task<IActionResult> DeleteFile(long id)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
        {
            return NotFound(ApiErrorResponse.Create("FILE_NOT_FOUND", "파일을 찾을 수 없습니다."));
        }

        var userId = GetUserId();
        var isAdmin = IsAdmin();

        // 권한 확인: 업로더 또는 관리자만 삭제 가능
        if (file.UploaderId != userId && !isAdmin)
        {
            return StatusCode(403, ApiErrorResponse.Create("FORBIDDEN", "파일을 삭제할 권한이 없습니다."));
        }

        await _fileService.DeleteAsync(id, userId);
        return NoContent();
    }

    /// <summary>
    /// 썸네일 조회 테스트 (T3-008)
    /// </summary>
    [HttpGet("files/{id:long}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(long id)
    {
        var thumbnailResult = await _fileService.GetThumbnailAsync(id);
        
        if (thumbnailResult == null)
        {
            return NotFound(ApiErrorResponse.Create("THUMBNAIL_NOT_FOUND", "썸네일을 찾을 수 없습니다."));
        }

        return File(thumbnailResult.FileStream, thumbnailResult.ContentType);
    }

    /// <summary>
    /// 게시물의 파일 목록 조회
    /// </summary>
    [HttpGet("posts/{postId:long}/files")]
    public async Task<IActionResult> GetFilesByPost(long postId)
    {
        var files = await _fileService.GetByPostIdAsync(postId);
        return Ok(ApiResponse<IEnumerable<FileInfoResponse>>.Ok(files));
    }

    #endregion

    #region Search Tests

    /// <summary>
    /// 통합 검색 테스트 (T3-009, T3-010, T3-013)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] PostSearchParameters request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(ApiErrorResponse.Create("QUERY_REQUIRED", "검색어를 입력해주세요."));
        }

        var result = await _searchService.SearchPostsAsync(request);
        return Ok(ApiResponse<PagedSearchResult<PostSearchResult>>.Ok(result));
    }

    /// <summary>
    /// 게시물 검색 테스트 (T3-009, T3-010, T3-012)
    /// </summary>
    [HttpGet("search/posts")]
    public async Task<IActionResult> SearchPosts([FromQuery] PostSearchParameters request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(ApiErrorResponse.Create("QUERY_REQUIRED", "검색어를 입력해주세요."));
        }

        var result = await _searchService.SearchPostsAsync(request);
        return Ok(ApiResponse<PagedSearchResult<PostSearchResult>>.Ok(result));
    }

    /// <summary>
    /// 태그 검색 테스트 (T3-011)
    /// </summary>
    [HttpGet("search/tags")]
    public async Task<IActionResult> SearchTags([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiErrorResponse.Create("QUERY_REQUIRED", "검색어를 입력해주세요."));
        }

        var request = new TagSearchParameters
        {
            Query = q,
            Limit = limit
        };

        var result = await _searchService.SearchTagsAsync(request);
        return Ok(ApiResponse<List<TagSearchResult>>.Ok(result));
    }

    /// <summary>
    /// 작성자 검색 테스트
    /// </summary>
    [HttpGet("search/authors")]
    public async Task<IActionResult> SearchAuthors([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiErrorResponse.Create("QUERY_REQUIRED", "검색어를 입력해주세요."));
        }

        var request = new AuthorSearchParameters
        {
            Query = q,
            Limit = limit
        };

        var result = await _searchService.SearchAuthorsAsync(request);
        return Ok(ApiResponse<List<AuthorSearchResult>>.Ok(result));
    }

    /// <summary>
    /// 검색어 자동완성/제안 테스트 (T3-014)
    /// </summary>
    [HttpGet("search/suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q, [FromQuery] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(ApiResponse<List<SearchSuggestion>>.Ok(new List<SearchSuggestion>()));
        }

        var result = await _searchService.GetSuggestionsAsync(q, limit);
        return Ok(ApiResponse<List<SearchSuggestion>>.Ok(result));
    }

    /// <summary>
    /// 빈 검색 결과 테스트 (T3-015)
    /// </summary>
    [HttpGet("search/empty-test")]
    public async Task<IActionResult> SearchEmptyTest()
    {
        var request = new PostSearchParameters
        {
            Query = "가나다라마바사아자차카타파하_존재하지않는검색어_" + Guid.NewGuid().ToString()
        };

        var result = await _searchService.SearchPostsAsync(request);
        return Ok(ApiResponse<PagedSearchResult<PostSearchResult>>.Ok(result));
    }

    #endregion

    #region Helper Methods

    private long GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdValue) && 
            long.TryParse(userIdValue, out var userId))
        {
            return userId;
        }
        return 1; // 테스트용 기본값
    }

    private string GetUserName()
    {
        if (Request.Headers.TryGetValue("X-User-Name", out var userName))
        {
            return userName.ToString();
        }
        return "TestUser";
    }

    private bool IsAdmin()
    {
        if (Request.Headers.TryGetValue("X-User-Role", out var role))
        {
            return role.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private long? GetPostIdFromQuery()
    {
        if (Request.Query.TryGetValue("postId", out var postIdValue) && 
            long.TryParse(postIdValue, out var postId))
        {
            return postId;
        }
        return null;
    }

    #endregion
}
