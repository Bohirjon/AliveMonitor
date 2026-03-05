import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Shield } from 'lucide-react';
import { subDays } from 'date-fns';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import AppLayout from '@/components/layout/AppLayout';
import DateRangePicker from '@/components/analytics/DateRangePicker';
import ResponseTimeChart from '@/components/analytics/ResponseTimeChart';
import UptimeCard from '@/components/analytics/UptimeCard';
import CheckLogTable from '@/components/analytics/CheckLogTable';
import IncidentHistoryTable from '@/components/analytics/IncidentHistoryTable';
import { useEndpoint } from '@/hooks/useEndpoints';
import { useAnalytics, useCheckLogs, useIncidents } from '@/hooks/useAnalytics';
import { EndpointStatus } from '@/types';

const statusConfig = {
  [EndpointStatus.Healthy]: { label: 'Healthy', variant: 'success' as const },
  [EndpointStatus.Unhealthy]: { label: 'Unhealthy', variant: 'destructive' as const },
  [EndpointStatus.Disabled]: { label: 'Disabled', variant: 'secondary' as const },
};

export default function EndpointDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [from, setFrom] = useState(subDays(new Date(), 7).toISOString());
  const [to, setTo] = useState(new Date().toISOString());
  const [checkPage, setCheckPage] = useState(1);

  const { data: endpoint } = useEndpoint(id!);
  const { data: analytics } = useAnalytics(id!, from, to);
  const { data: checkLogs } = useCheckLogs(id!, from, to, checkPage);
  const { data: incidents } = useIncidents(id!, from, to);

  if (!endpoint) {
    return (
      <AppLayout>
        <div className="flex h-64 items-center justify-center">
          <div className="h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        </div>
      </AppLayout>
    );
  }

  const config = statusConfig[endpoint.currentStatus];

  return (
    <AppLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <Link to="/dashboard">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold text-foreground">{endpoint.friendlyName}</h1>
              <Badge variant={config.variant}>{config.label}</Badge>
            </div>
            <p className="text-sm text-muted-foreground">{endpoint.url}</p>
          </div>
        </div>

        {/* SSL Certificate Status */}
        {endpoint.sslCheckEnabled && (
          <div className="rounded-lg border border-border bg-card p-4">
            <h2 className="mb-3 flex items-center gap-2 text-sm font-medium text-foreground">
              <Shield className="h-4 w-4" /> SSL Certificate
            </h2>
            <div className="grid grid-cols-4 gap-4">
              <div className="rounded-lg bg-muted/50 p-3">
                <p className="text-xs text-muted-foreground">Status</p>
                <p className={`text-lg font-semibold ${
                  endpoint.sslDaysUntilExpiry == null ? 'text-muted-foreground' :
                  endpoint.sslDaysUntilExpiry <= 1 ? 'text-destructive' :
                  endpoint.sslDaysUntilExpiry <= 7 ? 'text-warning' :
                  endpoint.sslDaysUntilExpiry <= 30 ? 'text-warning' :
                  'text-success'
                }`}>
                  {endpoint.sslDaysUntilExpiry == null ? 'Pending' :
                   endpoint.sslDaysUntilExpiry <= 7 ? 'Expiring Soon' : 'Valid'}
                </p>
              </div>
              <div className="rounded-lg bg-muted/50 p-3">
                <p className="text-xs text-muted-foreground">Expires</p>
                <p className="text-lg font-semibold text-foreground">
                  {endpoint.sslCertificateExpiresAt
                    ? new Date(endpoint.sslCertificateExpiresAt).toLocaleDateString()
                    : '—'}
                </p>
              </div>
              <div className="rounded-lg bg-muted/50 p-3">
                <p className="text-xs text-muted-foreground">Days Remaining</p>
                <p className={`text-lg font-semibold ${
                  endpoint.sslDaysUntilExpiry == null ? 'text-muted-foreground' :
                  endpoint.sslDaysUntilExpiry <= 1 ? 'text-destructive' :
                  endpoint.sslDaysUntilExpiry <= 7 ? 'text-warning' :
                  endpoint.sslDaysUntilExpiry <= 30 ? 'text-warning' :
                  'text-success'
                }`}>
                  {endpoint.sslDaysUntilExpiry ?? '—'}
                </p>
              </div>
              <div className="rounded-lg bg-muted/50 p-3">
                <p className="text-xs text-muted-foreground">Last Checked</p>
                <p className="text-lg font-semibold text-foreground">
                  {endpoint.sslLastCheckedAt
                    ? new Date(endpoint.sslLastCheckedAt).toLocaleString()
                    : '—'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Date Range */}
        <DateRangePicker from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); setCheckPage(1); }} />

        {/* Analytics Summary */}
        {analytics && (
          <UptimeCard
            uptimePercentage={analytics.uptimePercentage}
            avgResponseTimeMs={analytics.avgResponseTimeMs}
            totalChecks={analytics.totalChecks}
            totalIncidents={analytics.totalIncidents}
          />
        )}

        {/* Response Time Chart */}
        <div className="rounded-lg border border-border bg-card p-4">
          <h2 className="mb-3 text-sm font-medium text-foreground">Response Time</h2>
          {checkLogs ? <ResponseTimeChart data={checkLogs.items} /> : <div className="h-64 animate-pulse rounded bg-muted" />}
        </div>

        {/* Check Logs */}
        <div className="rounded-lg border border-border bg-card p-4">
          <h2 className="mb-3 text-sm font-medium text-foreground">Check Log</h2>
          {checkLogs ? <CheckLogTable data={checkLogs} onPageChange={setCheckPage} /> : <div className="h-32 animate-pulse rounded bg-muted" />}
        </div>

        {/* Incidents */}
        <div className="rounded-lg border border-border bg-card p-4">
          <h2 className="mb-3 text-sm font-medium text-foreground">Incident History</h2>
          {incidents ? <IncidentHistoryTable incidents={incidents} /> : <div className="h-32 animate-pulse rounded bg-muted" />}
        </div>
      </div>
    </AppLayout>
  );
}
