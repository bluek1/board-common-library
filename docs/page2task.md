# 페이지 2: 댓글/대댓글 및 좋아요/북마크 세부 작업 명세서

## 📋 개요

본 문서는 댓글/대댓글 기능과 좋아요/북마크 기능 구현을 위한 세부 작업 내용을 정의합니다.

**우선순위**: P0 (필수)  
**총 기능 수**: 12개  
**총 테스트 수**: 15개  
**진행 상태**: 🟢 완료

---

## 🔧 작업 목록

### 1. 데이터 모델 설계 및 구현

#### 1.1 댓글(Comment) 엔티티 설계
- [x] Comment 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Comment.cs`
  - `Id`: 고유 식별자 (long)
  - `Content`: 댓글 내용 (string, 필수, 최대 2000자)
  - `PostId`: 게시물 ID (long, FK)
  - `AuthorId`: 작성자 ID (long)
  - `AuthorName`: 작성자명 (string)
  - `ParentId`: 부모 댓글 ID (long?, 대댓글용)
  - `LikeCount`: 좋아요 수 (int, 기본값 0)
  - `IsBlinded`: 블라인드 여부 (bool, 기본값 false)
  - `IsDeleted`: 삭제 여부 (bool, 기본값 false)
  - `CreatedAt`: 생성일시 (DateTime)
  - `UpdatedAt`: 수정일시 (DateTime?)
  - `DeletedAt`: 삭제일시 (DateTime?)
  - Navigation Properties:
    - `Post`: 게시물 (Post)
    - `Parent`: 부모 댓글 (Comment?)
    - `Replies`: 자식 댓글 (ICollection<Comment>)

#### 1.2 좋아요(Like) 엔티티 설계
- [x] Like 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Like.cs`
  - `Id`: 고유 식별자 (long)
  - `UserId`: 사용자 ID (long)
  - `PostId`: 게시물 ID (long?, 게시물 좋아요 시)
  - `CommentId`: 댓글 ID (long?, 댓글 좋아요 시)
  - `CreatedAt`: 생성일시 (DateTime)
  - Unique Constraint: (UserId, PostId) 또는 (UserId, CommentId) 조합

