using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 게시물 상세 응답 DTO
/// </summary>
public class PostResponse
{
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 본문
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 카테고리
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// 게시물 상태
    /// </summary>
    public PostStatus Status { get; set; }
    
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
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 발행일시
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// 삭제 여부 (관리자용)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// 블라인드 처리 여부 (관리자용)
    /// </summary>
    public bool IsBlinded { get; set; }
}

/// <summary>
/// 게시물 목록용 요약 응답 DTO
/// </summary>
public class PostSummaryResponse
{
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 본문 미리보기 (최대 200자)
    /// </summary>
    public string ContentPreview { get; set; } = string.Empty;
    
    /// <summary>
    /// 카테고리
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 태그 목록
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string? AuthorName { get; set; }
    
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
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 임시저장 목록용 응답 DTO
/// </summary>
public class DraftPostResponse
{
    /// <summary>
    /// 임시저장 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 제목
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 본문 미리보기
    /// </summary>
    public string? ContentPreview { get; set; }
    
    /// <summary>
    /// 생성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
