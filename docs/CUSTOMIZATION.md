# 라이브러리 커스터마이징 가이드

## 📖 개요

Board Common Library는 상속(Inheritance)을 통한 확장을 지원하도록 설계되었습니다. 모든 서비스와 컨트롤러의 메서드가 `virtual`로 선언되어 있어, 사용자가 필요에 따라 특정 동작을 오버라이드하여 커스터마이징할 수 있습니다.

---

## 🏗️ 아키텍처 원칙

### 확장 가능한 설계 패턴

```
┌─────────────────────────────────────────────────────┐
│                  Your Application                    │
├─────────────────────────────────────────────────────┤
│   CustomPostService   │   CustomPostsController     │
│   (extends PostService)│   (extends PostsController) │
├─────────────────────────────────────────────────────┤
│                Board Common Library                  │
│   PostService (virtual methods)                      │
│   PostsController (virtual methods)                  │
└─────────────────────────────────────────────────────┘
```

### 핵심 원칙

1. **protected readonly 필드**: 모든 의존성 필드가 `protected readonly`로 선언되어 상속받는 클래스에서 접근 가능
2. **virtual 메서드**: 모든 public 메서드가 `virtual`로 선언되어 오버라이드 가능
3. **Hook 메서드**: 생명주기 이벤트를 위한 Hook 메서드 제공 (OnCreated, OnUpdated, OnDeleted)
4. **TryAdd 서비스 등록**: 사용자가 먼저 등록한 커스텀 서비스가 기본 서비스보다 우선

---

## 🔧 서비스 커스터마이징

### 1. PostService 커스터마이징

```csharp
using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;

public class CustomPostService : PostService
{
    private readonly ILogger<CustomPostService> _logger;
    
    public CustomPostService(
        BoardDbContext context, 
        ILogger<CustomPostService> logger) 
        : base(context)
    {
        _logger = logger;
    }
    
    // 게시물 생성 후 추가 로직 (Hook 메서드 오버라이드)
    protected override async Task OnPostCreatedAsync(Post post)
    {
        await base.OnPostCreatedAsync(post);
        
        // 커스텀 로직: 슬랙 알림, 검색 인덱스 업데이트 등
        _logger.LogInformation("새 게시물이 생성되었습니다: {PostId} - {Title}", post.Id, post.Title);
        await NotifySlackAsync(post);
    }
    
    // 게시물 조회 로직 오버라이드
    public override async Task<PostResponse?> GetByIdAsync(long id)
    {
        var post = await base.GetByIdAsync(id);
        
        // 커스텀 로직: 추가 데이터 로드, 감사 로그 등
        if (post != null)
        {
            _logger.LogInformation("게시물 조회: {PostId}", id);
        }
        
        return post;
    }
    
    // 게시물 목록 조회 커스터마이징
    protected override IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostQueryParameters parameters)
    {
        query = base.ApplyFilters(query, parameters);
        
        // 커스텀 필터 추가: 예) 특정 부서만 조회
        if (parameters.ExtendedFilters?.TryGetValue("department", out var dept) == true)
        {
            query = query.Where(p => p.ExtendedProperties!.Contains($"\"department\":\"{dept}\""));
        }
        
        return query;
    }
    
    // 응답 매핑 커스터마이징
    protected override PostResponse MapToResponse(Post post)
    {
        var response = base.MapToResponse(post);
        
        // 커스텀 필드 추가
        response.ExtendedProperties ??= new Dictionary<string, object>();
        response.ExtendedProperties["customField"] = "커스텀 값";
        
        return response;
    }
    
    private async Task NotifySlackAsync(Post post)
    {
        // Slack 알림 로직
        await Task.CompletedTask;
    }
}
```

### 2. CommentService 커스터마이징

```csharp
public class CustomCommentService : CommentService
{
    private readonly IBadWordFilter _badWordFilter;
    
    public CustomCommentService(
        BoardDbContext context,
        IBadWordFilter badWordFilter) 
        : base(context)
    {
        _badWordFilter = badWordFilter;
    }
    
    // 댓글 생성 전 검증 로직 추가
    public override async Task<CommentResponse> CreateAsync(
        long postId, 
        CreateCommentRequest request, 
        long authorId, 
        string authorName)
    {
        // 비속어 필터링
        if (_badWordFilter.ContainsBadWords(request.Content))
        {
            throw new ValidationException("부적절한 내용이 포함되어 있습니다.");
        }
        
        return await base.CreateAsync(postId, request, authorId, authorName);
    }
    
    // 댓글 삭제 시 추가 로직
    protected override async Task OnCommentDeletedAsync(Comment comment)
    {
        await base.OnCommentDeletedAsync(comment);
        
        // 관련 알림 삭제, 캐시 무효화 등
    }
}
```

### 3. QuestionService 커스터마이징

```csharp
public class CustomQuestionService : QuestionService
{
    private readonly IPointService _pointService;
    
    public CustomQuestionService(
        BoardDbContext context,
        IPointService pointService) 
        : base(context)
    {
        _pointService = pointService;
    }
    
    // 답변 채택 시 포인트 지급
    protected override async Task OnAnswerAcceptedAsync(Question question, Answer answer)
    {
        await base.OnAnswerAcceptedAsync(question, answer);
        
        // 답변 작성자에게 포인트 지급
        await _pointService.AddPointsAsync(answer.AuthorId, 50, "답변 채택");
        
        // 질문에 현상금이 있으면 추가 지급
        if (question.BountyPoints > 0)
        {
            await _pointService.AddPointsAsync(answer.AuthorId, question.BountyPoints, "현상금");
        }
    }
}
```

---

## 🎮 컨트롤러 커스터마이징

### 1. PostsController 커스터마이징

