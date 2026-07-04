import '../../data/models/auth_models.dart';

abstract class AuthRepository {
  Future<UserModel> login(String email, String password);
  Future<UserModel?> getCurrentUser();
  Future<UserModel?> getCachedUser();
  Future<bool> isLoggedIn();
  Future<void> logout();
}
