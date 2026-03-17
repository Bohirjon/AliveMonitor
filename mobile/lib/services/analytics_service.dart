import 'package:flutter/foundation.dart';
import '../models/analytics.dart';
import 'api_client.dart';

class AnalyticsService {
  final ApiClient _api;

  AnalyticsService(this._api);

  Future<AnalyticsSummary> getAnalytics(
    String endpointId, {
    String? from,
    String? to,
  }) async {
    final params = <String, String>{};
    if (from != null) params['from'] = from;
    if (to != null) params['to'] = to;

    final response = await _api.dio.get(
      '/endpoints/$endpointId/analytics',
      queryParameters: params,
    );
    return AnalyticsSummary.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<PaginatedResponse<HealthCheckLog>> getCheckLogs(
    String endpointId, {
    String? from,
    String? to,
    int page = 1,
    int pageSize = 20,
  }) async {
    final params = <String, String>{
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    };
    if (from != null) params['from'] = from;
    if (to != null) params['to'] = to;

    final response = await _api.dio.get(
      '/endpoints/$endpointId/checks',
      queryParameters: params,
    );
    debugPrint('=== CheckLogs raw response ===');
    debugPrint('${response.data}');
    final data = response.data as Map<String, dynamic>;
    return PaginatedResponse<HealthCheckLog>(
      items: (data['items'] as List)
          .map((e) => HealthCheckLog.fromJson(e as Map<String, dynamic>))
          .toList(),
      totalCount: data['totalCount'] as int,
      page: data['page'] as int,
      pageSize: data['pageSize'] as int,
      totalPages: data['totalPages'] as int,
    );
  }

  Future<List<Incident>> getIncidents(
    String endpointId, {
    String? from,
    String? to,
  }) async {
    final params = <String, String>{};
    if (from != null) params['from'] = from;
    if (to != null) params['to'] = to;

    final response = await _api.dio.get(
      '/endpoints/$endpointId/incidents',
      queryParameters: params,
    );
    debugPrint('=== Incidents raw response ===');
    debugPrint('${response.data}');
    return (response.data as List)
        .map((e) => Incident.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
