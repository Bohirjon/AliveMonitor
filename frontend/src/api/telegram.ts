import apiClient from './client';
import type { LinkCodeResponse, TelegramStatusResponse } from '@/types';

export async function generateLinkCode(teamId?: string): Promise<LinkCodeResponse> {
  const { data } = await apiClient.post<LinkCodeResponse>('/telegram/link-code', { teamId });
  return data;
}

export async function getTelegramStatus(): Promise<TelegramStatusResponse> {
  const { data } = await apiClient.get<TelegramStatusResponse>('/telegram/status');
  return data;
}

export async function getTeamTelegramStatus(teamId: string): Promise<TelegramStatusResponse> {
  const { data } = await apiClient.get<TelegramStatusResponse>(`/telegram/status/team/${teamId}`);
  return data;
}

export async function unlinkTelegram(): Promise<void> {
  await apiClient.delete('/telegram/unlink');
}

export async function unlinkTeamTelegram(teamId: string): Promise<void> {
  await apiClient.delete(`/telegram/unlink/team/${teamId}`);
}
