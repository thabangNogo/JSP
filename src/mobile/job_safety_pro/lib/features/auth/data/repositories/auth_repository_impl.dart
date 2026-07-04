import 'package:dio/dio.dart';
import '../../../../core/utils/connectivity_service.dart';
import '../datasources/auth_local_datasource.dart';
import '../datasources/auth_remote_datasource.dart';
import '../models/auth_models.dart';
import '../../domain/repositories/auth_repository.dart';

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({
    required AuthRemoteDataSource remote,
    required AuthLocalDataSource local,
    required ConnectivityService connectivity,
  })  : _remote = remote,
        _local = local,
        _connectivity = connectivity;

  final AuthRemoteDataSource _remote;
  final AuthLocalDataSource _local;
  final ConnectivityService _connectivity;

  @override
  Future<UserModel> login(String email, String password) async {
    final response = await _remote.login(
      LoginRequestModel(email: email, password: password),
    );
    await _local.saveSession(response);
    return response.user;
  }

  @override
  Future<UserModel?> getCachedUser() => _local.getCachedUser();

  @override
  Future<UserModel?> getCurrentUser() async {
    final cached = await _local.getCachedUser();
    if (!await _connectivity.isOnline) return cached;
    if (!await isLoggedIn()) return null;
    try {
      final user = await _remote.getCurrentUser();
      await _local.saveUser(user);
      return user;
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) return null;
      return cached;
    } catch (_) {
      return cached;
    }
  }

  @override
  Future<bool> isLoggedIn() => _local.hasSession();

  @override
  Future<void> logout() async {
    final refresh = await _local.getRefreshToken();
    if (refresh != null && await _connectivity.isOnline) {
      try {
        await _remote.logout(refresh);
      } catch (_) {}
    }
    await _local.clearSession();
  }
}
