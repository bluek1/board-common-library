using BoardCommonLibrary.DTOs;

namespace BoardCommonLibrary.Services.Interfaces;

/// <summary>
/// 댓글 서비스 인터페이스
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// 댓글 생성
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="request">생성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    /// <returns>생성된 댓글</returns>
    Task<CommentResponse> CreateAsync(long postId, CreateCommentRequest request, long authorId, string? authorName = null);
    
    /// <summary>
    /// 게시물의 댓글 목록 조회
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="parameters">쿼리 파라미터</param>
    /// <returns>댓글 목록</returns>
    Task<PagedResponse<CommentResponse>> GetByPostIdAsync(long postId, CommentQueryParameters parameters);
    
    /// <summary>
    /// 댓글 상세 조회
    /// </summary>
    /// <param name="id">댓글 ID</param>
    /// <returns>댓글 정보</returns>
    Task<CommentResponse?> GetByIdAsync(long id);
    
    /// <summary>
    /// 댓글 수정
    /// </summary>
    /// <param name="id">댓글 ID</param>
    /// <param name="request">수정 요청</param>
    /// <param name="currentUserId">현재 사용자 ID</param>
    /// <returns>수정된 댓글</returns>
    Task<CommentResponse?> UpdateAsync(long id, UpdateCommentRequest request, long currentUserId);
    
    /// <summary>
    /// 댓글 삭제
    /// </summary>
    /// <param name="id">댓글 ID</param>
    /// <param name="currentUserId">현재 사용자 ID</param>
    /// <param name="isAdmin">관리자 여부</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteAsync(long id, long currentUserId, bool isAdmin = false);
    
    /// <summary>
    /// 대댓글 생성
    /// </summary>
    /// <param name="parentId">부모 댓글 ID</param>
    /// <param name="request">생성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    /// <returns>생성된 대댓글</returns>
    Task<CommentResponse?> CreateReplyAsync(long parentId, CreateCommentRequest request, long authorId, string? authorName = null);
    
    /// <summary>
    /// 작성자 확인
    /// </summary>
    /// <param name="commentId">댓글 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>작성자 여부</returns>
    Task<bool> IsAuthorAsync(long commentId, long userId);
}
