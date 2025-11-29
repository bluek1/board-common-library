# 데모 프로그램 가이드

## 📖 개요

이 문서는 **Board Common Library** 데모 프로그램의 아키텍처, 구현 방법, 그리고 라이브러리 연동 방법을 상세히 설명합니다.

데모 프로그램은 다음 두 부분으로 구성됩니다:

| 구성요소 | 기술 스택 | 포트 | 설명 |
|---------|----------|------|------|
| **백엔드 API** | ASP.NET Core 8.0 | 5117 | Board Common Library를 사용한 REST API 서버 |
| **프론트엔드** | React 19 + TypeScript + Vite | 5173 | 게시판 UI 데모 |

---

## 🏗️ 전체 아키텍처

```
┌─────────────────────────────────────────────────────────────────┐
│                        프론트엔드 (React)                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │
│  │  Pages   │ │Components│ │ Contexts │ │   API    │           │
│  │ (페이지) │ │ (컴포넌트)│ │ (상태관리)│ │ (클라이언트)│           │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘           │
│       └────────────┴────────────┴────────────┘                  │
│                              │ HTTP/REST                        │
└──────────────────────────────┼──────────────────────────────────┘
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│                      백엔드 API (ASP.NET Core)                    │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                    Board Common Library                     │  │
│  │  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐  │  │
│  │  │Controllers│ │ Services  │ │Repositories│ │ Entities  │  │  │
│  │  └─────┬─────┘ └─────┬─────┘ └─────┬─────┘ └─────┬─────┘  │  │
│  │        └─────────────┴─────────────┴─────────────┘         │  │
│  └────────────────────────────────────────────────────────────┘  │
│                              │                                    │
│                              ▼                                    │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │              Entity Framework Core (InMemory DB)            │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 📁 프로젝트 구조

### 백엔드 (BoardDemo.Api)

```
demo/BoardDemo.Api/
├── BoardDemo.Api.csproj          # 프로젝트 파일 (라이브러리 참조)
├── Program.cs                    # 애플리케이션 진입점 및 설정
├── appsettings.json              # 애플리케이션 설정
├── Controllers/
│   └── AuthController.cs         # 인증 API (로그인, 회원가입, 토큰 갱신)
├── Models/
│   ├── AuthModels.cs             # 인증 관련 요청/응답 모델
│   └── TokenDto.cs               # JWT 토큰 DTO
├── Services/
│   ├── JwtService.cs             # JWT 토큰 생성 및 검증
│   └── AuthService.cs            # 인증 비즈니스 로직
└── Data/
    └── SeedData.cs               # 초기 테스트 데이터 생성
```

### 프론트엔드 (board-demo-web)

```
demo/board-demo-web/
├── package.json                  # 의존성 및 스크립트
├── vite.config.ts                # Vite 빌드 설정
├── tailwind.config.js            # TailwindCSS 설정
├── tsconfig.json                 # TypeScript 설정
└── src/
    ├── main.tsx                  # 애플리케이션 진입점
    ├── App.tsx                   # 라우터 및 레이아웃
    ├── index.css                 # 전역 스타일
    ├── api/                      # API 클라이언트
    │   ├── client.ts             # Axios 인스턴스 (인터셉터 포함)
    │   ├── auth.ts               # 인증 API
    │   ├── posts.ts              # 게시물 API
    │   ├── comments.ts           # 댓글 API
    │   ├── questions.ts          # Q&A API
    │   └── index.ts              # API 모듈 내보내기
    ├── components/               # 재사용 컴포넌트
    │   ├── Layout.tsx            # 공통 레이아웃
    │   ├── Header.tsx            # 헤더 네비게이션
    │   └── ProtectedRoute.tsx    # 인증 보호 라우트
    ├── contexts/                 # React Context
    │   └── AuthContext.tsx       # 인증 상태 관리
    ├── pages/                    # 페이지 컴포넌트
    │   ├── index.ts              # 페이지 내보내기
    │   ├── HomePage.tsx          # 홈페이지
    │   ├── LoginPage.tsx         # 로그인
    │   ├── RegisterPage.tsx      # 회원가입
    │   ├── PostListPage.tsx      # 게시물 목록
    │   ├── PostDetailPage.tsx    # 게시물 상세
    │   ├── PostCreatePage.tsx    # 게시물 작성
    │   ├── PostEditPage.tsx      # 게시물 수정
    │   ├── QuestionListPage.tsx  # Q&A 목록
    │   ├── QuestionDetailPage.tsx# Q&A 상세
    │   ├── QuestionCreatePage.tsx# 질문 작성
    │   ├── AdminDashboardPage.tsx# 관리자 대시보드
    │   ├── AdminPostsPage.tsx    # 게시물 관리
    │   └── AdminUsersPage.tsx    # 사용자 관리
    └── types/                    # TypeScript 타입 정의
        └── index.ts              # 전체 타입