```csharp
using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[Route("api/custom-posts")]  // 커스텀 라우트
public class CustomPostsController : PostsController
{
    private readonly INotificationService _notificationService;
    
    public CustomPostsController(
        IPostService postService,
        IViewCountService viewCountService,
        ILikeService likeService,
        IBookmarkService bookmarkService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator,
        INotificationService notificationService)
        : base(postService, viewCountService, likeService, bookmarkService, 
               createValidator, updateValidator, draftValidator)
    {
        _notificationService = notificationService;
    }
    
    // 게시물 생성 후 알림 발송
    [HttpPost]
    public override async Task<ActionResult<PostResponse>> Create(
        [FromBody] CreatePostRequest request)
    {
        var result = await base.Create(request);
        
        if (result.Result is CreatedAtActionResult createdResult 
            && createdResult.Value is PostResponse post)
        {
            // 구독자들에게 알림 발송
            await _notificationService.NotifySubscribersAsync(post.Id);
        }
        
        return result;
    }
    
    // 추가 엔드포인트
    [HttpGet("featured")]
    public virtual async Task<ActionResult<IEnumerable<PostResponse>>> GetFeaturedPosts()
    {
        var posts = await PostService.GetFeaturedPostsAsync();
        return Ok(posts);
    }
    
    // 현재 사용자 정보 커스터마이징
    protected override long GetCurrentUserId()
    {
        // 커스텀 인증 로직
        var customClaim = User.FindFirst("custom_user_id");
        if (customClaim != null && long.TryParse(customClaim.Value, out var userId))
        {
            return userId;
        }
        return base.GetCurrentUserId();
    }
}
```

### 2. AdminController 커스터마이징

```csharp
[Route("api/custom-admin")]
public class CustomAdminController : AdminController
{
    private readonly IAuditLogService _auditLogService;
    
    public CustomAdminController(
        IAdminService adminService,
        IReportService reportService,
        IValidator<ProcessReportRequest> processReportValidator,
        IAuditLogService auditLogService)
        : base(adminService, reportService, processReportValidator)
    {
        _auditLogService = auditLogService;
    }
    
    // 게시물 삭제 시 감사 로그 기록
    [HttpDelete("posts/{id}")]
    public override async Task<ActionResult> DeletePost(long id)
    {
        var result = await base.DeletePost(id);
        
        if (result is NoContentResult)
        {
            await _auditLogService.LogAsync(
                action: "DELETE_POST",
                entityType: "Post",
                entityId: id,
                userId: GetCurrentUserId(),
                ipAddress: GetClientIpAddress()
            );
        }
        
        return result;
    }
}
```

---

## 📦 서비스 등록

### 방법 1: 개별 등록 (AddBoardLibrary 호출 전)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 커스텀 서비스를 먼저 등록 (TryAdd로 인해 우선됨)
builder.Services.AddCustomPostService<CustomPostService>();
builder.Services.AddCustomCommentService<CustomCommentService>();

// 라이브러리 기본 설정 (커스텀 서비스가 등록되지 않은 것만 기본 서비스 사용)
builder.Services.AddBoardLibrary(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
});
```

### 방법 2: 확장 메서드 사용

```csharp
// Program.cs
builder.Services.AddBoardLibraryWithCustomServices(
    // 커스텀 서비스 설정
    config =>
    {
        config.UseCustomPostService<CustomPostService>();
        config.UseCustomCommentService<CustomCommentService>();
        config.UseCustomQuestionService<CustomQuestionService>();
    },
    // 라이브러리 옵션 설정
    options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.FileUpload.MaxFileSize = 20 * 1024 * 1024;
    }
);
```

### 방법 3: 직접 등록

```csharp
// Program.cs
builder.Services.AddScoped<IPostService, CustomPostService>();
builder.Services.AddScoped<ICommentService, CustomCommentService>();

// AddBoardLibrary에서 TryAddScoped를 사용하므로 
// 위에서 등록한 커스텀 서비스가 유지됨
builder.Services.AddBoardLibrary(options => { ... });
```

---

## 🪝 Hook 메서드

각 서비스는 생명주기 이벤트를 위한 Hook 메서드를 제공합니다:

### PostService Hooks

| Hook 메서드 | 호출 시점 | 용도 |
|------------|---------|------|
| `OnPostCreatedAsync(Post post)` | 게시물 생성 후 | 알림, 검색 인덱싱, 감사 로그 |
| `OnPostUpdatedAsync(Post post)` | 게시물 수정 후 | 캐시 무효화, 버전 관리 |
| `OnPostDeletedAsync(Post post)` | 게시물 삭제 후 | 관련 데이터 정리, 알림 |

### CommentService Hooks

| Hook 메서드 | 호출 시점 | 용도 |
|------------|---------|------|
| `OnCommentCreatedAsync(Comment comment)` | 댓글 생성 후 | 알림, 게시물 통계 업데이트 |
| `OnCommentUpdatedAsync(Comment comment)` | 댓글 수정 후 | 수정 이력 기록 |
| `OnCommentDeletedAsync(Comment comment)` | 댓글 삭제 후 | 관련 알림 삭제 |

### QuestionService Hooks

| Hook 메서드 | 호출 시점 | 용도 |
|------------|---------|------|
| `OnQuestionCreatedAsync(Question question)` | 질문 생성 후 | 태그 통계 업데이트 |
| `OnAnswerAcceptedAsync(Question, Answer)` | 답변 채택 시 | 포인트 지급, 알림 |

---

## 🔀 분해된 메서드 (Decomposed Methods)

복잡한 로직을 부분적으로 커스터마이징할 수 있도록 메서드가 분해되어 있습니다:

### PostService 쿼리 분해

```csharp
// 기본 쿼리 생성
protected virtual IQueryable<Post> BuildBaseQuery();

// 필터 적용
protected virtual IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostQueryParameters parameters);

// 정렬 적용
protected virtual IQueryable<Post> ApplySorting(IQueryable<Post> query, string? sort, string? order);

