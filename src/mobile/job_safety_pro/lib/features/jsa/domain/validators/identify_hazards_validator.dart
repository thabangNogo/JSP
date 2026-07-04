import '../../data/catalog/qjsa_hazard_catalog.dart';
import '../models/hazard_catalog_models.dart';

class IdentifyHazardsValidationResult {
  const IdentifyHazardsValidationResult({
    required this.isValid,
    this.message,
    this.hazardIdWithError,
  });

  final bool isValid;
  final String? message;
  final String? hazardIdWithError;
}

class IdentifyHazardsValidator {
  static IdentifyHazardsValidationResult validate(
    Map<String, SelectedHazardEntry> selections,
  ) {
    final selected =
        selections.values.where((entry) => entry.isSelected).toList();

    if (selected.isEmpty) {
      return const IdentifyHazardsValidationResult(
        isValid: false,
        message: 'Tick at least one hazard before continuing.',
      );
    }

    for (final entry in selected) {
      if (entry.isOther) {
        if (entry.customDescription.trim().isEmpty) {
          return const IdentifyHazardsValidationResult(
            isValid: false,
            hazardIdWithError: SelectedHazardEntry.otherCatalogId,
            message: 'For Other, describe what can cause injury or harm.',
          );
        }
        if (entry.customConsequence.trim().isEmpty) {
          return const IdentifyHazardsValidationResult(
            isValid: false,
            hazardIdWithError: SelectedHazardEntry.otherCatalogId,
            message: 'For Other, describe what can go wrong.',
          );
        }
        if (entry.customControlsText.trim().isEmpty) {
          return const IdentifyHazardsValidationResult(
            isValid: false,
            hazardIdWithError: SelectedHazardEntry.otherCatalogId,
            message: 'For Other, describe what you can do about it (controls).',
          );
        }
        continue;
      }

      final catalogHazard = QjsaHazardCatalog.byId(entry.catalogHazardId);
      if (catalogHazard == null) continue;

      if (catalogHazard.controls.isEmpty) continue;

      if (entry.selectedControlIds.isEmpty) {
        return IdentifyHazardsValidationResult(
          isValid: false,
          hazardIdWithError: entry.catalogHazardId,
          message:
              'For "${catalogHazard.name}", select at least one control that applies.',
        );
      }
    }

    return const IdentifyHazardsValidationResult(isValid: true);
  }
}
