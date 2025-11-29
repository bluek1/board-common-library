using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// Q&A 질문 서비스 인터페이스
/// </summary>
public interface IQuestionService
{
    #region CRUD
    
    /// <summary>
    /// 새 질문 생성
    /// </summary>
    /// <param name="request">질문 생성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    /// <returns>생성된 질문 응답</returns>
    Task<QuestionResponse> CreateAsync(CreateQuestionRequest request, long authorId, string authorName);
    
    /// <summary>
    /// 질문 상세 조회 (답변 포함)
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="currentUserId">현재 사용자 ID (투표 상태 확인용)</param>
    /// <returns>질문 상세 응답 (답변 포함)</returns>
    Task<QuestionDetailResponse?> GetByIdAsync(long id, long? currentUserId = null);
    
    /// <summary>
    /// 질문 목록 조회
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>페이징된 질문 목록</returns>
    Task<PagedResponse<QuestionResponse>> GetAllAsync(QuestionQueryParameters parameters);
    
    /// <summary>
    /// 질문 수정
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="request">수정 요청</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>수정된 질문 응답</returns>
    Task<QuestionResponse> UpdateAsync(long id, UpdateQuestionRequest request, long userId);
    
    /// <summary>
    /// 질문 삭제 (답변이 없는 경우만 가능)
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteAsync(long id, long userId);
    
    #endregion
    
    #region 조회수
    
    /// <summary>
    /// 조회수 증가
    /// </summary>
    /// <param name="id">질문 ID</param>
    Task IncrementViewCountAsync(long id);
    
    #endregion
    
    #region 상태 관리
    
    /// <summary>
    /// 질문 종료
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>수정된 질문 응답</returns>
    Task<QuestionResponse> CloseAsync(long id, long userId);
    
    /// <summary>
    /// 질문 다시 열기
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>수정된 질문 응답</returns>
    Task<QuestionResponse> ReopenAsync(long id, long userId);
    
    #endregion
    
    #region 추천
    
    /// <summary>
    /// 질문 추천/비추천
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="voteType">투표 유형</param>
    /// <returns>갱신된 추천수</returns>
    Task<int> VoteAsync(long id, long userId, VoteType voteType);
    
    /// <summary>
    /// 추천/비추천 취소
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>취소 성공 여부</returns>
    Task<bool> RemoveVoteAsync(long id, long userId);
    
    #endregion
    
    #region 유틸리티
    
    /// <summary>
    /// 질문 작성자 확인
    /// </summary>
    /// <param name="questionId">질문 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>작성자 여부</returns>
    Task<bool> IsAuthorAsync(long questionId, long userId);
    
    /// <summary>
    /// 질문 존재 여부 확인
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <returns>존재 여부</returns>
    Task<bool> ExistsAsync(long id);
    
    #endregion
}
