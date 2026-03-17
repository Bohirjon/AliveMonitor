import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:google_sign_in/google_sign_in.dart';
import '../config/constants.dart';
import '../models/auth_tokens.dart';
import '../models/user.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';
import '../services/settings_service.dart';

class AuthProvider extends ChangeNotifier {
  final ApiClient _apiClient;
  late final AuthService _authService;
  late final SettingsService _settingsService;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  User? _user;
  AuthTokens? _tokens;
  bool _isLoading = true;
  bool _googleInitialized = false;

  User? get user => _user;
  AuthTokens? get tokens => _tokens;
  bool get isAuthenticated => _tokens != null;
  bool get isLoading => _isLoading;

  AuthProvider(this._apiClient) {
    _authService = AuthService(_apiClient);
    _settingsService = SettingsService(_apiClient);

    _apiClient.onTokensRefreshed = (newTokens) {
      _tokens = newTokens;
      notifyListeners();
    };

    _apiClient.onAuthFailed = () {
      _user = null;
      _tokens = null;
      notifyListeners();
    };

    _loadStoredAuth();
  }

  Future<void> _ensureGoogleInitialized() async {
    if (_googleInitialized) return;
    await GoogleSignIn.instance.initialize(
      clientId: AppConstants.googleClientId,
      serverClientId: AppConstants.googleServerClientId,
    );
    _googleInitialized = true;
  }

  Future<void> _loadStoredAuth() async {
    try {
      final tokensJson = await _storage.read(key: 'tokens');
      final userJson = await _storage.read(key: 'user');

      if (tokensJson != null) {
        _tokens = AuthTokens.fromJson(
          json.decode(tokensJson) as Map<String, dynamic>,
        );

        if (userJson != null) {
          _user = User.fromJson(
            json.decode(userJson) as Map<String, dynamic>,
          );
        } else {
          // Fetch profile if we have tokens but no user
          try {
            _user = await _settingsService.getProfile();
            await _storage.write(
              key: 'user',
              value: json.encode(_user!.toJson()),
            );
          } catch (_) {
            // Token invalid
            _tokens = null;
            await _storage.delete(key: 'tokens');
          }
        }
      }
    } catch (_) {
      // Storage read failed
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> signInWithGoogle() async {
    await _ensureGoogleInitialized();

    final GoogleSignInAccount account;
    try {
      account = await GoogleSignIn.instance.authenticate();
    } on PlatformException catch (e) {
      if (e.code == 'sign_in_canceled') return; // user cancelled
      rethrow;
    }

    final idToken = account.authentication.idToken;
    if (idToken == null) throw Exception('Failed to get Google ID token');

    final ({AuthTokens tokens, User user}) result;
    try {
      result = await _authService.googleSignIn(idToken);
    } catch (e, stack) {
      debugPrint('=== Google Sign-In API Error ===');
      debugPrint('Error: $e');
      debugPrint('Stack: $stack');
      rethrow;
    }

    _tokens = result.tokens;
    _user = result.user;

    await _storage.write(
      key: 'tokens',
      value: json.encode(_tokens!.toJson()),
    );
    await _storage.write(
      key: 'user',
      value: json.encode(_user!.toJson()),
    );

    notifyListeners();
  }

  Future<void> signOut() async {
    if (_tokens != null) {
      try {
        await _authService.revokeToken(_tokens!.refreshToken);
      } catch (_) {
        // Best effort
      }
    }

    try {
      if (_googleInitialized) {
        await GoogleSignIn.instance.signOut();
      }
    } catch (_) {}

    _tokens = null;
    _user = null;
    await _storage.delete(key: 'tokens');
    await _storage.delete(key: 'user');
    notifyListeners();
  }

  void updateUser(User updatedUser) {
    _user = updatedUser;
    _storage.write(key: 'user', value: json.encode(_user!.toJson()));
    notifyListeners();
  }
}