// 페이징 및 프로젝션
protected virtual Task<PagedResult<PostListResponse>> ApplyPagingAndProject(
    IQueryable<Post> query, int page, int pageSize);

// 응답 매핑
protected virtual PostResponse MapToResponse(Post post);
protected virtual PostListResponse MapToListResponse(Post post);
```

### 사용 예시: 필터만 커스터마이징

```csharp
public class CustomPostService : PostService
{
    protected override IQueryable<Post> ApplyFilters(
        IQueryable<Post> query, 
        PostQueryParameters parameters)
    {
        // 기본 필터 적용
        query = base.ApplyFilters(query, parameters);
        
        // 추가 커스텀 필터
        if (!string.IsNullOrEmpty(parameters.CustomDepartment))
        {
            query = query.Where(p => p.Department == parameters.CustomDepartment);
        }
        
        return query;
    }
}
```

---

## 📝 확장 가능한 엔티티

### ExtendedProperties 사용

```csharp
// 게시물 생성 시 확장 필드 추가
var request = new CreatePostRequest
{
    Title = "테스트 게시물",
    Content = "내용",
    ExtendedProperties = new Dictionary<string, object>
    {
        ["department"] = "개발팀",
        ["priority"] = 1,
        ["customField"] = "커스텀 값"
    }
};
```

### 커스텀 엔티티 상속 (고급)

```csharp
// 커스텀 게시물 엔티티
public class CustomPost : Post
{
    public string Department { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? Deadline { get; set; }
}

// 커스텀 DbContext
public class CustomBoardDbContext : BoardDbContext
{
    public DbSet<CustomPost> CustomPosts => Set<CustomPost>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 커스텀 엔티티 설정
        modelBuilder.Entity<CustomPost>(entity =>
        {
            entity.Property(e => e.Department).HasMaxLength(100);
        });
    }
}
```

---

## ⚠️ 주의사항

### 1. base 호출

오버라이드 시 필요한 경우 `base` 메서드를 호출하여 기본 로직을 유지하세요:

```csharp
public override async Task<PostResponse?> GetByIdAsync(long id)
{
    // 기본 로직 실행
    var result = await base.GetByIdAsync(id);
    
    // 추가 로직
    // ...
    
    return result;
}
```

### 2. 서비스 등록 순서

`TryAddScoped`를 사용하므로, 커스텀 서비스는 `AddBoardLibrary()` 호출 **전에** 등록해야 합니다:

```csharp
// ✅ 올바른 순서
builder.Services.AddScoped<IPostService, CustomPostService>();
builder.Services.AddBoardLibrary(...);

// ❌ 잘못된 순서 (커스텀 서비스 무시됨)
builder.Services.AddBoardLibrary(...);
builder.Services.AddScoped<IPostService, CustomPostService>();  // 무시됨!
```

### 3. 생성자 매개변수

커스텀 서비스 생성자에서 기본 클래스의 의존성을 모두 전달해야 합니다:

```csharp
public class CustomPostService : PostService
{
    public CustomPostService(
        BoardDbContext context,  // 필수: 기본 클래스에 전달
        IMyCustomService myService)  // 선택: 추가 의존성
        : base(context)  // 기본 클래스 생성자 호출
    {
        // ...
    }
}
```

---

## 🛤️ API 경로 커스터마이징

### 개요

Board Common Library는 모든 API 경로를 사용자가 원하는 대로 커스터마이징할 수 있습니다. 
`ApiRouteOptions` 클래스를 통해 각 컨트롤러의 경로를 설정할 수 있습니다.

### 기본 경로

| 컨트롤러 | 기본 경로 | 설명 |
|---------|----------|------|
| PostsController | `/api/posts` | 게시물 API |
| CommentsController | `/api/comments` | 댓글 API |
| FilesController | `/api/files` | 파일 API |
| SearchController | `/api/search` | 검색 API |
| UsersController | `/api/users` | 사용자 API |
| QuestionsController | `/api/questions` | Q&A 질문 API |
| AnswersController | `/api/answers` | Q&A 답변 API |
| ReportsController | `/api/reports` | 신고 API |
| AdminController | `/api/admin` | 관리자 API |

### 커스터마이징 방법

#### 1. ApiRouteOptions 설정

```csharp
using BoardCommonLibrary.Configuration;
using BoardCommonLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// API 경로 옵션 설정
var apiRouteOptions = new ApiRouteOptions
{
    Prefix = "api/v1",        // 기본값: "api"
    Posts = "articles",       // /api/v1/articles (기본: posts)
    Comments = "replies",     // /api/v1/replies (기본: comments)
    Files = "attachments",    // /api/v1/attachments (기본: files)
    Search = "search",        // /api/v1/search (기본값 유지)
    Users = "members",        // /api/v1/members (기본: users)
    Questions = "qna",        // /api/v1/qna (기본: questions)
    Answers = "solutions",    // /api/v1/solutions (기본: answers)
    Reports = "reports",      // /api/v1/reports (기본값 유지)
    Admin = "management"      // /api/v1/management (기본: admin)
};

// BoardCommonLibrary 서비스 등록
builder.Services.AddBoardLibrary(options =>
{
    options.ConnectionString = "...";
    options.Routes = apiRouteOptions;
});

// 컨트롤러에 라우트 컨벤션 적용 (중요!)
builder.Services.AddControllers(options =>
{
    options.UseBoardLibraryRoutes(apiRouteOptions);
})
    .AddApplicationPart(typeof(BoardCommonLibrary.Controllers.PostsController).Assembly);
