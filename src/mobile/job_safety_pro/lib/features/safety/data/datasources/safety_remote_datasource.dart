import 'package:dio/dio.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../shared/models/api_response.dart';

class SafetyRemoteDataSource {
  SafetyRemoteDataSource(this._dio);

  final Dio _dio;

  Future<Map<String, dynamic>> submitStopUnsafeWork(Map<String, dynamic> payload) async {
    final response = await _dio.post(ApiConstants.stopUnsafeWork, data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<Map<String, dynamic>> submitNearMissReport(Map<String, dynamic> payload) async {
    final response = await _dio.post('${ApiConstants.nearMisses}/reports', data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<Map<String, dynamic>> getEmployeeKpis() async {
    final response = await _dio.get(ApiConstants.employeeSafetyKpis);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<int> getInjuryFreeDays() async {
    final response = await _dio.get(ApiConstants.injuryFreeDays);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data?['injuryFreeDays'] as int? ?? 0;
  }

  Future<List<Map<String, dynamic>>> getInjuries() async {
    final response = await _dio.get(ApiConstants.injuries);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => (data as List<dynamic>).cast<Map<String, dynamic>>(),
    );
    return api.data ?? [];
  }

  Future<Map<String, dynamic>> getInjuryById(String id) async {
    final response = await _dio.get('${ApiConstants.injuries}/$id');
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<Map<String, dynamic>> createInjury(Map<String, dynamic> payload) async {
    final response = await _dio.post(ApiConstants.injuries, data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<Map<String, dynamic>> updateInjury(String id, Map<String, dynamic> payload) async {
    final response = await _dio.put('${ApiConstants.injuries}/$id', data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<List<Map<String, dynamic>>> getNotifications() async {
    final response = await _dio.get(ApiConstants.notifications);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => (data as List<dynamic>).cast<Map<String, dynamic>>(),
    );
    return api.data ?? [];
  }

  Future<void> markNotificationRead(String id) async {
    await _dio.post('${ApiConstants.notifications}/$id/read');
  }

  Future<void> registerDevice(String fcmToken, String platform) async {
    await _dio.post('${ApiConstants.notifications}/devices', data: {
      'fcmToken': fcmToken,
      'platform': platform,
    });
  }
}
