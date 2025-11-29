using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 신고 대상 유형
/// </summary>
public enum ReportTargetType
{
    /// <summary>
    /// 게시물
    /// </summary>
    Post = 0,
    
    /// <summary>
    /// 댓글
    /// </summary>
    Comment = 1,
    
    /// <summary>
    /// Q&A 질문
    /// </summary>
    Question = 2,
    
    /// <summary>
    /// Q&A 답변
    /// </summary>
    Answer = 3
}

/// <summary>
/// 신고 사유
/// </summary>
public enum ReportReason
{
    /// <summary>
    /// 스팸/광고
    /// </summary>
    Spam = 0,
    
    /// <summary>
    /// 부적절한 내용
    /// </summary>
    Inappropriate = 1,
    
    /// <summary>
    /// 욕설/비방
    /// </summary>
    Harassment = 2,
    
    /// <summary>
    /// 저작권 침해
    /// </summary>
    Copyright = 3,
    
    /// <summary>
    /// 개인정보 노출
    /// </summary>
    PersonalInfo = 4,
    
    /// <summary>
    /// 기타
    /// </summary>
    Other = 99
}

/// <summary>
/// 신고 처리 상태
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// 대기 중
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// 승인 (콘텐츠 블라인드 처리)
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// 거부 (신고 기각)
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// 해결됨
    /// </summary>
    Resolved = 3
}

/// <summary>
/// 신고 엔티티
/// </summary>
public class Report : EntityBase
{
    /// <summary>
    /// 신고 대상 유형 (Post, Comment, Question, Answer)
    /// </summary>
    public ReportTargetType TargetType { get; set; }
    
    /// <summary>
    /// 신고 대상 ID
    /// </summary>
    public long TargetId { get; set; }
    
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
    /// 상세 설명 (선택적)
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 처리 상태
    /// </summary>
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    
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
    [MaxLength(500)]
    public string? ProcessingNote { get; set; }
}
