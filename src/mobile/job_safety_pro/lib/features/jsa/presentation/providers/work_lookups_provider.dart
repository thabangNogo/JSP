import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/work_lookups_remote_datasource.dart';
import '../../data/models/work_lookup_models.dart';

export '../../data/models/work_lookup_models.dart';

final workLookupsProvider = FutureProvider<WorkLookups>((ref) async {
  final remote = WorkLookupsRemoteDataSource(ref.watch(dioProvider));
  return remote.fetchAll();
});
