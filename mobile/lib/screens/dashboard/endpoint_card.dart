import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../models/monitored_endpoint.dart';
import '../../widgets/status_badge.dart';

class EndpointCard extends StatelessWidget {
  final MonitoredEndpoint endpoint;
  final VoidCallback onTap;
  final VoidCallback onToggle;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const EndpointCard({
    super.key,
    required this.endpoint,
    required this.onTap,
    required this.onToggle,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  StatusDot(status: endpoint.currentStatus),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      endpoint.friendlyName,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  PopupMenuButton<String>(
                    onSelected: (value) {
                      switch (value) {
                        case 'toggle':
                          onToggle();
                          break;
                        case 'edit':
                          onEdit();
                          break;
                        case 'delete':
                          onDelete();
                          break;
                      }
                    },
                    itemBuilder: (context) => [
                      PopupMenuItem(
                        value: 'toggle',
                        child: Row(
                          children: [
                            Icon(
                              endpoint.isEnabled
                                  ? Icons.pause
                                  : Icons.play_arrow,
                              size: 20,
                            ),
                            const SizedBox(width: 8),
                            Text(endpoint.isEnabled
                                ? 'Disable'
                                : 'Enable'),
                          ],
                        ),
                      ),
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
                        value: 'delete',
                        child: Row(
                          children: [
                            Icon(Icons.delete, size: 20,
                                color: theme.colorScheme.error),
                            const SizedBox(width: 8),
                            Text('Delete',
                                style: TextStyle(
                                    color: theme.colorScheme.error)),
                          ],
                        ),
                      ),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 4),
              Text(
                endpoint.url,
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                overflow: TextOverflow.ellipsis,
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  StatusBadge(status: endpoint.currentStatus),
                  const SizedBox(width: 12),
                  Icon(Icons.schedule, size: 14,
                      color: theme.colorScheme.onSurfaceVariant),
                  const SizedBox(width: 4),
                  Text(
                    'Every ${endpoint.intervalMinutes}m',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                  if (endpoint.lastCheckedAt != null) ...[
                    const Spacer(),
                    Text(
                      DateFormat('MMM d, HH:mm')
                          .format(endpoint.lastCheckedAt!.toLocal()),
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ],
              ),
              if (endpoint.sslCheckEnabled) ...[
                const SizedBox(height: 8),
                _SslInfo(endpoint: endpoint),
              ],
              if (endpoint.teamName != null) ...[
                const SizedBox(height: 8),
                Row(
                  children: [
                    Icon(Icons.group, size: 14,
                        color: theme.colorScheme.onSurfaceVariant),
                    const SizedBox(width: 4),
                    Text(
                      endpoint.teamName!,
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _SslInfo extends StatelessWidget {
  final MonitoredEndpoint endpoint;

  const _SslInfo({required this.endpoint});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final days = endpoint.sslDaysUntilExpiry;

    Color sslColor;
    String sslText;

    if (days == null) {
      sslColor = Colors.grey;
      sslText = 'SSL: Pending';
    } else if (days <= 1) {
      sslColor = Colors.red;
      sslText = 'SSL: Expires in $days day(s)';
    } else if (days <= 7) {
      sslColor = Colors.orange;
      sslText = 'SSL: $days days left';
    } else if (days <= 30) {
      sslColor = Colors.amber;
      sslText = 'SSL: $days days left';
    } else {
      sslColor = Colors.green;
      sslText = 'SSL: Valid ($days days)';
    }

    return Row(
      children: [
        Icon(Icons.lock, size: 14, color: sslColor),
        const SizedBox(width: 4),
        Text(
          sslText,
          style: theme.textTheme.bodySmall?.copyWith(color: sslColor),
        ),
      ],
    );
  }
}
