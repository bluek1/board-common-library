using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 단일 파일 업로드 요청
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// 업로드할 파일
    /// </summary>
    [Required(ErrorMessage = "파일은 필수입니다.")]
    public IFormFile File { get; set; } = null!;
    
    /// <summary>
    /// 연결할 게시물 ID (선택)
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 파일 설명 (선택)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// 다중 파일 업로드 요청
/// </summary>
public class MultipleFileUploadRequest
{
    /// <summary>
    /// 업로드할 파일 목록
    /// </summary>
    [Required(ErrorMessage = "파일은 필수입니다.")]
    public List<IFormFile> Files { get; set; } = new();
    
    /// <summary>
    /// 연결할 게시물 ID (선택)
    /// </summary>
    public long? PostId { get; set; }
}

/// <summary>
/// 파일 목록 조회 요청
/// </summary>
public class FileQueryParameters
{
    /// <summary>
    /// 페이지 번호 (1부터 시작)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지당 항목 수
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 게시물 ID로 필터링
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 업로더 ID로 필터링
    /// </summary>
    public long? UploaderId { get; set; }
    
    /// <summary>
    /// MIME 타입으로 필터링 (예: image/*, application/pdf)
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// 이미지만 조회
    /// </summary>
    public bool? IsImage { get; set; }
    
    /// <summary>
    /// 정렬 필드 (createdAt, fileName, fileSize)
    /// </summary>
    public string Sort { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 방향 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
}

/// <summary>
/// 파일을 게시물에 연결하는 요청
/// </summary>
public class AttachFileToPostRequest
{
    /// <summary>
    /// 파일 ID 목록
    /// </summary>
    [Required(ErrorMessage = "파일 ID는 필수입니다.")]
    public List<long> FileIds { get; set; } = new();
    
    /// <summary>
    /// 연결할 게시물 ID
    /// </summary>
    [Required(ErrorMessage = "게시물 ID는 필수입니다.")]
    public long PostId { get; set; }
}
