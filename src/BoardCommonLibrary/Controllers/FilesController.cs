using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 파일 관리 API 컨트롤러
/// 상속하여 커스터마이징 가능합니다.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    protected readonly IFileService FileService;
    
    /// <summary>
    /// 파일 컨트롤러 생성자
    /// </summary>
    public FilesController(IFileService fileService)
    {
        FileService = fileService;
    }
    
    /// <summary>
    /// 단일 파일 업로드
    /// </summary>
    /// <param name="file">업로드할 파일</param>
    /// <param name="postId">연결할 게시물 ID (선택)</param>
    /// <returns>업로드된 파일 정보</returns>
    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public virtual async Task<IActionResult> Upload(IFormFile file, [FromQuery] long? postId = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "파일이 필요합니다." });
        }
        
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        
        try
        {
            var result = await FileService.UploadAsync(file, userId, userName, postId);
            
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new FileUploadResponse
            {
                File = result,
                Message = "파일이 성공적으로 업로드되었습니다."
            });
        }
        catch (InvalidOperationException ex)
        {
            // 검증 실패
            if (ex.Message.Contains("크기"))
                return StatusCode(StatusCodes.Status413PayloadTooLarge, new { message = ex.Message });
            if (ex.Message.Contains("형식") || ex.Message.Contains("타입"))
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { message = ex.Message });
            
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// 다중 파일 업로드
    /// </summary>
    /// <param name="files">업로드할 파일 목록</param>
    /// <param name="postId">연결할 게시물 ID (선택)</param>
    /// <returns>업로드 결과</returns>
    [HttpPost("upload/multiple")]
    [Authorize]
    [ProducesResponseType(typeof(MultipleFileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] long? postId = null)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { message = "파일이 필요합니다." });
        }
        
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        
        var result = await FileService.UploadMultipleAsync(files, userId, userName, postId);
        
        if (result.SuccessCount == 0)
        {
            return BadRequest(result);
        }
        
        return StatusCode(StatusCodes.Status201Created, result);
    }
    
    /// <summary>
    /// 파일 정보 조회
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>파일 정보</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(FileInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetById(long id)
    {
        var file = await FileService.GetByIdAsync(id);
        
        if (file == null)
        {
            return NotFound(new { message = "파일을 찾을 수 없습니다." });
        }
        
        return Ok(file);
    }
    
    /// <summary>
    /// 파일 다운로드
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>파일 스트림</returns>
    [HttpGet("{id:long}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Download(long id)
    {
        var download = await FileService.DownloadAsync(id);
        
        if (download == null)
        {
            return NotFound(new { message = "파일을 찾을 수 없습니다." });
        }
        
        return File(download.FileStream, download.ContentType, download.FileName);
    }
    
    /// <summary>
    /// 썸네일 조회
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>썸네일 이미지</returns>
    [HttpGet("{id:long}/thumbnail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetThumbnail(long id)
    {
        var thumbnail = await FileService.GetThumbnailAsync(id);
        
        if (thumbnail == null)
        {
            return NotFound(new { message = "썸네일을 찾을 수 없습니다." });
        }
        
        return File(thumbnail.FileStream, thumbnail.ContentType);
    }
    
    /// <summary>
    /// 파일 삭제
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>삭제 결과</returns>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Delete(long id)
    {
        var userId = GetCurrentUserId();
        var result = await FileService.DeleteAsync(id, userId);
        
        if (!result)
        {
            var exists = await FileService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = "파일을 찾을 수 없습니다." });
            }
            
            return Forbid();
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// 파일 목록 조회
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>파일 목록</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<FileInfoResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAll([FromQuery] FileQueryParameters parameters)
    {
        var result = await FileService.GetAllAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 게시물의 첨부파일 목록 조회
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <returns>첨부파일 목록</returns>
    [HttpGet("post/{postId:long}")]
    [ProducesResponseType(typeof(List<FileInfoResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetByPost(long postId)
    {
        var files = await FileService.GetByPostIdAsync(postId);
        return Ok(files);
    }
    
    /// <summary>
    /// 내 파일 목록 조회
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>파일 목록</returns>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<FileInfoResponse>), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetMyFiles([FromQuery] FileQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        var result = await FileService.GetByUploaderAsync(userId, parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 파일을 게시물에 연결
    /// </summary>
    /// <param name="request">연결 요청</param>
    /// <returns>연결 결과</returns>
    [HttpPost("attach")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> AttachToPost([FromBody] AttachFileToPostRequest request)
    {
        var successCount = 0;
        foreach (var fileId in request.FileIds)
        {
            if (await FileService.AttachToPostAsync(fileId, request.PostId))
            {
                successCount++;
            }
        }
        
        return Ok(new { message = $"{successCount}개 파일이 게시물에 연결되었습니다.", successCount });
    }
    
    /// <summary>
    /// 파일을 게시물에서 분리
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>분리 결과</returns>
    [HttpDelete("{id:long}/detach")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> DetachFromPost(long id)
    {
        var result = await FileService.DetachFromPostAsync(id);
        
        if (!result)
        {
            return NotFound(new { message = "파일을 찾을 수 없습니다." });
        }
        
        return Ok(new { message = "파일이 게시물에서 분리되었습니다." });
    }
    
    /// <summary>
    /// 현재 사용자 ID 가져오기
    /// </summary>
    protected virtual long GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var claimUserId))
        {
            return claimUserId;
        }
        
        return 0;
    }
    
    /// <summary>
    /// 현재 사용자 이름 가져오기
    /// </summary>
    protected virtual string GetCurrentUserName()
    {
        if (Request.Headers.TryGetValue("X-User-Name", out var userNameHeader))
        {
            return userNameHeader!;
        }
        
        var nameClaim = User.FindFirst("name") ?? User.FindFirst("userName");
        return nameClaim?.Value ?? "Unknown";
    }
}
