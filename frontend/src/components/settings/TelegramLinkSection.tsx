import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';
import { useTelegramStatus, useGenerateLinkCode, useUnlinkTelegram } from '@/hooks/useTelegram';

export default function TelegramLinkSection() {
  const { data: status, isLoading } = useTelegramStatus();
  const generateCode = useGenerateLinkCode();
  const unlinkTelegram = useUnlinkTelegram();

  const [countdown, setCountdown] = useState(0);

  useEffect(() => {
    if (!generateCode.data) return;

    const expiresAt = new Date(generateCode.data.expiresAt).getTime();
    const interval = setInterval(() => {
      const remaining = Math.max(0, Math.floor((expiresAt - Date.now()) / 1000));
      setCountdown(remaining);
      if (remaining === 0) clearInterval(interval);
    }, 1000);

    return () => clearInterval(interval);
  }, [generateCode.data]);

  const handleGenerate = () => {
    generateCode.mutate(undefined);
  };

  if (isLoading) return null;

  return (
    <div className="rounded-lg border border-border bg-card p-6">
      <h2 className="mb-4 text-lg font-semibold text-foreground">Telegram Notifications</h2>

      {status?.isLinked ? (
        <div>
          <p className="mb-3 text-sm text-muted-foreground">
            Your Telegram is linked. You will receive alerts via Telegram in addition to email.
          </p>
          <Button
            variant="outline"
            size="sm"
            onClick={() => unlinkTelegram.mutate()}
            disabled={unlinkTelegram.isPending}
          >
            {unlinkTelegram.isPending ? 'Unlinking...' : 'Unlink Telegram'}
          </Button>
        </div>
      ) : (
        <div>
          <p className="mb-3 text-sm text-muted-foreground">
            Link your Telegram account to receive alerts via Telegram.
          </p>

          {generateCode.data && countdown > 0 ? (
            <div className="space-y-3">
              <div className="rounded-md border border-border bg-muted/50 p-4">
                <p className="mb-2 text-sm font-medium text-foreground">
                  Send this code to the bot or click the link:
                </p>
                <p className="mb-2 font-mono text-lg font-bold text-foreground">
                  {generateCode.data.code}
                </p>
                <a
                  href={generateCode.data.deepLink}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-primary hover:underline"
                >
                  Open in Telegram
                </a>
                <p className="mt-2 text-xs text-muted-foreground">
                  Expires in {Math.floor(countdown / 60)}:{(countdown % 60).toString().padStart(2, '0')}
                </p>
              </div>
            </div>
          ) : (
            <Button size="sm" onClick={handleGenerate} disabled={generateCode.isPending}>
              {generateCode.isPending ? 'Generating...' : 'Link Telegram'}
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
