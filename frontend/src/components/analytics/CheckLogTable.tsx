import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import type { PaginatedResponse, HealthCheckLog } from '@/types';

interface CheckLogTableProps {
  data: PaginatedResponse<HealthCheckLog>;
  onPageChange: (page: number) => void;
}

export default function CheckLogTable({ data, onPageChange }: CheckLogTableProps) {
  if (data.items.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No check logs found</p>;
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border text-left">
              <th className="px-3 py-2 font-medium text-muted-foreground">Time</th>
              <th className="px-3 py-2 font-medium text-muted-foreground">Status</th>
              <th className="px-3 py-2 font-medium text-muted-foreground">HTTP Code</th>
              <th className="px-3 py-2 font-medium text-muted-foreground">Response Time</th>
              <th className="px-3 py-2 font-medium text-muted-foreground">Error</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((log) => (
              <tr key={log.id} className="border-b border-border/50">
                <td className="px-3 py-2">{new Date(log.checkedAt).toLocaleString()}</td>
                <td className="px-3 py-2">
                  <Badge variant={log.isHealthy ? 'success' : 'destructive'}>
                    {log.isHealthy ? 'Healthy' : 'Unhealthy'}
                  </Badge>
                </td>
                <td className="px-3 py-2">{log.httpStatusCode ?? '-'}</td>
                <td className="px-3 py-2">{log.responseTimeMs}ms</td>
                <td className="max-w-xs truncate px-3 py-2 text-muted-foreground">{log.errorMessage ?? '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data.totalPages > 1 && (
        <div className="mt-3 flex items-center justify-between">
          <p className="text-xs text-muted-foreground">
            Page {data.page} of {data.totalPages} ({data.totalCount} total)
          </p>
          <div className="flex gap-1">
            <Button variant="outline" size="sm" disabled={data.page <= 1} onClick={() => onPageChange(data.page - 1)}>
              Previous
            </Button>
            <Button variant="outline" size="sm" disabled={data.page >= data.totalPages} onClick={() => onPageChange(data.page + 1)}>
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
