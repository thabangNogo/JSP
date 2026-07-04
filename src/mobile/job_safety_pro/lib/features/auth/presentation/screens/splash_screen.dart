import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/router/app_routes.dart';
import '../providers/auth_provider.dart';

class SplashScreen extends ConsumerStatefulWidget {
  const SplashScreen({super.key});

  @override
  ConsumerState<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends ConsumerState<SplashScreen> {
  var _navigated = false;

  @override
  void initState() {
    super.initState();
    Future.microtask(_bootstrap);
  }

  Future<void> _bootstrap() async {
    try {
      await ref.read(authProvider.notifier).checkSession().timeout(
            AppConfig.sessionCheckTimeout,
          );
    } catch (_) {
      ref.read(authProvider.notifier).clearSession();
    } finally {
      _navigateFromAuthState();
    }
  }

  void _navigateFromAuthState() {
    if (_navigated || !mounted) return;
    _navigated = true;
    final auth = ref.read(authProvider);
    context.go(auth.isAuthenticated ? AppRoutes.dashboard : AppRoutes.login);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.health_and_safety, size: 72, color: Theme.of(context).colorScheme.primary),
            const SizedBox(height: 16),
            Text(AppConfig.appName, style: Theme.of(context).textTheme.headlineSmall),
            const SizedBox(height: 24),
            const CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}