```

---

## 🔌 라이브러리 연동 방법

### 1. NuGet 패키지 참조

`BoardDemo.Api.csproj`에서 Board Common Library를 프로젝트 참조로 연결합니다:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Board Common Library 프로젝트 참조 -->
  <ItemGroup>
    <ProjectReference Include="..\..\src\BoardCommonLibrary\BoardCommonLibrary.csproj" />
  </ItemGroup>

  <!-- 추가 패키지 -->
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

### 2. Program.cs 설정

`Program.cs`에서 라이브러리 서비스를 등록하고 설정합니다:

```csharp
using BoardCommonLibrary;
using BoardCommonLibrary.Data;
using BoardDemo.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Board Common Library DbContext 등록
// ============================================
builder.Services.AddDbContext<BoardDbContext>(options =>
{
    // InMemory 데이터베이스 사용 (개발/테스트 환경)
    options.UseInMemoryDatabase("BoardDemoDb");
    
    // 실제 운영 환경에서는 SQL Server 사용
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ============================================
// 2. Board Common Library 서비스 등록
// ============================================
// 라이브러리에서 제공하는 확장 메서드로 모든 서비스 자동 등록
builder.Services.AddBoardLibraryServices();

// ============================================
// 3. JWT 인증 설정
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourDefaultSecretKey_AtLeast32Characters!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// ============================================
// 4. CORS 설정 (프론트엔드 연동용)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// 5. 컨트롤러 및 Swagger 등록
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 데모용 서비스 등록
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// ============================================
// 6. 데이터베이스 초기화 및 시드 데이터
// ============================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BoardDbContext>();
    context.Database.EnsureCreated();
    
    // 테스트 데이터 생성
    SeedData.Initialize(context);
}

// ============================================
// 7. 미들웨어 파이프라인 구성
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 3. 라이브러리 서비스 확장 메서드

`BoardCommonLibrary`에서 제공하는 `AddBoardLibraryServices()` 확장 메서드는 다음 서비스들을 자동으로 등록합니다:

```csharp
// BoardCommonLibrary/Extensions/ServiceCollectionExtensions.cs

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoardLibraryServices(this IServiceCollection services)
    {
        // Repository 등록
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        
        // Service 등록
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IAnswerService, AnswerService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ISearchService, SearchService>();
        
        return services;
    }
}
```

---

## 📡 API 엔드포인트 상세

### 인증 API (데모 전용)

| Method | Endpoint | 설명 | 인증 |
|--------|----------|------|------|
| POST | `/api/auth/login` | 로그인 | ❌ |
| POST | `/api/auth/register` | 회원가입 | ❌ |
| POST | `/api/auth/refresh` | 토큰 갱신 | ❌ |
| GET | `/api/auth/me` | 현재 사용자 정보 | ✅ |

### 게시물 API (라이브러리 제공)

| Method | Endpoint | 설명 | 인증 |
|--------|----------|------|------|
| GET | `/api/posts` | 게시물 목록 조회 | ❌ |
| GET | `/api/posts/{id}` | 게시물 상세 조회 | ❌ |
| POST | `/api/posts` | 게시물 작성 | ✅ |
| PUT | `/api/posts/{id}` | 게시물 수정 | ✅ |
| DELETE | `/api/posts/{id}` | 게시물 삭제 | ✅ |
| POST | `/api/posts/{id}/like` | 좋아요 토글 | ✅ |
| POST | `/api/posts/{id}/pin` | 상단고정 | ✅ (Admin) |
| DELETE | `/api/posts/{id}/pin` | 상단고정 해제 | ✅ (Admin) |

### 댓글 API (라이브러리 제공)

| Method | Endpoint | 설명 | 인증 |
|--------|----------|------|------|
| GET | `/api/posts/{postId}/comments` | 댓글 목록 | ❌ |
| POST | `/api/posts/{postId}/comments` | 댓글 작성 | ✅ |
| PUT | `/api/comments/{id}` | 댓글 수정 | ✅ |
| DELETE | `/api/comments/{id}` | 댓글 삭제 | ✅ |
| POST | `/api/comments/{id}/replies` | 대댓글 작성 | ✅ |

### Q&A API (라이브러리 제공)

| Method | Endpoint | 설명 | 인증 |
|--------|----------|------|------|
| GET | `/api/questions` | 질문 목록 | ❌ |
| GET | `/api/questions/{id}` | 질문 상세 | ❌ |
| POST | `/api/questions` | 질문 작성 | ✅ |
| PUT | `/api/questions/{id}` | 질문 수정 | ✅ |
| DELETE | `/api/questions/{id}` | 질문 삭제 | ✅ |
| POST | `/api/questions/{id}/vote` | 질문 추천 | ✅ |
| GET | `/api/questions/{id}/answers` | 답변 목록 | ❌ |
| POST | `/api/questions/{id}/answers` | 답변 작성 | ✅ |
| PUT | `/api/answers/{id}` | 답변 수정 | ✅ |
| DELETE | `/api/answers/{id}` | 답변 삭제 | ✅ |
| POST | `/api/answers/{id}/accept` | 답변 채택 | ✅ |
| POST | `/api/answers/{id}/vote` | 답변 추천 | ✅ |

---

## 🔐 인증 흐름

### JWT 토큰 기반 인증

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   클라이언트  │                    │   백엔드 API │                    │  데이터베이스 │
└──────┬──────┘                    └──────┬──────┘                    └──────┬──────┘
       │                                  │                                  │
       │ 1. POST /api/auth/login          │                                  │
       │ { email, password }              │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │ 2. 사용자 조회                    │
       │                                  │─────────────────────────────────>│
       │                                  │<─────────────────────────────────│
       │                                  │                                  │
       │                                  │ 3. 비밀번호 검증                  │
       │                                  │ 4. JWT 토큰 생성                  │
       │                                  │   - Access Token (60분)          │
       │                                  │   - Refresh Token (7일)          │
       │                                  │                                  │
       │ 5. 토큰 반환                      │                                  │
       │ { accessToken, refreshToken }    │                                  │
       │<─────────────────────────────────│                                  │
       │                                  │                                  │
       │ 6. API 요청 (인증 필요)            │                                  │
       │ Authorization: Bearer {token}    │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │ 7. 토큰 검증                      │
       │                                  │ 8. 요청 처리                      │
       │<─────────────────────────────────│                                  │
       │                                  │                                  │
```

### JwtService 구현

```csharp
// Services/JwtService.cs

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName ?? user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
            signingCredentials: new SigningCredentials(
                secretKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

---

## 🖥️ 프론트엔드 구현 상세

### 1. API 클라이언트 설정

Axios를 사용하여 백엔드 API와 통신합니다. 인터셉터를 통해 JWT 토큰 자동 첨부 및 토큰 갱신을 처리합니다.

```typescript
// src/api/client.ts

import axios from 'axios';

const API_BASE_URL = 'http://localhost:5117/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// 요청 인터셉터: JWT 토큰 자동 첨부
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 응답 인터셉터: 401 에러 시 토큰 갱신 시도
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
          refreshToken,
        });
        
        const { accessToken } = response.data.tokens;
        localStorage.setItem('accessToken', accessToken);
        
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        // 토큰 갱신 실패 시 로그아웃
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;
```

### 2. API 응답 형식 변환

백엔드 API는 `{ success, data, meta }` 형식으로 응답합니다. 프론트엔드에서 이를 처리합니다:

```typescript
// src/api/posts.ts

// API 응답 래퍼 타입
interface ApiPagedResponse<T> {
  success: boolean;
  data: T[];
  meta: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

interface ApiSingleResponse<T> {
  success: boolean;
  data: T;
}

export const postsApi = {
  // 게시물 목록 조회
  getAll: async (params?: QueryParams): Promise<PagedResult<Post>> => {
    const response = await api.get<ApiPagedResponse<Post>>('/posts', { params });
    const { data, meta } = response.data;
    return {
      items: data || [],
      page: meta.page,
      pageSize: meta.pageSize,
      totalCount: meta.totalCount,
      totalPages: meta.totalPages,
    };
  },

  // 게시물 상세 조회
  getById: async (id: number): Promise<Post> => {
    const response = await api.get<ApiSingleResponse<Post>>(`/posts/${id}`);
    return response.data.data;
  },
  
  // ... 기타 API 메서드
};
```

### 3. 인증 상태 관리 (Context API)

React Context를 사용하여 전역 인증 상태를 관리합니다:

```typescript
// src/contexts/AuthContext.tsx

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // 페이지 로드 시 저장된 토큰으로 사용자 정보 복원
    const token = localStorage.getItem('accessToken');
    if (token) {
      authApi.getMe()
        .then(setUser)
        .catch(() => {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        })
        .finally(() => setIsLoading(false));
    } else {
      setIsLoading(false);
    }
  }, []);

  const login = async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    if (response.tokens) {
      localStorage.setItem('accessToken', response.tokens.accessToken);
      localStorage.setItem('refreshToken', response.tokens.refreshToken);
      setUser(response.user!);
    }
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
```

### 4. 보호된 라우트

인증이 필요한 페이지를 보호하는 컴포넌트:

```typescript
// src/components/ProtectedRoute.tsx

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <div className="flex items-center justify-center min-h-screen">로딩 중...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}
```

### 5. 관리자 전용 라우트

```typescript
// src/App.tsx

function AdminRoute({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  
  if (user?.role !== 'Admin') {
    return <Navigate to="/" replace />;
  }
  
  return <>{children}</>;
}

// 라우터 설정
<Route path="/admin" element={<AdminRoute><AdminDashboardPage /></AdminRoute>} />
<Route path="/admin/posts" element={<AdminRoute><AdminPostsPage /></AdminRoute>} />
```

---

## 📊 데이터 모델

### 라이브러리에서 제공하는 엔티티

#### Post (게시물)

```csharp
public class Post : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public long AuthorId { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Published;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsPinned { get; set; }
    
    // Navigation properties
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}
```

#### Comment (댓글)

```csharp
public class Comment : EntityBase
{
    public string Content { get; set; } = string.Empty;
    public long PostId { get; set; }
    public long AuthorId { get; set; }
    public long? ParentId { get; set; }  // 대댓글용
    public int LikeCount { get; set; }
    
    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual User Author { get; set; } = null!;
    public virtual Comment? Parent { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
```

#### Question (질문)

```csharp
public class Question : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public QuestionStatus Status { get; set; } = QuestionStatus.Open;
    public int ViewCount { get; set; }
    public int VoteCount { get; set; }
    public int AnswerCount { get; set; }
    public long? AcceptedAnswerId { get; set; }
    public string? Tags { get; set; }
    
    // Navigation properties
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
```

#### Answer (답변)

```csharp
public class Answer : EntityBase
{
    public string Content { get; set; } = string.Empty;
    public long QuestionId { get; set; }
    public long AuthorId { get; set; }
    public bool IsAccepted { get; set; }
    public int VoteCount { get; set; }
    
    // Navigation properties
    public virtual Question Question { get; set; } = null!;
    public virtual User Author { get; set; } = null!;
}
```

### TypeScript 타입 정의

```typescript
// src/types/index.ts

export interface Post {
  id: number;
  title: string;
  content: string;
  category?: string;
  tags: string[];
  authorId: number;
  authorName?: string;
  status: PostStatus;
  viewCount: number;
  likeCount: number;
  commentCount: number;
  isPinned: boolean;
  createdAt: string;
  updatedAt?: string;
}

export enum PostStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
  Deleted = 3,
}

export interface Question {
  id: number;
  title: string;
  content: string;
  authorId: number;
  authorName?: string;
  status: QuestionStatus;
  viewCount: number;
  voteCount: number;
  answerCount: number;
  acceptedAnswerId?: number;
  tags: string[];
  createdAt: string;
  updatedAt?: string;
}

export enum QuestionStatus {
  Open = 0,
  Answered = 1,
  Closed = 2,
}
```

---

## 🎨 UI/UX 구현

### TailwindCSS 스타일링

데모에서는 TailwindCSS를 사용하여 빠르고 일관된 UI를 구현합니다:

```typescript
// 버튼 스타일 예시
<button className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg 
                   hover:bg-blue-700 transition disabled:opacity-50">
  등록
</button>

// 카드 스타일 예시
<div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 
                hover:shadow-md transition">
  {/* 콘텐츠 */}
</div>

// 입력 필드 스타일 예시
<input className="w-full px-4 py-2 border border-gray-300 rounded-lg 
                  focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
```

### 반응형 레이아웃

```typescript
// 그리드 레이아웃 예시
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  {items.map(item => <Card key={item.id} {...item} />)}
</div>

// 컨테이너 예시
<div className="max-w-4xl mx-auto px-4 py-8">
  {/* 콘텐츠 */}
</div>
```

---

## 🚀 실행 방법

### 1. 백엔드 서버 실행

```bash
cd demo/BoardDemo.Api
dotnet run
```

서버가 `http://localhost:5117`에서 시작됩니다.  
Swagger UI: `http://localhost:5117/swagger`

### 2. 프론트엔드 개발 서버 실행

```bash
cd demo/board-demo-web
npm install
npm run dev
```

개발 서버가 `http://localhost:5173`에서 시작됩니다.

### 3. 테스트 계정

| 역할 | 이메일 | 비밀번호 |
|------|--------|----------|
| 관리자 | admin@test.com | Admin123! |
| 사용자1 | user1@test.com | User123! |
| 사용자2 | user2@test.com | User123! |
| 사용자3 | user3@test.com | User123! |
| 중재자 | moderator@test.com | Mod123! |

---

## 📝 시드 데이터

데모 실행 시 자동으로 생성되는 테스트 데이터:

| 데이터 유형 | 수량 | 설명 |
|------------|------|------|
| 사용자 | 5명 | 관리자 1명, 사용자 3명, 중재자 1명 |
| 게시물 | 50개 | 다양한 카테고리와 태그 |
| 댓글 | 137개 | 게시물당 0~5개 랜덤 |
| 질문 | 20개 | Q&A 게시판용 |
| 답변 | 57개 | 질문당 1~5개 랜덤 |

시드 데이터 생성 코드:

```csharp
// Data/SeedData.cs

public static void Initialize(BoardDbContext context)
{
    if (context.Users.Any()) return;

    // 1. 사용자 생성
    var users = new List<User>
    {
        new User { Username = "admin", Email = "admin@test.com", Role = "Admin", ... },
        new User { Username = "user1", Email = "user1@test.com", Role = "User", ... },
        // ...
    };
    context.Users.AddRange(users);
    
    // 2. 게시물 생성
    var posts = GeneratePosts(users, 50);
    context.Posts.AddRange(posts);
    
    // 3. 댓글 생성
    var comments = GenerateComments(posts, users);
    context.Comments.AddRange(comments);
    
    // 4. 질문/답변 생성
    var questions = GenerateQuestions(users, 20);
    context.Questions.AddRange(questions);
    
    var answers = GenerateAnswers(questions, users);
    context.Answers.AddRange(answers);
    
    context.SaveChanges();
}
```

---

## 🔧 설정 파일

### appsettings.json (백엔드)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "BoardDemo.Api",
    "Audience": "BoardDemo.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BoardDemo;Trusted_Connection=true;"
  }
}
```

### vite.config.ts (프론트엔드)

```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5117',
        changeOrigin: true,
      },
    },
  },
});
```

---

## 📚 추가 참고 문서

- [PRD (제품 요구사항 문서)](PRD.md)
- [페이지별 기능 명세](PAGES.md)
- [NuGet 배포 가이드](NUGET.md)
- [테스트 가이드](TESTING.md)
- [GitHub Copilot 개발 지침서](../.github/copilot-instructions.md)

---

## 🆘 문제 해결

### 자주 발생하는 문제

#### 1. CORS 오류

프론트엔드에서 API 호출 시 CORS 오류가 발생하면 백엔드의 CORS 설정을 확인합니다:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

#### 2. 401 Unauthorized 오류

- JWT 토큰이 만료되었는지 확인
- `Authorization` 헤더 형식 확인: `Bearer {token}`
- 토큰 갱신 로직이 정상 동작하는지 확인

#### 3. 데이터베이스 초기화

InMemory 데이터베이스는 서버 재시작 시 초기화됩니다. 영구 저장이 필요하면 SQL Server 등으로 변경합니다.

---

*최종 업데이트: 2025-11-29*
