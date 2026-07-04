import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../network/dio_provider.dart';
import 'hive_service.dart';

/// Clears device-local data that belongs to one signed-in user.
class UserSessionCleanup {
  UserSessionCleanup(this._hiveService);

  final HiveService _hiveService;

  Future<void> clear() async {
    await HiveService.ensureAllBoxesOpen();
    await _hiveService.draftsBox.clear();
    await _hiveService.cachedJsasBox.clear();
    await _hiveService.deletedAssessmentIdsBox.clear();
    await _hiveService.syncQueueBox.clear();
    await _hiveService.userProfileBox.delete('employee_profile');
  }
}

final userSessionCleanupProvider = Provider<UserSessionCleanup>(
  (ref) => UserSessionCleanup(ref.watch(hiveServiceProvider)),
);
