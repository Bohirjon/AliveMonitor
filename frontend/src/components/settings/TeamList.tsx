import { useState } from 'react';
import { Pencil, Trash2, Users, MessageCircle, Webhook } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { useTeams, useCreateTeam, useUpdateTeam, useDeleteTeam } from '@/hooks/useTeams';
import { useGenerateLinkCode, useUnlinkTeamTelegram } from '@/hooks/useTelegram';
import TeamForm from './TeamForm';
import type { Team, CreateTeamRequest } from '@/types';

export default function TeamList() {
  const { data: teams, isLoading } = useTeams();
  const createTeam = useCreateTeam();
  const updateTeam = useUpdateTeam();
  const deleteTeam = useDeleteTeam();
  const generateCode = useGenerateLinkCode();
  const unlinkTeamTelegram = useUnlinkTeamTelegram();

  const [showForm, setShowForm] = useState(false);
  const [editingTeam, setEditingTeam] = useState<Team | null>(null);
  const [linkingTeamId, setLinkingTeamId] = useState<string | null>(null);

  const handleCreate = (data: CreateTeamRequest) => {
    createTeam.mutate(data, { onSuccess: () => setShowForm(false) });
  };

  const handleUpdate = (data: CreateTeamRequest) => {
    if (!editingTeam) return;
    updateTeam.mutate({ id: editingTeam.id, data }, { onSuccess: () => setEditingTeam(null) });
  };

  const handleLinkTelegram = (teamId: string) => {
    setLinkingTeamId(teamId);
    generateCode.mutate(teamId);
  };

  return (
    <div>
      <div className="mb-3 flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Teams</h2>
          <p className="text-sm text-muted-foreground">
            Create teams to notify multiple people when an endpoint goes down.
          </p>
        </div>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Users className="mr-1.5 h-3.5 w-3.5" /> Add Team
        </Button>
      </div>

      {isLoading && <p className="text-sm text-muted-foreground">Loading teams...</p>}

      {teams && teams.length === 0 && (
        <p className="text-sm text-muted-foreground">No teams yet. Create one to get started.</p>
      )}

      {teams && teams.length > 0 && (
        <div className="space-y-2">
          {teams.map((team) => (
            <div key={team.id} className="rounded-lg border border-border bg-card p-3">
              <div className="flex items-center justify-between">
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-foreground">{team.name}</p>
                    {team.telegramLinked && (
                      <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                        <MessageCircle className="h-3 w-3" /> Telegram
                      </span>
                    )}
                    {team.webhookUrl && (
                      <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                        <Webhook className="h-3 w-3" /> Webhook
                      </span>
                    )}
                  </div>
                  <p className="truncate text-xs text-muted-foreground">
                    {team.memberEmails.join(', ')}
                  </p>
                </div>
                <div className="ml-2 flex items-center gap-1">
                  {team.telegramLinked ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => unlinkTeamTelegram.mutate(team.id)}
                      disabled={unlinkTeamTelegram.isPending}
                    >
                      Unlink TG
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleLinkTelegram(team.id)}
                      disabled={generateCode.isPending && linkingTeamId === team.id}
                    >
                      Link TG
                    </Button>
                  )}
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Edit team ${team.name}`}
                    onClick={() => setEditingTeam(team)}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Delete team ${team.name}`}
                    onClick={() => deleteTeam.mutate(team.id)}
                  >
                    <Trash2 className="h-3.5 w-3.5 text-destructive" />
                  </Button>
                </div>
              </div>

              {linkingTeamId === team.id && generateCode.data && (
                <div className="mt-3 rounded-md border border-border bg-muted/50 p-3">
                  <p className="mb-1 text-xs font-medium text-foreground">
                    Send this code to the bot:
                  </p>
                  <p className="mb-1 font-mono text-sm font-bold text-foreground">
                    {generateCode.data.code}
                  </p>
                  <a
                    href={generateCode.data.deepLink}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-xs text-primary hover:underline"
                  >
                    Open in Telegram
                  </a>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <TeamForm
          onSubmit={handleCreate}
          onClose={() => setShowForm(false)}
          isPending={createTeam.isPending}
        />
      )}

      {editingTeam && (
        <TeamForm
          team={editingTeam}
          onSubmit={handleUpdate}
          onClose={() => setEditingTeam(null)}
          isPending={updateTeam.isPending}
        />
      )}
    </div>
  );
}
