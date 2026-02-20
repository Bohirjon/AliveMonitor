import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@/context/AuthContext';
import { createConnection, destroyConnection } from '@/lib/signalr';

export function useSignalR() {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!isAuthenticated) return;

    const connection = createConnection();

    connection.on('EndpointStatusChanged', () => {
      queryClient.invalidateQueries({ queryKey: ['endpoints'] });
    });

    connection.start().catch((err) => {
      console.error('SignalR connection failed:', err);
    });

    return () => {
      destroyConnection();
    };
  }, [isAuthenticated, queryClient]);
}
