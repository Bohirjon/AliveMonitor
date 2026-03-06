import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../models/analytics.dart';

class IncidentList extends StatelessWidget {
  final List<Incident> incidents;

  const IncidentList({super.key, required this.incidents});

  String _formatDuration(Duration duration) {
    if (duration.inDays > 0) {
      return '${duration.inDays}d ${duration.inHours.remainder(24)}h';
    } else if (duration.inHours > 0) {
      return '${duration.inHours}h ${duration.inMinutes.remainder(60)}m';
    } else {
      return '${duration.inMinutes}m';
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (incidents.isEmpty) {
      return Padding(
        padding: const EdgeInsets.all(16),
        child: Center(
          child: Text(
            'No incidents for this period',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ),
      );
    }

    return Card(
      child: Column(
        children: incidents.asMap().entries.map((entry) {
          final incident = entry.value;
          final isLast = entry.key == incidents.length - 1;
          final isResolved = incident.resolvedAt != null;

          final duration = isResolved
              ? incident.resolvedAt!.difference(incident.openedAt)
              : DateTime.now().toUtc().difference(incident.openedAt);

          return Column(
            children: [
              ListTile(
                dense: true,
                leading: Icon(
                  isResolved ? Icons.check_circle : Icons.warning,
                  color: isResolved ? Colors.green : Colors.orange,
                  size: 20,
                ),
                title: Row(
                  children: [
                    Expanded(
                      child: Text(
                        'Opened: ${DateFormat('MMM d, HH:mm').format(incident.openedAt.toLocal())}',
                        style: theme.textTheme.bodySmall,
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 6, vertical: 1),
                      decoration: BoxDecoration(
                        color: (isResolved ? Colors.green : Colors.orange)
                            .withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        isResolved ? 'Resolved' : 'Active',
                        style: TextStyle(
                          color: isResolved ? Colors.green : Colors.orange,
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
                subtitle: Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Row(
                    children: [
                      Text(
                        'Duration: ${_formatDuration(duration)}',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                      const SizedBox(width: 16),
                      Text(
                        'Failures: ${incident.failureCount}',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              if (!isLast) const Divider(height: 1),
            ],
          );
        }).toList(),
      ),
    );
  }
}
