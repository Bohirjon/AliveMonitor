import 'package:flutter/material.dart';
import '../models/monitored_endpoint.dart';

class StatusBadge extends StatelessWidget {
  final EndpointStatus status;

  const StatusBadge({super.key, required this.status});

  @override
  Widget build(BuildContext context) {
    final (color, label) = switch (status) {
      EndpointStatus.healthy => (Colors.green, 'Healthy'),
      EndpointStatus.unhealthy => (Colors.red, 'Unhealthy'),
      EndpointStatus.disabled => (Colors.grey, 'Disabled'),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: color,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}

class StatusDot extends StatelessWidget {
  final EndpointStatus status;
  final double size;

  const StatusDot({super.key, required this.status, this.size = 8});

  @override
  Widget build(BuildContext context) {
    final color = switch (status) {
      EndpointStatus.healthy => Colors.green,
      EndpointStatus.unhealthy => Colors.red,
      EndpointStatus.disabled => Colors.grey,
    };

    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: color,
        shape: BoxShape.circle,
      ),
    );
  }
}
