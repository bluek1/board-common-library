# GitHub Copilot 개발 지침서

## 📋 프로젝트 개요

**게시판 공통 라이브러리(Board Common Library)**는 ASP.NET Core 8.0+ 기반의 재사용 가능한 게시판 API 라이브러리입니다.
NuGet 패키지로 배포되어 다양한 프로젝트에서 게시판 기능을 쉽게 통합할 수 있습니다.

### 핵심 기술 스택

| 기술 | 버전 | 용도 |
|-----|------|------|
| **ASP.NET Core** | 8.0+ | Web API 프레임워크 |
| **Entity Framework Core** | 8.0+ | ORM |
| **MediatR** | 12.0+ | CQRS 패턴 구현 |
| **FluentValidation** | 11.0+ | 입력 검증 |
| **AutoMapper** | 12.0+ | 객체 매핑 |
| **Serilog** | 3.0+ | 구조화된 로깅 |

---

## 🏗️ 아키텍처 원칙

### 클린 아키텍처 레이어

```
┌─────────────────────────────────────────┐
│              Presentation               │  ← Controllers, API Endpoints
├─────────────────────────────────────────┤
│              Application                │  ← Services, DTOs, Validators
├─────────────────────────────────────────┤
│                Domain                   │  ← Entities, Interfaces, Value Objects
├─────────────────────────────────────────┤
│             Infrastructure              │  ← Repositories, External Services
└─────────────────────────────────────────┘
```

### 설계 패턴

1. **CQRS (Command Query Responsibility Segregation)**: MediatR를 사용한 명령/조회 분리
2. **Repository Pattern**: 데이터 접근 추상화
3. **Unit of Work**: 트랜잭션 관리
4. **Dependency Injection**: ASP.NET Core 기본 DI 컨테이너 사용

---

## 💻 코딩 컨벤션

### 네이밍 규칙

```csharp
// ✅ 올바른 예시

// 클래스명: PascalCase
public class PostService { }
public class CommentController { }

// 인터페이스: 'I' 접두사 + PascalCase
public interface IPostRepository { }
public interface IFileService { }

// 메서드명: PascalCase, 동사로 시작
public async Task<Post> GetByIdAsync(long id) { }
public async Task<bool> CreateAsync(Post post) { }
public async Task DeleteAsync(long id) { }

// 비동기 메서드: Async 접미사
public async Task<IEnumerable<Post>> GetAllAsync() { }

// 프로퍼티: PascalCase
public string Title { get; set; }
public DateTime CreatedAt { get; set; }

// 필드: _camelCase (private), camelCase (local)
private readonly IPostRepository _postRepository;
private int _retryCount;

// 상수: PascalCase 또는 UPPER_SNAKE_CASE
public const int MaxPageSize = 100;
public const string DEFAULT_CATEGORY = "general";

// Enum: PascalCase
public enum PostStatus { Draft, Published, Archived, Deleted }
```

### 코드 스타일

```csharp
// ✅ 선호하는 스타일

// 1. 널 체크는 패턴 매칭 사용
if (post is null)
    throw new ArgumentNullException(nameof(post));

// 2. 문자열 보간 사용 (string concatenation 대신)
var message = $"Post {post.Id} created by {post.AuthorId}";

// 3. LINQ 메서드 체인 사용
var activePosts = posts
    .Where(p => p.Status == PostStatus.Published)
    .OrderByDescending(p => p.CreatedAt)
    .Take(10)
    .ToList();

// 4. 삼항 연산자는 간단한 경우에만 사용
var status = isPublished ? PostStatus.Published : PostStatus.Draft;

// 5. using 선언 사용 (using 블록 대신)
using var stream = new FileStream(path, FileMode.Open);

// 6. 타겟 타입 new 표현식 사용
List<Post> posts = new();
Dictionary<string, object> properties = new();

// 7. 널 병합 연산자 활용
var title = post.Title ?? "제목 없음";
post.UpdatedAt ??= DateTime.UtcNow;
```

### 비동기 프로그래밍

```csharp
// ✅ 올바른 비동기 패턴

// 1. async/await 일관되게 사용
public async Task<Post> GetPostAsync(long id)
{
    var post = await _repository.GetByIdAsync(id);
    return post ?? throw new NotFoundException($"Post {id} not found");
}

// 2. ASP.NET Core에서는 ConfigureAwait(false) 불필요
// (SynchronizationContext가 없으므로 성능 이점 없음)
public async Task<IEnumerable<Post>> GetAllAsync()
{
    return await _repository.GetAllAsync();
}

// 3. CancellationToken 지원
public async Task<Post> CreateAsync(Post post, CancellationToken cancellationToken = default)
{
    await _repository.AddAsync(post, cancellationToken);
    return post;
}

// 4. Task.WhenAll로 병렬 실행
var tasks = new[]
{
    GetPostAsync(id1),
    GetPostAsync(id2),
    GetPostAsync(id3)
};
var posts = await Task.WhenAll(tasks);
```

