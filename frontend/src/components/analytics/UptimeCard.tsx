interface UptimeCardProps {
  uptimePercentage: number;
  avgResponseTimeMs: number;
  totalChecks: number;
  totalIncidents: number;
}

export default function UptimeCard({ uptimePercentage, avgResponseTimeMs, totalChecks, totalIncidents }: UptimeCardProps) {
  const uptimeColor = uptimePercentage >= 99 ? 'text-success' : uptimePercentage >= 95 ? 'text-warning' : 'text-destructive';

  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
      <StatCard label="Uptime" value={`${uptimePercentage}%`} className={uptimeColor} />
      <StatCard label="Avg Response" value={`${Math.round(avgResponseTimeMs)}ms`} />
      <StatCard label="Total Checks" value={totalChecks.toString()} />
      <StatCard label="Incidents" value={totalIncidents.toString()} className={totalIncidents > 0 ? 'text-destructive' : ''} />
    </div>
  );
}

function StatCard({ label, value, className = '' }: { label: string; value: string; className?: string }) {
  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`mt-1 text-2xl font-bold ${className || 'text-foreground'}`}>{value}</p>
    </div>
  );
}
