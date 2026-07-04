import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/storage/user_session_cleanup.dart';
import '../../../../core/network/dio_provider.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../core/utils/connectivity_service.dart';
import '../../data/datasources/auth_local_datasource.dart';
import '../../data/datasources/auth_remote_datasource.dart';
import '../../data/models/auth_models.dart';
import '../../data/repositories/auth_repository_impl.dart';
import '../../domain/repositories/auth_repository.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepositoryImpl(
    remote: AuthRemoteDataSource(ref.watch(dioProvider)),
    local: AuthLocalDataSource(
      ref.watch(secureStorageProvider),
      ref.watch(hiveServiceProvider),
    ),
    connectivity: ref.watch(connectivityServiceProvider),
  );
});

class AuthState {
  const AuthState({this.user, this.isLoading = false, this.error});

  final UserModel? user;
  final bool isLoading;
  final String? error;

  bool get isAuthenticated => user != null;

  AuthState copyWith({UserModel? user, bool? isLoading, String? error}) =>
      AuthState(
        user: user ?? this.user,
        isLoading: isLoading ?? this.isLoading,
        error: error,
      );
}

class AuthNotifier extends Notifier<AuthState> {
  @override
  AuthState build() => const AuthState();

  Future<void> checkSession() async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final repo = ref.read(authRepositoryProvider);
      if (!await repo.isLoggedIn()) {
        if (await repo.getCachedUser() != null) {
          await repo.logout();
        }
        state = const AuthState(isLoading: false);
        return;
      }

      final cached = await repo.getCachedUser();
      if (cached != null) {
        state = AuthState(user: cached, isLoading: false);
      }

      final user = await repo.getCurrentUser();
      if (user != null) {
        state = AuthState(user: user, isLoading: false);
      } else if (!await repo.isLoggedIn()) {
        await repo.logout();
        state = const AuthState(isLoading: false);
      } else if (cached == null) {
        await repo.logout();
        state = const AuthState(isLoading: false);
      }
    } catch (e) {
      state = AuthState(isLoading: false, error: dioErrorMessage(e));
    } finally {
      if (state.isLoading) {
        state = AuthState(user: state.user, isLoading: false, error: state.error);
      }
    }
  }

  void clearSession() {
    state = const AuthState(isLoading: false);
  }

  Future<bool> login(String email, String password) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final previousUserId = state.user?.id;
      final user = await ref.read(authRepositoryProvider).login(email, password);
      if (previousUserId != user.id) {
        await ref.read(userSessionCleanupProvider).clear();
      }
      state = AuthState(user: user, isLoading: false);
      return true;
    } catch (e) {
      state = AuthState(isLoading: false, error: dioErrorMessage(e));
      return false;
    }
  }

  Future<void> handleSessionExpired() async {
    await ref.read(userSessionCleanupProvider).clear();
    await ref.read(authRepositoryProvider).logout();
    if (ref.mounted) {
      state = const AuthState(isLoading: false);
    }
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).logout();
    await ref.read(userSessionCleanupProvider).clear();
    state = const AuthState();
  }
}

final authProvider = NotifierProvider<AuthNotifier, AuthState>(AuthNotifier.new);
