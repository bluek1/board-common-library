import api from './client';
import { Question, Answer, CreateQuestionRequest, CreateAnswerRequest, PagedResult, QueryParams } from '../types';

// API 응답 래퍼 타입
interface ApiPagedResponse<T> {
  success: boolean;
  data: T[];
  meta: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

interface ApiSingleResponse<T> {
  success: boolean;
  data: T;
}

interface ApiArrayResponse<T> {
  success: boolean;
  data: T[];
}

export const questionsApi = {
  // 질문 목록 조회
  getAll: async (params?: QueryParams): Promise<PagedResult<Question>> => {
    const response = await api.get<ApiPagedResponse<Question>>('/questions', { params });
    const { data, meta } = response.data;
    return {
      items: data || [],
      page: meta.page,
      pageSize: meta.pageSize,
      totalCount: meta.totalCount,
      totalPages: meta.totalPages,
    };
  },

  // 질문 상세 조회
  getById: async (id: number): Promise<Question> => {
    const response = await api.get<ApiSingleResponse<Question>>(`/questions/${id}`);
    return response.data.data;
  },

  // 질문 작성
  create: async (data: CreateQuestionRequest): Promise<Question> => {
    const response = await api.post<ApiSingleResponse<Question>>('/questions', data);
    return response.data.data;
  },

  // 질문 수정
  update: async (id: number, data: Partial<CreateQuestionRequest>): Promise<Question> => {
    const response = await api.put<ApiSingleResponse<Question>>(`/questions/${id}`, data);
    return response.data.data;
  },

  // 질문 삭제
  delete: async (id: number): Promise<void> => {
    await api.delete(`/questions/${id}`);
  },

  // 질문 추천
  vote: async (id: number): Promise<{ voted: boolean; voteCount: number }> => {
    const response = await api.post(`/questions/${id}/vote`);
    return response.data;
  },

  // 질문 종료
  close: async (id: number): Promise<Question> => {
    const response = await api.post<ApiSingleResponse<Question>>(`/questions/${id}/close`);
    return response.data.data;
  },
};

export const answersApi = {
  // 질문의 답변 목록 조회
  getByQuestionId: async (questionId: number): Promise<Answer[]> => {
    const response = await api.get<ApiArrayResponse<Answer>>(`/questions/${questionId}/answers`);
    return response.data.data || [];
  },

  // 답변 작성
  create: async (questionId: number, data: CreateAnswerRequest): Promise<Answer> => {
    const response = await api.post<ApiSingleResponse<Answer>>(`/questions/${questionId}/answers`, data);
    return response.data.data;
  },

  // 답변 수정
  update: async (id: number, content: string): Promise<Answer> => {
    const response = await api.put<ApiSingleResponse<Answer>>(`/answers/${id}`, { content });
    return response.data.data;
  },

  // 답변 삭제
  delete: async (id: number): Promise<void> => {
    await api.delete(`/answers/${id}`);
  },

  // 답변 채택
  accept: async (id: number): Promise<Answer> => {
    const response = await api.post<ApiSingleResponse<Answer>>(`/answers/${id}/accept`);
    return response.data.data;
  },

  // 답변 추천
  vote: async (id: number): Promise<{ voted: boolean; voteCount: number }> => {
    const response = await api.post(`/answers/${id}/vote`);
    return response.data;
  },
};
