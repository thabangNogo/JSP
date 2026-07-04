/// QJSA Step 1 — Quick Assessment models.

enum YesNoAnswer {
  yes,
  no;

  bool get isYes => this == YesNoAnswer.yes;
  bool get isNo => this == YesNoAnswer.no;
}

enum NoActionTaken {
  fixIssue,
  callSupervisor,
  other;

  String get label => switch (this) {
        NoActionTaken.fixIssue => 'Fix Issue',
        NoActionTaken.callSupervisor => 'Call Supervisor',
        NoActionTaken.other => 'Other',
      };

  static NoActionTaken? fromString(String? value) {
    if (value == null) return null;
    for (final action in NoActionTaken.values) {
      if (action.name == value) return action;
    }
    return null;
  }
}

/// Single yes/no question with optional action when answered No.
class QuickAssessmentQuestionResponse {
  const QuickAssessmentQuestionResponse({
    this.answer,
    this.actionTaken,
    this.otherComment = '',
  });

  factory QuickAssessmentQuestionResponse.fromJson(Map<String, dynamic> json) =>
      QuickAssessmentQuestionResponse(
        answer: json['answer'] == null
            ? null
            : (json['answer'] as bool
                ? YesNoAnswer.yes
                : YesNoAnswer.no),
        actionTaken: NoActionTaken.fromString(json['actionTaken'] as String?),
        otherComment: json['otherComment'] as String? ?? '',
      );

  final YesNoAnswer? answer;
  final NoActionTaken? actionTaken;
  final String otherComment;

  bool get isAnswered => answer != null;
  bool get requiresAction => answer == YesNoAnswer.no;

  Map<String, dynamic> toJson() => {
        if (answer != null) 'answer': answer!.isYes,
        if (actionTaken != null) 'actionTaken': actionTaken!.name,
        if (otherComment.isNotEmpty) 'otherComment': otherComment,
      };

  QuickAssessmentQuestionResponse copyWith({
    YesNoAnswer? answer,
    NoActionTaken? actionTaken,
    String? otherComment,
    bool clearAnswer = false,
    bool clearAction = false,
  }) {
    return QuickAssessmentQuestionResponse(
      answer: clearAnswer ? null : (answer ?? this.answer),
      actionTaken: clearAction ? null : (actionTaken ?? this.actionTaken),
      otherComment: otherComment ?? this.otherComment,
    );
  }
}

/// QJSA booklet Step 1 — both gate questions.
class QuickAssessmentModel {
  const QuickAssessmentModel({
    this.question1 = const QuickAssessmentQuestionResponse(),
    this.question2 = const QuickAssessmentQuestionResponse(),
  });

  factory QuickAssessmentModel.fromJson(Map<String, dynamic> json) =>
      QuickAssessmentModel(
        question1: json['question1'] != null
            ? QuickAssessmentQuestionResponse.fromJson(
                Map<String, dynamic>.from(json['question1'] as Map),
              )
            : const QuickAssessmentQuestionResponse(),
        question2: json['question2'] != null
            ? QuickAssessmentQuestionResponse.fromJson(
                Map<String, dynamic>.from(json['question2'] as Map),
              )
            : const QuickAssessmentQuestionResponse(),
      );

  static const question1Text =
      'Am I trained, competent and authorized to perform the task?';

  static const question2Text =
      'Do I have the tools, equipment and work permit to perform the task safely?';

  final QuickAssessmentQuestionResponse question1;
  final QuickAssessmentQuestionResponse question2;

  bool get hasAnyNo =>
      question1.answer == YesNoAnswer.no || question2.answer == YesNoAnswer.no;

  Map<String, dynamic> toJson() => {
        'question1': question1.toJson(),
        'question2': question2.toJson(),
      };

  QuickAssessmentModel copyWith({
    QuickAssessmentQuestionResponse? question1,
    QuickAssessmentQuestionResponse? question2,
  }) {
    return QuickAssessmentModel(
      question1: question1 ?? this.question1,
      question2: question2 ?? this.question2,
    );
  }

  String toSummaryText() {
    final buffer = StringBuffer('QJSA Quick Assessment\n');
    buffer.writeln('Q1: $question1Text');
    buffer.writeln('  Answer: ${_formatAnswer(question1)}');
    if (question1.requiresAction) {
      buffer.writeln('  Action: ${question1.actionTaken?.label ?? "—"}');
      if (question1.actionTaken == NoActionTaken.other) {
        buffer.writeln('  Comment: ${question1.otherComment}');
      }
    }
    buffer.writeln('Q2: $question2Text');
    buffer.writeln('  Answer: ${_formatAnswer(question2)}');
    if (question2.requiresAction) {
      buffer.writeln('  Action: ${question2.actionTaken?.label ?? "—"}');
      if (question2.actionTaken == NoActionTaken.other) {
        buffer.writeln('  Comment: ${question2.otherComment}');
      }
    }
    return buffer.toString();
  }

  static String _formatAnswer(QuickAssessmentQuestionResponse q) {
    if (q.answer == null) return 'Not answered';
    return q.answer!.isYes ? 'Yes' : 'No';
  }
}
