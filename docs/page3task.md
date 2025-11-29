# 페이지 3: 파일 첨부 및 검색 세부 작업 명세서

## 📋 개요

본 문서는 파일 첨부 기능과 검색 기능 구현을 위한 세부 작업 내용을 정의합니다.

**우선순위**: P0 (필수)  
**총 기능 수**: 10개  
**총 테스트 수**: 15개 (최소)  
**진행 상태**: 🔴 대기

---

## 🔧 작업 목록

### 1. 데이터 모델 설계 및 구현

#### 1.1 파일(FileAttachment) 엔티티 설계
- [ ] FileAttachment 엔티티 클래스 생성 `src/BoardCommonLibrary/Entities/FileAttachment.cs`
  - `Id`: 고유 식별자 (long)
  - `FileName`: 원본 파일명 (string, 필수, 최대 255자)
  - `StoredFileName`: 저장 파일명 (string, 필수, 최대 255자)
  - `ContentType`: MIME 타입 (string, 필수, 최대 100자)
  - `FileSize`: 파일 크기 (long, bytes)
  - `StoragePath`: 저장 경로 (string, 최대 500자)
  - `ThumbnailPath`: 썸네일 경로 (string?, 최대 500자)
  - `PostId`: 게시물 ID (long?, FK)
  - `UploaderId`: 업로더 ID (long)
  - `UploaderName`: 업로더명 (string?)
  - `DownloadCount`: 다운로드 횟수 (int, 기본값 0)
  - `IsImage`: 이미지 여부 (bool)
  - `Width`: 이미지 너비 (int?, 이미지인 경우)
  - `Height`: 이미지 높이 (int?, 이미지인 경우)
  - `CreatedAt`: 업로드일시 (DateTime)
  - `UpdatedAt`: 수정일시 (DateTime?)
  - `IsDeleted`: 삭제 여부 (bool, 기본값 false)
  - `DeletedAt`: 삭제일시 (DateTime?)
  - Navigation Properties:
    - `Post`: 게시물 (Post?)

#### 1.2 데이터베이스 설정
- [ ] FileAttachments 테이블 DbContext 설정 추가
- [ ] 인덱스 생성
  - (PostId, IsDeleted)
  - (UploaderId, CreatedAt)
  - (ContentType)
- [ ] 외래 키 관계 설정 (Post)

---

### 2. 파일 저장소 인터페이스 설계

#### 2.1 스토리지 추상화 인터페이스
- [ ] IFileStorageService 인터페이스 정의 `Services/Interfaces/IFileStorageService.cs`
  ```csharp
  public interface IFileStorageService
  {
      /// <summary>
      /// 파일 저장
      /// </summary>
      Task<FileStorageResult> SaveAsync(Stream fileStream, string fileName, string contentType);
      
      /// <summary>
      /// 파일 읽기
      /// </summary>
      Task<Stream?> GetAsync(string storagePath);
      
      /// <summary>
      /// 파일 삭제
      /// </summary>
      Task<bool> DeleteAsync(string storagePath);
      
      /// <summary>
      /// 파일 존재 여부 확인
      /// </summary>
      Task<bool> ExistsAsync(string storagePath);
      
      /// <summary>
      /// 썸네일 생성
      /// </summary>
      Task<string?> CreateThumbnailAsync(string storagePath, int width, int height);
  }
  ```

#### 2.2 로컬 스토리지 구현
- [ ] LocalFileStorageService 클래스 구현 `Services/LocalFileStorageService.cs`
  - 파일 저장 (UUID 기반 파일명 생성)
  - 파일 읽기 (스트림 반환)
  - 파일 삭제
  - 디렉토리 자동 생성
  - 날짜별 폴더 구조 (yyyy/MM/dd/)

#### 2.3 썸네일 서비스
- [ ] IThumbnailService 인터페이스 정의 `Services/Interfaces/IThumbnailService.cs`
  ```csharp
  public interface IThumbnailService
  {
      /// <summary>
      /// 썸네일 생성
      /// </summary>
      Task<ThumbnailResult?> GenerateAsync(Stream imageStream, int maxWidth, int maxHeight);
      
      /// <summary>
      /// 이미지 메타데이터 추출
      /// </summary>
      Task<ImageMetadata?> GetMetadataAsync(Stream imageStream);
  }
  ```

