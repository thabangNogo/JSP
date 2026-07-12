import 'package:dio/dio.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../shared/models/api_response.dart';

class PpeRemoteDataSource {
  PpeRemoteDataSource(this._dio);

  final Dio _dio;

  Future<Map<String, dynamic>> getMyPpe() async {
    final response = await _dio.get(ApiConstants.ppeMy);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }
}
