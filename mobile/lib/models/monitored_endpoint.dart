enum EndpointStatus { healthy, unhealthy, disabled }

EndpointStatus endpointStatusFromString(String s) {
  switch (s) {
    case 'Healthy':
      return EndpointStatus.healthy;
    case 'Unhealthy':
      return EndpointStatus.unhealthy;
    case 'Disabled':
      return EndpointStatus.disabled;
    default:
      return EndpointStatus.disabled;
  }
}

String endpointStatusToString(EndpointStatus s) {
  switch (s) {
    case EndpointStatus.healthy:
      return 'Healthy';
    case EndpointStatus.unhealthy:
      return 'Unhealthy';
    case EndpointStatus.disabled:
      return 'Disabled';
  }
}

class MonitoredEndpoint {
  final String id;
  final String friendlyName;
  final String url;
  final int intervalMinutes;
  final int timeoutSeconds;
  final bool isEnabled;
  final Map<String, String>? customHeaders;
  final int expectedStatusCode;
  final String? jsonPropertyName;
  final String? jsonPropertyExpectedValue;
  final EndpointStatus currentStatus;
  final DateTime? lastCheckedAt;
  final DateTime createdAt;
  final DateTime updatedAt;
  final String? teamId;
  final String? teamName;
  final bool sslCheckEnabled;
  final DateTime? sslLastCheckedAt;
  final DateTime? sslCertificateExpiresAt;
  final int? sslDaysUntilExpiry;

  MonitoredEndpoint({
    required this.id,
    required this.friendlyName,
    required this.url,
    required this.intervalMinutes,
    required this.timeoutSeconds,
    required this.isEnabled,
    this.customHeaders,
    required this.expectedStatusCode,
    this.jsonPropertyName,
    this.jsonPropertyExpectedValue,
    required this.currentStatus,
    this.lastCheckedAt,
    required this.createdAt,
    required this.updatedAt,
    this.teamId,
    this.teamName,
    required this.sslCheckEnabled,
    this.sslLastCheckedAt,
    this.sslCertificateExpiresAt,
    this.sslDaysUntilExpiry,
  });

  factory MonitoredEndpoint.fromJson(Map<String, dynamic> json) {
    return MonitoredEndpoint(
      id: json['id'] as String,
      friendlyName: json['friendlyName'] as String,
      url: json['url'] as String,
      intervalMinutes: json['intervalMinutes'] as int,
      timeoutSeconds: json['timeoutSeconds'] as int,
      isEnabled: json['isEnabled'] as bool,
      customHeaders: json['customHeaders'] != null
          ? Map<String, String>.from(json['customHeaders'] as Map)
          : null,
      expectedStatusCode: json['expectedStatusCode'] as int,
      jsonPropertyName: json['jsonPropertyName'] as String?,
      jsonPropertyExpectedValue:
          json['jsonPropertyExpectedValue'] as String?,
      currentStatus:
          endpointStatusFromString(json['currentStatus'] as String),
      lastCheckedAt: json['lastCheckedAt'] != null
          ? DateTime.parse(json['lastCheckedAt'] as String)
          : null,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
      teamId: json['teamId'] as String?,
      teamName: json['teamName'] as String?,
      sslCheckEnabled: json['sslCheckEnabled'] as bool? ?? false,
      sslLastCheckedAt: json['sslLastCheckedAt'] != null
          ? DateTime.parse(json['sslLastCheckedAt'] as String)
          : null,
      sslCertificateExpiresAt: json['sslCertificateExpiresAt'] != null
          ? DateTime.parse(json['sslCertificateExpiresAt'] as String)
          : null,
      sslDaysUntilExpiry: json['sslDaysUntilExpiry'] as int?,
    );
  }
}

class CreateEndpointRequest {
  final String friendlyName;
  final String url;
  final int intervalMinutes;
  final int timeoutSeconds;
  final Map<String, String>? customHeaders;
  final int expectedStatusCode;
  final String? jsonPropertyName;
  final String? jsonPropertyExpectedValue;
  final String? teamId;
  final bool sslCheckEnabled;

  CreateEndpointRequest({
    required this.friendlyName,
    required this.url,
    required this.intervalMinutes,
    required this.timeoutSeconds,
    this.customHeaders,
    required this.expectedStatusCode,
    this.jsonPropertyName,
    this.jsonPropertyExpectedValue,
    this.teamId,
    this.sslCheckEnabled = false,
  });

  Map<String, dynamic> toJson() => {
        'friendlyName': friendlyName,
        'url': url,
        'intervalMinutes': intervalMinutes,
        'timeoutSeconds': timeoutSeconds,
        if (customHeaders != null && customHeaders!.isNotEmpty)
          'customHeaders': customHeaders,
        'expectedStatusCode': expectedStatusCode,
        if (jsonPropertyName != null) 'jsonPropertyName': jsonPropertyName,
        if (jsonPropertyExpectedValue != null)
          'jsonPropertyExpectedValue': jsonPropertyExpectedValue,
        if (teamId != null) 'teamId': teamId,
        'sslCheckEnabled': sslCheckEnabled,
      };
}
