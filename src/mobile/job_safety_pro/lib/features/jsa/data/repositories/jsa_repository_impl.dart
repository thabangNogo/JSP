import '../../../../core/utils/connectivity_service.dart';
import '../../../../shared/enums/assessment_status.dart';
import '../../../sync/data/models/sync_queue_item.dart';
import '../../../sync/services/sync_service.dart';
import '../../domain/repositories/jsa_repository.dart';
import '../datasources/jsa_local_datasource.dart';
import '../datasources/jsa_remote_datasource.dart';
import '../models/jsa_models.dart';
import '../utils/assessment_detail_mapper.dart';
import '../utils/risk_level_id_resolver.dart';

class JsaRepositoryImpl implements JsaRepository {
  JsaRepositoryImpl({
    required JsaRemoteDataSource remote,
    required JsaLocalDataSource local,
    required ConnectivityService connectivity,
    required SyncService syncService,
  })  : _remote = remote,
        _local = local,
        _connectivity = connectivity,
        _syncService = syncService;

  final JsaRemoteDataSource _remote;
  final JsaLocalDataSource _local;
  final ConnectivityService _connectivity;
  final SyncService _syncService;

  @override
  Future<List<JsaModel>> getAssessmentHistory() async {
    if (await _connectivity.isOnline) {
      try {
        final jsas = await _remote.getJsas();
        await _local.cacheJsas(jsas);
        return jsas;
      } catch (_) {}
    }
    return _local.getCachedJsas();
  }

  @override
  Future<List<AssessmentDraftModel>> getDrafts() async {
    final deletedIds = await _local.getDeletedAssessmentIds();
    final local = (await _local.getDrafts())
        .where((d) => !_isDeletedDraft(d, deletedIds))
        .toList();
    if (!await _connectivity.isOnline) return local;

    try {
      final remoteDrafts = await _remote.getSummaries(AssessmentStatus.draft);
      final remoteSubmitted = await _remote.getSummaries(AssessmentStatus.submitted);
      final remoteApproved = await _remote.getSummaries(AssessmentStatus.approved);
      final remote = [...remoteDrafts, ...remoteSubmitted, ...remoteApproved];

      final byRemoteId = {for (final d in local) if (d.remoteId != null) d.remoteId!: d};
      final merged = <AssessmentDraftModel>[...local];

      for (final r in remote) {
        final remoteId = r.remoteId;
        if (remoteId == null || deletedIds.contains(remoteId)) {
          continue;
        }

        final existing = byRemoteId[remoteId];
        if (existing != null) {
          final idx = merged.indexWhere((d) => d.localId == existing.localId);
          if (idx >= 0) {
            merged[idx] = existing.copyWith(
              title: r.title.isNotEmpty ? r.title : existing.title,
              department: r.department.isNotEmpty ? r.department : existing.department,
              location: r.location.isNotEmpty ? r.location : existing.location,
              section: r.section.isNotEmpty ? r.section : existing.section,
              workLocationId: r.workLocationId ?? existing.workLocationId,
              workSectionId: r.workSectionId ?? existing.workSectionId,
              currentStep: r.currentStep,
              status: _preferStatus(existing.status, r.status),
              isSynced: true,
              updatedAt: r.updatedAt ?? existing.updatedAt,
            );
          }
        } else if (!deletedIds.contains(r.localId)) {
          merged.add(
            AssessmentDraftModel(
              localId: remoteId,
              remoteId: remoteId,
              title: r.title,
              jobDescription: r.jobDescription,
              companyId: r.companyId,
              plantId: r.plantId,
              departmentId: r.departmentId,
              department: r.department,
              location: r.location,
              section: r.section,
              workLocationId: r.workLocationId,
              workSectionId: r.workSectionId,
              currentStep: r.currentStep,
              status: r.status,
              isSynced: true,
              updatedAt: r.updatedAt,
            ),
          );
        }
      }

      final deduped = _dedupeAssessments(merged);
      for (final draft in deduped) {
        await _local.saveDraft(draft);
      }
      return deduped;
    } catch (_) {
      return local;
    }
  }

