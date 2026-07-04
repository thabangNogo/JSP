import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../../core/utils/dio_error_message.dart';
import '../../../../../shared/widgets/common_widgets.dart';
import '../../../data/models/jsa_models.dart';
import '../../../domain/services/assessment_validation_service.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../providers/jsa_providers.dart';
import '../../widgets/risk_level_dropdown_utils.dart';
import 'workflow_step_mixin.dart';
import 'workflow_routes.dart';

class AssessRisksScreen extends ConsumerStatefulWidget {
  const AssessRisksScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<AssessRisksScreen> createState() => _AssessRisksScreenState();
}

class _AssessRisksScreenState extends ConsumerState<AssessRisksScreen> with WorkflowStepMixin {
  @override
  String get draftId => widget.draftId;

  Future<void> _updateHazard(HazardDraftModel hazard, {String? riskLevelId}) async {
    final currentDraft = ref.read(assessmentWorkflowProvider);
    if (currentDraft == null) return;

    final updated = currentDraft.hazards.map((h) {
      if (h.id != hazard.id) return h;
      return h.copyWith(riskLevelId: riskLevelId);
    }).toList();

    await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
          currentDraft.copyWith(hazards: updated),
        );
  }

  bool _allHazardsAssessed(List<HazardDraftModel> hazards, List<RiskLevelModel> levels) {
    if (hazards.isEmpty) return false;
    return hazards.every(
      (h) => resolveRiskLevelDropdownValue(h.riskLevelId, levels) != null,
    );
  }

  Future<void> _continue() async {
    final draft = ref.read(assessmentWorkflowProvider);
    final validation = ref.read(assessmentValidationServiceProvider).validateAssessRisks(draft);
    if (!validation.isValid) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(validation.message ?? 'Complete risk assessment.')),
        );
      }
      return;
    }

    if (!mounted) return;
    await ref.read(assessmentWorkflowProvider.notifier).nextStep();
    await ref.read(draftAutoSaveServiceProvider).saveDraft(
          showSnackBar: true,
          context: context,
        );
    if (!mounted) return;
    context.go(WorkflowRoutes.path(widget.draftId, 4));
  }

  @override
  Widget build(BuildContext context) {
    final draft = ref.watch(assessmentWorkflowProvider);
    final riskLevelsAsync = ref.watch(riskLevelsProvider);
    final hazards = draft?.hazards ?? const <HazardDraftModel>[];

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text('Step 4: Assess risk level for each selected hazard.'),
          const SizedBox(height: 16),
          Expanded(
            child: riskLevelsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (e, _) => ErrorView(
                message: dioErrorMessage(e),
                onRetry: () => ref.invalidate(riskLevelsProvider),
              ),
              data: (levels) {
                if (hazards.isEmpty) {
                  return const Center(
                    child: Text(
                      'No hazards selected. Go back to Identify Hazards and select at least one.',
                      textAlign: TextAlign.center,
                    ),
                  );
                }

                if (levels.isEmpty) {
                  return const Center(child: Text('No risk levels available.'));
                }

                return ListView.separated(
                  itemCount: hazards.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 8),
                  itemBuilder: (context, index) {
                    final hazard = hazards[index];
                    return Card(
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              hazard.description,
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            if (hazard.consequence.isNotEmpty) ...[
                              const SizedBox(height: 4),
                              Text(
                                hazard.consequence,
                                style: TextStyle(
                                  fontSize: 13,
                                  color: Colors.grey.shade700,
                                  fontStyle: FontStyle.italic,
                                ),
                              ),
                            ],
                            const SizedBox(height: 12),
                            RiskLevelDropdownField(
                              label: 'Risk level',
                              levels: levels,
                              selectedId: hazard.riskLevelId,
                              onChanged: (value) => _updateHazard(hazard, riskLevelId: value),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                );
              },
            ),
          ),
          ElevatedButton(
            onPressed: riskLevelsAsync.maybeWhen(
              data: (levels) =>
                  draft != null && _allHazardsAssessed(hazards, levels) ? _continue : null,
              orElse: () => null,
            ),
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }
}
