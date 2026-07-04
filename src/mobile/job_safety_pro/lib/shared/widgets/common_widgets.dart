import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../core/router/app_routes.dart';
import '../../core/theme/app_colors.dart';

/// App bar with back (when possible) and always-available home navigation.
class AppNavigationBar extends StatelessWidget implements PreferredSizeWidget {
  const AppNavigationBar({
    super.key,
    required this.title,
    this.actions = const [],
  });

  final String title;
  final List<Widget> actions;

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);

  void _onLeadingPressed(BuildContext context) {
    if (context.canPop()) {
      context.pop();
    } else {
      context.go(AppRoutes.dashboard);
    }
  }

  @override
  Widget build(BuildContext context) {
    final canPop = context.canPop();

    return AppBar(
      title: Text(title),
      automaticallyImplyLeading: false,
      leading: IconButton(
        icon: Icon(canPop ? Icons.arrow_back : Icons.home_outlined),
        tooltip: canPop ? 'Back' : 'Home',
        onPressed: () => _onLeadingPressed(context),
      ),
      actions: [
        ...actions,
        IconButton(
          icon: const Icon(Icons.dashboard_outlined),
          tooltip: 'Dashboard',
          onPressed: () => context.go(AppRoutes.dashboard),
        ),
      ],
    );
  }
}

class OfflineBanner extends StatelessWidget {
  const OfflineBanner({super.key, required this.isOnline});

  final bool isOnline;

  @override
  Widget build(BuildContext context) {
    if (isOnline) return const SizedBox.shrink();
    return MaterialBanner(
      backgroundColor: AppColors.offline,
      content: const Text(
        'You are offline. Changes will sync when connected.',
        style: TextStyle(color: Colors.white),
      ),
      actions: const [SizedBox.shrink()],
    );
  }
}

class LoadingOverlay extends StatelessWidget {
  const LoadingOverlay({super.key, this.message = 'Loading...'});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.black26,
      alignment: Alignment.center,
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const CircularProgressIndicator(),
              const SizedBox(height: 16),
              Text(message),
            ],
          ),
        ),
      ),
    );
  }
}

class ErrorView extends StatelessWidget {
  const ErrorView({
    super.key,
    required this.message,
    this.onRetry,
    this.shrinkWrap = false,
  });

  final String message;
  final VoidCallback? onRetry;

  /// When true, use inside [ListView] / [Column] without flex expansion.
  final bool shrinkWrap;

  @override
  Widget build(BuildContext context) {
    final content = Column(
      mainAxisSize: MainAxisSize.min,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const Icon(Icons.error_outline, size: 48, color: AppColors.danger),
        const SizedBox(height: 16),
        Text(message, textAlign: TextAlign.center),
        if (onRetry != null) ...[
          const SizedBox(height: 16),
          ElevatedButton(onPressed: onRetry, child: const Text('Retry')),
        ],
      ],
    );

    if (shrinkWrap) {
      return Padding(padding: const EdgeInsets.symmetric(vertical: 32, horizontal: 24), child: content);
    }

    return Center(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: content,
      ),
    );
  }
}
