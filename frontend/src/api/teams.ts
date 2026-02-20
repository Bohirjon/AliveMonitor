import apiClient from './client';
import type { Team, CreateTeamRequest } from '@/types';

export async function getTeams(): Promise<Team[]> {
  const { data } = await apiClient.get<Team[]>('/teams');
  return data;
}

export async function getTeam(id: string): Promise<Team> {
  const { data } = await apiClient.get<Team>(`/teams/${id}`);
  return data;
}

export async function createTeam(request: CreateTeamRequest): Promise<Team> {
  const { data } = await apiClient.post<Team>('/teams', request);
  return data;
}

export async function updateTeam(id: string, request: CreateTeamRequest): Promise<Team> {
  const { data } = await apiClient.put<Team>(`/teams/${id}`, request);
  return data;
}

export async function deleteTeam(id: string): Promise<void> {
  await apiClient.delete(`/teams/${id}`);
}
