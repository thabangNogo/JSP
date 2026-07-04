import 'package:dio/dio.dart';
import '../../../../core/network/dio_provider.dart';
import '../models/employee_profile_model.dart';

class EmployeeProfileRemoteDataSource {
  EmployeeProfileRemoteDataSource(this._dio);

  final Dio _dio;

  Future<EmployeeProfileModel?> fetchMyProfile() async {
    final response = await _dio.get('/employee-profiles/me');
    final data = response.data['data'];
    if (data == null) return null;
    return EmployeeProfileModel.fromJson(Map<String, dynamic>.from(data as Map));
  }

  Future<EmployeeProfileModel> saveMyProfile(EmployeeProfileModel profile) async {
    final response = await _dio.put(
      '/employee-profiles/me',
      data: {
        'workDepartmentId': profile.workDepartmentId,
        'name': profile.name,
        'surname': profile.surname,
        'companyNumber': profile.companyNumber,
        'occupation': profile.occupation,
      },
    );
    final data = response.data['data'] as Map;
    return EmployeeProfileModel.fromJson(Map<String, dynamic>.from(data));
  }
}
