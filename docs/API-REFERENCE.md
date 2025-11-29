# BoardCommonLibrary API 레퍼런스

## 📖 개요

이 문서는 BoardCommonLibrary의 모든 API 엔드포인트에 대한 상세 레퍼런스입니다.

---

## 🔐 인증 헤더

API 호출 시 다음 헤더를 사용하여 사용자 정보를 전달합니다:

| 헤더 | 설명 | 필수 |
|-----|------|------|
| `X-User-Id` | 사용자 ID (숫자) | 인증 필요 API |
| `X-User-Name` | 사용자 이름 | 선택 |
| `X-Is-Admin` | 관리자 여부 (`true`/`false`) | 관리자 API |

---

## 📝 게시물 API

### 게시물 목록 조회

```http
GET /api/posts
```

**쿼리 파라미터:**

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `page` | int | 1 | 페이지 번호 |
| `pageSize` | int | 20 | 페이지 크기 (최대 100) |
| `sortBy` | string | "createdAt" | 정렬 기준 (`createdAt`, `viewCount`, `likeCount`, `commentCount`) |
| `sortOrder` | string | "desc" | 정렬 순서 (`asc`, `desc`) |
| `category` | string | null | 카테고리 필터 |
| `tag` | string | null | 태그 필터 |
| `authorId` | long | null | 작성자 ID 필터 |
| `status` | string | null | 상태 필터 (`Published`, `Draft`, `Archived`) |
| `search` | string | null | 검색어 (제목, 내용) |

**응답:** `200 OK`

```json
{
  "data": [
    {
      "id": 1,
      "title": "게시물 제목",
      "contentPreview": "게시물 내용 미리보기 (200자)...",
      "category": "공지",
      "tags": ["태그1", "태그2"],
      "authorId": 1,
      "authorName": "작성자",
      "viewCount": 100,
      "likeCount": 10,
      "commentCount": 5,
      "isPinned": false,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

---

### 게시물 상세 조회

```http
GET /api/posts/{id}
```

**경로 파라미터:**

| 파라미터 | 타입 | 설명 |
|---------|------|------|
| `id` | long | 게시물 ID |

**응답:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "게시물 제목",
    "content": "게시물 전체 내용",
    "category": "공지",
    "tags": ["태그1", "태그2"],
    "authorId": 1,
    "authorName": "작성자",
    "status": "Published",
    "viewCount": 101,
    "likeCount": 10,
    "commentCount": 5,
    "isPinned": false,
    "isDraft": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-02T00:00:00Z",
    "publishedAt": "2024-01-01T00:00:00Z"
  }
}
```

**에러 응답:** `404 Not Found`

```json
{
  "success": false,
  "code": "POST_NOT_FOUND",
  "message": "게시물을 찾을 수 없습니다."
}
```

---

### 게시물 작성

```http
POST /api/posts
```

**헤더:**

| 헤더 | 필수 | 설명 |
|-----|------|------|
| `X-User-Id` | ✅ | 작성자 ID |
| `X-User-Name` | ⬜ | 작성자 이름 |

**요청 본문:**

```json
{
  "title": "게시물 제목",
  "content": "게시물 내용",
  "category": "일반",
  "tags": ["태그1", "태그2"]
}
```

| 필드 | 타입 | 필수 | 검증 규칙 |
|-----|------|------|----------|
| `title` | string | ✅ | 1-200자 |
| `content` | string | ✅ | 1자 이상 |
| `category` | string | ⬜ | 최대 50자 |
| `tags` | string[] | ⬜ | 최대 10개 |

**응답:** `201 Created`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "게시물 제목",
    "content": "게시물 내용",
    ...
  }
}
```

**에러 응답:** `400 Bad Request`

```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "유효성 검증에 실패했습니다.",
  "errors": [
    { "field": "title", "message": "제목은 필수입니다." },
    { "field": "title", "message": "제목은 200자 이내여야 합니다." }
  ]
}
```

---

### 게시물 수정

```http
PUT /api/posts/{id}
```

**헤더:**

| 헤더 | 필수 | 설명 |
|-----|------|------|
| `X-User-Id` | ✅ | 현재 사용자 ID (작성자 또는 관리자) |
| `X-Is-Admin` | ⬜ | 관리자 여부 |

**요청 본문:**

```json
{
  "title": "수정된 제목",
  "content": "수정된 내용",
  "category": "수정된 카테고리",
  "tags": ["새태그1", "새태그2"]
}
```

> 💡 수정하고 싶은 필드만 포함하면 됩니다.

**응답:** `200 OK` | `403 Forbidden` | `404 Not Found`

---

### 게시물 삭제

```http
DELETE /api/posts/{id}
```

**헤더:**

| 헤더 | 필수 | 설명 |
|-----|------|------|
| `X-User-Id` | ✅ | 현재 사용자 ID |
| `X-Is-Admin` | ⬜ | 관리자 여부 |

**응답:** `204 No Content` | `403 Forbidden` | `404 Not Found`

---

### 게시물 상단고정

```http
POST /api/posts/{id}/pin
```

**응답:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "isPinned": true,
    ...
  }
}
```

---

### 게시물 상단고정 해제

```http
DELETE /api/posts/{id}/pin
```

**응답:** `200 OK`

---

### 임시저장

```http
POST /api/posts/draft
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**요청 본문:**

```json
{
  "title": "임시 제목",
  "content": "임시 내용",
  "category": "카테고리",
  "tags": ["태그"]
}
```

> 💡 임시저장은 모든 필드가 선택적입니다.

**응답:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "임시 제목",
    "content": "임시 내용",
    "isDraft": true,
    ...
  }
}
```

---

### 임시저장 목록 조회

