enum AssessmentStatus {
  draft(1),
  submitted(2),
  approved(3),
  rejected(4);

  const AssessmentStatus(this.value);
  final int value;

  static AssessmentStatus fromValue(int? value) {
    return AssessmentStatus.values.firstWhere(
      (s) => s.value == value,
      orElse: () => AssessmentStatus.draft,
    );
  }

  static AssessmentStatus fromName(String? name) {
    if (name == null) return AssessmentStatus.draft;
    final normalized = name.replaceAll(' ', '').toLowerCase();
    if (normalized == 'inreview') return AssessmentStatus.submitted;
    return AssessmentStatus.values.firstWhere(
      (s) => s.name.toLowerCase() == normalized,
      orElse: () => AssessmentStatus.draft,
    );
  }

  /// Parses API (`"Submitted"`), Hive (`int`), or enum name values.
  static AssessmentStatus parse(dynamic value) {
    if (value is int) return fromValue(value);
    if (value is String) return fromName(value);
    return AssessmentStatus.draft;
  }

  bool get isDraft => this == AssessmentStatus.draft;
  bool get canDelete => isDraft;
}
