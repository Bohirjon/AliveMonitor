import apiClient from './client';
import type { MonitoredEndpoint, CreateEndpointRequest } from '@/types';

export async function getEndpoints(search?: string, status?: string): Promise<MonitoredEndpoint[]> {
  const params = new URLSearchParams();
  if (search) params.set('search', search);
  if (status && status !== 'All') params.set('status', status);
  const { data } = await apiClient.get<MonitoredEndpoint[]>(`/endpoints?${params}`);
  return data;
}

export async function getEndpoint(id: string): Promise<MonitoredEndpoint> {
  const { data } = await apiClient.get<MonitoredEndpoint>(`/endpoints/${id}`);
  return data;
}

export async function createEndpoint(request: CreateEndpointRequest): Promise<MonitoredEndpoint> {
  const { data } = await apiClient.post<MonitoredEndpoint>('/endpoints', request);
  return data;
}

export async function updateEndpoint(id: string, request: CreateEndpointRequest): Promise<MonitoredEndpoint> {
  const { data } = await apiClient.put<MonitoredEndpoint>(`/endpoints/${id}`, request);
  return data;
}

export async function deleteEndpoint(id: string): Promise<void> {
  await apiClient.delete(`/endpoints/${id}`);
}

export async function toggleEndpoint(id: string): Promise<MonitoredEndpoint> {
  const { data } = await apiClient.patch<MonitoredEndpoint>(`/endpoints/${id}/toggle`);
  return data;
}
