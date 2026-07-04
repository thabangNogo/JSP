import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/catalog/qjsa_hazard_catalog.dart';
import '../../domain/models/hazard_catalog_models.dart';
import '../../domain/services/hazard_selection_mapper.dart';
import '../../domain/validators/identify_hazards_validator.dart';
import 'jsa_providers.dart';

class IdentifyHazardsFormState {
  const IdentifyHazardsFormState({
    required this.selections,
    this.showValidation = false,
    this.isHydrated = false,
  });

  final Map<String, SelectedHazardEntry> selections;
  final bool showValidation;
  final bool isHydrated;

  List<CatalogHazard> get catalog => QjsaHazardCatalog.hazards;

  int get selectedCount => selections.values.where((e) => e.isSelected).length;

  IdentifyHazardsValidationResult get validation =>
      IdentifyHazardsValidator.validate(selections);

  bool get canContinue => validation.isValid;

  IdentifyHazardsFormState copyWith({
    Map<String, SelectedHazardEntry>? selections,
    bool? showValidation,
    bool? isHydrated,
  }) {
    return IdentifyHazardsFormState(
      selections: selections ?? this.selections,
      showValidation: showValidation ?? this.showValidation,
      isHydrated: isHydrated ?? this.isHydrated,
    );
  }
}

class IdentifyHazardsNotifier extends Notifier<IdentifyHazardsFormState> {
  @override
  IdentifyHazardsFormState build() {
    return IdentifyHazardsFormState(
      selections: _emptySelections(),
      isHydrated: false,
    );
  }

  Map<String, SelectedHazardEntry> _emptySelections() {
    return {
      for (final hazard in QjsaHazardCatalog.hazards)
        hazard.id: SelectedHazardEntry(
          catalogHazardId: hazard.id,
          isSelected: false,
          selectedControlIds: {},
        ),
    };
  }

  void _updateEntry(String catalogHazardId, SelectedHazardEntry entry) {
    final updated = Map<String, SelectedHazardEntry>.from(state.selections);
    updated[catalogHazardId] = entry;
    state = state.copyWith(selections: updated, isHydrated: true);
  }

  void hydrateFromDraft(Map<String, SelectedHazardEntry> entries, {bool force = false}) {
    if (!force && state.isHydrated) return;
    state = IdentifyHazardsFormState(
      selections: entries,
      isHydrated: true,
      showValidation: false,
    );
  }

  void toggleHazard(String catalogHazardId, bool selected) {
    final catalogHazard = QjsaHazardCatalog.byId(catalogHazardId);
    if (catalogHazard == null) return;

    if (catalogHazardId == SelectedHazardEntry.otherCatalogId) {
      _updateEntry(
        catalogHazardId,
        selected
            ? SelectedHazardEntry(catalogHazardId: catalogHazardId, isSelected: true)
            : const SelectedHazardEntry(
                catalogHazardId: SelectedHazardEntry.otherCatalogId,
                isSelected: false,
              ),
      );
      return;
    }

    _updateEntry(
      catalogHazardId,
      SelectedHazardEntry(
        catalogHazardId: catalogHazardId,
        isSelected: selected,
        selectedControlIds: const {},
      ),
    );
  }

  void toggleControl(String catalogHazardId, String controlId, bool selected) {
    final entry = state.selections[catalogHazardId];
    if (entry == null || !entry.isSelected || entry.isOther) return;

    final controlIds = Set<String>.from(entry.selectedControlIds);
    if (selected) {
      controlIds.add(controlId);
    } else {
      controlIds.remove(controlId);
    }

    _updateEntry(catalogHazardId, entry.copyWith(selectedControlIds: controlIds));
  }

  void setCustomDescription(String text) {
    final entry = state.selections[SelectedHazardEntry.otherCatalogId];
    if (entry == null) return;
    _updateEntry(SelectedHazardEntry.otherCatalogId, entry.copyWith(customDescription: text));
  }

  void setCustomConsequence(String text) {
    final entry = state.selections[SelectedHazardEntry.otherCatalogId];
    if (entry == null) return;
    _updateEntry(SelectedHazardEntry.otherCatalogId, entry.copyWith(customConsequence: text));
  }

  void setCustomControlsText(String text) {
    final entry = state.selections[SelectedHazardEntry.otherCatalogId];
    if (entry == null) return;
    _updateEntry(SelectedHazardEntry.otherCatalogId, entry.copyWith(customControlsText: text));
  }

  void addPhoto(String catalogHazardId, String path) {
    final entry = state.selections[catalogHazardId];
    if (entry == null || !entry.isSelected) return;
    _updateEntry(
      catalogHazardId,
      entry.copyWith(photoPaths: [...entry.photoPaths, path]),
    );
  }

  void removePhoto(String catalogHazardId, int index) {
    final entry = state.selections[catalogHazardId];
    if (entry == null) return;
    final paths = List<String>.from(entry.photoPaths)..removeAt(index);
    _updateEntry(catalogHazardId, entry.copyWith(photoPaths: paths));
  }

  bool validateAndShowErrors() {
    if (!state.validation.isValid) {
      state = state.copyWith(showValidation: true);
      return false;
    }
    return true;
  }

  Future<bool> saveToDraft(String localDraftId) async {
    if (!validateAndShowErrors()) return false;

    var draft = ref.read(assessmentWorkflowProvider);
    if (draft == null || draft.localId != localDraftId) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(localDraftId);
      draft = ref.read(assessmentWorkflowProvider);
    }
    if (draft == null) return false;

    final mapped = HazardSelectionMapper.toDraftModelsPreservingExisting(
      state.selections,
      draft.hazards,
    );
    await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
          draft.copyWith(
            hazards: mapped.hazards,
            controls: mapped.controls,
          ),
        );
    return true;
  }
}

final identifyHazardsProvider =
    NotifierProvider<IdentifyHazardsNotifier, IdentifyHazardsFormState>(
  IdentifyHazardsNotifier.new,
);
