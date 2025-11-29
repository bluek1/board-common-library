using System.ComponentModel.DataAnnotations;
using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 질문 생성 요청
/// </summary>
public class CreateQuestionRequest
{
    /// <summary>
    /// 질문 제목 (필수, 최대 200자)
    /// </summary>
    [Required(ErrorMessage = "제목은 필수입니다.")]
    [MaxLength(200, ErrorMessage = "제목은 최대 200자입니다.")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 내용 (필수)
    /// </summary>
    [Required(ErrorMessage = "내용은 필수입니다.")]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 태그 목록 (선택적)
    /// </summary>
    public List<string>? Tags { get; set; }
    
    /// <summary>
    /// 현상금 포인트 (선택적)
    /// </summary>
    public int BountyPoints { get; set; }
}

/// <summary>
/// 질문 수정 요청
/// </summary>
public class UpdateQuestionRequest
{
    /// <summary>
    /// 질문 제목 (필수, 최대 200자)
    /// </summary>
    [Required(ErrorMessage = "제목은 필수입니다.")]
    [MaxLength(200, ErrorMessage = "제목은 최대 200자입니다.")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 내용 (필수)
    /// </summary>
    [Required(ErrorMessage = "내용은 필수입니다.")]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 태그 목록 (선택적)
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// 질문 목록 조회 파라미터
/// </summary>
public class QuestionQueryParameters
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
    /// 질문 상태 필터
    /// </summary>
    public QuestionStatus? Status { get; set; }
    
    /// <summary>
    /// 태그 필터
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// 작성자 ID 필터
    /// </summary>
    public long? AuthorId { get; set; }
    
    /// <summary>
    /// 정렬 기준 (createdAt, viewCount, voteCount, answerCount)
    /// </summary>
    public string Sort { get; set; } = "createdAt";
    
    /// <summary>
    /// 정렬 순서 (asc, desc)
    /// </summary>
    public string Order { get; set; } = "desc";
    
    /// <summary>
    /// 검색어
    /// </summary>
    public string? Query { get; set; }
}

/// <summary>
/// 답변 생성 요청
/// </summary>
public class CreateAnswerRequest
{
    /// <summary>
    /// 답변 내용 (필수)
    /// </summary>
    [Required(ErrorMessage = "내용은 필수입니다.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 답변 수정 요청
/// </summary>
public class UpdateAnswerRequest
{
    /// <summary>
    /// 답변 내용 (필수)
    /// </summary>
    [Required(ErrorMessage = "내용은 필수입니다.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 투표(추천/비추천) 요청
/// </summary>
public class VoteRequest
{
    /// <summary>
    /// 투표 유형 (Up: 추천, Down: 비추천)
    /// </summary>
    [Required]
    public VoteType VoteType { get; set; }
}