```http
GET /api/posts/draft
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**쿼리 파라미터:**

| 파라미터 | 타입 | 기본값 |
|---------|------|--------|
| `page` | int | 1 |
| `pageSize` | int | 20 |

**응답:** `200 OK` - 해당 사용자의 임시저장 목록

---

### 임시저장 발행

```http
POST /api/posts/draft/{id}/publish
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**응답:** `200 OK` - 발행된 게시물 정보

---

### 게시물 좋아요

```http
POST /api/posts/{id}/like
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**응답:** `200 OK`

```json
{
  "success": true,
  "data": {
    "targetId": 1,
    "targetType": "Post",
    "likeCount": 11,
    "isLiked": true
  }
}
```

---

### 게시물 좋아요 취소

```http
DELETE /api/posts/{id}/like
```

**응답:** `200 OK`

```json
{
  "success": true,
  "data": {
    "targetId": 1,
    "targetType": "Post",
    "likeCount": 10,
    "isLiked": false
  }
}
```

---

### 게시물 북마크 추가

```http
POST /api/posts/{id}/bookmark
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**응답:** `200 OK`

```json
{
  "success": true,
  "message": "북마크가 추가되었습니다."
}
```

---

### 게시물 북마크 해제

```http
DELETE /api/posts/{id}/bookmark
```

**응답:** `200 OK`

---

## 💬 댓글 API

### 댓글 목록 조회

```http
GET /api/posts/{postId}/comments
```

**쿼리 파라미터:**

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `page` | int | 1 | 페이지 번호 |
| `pageSize` | int | 20 | 페이지 크기 |
| `sortBy` | string | "createdAt" | 정렬 기준 |
| `sortOrder` | string | "asc" | 정렬 순서 |
| `includeReplies` | bool | true | 대댓글 포함 여부 |

**응답:** `200 OK`

```json
{
  "data": [
    {
      "id": 1,
      "content": "댓글 내용",
      "postId": 1,
      "authorId": 2,
      "authorName": "댓글 작성자",
      "parentId": null,
      "likeCount": 5,
      "replyCount": 2,
      "isDeleted": false,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "replies": [
        {
          "id": 2,
          "content": "대댓글 내용",
          "parentId": 1,
          ...
        }
      ]
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 10,
    "totalPages": 1
  }
}
```

---

### 댓글 작성

```http
POST /api/posts/{postId}/comments
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |
| `X-User-Name` | ⬜ |

**요청 본문:**

```json
{
  "content": "댓글 내용입니다."
}
```

| 필드 | 타입 | 필수 | 검증 규칙 |
|-----|------|------|----------|
| `content` | string | ✅ | 1-2000자 |

**응답:** `201 Created`

---

### 댓글 수정

```http
PUT /api/comments/{id}
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**요청 본문:**

```json
{
  "content": "수정된 댓글 내용"
}
```

**응답:** `200 OK` | `403 Forbidden` (본인 댓글만 수정 가능)

---

### 댓글 삭제

```http
DELETE /api/comments/{id}
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |
| `X-Is-Admin` | ⬜ |

**응답:** `204 No Content`

> 💡 대댓글이 있는 댓글은 내용만 삭제 처리되고 구조는 유지됩니다.

---

### 대댓글 작성

```http
POST /api/comments/{parentId}/replies
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |
| `X-User-Name` | ⬜ |

**요청 본문:**

```json
{
  "content": "대댓글 내용입니다."
}
```

**응답:** `201 Created`

---

### 대댓글 목록 조회

```http
GET /api/comments/{parentId}/replies
```

**응답:** `200 OK` - 해당 댓글의 대댓글 목록

---

### 댓글 좋아요

```http
POST /api/comments/{id}/like
```

**응답:** `200 OK`

---

### 댓글 좋아요 취소

```http
DELETE /api/comments/{id}/like
```

**응답:** `200 OK`

---

## 👤 사용자 API

### 내 북마크 목록 조회

```http
GET /api/users/me/bookmarks
```

**헤더:**

| 헤더 | 필수 |
|-----|------|
| `X-User-Id` | ✅ |

**쿼리 파라미터:**

| 파라미터 | 타입 | 기본값 |
|---------|------|--------|
| `page` | int | 1 |
| `pageSize` | int | 20 |

**응답:** `200 OK`

```json
{
  "data": [
    {
      "id": 1,
      "postId": 1,
      "postTitle": "북마크한 게시물",
      "postAuthorName": "작성자",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 5,
    "totalPages": 1
  }
}
```

---

## ⚠️ 에러 코드

| 코드 | HTTP 상태 | 설명 |
|-----|----------|------|
| `POST_NOT_FOUND` | 404 | 게시물을 찾을 수 없음 |
| `COMMENT_NOT_FOUND` | 404 | 댓글을 찾을 수 없음 |
| `VALIDATION_ERROR` | 400 | 유효성 검증 실패 |
| `FORBIDDEN` | 403 | 권한 없음 |
| `DUPLICATE_LIKE` | 409 | 이미 좋아요한 상태 |
| `DUPLICATE_BOOKMARK` | 409 | 이미 북마크한 상태 |
| `NOT_LIKED` | 400 | 좋아요하지 않은 상태 |
| `NOT_BOOKMARKED` | 400 | 북마크하지 않은 상태 |

---

## 📊 상태 열거형

### PostStatus
```csharp
public enum PostStatus
{
    Draft = 0,      // 임시저장
    Published = 1,  // 발행됨
    Archived = 2,   // 보관됨
    Deleted = 3     // 삭제됨
}
```

### LikeTargetType
```csharp
public enum LikeTargetType
{
    Post = 0,    // 게시물
    Comment = 1  // 댓글
}
```

---

*최종 업데이트: 2024-11-29*
