import { apiClient, unwrap } from './axiosClient';
import type { AuthResponse, LoginRequest } from '../types/auth';

export const authApi = {
  login: (payload: LoginRequest) =>
    unwrap<AuthResponse>(apiClient.post('/auth/login', payload)),

  me: () => unwrap(apiClient.get('/auth/me')),

  logout: (refreshToken: string) =>
    unwrap(apiClient.post('/auth/logout', { refreshToken })),
};
