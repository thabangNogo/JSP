import '../../data/models/jsa_models.dart';

abstract class JsaRepository {
  Future<List<JsaModel>> getAssessmentHistory();
  Future<List<AssessmentDraftModel>> getDrafts();
  Future<AssessmentDraftModel> saveDraft(AssessmentDraftModel draft);
  Future<void> deleteDraft(String localId);
  Future<bool> isDraftDeleted(String id);
  Future<AssessmentDraftModel?> getDraft(String localId);
  Future<AssessmentDetailModel> getAssessmentDetail(String localId);
  Future<List<RiskLevelModel>> getRiskLevels();
  Future<JsaModel> submitDraft(AssessmentDraftModel draft);
}
