using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// Q&A 답변 필수 항목 인터페이스
/// </summary>
public interface IAnswer : IEntity
{
    string Content { get; set; }
    long QuestionId { get; set; }
    long AuthorId { get; set; }
    bool IsAccepted { get; set; }
}

/// <summary>
/// Q&A 답변 엔티티
/// </summary>
public class Answer : EntityBase, IAnswer, ISoftDeletable, IHasExtendedProperties
{
    /// <summary>
    /// 답변 내용 (필수)
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 ID (FK)
    /// </summary>
    public long QuestionId { get; set; }
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 채택 여부
    /// </summary>
    public bool IsAccepted { get; set; }
    
    /// <summary>
    /// 추천수 (추천 - 비추천)
    /// </summary>
    public int VoteCount { get; set; }
    
    /// <summary>
    /// 추천 수
    /// </summary>
    public int UpvoteCount { get; set; }
    
    /// <summary>
    /// 비추천 수
    /// </summary>
    public int DownvoteCount { get; set; }
    
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
    /// 해당 질문
    /// </summary>
    public virtual Question Question { get; set; } = null!;
    
    /// <summary>
    /// 답변 추천/비추천 목록
    /// </summary>
    public virtual ICollection<AnswerVote> Votes { get; set; } = new List<AnswerVote>();
}
