import 'package:dio/dio.dart';

/// Extracts a short user-facing message from a [DioException].
String dioErrorMessage(Object error) {
  if (error is! DioException) return error.toString();

  if (error.type == DioExceptionType.connectionError ||
      error.type == DioExceptionType.connectionTimeout ||
      error.type == DioExceptionType.sendTimeout ||
      error.type == DioExceptionType.receiveTimeout) {
    return 'Cannot reach the server. Start the Job Safety Pro API '
        '(http://127.0.0.1:5101) and try again.';
  }

  final data = error.response?.data;
  if (data is Map<String, dynamic>) {
    final message = data['message'] as String?;
    if (message != null && message.isNotEmpty) return message;

    final errors = data['errors'];
    if (errors is Map<String, dynamic>) {
      final parts = <String>[];
      for (final entry in errors.entries) {
        final value = entry.value;
        if (value is List) {
          parts.addAll(value.map((e) => e.toString()));
        } else if (value != null) {
          parts.add(value.toString());
        }
      }
      if (parts.isNotEmpty) return parts.join('\n');
    }
  }

  if (error.response?.statusCode == 401) {
    return 'Your session has expired. Please sign in again.';
  }

  if (error.response?.statusCode == 400) {
    return 'The server could not accept this request. Check required fields and try again.';
  }

  final message = error.message;
  if (message != null && message.length <= 200) return message;

  return 'Something went wrong. Please try again.';
}
