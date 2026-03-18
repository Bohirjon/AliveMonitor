import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../models/team.dart';
import '../../providers/team_provider.dart';
import '../../services/api_client.dart';
import '../../services/telegram_service.dart';

class TeamsSection extends StatefulWidget {
  const TeamsSection({super.key});

  @override
  State<TeamsSection> createState() => _TeamsSectionState();
}

class _TeamsSectionState extends State<TeamsSection> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<TeamProvider>().fetchTeams();
    });
  }

  void _showTeamForm({Team? team}) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      builder: (_) => _TeamFormSheet(team: team),
    );
  }

  Future<void> _confirmDelete(Team team) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete Team'),
        content: Text('Are you sure you want to delete "${team.name}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      await context.read<TeamProvider>().deleteTeam(team.id);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Consumer<TeamProvider>(
      builder: (context, provider, _) {
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Teams',
                    style: theme.textTheme.titleSmall
                        ?.copyWith(fontWeight: FontWeight.w600)),
                IconButton(
                  onPressed: () => _showTeamForm(),
                  icon: const Icon(Icons.add),
                ),
              ],
            ),
            const SizedBox(height: 8),
            if (provider.isLoading && provider.teams.isEmpty)
              const Center(child: CircularProgressIndicator())
            else if (provider.teams.isEmpty)
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Center(
                    child: Text(
                      'No teams yet',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ),
                ),
              )
            else
              ...provider.teams.map((team) => _TeamCard(
                    team: team,
                    onEdit: () => _showTeamForm(team: team),
                    onDelete: () => _confirmDelete(team),
                  )),
          ],
        );
      },
    );
  }
}

class _TeamCard extends StatelessWidget {
  final Team team;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const _TeamCard({
    required this.team,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    team.name,
                    style: theme.textTheme.titleSmall
                        ?.copyWith(fontWeight: FontWeight.w600),
                  ),
                ),
                if (team.telegramLinked)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.green.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.telegram, size: 14, color: Colors.green),
                        SizedBox(width: 4),
                        Text('TG',
                            style: TextStyle(
                                color: Colors.green,
                                fontSize: 11,
                                fontWeight: FontWeight.w600)),
                      ],
                    ),
                  ),
                if (team.webhookUrl != null)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.blue.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.webhook, size: 14, color: Colors.blue),
                        SizedBox(width: 4),
                        Text('WH',
                            style: TextStyle(
                                color: Colors.blue,
                                fontSize: 11,
                                fontWeight: FontWeight.w600)),
                      ],
                    ),
                  ),
                PopupMenuButton<String>(
                  onSelected: (value) {
                    switch (value) {
                      case 'edit':
                        onEdit();
                        break;
                      case 'delete':
                        onDelete();
                        break;
                      case 'link_tg':
                        _showTelegramLink(context);
                        break;
                      case 'unlink_tg':
                        _unlinkTelegram(context);
                        break;
                    }
                  },
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'edit',
                      child: Row(
                        children: [
                          Icon(Icons.edit, size: 20),
                          SizedBox(width: 8),
                          Text('Edit'),
                        ],
                      ),
                    ),
                    PopupMenuItem(
                      value: team.telegramLinked ? 'unlink_tg' : 'link_tg',
                      child: Row(
                        children: [
                          const Icon(Icons.telegram, size: 20),
                          const SizedBox(width: 8),
                          Text(team.telegramLinked
                              ? 'Unlink Telegram'
                              : 'Link Telegram'),
                        ],
                      ),
                    ),
                    PopupMenuItem(
                      value: 'delete',
                      child: Row(
                        children: [
                          Icon(Icons.delete,
                              size: 20, color: theme.colorScheme.error),
                          const SizedBox(width: 8),
                          Text('Delete',
                              style:
                                  TextStyle(color: theme.colorScheme.error)),
                        ],
                      ),
                    ),
                  ],
                ),
              ],
            ),
            if (team.memberEmails.isNotEmpty) ...[
              const SizedBox(height: 8),
              Wrap(
                spacing: 6,
                runSpacing: 4,
                children: team.memberEmails
                    .map((email) => Chip(
                          label: Text(email, style: const TextStyle(fontSize: 11)),
                          visualDensity: VisualDensity.compact,
                          padding: EdgeInsets.zero,
                        ))
                    .toList(),
              ),
            ],
          ],
        ),
      ),
    );
  }

  void _showTelegramLink(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (_) => _TeamTelegramLinkSheet(teamId: team.id),
    );
  }

  Future<void> _unlinkTelegram(BuildContext context) async {
    try {
      final service = TelegramService(context.read<ApiClient>());
      await service.unlinkTeam(team.id);
      if (context.mounted) {
        context.read<TeamProvider>().fetchTeams();
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Telegram unlinked')),
        );
      }
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to unlink Telegram')),
        );
      }
    }
  }
}

class _TeamTelegramLinkSheet extends StatefulWidget {
  final String teamId;

  const _TeamTelegramLinkSheet({required this.teamId});

  @override
  State<_TeamTelegramLinkSheet> createState() =>
      _TeamTelegramLinkSheetState();
}

