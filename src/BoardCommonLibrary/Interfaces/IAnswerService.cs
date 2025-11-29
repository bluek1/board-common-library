using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// Q&A 답변 서비스 인터페이스
/// </summary>
public interface IAnswerService
{
    #region CRUD
    
    /// <summary>
    /// 새 답변 생성
    /// </summary>
    /// <param name="questionId">질문 ID</param>
    /// <param name="request">답변 생성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    /// <returns>생성된 답변 응답</returns>
    Task<AnswerResponse> CreateAsync(long questionId, CreateAnswerRequest request, long authorId, string authorName);
    
    /// <summary>
    /// 답변 상세 조회
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="currentUserId">현재 사용자 ID (투표 상태 확인용)</param>
    /// <returns>답변 응답</returns>
    Task<AnswerResponse?> GetByIdAsync(long id, long? currentUserId = null);
    
    /// <summary>
    /// 특정 질문의 답변 목록 조회
    /// </summary>
    /// <param name="questionId">질문 ID</param>
    /// <param name="currentUserId">현재 사용자 ID (투표 상태 확인용)</param>
    /// <returns>답변 목록</returns>
    Task<List<AnswerResponse>> GetByQuestionIdAsync(long questionId, long? currentUserId = null);
    
    /// <summary>
    /// 답변 수정
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="request">수정 요청</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>수정된 답변 응답</returns>
    Task<AnswerResponse> UpdateAsync(long id, UpdateAnswerRequest request, long userId);
    
    /// <summary>
    /// 답변 삭제
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteAsync(long id, long userId);
    
    #endregion
    
    #region 채택
    
    /// <summary>
    /// 답변 채택
    /// </summary>
    /// <param name="answerId">답변 ID</param>
    /// <param name="questionAuthorId">질문 작성자 ID</param>
    /// <returns>채택된 답변 응답</returns>
    Task<AnswerResponse> AcceptAsync(long answerId, long questionAuthorId);
    
    /// <summary>
    /// 답변 채택 취소
    /// </summary>
    /// <param name="answerId">답변 ID</param>
    /// <param name="questionAuthorId">질문 작성자 ID</param>
    /// <returns>취소 성공 여부</returns>
    Task<bool> UnacceptAsync(long answerId, long questionAuthorId);
    
    #endregion
    
    #region 추천
    
    /// <summary>
    /// 답변 추천/비추천
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="voteType">투표 유형</param>
    /// <returns>갱신된 답변 응답</returns>
    Task<AnswerResponse> VoteAsync(long id, long userId, VoteType voteType);
    
    /// <summary>
    /// 추천/비추천 취소
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>취소 성공 여부</returns>
    Task<bool> RemoveVoteAsync(long id, long userId);
    
    #endregion
    
    #region 유틸리티
    
    /// <summary>
    /// 답변 작성자 확인
    /// </summary>
    /// <param name="answerId">답변 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>작성자 여부</returns>
    Task<bool> IsAuthorAsync(long answerId, long userId);
    
    /// <summary>
    /// 답변 존재 여부 확인
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <returns>존재 여부</returns>
    Task<bool> ExistsAsync(long id);
    
    #endregion
}
