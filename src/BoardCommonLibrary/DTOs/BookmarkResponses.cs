namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 북마크 응답 DTO
/// </summary>
public class BookmarkResponse
{
    /// <summary>
    /// 북마크 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long PostId { get; set; }
    
    /// <summary>
    /// 게시물 정보
    /// </summary>
    public PostSummaryResponse Post { get; set; } = null!;
    
    /// <summary>
    /// 북마크 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 북마크 쿼리 파라미터
/// </summary>
public class BookmarkQueryParameters
{
    /// <summary>
    /// 페이지 번호 (기본값 1)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지 크기 (기본값 20, 최대 100)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 정렬 기준 (createdAt)
    /// </summary>
    public string SortBy { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string SortOrder { get; set; } = "desc";
}
