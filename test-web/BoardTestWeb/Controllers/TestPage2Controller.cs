using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 2 테스트 컨트롤러 - 댓글/좋아요/북마크
/// 실제 라이브러리 서비스를 사용한 통합 테스트
/// </summary>
[ApiController]
[Route("api/page2")]
public class TestPage2Controller : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    private readonly IBookmarkService _bookmarkService;
    
    // 테스트용 사용자 ID (실제 환경에서는 JWT 토큰에서 추출)
    private const long TestUserId = 1;
    private const string TestUserName = "테스트사용자";
    
    public TestPage2Controller(
        ICommentService commentService,
        ILikeService likeService,
        IBookmarkService bookmarkService)
    {
        _commentService = commentService;
        _likeService = likeService;
        _bookmarkService = bookmarkService;
    }

    /// <summary>
    /// 댓글 작성 테스트
    /// </summary>
    [HttpPost("posts/{postId}/comments")]
    public async Task<IActionResult> CreateComment(long postId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var result = await _commentService.CreateAsync(postId, request, TestUserId, TestUserName);
            return Created($"/api/comments/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 댓글 목록 조회 테스트
    /// </summary>
    [HttpGet("posts/{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId, [FromQuery] CommentQueryParameters parameters)
    {
        var result = await _commentService.GetByPostIdAsync(postId, parameters);
        return Ok(result);
    }

    /// <summary>
    /// 댓글 상세 조회 테스트
    /// </summary>
    [HttpGet("comments/{id}")]
    public async Task<IActionResult> GetComment(long id)
    {
        var result = await _commentService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { error = "댓글을 찾을 수 없습니다." });
        }
        return Ok(result);
    }

    /// <summary>
    /// 댓글 수정 테스트
    /// </summary>
    [HttpPut("comments/{id}")]
    public async Task<IActionResult> UpdateComment(long id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var result = await _commentService.UpdateAsync(id, request, TestUserId);
            if (result == null)
            {
                return NotFound(new { error = "댓글을 찾을 수 없습니다." });
            }
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 댓글 삭제 테스트
    /// </summary>
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> DeleteComment(long id)
    {
        try
        {
            var result = await _commentService.DeleteAsync(id, TestUserId);
            if (!result)
            {
                return NotFound(new { error = "댓글을 찾을 수 없습니다." });
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 대댓글 작성 테스트
    /// </summary>
    [HttpPost("comments/{id}/replies")]
    public async Task<IActionResult> CreateReply(long id, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var result = await _commentService.CreateReplyAsync(id, request, TestUserId, TestUserName);
            if (result == null)
            {
                return NotFound(new { error = "부모 댓글을 찾을 수 없습니다." });
            }
            return Created($"/api/comments/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 게시물 좋아요 테스트
    /// </summary>
    [HttpPost("posts/{id}/like")]
    public async Task<IActionResult> LikePost(long id)
    {
        try
        {
            var result = await _likeService.LikePostAsync(id, TestUserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 게시물 좋아요 취소 테스트
    /// </summary>
    [HttpDelete("posts/{id}/like")]
    public async Task<IActionResult> UnlikePost(long id)
    {
        var result = await _likeService.UnlikePostAsync(id, TestUserId);
        if (result == null)
        {
            return NotFound(new { error = "좋아요 기록을 찾을 수 없습니다." });
        }
        return Ok(result);
    }

    /// <summary>
    /// 댓글 좋아요 테스트
    /// </summary>
    [HttpPost("comments/{id}/like")]
    public async Task<IActionResult> LikeComment(long id)
    {
        try
        {
            var result = await _likeService.LikeCommentAsync(id, TestUserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 댓글 좋아요 취소 테스트
    /// </summary>
    [HttpDelete("comments/{id}/like")]
    public async Task<IActionResult> UnlikeComment(long id)
    {
        var result = await _likeService.UnlikeCommentAsync(id, TestUserId);
        if (result == null)
        {
            return NotFound(new { error = "좋아요 기록을 찾을 수 없습니다." });
        }
        return Ok(result);
    }

    /// <summary>
    /// 게시물 북마크 테스트
    /// </summary>
    [HttpPost("posts/{id}/bookmark")]
    public async Task<IActionResult> BookmarkPost(long id)
    {
        try
        {
            var result = await _bookmarkService.AddBookmarkAsync(id, TestUserId);
            return Ok(new { success = result, postId = id, bookmarked = result });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 게시물 북마크 해제 테스트
    /// </summary>
    [HttpDelete("posts/{id}/bookmark")]
    public async Task<IActionResult> UnbookmarkPost(long id)
    {
        var result = await _bookmarkService.RemoveBookmarkAsync(id, TestUserId);
        return Ok(new { success = result, postId = id, bookmarked = false });
    }

    /// <summary>
    /// 내 북마크 목록 테스트
    /// </summary>
    [HttpGet("users/me/bookmarks")]
    public async Task<IActionResult> GetMyBookmarks([FromQuery] BookmarkQueryParameters parameters)
    {
        var result = await _bookmarkService.GetUserBookmarksAsync(TestUserId, parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 북마크 여부 확인
    /// </summary>
    [HttpGet("posts/{id}/bookmark/status")]
    public async Task<IActionResult> CheckBookmarkStatus(long id)
    {
        var isBookmarked = await _bookmarkService.HasUserBookmarkedAsync(id, TestUserId);
        return Ok(new { postId = id, isBookmarked });
    }
    
    /// <summary>
    /// 좋아요 여부 확인
    /// </summary>
    [HttpGet("posts/{id}/like/status")]
    public async Task<IActionResult> CheckLikeStatus(long id)
    {
        var isLiked = await _likeService.HasUserLikedPostAsync(id, TestUserId);
        return Ok(new { postId = id, isLiked });
    }
}
