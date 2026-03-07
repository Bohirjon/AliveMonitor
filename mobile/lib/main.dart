import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'config/theme.dart';
import 'config/router.dart';
import 'services/api_client.dart';
import 'services/endpoint_service.dart';
import 'services/team_service.dart';
import 'services/signalr_service.dart';
import 'providers/auth_provider.dart';
import 'providers/theme_provider.dart';
import 'providers/endpoint_provider.dart';
import 'providers/team_provider.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const AliveMonitorApp());
}

class AliveMonitorApp extends StatefulWidget {
  const AliveMonitorApp({super.key});

  @override
  State<AliveMonitorApp> createState() => _AliveMonitorAppState();
}

class _AliveMonitorAppState extends State<AliveMonitorApp> {
  late final ApiClient _apiClient;
  late final AuthProvider _authProvider;
  late final ThemeProvider _themeProvider;
  late final EndpointProvider _endpointProvider;
  late final TeamProvider _teamProvider;
  late final SignalRService _signalRService;
  late final GoRouter _router;

  @override
  void initState() {
    super.initState();
    _apiClient = ApiClient();
    _authProvider = AuthProvider(_apiClient);
    _themeProvider = ThemeProvider();
    _endpointProvider = EndpointProvider(EndpointService(_apiClient));
    _teamProvider = TeamProvider(TeamService(_apiClient));
    _signalRService = SignalRService(_apiClient);
    _router = createRouter(_authProvider);

    // Connect SignalR when auth changes
    _authProvider.addListener(_handleAuthChange);
  }

  void _handleAuthChange() {
    if (_authProvider.isAuthenticated) {
      _signalRService.onEndpointStatusChanged = () {
        _endpointProvider.fetchEndpoints();
      };
      _signalRService.start();
    } else {
      _signalRService.stop();
    }
  }

  @override
  void dispose() {
    _authProvider.removeListener(_handleAuthChange);
    _signalRService.stop();
    _authProvider.dispose();
    _themeProvider.dispose();
    _endpointProvider.dispose();
    _teamProvider.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        Provider<ApiClient>.value(value: _apiClient),
        ChangeNotifierProvider<AuthProvider>.value(value: _authProvider),
        ChangeNotifierProvider<ThemeProvider>.value(value: _themeProvider),
        ChangeNotifierProvider<EndpointProvider>.value(
            value: _endpointProvider),
        ChangeNotifierProvider<TeamProvider>.value(value: _teamProvider),
      ],
      child: Consumer<ThemeProvider>(
        builder: (context, themeProvider, _) {
          return MaterialApp.router(
            title: 'AliveMonitor',
            debugShowCheckedModeBanner: false,
            theme: AppTheme.lightTheme,
            darkTheme: AppTheme.darkTheme,
            themeMode: themeProvider.themeMode,
            routerConfig: _router,
          );
        },
      ),
    );
  }
}
