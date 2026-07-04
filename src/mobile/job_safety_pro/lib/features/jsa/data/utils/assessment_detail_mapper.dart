import '../../../../shared/enums/assessment_status.dart';
import '../models/jsa_models.dart';

/// Splits stored job description into main text and embedded quick-assessment summary.
(String main, String? quickSummary) splitJobDescription(String jobDescription) {
  const separator = '\n\n---\n';
  final index = jobDescription.indexOf(separator);
  if (index < 0) {
    return (jobDescription.trim(), null);
  }
  final main = jobDescription.substring(0, index).trim();
  final quick = jobDescription.substring(index + separator.length).trim();
  return (main, quick.isEmpty ? null : quick);
}

AssessmentDetailModel assessmentDetailFromDraft(AssessmentDraftModel draft) {
  final (mainDescription, quickSummary) = splitJobDescription(draft.jobDescription);
  final quickFromModel = draft.quickAssessment.toSummaryText().trim();
  return AssessmentDetailModel(
    id: draft.remoteId ?? draft.localId,
    localId: draft.localId,
    title: draft.title,
    jobDescription: mainDescription,
    quickAssessmentSummary: quickSummary ?? (quickFromModel.isEmpty ? null : quickFromModel),
    observeStopNotes: draft.observeStopNotes,
    conversationNotes: draft.conversationNotes,
    status: draft.status,
    lastUpdated: draft.updatedAt,
    signOffName: draft.signOffName,
    signOffSurname: draft.signOffSurname,
    signOffCompanyNumber: draft.signOffCompanyNumber,
    signOffOccupation: draft.signOffOccupation,
    signaturePath: draft.signaturePath,
    department: draft.department,
    location: draft.location,
    section: draft.section,
    hazards: draft.hazards
        .map(
          (h) => HazardDetailItem(
            description: h.description,
            consequence: h.consequence,
            riskLevelId: h.riskLevelId,
            residualRiskLevelId: h.residualRiskLevelId,
          ),
        )
        .toList(),
    controls: draft.controls
        .map(
          (c) => ControlDetailItem(
            description: c.description,
            hierarchyOfControl: c.hierarchyOfControl,
            isImplemented: c.isImplemented,
          ),
        )
        .toList(),
    currentStep: draft.currentStep,
  );
}

AssessmentDetailModel assessmentDetailFromApi(
  Map<String, dynamic> json, {
  AssessmentDraftModel? localDraft,
}) {
  final status = _parseStatus(json['status']);
  final rawDescription = json['jobDescription'] as String? ?? '';
  final (mainDescription, quickSummary) = splitJobDescription(rawDescription);

  final hazards = (json['hazards'] as List<dynamic>? ?? [])
      .map((e) {
        final h = e as Map<String, dynamic>;
        return HazardDetailItem(
          description: h['description'] as String? ?? '',
          riskLevelId: h['riskLevelId']?.toString(),
          residualRiskLevelId: h['residualRiskLevelId']?.toString(),
        );
      })
      .toList();

  final controls = (json['controlMeasures'] as List<dynamic>? ?? [])
      .map((e) {
        final c = e as Map<String, dynamic>;
        return ControlDetailItem(
          description: c['description'] as String? ?? '',
          hierarchyOfControl: c['hierarchyOfControl'] as String? ?? '',
          isImplemented: c['isImplemented'] as bool? ?? false,
          hazardId: c['hazardId']?.toString(),
        );
      })
      .toList();

  final id = json['id']?.toString() ?? '';
  return AssessmentDetailModel(
    id: id,
    localId: localDraft?.localId ?? id,
    title: json['title'] as String? ?? '',
    jobDescription: mainDescription,
    quickAssessmentSummary: quickSummary ??
        (localDraft != null && localDraft.quickAssessment.toSummaryText().trim().isNotEmpty
            ? localDraft.quickAssessment.toSummaryText().trim()
            : null),
    observeStopNotes: localDraft?.observeStopNotes ?? '',
    conversationNotes: localDraft?.conversationNotes ?? '',
    status: status,
    lastUpdated: _parseDateTime(json['lastSavedAt']) ?? localDraft?.updatedAt,
    signOffName: json['signOffName'] as String? ?? localDraft?.signOffName,
    signOffSurname: json['signOffSurname'] as String? ?? localDraft?.signOffSurname,
    signOffCompanyNumber:
        json['signOffCompanyNumber'] as String? ?? localDraft?.signOffCompanyNumber,
    signOffOccupation: json['signOffOccupation'] as String? ?? localDraft?.signOffOccupation,
    signaturePath: json['signatureStoragePath'] as String? ?? localDraft?.signaturePath,
    department: json['department'] as String? ?? localDraft?.department ?? '',
    location: json['location'] as String? ?? localDraft?.location ?? '',
    section: json['section'] as String? ?? localDraft?.section ?? '',
    hazards: hazards.isNotEmpty ? hazards : (localDraft != null ? assessmentDetailFromDraft(localDraft).hazards : []),
    controls: controls.isNotEmpty ? controls : (localDraft != null ? assessmentDetailFromDraft(localDraft).controls : []),
    currentStep: json['currentStep'] as int? ?? localDraft?.currentStep ?? 0,
  );
}

AssessmentStatus _parseStatus(dynamic value) => AssessmentStatus.parse(value);

DateTime? _parseDateTime(dynamic value) {
  if (value == null) return null;
  return DateTime.tryParse(value.toString());
}
