import 'dart:io';

import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../auth/session_expired_provider.dart';
import '../config/app_config.dart';
import '../storage/hive_service.dart';
import '../storage/secure_storage_service.dart';
import 'interceptors/auth_interceptor.dart';

bool _isLocalDevHost(String host) =>
    host == 'localhost' || host == '127.0.0.1' || host == '10.0.2.2';

void _configureDevHttpClient(Dio dio) {
  if (kReleaseMode) return;

  final adapter = dio.httpClientAdapter;
  if (adapter is! IOHttpClientAdapter) return;

  adapter.createHttpClient = () {
    final client = HttpClient();
    client.badCertificateCallback = (cert, host, port) => _isLocalDevHost(host);
    return client;
  };
}

final secureStorageProvider = Provider<SecureStorageService>(
  (ref) => SecureStorageService(),
);

final hiveServiceProvider = Provider<HiveService>((ref) => HiveService());

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: AppConfig.connectTimeout,
      receiveTimeout: AppConfig.receiveTimeout,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ),
  );

  _configureDevHttpClient(dio);

  dio.interceptors.add(
    AuthInterceptor(
      dio: dio,
      secureStorage: ref.watch(secureStorageProvider),
      onSessionExpired: SessionExpiry.invoke,
    ),
  );
  dio.interceptors.add(LogInterceptor(requestBody: true, responseBody: true));

  return dio;
});