```

#### 2. 설정 옵션

| 옵션 | 타입 | 기본값 | 설명 |
|-----|------|--------|------|
| `Prefix` | string | `"api"` | 모든 API의 기본 접두사 |
| `Posts` | string | `"posts"` | 게시물 API 경로 |
| `Comments` | string | `"comments"` | 댓글 API 경로 |
| `Files` | string | `"files"` | 파일 API 경로 |
| `Search` | string | `"search"` | 검색 API 경로 |
| `Users` | string | `"users"` | 사용자 API 경로 |
| `Questions` | string | `"questions"` | Q&A 질문 API 경로 |
| `Answers` | string | `"answers"` | Q&A 답변 API 경로 |
| `Reports` | string | `"reports"` | 신고 API 경로 |
| `Admin` | string | `"admin"` | 관리자 API 경로 |

### 커스터마이징 예시

#### 버전 포함 API

```csharp
var apiRouteOptions = new ApiRouteOptions
{
    Prefix = "api/v2"  // 결과: /api/v2/posts, /api/v2/comments 등
};
```

#### 도메인 기반 API

```csharp
var apiRouteOptions = new ApiRouteOptions
{
    Prefix = "board-api",
    Posts = "threads",
    Comments = "messages"
};
// 결과: /board-api/threads, /board-api/messages
```

#### Q&A를 단일 경로로 통합

```csharp
var apiRouteOptions = new ApiRouteOptions
{
    Questions = "qna",
    Answers = "qna/answers"  // 계층 구조 가능
};
// 결과: /api/qna, /api/qna/answers
```

### 주의 사항

1. **AddControllers 호출 시 컨벤션 적용**: `UseBoardLibraryRoutes()`를 반드시 `AddControllers()` 내에서 호출해야 합니다.

2. **커스텀 컨트롤러는 별도 라우트**: 커스텀 컨트롤러를 상속받아 만든 경우, `[Route]` 어트리뷰트를 직접 지정해야 합니다.

```csharp
// 커스텀 컨트롤러는 자체 라우트 지정
[Route("api/v1/custom-posts")]
public class CustomPostsController : PostsController
{
    // ...
}
```

3. **CommentsController 특별 처리**: CommentsController는 여러 리소스에 대한 댓글을 처리하므로 내부적으로 `posts/{postId}/comments` 형태의 경로도 자동으로 업데이트됩니다.

---

## 🎯 다중 게시판 생성 예시

### 시나리오: 2개의 Q&A 게시판 운영

하나의 애플리케이션에서 **기술 Q&A**와 **일반 Q&A** 두 개의 게시판을 운영하고 싶은 경우, 
각각의 컨트롤러를 상속받아 별도의 API 엔드포인트로 구성할 수 있습니다.

### 1. 기술 Q&A 컨트롤러 (TechQuestionsController)

```csharp
using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 기술 Q&A 게시판 컨트롤러
/// 프로그래밍, 개발 관련 질문만 허용
/// </summary>
[Route("api/tech-qna")]
[Tags("Tech Q&A")]
public class TechQuestionsController : QuestionsController
{
    // 기술 관련 태그만 허용
    private static readonly HashSet<string> _allowedTechTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "C#", ".NET", "ASP.NET", "Entity Framework", "SQL", "Database",
        "JavaScript", "TypeScript", "React", "Vue", "Angular",
        "Docker", "Kubernetes", "Azure", "AWS", "DevOps",
        "API", "REST", "GraphQL", "Performance", "Security",
        "Git", "Architecture", "Design Pattern", "Testing",
        "Python", "Java", "Go", "Rust"
    };

    public TechQuestionsController(
        IQuestionService questionService,
        IAnswerService answerService,
        IValidator<CreateQuestionRequest> createQuestionValidator,
        IValidator<UpdateQuestionRequest> updateQuestionValidator,
        IValidator<CreateAnswerRequest> createAnswerValidator,
        IValidator<UpdateAnswerRequest> updateAnswerValidator)
        : base(questionService, answerService, createQuestionValidator, 
               updateQuestionValidator, createAnswerValidator, updateAnswerValidator)
    {
    }

    /// <summary>
    /// 기술 Q&A 질문 작성 (기술 태그 검증 포함)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<QuestionResponse>> Create(
        [FromBody] CreateQuestionRequest request)
    {
        // 태그 검증: 최소 1개의 기술 태그 필요
        if (request.Tags == null || !request.Tags.Any(t => _allowedTechTags.Contains(t)))
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "INVALID_TECH_TAG",
                    message = $"기술 Q&A에는 유효한 기술 태그가 최소 1개 이상 필요합니다. " +
                              $"허용 태그: {string.Join(", ", _allowedTechTags.Take(10))}..."
                }
            });
        }

        return await base.Create(request);
    }

    /// <summary>
    /// 허용된 기술 태그 목록 조회
    /// </summary>
    [HttpGet("allowed-tags")]
    public ActionResult<object> GetAllowedTags()
    {
        return Ok(new
        {
            tags = _allowedTechTags.OrderBy(t => t).ToList(),
            description = "기술 Q&A 게시판에서 사용 가능한 태그 목록입니다."
        });
    }
}
```

### 2. 일반 Q&A 컨트롤러 (GeneralQuestionsController)

```csharp
using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 일반 Q&A 게시판 컨트롤러
/// 일상, 취미, 생활 관련 질문용
/// </summary>
[Route("api/general-qna")]
[Tags("General Q&A")]
public class GeneralQuestionsController : QuestionsController
{
    // 일반 관련 태그
    private static readonly HashSet<string> _allowedGeneralTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "일상", "취미", "여행", "음식", "건강", "운동", "독서",
        "영화", "음악", "게임", "반려동물", "육아", "교육",
        "재테크", "부동산", "자동차", "패션", "뷰티",
        "질문", "추천", "의견", "도움요청", "기타",
        "취업", "진로", "창업"
    };

    public GeneralQuestionsController(
        IQuestionService questionService,
        IAnswerService answerService,
        IValidator<CreateQuestionRequest> createQuestionValidator,
        IValidator<UpdateQuestionRequest> updateQuestionValidator,
        IValidator<CreateAnswerRequest> createAnswerValidator,
        IValidator<UpdateAnswerRequest> updateAnswerValidator)
        : base(questionService, answerService, createQuestionValidator,
               updateQuestionValidator, createAnswerValidator, updateAnswerValidator)
    {
    }

    /// <summary>
    /// 일반 Q&A 질문 작성 (기술 태그 사용 시 경고)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<QuestionResponse>> Create(
        [FromBody] CreateQuestionRequest request)
    {
        // 기술 태그 사용 시 경고 (거부하지 않고 안내만)
        var techTags = new[] { "C#", ".NET", "JavaScript", "Python", "Docker" };
        if (request.Tags?.Any(t => techTags.Contains(t, StringComparer.OrdinalIgnoreCase)) == true)
        {
            // 헤더로 경고 전달
            Response.Headers.Append("X-Warning", 
                "기술 관련 질문은 /api/tech-qna를 이용해주세요.");
        }

        return await base.Create(request);
    }

    /// <summary>
    /// 허용된 일반 태그 목록 조회
    /// </summary>
    [HttpGet("allowed-tags")]
    public ActionResult<object> GetAllowedTags()
    {
        return Ok(new
        {
            tags = _allowedGeneralTags.OrderBy(t => t).ToList(),
            description = "일반 Q&A 게시판에서 사용 가능한 태그 목록입니다."
        });
    }
}
```

### 3. Program.cs 설정

```csharp
var builder = WebApplication.CreateBuilder(args);

