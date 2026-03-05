import { cn } from '@/lib/utils';
import type { HTMLAttributes } from 'react';

interface BadgeProps extends HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'success' | 'destructive' | 'secondary';
}

export function Badge({ className, variant = 'default', ...props }: BadgeProps) {
  return (
    <div
      className={cn(
        'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors',
        {
          'border-transparent bg-primary text-primary-foreground': variant === 'default',
          'border-transparent bg-success text-success-foreground': variant === 'success',
          'border-transparent bg-destructive text-destructive-foreground': variant === 'destructive',
          'border-transparent bg-secondary text-secondary-foreground': variant === 'secondary',
        },
        className,
      )}
      {...props}
    />
  );
}
