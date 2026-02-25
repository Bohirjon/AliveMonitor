import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTelegramStatus, generateLinkCode, unlinkTelegram, unlinkTeamTelegram } from '@/api/telegram';
import { toast } from 'sonner';

export function useTelegramStatus() {
  return useQuery({
    queryKey: ['telegram-status'],
    queryFn: getTelegramStatus,
  });
}

export function useGenerateLinkCode() {
  return useMutation({
    mutationFn: (teamId?: string) => generateLinkCode(teamId),
    onError: () => toast.error('Failed to generate link code'),
  });
}

export function useUnlinkTelegram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: unlinkTelegram,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['telegram-status'] });
      toast.success('Telegram unlinked');
    },
    onError: () => toast.error('Failed to unlink Telegram'),
  });
}

export function useUnlinkTeamTelegram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: unlinkTeamTelegram,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      toast.success('Team Telegram unlinked');
    },
    onError: () => toast.error('Failed to unlink team Telegram'),
  });
}
