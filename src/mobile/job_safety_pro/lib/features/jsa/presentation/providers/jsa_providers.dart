import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/dio_provider.dart';
import '../../../../core/utils/connectivity_service.dart';
import '../../../../shared/enums/assessment_workflow_step.dart';
import '../../../camera/services/photo_capture_service.dart';
import '../../../notifications/services/notification_service.dart';
import '../../../sync/services/sync_service.dart';
import '../../data/datasources/jsa_local_datasource.dart';
import '../../data/datasources/jsa_remote_datasource.dart';
import '../../../auth/presentation/providers/auth_provider.dart';
import '../../../profile/presentation/providers/employee_profile_provider.dart';
import '../../../safety/presentation/providers/safety_providers.dart';
import '../../../../shared/enums/assessment_status.dart';
import '../../data/models/jsa_models.dart';
import '../../data/repositories/jsa_repository_impl.dart';
import '../../data/utils/risk_level_id_resolver.dart';
import '../../domain/repositories/jsa_repository.dart';

final syncServiceProvider = Provider<SyncService>(
  (ref) => SyncService(ref.watch(hiveServiceProvider)),
);

final notificationServiceProvider = Provider<NotificationService>(
  (ref) => NotificationService(),
);

final photoCaptureServiceProvider = Provider<PhotoCaptureService>(
  (ref) => PhotoCaptureService(),
);

final jsaRepositoryProvider = Provider<JsaRepository>((ref) {
  return JsaRepositoryImpl(
    remote: JsaRemoteDataSource(ref.watch(dioProvider)),
    local: JsaLocalDataSource(ref.watch(hiveServiceProvider)),
    connectivity: ref.watch(connectivityServiceProvider),
    syncService: ref.watch(syncServiceProvider),
  );
});

final jsaHistoryProvider = FutureProvider<List<JsaModel>>((ref) async {
  return ref.watch(jsaRepositoryProvider).getAssessmentHistory();
});

final assessmentDraftsProvider = FutureProvider<List<AssessmentDraftModel>>((ref) async {
  ref.watch(authProvider.select((s) => s.user?.id));
  return ref.watch(jsaRepositoryProvider).getDrafts();
});

final assessmentDetailProvider =
    FutureProvider.family<AssessmentDetailModel, String>((ref, localId) async {
  return ref.watch(jsaRepositoryProvider).getAssessmentDetail(localId);
});

/// Reloads drafts from local storage and merges remote summaries when online.
Future<void> refreshAssessmentDrafts(WidgetRef ref) async {
  ref.invalidate(assessmentDraftsProvider);
  ref.invalidate(syncPendingCountProvider);
  ref.invalidate(employeeKpiProvider);
  ref.invalidate(injuryFreeDaysProvider);
  await Future.wait([
    ref.read(assessmentDraftsProvider.future),
    ref.read(syncPendingCountProvider.future),
    ref.read(employeeKpiProvider.notifier).load(),
    ref.read(injuryFreeDaysProvider.notifier).load(),
  ]);
}

final riskLevelsProvider = FutureProvider<List<RiskLevelModel>>((ref) async {
  return ref.watch(jsaRepositoryProvider).getRiskLevels();
});

class AssessmentWorkflowNotifier extends Notifier<AssessmentDraftModel?> {
  @override
  AssessmentDraftModel? build() => null;

  Future<void> loadDraft(String localId) async {
    state = await ref.read(jsaRepositoryProvider).getDraft(localId);
  }

  void clearSession() {
    state = null;
  }

  Future<void> startNewDraft({
    required String companyId,
    required String plantId,
    required String departmentId,
    required String title,
    required String jobDescription,
  }) async {
    state = null;
    final id = ref.read(syncServiceProvider).generateId();
    final draft = AssessmentDraftModel(
      localId: id,
      title: title,
      jobDescription: jobDescription,
      companyId: companyId,
      plantId: plantId,
      departmentId: departmentId,
      currentStep: 0,
      updatedAt: DateTime.now(),
    );
    state = await ref.read(jsaRepositoryProvider).saveDraft(draft);
  }

  Future<void> updateDraft(AssessmentDraftModel draft) async {
    state = await ref.read(jsaRepositoryProvider).saveDraft(draft);
  }

  /// Updates in-memory draft after persistence without a second local write.
  void applySavedDraft(AssessmentDraftModel draft) {
    state = draft;
  }

  Future<void> nextStep() async {
    if (state == null) return;
    final next = (state!.currentStep + 1).clamp(0, AssessmentWorkflowStep.totalSteps - 1);
    await updateDraft(state!.copyWith(currentStep: next));
  }

  Future<void> previousStep() async {
    if (state == null) return;
    final prev = (state!.currentStep - 1).clamp(0, AssessmentWorkflowStep.totalSteps - 1);
    await updateDraft(state!.copyWith(currentStep: prev));
  }

  AssessmentWorkflowStep get currentStepEnum =>
      AssessmentWorkflowStep.fromIndex(state?.currentStep ?? 0);

  Future<JsaModel?> submit() async {
    if (state == null) return null;
    final user = ref.read(authProvider).user;
    if (user?.plantId == null || user?.departmentId == null) {
      throw StateError(
        'Your profile is missing plant or department. Please log out and sign in again.',
      );
    }

    final profile = ref.read(employeeProfileProvider).profile;
    if (profile == null || !profile.isComplete) {
      throw StateError(
        'Complete your employee profile before submitting an assessment.',
      );
    }

    var draft = state!.copyWith(
      companyId: user!.companyId,
      plantId: user.plantId!,
      departmentId: user.departmentId!,
      signOffName: profile.name,
      signOffSurname: profile.surname,
      signOffCompanyNumber: profile.companyNumber,
      signOffOccupation: profile.occupation,
      status: AssessmentStatus.submitted,
    );

    if (draft.hazards.any((h) => h.riskLevelId == null)) {
      throw StateError(
        'Assign a risk level to every hazard on the Assess Risks step before submitting.',
      );
    }

    try {
      final levels = await ref.read(riskLevelsProvider.future);
      draft = normalizeDraftRiskLevelIds(draft, levels);
      state = draft;
      await ref.read(jsaRepositoryProvider).saveDraft(draft);
    } catch (_) {
      // Offline levels or network — repository will normalize when online.
    }

    state = draft;

    try {
      final result = await ref.read(jsaRepositoryProvider).submitDraft(draft);
      await ref.read(notificationServiceProvider).showAssessmentReminder(
            'Assessment Submitted',
            '"${draft.title}" has been submitted.',
          );
      state = null;
      return result;
    } on StateError {
      rethrow;
    }
  }
}

final assessmentWorkflowProvider =
    NotifierProvider<AssessmentWorkflowNotifier, AssessmentDraftModel?>(
  AssessmentWorkflowNotifier.new,
);

final syncPendingCountProvider = FutureProvider<int>((ref) async {
  return ref.watch(syncServiceProvider).pendingCount;
});
