import 'package:dio/dio.dart';
import '../../../../core/constants/api_constants.dart';
import '../../../../shared/models/api_response.dart';
import '../models/work_lookup_models.dart';

class WorkLookupsRemoteDataSource {
  WorkLookupsRemoteDataSource(this._dio);

  final Dio _dio;

  Future<WorkLookups> fetchAll() async {
    final response = await _dio.get(ApiConstants.workLookups);
    final api = ApiResponse.fromJson(
      response.data as Map<String, dynamic>,
      (data) => data as Map<String, dynamic>,
    );
    final json = api.data ?? {};
    return WorkLookups(
      departments: _mapList(json['departments']),
      locations: _mapList(json['locations']),
      sections: _mapList(json['sections']),
    );
  }

  List<WorkLookupItem> _mapList(dynamic value) {
    if (value is! List) return [];
    return value
        .map((e) => WorkLookupItem.fromJson(e as Map<String, dynamic>))
        .toList()
      ..sort((a, b) => a.sortOrder.compareTo(b.sortOrder));
  }
}
