import '../models/team.dart';
import 'api_client.dart';

class TelegramService {
  final ApiClient _api;

  TelegramService(this._api);

  Future<LinkCodeResponse> generateLinkCode({String? teamId}) async {
    final response = await _api.dio.post('/telegram/link-code', data: {
      // ignore: use_null_aware_elements
      if (teamId != null) 'teamId': teamId,
    });
    return LinkCodeResponse.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<TelegramStatusResponse> getStatus() async {
    final response = await _api.dio.get('/telegram/status');
    return TelegramStatusResponse.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<TelegramStatusResponse> getTeamStatus(String teamId) async {
    final response =
        await _api.dio.get('/telegram/status/team/$teamId');
    return TelegramStatusResponse.fromJson(
        response.data as Map<String, dynamic>);
  }

  Future<void> unlink() async {
    await _api.dio.delete('/telegram/unlink');
  }

  Future<void> unlinkTeam(String teamId) async {
    await _api.dio.delete('/telegram/unlink/team/$teamId');
  }
}
