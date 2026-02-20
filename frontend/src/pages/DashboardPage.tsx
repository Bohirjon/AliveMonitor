import { useState } from 'react';
import { Search, Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import EndpointCard from '@/components/endpoints/EndpointCard';
import EndpointForm from '@/components/endpoints/EndpointForm';
import AppLayout from '@/components/layout/AppLayout';
import { useEndpoints, useCreateEndpoint, useUpdateEndpoint } from '@/hooks/useEndpoints';
import { useSignalR } from '@/hooks/useSignalR';
import type { MonitoredEndpoint, CreateEndpointRequest } from '@/types';

const statusFilters = ['All', 'Healthy', 'Unhealthy', 'Disabled'];

export default function DashboardPage() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [showForm, setShowForm] = useState(false);
  const [editingEndpoint, setEditingEndpoint] = useState<MonitoredEndpoint | null>(null);

  useSignalR();

  const { data: endpoints, isLoading } = useEndpoints(search || undefined, statusFilter);
  const createMutation = useCreateEndpoint();
  const updateMutation = useUpdateEndpoint();

  const handleSubmit = (data: CreateEndpointRequest) => {
    if (editingEndpoint) {
      updateMutation.mutate({ id: editingEndpoint.id, data }, {
        onSuccess: () => { setShowForm(false); setEditingEndpoint(null); }
      });
    } else {
      createMutation.mutate(data, {
        onSuccess: () => setShowForm(false),
      });
    }
  };

  const handleEdit = (endpoint: MonitoredEndpoint) => {
    setEditingEndpoint(endpoint);
    setShowForm(true);
  };

  return (
    <AppLayout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
          <Button onClick={() => { setEditingEndpoint(null); setShowForm(true); }}>
            <Plus className="h-4 w-4" /> Add Endpoint
          </Button>
        </div>

        {/* Filters */}
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by name or URL..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
          <div className="flex gap-1">
            {statusFilters.map((s) => (
              <Button
                key={s}
                variant={statusFilter === s ? 'default' : 'outline'}
                size="sm"
                onClick={() => setStatusFilter(s)}
              >
                {s}
              </Button>
            ))}
          </div>
        </div>

        {/* Endpoint List */}
        {isLoading ? (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-24 animate-pulse rounded-lg bg-muted" />
            ))}
          </div>
        ) : endpoints && endpoints.length > 0 ? (
          <div className="space-y-3">
            {endpoints.map((endpoint) => (
              <EndpointCard key={endpoint.id} endpoint={endpoint} onEdit={handleEdit} />
            ))}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-border py-12 text-center">
            <p className="text-sm text-muted-foreground">
              {search || statusFilter !== 'All' ? 'No endpoints match your filters' : 'No endpoints yet. Add one to get started!'}
            </p>
          </div>
        )}
      </div>

      {showForm && (
        <EndpointForm
          endpoint={editingEndpoint}
          onSubmit={handleSubmit}
          onClose={() => { setShowForm(false); setEditingEndpoint(null); }}
          isPending={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </AppLayout>
  );
}