---

## 🔌 API 설계 가이드라인

### RESTful 엔드포인트 규칙

```csharp
// ✅ 올바른 API 설계

[ApiController]
[Route("api/v1/[controller]")]
public class PostsController : ControllerBase
{
    // GET /api/v1/posts - 목록 조회
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = "createdAt",
        [FromQuery] string? order = "desc")
    
    // GET /api/v1/posts/{id} - 단일 조회
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PostDto>> GetById(long id)
    
    // POST /api/v1/posts - 생성
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest request)
    
    // PUT /api/v1/posts/{id} - 전체 수정
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PostDto>> Update(long id, [FromBody] UpdatePostRequest request)
    
    // PATCH /api/v1/posts/{id} - 부분 수정
    [HttpPatch("{id:long}")]
    public async Task<ActionResult<PostDto>> PartialUpdate(long id, [FromBody] PatchPostRequest request)
    
    // DELETE /api/v1/posts/{id} - 삭제
    [HttpDelete("{id:long}")]
    public async Task<ActionResult> Delete(long id)
    
    // POST /api/v1/posts/{id}/pin - 액션 (상단고정)
    [HttpPost("{id:long}/pin")]
    public async Task<ActionResult> Pin(long id)
    
    // DELETE /api/v1/posts/{id}/pin - 액션 취소
    [HttpDelete("{id:long}/pin")]
    public async Task<ActionResult> Unpin(long id)
}
```

### HTTP 상태 코드 사용

```csharp
// 성공 응답
return Ok(data);                    // 200 - 일반 성공
return Created(uri, data);          // 201 - 생성 성공
return NoContent();                 // 204 - 삭제/수정 성공

// 클라이언트 에러
return BadRequest(errors);          // 400 - 잘못된 요청
return Unauthorized();              // 401 - 인증 필요
return Forbid();                    // 403 - 권한 없음
return NotFound();                  // 404 - 리소스 없음
return Conflict(message);           // 409 - 충돌 (중복 등)
return UnprocessableEntity(errors); // 422 - 유효성 검증 실패

// 서버 에러
return StatusCode(500, message);    // 500 - 서버 내부 오류
```

### API 응답 형식

```csharp
// 성공 응답 구조
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public MetaData? Meta { get; set; }
}

// 페이징 메타데이터
public class MetaData
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

// 에러 응답 구조
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public ApiError Error { get; set; } = new();
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Details { get; set; }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

---

## 📊 데이터 모델 설계

### 엔티티 베이스 클래스

```csharp
// 모든 엔티티의 기본 인터페이스
public interface IEntity
{
    long Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

// 엔티티 기본 클래스
public abstract class EntityBase : IEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// 소프트 삭제 지원
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

// 동적 필드 확장 지원
public interface IHasExtendedProperties
{
    Dictionary<string, object>? ExtendedProperties { get; set; }
}
```

### 게시물 엔티티 예시

```csharp
// 게시물 필수 항목 인터페이스
public interface IPost : IEntity
{
    string Title { get; set; }
    string Content { get; set; }
    long AuthorId { get; set; }
    PostStatus Status { get; set; }
}

// 게시물 엔티티 구현
public class Post : EntityBase, IPost, ISoftDeletable, IHasExtendedProperties
{
    // 필수 항목
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public long AuthorId { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    
    // 선택적 항목
    [MaxLength(250)]
    public string? Slug { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // 소프트 삭제
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // 동적 확장 필드
    public Dictionary<string, object>? ExtendedProperties { get; set; }
    
    // 네비게이션 프로퍼티
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
```

### EF Core 설정

```csharp
public class BoardDbContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 소프트 삭제 글로벌 필터
        modelBuilder.Entity<Post>()
            .HasQueryFilter(p => !p.IsDeleted);
        
        // 인덱스 설정
        modelBuilder.Entity<Post>()
            .HasIndex(p => p.Status);
        
        modelBuilder.Entity<Post>()
            .HasIndex(p => p.CreatedAt);
        
        // JSON 컬럼 설정 (ExtendedProperties)
        modelBuilder.Entity<Post>()
            .Property(p => p.ExtendedProperties)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null));
    }
}
```

---

## 🔐 보안 가이드라인

### 인증 (JWT)

```csharp
// JWT 설정
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
        };
    });
