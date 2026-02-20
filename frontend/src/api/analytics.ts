import apiClient from './client';
import type { AnalyticsSummary, HealthCheckLog, Incident, PaginatedResponse } from '@/types';

export async function getAnalytics(endpointId: string, from?: string, to?: string): Promise<AnalyticsSummary> {
  const params = new URLSearchParams();
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  const { data } = await apiClient.get<AnalyticsSummary>(`/endpoints/${endpointId}/analytics?${params}`);
  return data;
}

export async function getCheckLogs(
  endpointId: string, from?: string, to?: string, page = 1, pageSize = 20,
): Promise<PaginatedResponse<HealthCheckLog>> {
  const params = new URLSearchParams();
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  params.set('page', page.toString());
  params.set('pageSize', pageSize.toString());
  const { data } = await apiClient.get<PaginatedResponse<HealthCheckLog>>(`/endpoints/${endpointId}/checks?${params}`);
  return data;
}

export async function getIncidents(endpointId: string, from?: string, to?: string): Promise<Incident[]> {
  const params = new URLSearchParams();
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  const { data } = await apiClient.get<Incident[]>(`/endpoints/${endpointId}/incidents?${params}`);
  return data;
}
