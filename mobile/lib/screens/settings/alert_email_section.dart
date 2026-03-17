import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/user.dart';
import '../../providers/auth_provider.dart';
import '../../services/api_client.dart';
import '../../services/settings_service.dart';

class AlertEmailSection extends StatefulWidget {
  final User user;

  const AlertEmailSection({super.key, required this.user});

  @override
  State<AlertEmailSection> createState() => _AlertEmailSectionState();
}

class _AlertEmailSectionState extends State<AlertEmailSection> {
  late final TextEditingController _emailCtrl;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _emailCtrl = TextEditingController(text: widget.user.alertEmail);
  }

  @override
  void didUpdateWidget(covariant AlertEmailSection oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.user.alertEmail != widget.user.alertEmail) {
      _emailCtrl.text = widget.user.alertEmail;
    }
  }

  @override
  void dispose() {
    _emailCtrl.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() => _isSaving = true);
    try {
      final service = SettingsService(context.read<ApiClient>());
      await service.updateAlertEmail(_emailCtrl.text.trim());
      if (mounted) {
        context.read<AuthProvider>().updateUser(
              widget.user.copyWith(alertEmail: _emailCtrl.text.trim()),
            );
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Alert email updated')),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to update alert email')),
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
            Text('Alert Notifications',
                style: theme.textTheme.titleSmall
                    ?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            TextField(
              controller: _emailCtrl,
              decoration: const InputDecoration(
                labelText: 'Alert email',
                hintText: 'Enter alert email address',
              ),
              keyboardType: TextInputType.emailAddress,
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
