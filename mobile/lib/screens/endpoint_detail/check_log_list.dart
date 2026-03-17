import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../models/analytics.dart';

class CheckLogList extends StatelessWidget {
  final List<HealthCheckLog> checkLogs;

  const CheckLogList({super.key, required this.checkLogs});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (checkLogs.isEmpty) {
      return Padding(
        padding: const EdgeInsets.all(16),
        child: Center(
          child: Text(
            'No check logs for this period',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ),
      );
    }

    return Card(
      child: Column(
        children: checkLogs.asMap().entries.map((entry) {
          final log = entry.value;
          final isLast = entry.key == checkLogs.length - 1;

          return Column(
            children: [
              ListTile(
                dense: true,
                leading: Icon(
                  log.isHealthy ? Icons.check_circle : Icons.error,
                  color: log.isHealthy ? Colors.green : Colors.red,
                  size: 20,
                ),
                title: Row(
                  children: [
                    Text(
                      DateFormat('MMM d, HH:mm')
                          .format(log.checkedAt.toLocal()),
                      style: theme.textTheme.bodySmall,
                    ),
                    const Spacer(),
                    if (log.httpStatusCode != null)
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 6, vertical: 1),
                        decoration: BoxDecoration(
                          color: (log.httpStatusCode! >= 200 &&
                                      log.httpStatusCode! < 300
                                  ? Colors.green
                                  : Colors.red)
                              .withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          '${log.httpStatusCode}',
                          style: theme.textTheme.bodySmall?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    const SizedBox(width: 8),
                    Text(
                      '${log.responseTimeMs.toStringAsFixed(0)} ms',
                      style: theme.textTheme.bodySmall,
                    ),
                    if (log.retryAttempts > 0) ...[
                      const SizedBox(width: 8),
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 6, vertical: 1),
                        decoration: BoxDecoration(
                          color: Colors.orange.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          '${log.retryAttempts} retry${log.retryAttempts > 1 ? 'es' : ''}',
                          style: theme.textTheme.bodySmall?.copyWith(
                            fontWeight: FontWeight.w600,
                            color: Colors.orange,
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
                subtitle: log.errorMessage != null
                    ? Padding(
                        padding: const EdgeInsets.only(top: 4),
                        child: Text(
                          log.errorMessage!,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.error,
                          ),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      )
                    : null,
              ),
              if (!isLast) const Divider(height: 1),
            ],
          );
        }).toList(),
      ),
    );
  }
}
