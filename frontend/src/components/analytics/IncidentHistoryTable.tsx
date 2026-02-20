import { Badge } from '@/components/ui/Badge';
import type { Incident } from '@/types';
import { formatDistanceStrict } from 'date-fns';

interface IncidentHistoryTableProps {
  incidents: Incident[];
}

export default function IncidentHistoryTable({ incidents }: IncidentHistoryTableProps) {
  if (incidents.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No incidents found</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th className="px-3 py-2 font-medium text-muted-foreground">Opened</th>
            <th className="px-3 py-2 font-medium text-muted-foreground">Resolved</th>
            <th className="px-3 py-2 font-medium text-muted-foreground">Duration</th>
            <th className="px-3 py-2 font-medium text-muted-foreground">Failures</th>
            <th className="px-3 py-2 font-medium text-muted-foreground">Status</th>
          </tr>
        </thead>
        <tbody>
          {incidents.map((incident) => {
            const resolved = incident.resolvedAt ? new Date(incident.resolvedAt) : null;
            const duration = resolved
              ? formatDistanceStrict(new Date(incident.openedAt), resolved)
              : 'Ongoing';
            return (
              <tr key={incident.id} className="border-b border-border/50">
                <td className="px-3 py-2">{new Date(incident.openedAt).toLocaleString()}</td>
                <td className="px-3 py-2">{resolved ? resolved.toLocaleString() : '-'}</td>
                <td className="px-3 py-2">{duration}</td>
                <td className="px-3 py-2">{incident.failureCount}</td>
                <td className="px-3 py-2">
                  <Badge variant={resolved ? 'success' : 'destructive'}>
                    {resolved ? 'Resolved' : 'Open'}
                  </Badge>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
