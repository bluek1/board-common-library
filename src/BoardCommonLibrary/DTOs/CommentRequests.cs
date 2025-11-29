using System.ComponentModel.DataAnnotations;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 댓글 생성 요청 DTO
/// </summary>
public class CreateCommentRequest
{
    /// <summary>
    /// 댓글 내용 (필수, 최대 2000자)
    /// </summary>
    [Required(ErrorMessage = "댓글 내용은 필수입니다.")]
    [MaxLength(2000, ErrorMessage = "댓글은 2000자 이내여야 합니다.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 댓글 수정 요청 DTO
/// </summary>
public class UpdateCommentRequest
{
    /// <summary>
    /// 댓글 내용 (필수, 최대 2000자)
    /// </summary>
    [Required(ErrorMessage = "댓글 내용은 필수입니다.")]
    [MaxLength(2000, ErrorMessage = "댓글은 2000자 이내여야 합니다.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 댓글 쿼리 파라미터
/// </summary>
public class CommentQueryParameters
{
    /// <summary>
    /// 페이지 번호 (기본값 1)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지 크기 (기본값 20, 최대 100)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 정렬 기준 (createdAt, likeCount)
    /// </summary>
    public string SortBy { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string SortOrder { get; set; } = "asc";
    
    /// <summary>
    /// 대댓글 포함 여부 (기본값 true)
    /// </summary>
    public bool IncludeReplies { get; set; } = true;
}
