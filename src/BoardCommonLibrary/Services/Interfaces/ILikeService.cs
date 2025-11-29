using BoardCommonLibrary.DTOs;

namespace BoardCommonLibrary.Services.Interfaces;

/// <summary>
/// 좋아요 서비스 인터페이스
/// </summary>
public interface ILikeService
{
    /// <summary>
    /// 게시물 좋아요 추가
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 결과</returns>
    Task<LikeResponse> LikePostAsync(long postId, long userId);
    
    /// <summary>
    /// 게시물 좋아요 취소
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 결과</returns>
    Task<LikeResponse?> UnlikePostAsync(long postId, long userId);
    
    /// <summary>
    /// 댓글 좋아요 추가
    /// </summary>
    /// <param name="commentId">댓글 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 결과</returns>
    Task<LikeResponse> LikeCommentAsync(long commentId, long userId);
    
    /// <summary>
    /// 댓글 좋아요 취소
    /// </summary>
    /// <param name="commentId">댓글 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 결과</returns>
    Task<LikeResponse?> UnlikeCommentAsync(long commentId, long userId);
    
    /// <summary>
    /// 사용자가 게시물에 좋아요했는지 확인
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 여부</returns>
    Task<bool> HasUserLikedPostAsync(long postId, long userId);
    
    /// <summary>
    /// 사용자가 댓글에 좋아요했는지 확인
    /// </summary>
    /// <param name="commentId">댓글 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>좋아요 여부</returns>
    Task<bool> HasUserLikedCommentAsync(long commentId, long userId);
}
