/// Callback invoked when token refresh fails. Uses a static handler to avoid
/// modifying Riverpod providers during [AuthNotifier.build].
typedef SessionExpiredHandler = Future<void> Function();

abstract final class SessionExpiry {
  static SessionExpiredHandler? _handler;

  static void register(SessionExpiredHandler handler) {
    _handler = handler;
  }

  static Future<void> invoke() async {
    await _handler?.call();
  }
}
