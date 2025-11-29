using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 좋아요 엔티티
/// </summary>
public class Like : EntityBase
{
    /// <summary>
    /// 사용자 ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 게시물 ID (게시물 좋아요 시)
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 댓글 ID (댓글 좋아요 시)
    /// </summary>
    public long? CommentId { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// 게시물 (Navigation Property)
    /// </summary>
    public virtual Post? Post { get; set; }
    
    /// <summary>
    /// 댓글 (Navigation Property)
    /// </summary>
    public virtual Comment? Comment { get; set; }
}
