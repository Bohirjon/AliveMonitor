import apiClient from './client';
import type { User } from '@/types';

export async function getProfile(): Promise<User> {
  const { data } = await apiClient.get<User>('/settings/profile');
  return data;
}

export async function updateAlertEmail(alertEmail: string): Promise<void> {
  await apiClient.put('/settings/alert-email', { alertEmail });
}
