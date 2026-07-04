import 'package:dio/dio.dart';
import '../../constants/api_constants.dart';
import '../../storage/secure_storage_service.dart';

class AuthInterceptor extends Interceptor {
  AuthInterceptor({
    required Dio dio,
    required SecureStorageService secureStorage,
    Future<void> Function()? onSessionExpired,
  })  : _dio = dio,
        _secureStorage = secureStorage,
        _onSessionExpired = onSessionExpired;

  final Dio _dio;
  final SecureStorageService _secureStorage;
  final Future<void> Function()? _onSessionExpired;
  bool _isRefreshing = false;

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _secureStorage.getAccessToken();
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    if (err.response?.statusCode != 401 ||
        err.requestOptions.path.contains(ApiConstants.authLogin) ||
        err.requestOptions.path.contains(ApiConstants.authRefresh)) {
      return handler.next(err);
    }

    if (_isRefreshing) return handler.next(err);

    try {
      _isRefreshing = true;
      final refreshToken = await _secureStorage.getRefreshToken();
      if (refreshToken == null) {
        final callback = _onSessionExpired;
        if (callback != null) Future.microtask(callback);
        return handler.next(err);
      }

      final response = await _dio.post(
        ApiConstants.authRefresh,
        data: {'refreshToken': refreshToken},
        options: Options(headers: {'Authorization': null}),
      );

      final data = response.data['data'] as Map<String, dynamic>;
      await _secureStorage.saveTokens(
        accessToken: data['accessToken'] as String,
        refreshToken: data['refreshToken'] as String,
        expiry: DateTime.parse(data['accessTokenExpiresAt'] as String),
      );

      final request = err.requestOptions;
      request.headers['Authorization'] = 'Bearer ${data['accessToken']}';
      final retry = await _dio.fetch(request);
      return handler.resolve(retry);
    } catch (_) {
      await _secureStorage.clearTokens();
      final callback = _onSessionExpired;
      if (callback != null) {
        Future.microtask(callback);
      }
      return handler.next(err);
    } finally {
      _isRefreshing = false;
    }
  }
}
