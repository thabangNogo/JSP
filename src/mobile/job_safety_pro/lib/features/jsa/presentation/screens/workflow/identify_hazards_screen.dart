import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../data/catalog/qjsa_hazard_catalog.dart';
import '../../../domain/models/hazard_catalog_models.dart';
import '../../../domain/services/assessment_validation_service.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../../domain/services/hazard_selection_mapper.dart';
import '../../providers/identify_hazards_provider.dart';
import '../../providers/jsa_providers.dart';
import '../../widgets/glove_friendly/glove_friendly_controls.dart';
import '../../widgets/other_hazard_card.dart';
import 'workflow_step_mixin.dart';

class IdentifyHazardsScreen extends ConsumerStatefulWidget {
  const IdentifyHazardsScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<IdentifyHazardsScreen> createState() => _IdentifyHazardsScreenState();
}

class _IdentifyHazardsScreenState extends ConsumerState<IdentifyHazardsScreen>
    with WorkflowStepMixin {
  @override
  String get draftId => widget.draftId;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _ensureDraftLoaded());
  }

  Future<void> _ensureDraftLoaded() async {
    final draft = ref.read(assessmentWorkflowProvider);
    if (draft?.localId != widget.draftId) {
      await ref.read(assessmentWorkflowProvider.notifier).loadDraft(widget.draftId);
    }
    final loaded = ref.read(assessmentWorkflowProvider);
    if (loaded != null && mounted) {
      ref.read(identifyHazardsProvider.notifier).hydrateFromDraft(
            HazardSelectionMapper.entriesFromDraft(loaded),
            force: true,
          );
    }
  }

  void _showMessage(String message) {
    final messenger = ScaffoldMessenger.maybeOf(context);
    if (messenger != null) {
      messenger.showSnackBar(SnackBar(content: Text(message)));
    }
  }

  Future<void> _addOtherPhoto() async {
    final source = await showModalBottomSheet<ImageSource>(
      context: context,
      builder: (ctx) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.camera_alt),
              title: const Text('Camera'),
              onTap: () => Navigator.pop(ctx, ImageSource.camera),
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Gallery'),
              onTap: () => Navigator.pop(ctx, ImageSource.gallery),
            ),
          ],
        ),
      ),
    );
    if (source == null || !mounted) return;

    final photoService = ref.read(photoCaptureServiceProvider);
    final path = source == ImageSource.camera
        ? await photoService.captureFromCamera()
        : await photoService.pickFromGallery();
    if (path != null && mounted) {
      ref
          .read(identifyHazardsProvider.notifier)
          .addPhoto(SelectedHazardEntry.otherCatalogId, path);
    }
  }

  Future<void> _continue() async {
    final stepValidation = ref.read(assessmentValidationServiceProvider).validateIdentifyHazards(
          ref.read(identifyHazardsProvider),
        );
    if (!stepValidation.isValid) {
      ref.read(identifyHazardsProvider.notifier).validateAndShowErrors();
      _showMessage(stepValidation.message ?? 'Complete all hazards and controls.');
      return;
    }

    final notifier = ref.read(identifyHazardsProvider.notifier);
    final saved = await notifier.saveToDraft(widget.draftId);
    if (!saved) {
      if (mounted) {
        _showMessage('Could not save hazards. Please try again.');
      }
      return;
    }

    if (!mounted) return;
    await ref.read(assessmentWorkflowProvider.notifier).nextStep();
    await ref.read(draftAutoSaveServiceProvider).saveDraft(
          showSnackBar: true,
          context: context,
        );
    if (!mounted) return;
    context.go('/jsas/workflow/${widget.draftId}/risks');
  }

  @override
  Widget build(BuildContext context) {
    final formState = ref.watch(identifyHazardsProvider);
    final notifier = ref.read(identifyHazardsProvider.notifier);
    final theme = Theme.of(context);
    final draft = ref.watch(assessmentWorkflowProvider);

    ref.listen(assessmentWorkflowProvider, (previous, next) {
      if (next != null && previous == null && mounted) {
        notifier.hydrateFromDraft(
          HazardSelectionMapper.entriesFromDraft(next),
          force: true,
        );
      }
    });

    if (draft == null) {
      return const Center(child: CircularProgressIndicator());
    }

    final errorHazardId = formState.showValidation
        ? formState.validation.hazardIdWithError
        : null;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Expanded(
          child: ListView(
            padding: const EdgeInsets.all(kGloveHorizontalPadding),
            children: [
              Text(
                'Step 3: Identify Hazards',
                style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(
                'What can cause injury/harm/damage? Tick each hazard that applies. '
                'For each hazard, review what can go wrong (2.1) and tick every control (2.2).',
                style: theme.textTheme.bodyLarge?.copyWith(
                  height: 1.4,
                  color: Colors.grey.shade800,
                ),
              ),
              if (formState.showValidation && !formState.validation.isValid) ...[
                const SizedBox(height: 12),
                MaterialBanner(
                  backgroundColor: theme.colorScheme.errorContainer,
                  content: Text(
                    formState.validation.message ?? 'Please complete required fields.',
                    style: TextStyle(
                      color: theme.colorScheme.onErrorContainer,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  actions: const [SizedBox.shrink()],
                ),
              ],
              const SizedBox(height: 8),
              Text(
                '${formState.selectedCount} of ${QjsaHazardCatalog.hazards.length} hazards selected',
                style: theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.w600,
                  color: theme.colorScheme.primary,
                ),
              ),
              const SizedBox(height: 12),
              ...formState.catalog.map((catalogHazard) {
                final entry = formState.selections[catalogHazard.id]!;
                if (catalogHazard.id == SelectedHazardEntry.otherCatalogId) {
                  return OtherHazardCard(
                    entry: entry,
                    highlightError: errorHazardId == catalogHazard.id,
                    onSelectedChanged: (selected) =>
                        notifier.toggleHazard(catalogHazard.id, selected),
                    onDescriptionChanged: notifier.setCustomDescription,
                    onConsequenceChanged: notifier.setCustomConsequence,
                    onControlsChanged: notifier.setCustomControlsText,
                    onAddPhoto: _addOtherPhoto,
                    onRemovePhoto: (index) =>
                        notifier.removePhoto(catalogHazard.id, index),
                  );
                }
                return _HazardCheckboxCard(
                  catalogHazard: catalogHazard,
                  entry: entry,
                  highlightError: errorHazardId == catalogHazard.id,
                  onHazardChanged: (selected) =>
                      notifier.toggleHazard(catalogHazard.id, selected),
                  onControlChanged: (controlId, selected) =>
                      notifier.toggleControl(catalogHazard.id, controlId, selected),
                );
              }),
            ],
          ),
        ),
        Padding(
          padding: const EdgeInsets.all(kGloveHorizontalPadding),
          child: SizedBox(
            height: kGloveMinTouchHeight,
            child: ElevatedButton(
              onPressed: _continue,
              style: ElevatedButton.styleFrom(
                textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
              ),
              child: const Text('Continue'),
            ),
          ),
        ),
      ],
    );
  }
}

