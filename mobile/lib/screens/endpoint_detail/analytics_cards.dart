import 'package:flutter/material.dart';
import '../../models/analytics.dart';

class AnalyticsCards extends StatelessWidget {
  final AnalyticsSummary analytics;

  const AnalyticsCards({super.key, required this.analytics});

  @override
  Widget build(BuildContext context) {
    return GridView.count(
      crossAxisCount: 2,
      crossAxisSpacing: 12,
      mainAxisSpacing: 12,
      childAspectRatio: 1.8,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      children: [
        _StatCard(
          label: 'Uptime',
          value: '${analytics.uptimePercentage.toStringAsFixed(2)}%',
          color: analytics.uptimePercentage >= 99
              ? Colors.green
              : analytics.uptimePercentage >= 95
                  ? Colors.amber
                  : Colors.red,
        ),
        _StatCard(
          label: 'Avg Response',
          value: '${analytics.avgResponseTimeMs.toStringAsFixed(0)} ms',
          color: Colors.blue,
        ),
        _StatCard(
          label: 'Total Checks',
          value: analytics.totalChecks.toString(),
          color: Colors.indigo,
        ),
        _StatCard(
          label: 'Incidents',
          value: analytics.totalIncidents.toString(),
          color: analytics.totalIncidents > 0 ? Colors.red : Colors.green,
        ),
      ],
    );
  }
}

class _StatCard extends StatelessWidget {
  final String label;
  final String value;
  final Color color;

  const _StatCard({
    required this.label,
    required this.value,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              value,
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
