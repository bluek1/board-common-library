# NuGet 패키지 배포 가이드

## 📦 패키지 정보

| 항목 | 내용 |
|-----|------|
| **패키지명** | `BoardCommonLibrary` |
| **현재 버전** | 1.0.0 |
| **타겟 프레임워크** | .NET 8.0+ |
| **라이선스** | MIT |
| **저장소** | https://github.com/bluek1/board-common-library |

## 🚀 설치 방법

### NuGet 패키지 관리자
```powershell
Install-Package BoardCommonLibrary -Version 1.0.0
```

### .NET CLI
```bash
dotnet add package BoardCommonLibrary --version 1.0.0
```

### PackageReference (프로젝트 파일)
```xml
<PackageReference Include="BoardCommonLibrary" Version="1.0.0" />
```

## ⚙️ 기본 설정

### 1. Program.cs 설정

```csharp
using BoardCommonLibrary;

var builder = WebApplication.CreateBuilder(args);

// 게시판 라이브러리 서비스 등록
builder.Services.AddBoardLibrary(options =>
{
    // 데이터베이스 연결 문자열 설정
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // API 경로 설정 (선택사항)
    options.ApiPrefix = "/api";
    options.ApiVersion = "v1";
    options.IncludeVersionInUrl = true;
    
    // JWT 인증 설정
    options.JwtSettings.SecretKey = builder.Configuration["Jwt:SecretKey"];
    options.JwtSettings.Issuer = builder.Configuration["Jwt:Issuer"];
    options.JwtSettings.Audience = builder.Configuration["Jwt:Audience"];
    options.JwtSettings.ExpirationMinutes = 60;
});

var app = builder.Build();

// 게시판 미들웨어 사용
app.UseBoardLibrary();

app.Run();
```

### 2. appsettings.json 설정

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BoardDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-here-minimum-32-characters",
    "Issuer": "BoardCommonLibrary",
    "Audience": "BoardCommonLibrary.Users"
  },
  "BoardLibrary": {
    "FileUpload": {
      "MaxFileSize": 10485760,
      "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"],
      "StoragePath": "uploads"
    },
    "Pagination": {
      "DefaultPageSize": 20,
      "MaxPageSize": 100
    }
  }
}
```

## 📋 데이터베이스 마이그레이션

### Entity Framework Core 마이그레이션 적용
```bash
# 마이그레이션 생성
dotnet ef migrations add InitialCreate

# 데이터베이스 업데이트
dotnet ef database update
```

### 스키마 직접 생성 (SQL Server)
```sql
-- 마이그레이션 대신 직접 SQL 스크립트 실행 시
-- 스크립트는 패키지 설치 후 자동 생성됨
dotnet sql-script -o BoardSchema.sql
```

## 🔧 고급 설정

### API 경로 커스터마이징
```csharp
builder.Services.AddBoardLibrary(options =>
{
    // 개별 리소스 경로 설정
    options.Routes.Posts = "articles";      // /api/v1/articles
    options.Routes.Comments = "replies";    // /api/v1/replies
    options.Routes.Questions = "qna";       // /api/v1/qna
    options.Routes.Files = "attachments";   // /api/v1/attachments
});
```

### 다중 게시판 경로 설정
```csharp
builder.Services.AddBoardLibrary(options =>
{
    options.BoardRoutes.Add("notice", new BoardRouteOptions
    {
        PostsRoute = "notices",
        CommentsRoute = "notice-comments"
    });
    
    options.BoardRoutes.Add("community", new BoardRouteOptions
    {
        PostsRoute = "community-posts",
        CommentsRoute = "community-comments"
    });
});
```

### 파일 스토리지 설정
```csharp
builder.Services.AddBoardLibrary(options =>
{
    // 로컬 스토리지
    options.FileStorage.UseLocalStorage("./uploads");
    
    // 또는 Azure Blob Storage
    options.FileStorage.UseAzureBlobStorage(
        connectionString: "your-azure-connection-string",
        containerName: "board-files"
    );
    
    // 또는 AWS S3
    options.FileStorage.UseAwsS3(
        accessKey: "your-access-key",
        secretKey: "your-secret-key",
        bucketName: "board-files",
        region: "ap-northeast-2"
    );
});
```

### 캐싱 설정
```csharp
builder.Services.AddBoardLibrary(options =>
{
    // 인메모리 캐시 (기본값)
    options.Caching.UseInMemory();
    
    // 또는 Redis 캐시
    options.Caching.UseRedis(
        connectionString: "localhost:6379",
        instanceName: "board-cache"
    );
});
```

## 📝 패키지 배포 절차

### 1. 버전 업데이트
```xml
<!-- BoardCommonLibrary.csproj -->
<PropertyGroup>
    <Version>1.0.0</Version>
    <PackageVersion>1.0.0</PackageVersion>
</PropertyGroup>
```

### 2. 패키지 빌드
```bash
# Release 모드로 빌드
dotnet build -c Release

# NuGet 패키지 생성
dotnet pack -c Release -o ./nupkgs
```

### 3. NuGet.org 배포
```bash
# API 키 설정 (최초 1회)
dotnet nuget setapikey <your-api-key> --source https://api.nuget.org/v3/index.json

# 패키지 푸시
dotnet nuget push ./nupkgs/BoardCommonLibrary.1.0.0.nupkg --source https://api.nuget.org/v3/index.json
```

### 4. 프라이빗 NuGet 서버 배포 (선택사항)
```bash
# Azure Artifacts
dotnet nuget push ./nupkgs/BoardCommonLibrary.1.0.0.nupkg --source "AzureArtifacts" --api-key az

# GitHub Packages
dotnet nuget push ./nupkgs/BoardCommonLibrary.1.0.0.nupkg --source "github"
```

## 🏷️ 버전 관리

### 시맨틱 버저닝 (Semantic Versioning)
- **Major**: 호환되지 않는 API 변경
- **Minor**: 하위 호환 기능 추가
- **Patch**: 하위 호환 버그 수정

### 버전 이력

| 버전 | 릴리스 날짜 | 변경 내용 |
|-----|-----------|----------|
| 1.0.0 | 예정 | 초기 릴리스 - MVP 기능 |
| 1.1.0 | 예정 | 임시저장, 신고/블라인드, 관리자 API |
| 1.2.0 | 예정 | 알림/구독, 이벤트 시스템, 배치 작업 |
| 2.0.0 | 예정 | 플러그인 아키텍처, OAuth, 실시간 알림 |

## 📚 추가 문서

- [제품 요구사항 문서 (PRD)](PRD.md) - 상세 기능 명세
- [페이지별 기능 명세](PAGES.md) - 4페이지 구성 가이드
- [테스트 가이드](TESTING.md) - 테스트 케이스 및 웹서비스

## 🆘 지원

- **이슈 트래커**: [GitHub Issues](https://github.com/bluek1/board-common-library/issues)
- **문서**: [Wiki](https://github.com/bluek1/board-common-library/wiki)

---

*최종 업데이트: 2025-11-27*