- [ ] ThumbnailService 클래스 구현 `Services/ThumbnailService.cs`
  - SkiaSharp 또는 ImageSharp 사용
  - 비율 유지 리사이징
  - 품질 설정 (기본 80%)

---

### 3. 파일 업로드 기능 구현 (P3-001, P3-002, P3-006)

#### 3.1 파일 업로드 API
- [ ] POST `/api/files/upload` 엔드포인트 구현
- [ ] FilesController 클래스 생성 `Controllers/FilesController.cs`
- [ ] 단일 파일 업로드 로직
- [ ] 다중 파일 업로드 로직 (POST `/api/files/upload/multiple`)

#### 3.2 파일 검증 서비스
- [ ] IFileValidationService 인터페이스 정의 `Services/Interfaces/IFileValidationService.cs`
  ```csharp
  public interface IFileValidationService
  {
      /// <summary>
      /// 파일 검증
      /// </summary>
      FileValidationResult Validate(IFormFile file);
      
      /// <summary>
      /// 파일 시그니처 검증 (매직 넘버)
      /// </summary>
      bool ValidateFileSignature(Stream fileStream, string extension);
      
      /// <summary>
      /// 이미지 파일 여부 확인
      /// </summary>
      bool IsImageFile(string contentType);
  }
  ```

