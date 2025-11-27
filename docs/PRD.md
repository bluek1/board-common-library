# 게시판 공통 라이브러리 - 제품 요구사항 문서 (PRD)

## Product Requirements Document
### Board Common Library - ASP.NET Core 기반 범용 게시판 API 라이브러리

---

## 📋 목차

1. [개요](#1-개요)
2. [목표 및 핵심 가치](#2-목표-및-핵심-가치)
3. [권장 기술 스택](#3-권장-기술-스택)
4. [핵심 API 기능 (MVP)](#4-핵심-api-기능-mvp)
   - [Q&A 게시판](#47-qa-게시판-질문답변)
   - [템플릿 및 베이스 클래스](#48-템플릿-및-베이스-클래스)
5. [인증·권한·보안](#5-인증권한보안)
6. [운영·관리·확장](#6-운영관리확장)
7. [API 명세](#7-api-명세)
   - [API 경로 커스터마이징](#71-api-경로-커스터마이징)
8. [데이터 모델](#8-데이터-모델)
   - [동적 테이블 구조 및 인터페이스 기반 설계](#81-동적-테이블-구조-및-인터페이스-기반-설계)
9. [비기능 요구사항](#9-비기능-요구사항)
10. [릴리스 계획](#10-릴리스-계획)

---

## 1. 개요

### 1.1 프로젝트 소개

**게시판 공통 라이브러리**는 ASP.NET Core 기반의 재사용 가능한 게시판 API 라이브러리입니다. 다양한 프로젝트에서 쉽게 통합하여 사용할 수 있는 범용 게시판 기능을 제공합니다.

### 1.2 배경 및 필요성

- 프로젝트마다 반복되는 게시판 기능 개발 비용 절감
- 검증된 보안 패턴 및 모범 사례 적용
- 일관된 API 인터페이스 제공으로 개발 생산성 향상
- 플러그인 기반 확장 가능한 아키텍처 지원

### 1.3 대상 사용자

| 사용자 유형 | 설명 |
|------------|------|
| **개발자** | 라이브러리를 프로젝트에 통합하여 게시판 기능을 구현하는 개발자 |
| **관리자** | 게시판 콘텐츠 및 사용자를 관리하는 시스템 관리자 |
| **최종 사용자** | 게시판을 이용하여 콘텐츠를 생성하고 소비하는 사용자 |

---

## 2. 목표 및 핵심 가치

### 2.1 핵심 목표

1. **재사용성**: NuGet 패키지로 배포하여 다양한 ASP.NET Core 프로젝트에서 쉽게 사용
2. **확장성**: 플러그인 아키텍처를 통한 기능 확장 지원
3. **보안성**: OWASP 가이드라인을 준수한 보안 구현
4. **유지보수성**: 클린 아키텍처 원칙에 따른 코드 구조

### 2.2 핵심 기능 요약

| 카테고리 | 핵심 기능 |
|---------|----------|
| **CRUD** | 게시물 생성/조회/수정/삭제, 조회수, 임시저장, 상단고정 |
| **Q&A 게시판** | 질문/답변 CRUD, 답변 채택, 추천 시스템 |
| **권한·보안** | 역할 기반 권한, JWT/OAuth 인증, CSRF/XSS 방어 |
| **검색·페이징** | 제목/본문/태그 검색, 페이징, 정렬, 필터링 |
| **첨부·미디어** | 파일 업로드, 썸네일 생성, CDN 연동 |
| **댓글·대댓글** | 댓글 CRUD, 대댓글 지원, 좋아요/북마크 |
| **알림·구독** | 실시간 알림, 게시판/게시물 구독 |
| **관리자 기능** | 대시보드, 콘텐츠 관리, 통계, 일괄처리 |
| **확장성** | 플러그인 시스템, 커스텀 이벤트, 동적 필드 확장 |
| **템플릿·베이스 클래스** | 엔티티/서비스/컨트롤러 베이스 클래스 제공 |
| **API 커스터마이징** | 사용자 정의 API 경로 설정 지원 |
| **운영·모니터링** | 로그/감사, 백업/복원, 배치 작업 |

---

## 3. 권장 기술 스택

### 3.1 백엔드 프레임워크

| 기술 | 버전 | 용도 |
|-----|------|------|
| **ASP.NET Core** | 8.0+ | Web API 프레임워크 |
| **Entity Framework Core** | 8.0+ | ORM (Object-Relational Mapping) |
| **MediatR** | 12.0+ | CQRS 패턴 구현 |
| **FluentValidation** | 11.0+ | 입력 검증 |
| **AutoMapper** | 12.0+ | 객체 매핑 |
| **Serilog** | 3.0+ | 구조화된 로깅 |

### 3.2 인증 및 보안

| 기술 | 용도 |
|-----|------|
| **JWT (JSON Web Token)** | API 인증 토큰 |
| **OAuth 2.0 / OpenID Connect** | 외부 인증 제공자 통합 |
| **ASP.NET Core Identity** | 사용자 관리 (선택적) |
| **Data Protection API** | 데이터 암호화 |

### 3.3 데이터베이스

| 데이터베이스 | 지원 수준 |
|-------------|----------|
| **SQL Server** | 기본 지원 |
| **PostgreSQL** | 기본 지원 |
| **MySQL/MariaDB** | 기본 지원 |
| **SQLite** | 개발/테스트 용도 |

### 3.4 추가 인프라

| 기술 | 용도 |
|-----|------|
| **Redis** | 캐싱, 세션 관리 (선택적) |
| **Azure Blob Storage / AWS S3** | 파일 스토리지 |
| **SignalR** | 실시간 알림 (선택적) |
| **Elasticsearch** | 고급 검색 (선택적) |

---

## 4. 핵심 API 기능 (MVP)

### 4.1 게시물 관리 (Post Management)

#### 4.1.1 게시물 CRUD

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시물 작성** | 새 게시물 생성 (제목, 본문, 카테고리, 태그) | P0 |
| **게시물 조회** | 단일 게시물 상세 조회 | P0 |
| **게시물 수정** | 게시물 내용 수정 | P0 |
| **게시물 삭제** | 게시물 삭제 (소프트 삭제 지원) | P0 |
| **조회수 관리** | 조회수 증가, 중복 조회 방지 | P0 |
| **임시저장** | 작성 중인 게시물 임시 저장 | P1 |
| **상단고정 (공지)** | 특정 게시물 상단 고정 | P1 |
| **예약 발행** | 지정된 시간에 게시물 자동 발행 | P2 |

#### 4.1.2 게시물 속성

```
게시물 (Post)
├── ID (고유 식별자)
├── 제목 (Title)
├── 본문 (Content) - HTML/Markdown 지원
├── 작성자 (Author)
├── 카테고리 (Category)
├── 태그 (Tags)
├── 상태 (Status) - Draft/Published/Archived/Deleted
├── 조회수 (ViewCount)
├── 좋아요 수 (LikeCount)
├── 댓글 수 (CommentCount)
├── 상단고정 여부 (IsPinned)
├── 작성일시 (CreatedAt)
├── 수정일시 (UpdatedAt)
└── 발행일시 (PublishedAt)
```

### 4.2 목록 조회 및 검색

#### 4.2.1 목록 조회

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **페이징** | 오프셋/커서 기반 페이징 | P0 |
| **정렬** | 작성일, 조회수, 좋아요 수 등 기준 정렬 | P0 |
| **필터링** | 카테고리, 태그, 상태, 작성자별 필터 | P0 |
| **날짜 범위** | 특정 기간 내 게시물 조회 | P1 |

#### 4.2.2 검색

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **기본 검색** | 제목, 본문 내 키워드 검색 | P0 |
| **태그 검색** | 태그 기반 검색 | P0 |
| **작성자 검색** | 작성자명 기반 검색 | P1 |
| **고급 검색** | 복합 조건 검색 (AND/OR) | P2 |
| **전문 검색** | Elasticsearch 연동 (선택적) | P2 |

### 4.3 파일 첨부 및 미디어

#### 4.3.1 파일 업로드

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **파일 업로드** | 단일/다중 파일 업로드 | P0 |
| **이미지 업로드** | 이미지 파일 업로드 및 미리보기 | P0 |
| **썸네일 생성** | 이미지 업로드 시 자동 썸네일 생성 | P1 |
| **업로드 제한** | 파일 크기, 확장자, 개수 제한 | P0 |
| **바이러스 스캔** | 업로드 파일 악성코드 검사 | P1 |

#### 4.3.2 파일 관리

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **파일 다운로드** | 첨부 파일 다운로드 | P0 |
| **파일 삭제** | 첨부 파일 삭제 | P0 |
| **CDN 연동** | 파일 배포 CDN 연동 | P2 |
| **스토리지 추상화** | 로컬/클라우드 스토리지 지원 | P1 |

### 4.4 댓글 및 대댓글

#### 4.4.1 댓글 기능

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **댓글 작성** | 게시물에 댓글 작성 | P0 |
| **댓글 조회** | 게시물의 댓글 목록 조회 | P0 |
| **댓글 수정** | 본인 댓글 수정 | P0 |
| **댓글 삭제** | 본인 댓글 삭제 | P0 |
| **대댓글** | 댓글에 대한 답글 (중첩 지원) | P1 |
| **댓글 정렬** | 최신순, 인기순 정렬 | P1 |

### 4.5 좋아요 및 북마크

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시물 좋아요** | 게시물 좋아요/취소 | P0 |
| **댓글 좋아요** | 댓글 좋아요/취소 | P1 |
| **북마크** | 게시물 북마크/해제 | P1 |
| **북마크 목록** | 사용자 북마크 목록 조회 | P1 |

### 4.6 신고 및 블라인드

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시물 신고** | 부적절한 게시물 신고 | P1 |
| **댓글 신고** | 부적절한 댓글 신고 | P1 |
| **자동 블라인드** | 신고 누적 시 자동 숨김 처리 | P1 |
| **신고 관리** | 관리자의 신고 처리 | P1 |

### 4.7 Q&A 게시판 (질문/답변)

본 라이브러리는 일반 게시판 외에도 **Q&A 게시판** 기능을 제공합니다.

#### 4.7.1 질문 관리

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **질문 작성** | 새 질문 작성 (제목, 내용, 태그) | P0 |
| **질문 조회** | 단일 질문 상세 조회 | P0 |
| **질문 수정** | 질문 내용 수정 | P0 |
| **질문 삭제** | 질문 삭제 (답변이 없는 경우만) | P0 |
| **질문 검색** | 제목, 내용, 태그 기반 검색 | P0 |
| **질문 상태 관리** | Open/Answered/Closed 상태 관리 | P1 |
| **현상금 기능** | 질문에 포인트 현상금 설정 | P2 |

#### 4.7.2 답변 관리

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **답변 작성** | 질문에 대한 답변 작성 | P0 |
| **답변 조회** | 질문의 답변 목록 조회 | P0 |
| **답변 수정** | 본인 답변 수정 | P0 |
| **답변 삭제** | 본인 답변 삭제 | P0 |
| **답변 채택** | 질문자가 최적 답변 채택 | P0 |
| **답변 추천** | 답변 추천/비추천 | P1 |
| **답변 정렬** | 추천순, 최신순, 채택순 정렬 | P1 |

#### 4.7.3 Q&A 속성

```
질문 (Question)
├── ID (고유 식별자)
├── 제목 (Title)
├── 내용 (Content)
├── 작성자 (Author)
├── 상태 (Status) - Open/Answered/Closed
├── 조회수 (ViewCount)
├── 추천수 (VoteCount)
├── 태그 (Tags)
├── 채택된 답변 ID (AcceptedAnswerId)
├── 현상금 포인트 (BountyPoints)
├── 작성일시 (CreatedAt)
└── 수정일시 (UpdatedAt)

답변 (Answer)
├── ID (고유 식별자)
├── 내용 (Content)
├── 질문 ID (QuestionId)
├── 작성자 (Author)
├── 채택 여부 (IsAccepted)
├── 추천수 (VoteCount)
├── 작성일시 (CreatedAt)
└── 수정일시 (UpdatedAt)
```

### 4.8 템플릿 및 베이스 클래스

본 라이브러리는 각 기능별로 **템플릿 또는 베이스 클래스**를 제공하여 사용자가 쉽게 확장할 수 있도록 합니다.

#### 4.8.1 제공되는 베이스 클래스

| 베이스 클래스 | 설명 | 용도 |
|-------------|------|------|
| `EntityBase<TKey>` | 모든 엔티티의 기본 베이스 클래스 | ID, 생성/수정일시 공통 처리 |
| `PostBase` | 게시물 엔티티 베이스 | 게시물 공통 속성 및 동작 |
| `CommentBase` | 댓글 엔티티 베이스 | 댓글/대댓글 공통 처리 |
| `QuestionBase` | Q&A 질문 엔티티 베이스 | 질문 공통 속성 및 동작 |
| `AnswerBase` | Q&A 답변 엔티티 베이스 | 답변 공통 속성 및 동작 |
| `FileBase` | 첨부파일 엔티티 베이스 | 파일 관리 공통 처리 |

#### 4.8.2 서비스 템플릿

| 서비스 템플릿 | 설명 | 용도 |
|-------------|------|------|
| `CrudServiceBase<TEntity, TKey>` | CRUD 작업 기본 템플릿 | 엔티티별 CRUD 서비스 구현 |
| `PostServiceBase<TPost>` | 게시물 서비스 템플릿 | 게시물 비즈니스 로직 |
| `CommentServiceBase<TComment>` | 댓글 서비스 템플릿 | 댓글 비즈니스 로직 |
| `QnAServiceBase<TQuestion, TAnswer>` | Q&A 서비스 템플릿 | 질문/답변 비즈니스 로직 |
| `SearchServiceBase<TEntity>` | 검색 서비스 템플릿 | 검색 기능 구현 |

#### 4.8.3 컨트롤러 템플릿

| 컨트롤러 템플릿 | 설명 | 용도 |
|---------------|------|------|
| `CrudControllerBase<TEntity, TService>` | CRUD API 컨트롤러 템플릿 | 기본 CRUD 엔드포인트 |
| `PostControllerBase<TPost>` | 게시물 API 컨트롤러 템플릿 | 게시물 관련 엔드포인트 |
| `CommentControllerBase<TComment>` | 댓글 API 컨트롤러 템플릿 | 댓글 관련 엔드포인트 |
| `QnAControllerBase<TQuestion, TAnswer>` | Q&A API 컨트롤러 템플릿 | Q&A 관련 엔드포인트 |

#### 4.8.4 템플릿 사용 예시

```csharp
// 1. 커스텀 엔티티 정의 (베이스 클래스 상속)
public class MyPost : PostBase
{
    public string Department { get; set; } = string.Empty;
    public int Priority { get; set; }
}

// 2. 커스텀 서비스 정의 (서비스 템플릿 상속)
public class MyPostService : PostServiceBase<MyPost>
{
    public MyPostService(IRepository<MyPost> repository) : base(repository) { }
    
    // 프로젝트별 비즈니스 로직 추가
    public async Task<IEnumerable<MyPost>> GetByDepartmentAsync(string department)
    {
        return await Repository.FindAsync(p => p.Department == department);
    }
}

// 3. 커스텀 컨트롤러 정의 (컨트롤러 템플릿 상속)
[Route("api/my-posts")]
public class MyPostController : PostControllerBase<MyPost>
{
    private readonly MyPostService _myPostService;
    
    public MyPostController(MyPostService service) : base(service) 
    {
        _myPostService = service;
    }
    
    // 프로젝트별 추가 엔드포인트
    [HttpGet("by-department/{department}")]
    public async Task<IActionResult> GetByDepartment(string department)
    {
        var posts = await _myPostService.GetByDepartmentAsync(department);
        return Ok(posts);
    }
}
```

---

## 5. 인증·권한·보안

### 5.1 인증 (Authentication)

#### 5.1.1 JWT 인증

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **토큰 발급** | 로그인 성공 시 Access/Refresh 토큰 발급 | P0 |
| **토큰 갱신** | Refresh 토큰을 통한 Access 토큰 갱신 | P0 |
| **토큰 검증** | API 요청 시 토큰 유효성 검증 | P0 |
| **토큰 폐기** | 로그아웃 시 토큰 무효화 | P0 |

#### 5.1.2 OAuth 연동

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **소셜 로그인** | Google, GitHub, Microsoft 등 연동 | P2 |
| **외부 IdP 연동** | 기업 SSO 연동 지원 | P2 |

### 5.2 권한 (Authorization)

#### 5.2.1 역할 기반 접근 제어 (RBAC)

| 역할 | 권한 |
|-----|------|
| **Anonymous** | 공개 게시물 조회 |
| **User** | 게시물/댓글 CRUD (본인), 좋아요, 북마크, 신고 |
| **Moderator** | 게시물/댓글 관리, 신고 처리, 블라인드 |
| **Admin** | 전체 관리 기능, 사용자 관리, 시스템 설정 |

#### 5.2.2 권한 정책

```csharp
// 예시: 정책 기반 권한 설정
[Authorize(Policy = "CanEditPost")]
public async Task<IActionResult> UpdatePost(int id, UpdatePostRequest request)

[Authorize(Roles = "Admin,Moderator")]
public async Task<IActionResult> DeletePost(int id)
```

### 5.3 보안 (Security)

#### 5.3.1 입력 검증 및 방어

| 보안 항목 | 구현 방법 | 우선순위 |
|---------|----------|---------|
| **CSRF 방어** | Anti-Forgery Token, SameSite Cookie | P0 |
| **XSS 방어** | 입력 이스케이프, CSP 헤더 | P0 |
| **SQL Injection 방어** | Parameterized Query (EF Core 기본 지원) | P0 |
| **입력 검증** | FluentValidation 기반 서버측 검증 | P0 |
| **파일 검증** | 파일 시그니처 검증, 확장자 화이트리스트 | P0 |

#### 5.3.2 API 보안

| 보안 항목 | 구현 방법 | 우선순위 |
|---------|----------|---------|
| **Rate Limiting** | IP/사용자별 요청 제한 | P0 |
| **HTTPS 강제** | HTTP→HTTPS 리다이렉션 | P0 |
| **CORS 설정** | 허용된 Origin만 접근 가능 | P0 |
| **API Key** | 외부 시스템 연동용 API Key 지원 | P1 |

---

## 6. 운영·관리·확장

### 6.1 관리자 대시보드

#### 6.1.1 콘텐츠 관리

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시물 관리** | 전체 게시물 조회, 수정, 삭제 | P0 |
| **댓글 관리** | 전체 댓글 조회, 수정, 삭제 | P0 |
| **사용자 관리** | 사용자 조회, 권한 변경, 차단 | P1 |
| **신고 관리** | 신고 내역 조회 및 처리 | P1 |

#### 6.1.2 일괄 처리

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **일괄 삭제** | 선택된 게시물/댓글 일괄 삭제 | P1 |
| **일괄 상태 변경** | 게시물 상태 일괄 변경 | P1 |
| **일괄 이동** | 게시물 카테고리 일괄 이동 | P2 |

### 6.2 통계 및 분석

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시물 통계** | 기간별 게시물 수, 조회수, 좋아요 수 | P1 |
| **사용자 통계** | 활성 사용자 수, 신규 가입자 수 | P1 |
| **인기 콘텐츠** | 조회수/좋아요 기준 인기 게시물 | P1 |
| **검색어 통계** | 인기 검색어, 검색 트렌드 | P2 |

### 6.3 로그 및 감사

#### 6.3.1 로깅

| 로그 유형 | 내용 | 우선순위 |
|---------|------|---------|
| **애플리케이션 로그** | 에러, 경고, 정보 로그 | P0 |
| **액세스 로그** | API 요청/응답 로그 | P0 |
| **감사 로그** | 주요 데이터 변경 이력 | P1 |
| **보안 로그** | 인증 실패, 권한 위반 시도 | P0 |

#### 6.3.2 감사 추적

```
감사 이력 (AuditLog)
├── 액션 유형 (Create/Update/Delete)
├── 대상 엔티티 (Post/Comment/User)
├── 대상 ID
├── 변경 전 데이터
├── 변경 후 데이터
├── 수행자 (User)
├── 수행 일시
└── IP 주소
```

### 6.4 배치 작업

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **오래된 게시물 삭제** | N일 이상 된 게시물 자동 삭제 | P2 |
| **임시저장 정리** | 오래된 임시저장 게시물 삭제 | P2 |
| **첨부파일 정리** | 미사용 첨부파일 삭제 | P2 |
| **통계 집계** | 일/주/월간 통계 집계 | P2 |

### 6.5 백업 및 복원

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **데이터 백업** | 데이터베이스 백업 스케줄링 | P2 |
| **데이터 복원** | 백업 데이터 복원 | P2 |
| **데이터 내보내기** | 게시물 데이터 JSON/CSV 내보내기 | P2 |
| **데이터 가져오기** | 게시물 데이터 일괄 가져오기 | P2 |

### 6.6 확장성 (플러그인)

#### 6.6.1 플러그인 아키텍처

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **플러그인 인터페이스** | 표준화된 플러그인 인터페이스 정의 | P2 |
| **이벤트 시스템** | 게시물/댓글 생성/수정/삭제 이벤트 | P1 |
| **훅(Hook) 시스템** | 처리 전/후 커스텀 로직 삽입 | P2 |
| **커스텀 필드** | 동적 필드 추가 지원 | P2 |

#### 6.6.2 확장 포인트

```csharp
// 예시: 이벤트 핸들러
public class PostCreatedEventHandler : INotificationHandler<PostCreatedEvent>
{
    public async Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 게시물 생성 후 처리 로직
    }
}
```

### 6.7 알림 및 구독

| 기능 | 설명 | 우선순위 |
|-----|------|---------|
| **게시판 구독** | 특정 게시판의 새 게시물 알림 | P2 |
| **게시물 구독** | 특정 게시물의 새 댓글 알림 | P2 |
| **작성자 팔로우** | 특정 작성자의 새 게시물 알림 | P2 |
| **실시간 알림** | SignalR 기반 실시간 알림 | P2 |
| **이메일 알림** | 이메일을 통한 알림 발송 | P2 |

---

## 7. API 명세

### 7.1 API 경로 커스터마이징

본 라이브러리는 **공통 라이브러리**로서, 사용자가 API 경로를 프로젝트 요구사항에 맞게 **동적으로 정의**할 수 있도록 지원합니다.

#### 7.1.1 API 경로 설정 옵션

| 설정 항목 | 설명 | 기본값 |
|---------|------|--------|
| `ApiPrefix` | 모든 API의 기본 접두사 | `/api` |
| `ApiVersion` | API 버전 (URL 포함 여부 설정 가능) | `v1` |
| `PostsRoute` | 게시물 API 경로 | `posts` |
| `CommentsRoute` | 댓글 API 경로 | `comments` |
| `QuestionsRoute` | Q&A 질문 API 경로 | `questions` |
| `AnswersRoute` | Q&A 답변 API 경로 | `answers` |
| `FilesRoute` | 파일 API 경로 | `files` |
| `SearchRoute` | 검색 API 경로 | `search` |
| `AdminRoute` | 관리자 API 경로 | `admin` |

#### 7.1.2 API 설정 예시

```csharp
// Program.cs 또는 Startup.cs에서 설정
builder.Services.AddBoardLibrary(options =>
{
    // API 기본 경로 설정
    options.ApiPrefix = "/api";
    options.ApiVersion = "v1";
    options.IncludeVersionInUrl = true; // /api/v1/posts
    
    // 개별 리소스 경로 커스터마이징
    options.Routes.Posts = "articles";      // /api/v1/articles
    options.Routes.Comments = "replies";    // /api/v1/replies
    options.Routes.Questions = "qna";       // /api/v1/qna
    options.Routes.Files = "attachments";   // /api/v1/attachments
    
    // 관리자 API 경로 설정
    options.Routes.Admin = "management";    // /api/v1/management
});
```

#### 7.1.3 다중 게시판 경로 설정

여러 게시판을 운영하는 경우 각 게시판별로 독립적인 경로를 설정할 수 있습니다:

```csharp
builder.Services.AddBoardLibrary(options =>
{
    // 게시판별 경로 설정
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
    
    options.BoardRoutes.Add("support", new BoardRouteOptions
    {
        QuestionsRoute = "support-questions",
        AnswersRoute = "support-answers"
    });
});
```

#### 7.1.4 라우트 속성을 통한 커스터마이징

컨트롤러 레벨에서 라우트 속성을 오버라이드하여 경로를 변경할 수도 있습니다:

```csharp
// 기본 경로 사용
[Route("api/v1/[controller]")]
public class PostsController : PostControllerBase<Post> { }

// 커스텀 경로 사용 - 컨트롤러 이름은 리소스명 + Controller 형태로 통일
[Route("api/v1/articles")]
public class ArticlesController : PostControllerBase<Article> { }

// 버전 없이 사용 - 여러 단어 조합 시 PascalCase 사용
[Route("api/community/posts")]
public class CommunityPostsController : PostControllerBase<CommunityPost> { }
```

> **📝 컨트롤러 명명 규칙**: 컨트롤러 클래스는 `{리소스명}Controller` 형태로 명명합니다. 복합 단어의 경우 PascalCase를 사용합니다 (예: `CommunityPostsController`, `SupportQuestionsController`).

### 7.2 RESTful API 엔드포인트

#### 7.2.1 게시물 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/posts` | 게시물 목록 조회 |
| GET | `/api/posts/{id}` | 게시물 상세 조회 |
| POST | `/api/posts` | 게시물 작성 |
| PUT | `/api/posts/{id}` | 게시물 수정 |
| DELETE | `/api/posts/{id}` | 게시물 삭제 |
| POST | `/api/posts/{id}/pin` | 게시물 상단고정 |
| DELETE | `/api/posts/{id}/pin` | 게시물 상단고정 해제 |
| POST | `/api/posts/draft` | 임시저장 |
| GET | `/api/posts/draft` | 임시저장 목록 |

#### 7.2.2 댓글 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/posts/{postId}/comments` | 댓글 목록 조회 |
| POST | `/api/posts/{postId}/comments` | 댓글 작성 |
| PUT | `/api/comments/{id}` | 댓글 수정 |
| DELETE | `/api/comments/{id}` | 댓글 삭제 |
| POST | `/api/comments/{id}/replies` | 대댓글 작성 |

#### 7.2.3 파일 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| POST | `/api/files/upload` | 파일 업로드 |
| GET | `/api/files/{id}` | 파일 다운로드 |
| DELETE | `/api/files/{id}` | 파일 삭제 |
| GET | `/api/files/{id}/thumbnail` | 썸네일 조회 |

#### 7.2.4 사용자 활동 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| POST | `/api/posts/{id}/like` | 게시물 좋아요 |
| DELETE | `/api/posts/{id}/like` | 게시물 좋아요 취소 |
| POST | `/api/posts/{id}/bookmark` | 게시물 북마크 |
| DELETE | `/api/posts/{id}/bookmark` | 게시물 북마크 해제 |
| POST | `/api/posts/{id}/report` | 게시물 신고 |
| GET | `/api/users/me/bookmarks` | 내 북마크 목록 |

#### 7.2.5 검색 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/search?q={query}` | 통합 검색 |
| GET | `/api/search/posts?q={query}` | 게시물 검색 |
| GET | `/api/search/tags?q={query}` | 태그 검색 |

#### 7.2.6 관리자 API

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/admin/posts` | 전체 게시물 관리 |
| GET | `/api/admin/comments` | 전체 댓글 관리 |
| GET | `/api/admin/reports` | 신고 목록 |
| PUT | `/api/admin/reports/{id}` | 신고 처리 |
| GET | `/api/admin/statistics` | 통계 조회 |
| POST | `/api/admin/batch/delete` | 일괄 삭제 |

#### 7.2.7 Q&A API (질문/답변)

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/questions` | 질문 목록 조회 |
| GET | `/api/questions/{id}` | 질문 상세 조회 |
| POST | `/api/questions` | 질문 작성 |
| PUT | `/api/questions/{id}` | 질문 수정 |
| DELETE | `/api/questions/{id}` | 질문 삭제 |
| POST | `/api/questions/{id}/vote` | 질문 추천 |
| DELETE | `/api/questions/{id}/vote` | 질문 추천 취소 |
| POST | `/api/questions/{id}/close` | 질문 종료 |
| GET | `/api/questions/{questionId}/answers` | 답변 목록 조회 |
| POST | `/api/questions/{questionId}/answers` | 답변 작성 |
| PUT | `/api/answers/{id}` | 답변 수정 |
| DELETE | `/api/answers/{id}` | 답변 삭제 |
| POST | `/api/answers/{id}/accept` | 답변 채택 |
| POST | `/api/answers/{id}/vote` | 답변 추천 |
| DELETE | `/api/answers/{id}/vote` | 답변 추천 취소 |

### 7.3 API 응답 형식

#### 7.3.1 성공 응답

```json
{
  "success": true,
  "data": {
    // 응답 데이터
  },
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

#### 7.3.2 에러 응답

```json
{
  "success": false,
  "error": {
    "code": "POST_NOT_FOUND",
    "message": "게시물을 찾을 수 없습니다.",
    "details": [
      {
        "field": "id",
        "message": "유효하지 않은 게시물 ID입니다."
      }
    ]
  }
}
```

---

## 8. 데이터 모델

### 8.1 동적 테이블 구조 및 인터페이스 기반 설계

#### 8.1.1 설계 원칙

본 라이브러리는 **공통 라이브러리**로서, 다양한 프로젝트에서 유연하게 사용될 수 있도록 다음의 설계 원칙을 따릅니다:

| 원칙 | 설명 | 참조 |
|-----|------|------|
| **동적 필드 확장** | 테이블의 항목은 고정되지 않고 사용자 정의 필드를 동적으로 추가 가능 | [§8.1.3 동적 필드 확장](#813-동적-필드-확장-지원) |
| **인터페이스 기반 필수 항목** | 각 엔티티의 필수 항목은 인터페이스로 정의하여 강제성 확보 | [§8.1.2 인터페이스 정의](#812-필수-항목-인터페이스-정의) |
| **선택적 필드** | 필수 항목 외의 필드는 프로젝트 요구사항에 따라 선택적으로 확장 | [§8.3 테이블 스키마](#83-테이블-스키마-개념) |
| **유연한 상속 구조** | 베이스 엔티티를 상속하여 프로젝트별 커스터마이징 지원 | [§4.8 템플릿 및 베이스 클래스](#48-템플릿-및-베이스-클래스) |

#### 8.1.2 필수 항목 인터페이스 정의

각 엔티티의 필수 항목은 다음 인터페이스로 정의되어 구현 시 강제성을 가집니다:

```csharp
// 모든 엔티티의 기본 인터페이스
public interface IEntity
{
    long Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

// 게시물 필수 항목 인터페이스
public interface IPost : IEntity
{
    string Title { get; set; }
    string Content { get; set; }
    long AuthorId { get; set; }
    PostStatus Status { get; set; }
}

// 댓글 필수 항목 인터페이스
public interface IComment : IEntity
{
    string Content { get; set; }
    long PostId { get; set; }
    long AuthorId { get; set; }
}

// 첨부파일 필수 항목 인터페이스
public interface IFile : IEntity
{
    string FileName { get; set; }
    string ContentType { get; set; }
    long FileSize { get; set; }
    string StoragePath { get; set; }
}

// 사용자 필수 항목 인터페이스
public interface IUser : IEntity
{
    string Username { get; set; }
    string Email { get; set; }
}

// Q&A 답변 필수 항목 인터페이스
public interface IAnswer : IEntity
{
    string Content { get; set; }
    long QuestionId { get; set; }
    long AuthorId { get; set; }
    bool IsAccepted { get; set; }
}
```

#### 8.1.3 동적 필드 확장 지원

프로젝트별 요구사항에 따라 동적으로 필드를 확장할 수 있는 구조를 제공합니다:

```csharp
// 동적 필드 저장용 인터페이스
// ExtendedProperties는 데이터베이스에 JSON 형식(NVARCHAR(MAX))으로 저장됩니다.
// 값 타입은 JSON 직렬화가 가능한 기본 타입(string, int, bool, DateTime 등)을 사용해야 합니다.
public interface IHasExtendedProperties
{
    /// <summary>
    /// 동적 확장 필드. JSON 형식으로 데이터베이스에 저장됩니다.
    /// 지원 타입: string, int, long, double, bool, DateTime, string[], int[] 등
    /// 주의: 복잡한 중첩 객체는 별도 테이블 사용을 권장합니다.
    /// </summary>
    Dictionary<string, object>? ExtendedProperties { get; set; }
}

// 예시: 동적 필드를 포함한 게시물 엔티티
public class Post : IPost, IHasExtendedProperties
{
    // 필수 항목 (인터페이스 구현)
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public PostStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // 선택적 항목 (프로젝트별 확장 가능)
    public string? Slug { get; set; }
    public int ViewCount { get; set; }
    public bool IsPinned { get; set; }
    
    // 동적 확장 필드
    public Dictionary<string, object>? ExtendedProperties { get; set; }
}
```

#### 8.1.4 커스텀 엔티티 확장 예시

라이브러리 사용자는 다음과 같이 자신의 프로젝트에 맞게 엔티티를 확장할 수 있습니다:

```csharp
// 사용자 정의 게시물 엔티티 (필수 인터페이스 구현 필수)
public class CustomPost : Post
{
    // 프로젝트별 추가 필드
    public string? Department { get; set; }
    public int Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public List<string>? CustomTags { get; set; }
}

// 제네릭 기반 저장소 사용
public class CustomPostService : PostServiceBase<CustomPost>
{
    // 프로젝트별 비즈니스 로직 추가
}
```

### 8.2 핵심 엔티티

#### 8.2.1 Entity Relationship Diagram

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│     User     │────<│     Post     │>────│   Category   │
└──────────────┘     └──────────────┘     └──────────────┘
       │                    │                    
       │                    │ ┌──────────────┐
       │                    └─<│     Tag      │
       │                      └──────────────┘
       │              ┌──────────────┐
       └─────────────<│   Comment    │
                      └──────────────┘
                             │
                      ┌──────────────┐
                      │   Comment    │ (Self-referencing for replies)
                      └──────────────┘

※ Q&A 게시판 사용 시 추가 관계:
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│     User     │────<│   Question   │>────│    Answer    │
└──────────────┘     └──────────────┘     └──────────────┘
                           │
                    ┌──────────────┐
                    │   Comment    │ (Q&A 댓글은 기존 Comments 테이블 재사용, 
                    └──────────────┘  QuestionId 또는 AnswerId로 연결)
```

> **📝 Q&A 댓글**: Q&A 게시판의 댓글은 기존 Comments 테이블 구조를 재사용합니다. `PostId` 대신 `QuestionId` 또는 `AnswerId` 필드를 사용하여 질문이나 답변에 댓글을 연결합니다.

### 8.3 테이블 스키마 (개념)

#### 8.3.1 Posts 테이블 (필수 컬럼 + 선택적 컬럼)

| 컬럼 | 타입 | 필수 여부 | 설명 |
|-----|------|----------|------|
| Id | BIGINT | **필수** | Primary Key |
| Title | NVARCHAR(200) | **필수** | 제목 |
| Content | NVARCHAR(MAX) | **필수** | 본문 |
| AuthorId | BIGINT | **필수** | 작성자 FK |
| Status | INT | **필수** | 상태 (Draft/Published/Archived/Deleted) |
| CreatedAt | DATETIME2 | **필수** | 작성일시 |
| UpdatedAt | DATETIME2 | 선택 | 수정일시 |
| Slug | NVARCHAR(250) | 선택 | URL 슬러그 |
| ViewCount | INT | 선택 | 조회수 |
| LikeCount | INT | 선택 | 좋아요 수 |
| CommentCount | INT | 선택 | 댓글 수 |
| IsPinned | BIT | 선택 | 상단고정 여부 |
| CategoryId | BIGINT | 선택 | 카테고리 FK |
| PublishedAt | DATETIME2 | 선택 | 발행일시 |
| DeletedAt | DATETIME2 | 선택 | 삭제일시 (Soft Delete) |
| ExtendedProperties | NVARCHAR(MAX) | 선택 | 동적 확장 필드 (JSON) |

#### 8.3.2 Comments 테이블 (필수 컬럼 + 선택적 컬럼)

| 컬럼 | 타입 | 필수 여부 | 설명 |
|-----|------|----------|------|
| Id | BIGINT | **필수** | Primary Key |
| Content | NVARCHAR(2000) | **필수** | 댓글 내용 |
| PostId | BIGINT | 조건부 필수* | 게시물 FK |
| AuthorId | BIGINT | **필수** | 작성자 FK |
| CreatedAt | DATETIME2 | **필수** | 작성일시 |
| UpdatedAt | DATETIME2 | 선택 | 수정일시 |
| ParentId | BIGINT | 선택 | 부모 댓글 FK (대댓글) |
| QuestionId | BIGINT | 조건부 필수* | Q&A 질문 FK |
| AnswerId | BIGINT | 조건부 필수* | Q&A 답변 FK |
| LikeCount | INT | 선택 | 좋아요 수 |
| IsBlinded | BIT | 선택 | 블라인드 여부 |
| DeletedAt | DATETIME2 | 선택 | 삭제일시 |
| ExtendedProperties | NVARCHAR(MAX) | 선택 | 동적 확장 필드 (JSON) |

> **📝 조건부 필수***: `PostId`, `QuestionId`, `AnswerId` 중 하나는 반드시 설정되어야 합니다. 일반 게시판 댓글은 `PostId`를, Q&A 게시판 댓글은 `QuestionId` 또는 `AnswerId`를 사용합니다.

#### 8.3.3 Files 테이블 (필수 컬럼 + 선택적 컬럼)

| 컬럼 | 타입 | 필수 여부 | 설명 |
|-----|------|----------|------|
| Id | BIGINT | **필수** | Primary Key |
| FileName | NVARCHAR(255) | **필수** | 원본 파일명 |
| ContentType | NVARCHAR(100) | **필수** | MIME 타입 |
| FileSize | BIGINT | **필수** | 파일 크기 (bytes) |
| StoragePath | NVARCHAR(500) | **필수** | 저장 경로 |
| CreatedAt | DATETIME2 | **필수** | 업로드일시 |
| UpdatedAt | DATETIME2 | 선택 | 수정일시 |
| StoredFileName | NVARCHAR(255) | 선택 | 저장 파일명 |
| ThumbnailPath | NVARCHAR(500) | 선택 | 썸네일 경로 |
| PostId | BIGINT | 선택 | 게시물 FK |
| UploaderId | BIGINT | 선택 | 업로더 FK |
| ExtendedProperties | NVARCHAR(MAX) | 선택 | 동적 확장 필드 (JSON) |

#### 8.3.4 Questions 테이블 (Q&A 게시판용)

| 컬럼 | 타입 | 필수 여부 | 설명 |
|-----|------|----------|------|
| Id | BIGINT | **필수** | Primary Key |
| Title | NVARCHAR(200) | **필수** | 질문 제목 |
| Content | NVARCHAR(MAX) | **필수** | 질문 내용 |
| AuthorId | BIGINT | **필수** | 작성자 FK |
| Status | INT | **필수** | 상태 (Open/Answered/Closed) |
| CreatedAt | DATETIME2 | **필수** | 작성일시 |
| UpdatedAt | DATETIME2 | 선택 | 수정일시 |
| AcceptedAnswerId | BIGINT | 선택 | 채택된 답변 FK |
| ViewCount | INT | 선택 | 조회수 |
| VoteCount | INT | 선택 | 추천수 |
| BountyPoints | INT | 선택 | 현상금 포인트 |
| Tags | NVARCHAR(500) | 선택 | 태그 목록 (JSON) |
| ExtendedProperties | NVARCHAR(MAX) | 선택 | 동적 확장 필드 (JSON) |

#### 8.3.5 Answers 테이블 (Q&A 게시판용)

| 컬럼 | 타입 | 필수 여부 | 설명 |
|-----|------|----------|------|
| Id | BIGINT | **필수** | Primary Key |
| Content | NVARCHAR(MAX) | **필수** | 답변 내용 |
| QuestionId | BIGINT | **필수** | 질문 FK |
| AuthorId | BIGINT | **필수** | 작성자 FK |
| IsAccepted | BIT | **필수** | 채택 여부 |
| CreatedAt | DATETIME2 | **필수** | 작성일시 |
| UpdatedAt | DATETIME2 | 선택 | 수정일시 |
| VoteCount | INT | 선택 | 추천수 |
| ExtendedProperties | NVARCHAR(MAX) | 선택 | 동적 확장 필드 (JSON) |

---

## 9. 비기능 요구사항

### 9.1 성능

| 항목 | 목표 |
|-----|------|
| **API 응답 시간** | 평균 < 200ms, P99 < 500ms |
| **동시 사용자** | 1,000명 이상 동시 접속 처리 |
| **게시물 목록 조회** | 10,000건 기준 < 100ms |
| **검색 응답 시간** | 1,000,000건 기준 < 500ms |

### 9.2 확장성

| 항목 | 요구사항 |
|-----|---------|
| **수평 확장** | 로드밸런서 환경 지원 |
| **데이터베이스** | 읽기 복제본 지원 |
| **캐싱** | 분산 캐시 (Redis) 지원 |
| **스토리지** | 클라우드 스토리지 연동 |

### 9.3 가용성

| 항목 | 목표 |
|-----|------|
| **가용성** | 99.9% (연간 8.76시간 이내 다운타임) |
| **복구 시간** | RTO < 1시간, RPO < 1시간 |
| **헬스 체크** | `/health` 엔드포인트 제공 |

### 9.4 보안

| 항목 | 요구사항 |
|-----|---------|
| **데이터 암호화** | 전송 중 (TLS 1.2+), 저장 시 (민감 데이터) |
| **비밀번호 저장** | bcrypt/Argon2 해싱 |
| **보안 헤더** | HSTS, X-Content-Type-Options 등 |
| **취약점 관리** | 정기적 보안 스캔 및 패치 |

### 9.5 호환성

| 항목 | 지원 |
|-----|------|
| **.NET 버전** | .NET 8.0 LTS |
| **API 버전 관리** | URL 버저닝 (`/api/v1/posts`) |
| **하위 호환성** | Major 버전 내 하위 호환 유지 |

---

## 10. 릴리스 계획

### 10.1 버전 로드맵

#### Phase 1: MVP (v1.0) - 예상 기간: 8주

| 주차 | 목표 |
|-----|------|
| 1-2주 | 프로젝트 구조, 데이터 모델, 기본 인증 |
| 3-4주 | 게시물 CRUD, 목록 조회, 페이징 |
| 5-6주 | 댓글 기능, 좋아요/북마크, 파일 업로드 |
| 7-8주 | 테스트, 문서화, 릴리스 준비 |

**MVP 포함 기능:**
- ✅ 게시물 CRUD (조회수, 상단고정 포함)
- ✅ 댓글/대댓글
- ✅ 페이징, 정렬, 필터링
- ✅ 기본 검색 (제목, 본문, 태그)
- ✅ 파일 업로드 (기본 제한)
- ✅ 좋아요/북마크
- ✅ JWT 인증
- ✅ 역할 기반 권한
- ✅ 기본 보안 (CSRF, XSS, 입력 검증)

#### Phase 2: v1.1 - 예상 기간: 4주

- ✅ 임시저장
- ✅ 신고/블라인드
- ✅ 관리자 대시보드 API
- ✅ 감사 로그
- ✅ 썸네일 생성
- ✅ 파일 스캔

#### Phase 3: v1.2 - 예상 기간: 4주

- ✅ 알림/구독 시스템
- ✅ 이벤트 시스템
- ✅ 배치 작업
- ✅ 통계 API
- ✅ 예약 발행

#### Phase 4: v2.0 - 예상 기간: 6주

- ✅ 플러그인 아키텍처
- ✅ OAuth 연동
- ✅ Elasticsearch 연동
- ✅ 실시간 알림 (SignalR)
- ✅ CDN 연동
- ✅ 고급 캐싱

### 10.2 품질 보증

| 항목 | 기준 |
|-----|------|
| **단위 테스트 커버리지** | 80% 이상 |
| **통합 테스트** | 주요 API 시나리오 100% |
| **부하 테스트** | 목표 성능 달성 확인 |
| **보안 테스트** | OWASP Top 10 대응 확인 |
| **코드 리뷰** | 모든 PR 필수 리뷰 |

### 10.3 문서화

| 문서 | 내용 |
|-----|------|
| **API 문서** | Swagger/OpenAPI 기반 자동 생성 |
| **통합 가이드** | NuGet 패키지 설치 및 설정 가이드 |
| **개발자 가이드** | 확장 및 커스터마이징 가이드 |
| **마이그레이션 가이드** | 버전별 마이그레이션 안내 |

---

## 📝 부록

### A. 용어 정의

| 용어 | 정의 |
|-----|------|
| **CRUD** | Create, Read, Update, Delete - 기본 데이터 조작 |
| **MVP** | Minimum Viable Product - 최소 기능 제품 |
| **RBAC** | Role-Based Access Control - 역할 기반 접근 제어 |
| **ACL** | Access Control List - 접근 제어 목록 |
| **CSRF** | Cross-Site Request Forgery - 사이트 간 요청 위조 |
| **XSS** | Cross-Site Scripting - 크로스 사이트 스크립팅 |
| **CDN** | Content Delivery Network - 콘텐츠 전송 네트워크 |

### B. 참고 자료

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [REST API Design Guidelines](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)

---

## 📌 문서 정보

| 항목 | 내용 |
|-----|------|
| **버전** | 1.0 |
| **작성일** | 2025-11-27 |
| **작성자** | Board Common Library Team |
| **상태** | Draft |

---

*이 문서는 프로젝트 진행에 따라 지속적으로 업데이트됩니다.*