// BoardCommonLibrary 서비스 등록
builder.Services.AddBoardLibrary(options =>
{
    options.ConnectionString = "...";
});

// 컨트롤러 등록 (커스텀 컨트롤러 포함)
builder.Services.AddControllers()
    .AddApplicationPart(typeof(BoardCommonLibrary.Controllers.PostsController).Assembly);

var app = builder.Build();

app.MapControllers();
app.Run();
```

### 4. 결과 API 엔드포인트

| 게시판 | 엔드포인트 | 설명 |
|-------|-----------|------|
| 기술 Q&A | `GET /api/tech-qna` | 기술 질문 목록 |
| 기술 Q&A | `POST /api/tech-qna` | 기술 질문 작성 (태그 검증) |
| 기술 Q&A | `GET /api/tech-qna/allowed-tags` | 허용 기술 태그 목록 |
| 일반 Q&A | `GET /api/general-qna` | 일반 질문 목록 |
| 일반 Q&A | `POST /api/general-qna` | 일반 질문 작성 |
| 일반 Q&A | `GET /api/general-qna/allowed-tags` | 허용 일반 태그 목록 |

### 5. 테스트 예시

```bash
# 기술 Q&A - 허용 태그 조회
curl http://localhost:5117/api/tech-qna/allowed-tags
# {"tags":["Angular","API","ASP.NET","AWS","Azure","C#",...]}

# 일반 Q&A - 허용 태그 조회
curl http://localhost:5117/api/general-qna/allowed-tags
# {"tags":["건강","게임","교육","기타","도움요청",...]}

# 기술 Q&A - 질문 작성 (기술 태그 필수)
curl -X POST http://localhost:5117/api/tech-qna \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{"title":"C# LINQ 사용법","content":"...","tags":["C#",".NET"]}'
# 201 Created

# 기술 Q&A - 일반 태그로 질문 시도 (실패)
curl -X POST http://localhost:5117/api/tech-qna \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{"title":"여행 추천","content":"...","tags":["여행","추천"]}'
# 400 Bad Request - "기술 Q&A에는 유효한 기술 태그가 필요합니다"

# 일반 Q&A - 질문 작성
curl -X POST http://localhost:5117/api/general-qna \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{"title":"여행지 추천","content":"...","tags":["여행","추천"]}'
# 201 Created
```

### 핵심 포인트

1. **독립적인 라우트**: 각 컨트롤러에 `[Route("api/tech-qna")]`, `[Route("api/general-qna")]` 처럼 별도 경로 지정
2. **태그 유효성 검사**: `Create()` 메서드를 오버라이드하여 게시판별 태그 정책 적용
3. **추가 엔드포인트**: `GetAllowedTags()` 같은 게시판 전용 API 추가 가능
4. **Swagger 분리**: `[Tags("Tech Q&A")]` 어트리뷰트로 Swagger UI에서 그룹 분리

---

## 📝 복수 게시물 게시판 예시

### 시나리오: 공지사항 + 자유게시판 운영

하나의 애플리케이션에서 **공지사항**(관리자 전용)과 **자유게시판**(모든 회원 가능)을 운영하는 경우입니다.

### 1. 공지사항 컨트롤러 (NoticePostsController)

```csharp
using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 공지사항 게시판 컨트롤러
/// 관리자만 작성 가능, 모든 사용자 조회 가능
/// </summary>
[Route("api/notices")]
[Tags("공지사항 게시판")]
public class NoticePostsController : PostsController
{
    public NoticePostsController(
        IPostService postService,
        IViewCountService viewCountService,
        ILikeService likeService,
        IBookmarkService bookmarkService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator)
        : base(postService, viewCountService, likeService, bookmarkService,
               createValidator, updateValidator, draftValidator)
    {
    }

