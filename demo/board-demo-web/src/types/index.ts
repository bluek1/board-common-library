// 사용자 관련 타입
export interface User {
  id: number;
  username: string;
  email: string;
  displayName?: string;
  role: string;
  createdAt: string;
}

export interface AuthResponse {
  success: boolean;
  message?: string;
  user?: User;
  tokens?: TokenDto;
}

export interface TokenDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpires: string;
  refreshTokenExpires: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  displayName?: string;
}

// 게시물 관련 타입
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
  publishedAt?: string;
}

export enum PostStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
  Deleted = 3,
}

export interface CreatePostRequest {
  title: string;
  content: string;
  category?: string;
  tags?: string[];
}

export interface UpdatePostRequest {
  title?: string;
  content?: string;
  category?: string;
  tags?: string[];
}

// 댓글 관련 타입
export interface Comment {
  id: number;
  content: string;
  postId: number;
  authorId: number;
  authorName?: string;
  parentId?: number;
  likeCount: number;
  createdAt: string;
  updatedAt?: string;
  replies?: Comment[];
}

export interface CreateCommentRequest {
  content: string;
  parentId?: number;
}

// Q&A 관련 타입
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

export interface Answer {
  id: number;
  content: string;
  questionId: number;
  authorId: number;
  authorName?: string;
  voteCount: number;
  isAccepted: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateQuestionRequest {
  title: string;
  content: string;
  tags?: string[];
}

export interface CreateAnswerRequest {
  content: string;
}

// 페이징 관련 타입
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  sort?: string;
  order?: 'asc' | 'desc';
  search?: string;
  category?: string;
  status?: string;
}

// API 응답 타입
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: ApiError;
}

export interface ApiError {
  code: string;
  message: string;
  details?: ValidationError[];
}

export interface ValidationError {
  field: string;
  message: string;
}
