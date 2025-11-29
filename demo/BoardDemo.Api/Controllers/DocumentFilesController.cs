using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 문서 파일 컨트롤러
/// 문서 파일만 업로드 가능한 자료실 게시판
/// </summary>
[Route("api/documents")]
[Tags("자료실")]
public class DocumentFilesController : FilesController
{
    // 허용된 문서 확장자
    private static readonly HashSet<string> AllowedDocExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".hwp", ".zip"
    };

    // 최대 파일 크기 (50MB)
    private const long MaxDocumentSize = 50 * 1024 * 1024;

    public DocumentFilesController(IFileService fileService)
        : base(fileService)
    {
    }

    /// <summary>
    /// 자료실 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetDocumentInfo()
    {
        return Ok(new
        {
            BoardType = "Documents",
            AllowedExtensions = AllowedDocExtensions.ToArray(),
            MaxFileSizeMB = MaxDocumentSize / (1024 * 1024),
            Description = "문서 파일만 업로드 가능한 자료실입니다."
        });
    }

    /// <summary>
    /// 자료실 문서 업로드 (문서만 허용)
    /// </summary>
    [HttpPost("upload")]
    public override async Task<IActionResult> Upload(IFormFile file, [FromQuery] long? postId = null)
    {
        // 문서 파일 검증
        var validationResult = ValidateDocumentFile(file);
        if (validationResult != null)
        {
            return validationResult;
        }

        return await base.Upload(file, postId);
    }

    /// <summary>
    /// 자료실 다중 문서 업로드
    /// </summary>
    [HttpPost("upload/multiple")]
    public override async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] long? postId = null)
    {
        // 모든 파일이 문서인지 검증
        foreach (var file in files)
        {
            var validationResult = ValidateDocumentFile(file);
            if (validationResult != null)
            {
                return validationResult;
            }
        }

        return await base.UploadMultiple(files, postId);
    }

    /// <summary>
    /// 자료실 문서 다운로드
    /// </summary>
    [HttpGet("{id:long}/download")]
    public override async Task<IActionResult> Download(long id)
    {
        // 다운로드 횟수 로깅 등 추가 가능
        return await base.Download(id);
    }

    /// <summary>
    /// 문서 파일 검증
    /// </summary>
    private IActionResult? ValidateDocumentFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "파일이 필요합니다." });
        }

        // 확장자 검증
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedDocExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"문서 파일만 업로드 가능합니다. 허용 형식: {string.Join(", ", AllowedDocExtensions)}" });
        }

        // 파일 크기 검증
        if (file.Length > MaxDocumentSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { message = $"문서 크기는 {MaxDocumentSize / (1024 * 1024)}MB 이하여야 합니다." });
        }

        return null;
    }
}
