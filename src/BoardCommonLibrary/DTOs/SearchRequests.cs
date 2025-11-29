using System.ComponentModel.DataAnnotations;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 검색 요청 파라미터
/// </summary>
public class SearchParameters
{
    /// <summary>
    /// 검색어 (필수)
    /// </summary>
    [Required(ErrorMessage = "검색어는 필수입니다.")]
    [MinLength(2, ErrorMessage = "검색어는 최소 2자 이상이어야 합니다.")]
    [MaxLength(100, ErrorMessage = "검색어는 최대 100자입니다.")]
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 페이지 번호 (1부터 시작)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "페이지 번호는 1 이상이어야 합니다.")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지당 항목 수
    /// </summary>
    [Range(1, 100, ErrorMessage = "페이지 크기는 1~100 사이여야 합니다.")]
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 검색 대상 (title, content, all)
    /// </summary>
    public string SearchIn { get; set; } = "all";
    
    /// <summary>
    /// 정렬 필드 (relevance, createdAt, viewCount, likeCount)
    /// </summary>
    public string Sort { get; set; } = "relevance";
    
    /// <summary>
    /// 정렬 방향 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
}

/// <summary>
/// 게시물 검색 요청 파라미터
/// </summary>
public class PostSearchParameters : SearchParameters
{
    /// <summary>
    /// 카테고리 필터
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 필터 (쉼표로 구분)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// 작성자 ID 필터
    /// </summary>
    public long? AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명 필터
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// 게시물 상태 필터 (Published, Draft, Archived)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// 시작 날짜 필터
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// 종료 날짜 필터
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// 상단고정 게시물 포함 여부
    /// </summary>
    public bool IncludePinned { get; set; } = true;
    
    /// <summary>
    /// 최소 조회수 필터
    /// </summary>
    public int? MinViewCount { get; set; }
    
    /// <summary>
    /// 최소 좋아요 수 필터
    /// </summary>
    public int? MinLikeCount { get; set; }
    
    /// <summary>
    /// 첨부파일 있는 게시물만
    /// </summary>
    public bool? HasAttachments { get; set; }
}

/// <summary>
/// 태그 검색 요청 파라미터
/// </summary>
public class TagSearchParameters
{
    /// <summary>
    /// 검색어 (필수)
    /// </summary>
    [Required(ErrorMessage = "검색어는 필수입니다.")]
    [MinLength(1, ErrorMessage = "검색어는 최소 1자 이상이어야 합니다.")]
    [MaxLength(50, ErrorMessage = "검색어는 최대 50자입니다.")]
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 최대 결과 수
    /// </summary>
    [Range(1, 50, ErrorMessage = "최대 결과 수는 1~50 사이여야 합니다.")]
    public int Limit { get; set; } = 10;
    
    /// <summary>
    /// 최소 사용 횟수 필터
    /// </summary>
    public int? MinUsageCount { get; set; }
}

/// <summary>
/// 작성자 검색 요청 파라미터
/// </summary>
public class AuthorSearchParameters
{
    /// <summary>
    /// 검색어 (필수)
    /// </summary>
    [Required(ErrorMessage = "검색어는 필수입니다.")]
    [MinLength(2, ErrorMessage = "검색어는 최소 2자 이상이어야 합니다.")]
    [MaxLength(50, ErrorMessage = "검색어는 최대 50자입니다.")]
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 최대 결과 수
    /// </summary>
    [Range(1, 50, ErrorMessage = "최대 결과 수는 1~50 사이여야 합니다.")]
    public int Limit { get; set; } = 10;
    
    /// <summary>
    /// 최소 게시물 수 필터
    /// </summary>
    public int? MinPostCount { get; set; }
}

/// <summary>
/// 통합 검색 요청 파라미터
/// </summary>
public class UnifiedSearchParameters : SearchParameters
{
    /// <summary>
    /// 검색 대상 타입 (posts, tags, authors, all)
    /// </summary>
    public string SearchType { get; set; } = "all";
    
    /// <summary>
    /// 카테고리 필터 (게시물 검색 시)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 필터 (게시물 검색 시)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// 날짜 범위 시작 (게시물 검색 시)
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// 날짜 범위 종료 (게시물 검색 시)
    /// </summary>
    public DateTime? ToDate { get; set; }
}
