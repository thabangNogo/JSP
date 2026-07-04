import 'package:hive_flutter/hive_flutter.dart';
import '../constants/api_constants.dart';

class HiveService {
  static Future<void> init() async {
    await Hive.initFlutter();
    await ensureAllBoxesOpen();
  }

  /// Opens any Hive boxes that are not yet open (e.g. after hot restart).
  static Future<void> ensureAllBoxesOpen() async {
    await _openBoxIfNeeded<Map>(HiveBoxes.assessmentDrafts);
    await _openBoxIfNeeded(HiveBoxes.deletedAssessmentIds);
    await _openBoxIfNeeded<Map>(HiveBoxes.syncQueue);
    await _openBoxIfNeeded<Map>(HiveBoxes.cachedJsas);
    await _openBoxIfNeeded<Map>(HiveBoxes.userProfile);
  }

  static Future<void> _openBoxIfNeeded<T>(String name) async {
    if (Hive.isBoxOpen(name)) return;
    await Hive.openBox<T>(name);
  }

  Box<Map> get draftsBox => Hive.box<Map>(HiveBoxes.assessmentDrafts);

  Box get deletedAssessmentIdsBox => Hive.box(HiveBoxes.deletedAssessmentIds);

  Box<Map> get syncQueueBox => Hive.box<Map>(HiveBoxes.syncQueue);

  Box<Map> get cachedJsasBox => Hive.box<Map>(HiveBoxes.cachedJsas);

  Box<Map> get userProfileBox => Hive.box<Map>(HiveBoxes.userProfile);
}
