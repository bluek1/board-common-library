using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 3 테스트 컨트롤러 - 파일/검색
/// </summary>
[ApiController]
[Route("api/page3")]
public class TestPage3Controller : ControllerBase
{
    // TODO: 프로덕션에서는 IConfiguration을 통해 appsettings.json에서 설정 값을 읽도록 변경
    // 현재는 테스트 목적으로 하드코딩된 값 사용
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB (appsettings.json의 BoardLibrary:FileUpload:MaxFileSize와 동일)
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" }; // appsettings.json의 BoardLibrary:FileUpload:AllowedExtensions와 동일

    /// <summary>
    /// 파일 업로드 테스트
    /// </summary>
    [HttpPost("files/upload")]
    public IActionResult UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "파일이 없습니다." });
        }

        if (file.Length > MaxFileSize)
        {
            return StatusCode(413, new { error = "파일 크기가 최대 허용 크기(10MB)를 초과합니다." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return StatusCode(415, new { error = $"허용되지 않은 파일 형식입니다. 허용 확장자: {string.Join(", ", AllowedExtensions)}" });
        }

        return Created($"/api/files/{Guid.NewGuid()}", new
        {
            id = Guid.NewGuid(),
            fileName = file.FileName,
            contentType = file.ContentType,
            fileSize = file.Length,
            uploadedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 다중 파일 업로드 테스트
    /// </summary>
    [HttpPost("files/upload-multiple")]
    public IActionResult UploadMultipleFiles([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { error = "파일이 없습니다." });
        }

        var results = files.Select(file => new
        {
            id = Guid.NewGuid(),
            fileName = file.FileName,
            contentType = file.ContentType,
            fileSize = file.Length,
            uploadedAt = DateTime.UtcNow
        }).ToList();

        return Created("/api/files", results);
    }

    /// <summary>
    /// 파일 다운로드 테스트
    /// </summary>
    [HttpGet("files/{id}")]
    public IActionResult DownloadFile(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound(new { error = "파일을 찾을 수 없습니다." });
        }

        // 테스트용 더미 파일 데이터
        var bytes = System.Text.Encoding.UTF8.GetBytes("테스트 파일 내용입니다.");
        return File(bytes, "application/octet-stream", "test-file.txt");
    }

    /// <summary>
    /// 파일 삭제 테스트
    /// </summary>
    [HttpDelete("files/{id}")]
    public IActionResult DeleteFile(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound(new { error = "파일을 찾을 수 없습니다." });
        }

        return NoContent();
    }

    /// <summary>
    /// 썸네일 조회 테스트
    /// </summary>
    [HttpGet("files/{id}/thumbnail")]
    public IActionResult GetThumbnail(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound(new { error = "파일을 찾을 수 없습니다." });
        }

        // 1x1 픽셀 PNG 이미지 (테스트용)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
        return File(pngBytes, "image/png");
    }

    /// <summary>
    /// 통합 검색 테스트
    /// </summary>
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrEmpty(q))
        {
            return BadRequest(new { error = "검색어를 입력해주세요." });
        }

        var results = Enumerable.Range(1, 10).Select(i => new
        {
            id = i,
            title = $"{q}를 포함한 게시물 {i}",
            content = $"본문에 {q}가 포함되어 있습니다...",
            highlightedTitle = $"<mark>{q}</mark>를 포함한 게시물 {i}",
            highlightedContent = $"본문에 <mark>{q}</mark>가 포함되어 있습니다...",
            createdAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        return Ok(new
        {
            query = q,
            data = results,
            meta = new
            {
                page = page,
                pageSize = pageSize,
                totalCount = results.Count,
                totalPages = 1
            }
        });
    }

    /// <summary>
    /// 게시물 검색 테스트
    /// </summary>
    [HttpGet("search/posts")]
    public IActionResult SearchPosts([FromQuery] string q, [FromQuery] string? category = null, [FromQuery] string? tag = null)
    {
        var results = Enumerable.Range(1, 5).Select(i => new
        {
            id = i,
            title = $"{q}를 포함한 게시물 {i}",
            category = category ?? "일반",
            tags = new[] { tag ?? "태그1", "태그2" },
            createdAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        return Ok(results);
    }

    /// <summary>
    /// 태그 검색 테스트
    /// </summary>
    [HttpGet("search/tags")]
    public IActionResult SearchTags([FromQuery] string q)
    {
        var tags = new[] { "ASP.NET", "C#", "게시판", "API", "테스트" }
            .Where(t => t.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(t => new { name = t, postCount = Random.Shared.Next(1, 100) })
            .ToList();

        return Ok(tags);
    }
}
