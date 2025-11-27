using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 2 테스트 컨트롤러 - 댓글/좋아요
/// </summary>
[ApiController]
[Route("api/page2")]
public class TestPage2Controller : ControllerBase
{
    /// <summary>
    /// 댓글 작성 테스트
    /// </summary>
    [HttpPost("posts/{postId}/comments")]
    public IActionResult CreateComment(int postId, [FromBody] CreateCommentRequest request)
    {
        if (string.IsNullOrEmpty(request.Content))
        {
            return BadRequest(new { error = "댓글 내용은 필수입니다." });
        }

        return Created($"/api/comments/{1}", new
        {
            id = 1,
            postId = postId,
            content = request.Content,
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 댓글 목록 조회 테스트
    /// </summary>
    [HttpGet("posts/{postId}/comments")]
    public IActionResult GetComments(int postId)
    {
        var comments = Enumerable.Range(1, 5).Select(i => new
        {
            id = i,
            postId = postId,
            content = $"댓글 {i}",
            authorName = $"사용자{i}",
            likeCount = Random.Shared.Next(0, 50),
            createdAt = DateTime.UtcNow.AddHours(-i),
            replies = Enumerable.Range(1, 2).Select(j => new
            {
                id = i * 10 + j,
                parentId = i,
                content = $"대댓글 {j}",
                authorName = $"사용자{j + 5}",
                createdAt = DateTime.UtcNow.AddHours(-i).AddMinutes(-j * 30)
            }).ToList()
        }).ToList();

        return Ok(comments);
    }

    /// <summary>
    /// 댓글 수정 테스트
    /// </summary>
    [HttpPut("comments/{id}")]
    public IActionResult UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "댓글을 찾을 수 없습니다." });
        }

        return Ok(new { id = id, content = request.Content, updatedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// 댓글 삭제 테스트
    /// </summary>
    [HttpDelete("comments/{id}")]
    public IActionResult DeleteComment(int id)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "댓글을 찾을 수 없습니다." });
        }

        return NoContent();
    }

    /// <summary>
    /// 대댓글 작성 테스트
    /// </summary>
    [HttpPost("comments/{id}/replies")]
    public IActionResult CreateReply(int id, [FromBody] CreateCommentRequest request)
    {
        return Created($"/api/comments/{100}", new
        {
            id = 100,
            parentId = id,
            content = request.Content,
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 게시물 좋아요 테스트
    /// </summary>
    [HttpPost("posts/{id}/like")]
    public IActionResult LikePost(int id)
    {
        return Ok(new { id = id, liked = true, likeCount = 101 });
    }

    /// <summary>
    /// 게시물 좋아요 취소 테스트
    /// </summary>
    [HttpDelete("posts/{id}/like")]
    public IActionResult UnlikePost(int id)
    {
        return Ok(new { id = id, liked = false, likeCount = 100 });
    }

    /// <summary>
    /// 댓글 좋아요 테스트
    /// </summary>
    [HttpPost("comments/{id}/like")]
    public IActionResult LikeComment(int id)
    {
        return Ok(new { id = id, liked = true, likeCount = 11 });
    }

    /// <summary>
    /// 게시물 북마크 테스트
    /// </summary>
    [HttpPost("posts/{id}/bookmark")]
    public IActionResult BookmarkPost(int id)
    {
        return Ok(new { id = id, bookmarked = true });
    }

    /// <summary>
    /// 게시물 북마크 해제 테스트
    /// </summary>
    [HttpDelete("posts/{id}/bookmark")]
    public IActionResult UnbookmarkPost(int id)
    {
        return Ok(new { id = id, bookmarked = false });
    }

    /// <summary>
    /// 내 북마크 목록 테스트
    /// </summary>
    [HttpGet("users/me/bookmarks")]
    public IActionResult GetMyBookmarks()
    {
        var bookmarks = Enumerable.Range(1, 5).Select(i => new
        {
            id = i,
            postId = i * 10,
            postTitle = $"북마크한 게시물 {i}",
            bookmarkedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        return Ok(bookmarks);
    }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
