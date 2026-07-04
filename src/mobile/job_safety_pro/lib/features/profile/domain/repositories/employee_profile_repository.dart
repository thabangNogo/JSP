import '../../data/models/employee_profile_model.dart';

abstract class EmployeeProfileRepository {
  Future<EmployeeProfileModel?> getMyProfile();
  Future<EmployeeProfileModel> saveMyProfile(EmployeeProfileModel profile);
}
