import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../profile/data/models/employee_profile_model.dart';
import '../../data/models/jsa_models.dart';
import '../models/hazard_catalog_models.dart';
import '../../presentation/providers/identify_hazards_provider.dart';
import '../../presentation/providers/job_information_provider.dart';
import '../../presentation/providers/quick_assessment_provider.dart';
import '../validators/identify_hazards_validator.dart';
class StepValidationResult {
  const StepValidationResult({required this.isValid, this.message});

  final bool isValid;
  final String? message;
}

/// Central validation for each QJSA workflow step before navigation.
class AssessmentValidationService {
  StepValidationResult validateQuickAssessment(QuickAssessmentFormState state) {
    final result = state.validation;
    if (!result.isValid) {
      final message = result.errors.isNotEmpty
          ? result.errors.values.first
          : 'Please answer all quick assessment questions.';
      return StepValidationResult(isValid: false, message: message);
    }
    return const StepValidationResult(isValid: true);
  }

  StepValidationResult validateJobInformation(JobInformationFormState state) {
    if (state.workLocationId == null || state.workLocationId!.isEmpty) {
      return const StepValidationResult(
        isValid: false,
        message: 'Location is required.',
      );
    }
    if (state.workSectionId == null || state.workSectionId!.isEmpty) {
      return const StepValidationResult(
        isValid: false,
        message: 'Section is required.',
      );
    }
    return const StepValidationResult(isValid: true);
  }

  StepValidationResult validateIdentifyHazards(IdentifyHazardsFormState state) {
    final base = IdentifyHazardsValidator.validate(state.selections);
    if (!base.isValid) {
      return StepValidationResult(isValid: false, message: base.message);
    }

    for (final entry in state.selections.values.where((e) => e.isSelected)) {
      if (entry.isOther) {
        if (entry.customConsequence.trim().isEmpty) {
          return const StepValidationResult(
            isValid: false,
            message: 'Other hazards must include what can go wrong (2.1).',
          );
        }
        continue;
      }
      CatalogHazard? catalog;
      for (final c in state.catalog) {
        if (c.id == entry.catalogHazardId) {
          catalog = c;
          break;
        }
      }
      if (catalog != null && catalog.consequenceDescription.trim().isEmpty) {
        return const StepValidationResult(
          isValid: false,
          message: 'Every selected hazard must have a consequence (2.1).',
        );
      }
    }

    return const StepValidationResult(isValid: true);
  }

  StepValidationResult validateAssessRisks(AssessmentDraftModel? draft) {
    if (draft == null || draft.hazards.isEmpty) {
      return const StepValidationResult(
        isValid: false,
        message: 'Select hazards and assign a risk level for each.',
      );
    }
    if (draft.hazards.any((h) => h.riskLevelId == null)) {
      return const StepValidationResult(
        isValid: false,
        message: 'Assign a risk level to every selected hazard.',
      );
    }
    return const StepValidationResult(isValid: true);
  }

  StepValidationResult validateSignOff({
    required bool hasSignature,
    EmployeeProfileModel? profile,
  }) {
    if (!hasSignature) {
      return const StepValidationResult(
        isValid: false,
        message: 'Signature is required before you can submit.',
      );
    }
    if (profile != null && !profile.isComplete) {
      return const StepValidationResult(
        isValid: false,
        message: 'Complete your employee profile before signing off.',
      );
    }
    return const StepValidationResult(isValid: true);
  }

  StepValidationResult validateStep({
    required int stepIndex,
    required Ref ref,
    required AssessmentDraftModel? draft,
    required bool hasSignature,
    EmployeeProfileModel? profile,
  }) {
    switch (stepIndex) {
      case 0:
        return validateJobInformation(ref.read(jobInformationProvider));
      case 1:
        return validateQuickAssessment(ref.read(quickAssessmentProvider));
      case 2:
        return validateIdentifyHazards(ref.read(identifyHazardsProvider));
      case 3:
        return validateAssessRisks(draft);
      case 4:
        return validateSignOff(hasSignature: hasSignature, profile: profile);
      default:
        return const StepValidationResult(isValid: true);
    }
  }
}

final assessmentValidationServiceProvider = Provider<AssessmentValidationService>(
  (ref) => AssessmentValidationService(),
);
