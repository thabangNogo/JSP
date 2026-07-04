import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../features/auth/presentation/providers/auth_provider.dart';
import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/auth/presentation/screens/splash_screen.dart';
import '../../features/dashboard/presentation/screens/dashboard_screen.dart';
import '../../features/jsa/presentation/screens/assessment_detail_screen.dart';
import '../../features/jsa/presentation/screens/assessment_history_screen.dart';
import '../../features/jsa/presentation/screens/jsa_list_screen.dart';
import '../../features/jsa/presentation/screens/new_assessment_screen.dart';
import '../../features/jsa/presentation/screens/workflow/hazards_controls_screens.dart';
import '../../features/jsa/presentation/screens/workflow/identify_hazards_screen.dart';
import '../../features/jsa/presentation/screens/workflow/job_information_screen.dart';
import '../../features/jsa/presentation/screens/workflow/quick_assessment_screen.dart';
import '../../features/jsa/presentation/screens/workflow/sign_off_screen.dart';
import '../../features/jsa/presentation/screens/workflow/workflow_shell.dart';
import '../../features/profile/presentation/screens/edit_profile_screen.dart';
import '../../features/profile/presentation/screens/profile_screen.dart';
import '../../features/scanner/presentation/screens/qr_scanner_screen.dart';
import '../../features/settings/presentation/screens/settings_screen.dart';
import '../../features/safety/presentation/screens/capture_injury_screen.dart';
import '../../features/safety/presentation/screens/injury_detail_screen.dart';
import '../../features/safety/presentation/screens/injury_list_screen.dart';
import '../../features/safety/presentation/screens/notifications_screen.dart';
import '../../features/safety/presentation/screens/report_near_miss_screen.dart';
import '../../features/safety/presentation/screens/stop_unsafe_work_screen.dart';
import '../../features/sync/presentation/screens/sync_status_screen.dart';
import 'app_routes.dart';
import 'router_error_screen.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final refreshListenable = _AuthRefreshListenable(ref);

  final router = GoRouter(
    initialLocation: AppRoutes.splash,
    refreshListenable: refreshListenable,
    errorBuilder: (context, state) => RouterErrorScreen(error: state.error),
    redirect: (context, state) {
      final authState = ref.read(authProvider);
      final isAuthenticated = authState.isAuthenticated;
      final isLoading = authState.isLoading;
      final loggingIn = state.matchedLocation == AppRoutes.login;
      final onSplash = state.matchedLocation == AppRoutes.splash;

      if (isLoading || onSplash) return null;
      if (!isAuthenticated && !loggingIn) return AppRoutes.login;
      if (isAuthenticated && loggingIn) return AppRoutes.dashboard;
      return null;
    },
    routes: [
      GoRoute(path: AppRoutes.splash, builder: (_, __) => const SplashScreen()),
      GoRoute(path: AppRoutes.login, builder: (_, __) => const LoginScreen()),
      GoRoute(path: AppRoutes.dashboard, builder: (_, __) => const DashboardScreen()),
      GoRoute(path: AppRoutes.jsaList, builder: (_, __) => const JsaListScreen()),
      GoRoute(
        path: AppRoutes.jsaDetail,
        builder: (_, state) => AssessmentDetailScreen(
          localId: state.pathParameters['localId']!,
        ),
      ),
      GoRoute(path: AppRoutes.jsaHistory, builder: (_, __) => const AssessmentHistoryScreen()),
      GoRoute(path: AppRoutes.jsaNew, builder: (_, __) => const NewAssessmentScreen()),
      GoRoute(
        path: '/jsas/workflow/:draftId',
        redirect: (_, state) {
          final draftId = state.pathParameters['draftId']!;
          final path = state.uri.path;
          final base = '/jsas/workflow/$draftId';
          // Only redirect the bare workflow URL; child step routes must not bounce back.
          if (path == base || path == '$base/') {
            return '$base/job-information';
          }
          return null;
        },
        routes: [
          ShellRoute(
            builder: (context, state, child) => WorkflowShell(
              draftId: state.pathParameters['draftId']!,
              child: child,
            ),
            routes: [
              GoRoute(
                path: 'quick-assessment',
                builder: (_, state) => QuickAssessmentScreen(
                  draftId: state.pathParameters['draftId']!,
                ),
              ),
              GoRoute(
                path: 'job-information',
                builder: (_, state) => JobInformationScreen(
                  draftId: state.pathParameters['draftId']!,
                ),
              ),
              GoRoute(
                path: 'hazards',
                builder: (_, state) =>
                    IdentifyHazardsScreen(draftId: state.pathParameters['draftId']!),
              ),
              GoRoute(
                path: 'risks',
                builder: (_, state) =>
                    AssessRisksScreen(draftId: state.pathParameters['draftId']!),
              ),
              GoRoute(
                path: 'sign-off',
                builder: (_, state) =>
                    SignOffScreen(draftId: state.pathParameters['draftId']!),
              ),
            ],
          ),
        ],
      ),
      GoRoute(path: AppRoutes.qrScanner, builder: (_, __) => const QrScannerScreen()),
      GoRoute(path: AppRoutes.profile, builder: (_, __) => const ProfileScreen()),
      GoRoute(path: AppRoutes.profileEdit, builder: (_, __) => const EditProfileScreen()),
      GoRoute(path: AppRoutes.settings, builder: (_, __) => const SettingsScreen()),
      GoRoute(path: AppRoutes.syncStatus, builder: (_, __) => const SyncStatusScreen()),
      GoRoute(path: AppRoutes.stopUnsafeWork, builder: (_, __) => const StopUnsafeWorkScreen()),
      GoRoute(path: AppRoutes.reportNearMiss, builder: (_, __) => const ReportNearMissScreen()),
      GoRoute(path: AppRoutes.notifications, builder: (_, __) => const NotificationsScreen()),
      GoRoute(path: AppRoutes.injuries, builder: (_, __) => const InjuryListScreen()),
      GoRoute(path: AppRoutes.captureInjury, builder: (_, __) => const CaptureInjuryScreen()),
      GoRoute(
        path: AppRoutes.injuryDetail,
        builder: (_, state) => InjuryDetailScreen(injuryId: state.pathParameters['id']!),
      ),
    ],
  );

  ref.onDispose(router.dispose);
  return router;
});

class _AuthRefreshListenable extends ChangeNotifier {
  _AuthRefreshListenable(this.ref) {
    ref.listen(
      authProvider.select((s) => '${s.isAuthenticated}-${s.isLoading}'),
      (_, __) => notifyListeners(),
    );
  }

  final Ref ref;
}
