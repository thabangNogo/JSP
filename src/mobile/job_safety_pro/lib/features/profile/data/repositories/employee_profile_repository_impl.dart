import '../../../../core/utils/connectivity_service.dart';
import '../../domain/repositories/employee_profile_repository.dart';
import '../datasources/employee_profile_local_datasource.dart';
import '../datasources/employee_profile_remote_datasource.dart';
import '../models/employee_profile_model.dart';

class EmployeeProfileRepositoryImpl implements EmployeeProfileRepository {
  EmployeeProfileRepositoryImpl({
    required EmployeeProfileLocalDataSource local,
    required EmployeeProfileRemoteDataSource remote,
    required ConnectivityService connectivity,
  })  : _local = local,
        _remote = remote,
        _connectivity = connectivity;

  final EmployeeProfileLocalDataSource _local;
  final EmployeeProfileRemoteDataSource _remote;
  final ConnectivityService _connectivity;

  @override
  Future<EmployeeProfileModel?> getMyProfile() async {
    if (await _connectivity.isOnline) {
      try {
        final remote = await _remote.fetchMyProfile();
        if (remote != null) {
          await _local.saveProfile(remote);
          return remote;
        }
      } catch (_) {
        // Fall back to cache.
      }
    }
    return _local.getProfile();
  }

  @override
  Future<EmployeeProfileModel> saveMyProfile(EmployeeProfileModel profile) async {
    await _local.saveProfile(profile);
    if (await _connectivity.isOnline) {
      try {
        final saved = await _remote.saveMyProfile(profile);
        await _local.saveProfile(saved);
        return saved;
      } catch (_) {
        // Offline — local copy kept.
      }
    }
    return profile;
  }
}
