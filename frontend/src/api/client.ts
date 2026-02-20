import axios from 'axios';
import type { AuthTokens } from '@/types';

const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const stored = localStorage.getItem('tokens');
  if (stored) {
    const tokens: AuthTokens = JSON.parse(stored);
    config.headers.Authorization = `Bearer ${tokens.accessToken}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const stored = localStorage.getItem('tokens');
        if (!stored) throw new Error('No tokens');

        const tokens: AuthTokens = JSON.parse(stored);
        const { data } = await axios.post<AuthTokens>('/api/auth/refresh', {
          refreshToken: tokens.refreshToken,
        });

        localStorage.setItem('tokens', JSON.stringify(data));
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return apiClient(originalRequest);
      } catch {
        localStorage.removeItem('tokens');
        localStorage.removeItem('user');
        window.location.href = '/signin';
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  },
);

export default apiClient;
