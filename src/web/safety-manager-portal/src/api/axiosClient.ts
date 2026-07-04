import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { API_BASE_URL } from '../constants/filters';
import {
  clearAuth,
  getAccessToken,
  getRefreshToken,
  getStoredUser,
  isRememberMe,
  saveAuth,
} from '../auth/authStorage';
import type { ApiResponse } from '../types/api';
import type { AuthResponse } from '../types/auth';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { Accept: 'application/json' },
});

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;

  const { data } = await axios.post<ApiResponse<AuthResponse>>(
    `${API_BASE_URL}/auth/refresh`,
    { refreshToken },
  );

  const auth = data.data;
  const user = getStoredUser() ?? auth.user;
  saveAuth(auth.accessToken, auth.refreshToken, user, isRememberMe());
  return auth.accessToken;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (error.response?.status !== 401 || !original || original._retry) {
      return Promise.reject(error);
    }

    original._retry = true;

    if (!refreshPromise) {
      refreshPromise = refreshAccessToken()
        .catch(() => {
          clearAuth();
          window.location.href = '/login';
          return null;
        })
        .finally(() => {
          refreshPromise = null;
        });
    }

    const newToken = await refreshPromise;
    if (!newToken) return Promise.reject(error);

    original.headers.Authorization = `Bearer ${newToken}`;
    return apiClient(original);
  },
);

export async function unwrap<T>(promise: Promise<{ data: ApiResponse<T> }>): Promise<T> {
  const { data } = await promise;
  return data.data;
}
