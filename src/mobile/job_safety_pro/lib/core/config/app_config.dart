class AppConfig {
  static const String appName = 'Job Safety Pro';

  /// Override at run time, e.g.:
  /// flutter run --dart-define=API_BASE_URL=http://127.0.0.1:5101/api/v1
  /// flutter run --dart-define=API_BASE_URL=https://localhost:7130/api/v1
  ///
  /// Prefer HTTP on port 5101 for local dev. HTTPS on 7130 uses a dev cert;
  /// debug builds accept self-signed certs for localhost only.
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://127.0.0.1:5101/api/v1',
  );

  static const Duration connectTimeout = Duration(seconds: 10);
  static const Duration receiveTimeout = Duration(seconds: 10);
  static const Duration sessionCheckTimeout = Duration(seconds: 8);
}
