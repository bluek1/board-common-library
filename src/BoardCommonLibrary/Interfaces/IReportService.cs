using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 신고 서비스 인터페이스
/// </summary>
public interface IReportService
{
    #region 신고 생성 (일반 사용자)
    
    /// <summary>
    /// 신고 생성
    /// </summary>
    /// <param name="request">신고 생성 요청</param>
    /// <param name="reporterId">신고자 ID</param>
    /// <param name="reporterName">신고자명</param>
    /// <returns>생성된 신고 응답</returns>
    Task<ReportResponse> CreateAsync(CreateReportRequest request, long reporterId, string reporterName);
    
    #endregion
    
    #region 신고 조회 (관리자)
    
    /// <summary>
    /// 신고 상세 조회
    /// </summary>
    /// <param name="id">신고 ID</param>
    /// <returns>신고 응답</returns>
    Task<ReportResponse?> GetByIdAsync(long id);
    
    /// <summary>
    /// 신고 목록 조회
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>페이징된 신고 목록</returns>
    Task<PagedResponse<ReportResponse>> GetAllAsync(ReportQueryParameters parameters);
    
    #endregion
    
    #region 신고 처리 (관리자)
    
    /// <summary>
    /// 신고 처리
    /// </summary>
    /// <param name="id">신고 ID</param>
    /// <param name="request">처리 요청</param>
    /// <param name="processedById">처리자 ID</param>
    /// <param name="processedByName">처리자명</param>
    /// <returns>처리된 신고 응답</returns>
    Task<ReportResponse> ProcessAsync(long id, ProcessReportRequest request, long processedById, string processedByName);
    
    #endregion
    
    #region 신고 통계
    
    /// <summary>
    /// 대상별 신고 횟수 조회
    /// </summary>
    /// <param name="targetType">대상 유형</param>
    /// <param name="targetId">대상 ID</param>
    /// <returns>신고 횟수</returns>
    Task<int> GetReportCountAsync(ReportTargetType targetType, long targetId);
    
    /// <summary>
    /// 대기 중인 신고 수 조회
    /// </summary>
    /// <returns>대기 중인 신고 수</returns>
    Task<int> GetPendingCountAsync();
    
    #endregion
    
    #region 유틸리티
    
    /// <summary>
    /// 이미 신고했는지 확인
    /// </summary>
    /// <param name="targetType">대상 유형</param>
    /// <param name="targetId">대상 ID</param>
    /// <param name="reporterId">신고자 ID</param>
    /// <returns>신고 여부</returns>
    Task<bool> HasReportedAsync(ReportTargetType targetType, long targetId, long reporterId);
    
    /// <summary>
    /// 신고 대상 존재 여부 확인
    /// </summary>
    /// <param name="targetType">대상 유형</param>
    /// <param name="targetId">대상 ID</param>
    /// <returns>존재 여부</returns>
    Task<bool> TargetExistsAsync(ReportTargetType targetType, long targetId);
    
    #endregion
}
