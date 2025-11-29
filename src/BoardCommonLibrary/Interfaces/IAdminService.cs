using BoardCommonLibrary.DTOs;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 관리자 서비스 인터페이스
/// </summary>
public interface IAdminService
{
    #region 게시물 관리
    
    /// <summary>
    /// 전체 게시물 조회 (관리자용 - 삭제된 게시물 포함)
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>페이징된 게시물 목록</returns>
    Task<PagedResponse<PostResponse>> GetAllPostsAsync(AdminPostQueryParameters parameters);
    
    #endregion
    
    #region 댓글 관리
    
    /// <summary>
    /// 전체 댓글 조회 (관리자용 - 삭제된 댓글 포함)
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>페이징된 댓글 목록</returns>
    Task<PagedResponse<CommentResponse>> GetAllCommentsAsync(AdminCommentQueryParameters parameters);
    
    #endregion
    
    #region 일괄 처리
    
    /// <summary>
    /// 콘텐츠 일괄 삭제
    /// </summary>
    /// <param name="request">일괄 삭제 요청</param>
    /// <returns>삭제 결과</returns>
    Task<BatchDeleteResponse> BatchDeleteAsync(BatchDeleteRequest request);
    
    /// <summary>
    /// 콘텐츠 블라인드 처리
    /// </summary>
    /// <param name="request">블라인드 요청</param>
    /// <returns>처리 성공 여부</returns>
    Task<bool> BlindContentAsync(BlindContentRequest request);
    
    #endregion
    
    #region 통계
    
    /// <summary>
    /// 게시판 통계 조회
    /// </summary>
    /// <returns>통계 응답</returns>
    Task<BoardStatisticsResponse> GetStatisticsAsync();
    
    #endregion
}
