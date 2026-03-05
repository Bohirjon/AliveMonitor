import { useCallback, useRef } from 'react';
import { Link } from 'react-router-dom';
import { MoreVertical, Pencil, Trash2, ExternalLink, Users, Shield } from 'lucide-react';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { EndpointStatus, type MonitoredEndpoint } from '@/types';
import { useToggleEndpoint, useDeleteEndpoint } from '@/hooks/useEndpoints';
import { useClickOutside } from '@/hooks/useClickOutside';

const statusConfig = {
  [EndpointStatus.Healthy]: { label: 'Healthy', variant: 'success' as const, dot: 'bg-status-healthy' },
  [EndpointStatus.Unhealthy]: { label: 'Unhealthy', variant: 'destructive' as const, dot: 'bg-status-unhealthy' },
  [EndpointStatus.Disabled]: { label: 'Disabled', variant: 'secondary' as const, dot: 'bg-status-disabled' },
};

interface EndpointCardProps {
  endpoint: MonitoredEndpoint;
  onEdit: (endpoint: MonitoredEndpoint) => void;
  isMenuOpen: boolean;
  onMenuToggle: (open: boolean) => void;
}

export default function EndpointCard({ endpoint, onEdit, isMenuOpen, onMenuToggle }: EndpointCardProps) {
  const toggle = useToggleEndpoint();
  const remove = useDeleteEndpoint();
  const config = statusConfig[endpoint.currentStatus];
  const menuRef = useRef<HTMLDivElement>(null);
  const closeMenu = useCallback(() => onMenuToggle(false), [onMenuToggle]);
  useClickOutside(menuRef, closeMenu, isMenuOpen);

  return (
    <div className="rounded-lg border border-border bg-card p-4 transition-shadow hover:shadow-sm">
      <div className="flex items-start justify-between">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className={`h-2.5 w-2.5 rounded-full ${config.dot}`} />
            <Link
              to={`/endpoints/${endpoint.id}`}
              className="truncate text-sm font-medium text-foreground hover:text-primary"
            >
              {endpoint.friendlyName}
            </Link>
            <Badge variant={config.variant}>{config.label}</Badge>
          </div>
          <p className="mt-1 flex items-center gap-1 truncate text-xs text-muted-foreground">
            <ExternalLink className="h-3 w-3" />
            {endpoint.url}
          </p>
          <div className="mt-2 flex gap-4 text-xs text-muted-foreground">
            <span>Every {endpoint.intervalMinutes}m</span>
            {endpoint.lastCheckedAt && (
              <span>Last: {new Date(endpoint.lastCheckedAt).toLocaleString()}</span>
            )}
            {endpoint.sslCheckEnabled && endpoint.sslDaysUntilExpiry != null && (
              <span className={`flex items-center gap-1 ${
                endpoint.sslDaysUntilExpiry <= 1 ? 'text-destructive' :
                endpoint.sslDaysUntilExpiry <= 7 ? 'text-warning' :
                endpoint.sslDaysUntilExpiry <= 30 ? 'text-warning' :
                'text-success'
              }`}>
                <Shield className="h-3 w-3" /> SSL: {endpoint.sslDaysUntilExpiry}d
              </span>
            )}
            {endpoint.teamName && (
              <span className="flex items-center gap-1">
                <Users className="h-3 w-3" /> {endpoint.teamName}
              </span>
            )}
          </div>
        </div>

        <div className="relative ml-2 flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => toggle.mutate(endpoint.id)}
            disabled={toggle.isPending}
          >
            {endpoint.isEnabled ? 'Disable' : 'Enable'}
          </Button>
          <div ref={menuRef} className="relative">
            <Button variant="ghost" size="icon" onClick={() => onMenuToggle(!isMenuOpen)}>
              <MoreVertical className="h-4 w-4" />
            </Button>
            {isMenuOpen && (
              <div className="absolute right-0 top-full z-10 mt-1 w-36 rounded-md border border-border bg-popover py-1 shadow-md">
                <button
                  className="flex w-full items-center gap-2 px-3 py-1.5 text-sm text-foreground hover:bg-accent"
                  onClick={() => { onEdit(endpoint); onMenuToggle(false); }}
                >
                  <Pencil className="h-3.5 w-3.5" /> Edit
                </button>
                <button
                  className="flex w-full items-center gap-2 px-3 py-1.5 text-sm text-destructive hover:bg-accent"
                  onClick={() => { remove.mutate(endpoint.id); onMenuToggle(false); }}
                >
                  <Trash2 className="h-3.5 w-3.5" /> Delete
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
