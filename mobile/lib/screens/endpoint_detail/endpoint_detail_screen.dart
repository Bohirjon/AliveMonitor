import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../models/analytics.dart';
import '../../models/monitored_endpoint.dart';
import '../../services/analytics_service.dart';
import '../../services/api_client.dart';
import '../../services/endpoint_service.dart';
import '../../widgets/status_badge.dart';
import 'package:provider/provider.dart';
import 'analytics_cards.dart';
import 'response_time_chart.dart';
import 'check_log_list.dart';
import 'incident_list.dart';

class EndpointDetailScreen extends StatefulWidget {
  final String endpointId;

  const EndpointDetailScreen({super.key, required this.endpointId});

  @override
  State<EndpointDetailScreen> createState() => _EndpointDetailScreenState();
}

class _EndpointDetailScreenState extends State<EndpointDetailScreen> {
  late final AnalyticsService _analyticsService;
  late final EndpointService _endpointService;

  MonitoredEndpoint? _endpoint;
  AnalyticsSummary? _analytics;
  List<HealthCheckLog> _checkLogs = [];
  List<Incident> _incidents = [];
  bool _isLoading = true;
  String? _error;

  // Date range
  String _selectedRange = '24h';
  late DateTime _from;
  late DateTime _to;

  // Pagination
  int _page = 1;
  int _totalPages = 1;

  @override
  void initState() {
    super.initState();
    final api = context.read<ApiClient>();
    _analyticsService = AnalyticsService(api);
    _endpointService = EndpointService(api);
    _setDateRange('24h');
    _loadData();
  }

  void _setDateRange(String range) {
    _to = DateTime.now().toUtc();
    switch (range) {
      case '24h':
        _from = _to.subtract(const Duration(hours: 24));
        break;
      case '7d':
        _from = _to.subtract(const Duration(days: 7));
        break;
      case '30d':
        _from = _to.subtract(const Duration(days: 30));
        break;
    }
    _selectedRange = range;
    _page = 1;
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final fromStr = _from.toIso8601String();
      final toStr = _to.toIso8601String();

      final results = await Future.wait([
        _endpointService.getEndpoint(widget.endpointId),
        _analyticsService.getAnalytics(widget.endpointId,
            from: fromStr, to: toStr),
        _analyticsService.getCheckLogs(widget.endpointId,
            from: fromStr, to: toStr, page: _page),
        _analyticsService.getIncidents(widget.endpointId,
            from: fromStr, to: toStr),
      ]);

      _endpoint = results[0] as MonitoredEndpoint;
      _analytics = results[1] as AnalyticsSummary;
      final checkLogsResponse =
          results[2] as PaginatedResponse<HealthCheckLog>;
      _checkLogs = checkLogsResponse.items;
      _totalPages = checkLogsResponse.totalPages;
      _incidents = results[3] as List<Incident>;
    } catch (e, stack) {
      debugPrint('=== Endpoint Detail Load Error ===');
      debugPrint('Error: $e');
      debugPrint('Stack: $stack');
      _error = 'Failed to load endpoint details';
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _loadCheckLogs() async {
    try {
      final result = await _analyticsService.getCheckLogs(
        widget.endpointId,
        from: _from.toIso8601String(),
        to: _to.toIso8601String(),
        page: _page,
      );
      setState(() {
        _checkLogs = result.items;
        _totalPages = result.totalPages;
      });
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (_isLoading && _endpoint == null) {
      return Scaffold(
        appBar: AppBar(),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null && _endpoint == null) {
      return Scaffold(
        appBar: AppBar(),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_error!, style: TextStyle(color: theme.colorScheme.error)),
              const SizedBox(height: 16),
              FilledButton(onPressed: _loadData, child: const Text('Retry')),
            ],
          ),
        ),
      );
    }

    final ep = _endpoint!;

