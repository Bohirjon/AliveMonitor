import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/user.dart';
import '../../providers/auth_provider.dart';
import '../../services/api_client.dart';
import '../../services/settings_service.dart';

class WebhookSection extends StatefulWidget {
  final User user;

  const WebhookSection({super.key, required this.user});

  @override
  State<WebhookSection> createState() => _WebhookSectionState();
}

class _WebhookSectionState extends State<WebhookSection> {
  late final TextEditingController _urlCtrl;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _urlCtrl = TextEditingController(text: widget.user.webhookUrl ?? '');
  }

  @override
  void didUpdateWidget(covariant WebhookSection oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.user.webhookUrl != widget.user.webhookUrl) {
      _urlCtrl.text = widget.user.webhookUrl ?? '';
    }
  }

  @override
  void dispose() {
    _urlCtrl.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() => _isSaving = true);
    try {
      final service = SettingsService(context.read<ApiClient>());
      final url = _urlCtrl.text.trim();
      await service.updateWebhookUrl(url.isEmpty ? null : url);
      if (mounted) {
        context.read<AuthProvider>().updateUser(
              widget.user.copyWith(webhookUrl: url.isEmpty ? null : url),
            );
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Webhook URL updated')),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to update webhook URL')),
        );
      }
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Webhook Notifications',
                style: theme.textTheme.titleSmall
                    ?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            Text(
              'Receive a JSON POST request when an endpoint status changes.',
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _urlCtrl,
              decoration: const InputDecoration(
                labelText: 'Webhook URL',
                hintText: 'https://example.com/webhook',
              ),
              keyboardType: TextInputType.url,
            ),
            const SizedBox(height: 12),
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton(
                onPressed: _isSaving ? null : _save,
                child: _isSaving
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Save'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
