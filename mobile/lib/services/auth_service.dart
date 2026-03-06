import '../models/auth_tokens.dart';
import '../models/user.dart';
import 'api_client.dart';

class AuthService {
  final ApiClient _api;

  AuthService(this._api);

  Future<({AuthTokens tokens, User user})> googleSignIn(
      String idToken) async {
    final response = await _api.dio.post('/auth/google', data: {
      'idToken': idToken,
    });
    final data = response.data as Map<String, dynamic>;
    return (
      tokens: AuthTokens.fromJson(data),
      user: User.fromJson(data['user'] as Map<String, dynamic>),
    );
  }

  Future<void> revokeToken(String refreshToken) async {
    await _api.dio.post('/auth/revoke', data: {
      'refreshToken': refreshToken,
    });
  }
}
