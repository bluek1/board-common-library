# 페이지 4: 관리자 기능 및 Q&A 게시판 세부 작업 명세서

## 📋 개요

본 문서는 관리자 기능(콘텐츠 관리, 신고 처리, 통계)과 Q&A 게시판(질문/답변) 구현을 위한 세부 작업 내용을 정의합니다.

**우선순위**: P1 (중요)  
**총 기능 수**: 12개  
**총 테스트 수**: 15개 (최소)  
**진행 상태**: 🔴 대기

---

## 📊 기능 요약

| 영역 | 기능 ID | 기능명 | 우선순위 |
|-----|--------|-------|---------|
| **관리자 기능** | P4-001 | 전체 게시물 관리 | P0 |
| | P4-002 | 전체 댓글 관리 | P0 |
| | P4-003 | 신고 목록 조회 | P1 |
| | P4-004 | 신고 처리 | P1 |
| | P4-005 | 콘텐츠 블라인드 | P1 |
| | P4-006 | 일괄 삭제 | P1 |
| | P4-007 | 통계 조회 | P1 |
| **Q&A 게시판** | P4-008 | 질문 작성 | P0 |
| | P4-009 | 질문 조회 | P0 |
| | P4-010 | 답변 작성 | P0 |
| | P4-011 | 답변 채택 | P0 |
| | P4-012 | 답변 추천 | P1 |

---

## 🔧 작업 목록

### 1. Q&A 데이터 모델 설계 및 구현

#### 1.1 Question(질문) 엔티티 설계
- [ ] Question 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Question.cs`
  ```csharp
  public class Question : IEntity, ISoftDeletable, IHasExtendedProperties
  {
      // 필수 항목
      public long Id { get; set; }
      
      [Required, MaxLength(200)]
      public string Title { get; set; } = string.Empty;
      
      [Required]
      public string Content { get; set; } = string.Empty;
      
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      
      public QuestionStatus Status { get; set; } = QuestionStatus.Open;
      
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      
      // 선택적 항목
      public long? AcceptedAnswerId { get; set; }
      public int ViewCount { get; set; }
      public int VoteCount { get; set; }        // 추천수
      public int AnswerCount { get; set; }      // 답변 수
      public int BountyPoints { get; set; }     // 현상금 포인트
      
      public List<string> Tags { get; set; } = new();  // JSON 저장
      
      // 소프트 삭제
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }
      
      // 동적 확장 필드
      public Dictionary<string, object>? ExtendedProperties { get; set; }
      
      // Navigation Properties
      public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
      public virtual Answer? AcceptedAnswer { get; set; }
  }
  
  public enum QuestionStatus
  {
      Open = 0,       // 미해결
      Answered = 1,   // 답변됨
      Closed = 2      // 종료됨
  }
  ```

#### 1.2 Answer(답변) 엔티티 설계
- [ ] Answer 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Answer.cs`
  ```csharp
  public class Answer : IEntity, ISoftDeletable, IHasExtendedProperties
  {
      // 필수 항목
      public long Id { get; set; }
      
      [Required]
      public string Content { get; set; } = string.Empty;
      
      public long QuestionId { get; set; }
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      
      public bool IsAccepted { get; set; }
      
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      
      // 선택적 항목
      public int VoteCount { get; set; }        // 추천수 (추천 - 비추천)
      public int UpvoteCount { get; set; }      // 추천수
      public int DownvoteCount { get; set; }    // 비추천수
      
      // 소프트 삭제
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }
      
      // 동적 확장 필드
      public Dictionary<string, object>? ExtendedProperties { get; set; }
      
      // Navigation Properties
      public virtual Question Question { get; set; } = null!;
  }
  ```

