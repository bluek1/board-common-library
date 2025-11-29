using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 게시물 관리 조회 파라미터 (관리자용)
/// </summary>
public class AdminPostQueryParameters
{
    /// <summary>
    /// 페이지 번호 (기본값: 1)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지 크기 (기본값: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 게시물 상태 필터
    /// </summary>
    public PostStatus? Status { get; set; }
    
    /// <summary>
    /// 삭제 여부 필터 (null: 전체, true: 삭제됨, false: 미삭제)
    /// </summary>
    public bool? IsDeleted { get; set; }
    
    /// <summary>
    /// 블라인드 여부 필터
    /// </summary>
    public bool? IsBlinded { get; set; }
    
    /// <summary>
    /// 작성자 ID 필터
    /// </summary>
    public long? AuthorId { get; set; }
    
    /// <summary>
    /// 카테고리 필터
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 시작 날짜 필터
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// 종료 날짜 필터
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// 검색어
    /// </summary>
    public string? Query { get; set; }
    
    /// <summary>
    /// 정렬 기준 (createdAt, viewCount, likeCount)
    /// </summary>
    public string Sort { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
}

/// <summary>
/// 댓글 관리 조회 파라미터 (관리자용)
/// </summary>
public class AdminCommentQueryParameters
{
    /// <summary>
    /// 페이지 번호 (기본값: 1)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 페이지 크기 (기본값: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 삭제 여부 필터
    /// </summary>
    public bool? IsDeleted { get; set; }
    
    /// <summary>
    /// 블라인드 여부 필터
    /// </summary>
    public bool? IsBlinded { get; set; }
    
    /// <summary>
    /// 작성자 ID 필터
    /// </summary>
    public long? AuthorId { get; set; }
    
    /// <summary>
    /// 게시물 ID 필터
    /// </summary>
    public long? PostId { get; set; }
    
    /// <summary>
    /// 시작 날짜 필터
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// 종료 날짜 필터
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// 검색어
    /// </summary>
    public string? Query { get; set; }
    
    /// <summary>
    /// 정렬 기준 (createdAt, likeCount)
    /// </summary>
    public string Sort { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
}

/// <summary>
/// 일괄 처리 대상 유형
/// </summary>
public enum BatchTargetType
{
    /// <summary>
    /// 게시물
    /// </summary>
    Post = 0,
    
    /// <summary>
    /// 댓글
    /// </summary>
    Comment = 1,
    
    /// <summary>
    /// Q&A 질문
    /// </summary>
    Question = 2,
    
    /// <summary>
    /// Q&A 답변
    /// </summary>
    Answer = 3
}

/// <summary>
/// 일괄 삭제 요청
/// </summary>
public class BatchDeleteRequest
{
    /// <summary>
    /// 대상 유형
    /// </summary>
    [Required]
    public BatchTargetType TargetType { get; set; }
    
    /// <summary>
    /// 대상 ID 목록
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "최소 1개 이상의 항목을 선택해야 합니다.")]
    public List<long> Ids { get; set; } = new();
    
    /// <summary>
    /// 영구 삭제 여부 (기본: 소프트 삭제)
    /// </summary>
    public bool HardDelete { get; set; } = false;
}

/// <summary>
/// 일괄 삭제 응답
/// </summary>
public class BatchDeleteResponse
{
    /// <summary>
    /// 삭제 성공 수
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// 삭제 실패 수
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// 실패한 ID 목록
    /// </summary>
    public List<long> FailedIds { get; set; } = new();
}

/// <summary>
/// 콘텐츠 블라인드 요청
/// </summary>
public class BlindContentRequest
{
    /// <summary>
    /// 대상 유형
    /// </summary>
    [Required]
    public BatchTargetType TargetType { get; set; }
    
    /// <summary>
    /// 대상 ID
    /// </summary>
    [Required]
    public long TargetId { get; set; }
    
    /// <summary>
    /// 블라인드 여부 (true: 블라인드, false: 블라인드 해제)
    /// </summary>
    public bool IsBlinded { get; set; } = true;
    
    /// <summary>
    /// 블라인드 사유
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}
