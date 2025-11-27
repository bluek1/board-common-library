using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 게시물 상태
/// </summary>
public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
    Deleted = 3
}

/// <summary>
/// 게시물 필수 항목 인터페이스
/// </summary>
public interface IPost : IEntity
{
    string Title { get; set; }
    string Content { get; set; }
    long AuthorId { get; set; }
    PostStatus Status { get; set; }
}

/// <summary>
/// 게시물 엔티티
/// </summary>
public class Post : EntityBase, IPost, ISoftDeletable, IHasExtendedProperties
{
    /// <summary>
    /// 게시물 제목 (필수, 최대 200자)
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 게시물 본문 (필수)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 카테고리
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록 (JSON으로 저장)
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 작성자 ID (필수)
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// 게시물 상태
    /// </summary>
    public PostStatus Status { get; set; } = PostStatus.Draft;
    
    /// <summary>
    /// URL 슬러그
    /// </summary>
    public string? Slug { get; set; }
    
    /// <summary>
    /// 조회수
    /// </summary>
    public int ViewCount { get; set; }
    
    /// <summary>
    /// 좋아요 수
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 댓글 수
    /// </summary>
    public int CommentCount { get; set; }
    
    /// <summary>
    /// 상단고정 여부
    /// </summary>
    public bool IsPinned { get; set; }
    
    /// <summary>
    /// 임시저장 여부
    /// </summary>
    public bool IsDraft { get; set; }
    
    /// <summary>
    /// 발행일시
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// 삭제 여부 (소프트 삭제)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 삭제일시
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// 동적 확장 필드
    /// </summary>
    public Dictionary<string, object>? ExtendedProperties { get; set; }
}
