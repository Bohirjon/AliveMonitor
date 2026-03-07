import 'dart:io';

class AppConstants {
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://localhost:7150/api',
  );

  static String get signalRUrl =>
      '${apiBaseUrl.replaceFirst('/api', '')}/hubs/endpoint-status';

  static const String googleServerClientId =
      '51441565690-l2am3ot5sfl15hr81sc5aep20kopdmbt.apps.googleusercontent.com';

  static const String googleClientIdIos =
      '51441565690-85jlh00f4387iihb07c3du1oui85tpc9.apps.googleusercontent.com';

  static const String telegramBotName = 'AliveMonitorBot';

  static String? get googleClientId =>
      Platform.isIOS ? googleClientIdIos : null;
}