    /// <summary>
    /// 공지사항 작성 (관리자 전용)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Create(
        [FromBody] CreatePostRequest request)
    {
        // 관리자 권한 확인
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                error = new
                {
                    code = "ADMIN_ONLY",
                    message = "공지사항은 관리자만 작성할 수 있습니다."
                }
            });
        }

        // 공지사항 카테고리 설정
        request.Category = "notice";
        
        return await base.Create(request);
    }

    /// <summary>
    /// 게시판 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetBoardInfo()
    {
        return Ok(new
        {
            name = "공지사항",
            description = "관리자 공지사항 게시판입니다.",
            allowedRoles = new[] { "Admin" },
            features = new[] { "조회", "관리자 작성/수정/삭제" }
        });
    }

    protected override bool IsCurrentUserAdmin()
    {
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().Equals("admin", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
```

### 2. 자유게시판 컨트롤러 (FreeBoardPostsController)

```csharp
/// <summary>
/// 자유 게시판 컨트롤러
/// 모든 회원이 자유롭게 글을 작성할 수 있는 게시판
/// </summary>
[Route("api/free-board")]
[Tags("자유 게시판")]
public class FreeBoardPostsController : PostsController
{
    // 금지 단어 목록
    private static readonly HashSet<string> _forbiddenWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "광고", "홍보", "spam", "advertisement"
    };

    public FreeBoardPostsController(/* 생성자 매개변수 */)
        : base(/* */)
    {
    }

    /// <summary>
    /// 자유게시판 글 작성 (금지 단어 필터링)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Create(
        [FromBody] CreatePostRequest request)
    {
        // 금지 단어 체크
        var forbiddenWord = CheckForbiddenWords(request.Title, request.Content);
        if (forbiddenWord != null)
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "FORBIDDEN_WORD",
                    message = $"금지된 단어가 포함되어 있습니다: '{forbiddenWord}'"
                }
            });
        }

        request.Category = "free";
        return await base.Create(request);
    }

    /// <summary>
    /// 게시판 규칙 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetBoardInfo()
    {
        return Ok(new
        {
            name = "자유 게시판",
            description = "회원이라면 누구나 자유롭게 글을 작성할 수 있습니다.",
            rules = new[]
            {
                "광고/홍보 글은 삭제됩니다.",
                "타인을 비방하는 글은 금지됩니다."
            },
            features = new[] { "글 작성", "댓글", "좋아요", "북마크" }
        });
    }

    private string? CheckForbiddenWords(string title, string content)
    {
        var combined = $"{title} {content}";
        foreach (var word in _forbiddenWords)
        {
            if (combined.Contains(word, StringComparison.OrdinalIgnoreCase))
                return word;
        }
        return null;
    }
}
```

### 3. 결과 API 엔드포인트

| 게시판 | 엔드포인트 | 설명 |
|-------|-----------|------|
| 공지사항 | `GET /api/notices` | 공지사항 목록 |
| 공지사항 | `POST /api/notices` | 공지사항 작성 (관리자만) |
| 공지사항 | `GET /api/notices/info` | 게시판 정보 |
| 자유게시판 | `GET /api/free-board` | 자유게시판 목록 |
| 자유게시판 | `POST /api/free-board` | 자유게시판 글 작성 |
| 자유게시판 | `GET /api/free-board/info` | 게시판 정보 |

---

## 💬 복수 댓글 시스템 예시

### 시나리오: 공지사항 댓글 비활성화 + 자유게시판 댓글 필터링

### 1. 공지사항 댓글 컨트롤러 (댓글 작성 비활성화)

```csharp
/// <summary>
/// 공지사항 댓글 컨트롤러 (읽기 전용)
/// </summary>
[Route("api/notices")]
[Tags("공지사항 댓글")]
public class NoticeCommentsController : CommentsController
{
    public NoticeCommentsController(
        ICommentService commentService,
        ILikeService likeService,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
        : base(commentService, likeService, createValidator, updateValidator)
    {
    }

    /// <summary>
    /// 공지사항 댓글 목록 조회
    /// </summary>
    [HttpGet("{postId:long}/comments")]
    public override async Task<ActionResult<PagedResponse<CommentResponse>>> GetByPostId(
        long postId,
        [FromQuery] CommentQueryParameters parameters)
    {
        return await base.GetByPostId(postId, parameters);
    }

    /// <summary>
    /// 공지사항 댓글 작성 (비활성화)
    /// </summary>
    [HttpPost("{postId:long}/comments")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
        long postId,
        [FromBody] CreateCommentRequest request)
    {
        return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
            "COMMENTS_DISABLED",
            "공지사항에는 댓글을 작성할 수 없습니다."));
    }
}
```

### 2. 자유게시판 댓글 컨트롤러 (금지어 필터링 + 대댓글 제한)

```csharp
/// <summary>
/// 자유게시판 댓글 컨트롤러 (금지어 필터링, 대댓글 1단계 제한)
/// </summary>
[Route("api/free-board")]
[Tags("자유게시판 댓글")]
public class FreeBoardCommentsController : CommentsController
{
    private static readonly HashSet<string> ForbiddenWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "욕설", "비속어", "스팸", "광고"
    };

    public FreeBoardCommentsController(/* 생성자 */)
        : base(/* */)
    {
    }

    /// <summary>
    /// 자유게시판 댓글 작성 (금지어 필터링)
    /// </summary>
    [HttpPost("{postId:long}/comments")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
        long postId,
        [FromBody] CreateCommentRequest request)
    {
        if (ContainsForbiddenWords(request.Content))
        {
            return BadRequest(ApiErrorResponse.Create(
                "FORBIDDEN_WORDS",
                "댓글에 금지어가 포함되어 있습니다."));
        }

        return await base.Create(postId, request);
    }

    /// <summary>
    /// 대댓글 작성 (1단계만 허용)
    /// </summary>
    [HttpPost("comments/{parentId:long}/replies")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> CreateReply(
        long parentId,
        [FromBody] CreateCommentRequest request)
    {
        if (ContainsForbiddenWords(request.Content))
        {
            return BadRequest(ApiErrorResponse.Create(
                "FORBIDDEN_WORDS",
                "댓글에 금지어가 포함되어 있습니다."));
        }

        // 대댓글 깊이 제한
        var parentComment = await CommentService.GetByIdAsync(parentId);
        if (parentComment?.ParentId.HasValue == true)
        {
            return BadRequest(ApiErrorResponse.Create(
                "MAX_DEPTH_EXCEEDED",
                "자유게시판은 1단계 대댓글까지만 허용됩니다."));
        }

        return await base.CreateReply(parentId, request);
    }

    private static bool ContainsForbiddenWords(string content)
    {
        return ForbiddenWords.Any(word => 
            content.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}
```

---

## 📁 복수 파일 게시판 예시

### 시나리오: 이미지 갤러리 + 문서 자료실

### 1. 갤러리 파일 컨트롤러 (이미지만 허용)

```csharp
/// <summary>
/// 갤러리 파일 컨트롤러 (이미지 전용)
/// </summary>
[Route("api/gallery")]
[Tags("갤러리")]
public class GalleryFilesController : FilesController
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };
    
    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp"
    };
    
    private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

    public GalleryFilesController(IFileService fileService)
        : base(fileService)
    {
    }

    /// <summary>
    /// 갤러리 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetGalleryInfo()
    {
        return Ok(new
        {
            BoardType = "Gallery",
            AllowedExtensions = AllowedImageExtensions.ToArray(),
            MaxFileSizeMB = MaxImageSize / (1024 * 1024),
            Description = "이미지 파일만 업로드 가능한 갤러리입니다."
        });
    }

    /// <summary>
    /// 갤러리 이미지 업로드 (이미지만 허용)
    /// </summary>
    [HttpPost("upload")]
    public override async Task<IActionResult> Upload(IFormFile file, [FromQuery] long? postId = null)
    {
        var validationResult = ValidateImageFile(file);
        if (validationResult != null)
            return validationResult;

        return await base.Upload(file, postId);
    }

    private IActionResult? ValidateImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 필요합니다." });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
            return BadRequest(new { message = $"이미지 파일만 업로드 가능합니다. 허용 형식: {string.Join(", ", AllowedImageExtensions)}" });

        if (!AllowedImageMimeTypes.Contains(file.ContentType))
            return BadRequest(new { message = "올바른 이미지 파일이 아닙니다." });

        if (file.Length > MaxImageSize)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, 
                new { message = $"이미지 크기는 {MaxImageSize / (1024 * 1024)}MB 이하여야 합니다." });

        return null;
    }
}
```

### 2. 문서 자료실 컨트롤러 (문서만 허용)

```csharp
/// <summary>
/// 문서 자료실 컨트롤러 (문서 전용)
/// </summary>
[Route("api/documents")]
[Tags("자료실")]
public class DocumentFilesController : FilesController
{
    private static readonly HashSet<string> AllowedDocExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".hwp", ".zip"
    };
    
    private const long MaxDocumentSize = 50 * 1024 * 1024; // 50MB

    public DocumentFilesController(IFileService fileService)
        : base(fileService)
    {
    }

    /// <summary>
    /// 자료실 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetDocumentInfo()
    {
        return Ok(new
        {
            BoardType = "Documents",
            AllowedExtensions = AllowedDocExtensions.ToArray(),
            MaxFileSizeMB = MaxDocumentSize / (1024 * 1024),
            Description = "문서 파일만 업로드 가능한 자료실입니다."
        });
    }

    /// <summary>
    /// 자료실 문서 업로드 (문서만 허용)
    /// </summary>
    [HttpPost("upload")]
    public override async Task<IActionResult> Upload(IFormFile file, [FromQuery] long? postId = null)
    {
        var validationResult = ValidateDocumentFile(file);
        if (validationResult != null)
            return validationResult;

        return await base.Upload(file, postId);
    }

    private IActionResult? ValidateDocumentFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 필요합니다." });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedDocExtensions.Contains(extension))
            return BadRequest(new { message = $"문서 파일만 업로드 가능합니다. 허용 형식: {string.Join(", ", AllowedDocExtensions)}" });

        if (file.Length > MaxDocumentSize)
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { message = $"문서 크기는 {MaxDocumentSize / (1024 * 1024)}MB 이하여야 합니다." });

        return null;
    }
}
```

### 3. 결과 API 엔드포인트

| 게시판 | 엔드포인트 | 설명 |
|-------|-----------|------|
| 갤러리 | `GET /api/gallery/info` | 갤러리 정보 |
| 갤러리 | `POST /api/gallery/upload` | 이미지 업로드 (이미지만) |
| 갤러리 | `GET /api/gallery/{id}/thumbnail` | 썸네일 조회 |
| 자료실 | `GET /api/documents/info` | 자료실 정보 |
| 자료실 | `POST /api/documents/upload` | 문서 업로드 (문서만) |
| 자료실 | `GET /api/documents/{id}/download` | 문서 다운로드 |

---

## 🎯 복수 게시판 설계 핵심 원칙

### 1. 상속 기반 확장

모든 기본 컨트롤러(`PostsController`, `CommentsController`, `FilesController`, `QuestionsController`)는 
`protected readonly` 필드와 `virtual` 메서드를 제공하여 상속을 통한 커스터마이징을 지원합니다.

### 2. 라우트 독립성

각 커스텀 컨트롤러에 `[Route("api/custom-path")]` 어트리뷰트를 지정하여 
독립적인 API 엔드포인트를 생성합니다.

### 3. Swagger 그룹화

`[Tags("게시판명")]` 어트리뷰트로 Swagger UI에서 게시판별로 API를 그룹화합니다.

### 4. 비즈니스 로직 분리

- **권한 검증**: `IsCurrentUserAdmin()` 등 오버라이드
- **데이터 검증**: `Create()`, `Update()` 메서드에서 커스텀 검증 추가
- **추가 엔드포인트**: `GetBoardInfo()`, `GetAllowedTags()` 등 게시판 전용 API 추가

---

## 🔧 문제 해결 (Troubleshooting)

### 1. 401 Unauthorized 오류

#### 문제 현상

커스텀 컨트롤러에서 `POST`, `PUT`, `DELETE` 요청 시 401 Unauthorized 오류가 발생합니다.

```bash
# 요청
curl -X POST "http://localhost:5117/api/custom-posts" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{"title": "테스트", "content": "내용"}'

