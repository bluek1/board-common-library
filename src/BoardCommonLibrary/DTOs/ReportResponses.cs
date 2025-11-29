using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 신고 응답
/// </summary>
public class ReportResponse
{
    /// <summary>
    /// 신고 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 신고 대상 유형
    /// </summary>
    public ReportTargetType TargetType { get; set; }
    
    /// <summary>
    /// 신고 대상 유형 텍스트
    /// </summary>
    public string TargetTypeText { get; set; } = string.Empty;
    
    /// <summary>
    /// 신고 대상 ID
    /// </summary>
    public long TargetId { get; set; }
    
    /// <summary>
    /// 신고 대상 제목/내용 요약
    /// </summary>
    public string? TargetTitle { get; set; }
    
    /// <summary>
    /// 신고 대상 작성자명
    /// </summary>
    public string? TargetAuthorName { get; set; }
    
    /// <summary>
    /// 신고자 ID
    /// </summary>
    public long ReporterId { get; set; }
    
    /// <summary>
    /// 신고자명
    /// </summary>
    public string ReporterName { get; set; } = string.Empty;
    
    /// <summary>
    /// 신고 사유
    /// </summary>
    public ReportReason Reason { get; set; }
    
    /// <summary>
    /// 신고 사유 텍스트
    /// </summary>
    public string ReasonText { get; set; } = string.Empty;
    
    /// <summary>
    /// 상세 설명
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 신고 상태
    /// </summary>
    public ReportStatus Status { get; set; }
    
    /// <summary>
    /// 신고 상태 텍스트
    /// </summary>
    public string StatusText { get; set; } = string.Empty;
    
    /// <summary>
    /// 처리자 ID
    /// </summary>
    public long? ProcessedById { get; set; }
    
    /// <summary>
    /// 처리자명
    /// </summary>
    public string? ProcessedByName { get; set; }
    
    /// <summary>
    /// 처리 일시
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// 처리 메모
    /// </summary>
    public string? ProcessingNote { get; set; }
    
    /// <summary>
    /// 신고 일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// ReportTargetType을 한글 텍스트로 변환
    /// </summary>
    public static string GetTargetTypeText(ReportTargetType targetType)
    {
        return targetType switch
        {
            ReportTargetType.Post => "게시물",
            ReportTargetType.Comment => "댓글",
            ReportTargetType.Question => "질문",
            ReportTargetType.Answer => "답변",
            _ => "알 수 없음"
        };
    }
    
    /// <summary>
    /// ReportReason을 한글 텍스트로 변환
    /// </summary>
    public static string GetReasonText(ReportReason reason)
    {
        return reason switch
        {
            ReportReason.Spam => "스팸/광고",
            ReportReason.Inappropriate => "부적절한 내용",
            ReportReason.Harassment => "욕설/비방",
            ReportReason.Copyright => "저작권 침해",
            ReportReason.PersonalInfo => "개인정보 노출",
            ReportReason.Other => "기타",
            _ => "알 수 없음"
        };
    }
    
    /// <summary>
    /// ReportStatus를 한글 텍스트로 변환
    /// </summary>
    public static string GetStatusText(ReportStatus status)
    {
        return status switch
        {
            ReportStatus.Pending => "대기 중",
            ReportStatus.Approved => "승인됨",
            ReportStatus.Rejected => "거부됨",
            ReportStatus.Resolved => "해결됨",
            _ => "알 수 없음"
        };
    }
}