  bool _isDeletedDraft(AssessmentDraftModel draft, Set<String> deletedIds) {
    if (deletedIds.contains(draft.localId)) return true;
    final remoteId = draft.remoteId;
    return remoteId != null && deletedIds.contains(remoteId);
  }

  static int _statusRank(AssessmentStatus status) {
    switch (status) {
      case AssessmentStatus.approved:
        return 4;
      case AssessmentStatus.rejected:
        return 3;
      case AssessmentStatus.submitted:
        return 2;
      case AssessmentStatus.draft:
        return 1;
    }
  }

  static AssessmentStatus _preferStatus(AssessmentStatus a, AssessmentStatus b) {
    return _statusRank(a) >= _statusRank(b) ? a : b;
  }

  static List<AssessmentDraftModel> _dedupeAssessments(List<AssessmentDraftModel> items) {
    final byKey = <String, AssessmentDraftModel>{};
    for (final item in items) {
      final key = item.remoteId ?? item.localId;
      final existing = byKey[key];
      if (existing == null) {
        byKey[key] = item;
        continue;
      }
      byKey[key] = _statusRank(item.status) >= _statusRank(existing.status) ? item : existing;
    }
    return byKey.values.toList();
  }

  @override
  Future<AssessmentDraftModel> saveDraft(AssessmentDraftModel draft) async {
    if (await _local.isAssessmentDeleted(draft.localId) ||
        (draft.remoteId != null && await _local.isAssessmentDeleted(draft.remoteId!))) {
      throw StateError('This assessment was deleted and cannot be saved.');
    }

    var updated = draft.copyWith(isSynced: false);
    await _local.saveDraft(updated);

    final hasJobSite =
        updated.workLocationId != null &&
        updated.workLocationId!.isNotEmpty &&
        updated.workSectionId != null &&
        updated.workSectionId!.isNotEmpty;

    if (await _connectivity.isOnline &&
        draft.status == AssessmentStatus.draft &&
        hasJobSite) {
      try {
        final response = await _remote.saveDraft(_buildDraftPayload(updated));
        final remoteId = response['id']?.toString();
        if (remoteId != null) {
          updated = updated.copyWith(
            remoteId: remoteId,
            isSynced: true,
            workLocationId: response['workLocationId']?.toString() ?? updated.workLocationId,
            workSectionId: response['workSectionId']?.toString() ?? updated.workSectionId,
            department: response['department'] as String? ?? updated.department,
            location: response['location'] as String? ?? updated.location,
            section: response['section'] as String? ?? updated.section,
          );
          await _local.saveDraft(updated);
        }
      } catch (_) {}
    }
    return updated;
  }

  @override
  Future<void> deleteDraft(String localId) async {
    final draft = await _local.getDraft(localId);

    if (draft != null && !draft.status.canDelete) {
      throw StateError('Only draft assessments can be deleted.');
    }

    final idsToForget = <String>{
      localId,
      if (draft?.remoteId != null) draft!.remoteId!,
    };

    await _local.rememberDeletedAssessmentIds(idsToForget);

    if (await _connectivity.isOnline) {
      final remoteId = draft?.remoteId ?? localId;
      try {
        await _remote.deleteDraft(remoteId);
      } catch (_) {
        // Tombstone prevents re-import on refresh if the server delete fails.
      }
    }

    await _local.deleteDraft(localId);
    if (draft?.remoteId != null && draft!.remoteId != localId) {
      await _local.deleteDraft(draft.remoteId!);
    }
  }

  @override
  Future<AssessmentDraftModel?> getDraft(String localId) => _local.getDraft(localId);

  @override
  Future<bool> isDraftDeleted(String id) => _local.isAssessmentDeleted(id);

