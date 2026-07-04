import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../../core/router/app_routes.dart';
import '../../../../../core/utils/dio_error_message.dart';
import '../../../../profile/data/models/employee_profile_model.dart';
import '../../../../profile/presentation/providers/employee_profile_provider.dart';
import '../../../data/models/work_lookup_models.dart';
import '../../../domain/services/assessment_validation_service.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../providers/job_information_provider.dart';
import '../../providers/jsa_providers.dart';
import '../../providers/work_lookups_provider.dart';
import '../../widgets/glove_friendly/glove_friendly_controls.dart';
import 'workflow_step_mixin.dart';

class JobInformationScreen extends ConsumerStatefulWidget {
  const JobInformationScreen({super.key, required this.draftId});

  final String draftId;

  @override
  ConsumerState<JobInformationScreen> createState() => _JobInformationScreenState();
}

class _JobInformationScreenState extends ConsumerState<JobInformationScreen>
    with WorkflowStepMixin {
  @override
  String get draftId => widget.draftId;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(employeeProfileProvider.notifier).loadProfile();
      _hydrateFromDraft();
    });
  }

  Future<void> _hydrateFromDraft() async {
    var draft = ref.read(assessmentWorkflowProvider);
    if (draft?.localId != widget.draftId) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(widget.draftId);
      draft = ref.read(assessmentWorkflowProvider);
    }
    if (draft == null || !mounted) return;

    try {
      final lookups = await ref.read(workLookupsProvider.future);
      if (!mounted) return;
      ref.read(jobInformationProvider.notifier).hydrateFromDraft(
            workLocationId: resolveWorkLookupId(
              lookups.locations,
              id: draft.workLocationId,
              name: draft.location,
            ),
            workSectionId: resolveWorkLookupId(
              lookups.sections,
              id: draft.workSectionId,
              name: draft.section,
            ),
            force: true,
          );
    } catch (_) {
      if (!mounted) return;
      ref.read(jobInformationProvider.notifier).hydrateFromDraft(
            workLocationId: draft.workLocationId,
            workSectionId: draft.workSectionId,
            force: true,
          );
    }
  }

  Future<void> _continue() async {
    final profile = ref.read(employeeProfileProvider).profile;
    if (profile == null || !profile.isComplete) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Complete your employee profile (including department) before continuing.'),
          ),
        );
      }
      return;
    }

    final validation = ref.read(assessmentValidationServiceProvider).validateJobInformation(
          ref.read(jobInformationProvider),
        );
    if (!validation.isValid) {
      ref.read(jobInformationProvider.notifier).validate();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(validation.message ?? 'Complete all required fields.')),
        );
      }
      return;
    }

    final form = ref.read(jobInformationProvider);
    final lookups = await ref.read(workLookupsProvider.future);
    final locationName = lookups.locations
        .firstWhere((l) => l.id == form.workLocationId)
        .name;
    final sectionName =
        lookups.sections.firstWhere((s) => s.id == form.workSectionId).name;
    final departmentName = profile.resolveDepartmentName(lookups.departments);

    final draft = ref.read(assessmentWorkflowProvider);
    if (draft == null) return;

    await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
          draft.copyWith(
            workLocationId: form.workLocationId,
            workSectionId: form.workSectionId,
            location: locationName,
            section: sectionName,
            department: departmentName,
          ),
        );

    if (!mounted) return;
    await ref.read(assessmentWorkflowProvider.notifier).nextStep();
    await ref.read(draftAutoSaveServiceProvider).saveDraft(
          showSnackBar: true,
          context: context,
        );
    if (!mounted) return;
    context.go('/jsas/workflow/${widget.draftId}/quick-assessment');
  }

  @override
  Widget build(BuildContext context) {
    final profile = ref.watch(employeeProfileProvider).profile;
    final form = ref.watch(jobInformationProvider);
    final lookupsAsync = ref.watch(workLookupsProvider);

    return Padding(
      padding: const EdgeInsets.all(kGloveHorizontalPadding),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Step 1: Job Information',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                ),
          ),
          const SizedBox(height: 8),
          Text(
            'Your employee details are shown below. Select the job location and section.',
            style: TextStyle(color: Colors.grey.shade700, fontSize: 16),
          ),
          const SizedBox(height: kGloveSectionSpacing),
          Expanded(
            child: lookupsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (e, _) {
                final isUnauthorized =
                    e is DioException && e.response?.statusCode == 401;
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          isUnauthorized ? Icons.lock_outline : Icons.cloud_off,
                          size: 48,
                          color: Colors.grey.shade600,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          isUnauthorized
                              ? 'Your session has expired. Sign in again to load locations and sections.'
                              : dioErrorMessage(e),
                          textAlign: TextAlign.center,
                          style: const TextStyle(fontSize: 16),
                        ),
                        const SizedBox(height: 20),
                        if (!isUnauthorized)
                          OutlinedButton(
                            onPressed: () => ref.invalidate(workLookupsProvider),
                            child: const Text('Retry'),
                          ),
                        if (isUnauthorized)
                          ElevatedButton(
                            onPressed: () => context.go(AppRoutes.login),
                            child: const Text('Sign in'),
                          ),
                      ],
                    ),
                  ),
                );
              },
              data: (lookups) => ListView(
                children: [
                  _ReadOnlyProfileCard(
                    profile: profile,
                    departmentLabel: profile?.resolveDepartmentName(lookups.departments) ?? '',
                  ),
                  const SizedBox(height: kGloveSectionSpacing),
                  GloveFriendlyDropdown(
                    label: 'Location',
                    value: form.workLocationId,
                    items: lookups.locations,
                    onChanged: ref.read(jobInformationProvider.notifier).setWorkLocationId,
                    errorText: form.showValidation &&
                            (form.workLocationId == null || form.workLocationId!.isEmpty)
                        ? 'Location is required'
                        : null,
                  ),
                  const SizedBox(height: kGloveSectionSpacing),
                  GloveFriendlyDropdown(
                    label: 'Section',
                    value: form.workSectionId,
                    items: lookups.sections,
                    onChanged: ref.read(jobInformationProvider.notifier).setWorkSectionId,
                    errorText: form.showValidation &&
                            (form.workSectionId == null || form.workSectionId!.isEmpty)
                        ? 'Section is required'
                        : null,
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),
          SizedBox(
            height: kGloveMinTouchHeight,
            child: ElevatedButton(
              onPressed: _continue,
              child: const Text('Continue', style: TextStyle(fontSize: 18)),
            ),
          ),
        ],
      ),
    );
  }
}

