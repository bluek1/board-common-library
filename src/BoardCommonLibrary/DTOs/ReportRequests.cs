using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 신고 생성 요청
/// </summary>
public class CreateReportRequest
{
    /// <summary>
    /// 신고 대상 유형 (Post, Comment, Question, Answer)
    /// </summary>
    [Required]
    public ReportTargetType TargetType { get; set; }
    
    /// <summary>
    /// 신고 대상 ID
    /// </summary>
    [Required]
    public long TargetId { get; set; }
    
    /// <summary>
    /// 신고 사유
    /// </summary>
    [Required]
    public ReportReason Reason { get; set; }
    
    /// <summary>
    /// 상세 설명 (선택적, 최대 500자)
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// 신고 처리 요청 (관리자용)
/// </summary>
public class ProcessReportRequest
{
    /// <summary>
    /// 처리 상태 (Approved: 승인/블라인드, Rejected: 거부)
    /// </summary>
    [Required]
    public ReportStatus Status { get; set; }
    
    /// <summary>
    /// 처리 메모 (선택적, 최대 500자)
    /// </summary>
    [MaxLength(500)]
    public string? ProcessingNote { get; set; }
}

/// <summary>
/// 신고 목록 조회 파라미터
/// </summary>
public class ReportQueryParameters
{
    /// <summary>
    /// 페이지 번호 (기본값: 1)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지 크기 (기본값: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 신고 상태 필터
    /// </summary>
    public ReportStatus? Status { get; set; }
    
    /// <summary>
    /// 신고 대상 유형 필터
    /// </summary>
    public ReportTargetType? TargetType { get; set; }
    
    /// <summary>
    /// 신고 사유 필터
    /// </summary>
    public ReportReason? Reason { get; set; }
    
    /// <summary>
    /// 시작 날짜 필터
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// 종료 날짜 필터
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// 정렬 기준 (createdAt, processedAt)
    /// </summary>
    public string Sort { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
}
