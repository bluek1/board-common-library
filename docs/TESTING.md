# 테스트 가이드

## 📖 개요

본 문서는 게시판 공통 라이브러리의 테스트 전략과 테스트 웹서비스 사용 방법을 설명합니다.

---

## 🧪 테스트 전략

### 테스트 레벨

| 레벨 | 설명 | 도구 |
|-----|------|------|
| **단위 테스트** | 개별 클래스/메서드 테스트 | xUnit, Moq |
| **통합 테스트** | API 엔드포인트 테스트 | WebApplicationFactory |
| **E2E 테스트** | 전체 시나리오 테스트 | Playwright |
| **수동 테스트** | 테스트 웹서비스 활용 | Swagger UI |

### 테스트 커버리지 목표

| 항목 | 목표 |
|-----|------|
| 단위 테스트 커버리지 | 80% 이상 |
| 통합 테스트 커버리지 | 핵심 API 100% |
| 성능 테스트 | 목표 응답 시간 달성 확인 |

---

## 🌐 테스트 웹서비스

### 개요

테스트 웹서비스는 개발된 각 페이지 기능을 실시간으로 테스트하고 검증할 수 있는 웹 애플리케이션입니다.

### 프로젝트 구조

```
test-web/
├── BoardTestWeb/
│   ├── Controllers/
│   │   ├── TestPage1Controller.cs    # 페이지 1 테스트 (게시물 관리)
│   │   ├── TestPage2Controller.cs    # 페이지 2 테스트 (댓글/좋아요)
│   │   ├── TestPage3Controller.cs    # 페이지 3 테스트 (파일/검색)
│   │   └── TestPage4Controller.cs    # 페이지 4 테스트 (관리자/Q&A)
│   ├── Pages/
│   │   ├── Index.cshtml              # 메인 대시보드
│   │   ├── Page1/                    # 페이지 1 테스트 UI
│   │   ├── Page2/                    # 페이지 2 테스트 UI
│   │   ├── Page3/                    # 페이지 3 테스트 UI
│   │   └── Page4/                    # 페이지 4 테스트 UI
│   ├── Services/
│   │   └── TestExecutionService.cs   # 테스트 실행 서비스
│   ├── Models/
│   │   └── TestResult.cs             # 테스트 결과 모델
│   ├── appsettings.json
│   └── Program.cs
└── BoardTestWeb.Tests/
    ├── Page1Tests.cs                  # 페이지 1 단위 테스트
    ├── Page2Tests.cs                  # 페이지 2 단위 테스트
    ├── Page3Tests.cs                  # 페이지 3 단위 테스트
    └── Page4Tests.cs                  # 페이지 4 단위 테스트
```

### 실행 방법

```bash
# 테스트 웹서비스 디렉토리로 이동
cd test-web/BoardTestWeb

# 의존성 복원
dotnet restore

# 개발 모드 실행
dotnet run

# 또는 watch 모드로 실행 (자동 재시작)
dotnet watch run
```

### 접속 URL

| URL | 설명 |
|-----|------|
| `http://localhost:5000` | 테스트 대시보드 (메인) |
| `http://localhost:5000/page1` | 페이지 1 테스트 (게시물 관리) |
| `http://localhost:5000/page2` | 페이지 2 테스트 (댓글/좋아요) |
| `http://localhost:5000/page3` | 페이지 3 테스트 (파일/검색) |
| `http://localhost:5000/page4` | 페이지 4 테스트 (관리자/Q&A) |
| `http://localhost:5000/swagger` | Swagger API 문서 |

---

## 📄 페이지별 테스트 상세

### 페이지 1: 게시물 관리 테스트

#### 테스트 시나리오

```gherkin
기능: 게시물 CRUD

시나리오: 새 게시물 작성
  Given 사용자가 로그인한 상태이다
  When 유효한 제목과 본문을 입력하여 게시물을 작성한다
  Then 게시물이 성공적으로 생성된다
  And 게시물 ID가 반환된다

시나리오: 게시물 조회
  Given 게시물이 존재한다
  When 해당 게시물 ID로 조회 요청을 보낸다
  Then 게시물 상세 정보가 반환된다
  And 조회수가 1 증가한다

시나리오: 게시물 상단고정
  Given 관리자가 로그인한 상태이다
  And 게시물이 존재한다
  When 해당 게시물을 상단고정 설정한다
  Then 게시물의 isPinned가 true가 된다
  And 목록 조회 시 상단에 표시된다
```

#### UI 테스트 화면

- 게시물 작성 폼
- 게시물 목록 (페이징, 정렬, 필터)
- 게시물 상세 보기
- 상단고정 토글 버튼
- 임시저장 기능

### 페이지 2: 댓글/좋아요 테스트

#### 테스트 시나리오

```gherkin
기능: 댓글 및 대댓글

시나리오: 댓글 작성
  Given 사용자가 로그인한 상태이다
  And 게시물이 존재한다
  When 해당 게시물에 댓글을 작성한다
  Then 댓글이 성공적으로 생성된다
  And 게시물의 댓글 수가 1 증가한다

시나리오: 대댓글 작성
  Given 댓글이 존재한다
  When 해당 댓글에 대댓글을 작성한다
  Then 대댓글이 성공적으로 생성된다
  And 부모 댓글 아래에 표시된다

기능: 좋아요 및 북마크

시나리오: 게시물 좋아요
  Given 사용자가 로그인한 상태이다
  And 게시물이 존재한다
  When 해당 게시물에 좋아요를 누른다
  Then 좋아요가 추가된다
  And 좋아요 수가 1 증가한다

시나리오: 좋아요 취소
  Given 좋아요가 이미 눌린 게시물이 있다
  When 해당 게시물의 좋아요를 취소한다
  Then 좋아요가 제거된다
  And 좋아요 수가 1 감소한다
```