#### 1.3 Report(신고) 엔티티 설계
- [ ] Report 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Report.cs`
  ```csharp
  public class Report : IEntity
  {
      public long Id { get; set; }
      
      // 신고 대상
      public ReportTargetType TargetType { get; set; }  // Post, Comment, Question, Answer
      public long TargetId { get; set; }
      
      // 신고자 정보
      public long ReporterId { get; set; }
      public string ReporterName { get; set; } = string.Empty;
      
      // 신고 내용
      public ReportReason Reason { get; set; }
      public string? Description { get; set; }          // 상세 설명
      
      // 처리 상태
      public ReportStatus Status { get; set; } = ReportStatus.Pending;
      public long? ProcessedById { get; set; }          // 처리자 ID
      public string? ProcessedByName { get; set; }      // 처리자명
      public DateTime? ProcessedAt { get; set; }
      public string? ProcessingNote { get; set; }       // 처리 메모
      
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
  
  public enum ReportTargetType
  {
      Post = 0,
      Comment = 1,
      Question = 2,
      Answer = 3
  }
  
  public enum ReportReason
  {
      Spam = 0,               // 스팸/광고
      Inappropriate = 1,      // 부적절한 내용
      Harassment = 2,         // 욕설/비방
      Copyright = 3,          // 저작권 침해
      PersonalInfo = 4,       // 개인정보 노출
      Other = 99              // 기타
  }
  
  public enum ReportStatus
  {
      Pending = 0,            // 대기 중
      Approved = 1,           // 승인 (콘텐츠 블라인드)
      Rejected = 2,           // 거부 (신고 기각)
      Resolved = 3            // 해결됨
  }
  ```

#### 1.4 AnswerVote(답변 추천) 엔티티 설계
- [ ] AnswerVote 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/AnswerVote.cs`
  ```csharp
  public class AnswerVote
  {
      public long Id { get; set; }
      
      public long AnswerId { get; set; }
      public long UserId { get; set; }
      
      public VoteType VoteType { get; set; }  // Up, Down
      
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
      // Navigation
      public virtual Answer Answer { get; set; } = null!;
  }
  
  public enum VoteType
  {
      Up = 1,
      Down = -1
  }
  ```

#### 1.5 QuestionVote(질문 추천) 엔티티 설계 (선택적)
- [ ] QuestionVote 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/QuestionVote.cs`
  ```csharp
  public class QuestionVote
  {
      public long Id { get; set; }
      
      public long QuestionId { get; set; }
      public long UserId { get; set; }
      
      public VoteType VoteType { get; set; }
      
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
      // Navigation
      public virtual Question Question { get; set; } = null!;
  }
  ```

#### 1.6 데이터베이스 설정
- [ ] BoardDbContext에 DbSet 추가
  ```csharp
  public DbSet<Question> Questions => Set<Question>();
  public DbSet<Answer> Answers => Set<Answer>();
  public DbSet<Report> Reports => Set<Report>();
  public DbSet<AnswerVote> AnswerVotes => Set<AnswerVote>();
  public DbSet<QuestionVote> QuestionVotes => Set<QuestionVote>();
  ```

- [ ] OnModelCreating에서 엔티티 설정
  - Questions 테이블 인덱스: (Status, CreatedAt), (AuthorId), (Tags)
  - Answers 테이블 인덱스: (QuestionId, IsAccepted), (AuthorId)
  - Reports 테이블 인덱스: (TargetType, TargetId), (Status, CreatedAt)
  - AnswerVotes 테이블: 복합키 (AnswerId, UserId)
  - QuestionVotes 테이블: 복합키 (QuestionId, UserId)
  - 소프트 삭제 글로벌 필터 적용

---

### 2. Q&A DTO 설계

#### 2.1 Question 관련 DTO
- [ ] `src/BoardCommonLibrary/DTOs/QnARequests.cs` 생성

  ```csharp
  /// <summary>
  /// 질문 생성 요청
  /// </summary>
  public class CreateQuestionRequest
  {
      [Required(ErrorMessage = "제목은 필수입니다.")]
      [MaxLength(200, ErrorMessage = "제목은 최대 200자입니다.")]
      public string Title { get; set; } = string.Empty;
      
      [Required(ErrorMessage = "내용은 필수입니다.")]
      public string Content { get; set; } = string.Empty;
      
      public List<string>? Tags { get; set; }
      
      public int BountyPoints { get; set; }  // 현상금 (선택적)
  }
  
  /// <summary>
  /// 질문 수정 요청
  /// </summary>
  public class UpdateQuestionRequest
  {
      [Required(ErrorMessage = "제목은 필수입니다.")]
      [MaxLength(200, ErrorMessage = "제목은 최대 200자입니다.")]
      public string Title { get; set; } = string.Empty;
      
      [Required(ErrorMessage = "내용은 필수입니다.")]
      public string Content { get; set; } = string.Empty;
      
      public List<string>? Tags { get; set; }
  }
  
  /// <summary>
  /// 질문 목록 조회 파라미터
  /// </summary>
  public class QuestionQueryParameters
  {
      public int Page { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      
      public QuestionStatus? Status { get; set; }
      public string? Tag { get; set; }
      public long? AuthorId { get; set; }
      
      public string Sort { get; set; } = "createdAt";  // createdAt, viewCount, voteCount, answerCount
      public string Order { get; set; } = "desc";
      
      public string? Query { get; set; }  // 검색어
  }
  ```

#### 2.2 Answer 관련 DTO
  ```csharp
  /// <summary>
  /// 답변 생성 요청
  /// </summary>
  public class CreateAnswerRequest
  {
      [Required(ErrorMessage = "내용은 필수입니다.")]
      public string Content { get; set; } = string.Empty;
  }
  
  /// <summary>
  /// 답변 수정 요청
  /// </summary>
  public class UpdateAnswerRequest
  {
      [Required(ErrorMessage = "내용은 필수입니다.")]
      public string Content { get; set; } = string.Empty;
  }
  
  /// <summary>
  /// 답변 추천 요청
  /// </summary>
  public class VoteAnswerRequest
  {
      [Required]
      public VoteType VoteType { get; set; }
  }
  ```

#### 2.3 Q&A Response DTO
- [ ] `src/BoardCommonLibrary/DTOs/QnAResponses.cs` 생성

  ```csharp
  /// <summary>
  /// 질문 응답
  /// </summary>
  public class QuestionResponse
  {
      public long Id { get; set; }
      public string Title { get; set; } = string.Empty;
      public string Content { get; set; } = string.Empty;
      
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      
      public QuestionStatus Status { get; set; }
      public string StatusText { get; set; } = string.Empty;  // "미해결", "답변됨", "종료됨"
      
      public int ViewCount { get; set; }
      public int VoteCount { get; set; }
      public int AnswerCount { get; set; }
      public int BountyPoints { get; set; }
      
      public List<string> Tags { get; set; } = new();
      
      public long? AcceptedAnswerId { get; set; }
      
      public DateTime CreatedAt { get; set; }
      public DateTime? UpdatedAt { get; set; }
  }
  
  /// <summary>
  /// 질문 상세 응답 (답변 포함)
  /// </summary>
  public class QuestionDetailResponse : QuestionResponse
  {
      public List<AnswerResponse> Answers { get; set; } = new();
      public AnswerResponse? AcceptedAnswer { get; set; }
  }
  
  /// <summary>
  /// 답변 응답
  /// </summary>
  public class AnswerResponse
  {
      public long Id { get; set; }
      public string Content { get; set; } = string.Empty;
      
      public long QuestionId { get; set; }
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      
      public bool IsAccepted { get; set; }
      
      public int VoteCount { get; set; }
      public int UpvoteCount { get; set; }
      public int DownvoteCount { get; set; }
      
      public DateTime CreatedAt { get; set; }
      public DateTime? UpdatedAt { get; set; }
      
      // 현재 사용자의 투표 상태 (로그인 시)
      public VoteType? CurrentUserVote { get; set; }
  }
  ```

---

### 3. 신고(Report) DTO 설계

#### 3.1 Report 관련 DTO
- [ ] `src/BoardCommonLibrary/DTOs/ReportRequests.cs` 생성

  ```csharp
  /// <summary>
  /// 신고 생성 요청
  /// </summary>
  public class CreateReportRequest
  {
      [Required]
      public ReportTargetType TargetType { get; set; }
      
      [Required]
      public long TargetId { get; set; }
      
      [Required]
      public ReportReason Reason { get; set; }
      
      [MaxLength(500)]
      public string? Description { get; set; }
  }
  
  /// <summary>
  /// 신고 처리 요청
  /// </summary>
  public class ProcessReportRequest
  {
      [Required]
      public ReportStatus Status { get; set; }  // Approved, Rejected
      
      [MaxLength(500)]
      public string? ProcessingNote { get; set; }
  }
  
  /// <summary>
  /// 신고 목록 조회 파라미터
  /// </summary>
  public class ReportQueryParameters
  {
      public int Page { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      
      public ReportStatus? Status { get; set; }
      public ReportTargetType? TargetType { get; set; }
      public ReportReason? Reason { get; set; }
      
      public DateTime? FromDate { get; set; }
      public DateTime? ToDate { get; set; }
      
      public string Sort { get; set; } = "createdAt";
      public string Order { get; set; } = "desc";
  }
  ```

#### 3.2 Report Response DTO
- [ ] `src/BoardCommonLibrary/DTOs/ReportResponses.cs` 생성

  ```csharp
  /// <summary>
  /// 신고 응답
  /// </summary>
  public class ReportResponse
  {
      public long Id { get; set; }
      
      public ReportTargetType TargetType { get; set; }
      public string TargetTypeText { get; set; } = string.Empty;
      public long TargetId { get; set; }
      public string? TargetTitle { get; set; }      // 신고 대상 제목/내용 요약
      public string? TargetAuthorName { get; set; } // 신고 대상 작성자
      
      public long ReporterId { get; set; }
      public string ReporterName { get; set; } = string.Empty;
      
      public ReportReason Reason { get; set; }
      public string ReasonText { get; set; } = string.Empty;
      public string? Description { get; set; }
      
      public ReportStatus Status { get; set; }
      public string StatusText { get; set; } = string.Empty;
      
      public long? ProcessedById { get; set; }
      public string? ProcessedByName { get; set; }
      public DateTime? ProcessedAt { get; set; }
      public string? ProcessingNote { get; set; }
      
      public DateTime CreatedAt { get; set; }
  }
  ```

---

### 4. 관리자 DTO 설계

#### 4.1 Admin 관련 DTO
- [ ] `src/BoardCommonLibrary/DTOs/AdminRequests.cs` 생성

  ```csharp
  /// <summary>
  /// 게시물 관리 조회 파라미터
  /// </summary>
  public class AdminPostQueryParameters
  {
      public int Page { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      
      public PostStatus? Status { get; set; }
      public bool? IsDeleted { get; set; }
      public bool? IsBlinded { get; set; }
      public long? AuthorId { get; set; }
      public string? Category { get; set; }
      
      public DateTime? FromDate { get; set; }
      public DateTime? ToDate { get; set; }
      
      public string? Query { get; set; }
      public string Sort { get; set; } = "createdAt";
      public string Order { get; set; } = "desc";
  }
  
  /// <summary>
  /// 댓글 관리 조회 파라미터
  /// </summary>
  public class AdminCommentQueryParameters
  {
      public int Page { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      
      public bool? IsDeleted { get; set; }
      public bool? IsBlinded { get; set; }
      public long? AuthorId { get; set; }
      public long? PostId { get; set; }
      
      public DateTime? FromDate { get; set; }
      public DateTime? ToDate { get; set; }
      
      public string? Query { get; set; }
      public string Sort { get; set; } = "createdAt";
      public string Order { get; set; } = "desc";
  }
  
  /// <summary>
  /// 일괄 삭제 요청
  /// </summary>
  public class BatchDeleteRequest
  {
      [Required]
      public BatchTargetType TargetType { get; set; }
      
      [Required]
      [MinLength(1)]
      public List<long> Ids { get; set; } = new();
      
      /// <summary>
      /// 영구 삭제 여부 (기본: 소프트 삭제)
      /// </summary>
      public bool HardDelete { get; set; } = false;
  }
  
  public enum BatchTargetType
  {
      Post = 0,
      Comment = 1,
      Question = 2,
      Answer = 3
  }
  
  /// <summary>
  /// 블라인드 요청
  /// </summary>
  public class BlindContentRequest
  {
      [Required]
      public BatchTargetType TargetType { get; set; }
      
      [Required]
      public long TargetId { get; set; }
      
      /// <summary>
      /// true: 블라인드, false: 블라인드 해제
      /// </summary>
      public bool IsBlinded { get; set; } = true;
      
      public string? Reason { get; set; }
  }
  ```

#### 4.2 Statistics DTO
- [ ] `src/BoardCommonLibrary/DTOs/StatisticsResponses.cs` 생성

  ```csharp
  /// <summary>
  /// 게시판 통계 응답
  /// </summary>
  public class BoardStatisticsResponse
  {
      // 기본 통계
      public long TotalPosts { get; set; }
      public long TotalComments { get; set; }
      public long TotalQuestions { get; set; }
      public long TotalAnswers { get; set; }
      public long TotalFiles { get; set; }
      
      // 기간별 통계 (오늘)
      public long TodayPosts { get; set; }
      public long TodayComments { get; set; }
      public long TodayQuestions { get; set; }
      public long TodayAnswers { get; set; }
      
      // 기간별 통계 (이번 주)
      public long WeeklyPosts { get; set; }
      public long WeeklyComments { get; set; }
      public long WeeklyQuestions { get; set; }
      public long WeeklyAnswers { get; set; }
      
      // 기간별 통계 (이번 달)
      public long MonthlyPosts { get; set; }
      public long MonthlyComments { get; set; }
      public long MonthlyQuestions { get; set; }
      public long MonthlyAnswers { get; set; }
      
      // 활동 통계
      public long TotalViews { get; set; }
      public long TotalLikes { get; set; }
      public long ActiveUsers { get; set; }      // 최근 7일 활동 사용자
      
      // 신고 통계
      public long PendingReports { get; set; }
      public long TotalReports { get; set; }
      
      // 인기 콘텐츠
      public List<PopularPostResponse> PopularPosts { get; set; } = new();
      public List<PopularQuestionResponse> PopularQuestions { get; set; } = new();
      
      // 기간별 트렌드 (최근 7일)
      public List<DailyStatistics> DailyTrend { get; set; } = new();
  }
  
  public class PopularPostResponse
  {
      public long Id { get; set; }
      public string Title { get; set; } = string.Empty;
      public int ViewCount { get; set; }
      public int LikeCount { get; set; }
      public int CommentCount { get; set; }
  }
  
  public class PopularQuestionResponse
  {
      public long Id { get; set; }
      public string Title { get; set; } = string.Empty;
      public int ViewCount { get; set; }
      public int VoteCount { get; set; }
      public int AnswerCount { get; set; }
  }
  
  public class DailyStatistics
  {
      public DateTime Date { get; set; }
      public int PostCount { get; set; }
      public int CommentCount { get; set; }
      public int QuestionCount { get; set; }
      public int AnswerCount { get; set; }
  }
  ```

---

### 5. Q&A 서비스 인터페이스 설계

#### 5.1 IQuestionService 인터페이스
- [ ] `src/BoardCommonLibrary/Interfaces/IQuestionService.cs` 생성

  ```csharp
  public interface IQuestionService
  {
      // CRUD
      Task<QuestionResponse> CreateAsync(CreateQuestionRequest request, long authorId, string authorName);
      Task<QuestionDetailResponse?> GetByIdAsync(long id, long? currentUserId = null);
      Task<PagedResult<QuestionResponse>> GetAllAsync(QuestionQueryParameters parameters);
      Task<QuestionResponse> UpdateAsync(long id, UpdateQuestionRequest request, long userId);
      Task<bool> DeleteAsync(long id, long userId);
      
      // 조회수
      Task IncrementViewCountAsync(long id);
      
      // 상태 관리
      Task<QuestionResponse> CloseAsync(long id, long userId);
      Task<QuestionResponse> ReopenAsync(long id, long userId);
      
      // 추천
      Task<int> VoteAsync(long id, long userId, VoteType voteType);
      Task<bool> RemoveVoteAsync(long id, long userId);
      
      // 소유권 확인
      Task<bool> IsAuthorAsync(long questionId, long userId);
      Task<bool> ExistsAsync(long id);
  }
  ```

#### 5.2 IAnswerService 인터페이스
- [ ] `src/BoardCommonLibrary/Interfaces/IAnswerService.cs` 생성

  ```csharp
  public interface IAnswerService
  {
      // CRUD
      Task<AnswerResponse> CreateAsync(long questionId, CreateAnswerRequest request, long authorId, string authorName);
      Task<AnswerResponse?> GetByIdAsync(long id, long? currentUserId = null);
      Task<List<AnswerResponse>> GetByQuestionIdAsync(long questionId, long? currentUserId = null);
      Task<AnswerResponse> UpdateAsync(long id, UpdateAnswerRequest request, long userId);
      Task<bool> DeleteAsync(long id, long userId);
      
      // 채택
      Task<AnswerResponse> AcceptAsync(long answerId, long questionAuthorId);
      Task<bool> UnacceptAsync(long answerId, long questionAuthorId);
      
      // 추천
      Task<AnswerResponse> VoteAsync(long id, long userId, VoteType voteType);
      Task<bool> RemoveVoteAsync(long id, long userId);
      
      // 소유권 확인
      Task<bool> IsAuthorAsync(long answerId, long userId);
      Task<bool> ExistsAsync(long id);
  }
  ```

---

### 6. 신고 서비스 인터페이스 설계

#### 6.1 IReportService 인터페이스
- [ ] `src/BoardCommonLibrary/Interfaces/IReportService.cs` 생성

  ```csharp
  public interface IReportService
  {
      // 신고 생성 (일반 사용자)
      Task<ReportResponse> CreateAsync(CreateReportRequest request, long reporterId, string reporterName);
      
      // 신고 조회 (관리자)
      Task<ReportResponse?> GetByIdAsync(long id);
      Task<PagedResult<ReportResponse>> GetAllAsync(ReportQueryParameters parameters);
      
      // 신고 처리 (관리자)
      Task<ReportResponse> ProcessAsync(long id, ProcessReportRequest request, long processedById, string processedByName);
      
      // 신고 통계
      Task<int> GetPendingCountAsync();
      Task<Dictionary<ReportTargetType, int>> GetCountByTargetTypeAsync();
      
      // 중복 신고 확인
      Task<bool> HasReportedAsync(long reporterId, ReportTargetType targetType, long targetId);
      
      // 자동 블라인드 확인 (신고 N회 이상)
      Task<bool> ShouldAutoBlindAsync(ReportTargetType targetType, long targetId, int threshold = 5);
  }
  ```

---

### 7. 관리자 서비스 인터페이스 설계

#### 7.1 IAdminService 인터페이스
- [ ] `src/BoardCommonLibrary/Interfaces/IAdminService.cs` 생성

  ```csharp
  public interface IAdminService
  {
      // 게시물 관리
      Task<PagedResult<PostResponse>> GetPostsAsync(AdminPostQueryParameters parameters);
      Task<bool> BlindPostAsync(long id, string? reason = null);
      Task<bool> UnblindPostAsync(long id);
      Task<bool> RestorePostAsync(long id);  // 소프트 삭제 복원
      
      // 댓글 관리
      Task<PagedResult<CommentResponse>> GetCommentsAsync(AdminCommentQueryParameters parameters);
      Task<bool> BlindCommentAsync(long id, string? reason = null);
      Task<bool> UnblindCommentAsync(long id);
      Task<bool> RestoreCommentAsync(long id);
      
      // 일괄 처리
      Task<BatchDeleteResult> BatchDeleteAsync(BatchDeleteRequest request, long adminId);
      Task<BatchBlindResult> BatchBlindAsync(BatchTargetType targetType, List<long> ids, bool isBlinded, long adminId);
      
      // 통계
      Task<BoardStatisticsResponse> GetStatisticsAsync();
      Task<List<DailyStatistics>> GetDailyStatisticsAsync(DateTime fromDate, DateTime toDate);
  }
  
  public class BatchDeleteResult
  {
      public int TotalRequested { get; set; }
      public int SuccessCount { get; set; }
      public int FailedCount { get; set; }
      public List<BatchOperationError> Errors { get; set; } = new();
  }
  
  public class BatchBlindResult
  {
      public int TotalRequested { get; set; }
      public int SuccessCount { get; set; }
      public int FailedCount { get; set; }
      public List<BatchOperationError> Errors { get; set; } = new();
  }
  
  public class BatchOperationError
  {
      public long Id { get; set; }
      public string ErrorCode { get; set; } = string.Empty;
      public string ErrorMessage { get; set; } = string.Empty;
  }
  ```

---

### 8. 서비스 구현

#### 8.1 QuestionService 구현
- [ ] `src/BoardCommonLibrary/Services/QuestionService.cs` 생성
  - 질문 CRUD 구현
  - 조회수 증가 (중복 방지 옵션)
  - 상태 관리 (Open → Answered → Closed)
  - 추천 기능 (중복 방지)
  - 답변 채택 시 상태 자동 변경

#### 8.2 AnswerService 구현
- [ ] `src/BoardCommonLibrary/Services/AnswerService.cs` 생성
  - 답변 CRUD 구현
  - 채택 기능 (질문 작성자만 가능)
  - 채택 시 질문 상태 자동 변경
  - 추천/비추천 기능
  - 질문의 답변 수 자동 업데이트

#### 8.3 ReportService 구현
- [ ] `src/BoardCommonLibrary/Services/ReportService.cs` 생성
  - 신고 생성 (중복 신고 방지)
  - 신고 목록 조회 (필터링, 페이징)
  - 신고 처리 (승인/거부)
  - 승인 시 콘텐츠 자동 블라인드
  - 자동 블라인드 임계값 확인

#### 8.4 AdminService 구현
- [ ] `src/BoardCommonLibrary/Services/AdminService.cs` 생성
  - 전체 게시물/댓글 관리 조회
  - 블라인드 처리/해제
  - 소프트 삭제 복원
  - 일괄 삭제/블라인드
  - 통계 집계

---

### 9. 컨트롤러 구현

#### 9.1 QuestionsController 구현
- [ ] `src/BoardCommonLibrary/Controllers/QuestionsController.cs` 생성

  ```csharp
  [ApiController]
  [Route("api/questions")]
  public class QuestionsController : ControllerBase
  {
      // GET    /api/questions              - 질문 목록
      // GET    /api/questions/{id}         - 질문 상세 (답변 포함)
      // POST   /api/questions              - 질문 작성
      // PUT    /api/questions/{id}         - 질문 수정
      // DELETE /api/questions/{id}         - 질문 삭제
      // POST   /api/questions/{id}/vote    - 질문 추천
      // DELETE /api/questions/{id}/vote    - 질문 추천 취소
      // POST   /api/questions/{id}/close   - 질문 종료
  }
  ```

#### 9.2 AnswersController 구현
- [ ] `src/BoardCommonLibrary/Controllers/AnswersController.cs` 생성

  ```csharp
  [ApiController]
  [Route("api")]
  public class AnswersController : ControllerBase
  {
      // GET    /api/questions/{questionId}/answers  - 답변 목록
      // POST   /api/questions/{questionId}/answers  - 답변 작성
      // PUT    /api/answers/{id}                    - 답변 수정
      // DELETE /api/answers/{id}                    - 답변 삭제
      // POST   /api/answers/{id}/accept             - 답변 채택
      // POST   /api/answers/{id}/vote               - 답변 추천
      // DELETE /api/answers/{id}/vote               - 답변 추천 취소
  }
  ```

#### 9.3 ReportsController 구현
- [ ] `src/BoardCommonLibrary/Controllers/ReportsController.cs` 생성

  ```csharp
  [ApiController]
  [Route("api/reports")]
  public class ReportsController : ControllerBase
  {
      // POST   /api/reports                - 신고하기 (일반 사용자)
  }
  ```

#### 9.4 AdminController 구현
- [ ] `src/BoardCommonLibrary/Controllers/AdminController.cs` 생성

  ```csharp
  [ApiController]
  [Route("api/admin")]
  // [Authorize(Roles = "Admin,Moderator")]  // 실제 운영 시 활성화
  public class AdminController : ControllerBase
  {
      // GET    /api/admin/posts            - 전체 게시물 관리
      // GET    /api/admin/comments         - 전체 댓글 관리
      // GET    /api/admin/reports          - 신고 목록
      // GET    /api/admin/reports/{id}     - 신고 상세
      // PUT    /api/admin/reports/{id}     - 신고 처리
      // POST   /api/admin/blind            - 콘텐츠 블라인드
      // POST   /api/admin/batch/delete     - 일괄 삭제
      // GET    /api/admin/statistics       - 통계 조회
  }
  ```

---

### 10. DI 설정

#### 10.1 ServiceCollectionExtensions 업데이트
- [ ] `src/BoardCommonLibrary/Extensions/ServiceCollectionExtensions.cs`에 추가

  ```csharp
  // Q&A 서비스
  services.AddScoped<IQuestionService, QuestionService>();
  services.AddScoped<IAnswerService, AnswerService>();
  
  // 신고 서비스
  services.AddScoped<IReportService, ReportService>();
  
  // 관리자 서비스
  services.AddScoped<IAdminService, AdminService>();
  ```

---

### 11. 단위 테스트 작성

#### 11.1 QuestionServiceTests
- [ ] `tests/BoardCommonLibrary.Tests/Services/QuestionServiceTests.cs` 생성
  - CreateAsync_ValidRequest_ReturnsQuestion
  - CreateAsync_InvalidTitle_ThrowsValidationException
  - GetByIdAsync_ExistingQuestion_ReturnsQuestionWithAnswers
  - GetByIdAsync_NonExisting_ReturnsNull
  - UpdateAsync_ByAuthor_UpdatesSuccessfully
  - UpdateAsync_ByNonAuthor_ThrowsForbidden
  - DeleteAsync_NoAnswers_DeletesSuccessfully
  - DeleteAsync_HasAnswers_ThrowsException (답변 있으면 삭제 불가)
  - VoteAsync_FirstTime_AddsVote
  - VoteAsync_AlreadyVoted_UpdatesVote
  - CloseAsync_ByAuthor_ChangesStatus

#### 11.2 AnswerServiceTests
- [ ] `tests/BoardCommonLibrary.Tests/Services/AnswerServiceTests.cs` 생성
  - CreateAsync_ValidRequest_ReturnsAnswer
  - CreateAsync_IncreasesQuestionAnswerCount
  - AcceptAsync_ByQuestionAuthor_AcceptsAnswer
  - AcceptAsync_ByNonQuestionAuthor_ThrowsForbidden
  - AcceptAsync_ChangesQuestionStatusToAnswered
  - VoteAsync_Upvote_IncreasesVoteCount
  - VoteAsync_Downvote_DecreasesVoteCount
  - VoteAsync_ChangeVote_UpdatesCorrectly
  - DeleteAsync_DecreasesQuestionAnswerCount

#### 11.3 ReportServiceTests
- [ ] `tests/BoardCommonLibrary.Tests/Services/ReportServiceTests.cs` 생성
  - CreateAsync_ValidReport_ReturnsReport
  - CreateAsync_DuplicateReport_ThrowsException
  - ProcessAsync_Approved_BlindsContent
  - ProcessAsync_Rejected_DoesNotBlindContent
  - ShouldAutoBlindAsync_ExceedsThreshold_ReturnsTrue
  - GetPendingCountAsync_ReturnsPendingCount

#### 11.4 AdminServiceTests
- [ ] `tests/BoardCommonLibrary.Tests/Services/AdminServiceTests.cs` 생성
  - GetPostsAsync_ReturnsAllPosts_IncludingDeleted
  - BlindPostAsync_SetsIsBlindedTrue
  - BatchDeleteAsync_DeletesMultiplePosts
  - GetStatisticsAsync_ReturnsCorrectCounts

---

### 12. TestPage4Controller 구현

#### 12.1 테스트 웹 컨트롤러 업데이트
- [ ] `test-web/BoardTestWeb/Controllers/TestPage4Controller.cs` 업데이트
  - 실제 서비스를 주입받아 테스트
  - 모든 Q&A 엔드포인트 연결
  - 모든 관리자 엔드포인트 연결

---

### 13. 문서 업데이트

#### 13.1 PAGES.md 업데이트
- [ ] 페이지 4 상태를 🟢 완료로 변경
- [ ] 테스트 수 업데이트 (최소 15개 → 실제 테스트 수)
- [ ] 전체 진행률 100% 업데이트

#### 13.2 PRD.md 업데이트 (선택)
- [ ] Q&A 관련 API 명세 검토 및 보완

---

## 📋 테스트 케이스

| 테스트 ID | 테스트명 | 테스트 내용 | 예상 결과 |
|----------|---------|------------|----------|
| T4-001 | 관리자 게시물 조회 | 관리자가 전체 게시물 조회 (삭제된 것 포함) | 200 OK, 전체 목록 |
| T4-002 | 관리자 권한 검증 | 일반 사용자가 관리자 API 접근 | 403 Forbidden |
| T4-003 | 신고 목록 조회 | 관리자가 신고 목록 조회 | 200 OK, 신고 목록 |
| T4-004 | 신고 승인 처리 | 신고 승인 및 콘텐츠 블라인드 | 200 OK, 상태 변경 |
| T4-005 | 신고 거부 처리 | 신고 거부 | 200 OK, 상태 변경 |
| T4-006 | 콘텐츠 블라인드 | 게시물/댓글 블라인드 처리 | 200 OK, isBlinded = true |
| T4-007 | 일괄 삭제 | 여러 게시물 일괄 삭제 | 200 OK, 삭제 수 반환 |
| T4-008 | 통계 조회 | 게시판 통계 데이터 | 200 OK, 통계 데이터 |
| T4-009 | 질문 작성 성공 | Q&A 질문 작성 | 201 Created |
| T4-010 | 질문 목록 조회 | Q&A 질문 목록 | 200 OK, 질문 목록 |
| T4-011 | 답변 작성 성공 | 질문에 답변 작성 | 201 Created |
| T4-012 | 답변 채택 성공 | 질문자가 답변 채택 | 200 OK, isAccepted = true |
| T4-013 | 답변 채택 - 권한 없음 | 질문자 아닌 사용자가 채택 시도 | 403 Forbidden |
| T4-014 | 답변 추천 | 답변 추천 | voteCount 증가 |
| T4-015 | 질문 상태 변경 | 답변 채택 시 질문 상태 자동 변경 | status = Answered |

---

## ✅ 완료 조건

### 필수 완료 항목
- [ ] Question/Answer 엔티티 생성 및 DbContext 설정
- [ ] Report 엔티티 생성
- [ ] Q&A DTOs 생성 (Requests, Responses)
- [ ] Report DTOs 생성
- [ ] Admin DTOs 생성
- [ ] IQuestionService, IAnswerService 인터페이스 정의
- [ ] IReportService 인터페이스 정의
- [ ] IAdminService 인터페이스 정의
- [ ] QuestionService, AnswerService 구현
- [ ] ReportService 구현
- [ ] AdminService 구현
- [ ] QuestionsController, AnswersController 구현
- [ ] ReportsController 구현
- [ ] AdminController 구현
- [ ] DI 설정 완료
- [ ] 모든 API 엔드포인트 구현 완료
- [ ] 모든 테스트 케이스 통과 (15개 이상)
- [ ] 관리자 권한 검증 로직
- [ ] 신고 자동 블라인드 로직 (신고 N회 이상 시)
- [ ] Q&A 답변 채택 로직
- [ ] 통계 집계 기능

### 추가 완료 항목 (선택)
- [ ] 현상금(Bounty) 기능 구현
- [ ] 질문/답변 댓글 기능 (기존 Comment 테이블 활용)
- [ ] 인기 질문/답변 추천 알고리즘
- [ ] 감사 로그(AuditLog) 연동

---

## 🔄 작업 순서 권장

1. **1단계**: Q&A 엔티티 및 DbContext 설정 (Question, Answer, Vote)
2. **2단계**: Report 엔티티 설정
3. **3단계**: Q&A DTOs 생성
4. **4단계**: Report/Admin DTOs 생성  
5. **5단계**: 서비스 인터페이스 정의
6. **6단계**: 서비스 구현 (QuestionService → AnswerService → ReportService → AdminService)
7. **7단계**: 컨트롤러 구현
8. **8단계**: DI 설정
9. **9단계**: 단위 테스트 작성 및 실행
10. **10단계**: TestPage4Controller 업데이트
11. **11단계**: 문서 업데이트

---

## 📝 참고사항

### Q&A 비즈니스 로직
- 답변이 있는 질문은 삭제 불가 (또는 소프트 삭제만 가능)
- 답변 채택은 질문 작성자만 가능
- 답변 채택 시 질문 상태 자동으로 `Answered`로 변경
- 이미 채택된 답변이 있으면 기존 채택 취소 후 새로운 답변 채택
- 본인 답변에는 추천 불가

### 신고 비즈니스 로직
- 동일 콘텐츠에 동일 사용자가 중복 신고 불가
- 신고 승인 시 해당 콘텐츠 자동 블라인드
- 신고 N회 이상 누적 시 자동 블라인드 (임계값 설정 가능)
- 신고 처리 시 처리자 정보 및 메모 기록

### 관리자 권한
- 관리자(Admin): 모든 기능 접근 가능
- 모더레이터(Moderator): 콘텐츠 관리, 신고 처리 가능 (통계 제외)
- 테스트 환경에서는 권한 검증 비활성화 가능

---

*최종 업데이트: 2025-11-29*
