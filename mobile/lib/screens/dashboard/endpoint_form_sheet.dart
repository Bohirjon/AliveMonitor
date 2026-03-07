import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/monitored_endpoint.dart';
import '../../providers/endpoint_provider.dart';
import '../../providers/team_provider.dart';

class EndpointFormSheet extends StatefulWidget {
  final MonitoredEndpoint? endpoint;

  const EndpointFormSheet({super.key, this.endpoint});

  @override
  State<EndpointFormSheet> createState() => _EndpointFormSheetState();
}

class _EndpointFormSheetState extends State<EndpointFormSheet> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nameCtrl;
  late final TextEditingController _urlCtrl;
  late final TextEditingController _intervalCtrl;
  late final TextEditingController _timeoutCtrl;
  late final TextEditingController _statusCodeCtrl;
  late final TextEditingController _jsonPropCtrl;
  late final TextEditingController _jsonValueCtrl;

  String? _selectedTeamId;
  bool _sslCheckEnabled = false;
  List<MapEntry<TextEditingController, TextEditingController>> _headers = [];
  bool _isSubmitting = false;

  bool get _isEditing => widget.endpoint != null;

  @override
  void initState() {
    super.initState();
    final ep = widget.endpoint;
    _nameCtrl = TextEditingController(text: ep?.friendlyName ?? '');
    _urlCtrl = TextEditingController(text: ep?.url ?? '');
    _intervalCtrl =
        TextEditingController(text: (ep?.intervalMinutes ?? 5).toString());
    _timeoutCtrl =
        TextEditingController(text: (ep?.timeoutSeconds ?? 30).toString());
    _statusCodeCtrl =
        TextEditingController(text: (ep?.expectedStatusCode ?? 200).toString());
    _jsonPropCtrl = TextEditingController(text: ep?.jsonPropertyName ?? '');
    _jsonValueCtrl =
        TextEditingController(text: ep?.jsonPropertyExpectedValue ?? '');
    _selectedTeamId = ep?.teamId;
    _sslCheckEnabled = ep?.sslCheckEnabled ?? false;

    if (ep?.customHeaders != null) {
      _headers = ep!.customHeaders!.entries
          .map((e) => MapEntry(
                TextEditingController(text: e.key),
                TextEditingController(text: e.value),
              ))
          .toList();
    }

    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<TeamProvider>().fetchTeams();
    });
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _urlCtrl.dispose();
    _intervalCtrl.dispose();
    _timeoutCtrl.dispose();
    _statusCodeCtrl.dispose();
    _jsonPropCtrl.dispose();
    _jsonValueCtrl.dispose();
    for (final h in _headers) {
      h.key.dispose();
      h.value.dispose();
    }
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSubmitting = true);

    final headers = <String, String>{};
    for (final h in _headers) {
      if (h.key.text.isNotEmpty) {
        headers[h.key.text] = h.value.text;
      }
    }

    final request = CreateEndpointRequest(
      friendlyName: _nameCtrl.text.trim(),
      url: _urlCtrl.text.trim(),
      intervalMinutes: int.parse(_intervalCtrl.text),
      timeoutSeconds: int.parse(_timeoutCtrl.text),
      expectedStatusCode: int.parse(_statusCodeCtrl.text),
      customHeaders: headers.isNotEmpty ? headers : null,
      jsonPropertyName:
          _jsonPropCtrl.text.isNotEmpty ? _jsonPropCtrl.text : null,
      jsonPropertyExpectedValue:
          _jsonValueCtrl.text.isNotEmpty ? _jsonValueCtrl.text : null,
      teamId: _selectedTeamId,
      sslCheckEnabled: _sslCheckEnabled,
    );

    try {
      final provider = context.read<EndpointProvider>();
      if (_isEditing) {
        await provider.updateEndpoint(widget.endpoint!.id, request);
      } else {
        await provider.createEndpoint(request);
      }
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
                'Failed to ${_isEditing ? "update" : "create"} endpoint'),
          ),
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
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      child: DraggableScrollableSheet(
        initialChildSize: 0.9,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, scrollController) {
          return Form(
            key: _formKey,
            child: ListView(
              controller: scrollController,
              padding: const EdgeInsets.all(24),
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      _isEditing ? 'Edit Endpoint' : 'Add Endpoint',
                      style: theme.textTheme.headlineSmall,
                    ),
                    IconButton(
                      onPressed: () => Navigator.pop(context),
                      icon: const Icon(Icons.close),
                    ),
                  ],
                ),
                const SizedBox(height: 24),
                TextFormField(
                  controller: _nameCtrl,
                  decoration:
                      const InputDecoration(labelText: 'Friendly Name'),
                  validator: (v) =>
                      v == null || v.trim().isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _urlCtrl,
                  decoration: const InputDecoration(labelText: 'URL'),
                  keyboardType: TextInputType.url,
                  validator: (v) {
                    if (v == null || v.trim().isEmpty) return 'Required';
                    if (!v.startsWith('http://') &&
                        !v.startsWith('https://')) {
                      return 'Must start with http:// or https://';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: TextFormField(
                        controller: _intervalCtrl,
                        decoration: const InputDecoration(
                            labelText: 'Interval (min)'),
                        keyboardType: TextInputType.number,
                        validator: (v) {
                          final n = int.tryParse(v ?? '');
                          if (n == null || n < 1) return 'Min: 1';
                          return null;
                        },
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: TextFormField(
                        controller: _timeoutCtrl,
                        decoration: const InputDecoration(
                            labelText: 'Timeout (sec)'),
                        keyboardType: TextInputType.number,
                        validator: (v) {
                          final n = int.tryParse(v ?? '');
                          if (n == null || n < 1) return 'Min: 1';
                          return null;
                        },
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _statusCodeCtrl,
                  decoration: const InputDecoration(
                      labelText: 'Expected Status Code'),
                  keyboardType: TextInputType.number,
                  validator: (v) {
                    final n = int.tryParse(v ?? '');
                    if (n == null || n < 100 || n > 599) {
                      return 'Enter valid HTTP status code';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _jsonPropCtrl,
                  decoration: const InputDecoration(
                    labelText: 'JSON Property Name (optional)',
                  ),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _jsonValueCtrl,
                  decoration: const InputDecoration(
                    labelText: 'Expected JSON Value (optional)',
                  ),
                ),
                const SizedBox(height: 16),
                // Team selector
                Consumer<TeamProvider>(
                  builder: (context, teamProvider, _) {
                    // Reset selection if team not in loaded list
                    final validTeamIds = teamProvider.teams.map((t) => t.id).toSet();
                    final effectiveTeamId =
                        _selectedTeamId != null && validTeamIds.contains(_selectedTeamId)
                            ? _selectedTeamId
                            : null;

                    return DropdownButtonFormField<String?>(
                      value: effectiveTeamId,
                      decoration: const InputDecoration(labelText: 'Team'),
                      items: [
                        const DropdownMenuItem(
                          value: null,
                          child: Text('No team'),
                        ),
                        ...teamProvider.teams.map((t) => DropdownMenuItem(
                              value: t.id,
                              child: Text(t.name),
                            )),
                      ],
                      onChanged: (v) =>
                          setState(() => _selectedTeamId = v),
                    );
                  },
                ),
                const SizedBox(height: 16),
                SwitchListTile(
                  title: const Text('SSL Certificate Monitoring'),
                  contentPadding: EdgeInsets.zero,
                  value: _sslCheckEnabled,
                  onChanged: (v) =>
                      setState(() => _sslCheckEnabled = v),
                ),
                const SizedBox(height: 16),
                // Custom headers
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('Custom Headers',
                        style: theme.textTheme.titleSmall),
                    IconButton(
                      onPressed: () {
                        setState(() {
                          _headers.add(MapEntry(
                            TextEditingController(),
                            TextEditingController(),
                          ));
                        });
                      },
                      icon: const Icon(Icons.add),
                      iconSize: 20,
                    ),
                  ],
                ),
                ..._headers.asMap().entries.map((entry) {
                  final idx = entry.key;
                  final header = entry.value;
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 8),
                    child: Row(
                      children: [
                        Expanded(
                          child: TextField(
                            controller: header.key,
                            decoration:
                                const InputDecoration(hintText: 'Key'),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: TextField(
                            controller: header.value,
                            decoration:
                                const InputDecoration(hintText: 'Value'),
                          ),
                        ),
                        IconButton(
                          onPressed: () {
                            header.key.dispose();
                            header.value.dispose();
                            setState(() => _headers.removeAt(idx));
                          },
                          icon: const Icon(Icons.remove_circle_outline,
                              size: 20),
                        ),
                      ],
                    ),
                  );
                }),
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: _isSubmitting ? null : _submit,
                  child: _isSubmitting
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child:
                              CircularProgressIndicator(strokeWidth: 2),
                        )
                      : Text(_isEditing ? 'Update' : 'Create'),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
