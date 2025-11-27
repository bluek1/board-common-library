using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.Services.Interfaces;

/// <summary>
/// 게시물 서비스 인터페이스
/// </summary>
public interface IPostService
{
    /// <summary>
    /// 게시물 목록 조회
    /// </summary>
    Task<PagedResponse<PostSummaryResponse>> GetAllAsync(PostQueryParameters parameters);
    
    /// <summary>
    /// 게시물 상세 조회
    /// </summary>
    Task<PostResponse?> GetByIdAsync(long id);
    
    /// <summary>
    /// 게시물 생성
    /// </summary>
    Task<PostResponse> CreateAsync(CreatePostRequest request, long authorId, string? authorName = null);
    
    /// <summary>
    /// 게시물 수정
    /// </summary>
    Task<PostResponse?> UpdateAsync(long id, UpdatePostRequest request, long userId, bool isAdmin = false);
    
    /// <summary>
    /// 게시물 삭제 (소프트 삭제)
    /// </summary>
    Task<bool> DeleteAsync(long id, long userId, bool isAdmin = false);
    
    /// <summary>
    /// 게시물 상단고정 설정
    /// </summary>
    Task<PostResponse?> PinAsync(long id);
    
    /// <summary>
    /// 게시물 상단고정 해제
    /// </summary>
    Task<PostResponse?> UnpinAsync(long id);
    
    /// <summary>
    /// 임시저장
    /// </summary>
    Task<DraftPostResponse> SaveDraftAsync(DraftPostRequest request, long authorId, string? authorName = null);
    
    /// <summary>
    /// 임시저장 목록 조회
    /// </summary>
    Task<PagedResponse<DraftPostResponse>> GetDraftsAsync(long authorId, PagedRequest parameters);
    
    /// <summary>
    /// 게시물 발행 (임시저장 → 발행)
    /// </summary>
    Task<PostResponse?> PublishAsync(long draftId, long userId);
    
    /// <summary>
    /// 게시물 존재 여부 확인
    /// </summary>
    Task<bool> ExistsAsync(long id);
    
    /// <summary>
    /// 게시물 작성자 확인
    /// </summary>
    Task<bool> IsAuthorAsync(long postId, long userId);
}
