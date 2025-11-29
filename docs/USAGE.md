# BoardCommonLibrary 사용 가이드

## 📖 개요

**BoardCommonLibrary**는 ASP.NET Core 8.0 기반의 재사용 가능한 게시판 API 라이브러리입니다.
NuGet 패키지로 배포되어 다양한 프로젝트에서 게시판 기능을 쉽게 통합할 수 있습니다.

---

## 📦 설치 방법

### NuGet 패키지 관리자
```powershell
Install-Package BoardCommonLibrary -Version 1.0.0
```

### .NET CLI
```bash
dotnet add package BoardCommonLibrary --version 1.0.0
```

### PackageReference
```xml
<PackageReference Include="BoardCommonLibrary" Version="1.0.0" />
```

---

## ⚙️ 기본 설정

### 1. Program.cs 설정

```csharp
using BoardCommonLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 컨트롤러 등록
builder.Services.AddControllers();

// 게시판 라이브러리 서비스 등록
builder.Services.AddBoardLibrary(options =>
{
    // SQL Server 사용 시
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // 또는 InMemory 데이터베이스 사용 (개발/테스트용)
    // options.UseInMemoryDatabase = true;
    // options.InMemoryDatabaseName = "BoardTestDb";
});

// Swagger 설정 (선택사항)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. InMemory 데이터베이스 사용 (테스트용)

```csharp
// 간편 설정
builder.Services.AddBoardLibraryInMemory("MyTestDatabase");
```

### 3. appsettings.json 설정

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BoardDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

---

## 🏗️ 아키텍처 구조

```
BoardCommonLibrary/
├── Controllers/          # API 컨트롤러
│   ├── PostsController.cs
│   ├── CommentsController.cs
│   └── UsersController.cs
├── Services/             # 비즈니스 로직
│   ├── Interfaces/
│   │   ├── IPostService.cs
│   │   ├── ICommentService.cs
│   │   ├── ILikeService.cs
│   │   ├── IBookmarkService.cs
│   │   └── IViewCountService.cs
│   ├── PostService.cs
│   ├── CommentService.cs
│   ├── LikeService.cs
│   ├── BookmarkService.cs
│   └── ViewCountService.cs
├── Entities/             # 데이터 모델
│   ├── Post.cs
│   ├── Comment.cs
│   ├── Like.cs
│   ├── Bookmark.cs
│   └── ViewRecord.cs
├── DTOs/                 # 데이터 전송 객체
│   ├── PostRequests.cs
│   ├── PostResponses.cs
│   ├── CommentRequests.cs
│   ├── CommentResponses.cs
│   └── ...
├── Validators/           # 입력 검증
│   ├── PostValidators.cs
│   └── CommentValidators.cs
└── Extensions/           # 확장 메서드
    └── ServiceCollectionExtensions.cs
```

---

## 📋 제공되는 서비스

### 1. IPostService - 게시물 서비스

```csharp
public interface IPostService
{
    // 게시물 목록 조회 (페이징, 정렬, 필터링)
    Task<PagedResponse<PostSummaryResponse>> GetAllAsync(PostQueryParameters parameters);
    
    // 게시물 상세 조회
    Task<PostResponse?> GetByIdAsync(long id);
    
    // 게시물 생성
    Task<PostResponse> CreateAsync(CreatePostRequest request, long authorId, string? authorName = null);
    
    // 게시물 수정
    Task<PostResponse?> UpdateAsync(long id, UpdatePostRequest request, long userId, bool isAdmin = false);
    
    // 게시물 삭제 (소프트 삭제)
    Task<bool> DeleteAsync(long id, long userId, bool isAdmin = false);
    
    // 상단고정 설정/해제
    Task<PostResponse?> PinAsync(long id);
    Task<PostResponse?> UnpinAsync(long id);
    
    // 임시저장
    Task<DraftPostResponse> SaveDraftAsync(DraftPostRequest request, long authorId, string? authorName = null);
    Task<PagedResponse<DraftPostResponse>> GetDraftsAsync(long authorId, DraftQueryParameters parameters);
    Task<DraftPostResponse?> GetDraftByIdAsync(long id, long authorId);
    Task<bool> DeleteDraftAsync(long id, long authorId);
    Task<PostResponse> PublishDraftAsync(long draftId, long authorId);
}
```

### 2. ICommentService - 댓글 서비스

```csharp
public interface ICommentService
{
    // 댓글 생성
    Task<CommentResponse> CreateAsync(long postId, CreateCommentRequest request, long authorId, string? authorName = null);
    
    // 댓글 목록 조회
    Task<PagedResponse<CommentResponse>> GetByPostIdAsync(long postId, CommentQueryParameters parameters);
    
    // 댓글 상세 조회
    Task<CommentResponse?> GetByIdAsync(long id);
    
    // 댓글 수정
    Task<CommentResponse?> UpdateAsync(long id, UpdateCommentRequest request, long currentUserId);
    
    // 댓글 삭제
    Task<bool> DeleteAsync(long id, long currentUserId, bool isAdmin = false);
    
