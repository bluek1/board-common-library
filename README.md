# Board Common Library

범용 게시판 공통 라이브러리 - ASP.NET Core 기반의 재사용 가능한 게시판 API 라이브러리

## 📋 개요

**Board Common Library**는 다양한 프로젝트에서 쉽게 통합하여 사용할 수 있는 범용 게시판 기능을 제공하는 ASP.NET Core 기반 라이브러리입니다.

## ✨ 주요 기능

### 핵심 API 기능 (MVP)
- **게시물 CRUD**: 작성/수정/삭제/조회, 조회수 관리, 임시저장, 상단고정(공지)
- **목록 조회**: 페이징, 정렬, 필터링(카테고리/태그)
- **검색**: 제목/본문/태그 검색
- **파일 첨부**: 파일 업로드/썸네일 생성, 업로드 제한(용량/확장자), CDN 연동
- **댓글/대댓글**: 댓글 CRUD, 대댓글 지원
- **사용자 활동**: 좋아요, 북마크, 신고/블라인드

### 인증·권한·보안
- **인증**: JWT/OAuth 기반 인증
- **권한**: 역할 기반 접근 제어(RBAC), ACL 엔드포인트 보호
- **보안**: CSRF/XSS 방어, 입력 검증, 파일 스캔

### 운영·관리·확장
- **관리자 기능**: 콘텐츠 관리, 통계, 일괄처리 대시보드
- **운영**: 로그/감사, 백업/복원, 배치 작업(자동삭제 등)
- **확장성**: 플러그인 아키텍처, 이벤트 시스템

## 🛠️ 권장 기술 스택

| 기술 | 용도 |
|-----|------|
| **ASP.NET Core 8.0+** | Web API 프레임워크 |
| **Entity Framework Core 8.0+** | ORM |
| **FluentValidation 11.0+** | 입력 검증 |
| **JWT/OAuth** | 인증 |
| **SQL Server / PostgreSQL / MySQL** | 데이터베이스 |
| **Redis** | 캐싱 (선택적) |
| **SignalR** | 실시간 알림 (선택적) |

## 📖 문서

- [제품 요구사항 문서 (PRD)](docs/PRD.md) - 상세 기능 명세 및 API 설계
- [NuGet 배포 가이드](docs/NUGET.md) - 패키지 설치 및 배포 가이드
- [페이지별 기능 명세](docs/PAGES.md) - 4페이지 구성 및 테스트 케이스
- [테스트 가이드](docs/TESTING.md) - 테스트 웹서비스 사용 가이드
- [페이지 1 작업 명세](docs/page1task.md) - 게시물 관리 상세 작업 명세

## 🚀 시작하기

### NuGet 패키지 설치

```bash
# .NET CLI
dotnet add package BoardCommonLibrary --version 1.0.0

# 또는 패키지 관리자
Install-Package BoardCommonLibrary -Version 1.0.0
```

### 프로젝트에서 사용하기

```csharp
// Program.cs
using BoardCommonLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 게시판 라이브러리 서비스 등록 (SQL Server)
builder.Services.AddBoardLibrary(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 또는 InMemory DB 사용 (개발/테스트용)
builder.Services.AddBoardLibraryInMemory();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### 저장소 클론 및 빌드

```bash
# 저장소 클론
git clone https://github.com/bluek1/board-common-library.git

# 프로젝트 디렉토리로 이동
cd board-common-library

# 라이브러리 빌드
cd src/BoardCommonLibrary
dotnet build
```

### 테스트 웹서비스 실행

```bash
# 테스트 웹서비스 디렉토리로 이동
cd test-web/BoardTestWeb

# 의존성 복원 및 실행
dotnet restore
dotnet run