  @override
  Future<AssessmentDetailModel> getAssessmentDetail(String localId) async {
    final local = await _local.getDraft(localId);
    final remoteId = local?.remoteId ?? localId;

    if (await _connectivity.isOnline && remoteId.isNotEmpty) {
      try {
        final json = await _remote.getJsaById(remoteId);
        if (json.isNotEmpty) {
          return assessmentDetailFromApi(json, localDraft: local);
        }
      } catch (_) {}
    }

    if (local != null) {
      return assessmentDetailFromDraft(local);
    }

    throw StateError('Assessment not found.');
  }

  static final List<RiskLevelModel> _offlineRiskLevels = [
    RiskLevelModel(id: 'offline-low', code: 'LOW', name: 'Low', numericValue: 1, colorHex: '#4CAF50'),
    RiskLevelModel(id: 'offline-med', code: 'MED', name: 'Medium', numericValue: 2, colorHex: '#FF9800'),
    RiskLevelModel(id: 'offline-high', code: 'HIGH', name: 'High', numericValue: 3, colorHex: '#F44336'),
    RiskLevelModel(id: 'offline-crit', code: 'CRIT', name: 'Critical', numericValue: 4, colorHex: '#9C27B0'),
  ];

  @override
  Future<List<RiskLevelModel>> getRiskLevels() async {
    if (await _connectivity.isOnline) {
      try {
        final levels = await _remote.getRiskLevels();
        if (levels.isNotEmpty) return levels;
      } catch (_) {}
    }
    return _offlineRiskLevels;
  }

  String _buildJobDescriptionWithQuickAssessment(AssessmentDraftModel draft) {
    final summary = draft.quickAssessment.toSummaryText();
    if (summary.trim().isEmpty) return draft.jobDescription;
    return '${draft.jobDescription}\n\n---\n$summary';
  }

  Future<List<RiskLevelModel>> _fetchApiRiskLevels() async {
    try {
      final levels = await _remote.getRiskLevels();
      if (levels.isNotEmpty) return levels;
    } catch (_) {}
    throw StateError(
      'Could not load risk levels from the server. Check your connection and try again.',
    );
  }

  Map<String, dynamic> _buildHazardsAndControls(AssessmentDraftModel draft) {
    final hazards = <Map<String, dynamic>>[];
    for (var i = 0; i < draft.hazards.length; i++) {
      final hazard = draft.hazards[i];
      final riskLevelId = hazard.riskLevelId;
      if (riskLevelId == null) continue;

      final description = hazard.consequence.isNotEmpty
          ? '${hazard.description}\n\nWhat can go wrong: ${hazard.consequence}'
          : hazard.description;

      hazards.add({
        'clientHazardId': hazard.id,
        'description': description,
        'riskLevelId': riskLevelId,
        'residualRiskLevelId': riskLevelId,
        'sortOrder': i,
      });
    }

    final controls = draft.controls
        .map(
          (control) => {
            'clientHazardId': control.hazardId,
            'description': control.description,
            'hierarchyOfControl': control.hierarchyOfControl,
            'isImplemented': control.isImplemented,
          },
        )
        .toList();

    return {'hazards': hazards, 'controls': controls};
  }

  Map<String, dynamic> _buildDraftPayload(AssessmentDraftModel draft) {
    final children = _buildHazardsAndControls(draft);
    return {
      if (draft.remoteId != null) 'id': draft.remoteId,
      'companyId': draft.companyId,
      'plantId': draft.plantId,
      'departmentId': draft.departmentId,
      'title': draft.title,
      'jobDescription': _buildJobDescriptionWithQuickAssessment(draft),
      'currentStep': draft.currentStep,
      if (draft.workLocationId != null) 'workLocationId': draft.workLocationId,
      if (draft.workSectionId != null) 'workSectionId': draft.workSectionId,
      'signatureStoragePath': draft.signaturePath,
      if (draft.signOffName != null) 'signOffName': draft.signOffName,
      if (draft.signOffSurname != null) 'signOffSurname': draft.signOffSurname,
      if (draft.signOffCompanyNumber != null) 'signOffCompanyNumber': draft.signOffCompanyNumber,
      if (draft.signOffOccupation != null) 'signOffOccupation': draft.signOffOccupation,
      ...children,
    };
  }