#### 1.3 북마크(Bookmark) 엔티티 설계
- [x] Bookmark 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/Bookmark.cs`
  - `Id`: 고유 식별자 (long)
  - `UserId`: 사용자 ID (long)
  - `PostId`: 게시물 ID (long, FK)
  - `CreatedAt`: 생성일시 (DateTime)
  - Unique Constraint: (UserId, PostId) 조합

#### 1.4 데이터베이스 마이그레이션
- [x] Comments 테이블 생성 마이그레이션 작성
- [x] Likes 테이블 생성 마이그레이션 작성
- [x] Bookmarks 테이블 생성 마이그레이션 작성
- [x] 인덱스 생성
  - Comments: (PostId, IsDeleted, CreatedAt), (AuthorId), (ParentId)
  - Likes: (UserId, PostId), (UserId, CommentId)
  - Bookmarks: (UserId, PostId), (UserId, CreatedAt)
- [x] 외래 키 관계 설정

---

### 2. API 엔드포인트 구현

#### 2.1 댓글 작성 (P2-001)
- [x] POST `/api/posts/{postId}/comments` 엔드포인트 구현
- [x] 요청 DTO 생성 (CreateCommentRequest)
  - Content (필수, 최대 2000자)
- [x] 응답 DTO 생성 (CommentResponse)
  - Id, Content, PostId, AuthorId, AuthorName, ParentId
  - LikeCount, IsBlinded, CreatedAt, UpdatedAt
  - Replies (대댓글 목록, 선택적 포함)
- [x] 유효성 검증 로직 구현
  - 내용 필수 검증
  - 내용 최대 길이 검증 (2000자)
  - 게시물 존재 여부 검증
- [x] 서비스 계층 로직 구현
- [x] 게시물 댓글 수(CommentCount) 자동 증가 로직
- [x] 통합 테스트 완료

#### 2.2 댓글 조회 (P2-002)
- [x] GET `/api/posts/{postId}/comments` 엔드포인트 구현
- [x] 쿼리 파라미터 처리
  - `page`: 페이지 번호 (기본값 1)
  - `pageSize`: 페이지 크기 (기본값 20, 최대 100)
  - `sortBy`: 정렬 기준 (createdAt, likeCount)
  - `sortOrder`: 정렬 순서 (asc, desc)
  - `includeReplies`: 대댓글 포함 여부 (기본값 true)
- [x] 계층 구조 조회 로직 구현 (부모 댓글 → 자식 댓글)
- [x] 삭제된 댓글 처리 (내용은 "삭제된 댓글입니다"로 표시, 대댓글 있으면 구조 유지)
- [x] 블라인드 처리된 댓글 표시 로직
- [x] 페이지 응답 DTO 생성 (PagedResponse<CommentResponse>)
- [x] 통합 테스트 완료

#### 2.3 댓글 수정 (P2-003)
- [x] PUT `/api/comments/{id}` 엔드포인트 구현
- [x] 요청 DTO 생성 (UpdateCommentRequest)
  - Content (필수)
- [x] 권한 검증 로직 구현 (작성자만 수정 가능)
- [x] 수정일시 자동 갱신 로직
- [x] 403 에러 처리 (권한 없음)
- [x] 404 에러 처리 (댓글 미존재)
- [x] 통합 테스트 완료

#### 2.4 댓글 삭제 (P2-004)
- [x] DELETE `/api/comments/{id}` 엔드포인트 구현
- [x] 소프트 삭제 로직 구현 (IsDeleted = true, DeletedAt 설정)
- [x] 권한 검증 로직 구현 (작성자 또는 관리자만 삭제 가능)
- [x] 게시물 댓글 수(CommentCount) 자동 감소 로직
- [x] 대댓글이 있는 경우 처리
  - 내용만 "삭제된 댓글입니다"로 변경
  - 구조는 유지
- [x] 403 에러 처리 (권한 없음)
- [x] 통합 테스트 완료

#### 2.5 대댓글 작성 (P2-005)
- [x] POST `/api/comments/{id}/replies` 엔드포인트 구현
- [x] 요청 DTO 재사용 (CreateCommentRequest)
- [x] 부모 댓글 존재 여부 검증
- [x] 대댓글 깊이 제한 로직 (최대 2단계)
  - 부모 댓글의 ParentId가 null이 아니면 오류 반환
- [x] 부모 댓글의 PostId를 자동 상속
- [x] 게시물 댓글 수(CommentCount) 자동 증가 로직
- [x] 통합 테스트 완료

#### 2.6 대댓글 조회 (P2-006)
- [x] GET `/api/comments/{id}/replies` 엔드포인트 구현 (선택적)
- [x] 또는 댓글 조회 시 대댓글 자동 포함 (includeReplies 옵션)
- [x] 통합 테스트 완료

#### 2.7 게시물 좋아요 (P2-007)
- [x] POST `/api/posts/{id}/like` 엔드포인트 구현
- [x] 중복 좋아요 방지 로직 (동일 사용자 + 게시물 조합 체크)
- [x] 좋아요 추가 시 게시물 LikeCount 자동 증가
- [x] 이미 좋아요한 경우 409 Conflict 반환
- [x] 응답: 현재 좋아요 상태 및 총 좋아요 수
- [x] 통합 테스트 완료

#### 2.8 게시물 좋아요 취소 (P2-008)
- [x] DELETE `/api/posts/{id}/like` 엔드포인트 구현
- [x] 좋아요 취소 시 게시물 LikeCount 자동 감소
- [x] 좋아요하지 않은 경우 404 Not Found 반환
- [x] 응답: 현재 좋아요 상태 및 총 좋아요 수
- [x] 통합 테스트 완료

#### 2.9 댓글 좋아요 (P2-009)
- [x] POST `/api/comments/{id}/like` 엔드포인트 구현
- [x] 중복 좋아요 방지 로직 (동일 사용자 + 댓글 조합 체크)
- [x] 좋아요 추가 시 댓글 LikeCount 자동 증가
- [x] 이미 좋아요한 경우 409 Conflict 반환
- [x] 좋아요 취소 로직 (토글 방식 또는 별도 DELETE 엔드포인트)
- [x] 통합 테스트 완료

#### 2.10 북마크 추가 (P2-010)
- [x] POST `/api/posts/{id}/bookmark` 엔드포인트 구현
- [x] 중복 북마크 방지 로직 (동일 사용자 + 게시물 조합 체크)
- [x] 이미 북마크한 경우 409 Conflict 반환
- [x] 통합 테스트 완료

#### 2.11 북마크 해제 (P2-011)
- [x] DELETE `/api/posts/{id}/bookmark` 엔드포인트 구현
- [x] 북마크하지 않은 경우 404 Not Found 반환
- [x] 통합 테스트 완료

#### 2.12 북마크 목록 (P2-012)
- [x] GET `/api/users/me/bookmarks` 엔드포인트 구현
- [x] 쿼리 파라미터 처리
  - `page`: 페이지 번호 (기본값 1)
  - `pageSize`: 페이지 크기 (기본값 20, 최대 100)
  - `sortBy`: 정렬 기준 (createdAt - 북마크 일시)
  - `sortOrder`: 정렬 순서 (asc, desc)
- [x] 현재 사용자의 북마크 목록 조회 로직
- [x] 게시물 요약 정보 포함 (PostSummaryResponse)
- [x] 삭제된 게시물 제외 로직
- [x] 통합 테스트 완료

---

### 3. 비즈니스 로직 구현

#### 3.1 댓글 서비스 (CommentService)
- [x] ICommentService 인터페이스 정의 `Services/Interfaces/ICommentService.cs`
  ```csharp
  Task<CommentResponse> CreateAsync(long postId, CreateCommentRequest request, long authorId, string authorName);
  Task<PagedResponse<CommentResponse>> GetByPostIdAsync(long postId, CommentQueryParameters parameters);
  Task<CommentResponse> GetByIdAsync(long id);
  Task<CommentResponse> UpdateAsync(long id, UpdateCommentRequest request, long currentUserId);
  Task DeleteAsync(long id, long currentUserId, bool isAdmin);
  Task<CommentResponse> CreateReplyAsync(long parentId, CreateCommentRequest request, long authorId, string authorName);
  ```
- [x] CommentService 클래스 구현 `Services/CommentService.cs`
- [x] 의존성 주입 설정

#### 3.2 좋아요 서비스 (LikeService)
- [x] ILikeService 인터페이스 정의 `Services/Interfaces/ILikeService.cs`
  ```csharp
  Task<LikeResponse> LikePostAsync(long postId, long userId);
  Task<LikeResponse> UnlikePostAsync(long postId, long userId);
  Task<LikeResponse> LikeCommentAsync(long commentId, long userId);
  Task<LikeResponse> UnlikeCommentAsync(long commentId, long userId);
  Task<bool> HasUserLikedPostAsync(long postId, long userId);
  Task<bool> HasUserLikedCommentAsync(long commentId, long userId);
  ```
- [x] LikeService 클래스 구현 `Services/LikeService.cs`
- [x] 의존성 주입 설정

#### 3.3 북마크 서비스 (BookmarkService)
- [x] IBookmarkService 인터페이스 정의 `Services/Interfaces/IBookmarkService.cs`
  ```csharp
  Task AddBookmarkAsync(long postId, long userId);
  Task RemoveBookmarkAsync(long postId, long userId);
  Task<PagedResponse<BookmarkResponse>> GetUserBookmarksAsync(long userId, BookmarkQueryParameters parameters);
  Task<bool> HasUserBookmarkedAsync(long postId, long userId);
  ```
- [x] BookmarkService 클래스 구현 `Services/BookmarkService.cs`
- [x] 의존성 주입 설정

#### 3.4 통계 자동 업데이트 로직
- [x] 댓글 생성/삭제 시 Post.CommentCount 자동 갱신
- [x] 좋아요 추가/취소 시 Post.LikeCount 또는 Comment.LikeCount 자동 갱신
- [x] 트랜잭션 처리로 데이터 일관성 보장

---

### 4. DTO 정의

#### 4.1 댓글 관련 DTO
- [x] CreateCommentRequest
  ```csharp
  public class CreateCommentRequest
  {
      [Required]
      [MaxLength(2000)]
      public string Content { get; set; } = string.Empty;
  }
  ```

- [x] UpdateCommentRequest
  ```csharp
  public class UpdateCommentRequest
  {
      [Required]
      [MaxLength(2000)]
      public string Content { get; set; } = string.Empty;
  }
  ```

- [x] CommentResponse
  ```csharp
  public class CommentResponse
  {
      public long Id { get; set; }
      public string Content { get; set; } = string.Empty;
      public long PostId { get; set; }
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      public long? ParentId { get; set; }
      public int LikeCount { get; set; }
      public bool IsBlinded { get; set; }
      public bool IsDeleted { get; set; }
      public DateTime CreatedAt { get; set; }
      public DateTime? UpdatedAt { get; set; }
      public List<CommentResponse>? Replies { get; set; }
  }
  ```

- [x] CommentQueryParameters
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

#### 4.2 좋아요 관련 DTO
- [x] LikeResponse
  ```csharp
  public class LikeResponse
  {
      public bool IsLiked { get; set; }
      public int TotalLikeCount { get; set; }
  }
  ```

#### 4.3 북마크 관련 DTO
- [x] BookmarkResponse
  ```csharp
  public class BookmarkResponse
  {
      public long Id { get; set; }
      public long PostId { get; set; }
      public PostSummaryResponse Post { get; set; } = null!;
      public DateTime CreatedAt { get; set; }
  }
  ```

- [x] BookmarkQueryParameters
  ```csharp
  public class BookmarkQueryParameters
  {
      public int Page { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      public string SortBy { get; set; } = "createdAt";
      public string SortOrder { get; set; } = "desc";
  }
  ```

---

### 5. 유효성 검증기 구현

#### 5.1 댓글 검증기
- [x] CreateCommentRequestValidator
  ```csharp
  public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
  {
      public CreateCommentRequestValidator()
      {
          RuleFor(x => x.Content)
              .NotEmpty().WithMessage("댓글 내용은 필수입니다.")
              .MaximumLength(2000).WithMessage("댓글은 2000자 이내여야 합니다.");
      }
  }
  ```

- [x] UpdateCommentRequestValidator
  ```csharp
  public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
  {
      public UpdateCommentRequestValidator()
      {
          RuleFor(x => x.Content)
              .NotEmpty().WithMessage("댓글 내용은 필수입니다.")
              .MaximumLength(2000).WithMessage("댓글은 2000자 이내여야 합니다.");
      }
  }
  ```

---

### 6. 테스트 구현

#### 6.1 단위 테스트
- [x] T2-001: 댓글 작성 성공 테스트
- [x] T2-002: 댓글 작성 실패 - 미인증 테스트
- [x] T2-003: 댓글 목록 조회 테스트
- [x] T2-004: 댓글 수정 성공 테스트
- [x] T2-005: 댓글 수정 실패 - 권한 없음 테스트
- [x] T2-006: 댓글 삭제 성공 테스트
- [x] T2-007: 대댓글 작성 성공 테스트
- [x] T2-008: 대댓글 계층 조회 테스트 (2단계 제한 확인)
- [x] T2-009: 좋아요 토글 - 추가 테스트
- [x] T2-010: 좋아요 토글 - 취소 테스트
- [x] T2-011: 좋아요 중복 방지 테스트
- [x] T2-012: 북마크 추가 테스트
- [x] T2-013: 북마크 해제 테스트
- [x] T2-014: 북마크 목록 조회 테스트
- [x] T2-015: 댓글 수 동기화 테스트

#### 6.2 통합 테스트
- [x] API 엔드포인트 통합 테스트 작성 `TestPage2Controller`
- [x] 데이터베이스 연동 테스트 (InMemory DB)

#### 6.3 테스트 커버리지
- [x] 테스트 커버리지 80% 이상 달성 (66개 테스트 통과)

---

### 7. 문서화

#### 7.1 API 문서
- [x] Swagger/OpenAPI 문서 작성
- [x] API 사용 예제 작성

#### 7.2 코드 문서
- [x] 주요 클래스 및 메서드 XML 주석 작성
- [x] README 업데이트

---

## 📅 작업 일정 (완료)

| 단계 | 작업 내용 | 예상 소요 시간 | 상태 |
|-----|----------|--------------|------|
| 1단계 | 데이터 모델 설계 및 구현 | 4시간 | 🟢 완료 |
| 2단계 | 댓글 CRUD API 구현 (P2-001 ~ P2-006) | 8시간 | 🟢 완료 |
| 3단계 | 좋아요 기능 구현 (P2-007 ~ P2-009) | 4시간 | 🟢 완료 |
| 4단계 | 북마크 기능 구현 (P2-010 ~ P2-012) | 3시간 | 🟢 완료 |
| 5단계 | 통계 자동 업데이트 로직 구현 | 2시간 | 🟢 완료 |
| 6단계 | 테스트 작성 및 검증 | 6시간 | 🟢 완료 |
| 7단계 | 문서화 | 2시간 | 🟢 완료 |
| **합계** | | **29시간** | **100%** |

---

## ✅ 완료 기준

### 기능 완료 기준
- [x] 모든 API 엔드포인트 구현 완료 (11개)
- [x] 대댓글 계층 구조 지원 (최대 2단계)
- [x] 좋아요/북마크 중복 방지 로직 정상 동작
- [x] 게시물 통계 자동 업데이트 (댓글 수, 좋아요 수)
- [x] 삭제된 댓글 처리 로직 (대댓글 있을 경우 구조 유지)

### 테스트 완료 기준
- [x] 모든 테스트 케이스 통과 (66개 - 15개 요구사항 초과 달성)
- [x] 테스트 커버리지 80% 이상

### 문서화 완료 기준
- [x] API 문서 작성 완료 (Swagger)
- [x] 코드 주석 작성 완료

---

## 📁 생성 예정 파일 목록

### 라이브러리 (src/BoardCommonLibrary/)
```
├── Controllers/
│   ├── CommentsController.cs      # 댓글 API 컨트롤러
│   ├── LikesController.cs         # 좋아요 API 컨트롤러 (또는 기존 컨트롤러에 통합)
│   └── BookmarksController.cs     # 북마크 API 컨트롤러
├── DTOs/
│   ├── CommentRequests.cs         # 댓글 요청 DTO
│   ├── CommentResponses.cs        # 댓글 응답 DTO
│   ├── LikeResponses.cs           # 좋아요 응답 DTO
│   └── BookmarkResponses.cs       # 북마크 응답 DTO
├── Entities/
│   ├── Comment.cs                 # 댓글 엔티티
│   ├── Like.cs                    # 좋아요 엔티티
│   └── Bookmark.cs                # 북마크 엔티티
├── Services/
│   ├── Interfaces/
│   │   ├── ICommentService.cs
│   │   ├── ILikeService.cs
│   │   └── IBookmarkService.cs
│   ├── CommentService.cs
│   ├── LikeService.cs
│   └── BookmarkService.cs
└── Validators/
    └── CommentValidators.cs       # 댓글 유효성 검증기
```

### 테스트 웹서비스 (test-web/BoardTestWeb/)
```
├── Controllers/
│   └── TestPage2Controller.cs     # 업데이트
```

### 단위 테스트 (tests/BoardCommonLibrary.Tests/)
```
├── Services/
│   ├── CommentServiceTests.cs
│   ├── LikeServiceTests.cs
│   └── BookmarkServiceTests.cs
├── Validators/
│   └── CommentValidatorsTests.cs
└── DTOs/
    └── CommentDtoTests.cs
```

---

## 🔗 관련 문서

- [PAGES.md](./PAGES.md) - 전체 페이지 기능 명세서
- [PRD.md](./PRD.md) - 제품 요구사항 문서
- [TESTING.md](./TESTING.md) - 테스트 가이드
- [page1task.md](./page1task.md) - 페이지 1 작업 명세서

---

*최종 업데이트: 2025-11-29*
