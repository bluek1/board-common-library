using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// Q&A 질문 상태
/// </summary>
public enum QuestionStatus
{
    /// <summary>
    /// 미해결 (기본값)
    /// </summary>
    Open = 0,
    
    /// <summary>
    /// 답변됨 (답변 채택 시)
    /// </summary>
    Answered = 1,
    
    /// <summary>
    /// 종료됨
    /// </summary>
    Closed = 2
}

/// <summary>
/// Q&A 질문 필수 항목 인터페이스
/// </summary>
public interface IQuestion : IEntity
{
    string Title { get; set; }
    string Content { get; set; }
    long AuthorId { get; set; }
    QuestionStatus Status { get; set; }
}

/// <summary>
/// Q&A 질문 엔티티
/// </summary>
public class Question : EntityBase, IQuestion, ISoftDeletable, IHasExtendedProperties
{
    /// <summary>
    /// 질문 제목 (필수, 최대 200자)
    /// </summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 내용 (필수)
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 작성자 ID (필수)
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 상태 (Open/Answered/Closed)
    /// </summary>
    public QuestionStatus Status { get; set; } = QuestionStatus.Open;
    
    /// <summary>
    /// 채택된 답변 ID
    /// </summary>
    public long? AcceptedAnswerId { get; set; }
    
    /// <summary>
    /// 조회수
    /// </summary>
    public int ViewCount { get; set; }
    
    /// <summary>
    /// 추천수 (추천 - 비추천)
    /// </summary>
    public int VoteCount { get; set; }
    
    /// <summary>
    /// 답변 수
    /// </summary>
    public int AnswerCount { get; set; }
    
    /// <summary>
    /// 현상금 포인트
    /// </summary>
    public int BountyPoints { get; set; }
    
    /// <summary>
    /// 태그 목록 (JSON 저장)
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    // ISoftDeletable 구현
    /// <summary>
    /// 삭제 여부
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 삭제 일시
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    // IHasExtendedProperties 구현
    /// <summary>
    /// 동적 확장 필드 (JSON 형식으로 저장)
    /// </summary>
    public Dictionary<string, object>? ExtendedProperties { get; set; }
    
    // Navigation Properties
    /// <summary>
    /// 질문에 대한 답변 목록
    /// </summary>
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    
    /// <summary>
    /// 채택된 답변
    /// </summary>
    public virtual Answer? AcceptedAnswer { get; set; }
    
    /// <summary>
    /// 질문 추천/비추천 목록
    /// </summary>
    public virtual ICollection<QuestionVote> Votes { get; set; } = new List<QuestionVote>();
}
