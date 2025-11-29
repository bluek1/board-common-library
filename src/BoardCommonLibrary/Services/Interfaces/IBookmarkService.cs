using BoardCommonLibrary.DTOs;

namespace BoardCommonLibrary.Services.Interfaces;

/// <summary>
/// 북마크 서비스 인터페이스
/// </summary>
public interface IBookmarkService
{
    /// <summary>
    /// 북마크 추가
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>성공 여부</returns>
    Task<bool> AddBookmarkAsync(long postId, long userId);
    
    /// <summary>
    /// 북마크 해제
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>성공 여부</returns>
    Task<bool> RemoveBookmarkAsync(long postId, long userId);
    
    /// <summary>
    /// 사용자 북마크 목록 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="parameters">쿼리 파라미터</param>
    /// <returns>북마크 목록</returns>
    Task<PagedResponse<BookmarkResponse>> GetUserBookmarksAsync(long userId, BookmarkQueryParameters parameters);
    
    /// <summary>
    /// 사용자가 북마크했는지 확인
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>북마크 여부</returns>
    Task<bool> HasUserBookmarkedAsync(long postId, long userId);
}
