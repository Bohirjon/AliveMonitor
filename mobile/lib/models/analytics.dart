class AnalyticsSummary {
  final double uptimePercentage;
  final double avgResponseTimeMs;
  final int totalChecks;
  final int totalIncidents;

  AnalyticsSummary({
    required this.uptimePercentage,
    required this.avgResponseTimeMs,
    required this.totalChecks,
    required this.totalIncidents,
  });

  factory AnalyticsSummary.fromJson(Map<String, dynamic> json) =>
      AnalyticsSummary(
        uptimePercentage: (json['uptimePercentage'] as num).toDouble(),
        avgResponseTimeMs: (json['avgResponseTimeMs'] as num).toDouble(),
        totalChecks: json['totalChecks'] as int,
        totalIncidents: json['totalIncidents'] as int,
      );
}

class HealthCheckLog {
  final String id;
  final String endpointId;
  final DateTime checkedAt;
  final int? httpStatusCode;
  final double responseTimeMs;
  final bool isHealthy;
  final String? errorMessage;

  HealthCheckLog({
    required this.id,
    required this.endpointId,
    required this.checkedAt,
    this.httpStatusCode,
    required this.responseTimeMs,
    required this.isHealthy,
    this.errorMessage,
  });

  factory HealthCheckLog.fromJson(Map<String, dynamic> json) =>
      HealthCheckLog(
        id: json['id'] as String,
        endpointId: json['endpointId'] as String,
        checkedAt: DateTime.parse(json['checkedAt'] as String),
        httpStatusCode: json['httpStatusCode'] as int?,
        responseTimeMs: (json['responseTimeMs'] as num).toDouble(),
        isHealthy: json['isHealthy'] as bool,
        errorMessage: json['errorMessage'] as String?,
      );
}

class Incident {
  final String id;
  final String endpointId;
  final DateTime openedAt;
  final DateTime lastNotifiedAt;
  final DateTime? resolvedAt;
  final int failureCount;

  Incident({
    required this.id,
    required this.endpointId,
    required this.openedAt,
    required this.lastNotifiedAt,
    this.resolvedAt,
    required this.failureCount,
  });

  factory Incident.fromJson(Map<String, dynamic> json) => Incident(
        id: json['id'] as String,
        endpointId: json['endpointId'] as String,
        openedAt: DateTime.parse(json['openedAt'] as String),
        lastNotifiedAt: DateTime.parse(json['lastNotifiedAt'] as String),
        resolvedAt: json['resolvedAt'] != null
            ? DateTime.parse(json['resolvedAt'] as String)
            : null,
        failureCount: json['failureCount'] as int,
      );
}

class PaginatedResponse<T> {
  final List<T> items;
  final int totalCount;
  final int page;
  final int pageSize;
  final int totalPages;

  PaginatedResponse({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.totalPages,
  });
}
