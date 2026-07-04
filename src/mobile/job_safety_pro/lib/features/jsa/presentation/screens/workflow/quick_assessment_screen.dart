import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../data/models/jsa_models.dart';
import '../../../domain/models/quick_assessment_models.dart';
import '../../../domain/validators/quick_assessment_validator.dart';
import '../../../domain/services/assessment_validation_service.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../providers/jsa_providers.dart';
import '../../providers/quick_assessment_provider.dart';
import '../../widgets/glove_friendly/glove_friendly_controls.dart';
import 'workflow_step_mixin.dart';

class QuickAssessmentScreen extends ConsumerStatefulWidget {
  const QuickAssessmentScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<QuickAssessmentScreen> createState() => _QuickAssessmentScreenState();
}

class _QuickAssessmentScreenState extends ConsumerState<QuickAssessmentScreen>
    with WorkflowStepMixin {
  @override
  String get draftId => widget.draftId;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _ensureDraftLoaded());
  }

  Future<void> _ensureDraftLoaded() async {
    final draft = ref.read(assessmentWorkflowProvider);
    if (draft?.localId != widget.draftId) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(widget.draftId);
    }
    final loaded = ref.read(assessmentWorkflowProvider);
    if (loaded != null && mounted) {
      ref.read(quickAssessmentProvider.notifier).hydrateFromDraft(
            loaded.quickAssessment,
            force: true,
          );
    }
  }

  void _showMessage(String message) {
    final messenger = ScaffoldMessenger.maybeOf(context);
    if (messenger != null) {
      messenger.showSnackBar(SnackBar(content: Text(message)));
      return;
    }
    debugPrint('QuickAssessment: $message');
  }

  Future<void> _continue() async {
    try {
      final validation = ref.read(assessmentValidationServiceProvider).validateQuickAssessment(
            ref.read(quickAssessmentProvider),
          );
      if (!validation.isValid) {
        ref.read(quickAssessmentProvider.notifier).validateAndShowErrors();
        _showMessage(validation.message ?? 'Please complete all questions.');
        return;
      }

      final notifier = ref.read(quickAssessmentProvider.notifier);
      final saved = await notifier.saveToDraft(widget.draftId);
      if (!saved) {
        _showMessage('Could not save assessment. Please try again.');
        return;
      }

      if (!mounted) return;
      await ref.read(assessmentWorkflowProvider.notifier).nextStep();
      await ref.read(draftAutoSaveServiceProvider).saveDraft(
            showSnackBar: true,
            context: context,
          );
      if (!mounted) return;
      context.go('/jsas/workflow/${widget.draftId}/hazards');
    } catch (e, st) {
      debugPrint('QuickAssessment continue failed: $e\n$st');
      if (mounted) {
        _showMessage('Something went wrong. Please try again.');
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final formState = ref.watch(quickAssessmentProvider);
    final notifier = ref.read(quickAssessmentProvider.notifier);
    final assessment = formState.assessment;
    final validation = formState.validation;
    final showErrors = formState.showValidation;
    final draft = ref.watch(assessmentWorkflowProvider);

    ref.listen<AssessmentDraftModel?>(assessmentWorkflowProvider, (previous, next) {
      if (next != null && previous == null && mounted) {
        notifier.hydrateFromDraft(next.quickAssessment, force: true);
      }
    });

    if (draft == null) {
      return const Center(child: CircularProgressIndicator());
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Expanded(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(kGloveHorizontalPadding),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Step 2: Quick Assessment',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Answer both questions before continuing. If you answer No, you must record the action taken.',
                  style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                        height: 1.4,
                        color: Colors.grey.shade800,
                      ),
                ),
                const SizedBox(height: kGloveSectionSpacing),
                QuickAssessmentQuestionCard(
                  questionNumber: 1,
                  questionText: QuickAssessmentModel.question1Text,
                  response: assessment.question1,
                  onAnswerChanged: notifier.setQuestion1Answer,
                  onActionChanged: notifier.setQuestion1Action,
                  onOtherCommentChanged: notifier.setQuestion1OtherComment,
                  answerError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question1Key)
                      : null,
                  actionError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question1ActionKey)
                      : null,
                  otherError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question1OtherKey)
                      : null,
                ),
                const SizedBox(height: 16),
                QuickAssessmentQuestionCard(
                  questionNumber: 2,
                  questionText: QuickAssessmentModel.question2Text,
                  response: assessment.question2,
                  onAnswerChanged: notifier.setQuestion2Answer,
                  onActionChanged: notifier.setQuestion2Action,
                  onOtherCommentChanged: notifier.setQuestion2OtherComment,
                  answerError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question2Key)
                      : null,
                  actionError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question2ActionKey)
                      : null,
                  otherError: showErrors
                      ? validation.errorFor(QuickAssessmentValidator.question2OtherKey)
                      : null,
                ),
                if (assessment.hasAnyNo) ...[
                  const SizedBox(height: 16),
                  MaterialBanner(
                    backgroundColor: Colors.orange.shade50,
                    content: const Text(
                      'You answered No to at least one question. Ensure the required action is documented before continuing.',
                      style: TextStyle(fontSize: 16),
                    ),
                    actions: const [SizedBox.shrink()],
                  ),
                ],
              ],
            ),
          ),
        ),
        Padding(
          padding: const EdgeInsets.all(kGloveHorizontalPadding),
          child: SizedBox(
            height: kGloveMinTouchHeight,
            child: ElevatedButton(
              onPressed: _continue,
              style: ElevatedButton.styleFrom(
                textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
              ),
              child: const Text('Continue'),
            ),
          ),
        ),
      ],
    );
  }
}
