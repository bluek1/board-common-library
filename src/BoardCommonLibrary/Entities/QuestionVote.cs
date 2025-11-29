using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// Q&A 질문 추천/비추천 엔티티
/// </summary>
public class QuestionVote : EntityBase
{
    /// <summary>
    /// 질문 ID (FK)
    /// </summary>
    public long QuestionId { get; set; }
    
    /// <summary>
    /// 투표한 사용자 ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 투표 유형 (Up: 추천, Down: 비추천)
    /// </summary>
    public VoteType VoteType { get; set; }
    
    // Navigation Property
    /// <summary>
    /// 해당 질문
    /// </summary>
    public virtual Question Question { get; set; } = null!;
}
