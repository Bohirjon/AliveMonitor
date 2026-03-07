import 'dart:io';

class AppConstants {
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5000/api',
  );

  static const String signalRUrl = String.fromEnvironment(
    'SIGNALR_URL',
    defaultValue: 'http://10.0.2.2:5000/hubs/endpoint-status',
  );

  // Web/Server client ID - used as serverClientId so ID token audience
  // matches the backend's GoogleAuth:ClientId
  static const String googleServerClientId =
      '51441565690-l2am3ot5sfl15hr81sc5aep20kopdmbt.apps.googleusercontent.com';

  // iOS client ID - create in Google Cloud Console > Credentials > OAuth 2.0 Client IDs
  // Type: iOS, Bundle ID: com.alivemonitor.aliveMonitor
  static const String googleClientIdIos =
      '51441565690-85jlh00f4387iihb07c3du1oui85tpc9.apps.googleusercontent.com';

  static const String telegramBotName = 'AliveMonitorBot';

  /// Returns the platform-appropriate Google client ID for GoogleSignIn.initialize()
  static String? get googleClientId =>
      Platform.isIOS ? googleClientIdIos : null;
}
