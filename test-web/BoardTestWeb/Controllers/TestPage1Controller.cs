using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 1 테스트 컨트롤러 - 게시물 관리
/// </summary>
[ApiController]
[Route("api/page1")]
public class TestPage1Controller : ControllerBase
{
    /// <summary>
    /// 게시물 작성 테스트
    /// </summary>
    [HttpPost("posts")]
    public IActionResult CreatePost([FromBody] CreatePostRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
        {
            return BadRequest(new { error = "제목은 필수입니다." });
        }

        return Created($"/api/posts/{1}", new { id = 1, title = request.Title, content = request.Content });
    }

    /// <summary>
    /// 게시물 조회 테스트
    /// </summary>
    [HttpGet("posts/{id}")]
    public IActionResult GetPost(int id)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "게시물을 찾을 수 없습니다." });
        }

        return Ok(new
        {
            id = id,
            title = "테스트 게시물",
            content = "테스트 내용입니다.",
            viewCount = 100,
            isPinned = false,
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 게시물 목록 조회 테스트
    /// </summary>
    [HttpGet("posts")]
    public IActionResult GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null)
    {
        var posts = Enumerable.Range(1, pageSize).Select(i => new
        {
            id = (page - 1) * pageSize + i,
            title = $"게시물 {(page - 1) * pageSize + i}",
            viewCount = Random.Shared.Next(0, 1000),
            createdAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
        }).ToList();

        return Ok(new
        {
            data = posts,
            meta = new
            {
                page = page,
                pageSize = pageSize,
                totalCount = 100,
                totalPages = 5
            }
        });
    }

    /// <summary>
    /// 게시물 수정 테스트
    /// </summary>
    [HttpPut("posts/{id}")]
    public IActionResult UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "게시물을 찾을 수 없습니다." });
        }

        return Ok(new { id = id, title = request.Title, content = request.Content, updatedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// 게시물 삭제 테스트
    /// </summary>
    [HttpDelete("posts/{id}")]
    public IActionResult DeletePost(int id)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "게시물을 찾을 수 없습니다." });
        }

        return NoContent();
    }

    /// <summary>
    /// 상단고정 설정 테스트
    /// </summary>
    [HttpPost("posts/{id}/pin")]
    public IActionResult PinPost(int id)
    {
        return Ok(new { id = id, isPinned = true });
    }

    /// <summary>
    /// 상단고정 해제 테스트
    /// </summary>
    [HttpDelete("posts/{id}/pin")]
    public IActionResult UnpinPost(int id)
    {
        return Ok(new { id = id, isPinned = false });
    }

    /// <summary>
    /// 임시저장 테스트
    /// </summary>
    [HttpPost("posts/draft")]
    public IActionResult SaveDraft([FromBody] CreatePostRequest request)
    {
        return Ok(new { draftId = Guid.NewGuid(), title = request.Title, savedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// 임시저장 목록 테스트
    /// </summary>
    [HttpGet("posts/draft")]
    public IActionResult GetDrafts()
    {
        var drafts = Enumerable.Range(1, 3).Select(i => new
        {
            draftId = Guid.NewGuid(),
            title = $"임시저장 {i}",
            savedAt = DateTime.UtcNow.AddHours(-i)
        }).ToList();

        return Ok(drafts);
    }
}

public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