#### UI 테스트 화면

- 댓글 목록 (계층 구조)
- 댓글 작성 폼
- 대댓글 작성 폼
- 좋아요 버튼 (토글)
- 북마크 버튼 (토글)
- 내 북마크 목록

### 페이지 3: 파일/검색 테스트

#### 테스트 시나리오

```gherkin
기능: 파일 업로드

시나리오: 이미지 파일 업로드
  Given 사용자가 로그인한 상태이다
  When 허용된 이미지 파일을 업로드한다
  Then 파일이 성공적으로 업로드된다
  And 썸네일이 자동 생성된다

시나리오: 파일 크기 초과
  Given 사용자가 로그인한 상태이다
  When 최대 허용 크기를 초과하는 파일을 업로드한다
  Then 업로드가 실패한다
  And 오류 메시지가 표시된다

기능: 검색

시나리오: 키워드 검색
  Given 게시물이 여러 개 존재한다
  When "테스트"라는 키워드로 검색한다
  Then "테스트"를 포함하는 게시물 목록이 반환된다
  And 검색어가 하이라이트 표시된다

시나리오: 태그 검색
  Given 태그가 지정된 게시물이 존재한다
  When 특정 태그로 검색한다
  Then 해당 태그가 있는 게시물 목록이 반환된다
```

#### UI 테스트 화면

- 파일 업로드 (드래그앤드롭)
- 파일 목록 (미리보기)
- 검색 입력 폼
- 검색 결과 목록
- 검색 필터 (카테고리, 태그, 날짜)

### 페이지 4: 관리자/Q&A 테스트

#### 테스트 시나리오

```gherkin
기능: 관리자 기능

시나리오: 신고 처리
  Given 관리자가 로그인한 상태이다
  And 신고된 게시물이 존재한다
  When 해당 신고를 승인 처리한다
  Then 해당 게시물이 블라인드 처리된다
  And 신고 상태가 "처리완료"로 변경된다

시나리오: 일괄 삭제
  Given 관리자가 로그인한 상태이다
  And 삭제할 게시물들을 선택한다
  When 일괄 삭제를 실행한다
  Then 선택된 모든 게시물이 삭제된다
  And 삭제된 게시물 수가 반환된다

기능: Q&A 게시판

시나리오: 질문 작성
  Given 사용자가 로그인한 상태이다
  When 질문을 작성한다
  Then 질문이 성공적으로 생성된다
  And 질문 상태가 "Open"으로 설정된다

시나리오: 답변 채택
  Given 질문자가 로그인한 상태이다
  And 자신의 질문에 답변이 존재한다
  When 해당 답변을 채택한다
  Then 답변의 isAccepted가 true가 된다
  And 질문 상태가 "Answered"로 변경된다
```

#### UI 테스트 화면

- 관리자 대시보드
- 신고 관리 화면
- 통계 차트
- Q&A 질문 목록
- Q&A 질문 상세 (답변 포함)
- 답변 채택 버튼

---

## 🔧 테스트 실행

### 단위 테스트 실행

```bash
# 전체 테스트 실행
dotnet test

# 특정 페이지 테스트만 실행
dotnet test --filter "FullyQualifiedName~Page1Tests"
dotnet test --filter "FullyQualifiedName~Page2Tests"
dotnet test --filter "FullyQualifiedName~Page3Tests"
dotnet test --filter "FullyQualifiedName~Page4Tests"

# 테스트 결과 리포트 생성
dotnet test --logger "html;LogFileName=TestResults.html"
```

### 커버리지 리포트

```bash
# 커버리지 수집
dotnet test --collect:"XPlat Code Coverage"

# 리포트 생성 (ReportGenerator 필요)
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport
```

---

## 📈 테스트 결과 대시보드

테스트 웹서비스의 메인 대시보드에서 각 페이지별 테스트 진행 상황을 확인할 수 있습니다.

### 대시보드 기능

- **전체 진행 현황**: 4개 페이지 전체 완료율
- **페이지별 상세**: 각 페이지의 테스트 케이스 통과율
- **최근 테스트 결과**: 마지막 테스트 실행 결과
- **실패 테스트 목록**: 실패한 테스트 케이스 상세

### API 엔드포인트

| 메서드 | 엔드포인트 | 설명 |
|-------|-----------|------|
| GET | `/api/test/status` | 전체 테스트 상태 조회 |
| GET | `/api/test/page/{pageNumber}` | 페이지별 테스트 상태 |
| POST | `/api/test/run/{pageNumber}` | 페이지별 테스트 실행 |
| POST | `/api/test/run/all` | 전체 테스트 실행 |
| GET | `/api/test/results` | 테스트 결과 조회 |

---

## 📝 테스트 데이터

### 시드 데이터

테스트 웹서비스 실행 시 다음 시드 데이터가 자동 생성됩니다:

| 데이터 | 수량 | 설명 |
|-------|-----|------|
| 사용자 | 5명 | 관리자 1명, 일반 사용자 4명 |
| 게시물 | 50개 | 각 카테고리별 10개씩 |
| 댓글 | 200개 | 게시물당 평균 4개 |
| 파일 | 20개 | 이미지 10개, 문서 10개 |
| 질문 | 10개 | Q&A 게시판용 |
| 답변 | 30개 | 질문당 평균 3개 |

### 테스트 계정

| 역할 | 이메일 | 비밀번호 |
|-----|-------|---------|
| 관리자 | admin@test.com | Admin123! |
| 사용자1 | user1@test.com | User123! |
| 사용자2 | user2@test.com | User123! |
| 사용자3 | user3@test.com | User123! |
| 사용자4 | user4@test.com | User123! |

---

*최종 업데이트: 2025-11-27*
