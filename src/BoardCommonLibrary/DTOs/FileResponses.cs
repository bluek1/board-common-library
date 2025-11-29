namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 파일 정보 응답 DTO
/// </summary>
public class FileInfoResponse
{
    /// <summary>
    /// 파일 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 원본 파일명
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 저장된 파일명
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME 타입
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// 파일 크기 (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// 포맷된 파일 크기 (예: "1.5 MB")
    /// </summary>
    public string FileSizeFormatted { get; set; } = string.Empty;
    
    /// <summary>
    /// 연결된 게시물 ID
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 업로더 ID
    /// </summary>
    public long UploaderId { get; set; }
    
    /// <summary>
    /// 업로더 이름
    /// </summary>
    public string UploaderName { get; set; } = string.Empty;
    
    /// <summary>
    /// 다운로드 횟수
    /// </summary>
    public int DownloadCount { get; set; }
    
    /// <summary>
    /// 이미지 파일 여부
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
    /// 썸네일 URL (이미지인 경우)
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// 다운로드 URL
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 업로드 일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정 일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 파일 업로드 응답 DTO
/// </summary>
public class FileUploadResponse
{
    /// <summary>
    /// 업로드된 파일 정보
    /// </summary>
    public FileInfoResponse File { get; set; } = new();
    
    /// <summary>
    /// 업로드 성공 메시지
    /// </summary>
    public string Message { get; set; } = "파일이 성공적으로 업로드되었습니다.";
}

/// <summary>
/// 다중 파일 업로드 응답 DTO
/// </summary>
public class MultipleFileUploadResponse
{
    /// <summary>
    /// 업로드된 파일 목록
    /// </summary>
    public List<FileInfoResponse> Files { get; set; } = new();
    
    /// <summary>
    /// 총 업로드된 파일 수
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 성공한 파일 수
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// 실패한 파일 수
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// 실패한 파일 목록
    /// </summary>
    public List<FileUploadError> Errors { get; set; } = new();
    
    /// <summary>
    /// 메시지
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 파일 업로드 에러 정보
/// </summary>
public class FileUploadError
{
    /// <summary>
    /// 실패한 파일명
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 에러 코드
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 에러 메시지
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// 파일 다운로드 응답 (스트림 반환용)
/// </summary>
public class FileDownloadResponse
{
    /// <summary>
    /// 파일 스트림
    /// </summary>
    public Stream FileStream { get; set; } = Stream.Null;
    
    /// <summary>
    /// 원본 파일명
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME 타입
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// 파일 크기
    /// </summary>
    public long FileSize { get; set; }
}

/// <summary>
/// 파일 검증 결과
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// 유효 여부
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// 검증 실패 이유 목록
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// 감지된 MIME 타입
    /// </summary>
    public string? DetectedContentType { get; set; }
    
    /// <summary>
    /// 이미지 여부 (이미지 타입인 경우에만 설정)
    /// </summary>
    public bool? IsImage { get; set; }
}
