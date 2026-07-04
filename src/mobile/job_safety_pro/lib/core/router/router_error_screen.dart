import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../config/app_config.dart';
import '../../shared/widgets/common_widgets.dart';
import '../utils/dio_error_message.dart';
import 'app_routes.dart';

/// Replaces go_router's [MaterialErrorScreen], which overflows on long exceptions.
class RouterErrorScreen extends StatelessWidget {
  const RouterErrorScreen({super.key, required this.error});

  final Exception? error;

  @override
  Widget build(BuildContext context) {
    final message = _messageFor(error);

    return Scaffold(
      appBar: AppBar(title: const Text('Something went wrong')),
      body: ErrorView(
        message: message,
        onRetry: () => context.go(AppRoutes.dashboard),
      ),
    );
  }

  static String _messageFor(Exception? error) {
    if (error == null) return 'This page could not be opened.';
    if (error is DioException) return dioErrorMessage(error);

    final text = error.toString();
    if (text.contains('ProviderException') ||
        text.contains('modify other providers during their initialization')) {
      return 'The app hit a startup error. Fully stop the app and run it again '
          '(press R in the terminal for hot restart).';
    }

    if (text.contains('DioException') || text.contains('Connection refused')) {
      return 'Cannot reach the server (${AppConfig.apiBaseUrl}). '
          'Check your connection and sign in again.';
    }

    if (text.startsWith('GoException:')) {
      final body = text.replaceFirst('GoException:', '').trim();
      return body.length > 240 ? '${body.substring(0, 240)}…' : body;
    }

    return text.length > 240 ? '${text.substring(0, 240)}…' : text;
  }
}