# 응답
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401
}
```

#### 원인

`PostsController`의 `GetCurrentUserId()` 메서드가 기본적으로 `X-User-Id` 헤더만 읽도록 구현되어 있습니다.
JWT 토큰의 Claims에서 사용자 ID를 추출하는 코드가 주석 처리되어 있습니다.

```csharp
// PostsController.cs - GetCurrentUserId() 메서드
protected virtual long? GetCurrentUserId()
{
    // 테스트용: X-User-Id 헤더에서 조회
    if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
        long.TryParse(userIdHeader, out var userId))
    {
        return userId;
    }
    
    // TODO: 실제 환경에서는 JWT Claims에서 사용자 ID 조회
    // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // if (long.TryParse(userIdClaim, out var claimUserId))
    // {
    //     return claimUserId;
    // }
    
    return null;  // userId가 null이면 401 반환
}
```

#### 해결 방법 1: 헤더 사용 (테스트/개발 환경)

JWT 토큰과 함께 `X-User-Id`, `X-User-Name` 헤더를 추가합니다:

```bash
curl -X POST "http://localhost:5117/api/custom-posts" \
  -H "Authorization: Bearer <your-token>" \
  -H "X-User-Id: 1" \
  -H "X-User-Name: admin" \
  -H "Content-Type: application/json" \
  -d '{"title": "테스트", "content": "내용"}'
