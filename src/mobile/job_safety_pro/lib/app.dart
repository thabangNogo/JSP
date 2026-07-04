import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/auth/session_expired_provider.dart';
import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/presentation/providers/auth_provider.dart';

class JobSafetyProApp extends ConsumerStatefulWidget {
  const JobSafetyProApp({super.key});

  @override
  ConsumerState<JobSafetyProApp> createState() => _JobSafetyProAppState();
}

class _JobSafetyProAppState extends ConsumerState<JobSafetyProApp> {
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
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      title: 'Job Safety Pro',
      theme: AppTheme.light,
      routerConfig: router,
      debugShowCheckedModeBanner: false,
    );
  }
}
