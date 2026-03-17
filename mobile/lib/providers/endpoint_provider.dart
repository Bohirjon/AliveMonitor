import 'package:flutter/material.dart';
import '../models/monitored_endpoint.dart';
import '../services/endpoint_service.dart';

class EndpointProvider extends ChangeNotifier {
  final EndpointService _service;

  List<MonitoredEndpoint> _endpoints = [];
  bool _isLoading = false;
  String? _error;
  String _searchQuery = '';
  String _statusFilter = 'All';

  List<MonitoredEndpoint> get endpoints => _endpoints;
  bool get isLoading => _isLoading;
  String? get error => _error;
  String get searchQuery => _searchQuery;
  String get statusFilter => _statusFilter;

  EndpointProvider(this._service);

  Future<void> fetchEndpoints() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      _endpoints = await _service.getEndpoints(
        search: _searchQuery.isNotEmpty ? _searchQuery : null,
        status: _statusFilter != 'All' ? _statusFilter : null,
      );
    } catch (e) {
      _error = 'Failed to load endpoints';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void setSearchQuery(String query) {
    _searchQuery = query;
    fetchEndpoints();
  }

  void setStatusFilter(String filter) {
    _statusFilter = filter;
    fetchEndpoints();
  }

  Future<MonitoredEndpoint> createEndpoint(
      CreateEndpointRequest request) async {
    final endpoint = await _service.createEndpoint(request);
    await fetchEndpoints();
    return endpoint;
  }

  Future<MonitoredEndpoint> updateEndpoint(
      String id, CreateEndpointRequest request) async {
    final endpoint = await _service.updateEndpoint(id, request);
    await fetchEndpoints();
    return endpoint;
  }

  Future<void> deleteEndpoint(String id) async {
    await _service.deleteEndpoint(id);
    await fetchEndpoints();
  }

  Future<void> toggleEndpoint(String id) async {
    await _service.toggleEndpoint(id);
    await fetchEndpoints();
  }
}
