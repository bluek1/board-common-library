import api, { TokenStorage } from './client';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../types';

export const authApi = {
  // 로그인
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', data);
    if (response.data.success && response.data.tokens) {
      TokenStorage.setAccessToken(response.data.tokens.accessToken);
      TokenStorage.setRefreshToken(response.data.tokens.refreshToken);
    }
    return response.data;
  },

  // 회원가입
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/register', data);
    if (response.data.success && response.data.tokens) {
      TokenStorage.setAccessToken(response.data.tokens.accessToken);
      TokenStorage.setRefreshToken(response.data.tokens.refreshToken);
    }
    return response.data;
  },

  // 로그아웃
  logout: async (): Promise<void> => {
    const refreshToken = TokenStorage.getRefreshToken();
    if (refreshToken) {
      try {
        await api.post('/auth/logout', { refreshToken });
      } catch (error) {
        console.error('Logout error:', error);
      }
    }
    TokenStorage.clearTokens();
  },

  // 현재 사용자 정보 조회
  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await api.get<User>('/auth/me');
      return response.data;
    } catch (error) {
      return null;
    }
  },

  // 토큰 갱신
  refreshToken: async (): Promise<AuthResponse> => {
    const refreshToken = TokenStorage.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token');
    }
    const response = await api.post<AuthResponse>('/auth/refresh', { refreshToken });
    if (response.data.success && response.data.tokens) {
      TokenStorage.setAccessToken(response.data.tokens.accessToken);
      TokenStorage.setRefreshToken(response.data.tokens.refreshToken);
    }
    return response.data;
  },

  // 토큰 유효성 검증
  validateToken: async (): Promise<boolean> => {
    try {
      await api.get('/auth/validate');
      return true;
    } catch (error) {
      return false;
    }
  },
};
