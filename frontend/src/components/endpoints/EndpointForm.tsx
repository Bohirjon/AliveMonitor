import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { X, Plus, Trash2 } from 'lucide-react';
import type { MonitoredEndpoint, CreateEndpointRequest } from '@/types';

interface EndpointFormProps {
  endpoint?: MonitoredEndpoint | null;
  onSubmit: (data: CreateEndpointRequest) => void;
  onClose: () => void;
  isPending: boolean;
}

export default function EndpointForm({ endpoint, onSubmit, onClose, isPending }: EndpointFormProps) {
  const [friendlyName, setFriendlyName] = useState('');
  const [url, setUrl] = useState('');
  const [intervalMinutes, setIntervalMinutes] = useState(1);
  const [timeoutSeconds, setTimeoutSeconds] = useState(30);
  const [expectedStatusCode, setExpectedStatusCode] = useState(200);
  const [jsonPropertyName, setJsonPropertyName] = useState('');
  const [jsonPropertyExpectedValue, setJsonPropertyExpectedValue] = useState('');
  const [headers, setHeaders] = useState<Array<{ key: string; value: string }>>([]);

  useEffect(() => {
    if (endpoint) {
      setFriendlyName(endpoint.friendlyName);
      setUrl(endpoint.url);
      setIntervalMinutes(endpoint.intervalMinutes);
      setTimeoutSeconds(endpoint.timeoutSeconds);
      setExpectedStatusCode(endpoint.expectedStatusCode);
      setJsonPropertyName(endpoint.jsonPropertyName ?? '');
      setJsonPropertyExpectedValue(endpoint.jsonPropertyExpectedValue ?? '');
      if (endpoint.customHeaders) {
        setHeaders(Object.entries(endpoint.customHeaders).map(([key, value]) => ({ key, value })));
      }
    }
  }, [endpoint]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const customHeaders = headers.length > 0
      ? Object.fromEntries(headers.filter(h => h.key).map(h => [h.key, h.value]))
      : undefined;

    onSubmit({
      friendlyName,
      url,
      intervalMinutes,
      timeoutSeconds,
      customHeaders,
      expectedStatusCode,
      jsonPropertyName: jsonPropertyName || undefined,
      jsonPropertyExpectedValue: jsonPropertyExpectedValue || undefined,
    });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-lg rounded-lg border border-border bg-card p-6 shadow-lg">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-foreground">
            {endpoint ? 'Edit Endpoint' : 'Add Endpoint'}
          </h2>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Friendly Name</label>
            <Input value={friendlyName} onChange={(e) => setFriendlyName(e.target.value)} required />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">URL</label>
            <Input value={url} onChange={(e) => setUrl(e.target.value)} placeholder="https://api.example.com/health" required />
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Interval (min)</label>
              <Input type="number" min={1} value={intervalMinutes} onChange={(e) => setIntervalMinutes(Number(e.target.value))} />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Timeout (sec)</label>
              <Input type="number" min={1} value={timeoutSeconds} onChange={(e) => setTimeoutSeconds(Number(e.target.value))} />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Expected Status</label>
              <Input type="number" value={expectedStatusCode} onChange={(e) => setExpectedStatusCode(Number(e.target.value))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">JSON Property</label>
              <Input value={jsonPropertyName} onChange={(e) => setJsonPropertyName(e.target.value)} placeholder="status" />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Expected Value</label>
              <Input value={jsonPropertyExpectedValue} onChange={(e) => setJsonPropertyExpectedValue(e.target.value)} placeholder="healthy" />
            </div>
          </div>

          {/* Custom Headers */}
          <div>
            <div className="mb-1 flex items-center justify-between">
              <label className="text-sm font-medium text-foreground">Custom Headers</label>
              <Button type="button" variant="ghost" size="sm" onClick={() => setHeaders([...headers, { key: '', value: '' }])}>
                <Plus className="h-3.5 w-3.5" /> Add
              </Button>
            </div>
            {headers.map((header, i) => (
              <div key={i} className="mb-2 flex gap-2">
                <Input placeholder="Key" value={header.key} onChange={(e) => {
                  const updated = [...headers];
                  updated[i] = { ...updated[i], key: e.target.value };
                  setHeaders(updated);
                }} />
                <Input placeholder="Value" value={header.value} onChange={(e) => {
                  const updated = [...headers];
                  updated[i] = { ...updated[i], value: e.target.value };
                  setHeaders(updated);
                }} />
                <Button type="button" variant="ghost" size="icon" onClick={() => setHeaders(headers.filter((_, j) => j !== i))}>
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            ))}
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : endpoint ? 'Update' : 'Create'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
