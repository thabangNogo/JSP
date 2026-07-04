import 'package:uuid/uuid.dart';
import '../../data/catalog/qjsa_hazard_catalog.dart';
import '../../data/models/jsa_models.dart';
import '../models/hazard_catalog_models.dart';

/// Maps between catalog selections and draft persistence models.
class HazardSelectionMapper {
  const HazardSelectionMapper._();

  static const _uuid = Uuid();

  static Map<String, SelectedHazardEntry> entriesFromDraft(AssessmentDraftModel draft) {
    final catalog = QjsaHazardCatalog.byIdMap;
    final entries = <String, SelectedHazardEntry>{};

    for (final catalogHazard in QjsaHazardCatalog.hazards) {
      entries[catalogHazard.id] = SelectedHazardEntry(
        catalogHazardId: catalogHazard.id,
        isSelected: false,
        selectedControlIds: {},
      );
    }

    for (final hazard in draft.hazards) {
      final catalogId = hazard.catalogHazardId ?? _matchCatalogId(hazard.description, catalog);
      if (catalogId == null) continue;

      if (catalogId == SelectedHazardEntry.otherCatalogId) {
        final controlText = draft.controls
            .where((c) => c.hazardId == hazard.id)
            .map((c) => c.description)
            .join('\n');
        entries[catalogId] = SelectedHazardEntry(
          catalogHazardId: catalogId,
          isSelected: true,
          customDescription: hazard.description,
          customConsequence: hazard.consequence,
          customControlsText: controlText,
          photoPaths: hazard.photoPaths,
        );
        continue;
      }

      final controlIds = draft.controls
          .where((c) => c.hazardId == hazard.id)
          .map((c) => c.catalogControlId)
          .whereType<String>()
          .toSet();

      entries[catalogId] = SelectedHazardEntry(
        catalogHazardId: catalogId,
        isSelected: true,
        selectedControlIds: controlIds,
        photoPaths: hazard.photoPaths,
      );
    }

    return entries;
  }

  static const _legacyNameToId = <String, String>{
    'slips, trips and falls': 'uneven-ground',
    'contact with moving machinery': 'moving-machinery',
    'manual handling / lifting': 'poor-stacking-storage',
    'hot surfaces / burns': 'oil-spills',
    'chemical exposure': 'oil-spills',
    'noise exposure': 'dust-noise',
    'falling objects': 'falling-objects',
    'electrical hazards': 'electrical-cables',
  };

  static String? _matchCatalogId(String description, Map<String, CatalogHazard> catalog) {
    final normalized = description.trim().toLowerCase();
    final legacyId = _legacyNameToId[normalized];
    if (legacyId != null && catalog.containsKey(legacyId)) {
      return legacyId;
    }
    for (final entry in catalog.entries) {
      if (entry.value.name.toLowerCase() == normalized) {
        return entry.key;
      }
    }
    return null;
  }

  static List<String> _splitCustomControls(String text) {
    return text
        .split(RegExp(r'[\n;]+'))
        .map((s) => s.trim())
        .where((s) => s.isNotEmpty)
        .toList();
  }

  static ({List<HazardDraftModel> hazards, List<ControlDraftModel> controls}) toDraftModels(
    Map<String, SelectedHazardEntry> selections,
  ) {
    final hazards = <HazardDraftModel>[];
    final controls = <ControlDraftModel>[];

    for (final entry in selections.values) {
      if (!entry.isSelected) continue;

      final hazardInstanceId = _uuid.v4();

      if (entry.isOther) {
        hazards.add(
          HazardDraftModel(
            id: hazardInstanceId,
            catalogHazardId: SelectedHazardEntry.otherCatalogId,
            description: entry.customDescription.trim(),
            consequence: entry.customConsequence.trim(),
            photoPaths: entry.photoPaths,
          ),
        );
        var controlIndex = 0;
        for (final line in _splitCustomControls(entry.customControlsText)) {
          controls.add(
            ControlDraftModel(
              id: '${hazardInstanceId}_custom_${controlIndex++}',
              description: line,
              hierarchyOfControl: 'Administrative',
              hazardId: hazardInstanceId,
              isImplemented: true,
            ),
          );
        }
        continue;
      }

      final catalogHazard = QjsaHazardCatalog.byId(entry.catalogHazardId);
      if (catalogHazard == null) continue;

      hazards.add(
        HazardDraftModel(
          id: hazardInstanceId,
          catalogHazardId: catalogHazard.id,
          description: catalogHazard.name,
          consequence: catalogHazard.consequenceDescription,
          selectedControlIds: entry.selectedControlIds.toList(),
          photoPaths: entry.photoPaths,
        ),
      );

      for (final control in catalogHazard.controls) {
        if (!entry.selectedControlIds.contains(control.id)) continue;
        controls.add(
          ControlDraftModel(
            id: '${hazardInstanceId}_${control.id}',
            catalogControlId: control.id,
            description: control.description,
            hierarchyOfControl: control.hierarchyOfControl,
            hazardId: hazardInstanceId,
            isImplemented: true,
          ),
        );
      }
    }

    return (hazards: hazards, controls: controls);
  }

  /// Rebuilds hazards/controls from selections while keeping stable ids and risk
  /// levels from [existingHazards] (auto-save must not wipe Assess Risks data).
  static ({List<HazardDraftModel> hazards, List<ControlDraftModel> controls})
      toDraftModelsPreservingExisting(
    Map<String, SelectedHazardEntry> selections,
    List<HazardDraftModel> existingHazards,
  ) {
    final mapped = toDraftModels(selections);
    if (existingHazards.isEmpty) {
      return mapped;
    }

    final idRemap = <String, String>{};
    final hazards = <HazardDraftModel>[];

    for (final hazard in mapped.hazards) {
      final previous = _findMatchingHazard(hazard, existingHazards);
      if (previous != null) {
        idRemap[hazard.id] = previous.id;
        hazards.add(
          hazard.copyWith(
            id: previous.id,
            riskLevelId: previous.riskLevelId,
            residualRiskLevelId: previous.residualRiskLevelId,
          ),
        );
      } else {
        hazards.add(hazard);
      }
    }

    final controls = mapped.controls.map((control) {
      final remappedHazardId = idRemap[control.hazardId];
      if (remappedHazardId == null) return control;
      return ControlDraftModel(
        id: control.id,
        description: control.description,
        hierarchyOfControl: control.hierarchyOfControl,
        catalogControlId: control.catalogControlId,
        hazardId: remappedHazardId,
        isImplemented: control.isImplemented,
      );
    }).toList();

    return (hazards: hazards, controls: controls);
  }

  static HazardDraftModel? _findMatchingHazard(
    HazardDraftModel hazard,
    List<HazardDraftModel> existingHazards,
  ) {
    for (final existing in existingHazards) {
      if (hazard.catalogHazardId != null &&
          existing.catalogHazardId == hazard.catalogHazardId) {
        return existing;
      }
    }
    return null;
  }
}
