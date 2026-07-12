import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/auth/session_expired_provider.dart';
import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/presentation/providers/auth_provider.dart';
import 'features/notifications/services/fcm_push_service.dart';
import 'features/safety/presentation/providers/safety_providers.dart';

class JobSafetyProApp extends ConsumerStatefulWidget {
  const JobSafetyProApp({super.key});

  @override
  ConsumerState<JobSafetyProApp> createState() => _JobSafetyProAppState();
}

class _JobSafetyProAppState extends ConsumerState<JobSafetyProApp> {
  FcmPushService? _fcmService;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      SessionExpiry.register(
        () => ref.read(authProvider.notifier).handleSessionExpired(),
      );
    });
  }

  @override
  void dispose() {
    _fcmService?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);

    ref.listen<bool>(
      authProvider.select((s) => s.isAuthenticated),
      (previous, next) {
        if (next) {
          _fcmService?.dispose();
          _fcmService = FcmPushService(
            remote: ref.read(safetyRemoteDataSourceProvider),
          );
          _fcmService!.init();
        } else {
          _fcmService?.dispose();
          _fcmService = null;
        }
      },
    );

    return MaterialApp.router(
      title: 'Job Safety Pro',
      theme: AppTheme.light,
      routerConfig: router,
      debugShowCheckedModeBanner: false,
    );
  }
}