```

### 권한 (RBAC)

```csharp
// 역할 정의
public static class Roles
{
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";
    public const string User = "User";
}

// 정책 기반 권한
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditPost", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "post:edit") ||
            context.User.IsInRole(Roles.Admin)));
    
    options.AddPolicy("CanDeletePost", policy =>
        policy.RequireRole(Roles.Admin, Roles.Moderator));
    
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Roles.Admin));
});

// 컨트롤러에서 사용
[Authorize(Policy = "CanEditPost")]
public async Task<ActionResult> UpdatePost(long id, UpdatePostRequest request)

[Authorize(Roles = "Admin,Moderator")]
public async Task<ActionResult> DeletePost(long id)
```

### 입력 검증 (FluentValidation)

```csharp
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("제목은 필수입니다.")
            .MaximumLength(200).WithMessage("제목은 200자 이내여야 합니다.");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("본문은 필수입니다.");
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}
```

### 보안 헤더 및 CSRF/XSS 방어

```csharp
// 보안 미들웨어 설정
app.UseHsts();
app.UseHttpsRedirection();

// CSP 헤더
app.Use(async (context, next) =>
{
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");
    
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    await next();
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});
```

### 파일 업로드 보안

```csharp
public class FileValidationService
{
    private readonly HashSet<string> _allowedExtensions = new()
    {
        ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"
    };
    
    private readonly Dictionary<string, byte[]> _fileSignatures = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46 } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
    };
    
    public bool ValidateFile(IFormFile file, long maxSize = 10 * 1024 * 1024)
    {
        // 1. 파일 크기 검증
        if (file.Length > maxSize)
            return false;
        
        // 2. 확장자 검증
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;
        
        // 3. 파일 시그니처 검증
        if (_fileSignatures.TryGetValue(extension, out var signature))
        {
            using var reader = file.OpenReadStream();
            var headerBytes = new byte[signature.Length];
            reader.Read(headerBytes, 0, signature.Length);
            
            if (!headerBytes.Take(signature.Length).SequenceEqual(signature))
                return false;
        }
        
        return true;
    }
}
```

---

## 🧪 테스트 가이드라인

### 단위 테스트

```csharp
public class PostServiceTests
{
    private readonly Mock<IPostRepository> _mockRepository;
    private readonly PostService _service;
    