class _TeamTelegramLinkSheetState extends State<_TeamTelegramLinkSheet> {
  LinkCodeResponse? _linkCode;
  bool _isLoading = true;
  Timer? _countdownTimer;
  Duration? _remaining;

  @override
  void initState() {
    super.initState();
    _generateCode();
  }

  @override
  void dispose() {
    _countdownTimer?.cancel();
    super.dispose();
  }

  Future<void> _generateCode() async {
    try {
      final service = TelegramService(context.read<ApiClient>());
      final code = await service.generateLinkCode(teamId: widget.teamId);
      setState(() {
        _linkCode = code;
        _isLoading = false;
      });
      _startCountdown();
    } catch (_) {
      if (mounted) {
        setState(() => _isLoading = false);
        Navigator.pop(context);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to generate link code')),
        );
      }
    }
  }

  void _startCountdown() {
    _countdownTimer = Timer.periodic(const Duration(seconds: 1), (_) {
      if (_linkCode == null) return;
      final remaining =
          _linkCode!.expiresAt.difference(DateTime.now().toUtc());
      if (remaining.isNegative) {
        _countdownTimer?.cancel();
        if (mounted) Navigator.pop(context);
      } else {
        setState(() => _remaining = remaining);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text('Link Telegram',
                    style: theme.textTheme.titleMedium),
                const SizedBox(height: 16),
                if (_linkCode != null) ...[
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: theme.colorScheme.surfaceContainerHighest,
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      _linkCode!.code,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.headlineMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        letterSpacing: 4,
                      ),
                    ),
                  ),
                  if (_remaining != null) ...[
                    const SizedBox(height: 8),
                    Text(
                      'Expires in ${_remaining!.inMinutes}:${(_remaining!.inSeconds.remainder(60)).toString().padLeft(2, '0')}',
                      style: theme.textTheme.bodySmall,
                    ),
                  ],
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton.icon(
                          onPressed: () {
                            Clipboard.setData(
                                ClipboardData(text: _linkCode!.code));
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(content: Text('Copied')),
                            );
                          },
                          icon: const Icon(Icons.copy, size: 18),
                          label: const Text('Copy'),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: FilledButton.icon(
                          onPressed: () {
                            launchUrl(Uri.parse(_linkCode!.deepLink));
                          },
                          icon: const Icon(Icons.open_in_new, size: 18),
                          label: const Text('Open TG'),
                        ),
                      ),
                    ],
                  ),
                ],
                const SizedBox(height: 16),
              ],
            ),
    );
  }
}

class _TeamFormSheet extends StatefulWidget {
  final Team? team;

  const _TeamFormSheet({this.team});

  @override
  State<_TeamFormSheet> createState() => _TeamFormSheetState();
}

class _TeamFormSheetState extends State<_TeamFormSheet> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nameCtrl;
  late final TextEditingController _emailsCtrl;
  late final TextEditingController _webhookUrlCtrl;
  bool _isSubmitting = false;

  bool get _isEditing => widget.team != null;

  @override
  void initState() {
    super.initState();
    _nameCtrl = TextEditingController(text: widget.team?.name ?? '');
    _emailsCtrl = TextEditingController(
      text: widget.team?.memberEmails.join(', ') ?? '',
    );
    _webhookUrlCtrl = TextEditingController(text: widget.team?.webhookUrl ?? '');
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _emailsCtrl.dispose();
    _webhookUrlCtrl.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isSubmitting = true);

    final emails = _emailsCtrl.text
        .split(',')
        .map((e) => e.trim())
        .where((e) => e.isNotEmpty)
        .toList();

    final webhookUrl = _webhookUrlCtrl.text.trim();
    final request = CreateTeamRequest(
      name: _nameCtrl.text.trim(),
      memberEmails: emails,
      webhookUrl: webhookUrl.isEmpty ? null : webhookUrl,
    );

    try {
      final provider = context.read<TeamProvider>();
      if (_isEditing) {
        await provider.updateTeam(widget.team!.id, request);
      } else {
        await provider.createTeam(request);
      }
      if (mounted) Navigator.pop(context);
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
              content:
                  Text('Failed to ${_isEditing ? "update" : "create"} team')),
        );
      }
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: EdgeInsets.only(
        left: 24,
        right: 24,
        top: 24,
        bottom: MediaQuery.of(context).viewInsets.bottom + 24,
      ),
      child: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              _isEditing ? 'Edit Team' : 'Add Team',
              style: theme.textTheme.titleMedium,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _nameCtrl,
              decoration: const InputDecoration(labelText: 'Team Name'),
              validator: (v) =>
                  v == null || v.trim().isEmpty ? 'Required' : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _emailsCtrl,
              decoration: const InputDecoration(
                labelText: 'Member Emails',
                hintText: 'Comma-separated emails',
              ),
              maxLines: 3,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _webhookUrlCtrl,
              decoration: const InputDecoration(
                labelText: 'Webhook URL',
                hintText: 'https://example.com/webhook',
              ),
              keyboardType: TextInputType.url,
            ),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: _isSubmitting ? null : _submit,
              child: _isSubmitting
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(_isEditing ? 'Update' : 'Create'),
            ),
          ],
        ),
      ),
    );
  }
}