  Map<String, dynamic> _buildSubmitPayload(AssessmentDraftModel draft) {
    final children = _buildHazardsAndControls(draft);
    return {
      'companyId': draft.companyId,
      'plantId': draft.plantId,
      'departmentId': draft.departmentId,
      'title': draft.title,
      'jobDescription': _buildJobDescriptionWithQuickAssessment(draft),
      'workLocationId': draft.workLocationId,
      'workSectionId': draft.workSectionId,
      'observeStopNotes': draft.observeStopNotes,
      'conversationNotes': draft.conversationNotes,
      'signatureStoragePath': draft.signaturePath,
      if (draft.signOffName != null) 'signOffName': draft.signOffName,
      if (draft.signOffSurname != null) 'signOffSurname': draft.signOffSurname,
      if (draft.signOffCompanyNumber != null) 'signOffCompanyNumber': draft.signOffCompanyNumber,
      if (draft.signOffOccupation != null) 'signOffOccupation': draft.signOffOccupation,
      ...children,
    };
  }

  @override
  Future<JsaModel> submitDraft(AssessmentDraftModel draft) async {
    final isOnline = await _connectivity.isOnline;
    late final AssessmentDraftModel normalizedDraft;

    if (isOnline) {
      final apiRiskLevels = await _fetchApiRiskLevels();
      normalizedDraft = normalizeDraftRiskLevelIds(draft, apiRiskLevels);

      final hasInvalidHazard = normalizedDraft.hazards.any(
        (hazard) =>
            hazard.riskLevelId == null || !isValidGuid(hazard.riskLevelId!),
      );

      if (hasInvalidHazard) {
        final missing = normalizedDraft.hazards
            .where((h) => h.riskLevelId == null || !isValidGuid(h.riskLevelId!))
            .map((h) => h.description)
            .join(', ');
        throw StateError(
          missing.isEmpty
              ? 'One or more hazards have invalid risk levels. Re-open Assess Risks and select levels again.'
              : 'Set a valid risk level for: $missing. Open Assess Risks (step 3) and select again.',
        );
      }

      await _local.saveDraft(normalizedDraft);
    } else {
      normalizedDraft = draft;
    }

    final submittedDraft = normalizedDraft.copyWith(
      status: AssessmentStatus.submitted,
      currentStep: 4,
    );
    await _local.saveDraft(submittedDraft);

    final payload = _buildSubmitPayload(submittedDraft);

    if (isOnline) {
      try {
        String? remoteId = submittedDraft.remoteId;
        if (remoteId == null) {
          final saved = await _remote.saveDraft(_buildDraftPayload(submittedDraft));
          remoteId = saved['id']?.toString();
        }

        if (remoteId != null) {
          await _remote.submitDraft(remoteId, payload);
          await _local.saveDraft(
            submittedDraft.copyWith(remoteId: remoteId, isSynced: true),
          );
          return JsaModel(
            id: remoteId,
            title: submittedDraft.title,
            jobDescription: submittedDraft.jobDescription,
            status: 'Submitted',
            plantId: submittedDraft.plantId,
            departmentId: submittedDraft.departmentId,
          );
        }

        final jsa = await _remote.createJsa(payload);
        await _local.saveDraft(submittedDraft.copyWith(remoteId: jsa.id, isSynced: true));
        return jsa;
      } catch (e) {
        throw StateError('Submit failed: $e');
      }
    }

    await _syncService.enqueue(
      SyncQueueItem(
        id: draft.localId,
        action: SyncActionType.createJsa,
        payload: payload,
        createdAt: DateTime.now(),
      ),
    );
    return JsaModel(
      id: draft.localId,
      title: draft.title,
      jobDescription: draft.jobDescription,
      status: 'Submitted',
      plantId: draft.plantId,
      departmentId: draft.departmentId,
    );
  }
}
