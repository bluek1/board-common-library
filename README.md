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
| **JWT/OAuth** | 인증 |
| **SQL Server / PostgreSQL / MySQL** | 데이터베이스 |
| **Redis** | 캐싱 (선택적) |
| **SignalR** | 실시간 알림 (선택적) |

## 📖 문서

- [제품 요구사항 문서 (PRD)](docs/PRD.md) - 상세 기능 명세 및 API 설계
- [NuGet 배포 가이드](docs/NUGET.md) - 패키지 설치 및 배포 가이드
- [페이지별 기능 명세](docs/PAGES.md) - 4페이지 구성 및 테스트 케이스
- [테스트 가이드](docs/TESTING.md) - 테스트 웹서비스 사용 가이드

## 🚀 시작하기

### NuGet 패키지 설치

```bash
# .NET CLI
dotnet add package BoardCommonLibrary --version 1.0.0

# 또는 패키지 관리자
Install-Package BoardCommonLibrary -Version 1.0.0
```

### 저장소 클론 및 빌드

```bash
# 저장소 클론
git clone https://github.com/bluek1/board-common-library.git

# 프로젝트 디렉토리로 이동
cd board-common-library
```

### 테스트 웹서비스 실행

```bash
# 테스트 웹서비스 디렉토리로 이동
cd test-web/BoardTestWeb

# 의존성 복원 및 실행
dotnet restore
dotnet run

# 웹 브라우저에서 http://localhost:5000 접속
```

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
