import 'package:flutter/material.dart';
import '../models/team.dart';
import '../services/team_service.dart';

class TeamProvider extends ChangeNotifier {
  final TeamService _service;

  List<Team> _teams = [];
  bool _isLoading = false;
  String? _error;

  List<Team> get teams => _teams;
  bool get isLoading => _isLoading;
  String? get error => _error;

  TeamProvider(this._service);

  Future<void> fetchTeams() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      _teams = await _service.getTeams();
    } catch (e) {
      _error = 'Failed to load teams';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<Team> createTeam(CreateTeamRequest request) async {
    final team = await _service.createTeam(request);
    await fetchTeams();
    return team;
  }

  Future<Team> updateTeam(String id, CreateTeamRequest request) async {
    final team = await _service.updateTeam(id, request);
    await fetchTeams();
    return team;
  }

  Future<void> deleteTeam(String id) async {
    await _service.deleteTeam(id);
    await fetchTeams();
  }
}
