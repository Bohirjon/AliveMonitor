import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { X, Plus, Trash2 } from 'lucide-react';
import type { Team, CreateTeamRequest } from '@/types';

interface TeamFormProps {
  team?: Team | null;
  onSubmit: (data: CreateTeamRequest) => void;
  onClose: () => void;
  isPending: boolean;
}

export default function TeamForm({ team, onSubmit, onClose, isPending }: TeamFormProps) {
  const [name, setName] = useState('');
  const [emails, setEmails] = useState<string[]>(['']);
  const [webhookUrl, setWebhookUrl] = useState('');

  useEffect(() => {
    if (team) {
      setName(team.name);
      setEmails(team.memberEmails.length > 0 ? [...team.memberEmails] : ['']);
      setWebhookUrl(team.webhookUrl ?? '');
    }
  }, [team]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const filtered = emails.filter((email) => email.trim() !== '');
    if (filtered.length === 0) return;
    onSubmit({ name, memberEmails: filtered, webhookUrl: webhookUrl.trim() || undefined });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-lg rounded-lg border border-border bg-card p-6 shadow-lg">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-foreground">
            {team ? 'Edit Team' : 'Add Team'}
          </h2>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Team Name</label>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Backend Team" required />
          </div>

          <div>
            <div className="mb-1 flex items-center justify-between">
              <label className="text-sm font-medium text-foreground">Member Emails</label>
              <Button type="button" variant="ghost" size="sm" onClick={() => setEmails([...emails, ''])}>
                <Plus className="h-3.5 w-3.5" /> Add
              </Button>
            </div>
            {emails.map((email, i) => (
              <div key={i} className="mb-2 flex gap-2">
                <Input
                  type="email"
                  placeholder="member@example.com"
                  value={email}
                  onChange={(e) => {
                    const updated = [...emails];
                    updated[i] = e.target.value;
                    setEmails(updated);
                  }}
                />
                {emails.length > 1 && (
                  <Button type="button" variant="ghost" size="icon" onClick={() => setEmails(emails.filter((_, j) => j !== i))}>
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                )}
              </div>
            ))}
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Webhook URL</label>
            <Input
              type="url"
              placeholder="https://example.com/webhook"
              value={webhookUrl}
              onChange={(e) => setWebhookUrl(e.target.value)}
            />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : team ? 'Update' : 'Create'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
