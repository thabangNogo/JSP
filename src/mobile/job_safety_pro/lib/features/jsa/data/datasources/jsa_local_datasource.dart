import '../../../../core/storage/hive_service.dart';
import '../models/jsa_models.dart';

class JsaLocalDataSource {
  JsaLocalDataSource(this._hiveService);

  final HiveService _hiveService;

  Future<void> _ensureReady() => HiveService.ensureAllBoxesOpen();

  Future<List<AssessmentDraftModel>> getDrafts() async {
    await _ensureReady();
    return _hiveService.draftsBox.values
        .map((e) => AssessmentDraftModel.fromJson(Map<String, dynamic>.from(e)))
        .toList()
      ..sort((a, b) => (b.updatedAt ?? DateTime.now())
          .compareTo(a.updatedAt ?? DateTime.now()));
  }

  Future<AssessmentDraftModel?> getDraft(String localId) async {
    await _ensureReady();
    final map = _hiveService.draftsBox.get(localId);
    if (map == null) return null;
    return AssessmentDraftModel.fromJson(Map<String, dynamic>.from(map));
  }

  Future<void> saveDraft(AssessmentDraftModel draft) async {
    await _ensureReady();
    await _hiveService.draftsBox.put(draft.localId, draft.toJson());
  }

  Future<void> deleteDraft(String localId) async {
    await _ensureReady();
    await _hiveService.draftsBox.delete(localId);
  }

  static const _deletedIdsKey = 'ids';

  Future<Set<String>> getDeletedAssessmentIds() async {
    await _ensureReady();
    final raw = _hiveService.deletedAssessmentIdsBox.get(_deletedIdsKey);
    if (raw is! List) return {};
    return raw.map((e) => e.toString()).where((id) => id.isNotEmpty).toSet();
  }

  Future<void> rememberDeletedAssessmentIds(Iterable<String> ids) async {
    await _ensureReady();
    final merged = await getDeletedAssessmentIds();
    merged.addAll(ids.where((id) => id.isNotEmpty));
    await _hiveService.deletedAssessmentIdsBox.put(_deletedIdsKey, merged.toList());
  }

  Future<bool> isAssessmentDeleted(String id) async {
    if (id.isEmpty) return false;
    return (await getDeletedAssessmentIds()).contains(id);
  }

  Future<void> cacheJsas(List<JsaModel> jsas) async {
    await _ensureReady();
    await _hiveService.cachedJsasBox.clear();
    for (final jsa in jsas) {
      await _hiveService.cachedJsasBox.put(jsa.id, jsa.toJson());
    }
  }

  Future<List<JsaModel>> getCachedJsas() async {
    await _ensureReady();
    return _hiveService.cachedJsasBox.values
        .map((e) => JsaModel.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }
}
