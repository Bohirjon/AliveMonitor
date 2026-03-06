import '../models/team.dart';
import 'api_client.dart';

class TeamService {
  final ApiClient _api;

  TeamService(this._api);

  Future<List<Team>> getTeams() async {
    final response = await _api.dio.get('/teams');
    return (response.data as List)
        .map((e) => Team.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Team> getTeam(String id) async {
    final response = await _api.dio.get('/teams/$id');
    return Team.fromJson(response.data as Map<String, dynamic>);
  }

  Future<Team> createTeam(CreateTeamRequest request) async {
    final response = await _api.dio.post('/teams', data: request.toJson());
    return Team.fromJson(response.data as Map<String, dynamic>);
  }

  Future<Team> updateTeam(String id, CreateTeamRequest request) async {
    final response =
        await _api.dio.put('/teams/$id', data: request.toJson());
    return Team.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> deleteTeam(String id) async {
    await _api.dio.delete('/teams/$id');
  }
}
