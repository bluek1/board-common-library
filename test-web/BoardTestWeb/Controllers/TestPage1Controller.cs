using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 1 테스트 컨트롤러 - 게시물 관리
/// 실제 BoardCommonLibrary 서비스를 호출하여 테스트합니다.
/// </summary>
[ApiController]
[Route("api/page1")]
public class TestPage1Controller : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IViewCountService _viewCountService;
    
    public TestPage1Controller(IPostService postService, IViewCountService viewCountService)
    {
        _postService = postService;
        _viewCountService = viewCountService;
    }

    /// <summary>
    /// 게시물 작성 테스트 (T1-001, T1-002)
    /// </summary>
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            // 테스트용 사용자 ID (헤더에서 가져오거나 기본값 사용)
            var userId = GetUserId();
            var userName = GetUserName();
            
            var post = await _postService.CreateAsync(request, userId, userName);
            return Created($"/api/posts/{post.Id}", ApiResponse<PostResponse>.Ok(post));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new ValidationError 
            { 
                Field = e.PropertyName, 
                Message = e.ErrorMessage 
            }).ToList();
            
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "입력값이 유효하지 않습니다.", errors));
        }
    }

    /// <summary>
    /// 게시물 조회 테스트 (T1-003, T1-004, T1-010)
    /// </summary>
    [HttpGet("posts/{id:long}")]
    public async Task<IActionResult> GetPost(long id)
    {
        var post = await _postService.GetByIdAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create("POST_NOT_FOUND", "게시물을 찾을 수 없습니다."));
        }
        
        // 조회수 증가
        var userId = GetUserIdNullable();
        var ipAddress = GetClientIpAddress();
        await _viewCountService.IncrementViewCountAsync(id, userId, ipAddress);
        
        // 조회수 갱신
        post.ViewCount = await _viewCountService.GetViewCountAsync(id);
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }

    /// <summary>
    /// 게시물 목록 조회 테스트 (T1-008, T1-009)
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] PostQueryParameters parameters)
    {
        var result = await _postService.GetAllAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// 게시물 수정 테스트 (T1-005, T1-006)
    /// </summary>
    [HttpPut("posts/{id:long}")]
    public async Task<IActionResult> UpdatePost(long id, [FromBody] UpdatePostRequest request)
    {
        if (!await _postService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create("POST_NOT_FOUND", "게시물을 찾을 수 없습니다."));
        }
        
        var userId = GetUserId();
        var isAdmin = IsAdmin();
        var isAuthor = await _postService.IsAuthorAsync(id, userId);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(403, ApiErrorResponse.Create("FORBIDDEN", "게시물을 수정할 권한이 없습니다."));
        }
        
        var post = await _postService.UpdateAsync(id, request, userId, isAdmin);
        return Ok(ApiResponse<PostResponse>.Ok(post!));
    }

    /// <summary>
    /// 게시물 삭제 테스트 (T1-007)
    /// </summary>
    [HttpDelete("posts/{id:long}")]
    public async Task<IActionResult> DeletePost(long id)
    {
        if (!await _postService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create("POST_NOT_FOUND", "게시물을 찾을 수 없습니다."));
        }
        
        var userId = GetUserId();
        var isAdmin = IsAdmin();
        var isAuthor = await _postService.IsAuthorAsync(id, userId);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(403, ApiErrorResponse.Create("FORBIDDEN", "게시물을 삭제할 권한이 없습니다."));
        }
        
        await _postService.DeleteAsync(id, userId, isAdmin);
        return NoContent();
    }

    /// <summary>
    /// 상단고정 설정 테스트 (T1-012)
    /// </summary>
    [HttpPost("posts/{id:long}/pin")]
    public async Task<IActionResult> PinPost(long id)
    {
        if (!IsAdmin())
        {
            return StatusCode(403, ApiErrorResponse.Create("FORBIDDEN", "관리자만 상단고정할 수 있습니다."));
        }
        
        var post = await _postService.PinAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create("POST_NOT_FOUND", "게시물을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }

    /// <summary>
    /// 상단고정 해제 테스트 (T1-013)
    /// </summary>
    [HttpDelete("posts/{id:long}/pin")]
    public async Task<IActionResult> UnpinPost(long id)
    {
        if (!IsAdmin())
        {
            return StatusCode(403, ApiErrorResponse.Create("FORBIDDEN", "관리자만 상단고정을 해제할 수 있습니다."));
        }
        
        var post = await _postService.UnpinAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create("POST_NOT_FOUND", "게시물을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }

    /// <summary>
    /// 임시저장 테스트 (T1-014)
    /// </summary>
    [HttpPost("posts/draft")]
    public async Task<IActionResult> SaveDraft([FromBody] DraftPostRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        var draft = await _postService.SaveDraftAsync(request, userId, userName);
        return Ok(ApiResponse<DraftPostResponse>.Ok(draft));
    }

    /// <summary>
    /// 임시저장 목록 테스트 (T1-015)
    /// </summary>
    [HttpGet("posts/draft")]
    public async Task<IActionResult> GetDrafts([FromQuery] PagedRequest parameters)
    {
        var userId = GetUserId();
        var drafts = await _postService.GetDraftsAsync(userId, parameters);
        return Ok(drafts);
    }
    
    /// <summary>
    /// 조회수 중복 방지 테스트 (T1-011)
    /// </summary>
    [HttpGet("posts/{id:long}/view-check")]
    public async Task<IActionResult> CheckViewDuplicate(long id)
    {
        var userId = GetUserIdNullable();
        var ipAddress = GetClientIpAddress();
        
        var hasViewed = await _viewCountService.HasViewedAsync(id, userId, ipAddress);
        return Ok(new { postId = id, hasViewed = hasViewed });
    }
    
    #region Helper Methods
    
    private long GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        return 1; // 테스트용 기본 사용자 ID
    }
    
    private long? GetUserIdNullable()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        return null;
    }
    
    private string GetUserName()
    {
        if (Request.Headers.TryGetValue("X-User-Name", out var userNameHeader))
        {
            return userNameHeader.ToString();
        }
        return "TestUser";
    }
    
    private bool IsAdmin()
    {
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().Equals("admin", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
    
    private string? GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ipAddress = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
        }
        
        return ipAddress;
    }
    
    #endregion
}