class _ReadOnlyProfileCard extends StatelessWidget {
  const _ReadOnlyProfileCard({
    required this.profile,
    required this.departmentLabel,
  });

  final EmployeeProfileModel? profile;
  final String departmentLabel;

  @override
  Widget build(BuildContext context) {
    final p = profile;
    if (p == null || !p.isComplete) {
      return Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            'Complete your employee profile before starting an assessment.',
            style: TextStyle(color: Colors.orange.shade800, fontSize: 16),
          ),
        ),
      );
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Employee details',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 12),
            _ReadOnlyRow(label: 'Name', value: p.name),
            _ReadOnlyRow(label: 'Surname', value: p.surname),
            _ReadOnlyRow(label: 'Company number', value: p.companyNumber),
            _ReadOnlyRow(label: 'Occupation', value: p.occupation),
            _ReadOnlyRow(
              label: 'Department',
              value: departmentLabel.isNotEmpty ? departmentLabel : '—',
            ),
          ],
        ),
      ),
    );
  }
}

class _ReadOnlyRow extends StatelessWidget {
  const _ReadOnlyRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 130,
            child: Text(
              label,
              style: TextStyle(color: Colors.grey.shade700, fontWeight: FontWeight.w600),
            ),
          ),
          Expanded(child: Text(value.isEmpty ? '—' : value, style: const TextStyle(fontSize: 16))),
        ],
      ),
    );
  }
}
