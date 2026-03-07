import 'dart:io';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'dart:convert';
import '../models/auth_tokens.dart';
import '../config/constants.dart';

class ApiClient {
  late final Dio dio;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  // Callbacks set by AuthProvider
  void Function(AuthTokens)? onTokensRefreshed;
  void Function()? onAuthFailed;

  ApiClient() {
    dio = Dio(BaseOptions(
      baseUrl: AppConstants.apiBaseUrl,
      headers: {'Content-Type': 'application/json'},
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 15),
    ));

    // Trust self-signed certificates in debug mode (local dev server)
    if (kDebugMode) {
      (dio.httpClientAdapter as IOHttpClientAdapter).createHttpClient = () {
        final client = HttpClient();
        client.badCertificateCallback = (cert, host, port) => true;
        return client;
      };
    }

    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final tokensJson = await _storage.read(key: 'tokens');
        if (tokensJson != null) {
          final tokens = AuthTokens.fromJson(
            json.decode(tokensJson) as Map<String, dynamic>,
          );
          options.headers['Authorization'] = 'Bearer ${tokens.accessToken}';
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        if (error.response?.statusCode == 401 &&
            !(error.requestOptions.extra['_retry'] == true)) {
          error.requestOptions.extra['_retry'] = true;

          try {
            final tokensJson = await _storage.read(key: 'tokens');
            if (tokensJson == null) throw Exception('No tokens');

            final tokens = AuthTokens.fromJson(
              json.decode(tokensJson) as Map<String, dynamic>,
            );

            final refreshDio = Dio(BaseOptions(
              baseUrl: AppConstants.apiBaseUrl,
              headers: {'Content-Type': 'application/json'},
            ));

            final response = await refreshDio.post(
              '/auth/refresh',
              data: {'refreshToken': tokens.refreshToken},
            );

            final newTokens = AuthTokens.fromJson(
              response.data as Map<String, dynamic>,
            );

            await _storage.write(
              key: 'tokens',
              value: json.encode(newTokens.toJson()),
            );

            onTokensRefreshed?.call(newTokens);

            // Retry original request
            error.requestOptions.headers['Authorization'] =
                'Bearer ${newTokens.accessToken}';
            final retryResponse = await dio.fetch(error.requestOptions);
            return handler.resolve(retryResponse);
          } catch (_) {
            await _storage.delete(key: 'tokens');
            await _storage.delete(key: 'user');
            onAuthFailed?.call();
            return handler.reject(error);
          }
        }
        handler.next(error);
      },
    ));
  }

  Future<String?> getAccessToken() async {
    final tokensJson = await _storage.read(key: 'tokens');
    if (tokensJson == null) return null;
    final tokens = AuthTokens.fromJson(
      json.decode(tokensJson) as Map<String, dynamic>,
    );
    return tokens.accessToken;
  }
}
