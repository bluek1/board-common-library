using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 사용자 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;
    
    public UsersController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }
    
    /// <summary>
    /// 내 북마크 목록 조회
    /// </summary>
    /// <remarks>
    /// 현재 로그인한 사용자의 북마크 목록을 조회합니다.
    /// </remarks>
    /// <param name="parameters">쿼리 파라미터</param>
    [HttpGet("me/bookmarks")]
    [ProducesResponseType(typeof(PagedResponse<BookmarkResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<BookmarkResponse>>> GetMyBookmarks(
        [FromQuery] BookmarkQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var result = await _bookmarkService.GetUserBookmarksAsync(userId.Value, parameters);
        return Ok(result);
    }
    
    #region Helper Methods
    
    private long? GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        return null;
    }
    
    #endregion
}
