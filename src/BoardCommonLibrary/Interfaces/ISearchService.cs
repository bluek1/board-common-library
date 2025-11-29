using BoardCommonLibrary.DTOs;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 검색 서비스 인터페이스
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 게시물 검색
    /// </summary>
    /// <param name="parameters">검색 파라미터</param>
    /// <returns>검색 결과 (페이징)</returns>
    Task<PagedSearchResult<PostSearchResult>> SearchPostsAsync(PostSearchParameters parameters);
    
    /// <summary>
    /// 태그 검색
    /// </summary>
    /// <param name="parameters">검색 파라미터</param>
    /// <returns>태그 검색 결과</returns>
    Task<List<TagSearchResult>> SearchTagsAsync(TagSearchParameters parameters);
    
    /// <summary>
    /// 작성자 검색
    /// </summary>
    /// <param name="parameters">검색 파라미터</param>
    /// <returns>작성자 검색 결과</returns>
    Task<List<AuthorSearchResult>> SearchAuthorsAsync(AuthorSearchParameters parameters);
    
    /// <summary>
    /// 통합 검색
    /// </summary>
    /// <param name="parameters">검색 파라미터</param>
    /// <returns>통합 검색 결과</returns>
    Task<UnifiedSearchResult> SearchAllAsync(UnifiedSearchParameters parameters);
    
    /// <summary>
    /// 인기 검색어 조회
    /// </summary>
    /// <param name="limit">조회 개수</param>
    /// <returns>인기 검색어 목록</returns>
    Task<List<PopularSearchResult>> GetPopularSearchesAsync(int limit = 10);
    
    /// <summary>
    /// 검색 제안 (자동완성)
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="limit">제안 개수</param>
    /// <returns>검색 제안 목록</returns>
    Task<List<SearchSuggestion>> GetSuggestionsAsync(string query, int limit = 5);
    
    /// <summary>
    /// 검색어 저장 (검색 통계용)
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="userId">검색 사용자 ID (선택)</param>
    /// <returns>완료</returns>
    Task RecordSearchAsync(string query, long? userId = null);
    
    /// <summary>
    /// 검색어 하이라이트 처리
    /// </summary>
    /// <param name="text">원본 텍스트</param>
    /// <param name="query">검색어</param>
    /// <param name="highlightTag">하이라이트 태그 (기본: mark)</param>
    /// <returns>하이라이트 처리된 텍스트</returns>
    string HighlightSearchTerms(string text, string query, string highlightTag = "mark");
    
    /// <summary>
    /// 본문 미리보기 생성
    /// </summary>
    /// <param name="content">전체 본문</param>
    /// <param name="query">검색어</param>
    /// <param name="previewLength">미리보기 길이</param>
    /// <returns>검색어 주변의 미리보기 텍스트</returns>
    string GenerateContentPreview(string content, string query, int previewLength = 150);
    
    /// <summary>
    /// 검색 인덱스 재구축 (관리자용)
    /// </summary>
    /// <returns>완료</returns>
    Task RebuildSearchIndexAsync();
}
