import { useQuery } from '@tanstack/react-query';
import { getAnalytics, getCheckLogs, getIncidents } from '@/api/analytics';

export function useAnalytics(endpointId: string, from?: string, to?: string) {
  return useQuery({
    queryKey: ['analytics', endpointId, from, to],
    queryFn: () => getAnalytics(endpointId, from, to),
    enabled: !!endpointId,
  });
}

export function useCheckLogs(endpointId: string, from?: string, to?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['checkLogs', endpointId, from, to, page, pageSize],
    queryFn: () => getCheckLogs(endpointId, from, to, page, pageSize),
    enabled: !!endpointId,
  });
}

export function useIncidents(endpointId: string, from?: string, to?: string) {
  return useQuery({
    queryKey: ['incidents', endpointId, from, to],
    queryFn: () => getIncidents(endpointId, from, to),
    enabled: !!endpointId,
  });
}
