namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 페이징 요청 파라미터
/// </summary>
public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 20;
    
    /// <summary>
    /// 페이지 번호 (기본값: 1)
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }
    
    /// <summary>
    /// 페이지 크기 (기본값: 20, 최대: 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : (value > 100 ? 100 : value);
    }
    
    /// <summary>
    /// 정렬 기준 (createdAt, viewCount, title)
    /// </summary>
    public string? SortBy { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// 게시물 목록 조회 파라미터
/// </summary>
public class PostQueryParameters : PagedRequest
{
    /// <summary>
    /// 카테고리 필터
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 작성자 ID 필터
    /// </summary>
    public long? AuthorId { get; set; }
    
    /// <summary>
    /// 태그 필터
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// 검색어 (제목, 본문)
    /// </summary>
    public string? Search { get; set; }
}

/// <summary>
/// 페이징 응답 메타데이터
/// </summary>
public class PagedMetaData
{
    /// <summary>
    /// 현재 페이지 번호
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 전체 항목 수
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 전체 페이지 수
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// 이전 페이지 존재 여부
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// 다음 페이지 존재 여부
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// 페이징된 응답
/// </summary>
/// <typeparam name="T">데이터 타입</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// 성공 여부
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// 데이터 목록
    /// </summary>
    public List<T> Data { get; set; } = new();
    
    /// <summary>
    /// 페이징 메타데이터
    /// </summary>
    public PagedMetaData Meta { get; set; } = new();
    
    /// <summary>
    /// 페이징 응답 생성
    /// </summary>
    public static PagedResponse<T> Create(List<T> data, int page, int pageSize, int totalCount)
    {
        return new PagedResponse<T>
        {
            Data = data,
            Meta = new PagedMetaData
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        };
    }
}
