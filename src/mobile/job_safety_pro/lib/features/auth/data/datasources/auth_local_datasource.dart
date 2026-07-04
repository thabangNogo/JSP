import '../../../../core/storage/hive_service.dart';
import '../../../../core/storage/secure_storage_service.dart';
import '../models/auth_models.dart';

class AuthLocalDataSource {
  AuthLocalDataSource(this._secureStorage, this._hiveService);

  final SecureStorageService _secureStorage;
  final HiveService _hiveService;

  Future<void> saveSession(AuthResponseModel response) async {
    await _secureStorage.saveTokens(
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiry: response.accessTokenExpiresAt,
    );
    await saveUser(response.user);
  }

  Future<void> saveUser(UserModel user) async {
    await _hiveService.userProfileBox.put('current', user.toJson());
  }

  Future<UserModel?> getCachedUser() async {
    final map = _hiveService.userProfileBox.get('current');
    if (map == null) return null;
    return UserModel.fromJson(Map<String, dynamic>.from(map));
  }

  Future<String?> getRefreshToken() => _secureStorage.getRefreshToken();

  Future<bool> hasSession() async {
    final token = await _secureStorage.getAccessToken();
    return token != null && token.isNotEmpty;
  }

  Future<void> clearSession() async {
    await _secureStorage.clearAll();
    await _hiveService.userProfileBox.delete('current');
  }
}
