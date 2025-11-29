namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 게시물 검색 결과 DTO
/// </summary>
public class PostSearchResult
{
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 제목 (검색어 하이라이트)
    /// </summary>
    public string TitleHighlighted { get; set; } = string.Empty;
    
    /// <summary>
    /// 본문 미리보기
    /// </summary>
    public string ContentPreview { get; set; } = string.Empty;
    
    /// <summary>
    /// 본문 미리보기 (검색어 하이라이트)
    /// </summary>
    public string ContentPreviewHighlighted { get; set; } = string.Empty;
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자 이름
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 카테고리
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 상태
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 조회수
    /// </summary>
    public int ViewCount { get; set; }
    
    /// <summary>
    /// 좋아요 수
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 댓글 수
    /// </summary>
    public int CommentCount { get; set; }
    
    /// <summary>
    /// 첨부파일 수
    /// </summary>
    public int AttachmentCount { get; set; }
    
    /// <summary>
    /// 상단고정 여부
    /// </summary>
    public bool IsPinned { get; set; }
    
    /// <summary>
    /// 작성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 검색 관련성 점수 (옵션)
    /// </summary>
    public double? RelevanceScore { get; set; }
}

/// <summary>
/// 태그 검색 결과 DTO
/// </summary>
public class TagSearchResult
{
    /// <summary>
    /// 태그 이름
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 태그 이름 (검색어 하이라이트)
    /// </summary>
    public string NameHighlighted { get; set; } = string.Empty;
    
    /// <summary>
    /// 사용 횟수
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// 최근 사용일
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// 작성자 검색 결과 DTO
/// </summary>
public class AuthorSearchResult
{
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 작성자 이름
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 작성자 이름 (검색어 하이라이트)
    /// </summary>
    public string NameHighlighted { get; set; } = string.Empty;
    
    /// <summary>
    /// 이메일 (마스킹 처리)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 작성한 게시물 수
    /// </summary>
    public int PostCount { get; set; }
    
    /// <summary>
    /// 작성한 댓글 수
    /// </summary>
    public int CommentCount { get; set; }
    
    /// <summary>
    /// 가입일
    /// </summary>
    public DateTime? JoinedAt { get; set; }
}

/// <summary>
/// 통합 검색 결과 DTO
/// </summary>
public class UnifiedSearchResult
{
    /// <summary>
    /// 게시물 검색 결과
    /// </summary>
    public SearchResultSection<PostSearchResult> Posts { get; set; } = new();
    
    /// <summary>
    /// 태그 검색 결과
    /// </summary>
    public SearchResultSection<TagSearchResult> Tags { get; set; } = new();
    
    /// <summary>
    /// 작성자 검색 결과
    /// </summary>
    public SearchResultSection<AuthorSearchResult> Authors { get; set; } = new();
    
    /// <summary>
    /// 검색어
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 검색 소요 시간 (밀리초)
    /// </summary>
    public long SearchTimeMs { get; set; }
}

/// <summary>
/// 검색 결과 섹션 DTO
/// </summary>
/// <typeparam name="T">결과 항목 타입</typeparam>
public class SearchResultSection<T>
{
    /// <summary>
    /// 결과 항목 목록
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// 총 결과 수
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 더 많은 결과가 있는지 여부
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// 페이징된 검색 결과 DTO
/// </summary>
/// <typeparam name="T">결과 항목 타입</typeparam>
public class PagedSearchResult<T>
{
    /// <summary>
    /// 결과 항목 목록
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// 검색어
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 총 결과 수
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 총 페이지 수
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// 현재 페이지
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 이전 페이지 존재 여부
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    /// 다음 페이지 존재 여부
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
    
    /// <summary>
    /// 검색 소요 시간 (밀리초)
    /// </summary>
    public long SearchTimeMs { get; set; }
}

/// <summary>
/// 인기 검색어 결과 DTO
/// </summary>
public class PopularSearchResult
{
    /// <summary>
    /// 검색어
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// 검색 횟수
    /// </summary>
    public int SearchCount { get; set; }
    
    /// <summary>
    /// 순위
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// 트렌드 (up, down, same)
    /// </summary>
    public string Trend { get; set; } = "same";
    
    /// <summary>
    /// 순위 변동 (양수: 상승, 음수: 하락)
    /// </summary>
    public int RankChange { get; set; }
}

/// <summary>
/// 검색 제안 결과 DTO
/// </summary>
public class SearchSuggestion
{
    /// <summary>
    /// 제안 텍스트
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// 제안 유형 (query, tag, author, post)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 관련 횟수 (검색 횟수 또는 사용 횟수)
    /// </summary>
    public int Count { get; set; }
}
