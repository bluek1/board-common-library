using BoardCommonLibrary.Entities;

namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 질문 응답
/// </summary>
public class QuestionResponse
{
    /// <summary>
    /// 질문 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 질문 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 내용
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 상태
    /// </summary>
    public QuestionStatus Status { get; set; }
    
    /// <summary>
    /// 질문 상태 텍스트 ("미해결", "답변됨", "종료됨")
    /// </summary>
    public string StatusText { get; set; } = string.Empty;
    
    /// <summary>
    /// 조회수
    /// </summary>
    public int ViewCount { get; set; }
    
    /// <summary>
    /// 추천수
    /// </summary>
    public int VoteCount { get; set; }
    
    /// <summary>
    /// 답변 수
    /// </summary>
    public int AnswerCount { get; set; }
    
    /// <summary>
    /// 현상금 포인트
    /// </summary>
    public int BountyPoints { get; set; }
    
    /// <summary>
    /// 태그 목록
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 채택된 답변 ID
    /// </summary>
    public long? AcceptedAnswerId { get; set; }
    
    /// <summary>
    /// 작성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 현재 사용자의 투표 상태 (로그인 시)
    /// </summary>
    public VoteType? CurrentUserVote { get; set; }
    
    /// <summary>
    /// QuestionStatus를 한글 텍스트로 변환
    /// </summary>
    public static string GetStatusText(QuestionStatus status)
    {
        return status switch
        {
            QuestionStatus.Open => "미해결",
            QuestionStatus.Answered => "답변됨",
            QuestionStatus.Closed => "종료됨",
            _ => "알 수 없음"
        };
    }
}

/// <summary>
/// 질문 상세 응답 (답변 포함)
/// </summary>
public class QuestionDetailResponse : QuestionResponse
{
    /// <summary>
    /// 답변 목록
    /// </summary>
    public List<AnswerResponse> Answers { get; set; } = new();
    
    /// <summary>
    /// 채택된 답변
    /// </summary>
    public AnswerResponse? AcceptedAnswer { get; set; }
}

/// <summary>
/// 답변 응답
/// </summary>
public class AnswerResponse
{
    /// <summary>
    /// 답변 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 답변 내용
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 질문 ID
    /// </summary>
    public long QuestionId { get; set; }
    
    /// <summary>
    /// 작성자 ID
    /// </summary>
    public long AuthorId { get; set; }
    
    /// <summary>
    /// 작성자명
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
    
    /// <summary>
    /// 채택 여부
    /// </summary>
    public bool IsAccepted { get; set; }
    
    /// <summary>
    /// 추천수 (추천 - 비추천)
    /// </summary>
    public int VoteCount { get; set; }
    
    /// <summary>
    /// 추천 수
    /// </summary>
    public int UpvoteCount { get; set; }
    
    /// <summary>
    /// 비추천 수
    /// </summary>
    public int DownvoteCount { get; set; }
    
    /// <summary>
    /// 작성일시
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 수정일시
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 현재 사용자의 투표 상태 (로그인 시)
    /// </summary>
    public VoteType? CurrentUserVote { get; set; }
}
