import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../models/analytics.dart';

class ResponseTimeChart extends StatelessWidget {
  final List<HealthCheckLog> checkLogs;

  const ResponseTimeChart({super.key, required this.checkLogs});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (checkLogs.isEmpty) {
      return const Center(child: Text('No data'));
    }

    // Sort by time ascending
    final sorted = List<HealthCheckLog>.from(checkLogs)
      ..sort((a, b) => a.checkedAt.compareTo(b.checkedAt));

    final spots = sorted.asMap().entries.map((e) {
      return FlSpot(
        e.key.toDouble(),
        e.value.responseTimeMs,
      );
    }).toList();

    final maxY = sorted.fold<double>(
            0, (max, log) => log.responseTimeMs > max ? log.responseTimeMs : max) *
        1.2;

    return LineChart(
      LineChartData(
        gridData: FlGridData(
          show: true,
          drawVerticalLine: false,
          horizontalInterval: maxY > 0 ? maxY / 4 : 1,
          getDrawingHorizontalLine: (value) => FlLine(
            color: theme.colorScheme.outlineVariant.withValues(alpha: 0.3),
            strokeWidth: 1,
          ),
        ),
        titlesData: FlTitlesData(
          rightTitles:
              const AxisTitles(sideTitles: SideTitles(showTitles: false)),
          topTitles:
              const AxisTitles(sideTitles: SideTitles(showTitles: false)),
          bottomTitles: AxisTitles(
            sideTitles: SideTitles(
              showTitles: true,
              reservedSize: 30,
              interval: (sorted.length / 4).ceilToDouble().clamp(1, double.infinity),
              getTitlesWidget: (value, meta) {
                final idx = value.toInt();
                if (idx < 0 || idx >= sorted.length) {
                  return const SizedBox.shrink();
                }
                return Padding(
                  padding: const EdgeInsets.only(top: 8),
                  child: Text(
                    DateFormat('HH:mm')
                        .format(sorted[idx].checkedAt.toLocal()),
                    style: theme.textTheme.bodySmall?.copyWith(
                      fontSize: 10,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                );
              },
            ),
          ),
          leftTitles: AxisTitles(
            sideTitles: SideTitles(
              showTitles: true,
              reservedSize: 50,
              getTitlesWidget: (value, meta) {
                return Text(
                  '${value.toInt()} ms',
                  style: theme.textTheme.bodySmall?.copyWith(
                    fontSize: 10,
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                );
              },
            ),
          ),
        ),
        borderData: FlBorderData(show: false),
        lineBarsData: [
          LineChartBarData(
            spots: spots,
            isCurved: true,
            color: theme.colorScheme.primary,
            barWidth: 2,
            isStrokeCapRound: true,
            dotData: const FlDotData(show: false),
            belowBarData: BarAreaData(
              show: true,
              color: theme.colorScheme.primary.withValues(alpha: 0.1),
            ),
          ),
        ],
        lineTouchData: LineTouchData(
          touchTooltipData: LineTouchTooltipData(
            getTooltipItems: (spots) {
              return spots.map((spot) {
                final idx = spot.x.toInt();
                if (idx < 0 || idx >= sorted.length) return null;
                final log = sorted[idx];
                return LineTooltipItem(
                  '${log.responseTimeMs.toStringAsFixed(0)} ms\n${DateFormat('MM/dd HH:mm').format(log.checkedAt.toLocal())}',
                  TextStyle(
                    color: theme.colorScheme.onPrimary,
                    fontSize: 12,
                  ),
                );
              }).toList();
            },
          ),
        ),
        minY: 0,
        maxY: maxY > 0 ? maxY : 100,
      ),
    );
  }
}