- [ ] FileValidationService 클래스 구현 `Services/FileValidationService.cs`
  - 파일 크기 검증 (기본 최대 10MB)
  - 확장자 검증 (화이트리스트 방식)
    - 이미지: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`
    - 문서: `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`
    - 기타: `.txt`, `.zip`, `.rar`
  - 파일 시그니처(매직 넘버) 검증
    - JPEG: `FF D8 FF`
    - PNG: `89 50 4E 47`
    - GIF: `47 49 46 38`
    - PDF: `25 50 44 46`
  - MIME 타입 검증

#### 3.3 파일 업로드 요청/응답 DTO
- [ ] FileUploadRequest DTO (multipart/form-data)
  ```csharp
  public class FileUploadRequest
  {
      [Required]
      public IFormFile File { get; set; } = null!;
      
      public long? PostId { get; set; }
  }
  ```

- [ ] FileUploadResponse DTO
  ```csharp
  public class FileUploadResponse
  {
      public long Id { get; set; }
      public string FileName { get; set; } = string.Empty;
      public string ContentType { get; set; } = string.Empty;
      public long FileSize { get; set; }
      public string? ThumbnailUrl { get; set; }
      public bool IsImage { get; set; }
      public int? Width { get; set; }
      public int? Height { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```

- [ ] MultipleFileUploadResponse DTO
  ```csharp
  public class MultipleFileUploadResponse
  {
      public List<FileUploadResponse> SuccessFiles { get; set; } = new();
      public List<FileUploadError> FailedFiles { get; set; } = new();
  }
  
  public class FileUploadError
  {
      public string FileName { get; set; } = string.Empty;
      public string ErrorCode { get; set; } = string.Empty;
      public string ErrorMessage { get; set; } = string.Empty;
  }
  ```

#### 3.4 파일 업로드 옵션 설정
- [ ] FileUploadOptions 클래스 정의 `Configuration/FileUploadOptions.cs`
  ```csharp
  public class FileUploadOptions
  {
      /// <summary>
      /// 최대 파일 크기 (bytes, 기본 10MB)
      /// </summary>
      public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
      
      /// <summary>
      /// 허용된 확장자 목록
      /// </summary>
      public List<string> AllowedExtensions { get; set; } = new()
      {
          ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
          ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
          ".txt", ".zip", ".rar"
      };
      
      /// <summary>
      /// 허용된 이미지 확장자
      /// </summary>
      public List<string> ImageExtensions { get; set; } = new()
      {
          ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
      };
      
      /// <summary>
      /// 다중 업로드 시 최대 파일 개수
      /// </summary>
      public int MaxFileCount { get; set; } = 10;
      
      /// <summary>
      /// 썸네일 최대 너비
      /// </summary>
      public int ThumbnailMaxWidth { get; set; } = 200;
      
      /// <summary>
      /// 썸네일 최대 높이
      /// </summary>
      public int ThumbnailMaxHeight { get; set; } = 200;
      
      /// <summary>
      /// 저장 경로 (로컬 스토리지)
      /// </summary>
      public string StoragePath { get; set; } = "uploads";
  }
  ```

---

### 4. 썸네일 생성 기능 구현 (P3-003)

#### 4.1 이미지 업로드 시 자동 썸네일 생성
- [ ] 이미지 파일 업로드 감지 로직
- [ ] 비동기 썸네일 생성 로직
- [ ] 썸네일 저장 경로 설정 (`thumbnails/` 하위)
- [ ] 이미지 메타데이터 추출 (Width, Height)

#### 4.2 썸네일 조회 API
- [ ] GET `/api/files/{id}/thumbnail` 엔드포인트 구현
- [ ] 썸네일 없는 경우 404 반환
- [ ] 캐시 헤더 설정 (Cache-Control)

---

### 5. 파일 다운로드 기능 구현 (P3-004)

#### 5.1 파일 다운로드 API
- [ ] GET `/api/files/{id}` 엔드포인트 구현
- [ ] 파일 스트림 반환 로직
- [ ] Content-Disposition 헤더 설정 (attachment; filename="...")
- [ ] 다운로드 횟수 증가 로직
- [ ] 삭제된 파일 404 처리
- [ ] 권한 검증 (선택적)

#### 5.2 파일 정보 조회 API
- [ ] GET `/api/files/{id}/info` 엔드포인트 구현
- [ ] FileInfoResponse DTO
  ```csharp
  public class FileInfoResponse
  {
      public long Id { get; set; }
      public string FileName { get; set; } = string.Empty;
      public string ContentType { get; set; } = string.Empty;
      public long FileSize { get; set; }
      public string FileSizeFormatted { get; set; } = string.Empty; // "1.5 MB"
      public int DownloadCount { get; set; }
      public bool IsImage { get; set; }
      public int? Width { get; set; }
      public int? Height { get; set; }
      public long? PostId { get; set; }
      public long UploaderId { get; set; }
      public string? UploaderName { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```

---

### 6. 파일 삭제 기능 구현 (P3-005)

#### 6.1 파일 삭제 API
- [ ] DELETE `/api/files/{id}` 엔드포인트 구현
- [ ] 소프트 삭제 로직 (IsDeleted = true)
- [ ] 권한 검증 (업로더 또는 관리자만 삭제 가능)
- [ ] 실제 파일 삭제는 배치 처리로 (선택적)

#### 6.2 게시물 파일 목록 조회
- [ ] GET `/api/posts/{postId}/files` 엔드포인트 구현
- [ ] 게시물에 첨부된 파일 목록 반환

---

### 7. 검색 기능 구현 (P3-007 ~ P3-010)

#### 7.1 검색 서비스 인터페이스
- [ ] ISearchService 인터페이스 정의 `Services/Interfaces/ISearchService.cs`
  ```csharp
  public interface ISearchService
  {
      /// <summary>
      /// 게시물 통합 검색
      /// </summary>
      Task<PagedResponse<PostSearchResult>> SearchPostsAsync(SearchParameters parameters);
      
      /// <summary>
      /// 태그 검색
      /// </summary>
      Task<IEnumerable<TagSearchResult>> SearchTagsAsync(string query, int limit = 10);
      
      /// <summary>
      /// 작성자 검색
      /// </summary>
      Task<IEnumerable<AuthorSearchResult>> SearchAuthorsAsync(string query, int limit = 10);
  }
  ```

#### 7.2 검색 서비스 구현
- [ ] SearchService 클래스 구현 `Services/SearchService.cs`
  - SQL LIKE 기반 검색 (기본)
  - 제목 검색
  - 본문 검색
  - 제목 + 본문 통합 검색
  - 대소문자 구분 없음

#### 7.3 검색 요청/응답 DTO
- [ ] SearchParameters DTO
  ```csharp
  public class SearchParameters
  {
      /// <summary>
      /// 검색어 (필수)
      /// </summary>
      [Required]
      [MinLength(2, ErrorMessage = "검색어는 2자 이상이어야 합니다.")]
      public string Query { get; set; } = string.Empty;
      
      /// <summary>
      /// 검색 대상 (title, content, all)
      /// </summary>
      public string SearchIn { get; set; } = "all";
      
      /// <summary>
      /// 카테고리 필터
      /// </summary>
      public string? Category { get; set; }
      
      /// <summary>
      /// 태그 필터
      /// </summary>
      public string? Tag { get; set; }
      
      /// <summary>
      /// 작성자 ID 필터
      /// </summary>
      public long? AuthorId { get; set; }
      
      /// <summary>
      /// 시작 날짜 필터
      /// </summary>
      public DateTime? FromDate { get; set; }
      
      /// <summary>
      /// 종료 날짜 필터
      /// </summary>
      public DateTime? ToDate { get; set; }
      
      /// <summary>
      /// 페이지 번호
      /// </summary>
      public int Page { get; set; } = 1;
      
      /// <summary>
      /// 페이지 크기
      /// </summary>
      public int PageSize { get; set; } = 20;
      
      /// <summary>
      /// 정렬 기준 (relevance, createdAt, viewCount, likeCount)
      /// </summary>
      public string SortBy { get; set; } = "relevance";
      
      /// <summary>
      /// 정렬 순서
      /// </summary>
      public string SortOrder { get; set; } = "desc";
  }
  ```

- [ ] PostSearchResult DTO
  ```csharp
  public class PostSearchResult
  {
      public long Id { get; set; }
      public string Title { get; set; } = string.Empty;
      public string ContentPreview { get; set; } = string.Empty;
      public string? HighlightedTitle { get; set; }
      public string? HighlightedContent { get; set; }
      public string? Category { get; set; }
      public List<string> Tags { get; set; } = new();
      public long AuthorId { get; set; }
      public string? AuthorName { get; set; }
      public int ViewCount { get; set; }
      public int LikeCount { get; set; }
      public int CommentCount { get; set; }
      public DateTime CreatedAt { get; set; }
      public double? RelevanceScore { get; set; }
  }
  ```

- [ ] TagSearchResult DTO
  ```csharp
  public class TagSearchResult
  {
      public string TagName { get; set; } = string.Empty;
      public int PostCount { get; set; }
  }
  ```

- [ ] AuthorSearchResult DTO
  ```csharp
  public class AuthorSearchResult
  {
      public long AuthorId { get; set; }
      public string AuthorName { get; set; } = string.Empty;
      public int PostCount { get; set; }
  }
  ```

#### 7.4 검색 API 엔드포인트
- [ ] SearchController 클래스 생성 `Controllers/SearchController.cs`
- [ ] GET `/api/search?q={query}` - 통합 검색
- [ ] GET `/api/search/posts?q={query}` - 게시물 검색
- [ ] GET `/api/search/tags?q={query}` - 태그 검색
- [ ] GET `/api/search/authors?q={query}` - 작성자 검색

#### 7.5 검색어 하이라이팅 (P3-014)
- [ ] 검색어 하이라이팅 유틸리티 `Utils/SearchHighlighter.cs`
  ```csharp
  public static class SearchHighlighter
  {
      /// <summary>
      /// 검색어를 HTML 태그로 감싸서 하이라이팅
      /// </summary>
      public static string Highlight(string text, string query, string tagName = "mark");
      
      /// <summary>
      /// 검색어 주변 텍스트 추출 (미리보기용)
      /// </summary>
      public static string ExtractPreview(string text, string query, int previewLength = 200);
  }
  ```

---

### 8. 파일 서비스 구현

#### 8.1 파일 서비스 인터페이스
- [ ] IFileService 인터페이스 정의 `Services/Interfaces/IFileService.cs`
  ```csharp
  public interface IFileService
  {
      /// <summary>
      /// 파일 업로드
      /// </summary>
      Task<FileUploadResponse> UploadAsync(IFormFile file, long uploaderId, string? uploaderName = null, long? postId = null);
      
      /// <summary>
      /// 다중 파일 업로드
      /// </summary>
      Task<MultipleFileUploadResponse> UploadMultipleAsync(IEnumerable<IFormFile> files, long uploaderId, string? uploaderName = null, long? postId = null);
      
      /// <summary>
      /// 파일 정보 조회
      /// </summary>
      Task<FileInfoResponse?> GetInfoAsync(long id);
      
      /// <summary>
      /// 파일 스트림 조회
      /// </summary>
      Task<FileStreamResult?> GetStreamAsync(long id);
      
      /// <summary>
      /// 썸네일 스트림 조회
      /// </summary>
      Task<FileStreamResult?> GetThumbnailStreamAsync(long id);
      
      /// <summary>
      /// 파일 삭제
      /// </summary>
      Task<bool> DeleteAsync(long id, long userId, bool isAdmin = false);
      
      /// <summary>
      /// 게시물 파일 목록 조회
      /// </summary>
      Task<IEnumerable<FileInfoResponse>> GetByPostIdAsync(long postId);
      
      /// <summary>
      /// 파일과 게시물 연결
      /// </summary>
      Task<bool> AttachToPostAsync(long fileId, long postId, long userId);
  }
  
  public class FileStreamResult
  {
      public Stream Stream { get; set; } = null!;
      public string FileName { get; set; } = string.Empty;
      public string ContentType { get; set; } = string.Empty;
  }
  ```

#### 8.2 파일 서비스 구현
- [ ] FileService 클래스 구현 `Services/FileService.cs`
  - IFileStorageService 의존성 주입
  - IFileValidationService 의존성 주입
  - IThumbnailService 의존성 주입
  - 파일 업로드/다운로드/삭제 로직

---

### 9. 의존성 주입 설정

#### 9.1 서비스 등록
- [ ] ServiceCollectionExtensions 업데이트
  ```csharp
  // 파일 관련 서비스
  services.AddScoped<IFileService, FileService>();
  services.AddScoped<IFileStorageService, LocalFileStorageService>();
  services.AddScoped<IFileValidationService, FileValidationService>();
  services.AddScoped<IThumbnailService, ThumbnailService>();
  
  // 검색 서비스
  services.AddScoped<ISearchService, SearchService>();
  
  // 파일 업로드 옵션
  services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));
  ```

---

### 10. 테스트 구현

#### 10.1 단위 테스트
- [ ] T3-001: 파일 업로드 성공 테스트
- [ ] T3-002: 파일 업로드 실패 - 크기 초과 테스트
- [ ] T3-003: 파일 업로드 실패 - 확장자 불허 테스트
- [ ] T3-004: 다중 파일 업로드 테스트
- [ ] T3-005: 파일 다운로드 성공 테스트
- [ ] T3-006: 파일 다운로드 실패 - 미존재 테스트
- [ ] T3-007: 파일 삭제 성공 테스트
- [ ] T3-008: 썸네일 조회 테스트
- [ ] T3-009: 제목 검색 테스트
- [ ] T3-010: 본문 검색 테스트
- [ ] T3-011: 태그 검색 테스트
- [ ] T3-012: 복합 조건 검색 테스트
- [ ] T3-013: 검색 결과 페이징 테스트
- [ ] T3-014: 검색 결과 하이라이팅 테스트
- [ ] T3-015: 빈 검색 결과 테스트

#### 10.2 추가 테스트 (선택)
- [ ] 파일 시그니처 검증 테스트
- [ ] 이미지 메타데이터 추출 테스트
- [ ] 썸네일 생성 테스트
- [ ] 권한 검증 테스트

#### 10.3 테스트 파일 목록
```
tests/BoardCommonLibrary.Tests/
├── Services/
│   ├── FileServiceTests.cs
│   ├── FileValidationServiceTests.cs
│   ├── SearchServiceTests.cs
│   └── ThumbnailServiceTests.cs
├── Validators/
│   └── FileValidatorsTests.cs
└── Utils/
    └── SearchHighlighterTests.cs
```

#### 10.4 통합 테스트
- [ ] TestPage3Controller 업데이트
- [ ] 파일 업로드/다운로드 E2E 테스트
- [ ] 검색 API 통합 테스트

---

### 11. 문서화

#### 11.1 API 문서
- [ ] Swagger/OpenAPI 문서 작성
  - 파일 업로드 API (multipart/form-data)
  - 파일 다운로드 API
  - 검색 API
- [ ] API 사용 예제 작성

#### 11.2 코드 문서
- [ ] 주요 클래스 및 메서드 XML 주석 작성
- [ ] README 업데이트

---

## 📅 작업 일정 (예상)

| 단계 | 작업 내용 | 예상 소요 시간 | 상태 |
|-----|----------|--------------|------|
| 1단계 | 데이터 모델 설계 및 구현 | 3시간 | 🔴 대기 |
| 2단계 | 스토리지 추상화 및 로컬 스토리지 구현 | 4시간 | 🔴 대기 |
| 3단계 | 파일 검증 서비스 구현 | 3시간 | 🔴 대기 |
| 4단계 | 파일 업로드 API 구현 (P3-001, P3-002, P3-006) | 5시간 | 🔴 대기 |
| 5단계 | 썸네일 생성 기능 구현 (P3-003) | 4시간 | 🔴 대기 |
| 6단계 | 파일 다운로드/삭제 API 구현 (P3-004, P3-005) | 3시간 | 🔴 대기 |
| 7단계 | 검색 서비스 구현 (P3-007 ~ P3-010) | 6시간 | 🔴 대기 |
| 8단계 | 검색어 하이라이팅 구현 | 2시간 | 🔴 대기 |
| 9단계 | 테스트 작성 및 검증 | 6시간 | 🔴 대기 |
| 10단계 | 문서화 | 2시간 | 🔴 대기 |
| **합계** | | **38시간** | **0%** |

---

## ✅ 완료 기준

### 기능 완료 기준
- [ ] 모든 API 엔드포인트 구현 완료 (10개)
- [ ] 파일 크기, 확장자, 시그니처 검증 로직 정상 동작
- [ ] 이미지 파일 자동 썸네일 생성
- [ ] 검색 기능 정상 동작 (제목, 본문, 태그)
- [ ] 검색어 하이라이팅 기능

### 테스트 완료 기준
- [ ] 모든 테스트 케이스 통과 (최소 15개)
- [ ] 테스트 커버리지 80% 이상

### 문서화 완료 기준
- [ ] API 문서 작성 완료 (Swagger)
- [ ] 코드 주석 작성 완료

---

## 📁 생성 예정 파일 목록

### 라이브러리 (src/BoardCommonLibrary/)
```
├── Configuration/
│   └── FileUploadOptions.cs           # 파일 업로드 설정
├── Controllers/
│   ├── FilesController.cs             # 파일 API 컨트롤러
│   └── SearchController.cs            # 검색 API 컨트롤러
├── DTOs/
│   ├── FileRequests.cs                # 파일 요청 DTO
│   ├── FileResponses.cs               # 파일 응답 DTO
│   ├── SearchRequests.cs              # 검색 요청 DTO
│   └── SearchResponses.cs             # 검색 응답 DTO
├── Entities/
│   └── FileAttachment.cs              # 파일 엔티티
├── Services/
│   ├── Interfaces/
│   │   ├── IFileService.cs
│   │   ├── IFileStorageService.cs
│   │   ├── IFileValidationService.cs
│   │   ├── IThumbnailService.cs
│   │   └── ISearchService.cs
│   ├── FileService.cs
│   ├── LocalFileStorageService.cs
│   ├── FileValidationService.cs
│   ├── ThumbnailService.cs
│   └── SearchService.cs
├── Utils/
│   └── SearchHighlighter.cs           # 검색어 하이라이팅 유틸리티
└── Validators/
    └── FileValidators.cs              # 파일 검증기 (선택적)
```

### 테스트 웹서비스 (test-web/BoardTestWeb/)
```
├── Controllers/
│   └── TestPage3Controller.cs         # 업데이트
```

### 단위 테스트 (tests/BoardCommonLibrary.Tests/)
```
├── Services/
│   ├── FileServiceTests.cs
│   ├── FileValidationServiceTests.cs
│   ├── SearchServiceTests.cs
│   └── ThumbnailServiceTests.cs
├── Utils/
│   └── SearchHighlighterTests.cs
└── Validators/
    └── FileValidatorsTests.cs
```

---

## ⚠️ 주의사항

### 파일 업로드 보안
1. **파일 확장자 검증**: 화이트리스트 방식 사용
2. **파일 시그니처 검증**: 매직 넘버로 실제 파일 타입 확인
3. **파일명 새니타이징**: Path Traversal 공격 방지
4. **저장 파일명**: UUID 기반으로 생성 (원본 파일명 노출 방지)
5. **업로드 경로**: 웹 루트 외부에 저장

### 검색 보안
1. **SQL Injection 방지**: 파라미터화된 쿼리 사용
2. **XSS 방지**: 하이라이팅 시 HTML 이스케이프
3. **Rate Limiting**: 검색 요청 제한 (선택적)

### 성능 고려사항
1. **대용량 파일 처리**: 스트리밍 방식 사용
2. **썸네일 캐싱**: 생성된 썸네일 재사용
3. **검색 인덱스**: 필요 시 Full-Text Search 적용
4. **페이징**: 검색 결과 페이징 필수

---

## 🔗 관련 문서

- [PAGES.md](./PAGES.md) - 전체 페이지 기능 명세서
- [PRD.md](./PRD.md) - 제품 요구사항 문서
- [TESTING.md](./TESTING.md) - 테스트 가이드
- [page1task.md](./page1task.md) - 페이지 1 작업 명세서
- [page2task.md](./page2task.md) - 페이지 2 작업 명세서

---

*최종 업데이트: 2025-11-29*
