import '../../data/models/jsa_models.dart';
import '../../../../shared/enums/assessment_status.dart';

/// Plain-text assessment summary for on-screen review and future PDF export.
class AssessmentSummaryBuilder {
  static String build(AssessmentDetailModel detail) {
    final buffer = StringBuffer()
      ..writeln('Job Safety Assessment')
      ..writeln('====================')
      ..writeln('Title: ${detail.title}')
      ..writeln('Status: ${_statusLabel(detail.status)}')
      ..writeln('Last updated: ${detail.lastUpdated?.toLocal() ?? '—'}')
      ..writeln()
      ..writeln('Job information')
      ..writeln('Department: ${detail.department}')
      ..writeln('Location: ${detail.location}')
      ..writeln('Section: ${detail.section}')
      ..writeln();

    if (detail.jobDescription.isNotEmpty) {
      buffer
        ..writeln('Job description')
        ..writeln(detail.jobDescription)
        ..writeln();
    }

    if (detail.quickAssessmentSummary != null &&
        detail.quickAssessmentSummary!.trim().isNotEmpty) {
      buffer
        ..writeln('Quick assessment')
        ..writeln(detail.quickAssessmentSummary)
        ..writeln();
    }

    buffer
      ..writeln('Sign-off')
      ..writeln('Name: ${detail.signOffName ?? '—'}')
      ..writeln('Surname: ${detail.signOffSurname ?? '—'}')
      ..writeln('Company number: ${detail.signOffCompanyNumber ?? '—'}')
      ..writeln('Occupation: ${detail.signOffOccupation ?? '—'}')
      ..writeln()
      ..writeln('Hazards: ${detail.hazards.length}')
      ..writeln('Controls: ${detail.controls.length}');

    return buffer.toString();
  }

  static String _statusLabel(AssessmentStatus status) {
    return switch (status) {
      AssessmentStatus.draft => 'Draft',
      AssessmentStatus.submitted => 'Submitted',
      AssessmentStatus.approved => 'Approved',
      AssessmentStatus.rejected => 'Rejected',
    };
  }
}