```

#### 해결 방법 2: GetCurrentUserId() 오버라이드 (프로덕션 환경)

커스텀 컨트롤러에서 `GetCurrentUserId()` 메서드를 오버라이드하여 JWT Claims를 읽도록 구현합니다:

```csharp
public class CustomPostsController : PostsController
{
    public CustomPostsController(IPostService postService) : base(postService)
    {
    }
    
    // JWT Claims에서 사용자 ID 추출
    protected override long? GetCurrentUserId()
    {
        // 1. 헤더 확인 (테스트용)
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var headerUserId))
        {
            return headerUserId;
        }
        
        // 2. JWT Claims 확인 (프로덕션)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst("userId")?.Value;
        
        if (long.TryParse(userIdClaim, out var claimUserId))
        {
            return claimUserId;
        }
        
        return null;
    }
    
    // 마찬가지로 GetCurrentUserName()도 오버라이드 가능
    protected override string? GetCurrentUserName()
    {
        if (Request.Headers.TryGetValue("X-User-Name", out var nameHeader))
        {
            return nameHeader;
        }
        
        return User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            ?? User.FindFirst("name")?.Value
            ?? User.FindFirst("preferred_username")?.Value;
    }
}
```

#### 해결 방법 3: 미들웨어로 헤더 자동 설정

JWT 토큰이 유효할 때 자동으로 헤더를 설정하는 미들웨어를 추가합니다:

```csharp
// UserContextMiddleware.cs
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    
    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // JWT Claims에서 사용자 정보 추출
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value
                        ?? context.User.FindFirst("name")?.Value;
            
            // 헤더가 없으면 Claims 값으로 설정
            if (!string.IsNullOrEmpty(userId) && 
                !context.Request.Headers.ContainsKey("X-User-Id"))
            {
                context.Request.Headers["X-User-Id"] = userId;
            }
            
            if (!string.IsNullOrEmpty(userName) && 
                !context.Request.Headers.ContainsKey("X-User-Name"))
            {
                context.Request.Headers["X-User-Name"] = userName;
            }
        }
        
        await _next(context);
    }
}

// Program.cs에서 등록
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();  // 인증 후에 추가
app.UseAuthorization();
```

### 2. 커스텀 서비스가 적용되지 않음

#### 문제 현상

커스텀 서비스를 등록했지만 기본 서비스가 계속 사용됩니다.

#### 원인

`AddBoardLibrary()` 호출 **후에** 커스텀 서비스를 등록한 경우입니다.

#### 해결 방법

커스텀 서비스는 반드시 `AddBoardLibrary()` **전에** 등록해야 합니다:

```csharp
// Program.cs

// ✅ 올바른 순서
builder.Services.AddScoped<IPostService, CustomPostService>();
builder.Services.AddBoardLibrary(options => { ... });

// ❌ 잘못된 순서
builder.Services.AddBoardLibrary(options => { ... });
builder.Services.AddScoped<IPostService, CustomPostService>();  // 무시됨!
```

### 3. 훅 메서드가 호출되지 않음

#### 문제 현상

`OnPostCreatedAsync()` 등의 훅 메서드를 오버라이드했지만 호출되지 않습니다.

#### 원인

1. 커스텀 서비스가 올바르게 등록되지 않음
2. 컨트롤러가 기본 서비스를 직접 주입받고 있음

#### 해결 방법

1. 서비스 등록 순서 확인 (위의 문제 2 참조)
2. 컨트롤러에서 인터페이스로 주입받도록 확인:

```csharp
public class CustomPostsController : PostsController
{
    // ✅ 인터페이스로 주입
    public CustomPostsController(IPostService postService) : base(postService)
    {
    }
    
    // ❌ 구체 클래스로 주입하면 DI가 올바르게 작동하지 않을 수 있음
    // public CustomPostsController(PostService postService) : base(postService)
}
```

---

## 📚 참고 문서

- [PRD (제품 요구사항 문서)](PRD.md)
- [테스트 가이드](TESTING.md)
- [NuGet 배포 가이드](NUGET.md)

---

*최종 업데이트: 2025-01-24*
