import '../models/quick_assessment_models.dart';

class QuickAssessmentValidationResult {
  const QuickAssessmentValidationResult({
    required this.isValid,
    this.errors = const {},
  });

  final bool isValid;
  final Map<String, String> errors;

  String? errorFor(String key) => errors[key];
}

class QuickAssessmentValidator {
  static const question1Key = 'question1';
  static const question2Key = 'question2';
  static const question1ActionKey = 'question1Action';
  static const question2ActionKey = 'question2Action';
  static const question1OtherKey = 'question1Other';
  static const question2OtherKey = 'question2Other';

  static QuickAssessmentValidationResult validate(QuickAssessmentModel model) {
    final errors = <String, String>{};

    _validateQuestion(
      model.question1,
      answerKey: question1Key,
      actionKey: question1ActionKey,
      otherKey: question1OtherKey,
      errors: errors,
    );

    _validateQuestion(
      model.question2,
      answerKey: question2Key,
      actionKey: question2ActionKey,
      otherKey: question2OtherKey,
      errors: errors,
    );

    return QuickAssessmentValidationResult(
      isValid: errors.isEmpty,
      errors: errors,
    );
  }

  static void _validateQuestion(
    QuickAssessmentQuestionResponse question, {
    required String answerKey,
    required String actionKey,
    required String otherKey,
    required Map<String, String> errors,
  }) {
    if (!question.isAnswered) {
      errors[answerKey] = 'Please select Yes or No';
      return;
    }

    if (!question.requiresAction) return;

    if (question.actionTaken == null) {
      errors[actionKey] = 'Action taken is required when you answer No';
      return;
    }

    if (question.actionTaken == NoActionTaken.other &&
        question.otherComment.trim().isEmpty) {
      errors[otherKey] = 'Please describe the action taken';
    }
  }
}
