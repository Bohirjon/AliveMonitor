import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getEndpoints, getEndpoint, createEndpoint, updateEndpoint, deleteEndpoint, toggleEndpoint } from '@/api/endpoints';
import type { CreateEndpointRequest } from '@/types';
import { toast } from 'sonner';

export function useEndpoints(search?: string, status?: string) {
  return useQuery({
    queryKey: ['endpoints', search, status],
    queryFn: () => getEndpoints(search, status),
  });
}

export function useEndpoint(id: string) {
  return useQuery({
    queryKey: ['endpoint', id],
    queryFn: () => getEndpoint(id),
    enabled: !!id,
  });
}

export function useCreateEndpoint() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createEndpoint,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['endpoints'] });
      toast.success('Endpoint created successfully');
    },
    onError: () => toast.error('Failed to create endpoint'),
  });
}

export function useUpdateEndpoint() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateEndpointRequest }) => updateEndpoint(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['endpoints'] });
      toast.success('Endpoint updated successfully');
    },
    onError: () => toast.error('Failed to update endpoint'),
  });
}

export function useDeleteEndpoint() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteEndpoint,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['endpoints'] });
      toast.success('Endpoint deleted');
    },
    onError: () => toast.error('Failed to delete endpoint'),
  });
}

export function useToggleEndpoint() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: toggleEndpoint,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['endpoints'] });
    },
    onError: () => toast.error('Failed to toggle endpoint'),
  });
}