    // 대댓글 생성
    Task<CommentResponse> CreateReplyAsync(long parentCommentId, CreateCommentRequest request, long authorId, string? authorName = null);
    
    // 대댓글 목록 조회
    Task<PagedResponse<CommentResponse>> GetRepliesAsync(long parentCommentId, CommentQueryParameters parameters);
}
```

### 3. ILikeService - 좋아요 서비스

```csharp
public interface ILikeService
{
    // 게시물 좋아요 추가/취소
    Task<LikeResponse> LikePostAsync(long postId, long userId);
    Task<LikeResponse?> UnlikePostAsync(long postId, long userId);
    
    // 댓글 좋아요 추가/취소
    Task<LikeResponse> LikeCommentAsync(long commentId, long userId);
    Task<LikeResponse?> UnlikeCommentAsync(long commentId, long userId);
    
    // 좋아요 여부 확인
    Task<bool> HasUserLikedPostAsync(long postId, long userId);
    Task<bool> HasUserLikedCommentAsync(long commentId, long userId);
}
```

### 4. IBookmarkService - 북마크 서비스

```csharp
public interface IBookmarkService
{
    // 북마크 추가/해제
    Task<bool> AddBookmarkAsync(long postId, long userId);
    Task<bool> RemoveBookmarkAsync(long postId, long userId);
    
    // 북마크 목록 조회
    Task<PagedResponse<BookmarkResponse>> GetUserBookmarksAsync(long userId, BookmarkQueryParameters parameters);
    
    // 북마크 여부 확인
    Task<bool> HasUserBookmarkedAsync(long postId, long userId);
}
```

### 5. IViewCountService - 조회수 서비스

```csharp
public interface IViewCountService
{
    // 조회수 증가 (24시간 중복 방지)
    Task<bool> IncrementViewCountAsync(long postId, long? userId, string? ipAddress);
    