    return Scaffold(
      appBar: AppBar(
        title: Text(ep.friendlyName),
      ),
      body: RefreshIndicator(
        onRefresh: _loadData,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // Header info
            Row(
              children: [
                Expanded(
                  child: Text(
                    ep.url,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                StatusBadge(status: ep.currentStatus),
              ],
            ),
            const SizedBox(height: 16),

            // SSL Card
            if (ep.sslCheckEnabled) ...[
              _SslCard(endpoint: ep),
              const SizedBox(height: 16),
            ],

            // Date range picker
            Row(
              children: ['24h', '7d', '30d'].map((range) {
                final isSelected = _selectedRange == range;
                return Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: ChoiceChip(
                    label: Text(range),
                    selected: isSelected,
                    onSelected: (_) {
                      _setDateRange(range);
                      _loadData();
                    },
                  ),
                );
              }).toList(),
            ),
            const SizedBox(height: 16),

            // Analytics summary cards
            if (_analytics != null)
              AnalyticsCards(analytics: _analytics!),
            const SizedBox(height: 16),

            // Response time chart
            if (_checkLogs.isNotEmpty) ...[
              Text('Response Time',
                  style: theme.textTheme.titleMedium
                      ?.copyWith(fontWeight: FontWeight.w600)),
              const SizedBox(height: 8),
              SizedBox(
                height: 200,
                child: ResponseTimeChart(checkLogs: _checkLogs),
              ),
              const SizedBox(height: 24),
            ],

            // Check logs
            Text('Check Log',
                style: theme.textTheme.titleMedium
                    ?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            CheckLogList(checkLogs: _checkLogs),
            if (_totalPages > 1)
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  IconButton(
                    onPressed: _page > 1
                        ? () {
                            setState(() => _page--);
                            _loadCheckLogs();
                          }
                        : null,
                    icon: const Icon(Icons.chevron_left),
                  ),
                  Text('$_page / $_totalPages'),
                  IconButton(
                    onPressed: _page < _totalPages
                        ? () {
                            setState(() => _page++);
                            _loadCheckLogs();
                          }
                        : null,
                    icon: const Icon(Icons.chevron_right),
                  ),
                ],
              ),
            const SizedBox(height: 24),

            // Incidents
            Text('Incident History',
                style: theme.textTheme.titleMedium
                    ?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            IncidentList(incidents: _incidents),
          ],
        ),
      ),
    );
  }
}

class _SslCard extends StatelessWidget {
  final MonitoredEndpoint endpoint;

  const _SslCard({required this.endpoint});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final days = endpoint.sslDaysUntilExpiry;

    Color statusColor;
    String statusText;
    if (days == null) {
      statusColor = Colors.grey;
      statusText = 'Pending';
    } else if (days <= 1) {
      statusColor = Colors.red;
      statusText = 'Critical';
    } else if (days <= 7) {
      statusColor = Colors.orange;
      statusText = 'Expiring Soon';
    } else if (days <= 30) {
      statusColor = Colors.amber;
      statusText = 'Expiring Soon';
    } else {
      statusColor = Colors.green;
      statusText = 'Valid';
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.lock, color: statusColor, size: 20),
                const SizedBox(width: 8),
                Text('SSL Certificate',
                    style: theme.textTheme.titleSmall
                        ?.copyWith(fontWeight: FontWeight.w600)),
                const Spacer(),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: statusColor.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(statusText,
                      style: TextStyle(
                          color: statusColor,
                          fontSize: 12,
                          fontWeight: FontWeight.w600)),
                ),
              ],
            ),
            if (endpoint.sslCertificateExpiresAt != null) ...[
              const SizedBox(height: 8),
              Text(
                'Expires: ${DateFormat('MMM d, yyyy').format(endpoint.sslCertificateExpiresAt!.toLocal())}',
                style: theme.textTheme.bodySmall,
              ),
            ],
            if (days != null) ...[
              const SizedBox(height: 4),
              Text(
                '$days days remaining',
                style: theme.textTheme.bodySmall?.copyWith(color: statusColor),
              ),
            ],
            if (endpoint.sslLastCheckedAt != null) ...[
              const SizedBox(height: 4),
              Text(
                'Last checked: ${DateFormat('MMM d, HH:mm').format(endpoint.sslLastCheckedAt!.toLocal())}',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
