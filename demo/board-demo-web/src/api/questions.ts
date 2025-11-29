import api from './client';
import { Question, Answer, CreateQuestionRequest, CreateAnswerRequest, PagedResult, QueryParams } from '../types';

export const questionsApi = {
  // 질문 목록 조회
  getAll: async (params?: QueryParams): Promise<PagedResult<Question>> => {
    const response = await api.get<PagedResult<Question>>('/questions', { params });
    return response.data;
  },

  // 질문 상세 조회
  getById: async (id: number): Promise<Question> => {
    const response = await api.get<Question>(`/questions/${id}`);
    return response.data;
  },

  // 질문 작성
  create: async (data: CreateQuestionRequest): Promise<Question> => {
    const response = await api.post<Question>('/questions', data);
    return response.data;
  },

  // 질문 수정
  update: async (id: number, data: Partial<CreateQuestionRequest>): Promise<Question> => {
    const response = await api.put<Question>(`/questions/${id}`, data);
    return response.data;
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
    const response = await api.post<Question>(`/questions/${id}/close`);
    return response.data;
  },
};

export const answersApi = {
  // 질문의 답변 목록 조회
  getByQuestionId: async (questionId: number): Promise<Answer[]> => {
    const response = await api.get<Answer[]>(`/questions/${questionId}/answers`);
    return response.data;
  },

  // 답변 작성
  create: async (questionId: number, data: CreateAnswerRequest): Promise<Answer> => {
    const response = await api.post<Answer>(`/questions/${questionId}/answers`, data);
    return response.data;
  },

  // 답변 수정
  update: async (id: number, content: string): Promise<Answer> => {
    const response = await api.put<Answer>(`/answers/${id}`, { content });
    return response.data;
  },

  // 답변 삭제
  delete: async (id: number): Promise<void> => {
    await api.delete(`/answers/${id}`);
  },

  // 답변 채택
  accept: async (id: number): Promise<Answer> => {
    const response = await api.post<Answer>(`/answers/${id}/accept`);
    return response.data;
  },

  // 답변 추천
  vote: async (id: number): Promise<{ voted: boolean; voteCount: number }> => {
    const response = await api.post(`/answers/${id}/vote`);
    return response.data;
  },
};