    // 조회수 조회
    Task<int> GetViewCountAsync(long postId);
}
```

---

## 🌐 API 엔드포인트

### 게시물 API (`/api/posts`)

| 메서드 | 엔드포인트 | 설명 |
|-------|-----------|------|
| GET | `/api/posts` | 게시물 목록 조회 |
| GET | `/api/posts/{id}` | 게시물 상세 조회 |
| POST | `/api/posts` | 게시물 작성 |
| PUT | `/api/posts/{id}` | 게시물 수정 |
| DELETE | `/api/posts/{id}` | 게시물 삭제 |
| POST | `/api/posts/{id}/pin` | 상단고정 설정 |
| DELETE | `/api/posts/{id}/pin` | 상단고정 해제 |
| POST | `/api/posts/draft` | 임시저장 |
| GET | `/api/posts/draft` | 임시저장 목록 |
| GET | `/api/posts/draft/{id}` | 임시저장 상세 |
| DELETE | `/api/posts/draft/{id}` | 임시저장 삭제 |
| POST | `/api/posts/draft/{id}/publish` | 임시저장 발행 |
| POST | `/api/posts/{id}/like` | 좋아요 |
| DELETE | `/api/posts/{id}/like` | 좋아요 취소 |
| POST | `/api/posts/{id}/bookmark` | 북마크 추가 |
| DELETE | `/api/posts/{id}/bookmark` | 북마크 해제 |

### 댓글 API (`/api/comments`, `/api/posts/{postId}/comments`)

| 메서드 | 엔드포인트 | 설명 |
|-------|-----------|------|
| GET | `/api/posts/{postId}/comments` | 댓글 목록 조회 |
| POST | `/api/posts/{postId}/comments` | 댓글 작성 |
| GET | `/api/comments/{id}` | 댓글 상세 조회 |
| PUT | `/api/comments/{id}` | 댓글 수정 |
| DELETE | `/api/comments/{id}` | 댓글 삭제 |
| POST | `/api/comments/{id}/replies` | 대댓글 작성 |
| GET | `/api/comments/{id}/replies` | 대댓글 목록 |
| POST | `/api/comments/{id}/like` | 댓글 좋아요 |
| DELETE | `/api/comments/{id}/like` | 댓글 좋아요 취소 |

### 사용자 API (`/api/users`)

| 메서드 | 엔드포인트 | 설명 |
|-------|-----------|------|
| GET | `/api/users/me/bookmarks` | 내 북마크 목록 |

---

## 📊 DTO (Data Transfer Objects)

### 요청 DTO

#### CreatePostRequest - 게시물 생성
```csharp
public class CreatePostRequest
{
    public string Title { get; set; }      // 필수, 최대 200자
    public string Content { get; set; }    // 필수
    public string? Category { get; set; }  // 선택
    public List<string>? Tags { get; set; } // 선택
}
```

#### UpdatePostRequest - 게시물 수정
```csharp
public class UpdatePostRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
}
```

#### CreateCommentRequest - 댓글 생성
```csharp
public class CreateCommentRequest
{
    public string Content { get; set; }  // 필수, 최대 2000자
}
```

### 응답 DTO

#### PostResponse - 게시물 상세
```csharp
public class PostResponse
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; }
    public long AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public PostStatus Status { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDraft { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
```

#### PagedResponse<T> - 페이징 응답
```csharp
public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public PagedMetadata Meta { get; set; }
}

public class PagedMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
```

#### ApiResponse<T> - API 응답 래퍼
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Code { get; set; }
    public string Message { get; set; }
    public List<ValidationError>? Errors { get; set; }
}
```

---

## 🔍 쿼리 파라미터

### PostQueryParameters - 게시물 목록 조회
```csharp
public class PostQueryParameters
{
    public int Page { get; set; } = 1;           // 페이지 번호
    public int PageSize { get; set; } = 20;      // 페이지 크기 (최대 100)
    public string SortBy { get; set; } = "createdAt"; // 정렬 기준
    public string SortOrder { get; set; } = "desc";   // 정렬 순서
    public string? Category { get; set; }        // 카테고리 필터
    public string? Tag { get; set; }             // 태그 필터
    public string? AuthorId { get; set; }        // 작성자 필터
    public string? Status { get; set; }          // 상태 필터
    public string? Search { get; set; }          // 검색어
}
```

### CommentQueryParameters - 댓글 목록 조회
```csharp
public class CommentQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "createdAt";
    public string SortOrder { get; set; } = "asc";
    public bool IncludeReplies { get; set; } = true;
}
```

---

## 💡 사용 예시

### 1. 게시물 목록 조회

```http
GET /api/posts?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc&category=공지
```

**응답:**
```json
{
  "data": [
    {
      "id": 1,
      "title": "첫 번째 게시물",
      "contentPreview": "게시물 내용 미리보기...",
      "category": "공지",
      "tags": ["중요", "공지"],
      "authorId": 1,
      "authorName": "관리자",
      "viewCount": 100,
      "likeCount": 10,
      "commentCount": 5,
      "isPinned": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

### 2. 게시물 작성

```http
POST /api/posts
Content-Type: application/json
X-User-Id: 1
X-User-Name: 홍길동

{
  "title": "새 게시물",
  "content": "게시물 내용입니다.",
  "category": "일반",
  "tags": ["태그1", "태그2"]
}
```

### 3. 댓글 작성

```http
POST /api/posts/1/comments
Content-Type: application/json
X-User-Id: 2
X-User-Name: 김철수

{
  "content": "좋은 게시물이네요!"
}
```

### 4. 대댓글 작성

```http
POST /api/comments/1/replies
Content-Type: application/json
X-User-Id: 1

{
  "content": "감사합니다!"
}
```

### 5. 좋아요

```http
POST /api/posts/1/like
X-User-Id: 2
```

### 6. 북마크

```http
POST /api/posts/1/bookmark
X-User-Id: 2
```

---

## 🔧 커스텀 컨트롤러 작성

라이브러리의 서비스를 주입받아 커스텀 컨트롤러를 작성할 수 있습니다.

```csharp
using BoardCommonLibrary.Services.Interfaces;
using BoardCommonLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MyPostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    
    public MyPostsController(
        IPostService postService,
        ICommentService commentService,
        ILikeService likeService)
    {
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
    }
    
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularPosts()
    {
        var parameters = new PostQueryParameters
        {
            SortBy = "likeCount",
            SortOrder = "desc",
            PageSize = 10
        };
        
        var result = await _postService.GetAllAsync(parameters);
        return Ok(result);
    }
    
    [HttpGet("{id}/full")]
    public async Task<IActionResult> GetPostWithComments(long id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post == null) return NotFound();
        
        var comments = await _commentService.GetByPostIdAsync(id, new CommentQueryParameters());
        
        return Ok(new { Post = post, Comments = comments });
    }
}
```

---

## 🧪 테스트

### 테스트 프로젝트 설정

```csharp
using BoardCommonLibrary.Extensions;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class PostServiceTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostService _postService;
    
    public PostServiceTests()
    {
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory("TestDb");
        
        _serviceProvider = services.BuildServiceProvider();
        _postService = _serviceProvider.GetRequiredService<IPostService>();
    }
    
    [Fact]
    public async Task CreatePost_ShouldReturnNewPost()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "테스트 제목",
            Content = "테스트 내용"
        };
        
        // Act
        var result = await _postService.CreateAsync(request, authorId: 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("테스트 제목", result.Title);
    }
}
```

---

## 📈 현재 개발 현황

| 기능 | 상태 | 테스트 수 |
|-----|------|----------|
| **페이지 1: 게시물 관리** | ✅ 완료 | 119개 |
| **페이지 2: 댓글/좋아요/북마크** | ✅ 완료 | 66개 |
| **페이지 3: 파일/검색** | 🔴 대기 | - |
| **페이지 4: 관리자/Q&A** | 🔴 대기 | - |
| **전체** | **50%** | **185개** |

---

## 📚 참고 문서

- [PRD (제품 요구사항 문서)](PRD.md)
- [페이지별 기능 명세](PAGES.md)
- [NuGet 배포 가이드](NUGET.md)
- [테스트 가이드](TESTING.md)

---

*최종 업데이트: 2024-11-29*
