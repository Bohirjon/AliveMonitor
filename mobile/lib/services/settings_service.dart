import '../models/user.dart';
import 'api_client.dart';

class SettingsService {
  final ApiClient _api;

  SettingsService(this._api);

  Future<User> getProfile() async {
    final response = await _api.dio.get('/settings/profile');
    return User.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> updateAlertEmail(String alertEmail) async {
    await _api.dio.put('/settings/alert-email', data: {
      'alertEmail': alertEmail,
    });
  }

  Future<void> updateWebhookUrl(String? webhookUrl) async {
    await _api.dio.put('/settings/webhook-url', data: {
      'webhookUrl': webhookUrl,
    });
  }
}
