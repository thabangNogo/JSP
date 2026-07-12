import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/ppe_remote_datasource.dart';

final ppeRemoteDataSourceProvider = Provider<PpeRemoteDataSource>((ref) {
  return PpeRemoteDataSource(ref.watch(dioProvider));
});

final myPpeProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return ref.watch(ppeRemoteDataSourceProvider).getMyPpe();
});
