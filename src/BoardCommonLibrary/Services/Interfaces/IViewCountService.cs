namespace BoardCommonLibrary.Services.Interfaces;

/// <summary>
/// 조회수 서비스 인터페이스
/// </summary>
public interface IViewCountService
{
    /// <summary>
    /// 조회수 증가 (중복 체크 포함)
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID (비회원은 null)</param>
    /// <param name="ipAddress">IP 주소</param>
    /// <returns>조회수 증가 여부</returns>
    Task<bool> IncrementViewCountAsync(long postId, long? userId, string? ipAddress);
    
    /// <summary>
    /// 조회수 조회
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <returns>조회수</returns>
    Task<int> GetViewCountAsync(long postId);
    
    /// <summary>
    /// 중복 조회 여부 확인
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="ipAddress">IP 주소</param>
    /// <returns>중복 여부</returns>
    Task<bool> HasViewedAsync(long postId, long? userId, string? ipAddress);
}
