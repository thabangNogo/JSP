import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'identify_hazards_provider.dart';
import 'job_information_provider.dart';
import 'jsa_providers.dart';
import 'quick_assessment_provider.dart';

/// Clears in-memory workflow step forms so a new or resumed draft starts fresh.
void resetWorkflowFormProviders(WidgetRef ref) {
  ref.invalidate(quickAssessmentProvider);
  ref.invalidate(jobInformationProvider);
  ref.invalidate(identifyHazardsProvider);
  ref.read(assessmentWorkflowProvider.notifier).clearSession();
}
