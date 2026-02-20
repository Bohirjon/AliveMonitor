import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import AppLayout from '@/components/layout/AppLayout';
import { useAuth } from '@/context/AuthContext';
import { updateAlertEmail } from '@/api/settings';
import { toast } from 'sonner';

export default function SettingsPage() {
  const { user } = useAuth();
  const [alertEmail, setAlertEmail] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (user) setAlertEmail(user.alertEmail);
  }, [user]);

  const handleSave = async () => {
    setSaving(true);
    try {
      await updateAlertEmail(alertEmail);
      toast.success('Alert email updated');
    } catch {
      toast.error('Failed to update alert email');
    } finally {
      setSaving(false);
    }
  };

  if (!user) return null;

  return (
    <AppLayout>
      <div className="space-y-6">
        <h1 className="text-2xl font-bold text-foreground">Settings</h1>

        {/* Profile */}
        <div className="rounded-lg border border-border bg-card p-6">
          <h2 className="mb-4 text-lg font-semibold text-foreground">Profile</h2>
          <div className="flex items-center gap-4">
            {user.avatarUrl ? (
              <img src={user.avatarUrl} alt="" className="h-16 w-16 rounded-full" />
            ) : (
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-primary text-xl font-medium text-primary-foreground">
                {user.name.charAt(0)}
              </div>
            )}
            <div>
              <p className="text-lg font-medium text-foreground">{user.name}</p>
              <p className="text-sm text-muted-foreground">{user.email}</p>
            </div>
          </div>
        </div>

        {/* Alert Email */}
        <div className="rounded-lg border border-border bg-card p-6">
          <h2 className="mb-4 text-lg font-semibold text-foreground">Alert Notifications</h2>
          <p className="mb-3 text-sm text-muted-foreground">
            All endpoint alerts will be sent to this email address.
          </p>
          <div className="flex gap-3">
            <Input
              type="email"
              value={alertEmail}
              onChange={(e) => setAlertEmail(e.target.value)}
              className="max-w-sm"
            />
            <Button onClick={handleSave} disabled={saving}>
              {saving ? 'Saving...' : 'Save'}
            </Button>
          </div>
        </div>
      </div>
    </AppLayout>
  );
}
