import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../shared/enums/assessment_status.dart';
import '../../domain/services/hazard_selection_mapper.dart';
import '../../presentation/providers/identify_hazards_provider.dart';
import '../../presentation/providers/job_information_provider.dart';
import '../../presentation/providers/jsa_providers.dart';
import '../../presentation/providers/quick_assessment_provider.dart';
import '../../../profile/presentation/providers/employee_profile_provider.dart';

/// Persists the active assessment draft locally (and syncs step data).
class DraftAutoSaveService {
  DraftAutoSaveService(this.ref);

  final Ref ref;
  Timer? _timer;
  String? _activeDraftId;
  Future<bool>? _saveInFlight;

  void start(String draftId) {
    _activeDraftId = draftId;
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 30), (_) {
      saveDraft(showSnackBar: true);
    });
  }

  void stop() {
    _timer?.cancel();
    _timer = null;
    _activeDraftId = null;
  }

  bool isTracking(String draftId) => _activeDraftId == draftId;

  Future<bool> saveDraft({bool showSnackBar = false, BuildContext? context}) async {
    if (_saveInFlight != null) {
      return _saveInFlight!;
    }

    final draftId = _activeDraftId;
    if (draftId == null) return false;

    _saveInFlight = _saveDraftInternal(
      draftId: draftId,
      showSnackBar: showSnackBar,
      context: context,
    );
    try {
      return await _saveInFlight!;
    } finally {
      _saveInFlight = null;
    }
  }

  Future<bool> _saveDraftInternal({
    required String draftId,
    required bool showSnackBar,
    BuildContext? context,
  }) async {

    if (await ref.read(jsaRepositoryProvider).isDraftDeleted(draftId)) {
      stop();
      return false;
    }

    var draft = ref.read(assessmentWorkflowProvider);
    if (draft == null || draft.localId != draftId) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(draftId);
      draft = ref.read(assessmentWorkflowProvider);
    }
    if (draft == null) {
      stop();
      return false;
    }

    final quick = ref.read(quickAssessmentProvider);
    final jobInfo = ref.read(jobInformationProvider);
    final hazards = ref.read(identifyHazardsProvider);
    final profile = ref.read(employeeProfileProvider).profile;

    final mapped = HazardSelectionMapper.toDraftModelsPreservingExisting(
      hazards.selections,
      draft.hazards,
    );

    final workflow = ref.read(assessmentWorkflowProvider);
    final step = workflow?.currentStep ?? draft.currentStep;

    final updated = draft.copyWith(
      quickAssessment: quick.assessment,
      workLocationId: jobInfo.isHydrated ? jobInfo.workLocationId : draft.workLocationId,
      workSectionId: jobInfo.isHydrated ? jobInfo.workSectionId : draft.workSectionId,
      hazards: mapped.hazards,
      controls: mapped.controls,
      currentStep: step,
      signOffName: profile?.name,
      signOffSurname: profile?.surname,
      signOffCompanyNumber: profile?.companyNumber,
      signOffOccupation: profile?.occupation,
      status: AssessmentStatus.draft,
    );

    final saved = await ref.read(jsaRepositoryProvider).saveDraft(updated);
    ref.read(assessmentWorkflowProvider.notifier).applySavedDraft(saved);
    ref.invalidate(assessmentDraftsProvider);

    if (showSnackBar && context != null && context.mounted) {
      ScaffoldMessenger.maybeOf(context)?.showSnackBar(
        const SnackBar(content: Text('Draft saved')),
      );
    }
    return true;
  }
}

final draftAutoSaveServiceProvider = Provider<DraftAutoSaveService>((ref) {
  final service = DraftAutoSaveService(ref);
  ref.onDispose(service.stop);
  return service;
});
