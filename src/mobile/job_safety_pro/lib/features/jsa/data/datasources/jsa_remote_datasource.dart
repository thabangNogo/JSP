import 'package:dio/dio.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../shared/enums/assessment_status.dart';
import '../../../../shared/models/api_response.dart';
import '../models/jsa_models.dart';

class JsaRemoteDataSource {
  JsaRemoteDataSource(this._dio);

  final Dio _dio;

  Future<Map<String, dynamic>> getJsaById(String id) async {
    final response = await _dio.get('${ApiConstants.jsas}/$id');
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<List<JsaModel>> getJsas() async {
    final response = await _dio.get(ApiConstants.jsas);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => (data as List<dynamic>)
          .map((e) => JsaModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
    return api.data ?? [];
  }

  Future<List<AssessmentDraftModel>> getSummaries(AssessmentStatus status) async {
    final response = await _dio.get(
      ApiConstants.jsaSummaries,
      queryParameters: {'status': status.value},
    );
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => (data as List<dynamic>).map((e) {
        final json = e as Map<String, dynamic>;
        return AssessmentDraftModel(
          localId: json['id']?.toString() ?? '',
          remoteId: json['id']?.toString(),
          title: json['title'] as String? ?? '',
          jobDescription: '',
          companyId: '',
          plantId: '',
          departmentId: '',
          currentStep: json['currentStep'] as int? ?? 0,
          department: json['department'] as String? ?? '',
          location: json['location'] as String? ?? '',
          section: json['section'] as String? ?? '',
          workLocationId: json['workLocationId']?.toString(),
          workSectionId: json['workSectionId']?.toString(),
          updatedAt: json['lastSavedAt'] != null
              ? DateTime.parse(json['lastSavedAt'] as String)
              : null,
          status: AssessmentStatus.parse(json['status']),
          isSynced: true,
        );
      }).toList(),
    );
    return api.data ?? [];
  }

  Future<Map<String, dynamic>> saveDraft(Map<String, dynamic> payload) async {
    final response = await _dio.post(ApiConstants.jsaDrafts, data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<Map<String, dynamic>> submitDraft(String remoteId, Map<String, dynamic> payload) async {
    final response = await _dio.post('${ApiConstants.jsas}/$remoteId/submit', data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    return api.data ?? {};
  }

  Future<void> deleteDraft(String remoteId) async {
    await _dio.delete('${ApiConstants.jsas}/$remoteId');
  }

  Future<JsaModel> createJsa(Map<String, dynamic> payload) async {
    final response = await _dio.post(ApiConstants.jsas, data: payload);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => JsaModel.fromJson(data as Map<String, dynamic>),
    );
    return api.data!;
  }

  Future<List<RiskLevelModel>> getRiskLevels() async {
    final response = await _dio.get(ApiConstants.riskLevels);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => (data as List<dynamic>)
          .map((e) => RiskLevelModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
    return api.data ?? [];
  }
}
