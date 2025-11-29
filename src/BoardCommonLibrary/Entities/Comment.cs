using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 댓글 필수 항목 인터페이스
/// </summary>
public interface IComment : IEntity
{
    string Content { get; set; }
    long PostId { get; set; }
    long AuthorId { get; set; }
}

/// <summary>
/// 댓글 엔티티
/// </summary>
public class Comment : EntityBase, IComment, ISoftDeletable
{
    /// <summary>
    /// 댓글 내용 (필수, 최대 2000자)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 게시물 ID (FK)
    /// </summary>
    public long PostId { get; set; }
    
    /// <summary>
    /// 작성자 ID (필수)
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// 부모 댓글 ID (대댓글인 경우)
    /// </summary>
    public long? ParentId { get; set; }
    
    /// <summary>
    /// 좋아요 수
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 블라인드 여부
    /// </summary>
    public bool IsBlinded { get; set; }
    
    /// <summary>
    /// 삭제 여부 (소프트 삭제)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 삭제일시
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// 게시물 (Navigation Property)
    /// </summary>
    public virtual Post? Post { get; set; }
    
    /// <summary>
    /// 부모 댓글 (Navigation Property)
    /// </summary>
    public virtual Comment? Parent { get; set; }
    
    /// <summary>
    /// 자식 댓글 (대댓글) 목록
    /// </summary>
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
