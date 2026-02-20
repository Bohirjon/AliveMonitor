import apiClient from './client';
import type { AuthTokens, User } from '@/types';

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export async function googleSignIn(idToken: string): Promise<{ tokens: AuthTokens; user: User }> {
  const { data } = await apiClient.post<AuthResponse>('/auth/google', { idToken });
  return {
    tokens: { accessToken: data.accessToken, refreshToken: data.refreshToken },
    user: data.user,
  };
}

export async function revokeToken(refreshToken: string): Promise<void> {
  await apiClient.post('/auth/revoke', { refreshToken });
}
