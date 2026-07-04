import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/dio_provider.dart';
import '../../../../core/storage/hive_service.dart';
import '../../../../core/utils/connectivity_service.dart';
import '../../data/datasources/employee_profile_local_datasource.dart';
import '../../data/datasources/employee_profile_remote_datasource.dart';
import '../../data/models/employee_profile_model.dart';
import '../../data/repositories/employee_profile_repository_impl.dart';
import '../../domain/repositories/employee_profile_repository.dart';

final employeeProfileRepositoryProvider = Provider<EmployeeProfileRepository>((ref) {
  return EmployeeProfileRepositoryImpl(
    local: EmployeeProfileLocalDataSource(ref.watch(hiveServiceProvider)),
    remote: EmployeeProfileRemoteDataSource(ref.watch(dioProvider)),
    connectivity: ref.watch(connectivityServiceProvider),
  );
});

class EmployeeProfileState {
  const EmployeeProfileState({
    this.profile,
    this.isLoading = false,
    this.error,
  });

  final EmployeeProfileModel? profile;
  final bool isLoading;
  final String? error;

  bool get isComplete => profile?.isComplete ?? false;

  EmployeeProfileState copyWith({
    EmployeeProfileModel? profile,
    bool? isLoading,
    String? error,
  }) {
    return EmployeeProfileState(
      profile: profile ?? this.profile,
      isLoading: isLoading ?? this.isLoading,
      error: error,
    );
  }
}

class EmployeeProfileNotifier extends Notifier<EmployeeProfileState> {
  @override
  EmployeeProfileState build() {
    Future.microtask(loadProfile);
    return const EmployeeProfileState(isLoading: true);
  }

  Future<void> loadProfile() async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final profile = await ref.read(employeeProfileRepositoryProvider).getMyProfile();
      state = EmployeeProfileState(profile: profile, isLoading: false);
    } catch (e) {
      state = EmployeeProfileState(isLoading: false, error: e.toString());
    }
  }

  Future<bool> saveProfile(EmployeeProfileModel profile) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final saved =
          await ref.read(employeeProfileRepositoryProvider).saveMyProfile(profile);
      state = EmployeeProfileState(profile: saved, isLoading: false);
      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
      return false;
    }
  }
}

final employeeProfileProvider =
    NotifierProvider<EmployeeProfileNotifier, EmployeeProfileState>(
  EmployeeProfileNotifier.new,
);
