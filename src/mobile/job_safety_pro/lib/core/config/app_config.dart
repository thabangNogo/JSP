class AppConfig {
  static const String appName = 'Job Safety Pro';

  /// Override at run time for local dev, e.g.:
  /// flutter run --dart-define=API_BASE_URL=http://127.0.0.1:5101/api/v1
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://safetyapi.homesteadflow.com/api/v1',
  );

  static const Duration connectTimeout = Duration(seconds: 10);
  static const Duration receiveTimeout = Duration(seconds: 10);
  static const Duration sessionCheckTimeout = Duration(seconds: 8);
}