class _HazardCheckboxCard extends StatelessWidget {
  const _HazardCheckboxCard({
    required this.catalogHazard,
    required this.entry,
    required this.onHazardChanged,
    required this.onControlChanged,
    this.highlightError = false,
  });

  final CatalogHazard catalogHazard;
  final SelectedHazardEntry entry;
  final ValueChanged<bool> onHazardChanged;
  final void Function(String controlId, bool selected) onControlChanged;
  final bool highlightError;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final totalControls = catalogHazard.controls.length;
    final checkedControls = entry.selectedControlIds.length;
    final hasApplicableControls = entry.isSelected && checkedControls > 0;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      clipBehavior: Clip.antiAlias,
      shape: highlightError
          ? RoundedRectangleBorder(
              side: BorderSide(color: theme.colorScheme.error, width: 2),
              borderRadius: BorderRadius.circular(12),
            )
          : null,
      child: Column(
        children: [
          InkWell(
            onTap: () => onHazardChanged(!entry.isSelected),
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              child: Row(
                children: [
                  SizedBox(
                    width: 48,
                    height: 48,
                    child: Checkbox(
                      value: entry.isSelected,
                      onChanged: (value) => onHazardChanged(value ?? false),
                      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                  ),
                  Expanded(
                    child: Text(
                      catalogHazard.name,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontSize: 18,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (entry.isSelected) ...[
            const Divider(height: 1),
            ExpansionTile(
              initiallyExpanded: true,
              tilePadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
              title: Text(
                '2.1 — What can go wrong?',
                style: theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.w700,
                  fontSize: 16,
                ),
              ),
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                  child: Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.red.shade50,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.red.shade100),
                    ),
                    child: Text(
                      catalogHazard.consequenceDescription,
                      style: theme.textTheme.bodyLarge?.copyWith(
                        height: 1.45,
                        fontSize: 16,
                      ),
                    ),
                  ),
                ),
              ],
            ),
            ExpansionTile(
              initiallyExpanded: true,
              tilePadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
              title: Text(
                '2.2 — What can I do about it?',
                style: theme.textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.w700,
                  fontSize: 16,
                ),
              ),
              subtitle: Text(
                hasApplicableControls
                    ? '$checkedControls of $totalControls applicable controls selected'
                    : 'Select all controls that apply (at least one required)',
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: hasApplicableControls
                      ? Colors.green.shade700
                      : (highlightError ? theme.colorScheme.error : Colors.grey.shade700),
                ),
              ),
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(8, 0, 8, 12),
                  child: Column(
                    children: catalogHazard.controls.map((control) {
                      final isChecked = entry.selectedControlIds.contains(control.id);
                      return Padding(
                        padding: const EdgeInsets.only(bottom: 4),
                        child: Material(
                          color: isChecked
                              ? theme.colorScheme.primary.withValues(alpha: 0.08)
                              : Colors.grey.shade50,
                          borderRadius: BorderRadius.circular(12),
                          child: CheckboxListTile(
                            value: isChecked,
                            onChanged: (value) =>
                                onControlChanged(control.id, value ?? false),
                            controlAffinity: ListTileControlAffinity.leading,
                            contentPadding: const EdgeInsets.symmetric(
                              horizontal: 12,
                              vertical: 4,
                            ),
                            title: Text(
                              control.description,
                              style: const TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        ),
                      );
                    }).toList(),
                  ),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
