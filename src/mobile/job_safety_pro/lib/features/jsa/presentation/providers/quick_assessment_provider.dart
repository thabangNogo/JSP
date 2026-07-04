import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../domain/models/quick_assessment_models.dart';
import '../../domain/validators/quick_assessment_validator.dart';
import 'jsa_providers.dart';

class QuickAssessmentFormState {
  const QuickAssessmentFormState({
    required this.assessment,
    this.showValidation = false,
    this.isHydrated = false,
  });

  final QuickAssessmentModel assessment;
  final bool showValidation;
  final bool isHydrated;

  QuickAssessmentValidationResult get validation =>
      QuickAssessmentValidator.validate(assessment);

  bool get canContinue => validation.isValid;

  QuickAssessmentFormState copyWith({
    QuickAssessmentModel? assessment,
    bool? showValidation,
    bool? isHydrated,
  }) {
    return QuickAssessmentFormState(
      assessment: assessment ?? this.assessment,
      showValidation: showValidation ?? this.showValidation,
      isHydrated: isHydrated ?? this.isHydrated,
    );
  }
}

class QuickAssessmentNotifier extends Notifier<QuickAssessmentFormState> {
  @override
  QuickAssessmentFormState build() {
    // Do not watch assessmentWorkflowProvider — rebuilds wipe in-progress answers.
    return const QuickAssessmentFormState(
      assessment: QuickAssessmentModel(),
      isHydrated: false,
    );
  }

  void hydrateFromDraft(QuickAssessmentModel quickAssessment, {bool force = false}) {
    if (!force && state.isHydrated) return;
    if (!force &&
        (state.assessment.question1.isAnswered || state.assessment.question2.isAnswered)) {
      return;
    }
    state = QuickAssessmentFormState(
      assessment: quickAssessment,
      isHydrated: true,
      showValidation: false,
    );
  }

  void setQuestion1Answer(YesNoAnswer answer) {
    var q1 = state.assessment.question1.copyWith(answer: answer);
    if (answer.isYes) {
      q1 = q1.copyWith(clearAction: true, otherComment: '');
    }
    state = state.copyWith(
      assessment: state.assessment.copyWith(question1: q1),
      isHydrated: true,
    );
  }

  void setQuestion2Answer(YesNoAnswer answer) {
    var q2 = state.assessment.question2.copyWith(answer: answer);
    if (answer.isYes) {
      q2 = q2.copyWith(clearAction: true, otherComment: '');
    }
    state = state.copyWith(
      assessment: state.assessment.copyWith(question2: q2),
      isHydrated: true,
    );
  }

  void setQuestion1Action(NoActionTaken action) {
    final q1 = state.assessment.question1.copyWith(
      actionTaken: action,
      otherComment:
          action == NoActionTaken.other ? state.assessment.question1.otherComment : '',
    );
    state = state.copyWith(
      assessment: state.assessment.copyWith(question1: q1),
    );
  }

  void setQuestion2Action(NoActionTaken action) {
    final q2 = state.assessment.question2.copyWith(
      actionTaken: action,
      otherComment:
          action == NoActionTaken.other ? state.assessment.question2.otherComment : '',
    );
    state = state.copyWith(
      assessment: state.assessment.copyWith(question2: q2),
    );
  }

  void setQuestion1OtherComment(String comment) {
    state = state.copyWith(
      assessment: state.assessment.copyWith(
        question1: state.assessment.question1.copyWith(otherComment: comment),
      ),
    );
  }

  void setQuestion2OtherComment(String comment) {
    state = state.copyWith(
      assessment: state.assessment.copyWith(
        question2: state.assessment.question2.copyWith(otherComment: comment),
      ),
    );
  }

  bool validateAndShowErrors() {
    final result = state.validation;
    if (!result.isValid) {
      state = state.copyWith(showValidation: true);
      return false;
    }
    return true;
  }

  Future<bool> saveToDraft(String localDraftId) async {
    if (!validateAndShowErrors()) return false;

    var draft = ref.read(assessmentWorkflowProvider);
    if (draft == null) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(localDraftId);
      draft = ref.read(assessmentWorkflowProvider);
    }
    if (draft == null) return false;

    await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
          draft.copyWith(quickAssessment: state.assessment),
        );
    return true;
  }
}

final quickAssessmentProvider =
    NotifierProvider<QuickAssessmentNotifier, QuickAssessmentFormState>(
  QuickAssessmentNotifier.new,
);
