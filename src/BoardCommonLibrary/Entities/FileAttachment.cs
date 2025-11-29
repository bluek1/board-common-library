using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 파일 첨부 필수 항목 인터페이스
/// </summary>
public interface IFileAttachment : IEntity
{
    string FileName { get; set; }
    string ContentType { get; set; }
    long FileSize { get; set; }
    string StoragePath { get; set; }
}

/// <summary>
/// 파일 첨부 엔티티
/// </summary>
public class FileAttachment : EntityBase, IFileAttachment, ISoftDeletable, IHasExtendedProperties
{
    /// <summary>
    /// 원본 파일명 (필수, 최대 255자)
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 저장 파일명 (UUID 기반, 최대 255자)
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME 타입 (필수, 최대 100자)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// 파일 크기 (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// 저장 경로 (최대 500자)
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 썸네일 경로 (이미지인 경우, 최대 500자)
    /// </summary>
    public string? ThumbnailPath { get; set; }
    
    /// <summary>
    /// 게시물 ID (FK, null 가능 - 임시 파일)
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 업로더 ID
    /// </summary>
    public long UploaderId { get; set; }
    
    /// <summary>
    /// 업로더명
    /// </summary>
    public string? UploaderName { get; set; }
    
    /// <summary>
    /// 다운로드 횟수
    /// </summary>
    public int DownloadCount { get; set; }
    
    /// <summary>
    /// 이미지 여부
    /// </summary>
    public bool IsImage { get; set; }
    
    /// <summary>
    /// 이미지 너비 (이미지인 경우)
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// 이미지 높이 (이미지인 경우)
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// 삭제 여부 (소프트 삭제)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 삭제일시
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// 동적 확장 필드
    /// </summary>
    public Dictionary<string, object>? ExtendedProperties { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// 연결된 게시물
    /// </summary>
    public virtual Post? Post { get; set; }
}
