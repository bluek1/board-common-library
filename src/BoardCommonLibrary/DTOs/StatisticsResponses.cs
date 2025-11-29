namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 게시판 통계 응답
/// </summary>
public class BoardStatisticsResponse
{
    // 기본 통계
    /// <summary>
    /// 전체 게시물 수
    /// </summary>
    public long TotalPosts { get; set; }
    
    /// <summary>
    /// 전체 댓글 수
    /// </summary>
    public long TotalComments { get; set; }
    
    /// <summary>
    /// 전체 질문 수
    /// </summary>
    public long TotalQuestions { get; set; }
    
    /// <summary>
    /// 전체 답변 수
    /// </summary>
    public long TotalAnswers { get; set; }
    
    /// <summary>
    /// 전체 파일 수
    /// </summary>
    public long TotalFiles { get; set; }
    
    // 기간별 통계 (오늘)
    /// <summary>
    /// 오늘 게시물 수
    /// </summary>
    public long TodayPosts { get; set; }
    
    /// <summary>
    /// 오늘 댓글 수
    /// </summary>
    public long TodayComments { get; set; }
    
    /// <summary>
    /// 오늘 질문 수
    /// </summary>
    public long TodayQuestions { get; set; }
    
    /// <summary>
    /// 오늘 답변 수
    /// </summary>
    public long TodayAnswers { get; set; }
    
    // 기간별 통계 (이번 주)
    /// <summary>
    /// 이번 주 게시물 수
    /// </summary>
    public long WeeklyPosts { get; set; }
    
    /// <summary>
    /// 이번 주 댓글 수
    /// </summary>
    public long WeeklyComments { get; set; }
    
    /// <summary>
    /// 이번 주 질문 수
    /// </summary>
    public long WeeklyQuestions { get; set; }
    
    /// <summary>
    /// 이번 주 답변 수
    /// </summary>
    public long WeeklyAnswers { get; set; }
    
    // 기간별 통계 (이번 달)
    /// <summary>
    /// 이번 달 게시물 수
    /// </summary>
    public long MonthlyPosts { get; set; }
    
    /// <summary>
    /// 이번 달 댓글 수
    /// </summary>
    public long MonthlyComments { get; set; }
    
    /// <summary>
    /// 이번 달 질문 수
    /// </summary>
    public long MonthlyQuestions { get; set; }
    
    /// <summary>
    /// 이번 달 답변 수
    /// </summary>
    public long MonthlyAnswers { get; set; }
    
    // 활동 통계
    /// <summary>
    /// 총 조회수
    /// </summary>
    public long TotalViews { get; set; }
    
    /// <summary>
    /// 총 좋아요 수
    /// </summary>
    public long TotalLikes { get; set; }
    
    /// <summary>
    /// 활성 사용자 수 (최근 7일)
    /// </summary>
    public long ActiveUsers { get; set; }
    
    // 신고 통계
    /// <summary>
    /// 대기 중인 신고 수
    /// </summary>
    public long PendingReports { get; set; }
    
    /// <summary>
    /// 전체 신고 수
    /// </summary>
    public long TotalReports { get; set; }
    
    // 인기 콘텐츠
    /// <summary>
    /// 인기 게시물 목록
    /// </summary>
    public List<PopularPostResponse> PopularPosts { get; set; } = new();
    
    /// <summary>
    /// 인기 질문 목록
    /// </summary>
    public List<PopularQuestionResponse> PopularQuestions { get; set; } = new();
    
    // 기간별 트렌드
    /// <summary>
    /// 일별 통계 (최근 7일)
    /// </summary>
    public List<DailyStatistics> DailyTrend { get; set; } = new();
}

/// <summary>
/// 인기 게시물 응답
/// </summary>
public class PopularPostResponse
{
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 게시물 제목
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
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
}

/// <summary>
/// 인기 질문 응답
/// </summary>
public class PopularQuestionResponse
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
}

/// <summary>
/// 일별 통계
/// </summary>
public class DailyStatistics
{
    /// <summary>
    /// 날짜
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// 게시물 수
    /// </summary>
    public int PostCount { get; set; }
    
    /// <summary>
    /// 댓글 수
    /// </summary>
    public int CommentCount { get; set; }
    
    /// <summary>
    /// 질문 수
    /// </summary>
    public int QuestionCount { get; set; }
    
    /// <summary>
    /// 답변 수
    /// </summary>
    public int AnswerCount { get; set; }
}
