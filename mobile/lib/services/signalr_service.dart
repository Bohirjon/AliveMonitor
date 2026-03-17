import 'package:signalr_netcore/signalr_client.dart';
import '../config/constants.dart';
import 'api_client.dart';

class SignalRService {
  HubConnection? _connection;
  final ApiClient _apiClient;
  void Function()? onEndpointStatusChanged;

  SignalRService(this._apiClient);

  Future<void> start() async {
    final accessToken = await _apiClient.getAccessToken();
    if (accessToken == null) return;

    _connection = HubConnectionBuilder()
        .withUrl(
          AppConstants.signalRUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async {
              return await _apiClient.getAccessToken() ?? '';
            },
          ),
        )
        .withAutomaticReconnect()
        .build();

    _connection!.on('EndpointStatusChanged', (_) {
      onEndpointStatusChanged?.call();
    });

    try {
      await _connection!.start();
    } catch (e) {
      // SignalR connection failed - will retry on reconnect
    }
  }

  Future<void> stop() async {
    await _connection?.stop();
    _connection = null;
  }
}
