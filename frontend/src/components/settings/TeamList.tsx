import { useState } from 'react';
import { Pencil, Trash2, Users } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { useTeams, useCreateTeam, useUpdateTeam, useDeleteTeam } from '@/hooks/useTeams';
import TeamForm from './TeamForm';
import type { Team, CreateTeamRequest } from '@/types';

export default function TeamList() {
  const { data: teams, isLoading } = useTeams();
  const createTeam = useCreateTeam();
  const updateTeam = useUpdateTeam();
  const deleteTeam = useDeleteTeam();

  const [showForm, setShowForm] = useState(false);
  const [editingTeam, setEditingTeam] = useState<Team | null>(null);

  const handleCreate = (data: CreateTeamRequest) => {
    createTeam.mutate(data, { onSuccess: () => setShowForm(false) });
  };

  const handleUpdate = (data: CreateTeamRequest) => {
    if (!editingTeam) return;
    updateTeam.mutate({ id: editingTeam.id, data }, { onSuccess: () => setEditingTeam(null) });
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
            <div key={team.id} className="flex items-center justify-between rounded-lg border border-border bg-card p-3">
              <div className="min-w-0 flex-1">
                <p className="text-sm font-medium text-foreground">{team.name}</p>
                <p className="truncate text-xs text-muted-foreground">
                  {team.memberEmails.join(', ')}
                </p>
              </div>
              <div className="ml-2 flex items-center gap-1">
                <Button variant="ghost" size="icon" onClick={() => setEditingTeam(team)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button variant="ghost" size="icon" onClick={() => deleteTeam.mutate(team.id)}>
                  <Trash2 className="h-3.5 w-3.5 text-destructive" />
                </Button>
              </div>
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
