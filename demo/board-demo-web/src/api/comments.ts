import api from './client';
import { Comment, CreateCommentRequest } from '../types';

// API 응답 래퍼 타입
interface ApiArrayResponse<T> {
  success: boolean;
  data: T[];
}

interface ApiSingleResponse<T> {
  success: boolean;
  data: T;
}

export const commentsApi = {
  // 게시물의 댓글 목록 조회
  getByPostId: async (postId: number): Promise<Comment[]> => {
    const response = await api.get<ApiArrayResponse<Comment>>(`/posts/${postId}/comments`);
    return response.data.data || [];
  },

  // 댓글 작성
  create: async (postId: number, data: CreateCommentRequest): Promise<Comment> => {
    const response = await api.post<ApiSingleResponse<Comment>>(`/posts/${postId}/comments`, data);
    return response.data.data;
  },

  // 댓글 수정
  update: async (id: number, content: string): Promise<Comment> => {
    const response = await api.put<ApiSingleResponse<Comment>>(`/comments/${id}`, { content });
    return response.data.data;
  },

  // 댓글 삭제
  delete: async (id: number): Promise<void> => {
    await api.delete(`/comments/${id}`);
  },

  // 대댓글 작성
  createReply: async (commentId: number, content: string): Promise<Comment> => {
    const response = await api.post<ApiSingleResponse<Comment>>(`/comments/${commentId}/replies`, { content });
    return response.data.data;
  },

  // 댓글 좋아요 토글
  toggleLike: async (id: number): Promise<{ liked: boolean; likeCount: number }> => {
    const response = await api.post(`/comments/${id}/like`);
    return response.data;
  },
};
