using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 북마크 엔티티
/// </summary>
public class Bookmark : EntityBase
{
    /// <summary>
    /// 사용자 ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 게시물 ID (FK)
    /// </summary>
    public long PostId { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// 게시물 (Navigation Property)
    /// </summary>
    public virtual Post? Post { get; set; }
}