    public PostServiceTests()
    {
        _mockRepository = new Mock<IPostRepository>();
        _service = new PostService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingPost_ReturnsPost()
    {
        // Arrange
        var expectedPost = new Post { Id = 1, Title = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(expectedPost);
        
        // Act
        var result = await _service.GetByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPost.Id, result.Id);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingPost_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Post?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByIdAsync(999));
    }
}
```

### 통합 테스트

```csharp
public class PostsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public PostsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetPosts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/posts");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", 
            response.Content.Headers.ContentType?.MediaType);
    }
    
    [Fact]
    public async Task CreatePost_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new { Title = "Test Post", Content = "Test Content" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/v1/posts", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### 테스트 명명 규칙

```csharp
// 패턴: [테스트대상]_[시나리오]_[예상결과]
public void CreatePost_WithValidData_ReturnsSuccess()
public void CreatePost_WithEmptyTitle_ThrowsValidationException()
public void GetPost_WithNonExistingId_ThrowsNotFoundException()
public void UpdatePost_WithoutPermission_ThrowsForbiddenException()
```

---

## 📁 프로젝트 구조

```
BoardCommonLibrary/
├── src/
│   ├── BoardCommonLibrary/              # 메인 라이브러리
│   │   ├── Controllers/                 # API 컨트롤러
│   │   │   └── Base/                    # 베이스 컨트롤러
│   │   ├── Services/                    # 비즈니스 로직
│   │   │   └── Base/                    # 베이스 서비스
│   │   ├── Repositories/                # 데이터 접근
│   │   ├── Entities/                    # 도메인 엔티티
│   │   │   └── Base/                    # 베이스 엔티티
│   │   ├── Interfaces/                  # 인터페이스
│   │   ├── DTOs/                        # 데이터 전송 객체
│   │   ├── Validators/                  # 유효성 검증
│   │   ├── Extensions/                  # 확장 메서드
│   │   └── Configuration/               # 설정 클래스
│   └── BoardCommonLibrary.Abstractions/ # 인터페이스/추상화
├── tests/
│   ├── BoardCommonLibrary.UnitTests/    # 단위 테스트
│   └── BoardCommonLibrary.IntegrationTests/ # 통합 테스트
├── test-web/
│   └── BoardTestWeb/                    # 테스트 웹서비스
└── docs/                                # 문서
```

---

## 🔄 주요 기능 구현 가이드

### 게시물 CRUD

```csharp
// 게시물 서비스 인터페이스
public interface IPostService
{
    Task<PagedResult<PostDto>> GetAllAsync(PostQueryParameters parameters);
    Task<PostDto> GetByIdAsync(long id);
    Task<PostDto> CreateAsync(CreatePostRequest request, long authorId);
    Task<PostDto> UpdateAsync(long id, UpdatePostRequest request, long userId);
    Task DeleteAsync(long id, long userId);
    Task<PostDto> PinAsync(long id);
    Task<PostDto> UnpinAsync(long id);
}
```

### 댓글 처리

```csharp
// 댓글 서비스 (대댓글 포함)
public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByPostIdAsync(long postId);
    Task<CommentDto> CreateAsync(long postId, CreateCommentRequest request, long authorId);
    Task<CommentDto> CreateReplyAsync(long parentCommentId, CreateCommentRequest request, long authorId);
    Task<CommentDto> UpdateAsync(long id, UpdateCommentRequest request, long userId);
    Task DeleteAsync(long id, long userId);
}
```

### 좋아요/북마크

```csharp
// 사용자 활동 서비스
public interface IUserActivityService
{
    Task<bool> ToggleLikeAsync(long postId, long userId);
    Task<bool> ToggleBookmarkAsync(long postId, long userId);
    Task<PagedResult<PostDto>> GetBookmarksAsync(long userId, PaginationParameters parameters);
}
```

### 파일 업로드

```csharp
// 파일 서비스
public interface IFileService
{
    Task<FileDto> UploadAsync(IFormFile file, long uploaderId);
    Task<Stream> DownloadAsync(long fileId);
    Task<Stream> GetThumbnailAsync(long fileId);
    Task DeleteAsync(long fileId, long userId);
}
```

### 검색

```csharp
// 검색 서비스
public interface ISearchService
{
    Task<SearchResult<PostDto>> SearchPostsAsync(SearchParameters parameters);
    Task<IEnumerable<TagDto>> SearchTagsAsync(string query);
    Task<SearchResult<QuestionDto>> SearchQuestionsAsync(SearchParameters parameters);
}
```

### Q&A 게시판

```csharp
// Q&A 서비스
public interface IQnAService
{
    // 질문
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionRequest request, long authorId);
    Task<QuestionDto> GetQuestionByIdAsync(long id);
    Task CloseQuestionAsync(long id, long userId);
    
    // 답변
    Task<AnswerDto> CreateAnswerAsync(long questionId, CreateAnswerRequest request, long authorId);
    Task<AnswerDto> AcceptAnswerAsync(long answerId, long userId);
    Task<AnswerDto> VoteAnswerAsync(long answerId, long userId, bool isUpvote);
}
```

---

## ⚠️ 주의사항

### 하지 말아야 할 것

```csharp
// ❌ 잘못된 예시

// 1. 동기 메서드에서 .Result 또는 .Wait() 사용
var post = GetPostAsync(id).Result;  // 데드락 위험!

// 2. 예외를 삼키지 않기
catch (Exception) { }  // 로그 없이 무시

// 3. 하드코딩된 설정값
var connectionString = "Server=localhost;...";  // 설정 파일 사용

// 4. SQL 직접 조합
var query = $"SELECT * FROM Posts WHERE Id = {id}";  // SQL Injection 위험

// 5. 비밀번호 평문 저장
user.Password = request.Password;  // 해시 필수
```

### 해야 할 것

```csharp
// ✅ 올바른 예시

// 1. 비동기 메서드 올바르게 사용
var post = await GetPostAsync(id);

// 2. 예외 처리 및 로깅
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to get post {PostId}", id);
    throw;
}

// 3. 설정 파일 사용
var connectionString = _configuration.GetConnectionString("DefaultConnection");

// 4. 파라미터화된 쿼리 (EF Core 기본 지원)
var post = await _context.Posts.FindAsync(id);

// 5. 비밀번호 해시
user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
```

---

## 📚 참고 문서

- [PRD (제품 요구사항 문서)](../docs/PRD.md)
- [페이지별 기능 명세](../docs/PAGES.md)
- [NuGet 배포 가이드](../docs/NUGET.md)
- [테스트 가이드](../docs/TESTING.md)
- [ASP.NET Core 문서](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core 문서](https://learn.microsoft.com/en-us/ef/core/)

---

*이 지침서는 프로젝트 진행에 따라 지속적으로 업데이트됩니다.*
*최종 업데이트: 2025-11-27*
