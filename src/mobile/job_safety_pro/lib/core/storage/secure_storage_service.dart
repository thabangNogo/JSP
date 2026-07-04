import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../constants/api_constants.dart';

class SecureStorageService {
  SecureStorageService({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
    required DateTime expiry,
  }) async {
    await _storage.write(key: StorageKeys.accessToken, value: accessToken);
    await _storage.write(key: StorageKeys.refreshToken, value: refreshToken);
    await _storage.write(
      key: StorageKeys.tokenExpiry,
      value: expiry.toIso8601String(),
    );
  }

  Future<String?> getAccessToken() => _storage.read(key: StorageKeys.accessToken);

  Future<String?> getRefreshToken() => _storage.read(key: StorageKeys.refreshToken);

  Future<DateTime?> getTokenExpiry() async {
    final value = await _storage.read(key: StorageKeys.tokenExpiry);
    if (value == null) return null;
    return DateTime.tryParse(value);
  }

  Future<void> clearTokens() async {
    await _storage.delete(key: StorageKeys.accessToken);
    await _storage.delete(key: StorageKeys.refreshToken);
    await _storage.delete(key: StorageKeys.tokenExpiry);
  }

  Future<void> clearAll() => _storage.deleteAll();
}