# 웹 브라우저에서 http://localhost:5000 접속
# Swagger UI: http://localhost:5000/swagger
```

## 📡 API 사용 예제

### 게시물 작성

```bash
# curl
curl -X POST http://localhost:5000/api/posts \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -H "X-User-Name: testuser" \
  -d '{
    "title": "첫 번째 게시물",
    "content": "게시물 내용입니다.",
    "category": "일반",
    "tags": ["공지", "테스트"]
  }'
```

```powershell
# PowerShell
$body = @{
    title = "첫 번째 게시물"
    content = "게시물 내용입니다."
    category = "일반"
    tags = @("공지", "테스트")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/posts" `
    -Method POST `
    -ContentType "application/json" `
    -Headers @{ "X-User-Id" = "1"; "X-User-Name" = "testuser" } `
    -Body $body
```

### 게시물 목록 조회

```bash
# curl - 페이징, 정렬, 필터링
curl "http://localhost:5000/api/posts?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc&category=일반"
```

```powershell
# PowerShell
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/posts?page=1&pageSize=10" -Method GET
$response | ConvertTo-Json -Depth 10
```

### 게시물 상세 조회

```bash
# curl
curl http://localhost:5000/api/posts/1 \
  -H "X-User-Id: 1"
```

```powershell
# PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/api/posts/1" `
    -Method GET `
    -Headers @{ "X-User-Id" = "1" }
```

### 게시물 수정

```bash
# curl
curl -X PUT http://localhost:5000/api/posts/1 \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{
    "title": "수정된 제목",
    "content": "수정된 내용입니다."
  }'
```

### 게시물 삭제 (소프트 삭제)

```bash
# curl
curl -X DELETE http://localhost:5000/api/posts/1 \
  -H "X-User-Id: 1"
```

### 상단고정 설정/해제 (관리자 전용)

```bash
# 상단고정 설정
curl -X POST http://localhost:5000/api/posts/1/pin \
  -H "X-User-Id: 1" \
  -H "X-User-Role: Admin"

# 상단고정 해제
curl -X DELETE http://localhost:5000/api/posts/1/pin \
  -H "X-User-Id: 1" \
  -H "X-User-Role: Admin"
```

### 임시저장

```bash
# 임시저장 생성
curl -X POST http://localhost:5000/api/posts/draft \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -H "X-User-Name: testuser" \
  -d '{
    "title": "임시 제목",
    "content": "작성 중인 내용..."
  }'

# 임시저장 목록 조회
curl http://localhost:5000/api/posts/draft \
  -H "X-User-Id: 1"

# 임시저장 발행
curl -X POST http://localhost:5000/api/posts/1/publish \
  -H "X-User-Id: 1"
```

## 🔐 인증 헤더

현재 버전에서는 간단한 헤더 기반 인증을 사용합니다:

| 헤더 | 설명 | 필수 여부 |
|-----|------|----------|
| `X-User-Id` | 사용자 ID (long) | 쓰기 작업 시 필수 |
| `X-User-Name` | 사용자명 | 게시물 작성 시 필수 |
| `X-User-Role` | 사용자 역할 (Admin, Moderator, User) | 관리자 기능 시 필수 |

## 📊 페이지 구성

본 라이브러리는 4개의 페이지로 기능이 구성되어 있습니다:

| 페이지 | 기능 | 테스트 수 |
|-------|------|----------|
| **페이지 1** | 게시물 관리 (CRUD, 조회수, 상단고정, 임시저장) | 15개 |
| **페이지 2** | 댓글/대댓글, 좋아요, 북마크 | 15개 |
| **페이지 3** | 파일 업로드, 썸네일, 검색 | 15개 |
| **페이지 4** | 관리자 기능, Q&A 게시판 | 15개 |

각 페이지별 상세 기능과 테스트 케이스는 [페이지별 기능 명세](docs/PAGES.md)를 참조하세요.

## 📄 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE)를 따릅니다.

## 📬 기여하기

프로젝트 기여에 관심이 있으시다면 Issues나 Pull Request를 통해 참여해 주세요!
