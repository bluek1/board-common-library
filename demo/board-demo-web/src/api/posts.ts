import api from './client';
import { Post, CreatePostRequest, UpdatePostRequest, PagedResult, QueryParams } from '../types';

export const postsApi = {
  // 게시물 목록 조회
  getAll: async (params?: QueryParams): Promise<PagedResult<Post>> => {
    const response = await api.get<PagedResult<Post>>('/posts', { params });
    return response.data;
  },

  // 게시물 상세 조회
  getById: async (id: number): Promise<Post> => {
    const response = await api.get<Post>(`/posts/${id}`);
    return response.data;
  },

  // 게시물 작성
  create: async (data: CreatePostRequest): Promise<Post> => {
    const response = await api.post<Post>('/posts', data);
    return response.data;
  },

  // 게시물 수정
  update: async (id: number, data: UpdatePostRequest): Promise<Post> => {
    const response = await api.put<Post>(`/posts/${id}`, data);
    return response.data;
  },

  // 게시물 삭제
  delete: async (id: number): Promise<void> => {
    await api.delete(`/posts/${id}`);
  },

  // 좋아요 토글
  toggleLike: async (id: number): Promise<{ liked: boolean; likeCount: number }> => {
    const response = await api.post(`/posts/${id}/like`);
    return response.data;
  },

  // 상단고정 설정
  pin: async (id: number): Promise<Post> => {
    const response = await api.post<Post>(`/posts/${id}/pin`);
    return response.data;
  },

  // 상단고정 해제
  unpin: async (id: number): Promise<Post> => {
    const response = await api.delete<Post>(`/posts/${id}/pin`);
    return response.data;
  },

  // 임시저장
  saveDraft: async (data: CreatePostRequest): Promise<Post> => {
    const response = await api.post<Post>('/posts/draft', data);
    return response.data;
  },

  // 임시저장 목록
  getDrafts: async (): Promise<Post[]> => {
    const response = await api.get<Post[]>('/posts/draft');
    return response.data;
  },
};
