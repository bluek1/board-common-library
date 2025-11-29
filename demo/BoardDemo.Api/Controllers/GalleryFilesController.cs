using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 갤러리(이미지) 파일 컨트롤러
/// 이미지 파일만 업로드 가능한 갤러리 게시판
/// </summary>
[Route("api/gallery")]
[Tags("갤러리")]
public class GalleryFilesController : FilesController
{
    // 허용된 이미지 확장자
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    // 이미지 MIME 타입
    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp"
    };

    // 최대 파일 크기 (5MB)
    private const long MaxImageSize = 5 * 1024 * 1024;

    public GalleryFilesController(IFileService fileService)
        : base(fileService)
    {
    }

    /// <summary>
    /// 갤러리 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetGalleryInfo()
    {
        return Ok(new
        {
            BoardType = "Gallery",
            AllowedExtensions = AllowedImageExtensions.ToArray(),
            MaxFileSizeMB = MaxImageSize / (1024 * 1024),
            Description = "이미지 파일만 업로드 가능한 갤러리입니다."
        });
    }

    /// <summary>
    /// 갤러리 이미지 업로드 (이미지만 허용)
    /// </summary>
    [HttpPost("upload")]
    public override async Task<IActionResult> Upload(IFormFile file, [FromQuery] long? postId = null)
    {
        // 이미지 파일 검증
        var validationResult = ValidateImageFile(file);
        if (validationResult != null)
        {
            return validationResult;
        }

        return await base.Upload(file, postId);
    }

    /// <summary>
    /// 갤러리 다중 이미지 업로드
    /// </summary>
    [HttpPost("upload/multiple")]
    public override async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] long? postId = null)
    {
        // 모든 파일이 이미지인지 검증
        foreach (var file in files)
        {
            var validationResult = ValidateImageFile(file);
            if (validationResult != null)
            {
                return validationResult;
            }
        }

        return await base.UploadMultiple(files, postId);
    }

    /// <summary>
    /// 갤러리 이미지 썸네일 조회
    /// </summary>
    [HttpGet("{id:long}/thumbnail")]
    public override async Task<IActionResult> GetThumbnail(long id)
    {
        return await base.GetThumbnail(id);
    }

    /// <summary>
    /// 이미지 파일 검증
    /// </summary>
    private IActionResult? ValidateImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "파일이 필요합니다." });
        }

        // 확장자 검증
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"이미지 파일만 업로드 가능합니다. 허용 형식: {string.Join(", ", AllowedImageExtensions)}" });
        }

        // MIME 타입 검증
        if (!AllowedImageMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = "올바른 이미지 파일이 아닙니다." });
        }

        // 파일 크기 검증
        if (file.Length > MaxImageSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, 
                new { message = $"이미지 크기는 {MaxImageSize / (1024 * 1024)}MB 이하여야 합니다." });
        }

        return null;
    }
}
