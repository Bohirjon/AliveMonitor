import '../models/monitored_endpoint.dart';
import 'api_client.dart';

class EndpointService {
  final ApiClient _api;

  EndpointService(this._api);

  Future<List<MonitoredEndpoint>> getEndpoints({
    String? search,
    String? status,
  }) async {
    final params = <String, String>{};
    if (search != null && search.isNotEmpty) params['search'] = search;
    if (status != null && status != 'All') params['status'] = status;

    final response = await _api.dio.get(
      '/endpoints',
      queryParameters: params,
    );
    return (response.data as List)
        .map((e) => MonitoredEndpoint.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<MonitoredEndpoint> getEndpoint(String id) async {
    final response = await _api.dio.get('/endpoints/$id');
    return MonitoredEndpoint.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<MonitoredEndpoint> createEndpoint(
      CreateEndpointRequest request) async {
    final response = await _api.dio.post('/endpoints', data: request.toJson());
    return MonitoredEndpoint.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<MonitoredEndpoint> updateEndpoint(
      String id, CreateEndpointRequest request) async {
    final response =
        await _api.dio.put('/endpoints/$id', data: request.toJson());
    return MonitoredEndpoint.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<void> deleteEndpoint(String id) async {
    await _api.dio.delete('/endpoints/$id');
  }

  Future<MonitoredEndpoint> toggleEndpoint(String id) async {
    final response = await _api.dio.patch('/endpoints/$id/toggle');
    return MonitoredEndpoint.fromJson(
        response.data as Map<String, dynamic>);
  }
}
