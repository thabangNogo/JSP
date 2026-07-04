import 'package:dio/dio.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../shared/models/api_response.dart';
import '../models/auth_models.dart';

class AuthRemoteDataSource {
  AuthRemoteDataSource(this._dio);

  final Dio _dio;

  Future<AuthResponseModel> login(LoginRequestModel request) async {
    final response = await _dio.post(ApiConstants.authLogin, data: request.toJson());
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => AuthResponseModel.fromJson(data as Map<String, dynamic>),
    );
    return api.data!;
  }

  Future<UserModel> getCurrentUser() async {
    final response = await _dio.get(ApiConstants.authMe);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => UserModel.fromJson(data as Map<String, dynamic>),
    );
    return api.data!;
  }

  Future<void> logout(String refreshToken) async {
    await _dio.post(ApiConstants.authLogout, data: {'refreshToken': refreshToken});
  }
}
