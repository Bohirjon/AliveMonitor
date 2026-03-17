import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../models/team.dart';
import '../../providers/auth_provider.dart';
import '../../services/api_client.dart';
import '../../services/telegram_service.dart';

class TelegramSection extends StatefulWidget {
  const TelegramSection({super.key});

  @override
  State<TelegramSection> createState() => _TelegramSectionState();
}

class _TelegramSectionState extends State<TelegramSection> {
  late final TelegramService _telegramService;
  TelegramStatusResponse? _status;
  LinkCodeResponse? _linkCode;
  bool _isLoading = true;
  bool _isGenerating = false;
  Timer? _countdownTimer;
  Duration? _remaining;

  @override
  void initState() {
    super.initState();
    _telegramService = TelegramService(context.read<ApiClient>());
    _loadStatus();
  }

  @override
  void dispose() {
    _countdownTimer?.cancel();
    super.dispose();
  }

  Future<void> _loadStatus() async {
    try {
      final status = await _telegramService.getStatus();
      if (mounted) setState(() => _status = status);
    } catch (_) {}
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _generateCode() async {
    setState(() => _isGenerating = true);
    try {
      final code = await _telegramService.generateLinkCode();
      setState(() => _linkCode = code);
      _startCountdown();
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to generate link code')),
        );
      }
    } finally {
      if (mounted) setState(() => _isGenerating = false);
    }
  }

  void _startCountdown() {
    _countdownTimer?.cancel();
    _countdownTimer = Timer.periodic(const Duration(seconds: 1), (_) {
      if (_linkCode == null) {
        _countdownTimer?.cancel();
        return;
      }
      final remaining =
          _linkCode!.expiresAt.difference(DateTime.now().toUtc());
      if (remaining.isNegative) {
        _countdownTimer?.cancel();
        setState(() {
          _linkCode = null;
          _remaining = null;
        });
      } else {
        setState(() => _remaining = remaining);
      }
    });
  }

  Future<void> _unlink() async {
    try {
      await _telegramService.unlink();
      setState(() {
        _status = TelegramStatusResponse(isLinked: false);
        _linkCode = null;
      });
      if (mounted) {
        context.read<AuthProvider>().updateUser(
              context.read<AuthProvider>().user!.copyWith(telegramLinked: false),
            );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to unlink Telegram')),
        );
      }
    }
  }

  String _formatRemaining() {
    if (_remaining == null) return '';
    final m = _remaining!.inMinutes;
    final s = _remaining!.inSeconds.remainder(60);
    return '$m:${s.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isLinked = _status?.isLinked ?? false;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Text('Telegram Notifications',
                    style: theme.textTheme.titleSmall
                        ?.copyWith(fontWeight: FontWeight.w600)),
                const SizedBox(width: 8),
                if (isLinked)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.green.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Text('Linked',
                        style: TextStyle(
                            color: Colors.green,
                            fontSize: 11,
                            fontWeight: FontWeight.w600)),
                  ),
              ],
            ),
            const SizedBox(height: 12),
            if (_isLoading)
              const Center(child: CircularProgressIndicator())
            else if (isLinked)
              FilledButton.tonal(
                onPressed: _unlink,
                child: const Text('Unlink Telegram'),
              )
            else ...[
              if (_linkCode != null) ...[
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.surfaceContainerHighest,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Column(
                    children: [
                      Text(
                        _linkCode!.code,
                        style: theme.textTheme.headlineMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          letterSpacing: 4,
                        ),
                      ),
                      const SizedBox(height: 8),
                      if (_remaining != null)
                        Text(
                          'Expires in ${_formatRemaining()}',
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                    ],
                  ),
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: () {
                          Clipboard.setData(
                              ClipboardData(text: _linkCode!.code));
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                                content: Text('Code copied to clipboard')),
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
                        label: const Text('Open Telegram'),
                      ),
                    ),
                  ],
                ),
              ] else
                FilledButton(
                  onPressed: _isGenerating ? null : _generateCode,
                  child: _isGenerating
                      ? const SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Link Telegram'),
                ),
            ],
          ],
        ),
      ),
    );
  }
}
