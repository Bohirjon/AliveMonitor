import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { HealthCheckLog } from '@/types';
import { format } from 'date-fns';

interface ResponseTimeChartProps {
  data: HealthCheckLog[];
}

export default function ResponseTimeChart({ data }: ResponseTimeChartProps) {
  const chartData = data.map((log) => ({
    time: format(new Date(log.checkedAt), 'MM/dd HH:mm'),
    responseTime: log.responseTimeMs,
    healthy: log.isHealthy,
  }));

  if (chartData.length === 0) {
    return (
      <div className="flex h-64 items-center justify-center rounded-lg border border-dashed border-border text-sm text-muted-foreground">
        No data available
      </div>
    );
  }

  return (
    <div className="h-64">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
          <XAxis dataKey="time" tick={{ fontSize: 11 }} stroke="var(--color-muted-foreground)" />
          <YAxis tick={{ fontSize: 11 }} stroke="var(--color-muted-foreground)" label={{ value: 'ms', position: 'insideLeft' }} />
          <Tooltip
            contentStyle={{
              backgroundColor: 'var(--color-card)',
              border: '1px solid var(--color-border)',
              borderRadius: '6px',
              fontSize: 12,
            }}
          />
          <Line type="monotone" dataKey="responseTime" stroke="var(--color-primary)" strokeWidth={2} dot={false} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
