import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../providers/auth_provider.dart';
import '../screens/signin/signin_screen.dart';
import '../screens/dashboard/dashboard_screen.dart';
import '../screens/endpoint_detail/endpoint_detail_screen.dart';
import '../screens/settings/settings_screen.dart';
import '../widgets/app_shell.dart';

final _rootNavigatorKey = GlobalKey<NavigatorState>();
final _shellNavigatorKey = GlobalKey<NavigatorState>();

GoRouter createRouter(AuthProvider authProvider) {
  return GoRouter(
    navigatorKey: _rootNavigatorKey,
    initialLocation: '/dashboard',
    refreshListenable: authProvider,
    redirect: (context, state) {
      final isAuth = authProvider.isAuthenticated;
      final isLoading = authProvider.isLoading;
      final isSignIn = state.matchedLocation == '/signin';

      if (isLoading) return null;
      if (!isAuth && !isSignIn) return '/signin';
      if (isAuth && isSignIn) return '/dashboard';
      return null;
    },
    routes: [
      GoRoute(
        path: '/signin',
        builder: (context, state) => const SignInScreen(),
      ),
      ShellRoute(
        navigatorKey: _shellNavigatorKey,
        builder: (context, state, child) => AppShell(child: child),
        routes: [
          GoRoute(
            path: '/dashboard',
            builder: (context, state) => const DashboardScreen(),
          ),
          GoRoute(
            path: '/endpoints/:id',
            builder: (context, state) => EndpointDetailScreen(
              endpointId: state.pathParameters['id']!,
            ),
          ),
          GoRoute(
            path: '/settings',
            builder: (context, state) => const SettingsScreen(),
          ),
        ],
      ),
    ],
  );
}
