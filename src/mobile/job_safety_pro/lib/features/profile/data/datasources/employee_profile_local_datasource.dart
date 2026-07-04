import '../../../../core/storage/hive_service.dart';
import '../models/employee_profile_model.dart';

class EmployeeProfileLocalDataSource {
  EmployeeProfileLocalDataSource(this._hive);

  final HiveService _hive;
  static const _key = 'employee_profile';

  Future<EmployeeProfileModel?> getProfile() async {
    final map = _hive.userProfileBox.get(_key);
    if (map == null) return null;
    return EmployeeProfileModel.fromJson(Map<String, dynamic>.from(map));
  }

  Future<void> saveProfile(EmployeeProfileModel profile) async {
    await _hive.userProfileBox.put(_key, profile.toJson());
  }
}
