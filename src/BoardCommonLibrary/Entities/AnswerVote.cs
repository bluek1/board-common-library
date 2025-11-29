using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 투표 유형 (추천/비추천)
/// </summary>
public enum VoteType
{
    /// <summary>
    /// 추천
    /// </summary>
    Up = 1,
    
    /// <summary>
    /// 비추천
    /// </summary>
    Down = -1
}

/// <summary>
/// Q&A 답변 추천/비추천 엔티티
/// </summary>
public class AnswerVote : EntityBase
{
    /// <summary>
    /// 답변 ID (FK)
    /// </summary>
    public long AnswerId { get; set; }
    
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
    /// 해당 답변
    /// </summary>
    public virtual Answer Answer { get; set; } = null!;
}
