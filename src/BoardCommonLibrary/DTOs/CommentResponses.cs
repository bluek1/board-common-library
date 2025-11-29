namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 댓글 응답 DTO
/// </summary>
public class CommentResponse
{
    /// <summary>
    /// 댓글 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 댓글 내용
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long PostId { get; set; }
    
    /// <summary>
    /// 작성자 ID
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
    /// 삭제 여부
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 대댓글 목록
    /// </summary>
    public List<CommentResponse>? Replies { get; set; }
}
