namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 게시물 생성 요청 DTO
/// </summary>
public class CreatePostRequest
{
    /// <summary>
    /// 게시물 제목 (필수, 최대 200자)
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 게시물 본문 (필수)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 카테고리 (선택)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록 (선택)
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// 게시물 수정 요청 DTO
/// </summary>
public class UpdatePostRequest
{
    /// <summary>
    /// 게시물 제목 (선택)
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 게시물 본문 (선택)
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// 카테고리 (선택)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록 (선택)
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// 임시저장 요청 DTO
/// </summary>
public class DraftPostRequest
{
    /// <summary>
    /// 게시물 제목
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 게시물 본문
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// 카테고리
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록
    /// </summary>
    public List<string>? Tags { get; set; }
    
    /// <summary>
    /// 기존 임시저장 ID (덮어쓰기 시 사용)
    /// </summary>
    public long? ExistingDraftId { get; set; }
}
