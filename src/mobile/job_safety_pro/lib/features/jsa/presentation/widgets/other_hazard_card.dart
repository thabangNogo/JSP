import 'dart:io';

import 'package:flutter/material.dart';
import '../../domain/models/hazard_catalog_models.dart';
import 'glove_friendly/glove_friendly_controls.dart';

class OtherHazardCard extends StatefulWidget {
  const OtherHazardCard({
    super.key,
    required this.entry,
    required this.highlightError,
    required this.onSelectedChanged,
    required this.onDescriptionChanged,
    required this.onConsequenceChanged,
    required this.onControlsChanged,
    required this.onAddPhoto,
    required this.onRemovePhoto,
  });

  final SelectedHazardEntry entry;
  final bool highlightError;
  final ValueChanged<bool> onSelectedChanged;
  final ValueChanged<String> onDescriptionChanged;
  final ValueChanged<String> onConsequenceChanged;
  final ValueChanged<String> onControlsChanged;
  final VoidCallback onAddPhoto;
  final void Function(int index) onRemovePhoto;

  @override
  State<OtherHazardCard> createState() => _OtherHazardCardState();
}

class _OtherHazardCardState extends State<OtherHazardCard> {
  late final TextEditingController _descriptionController;
  late final TextEditingController _consequenceController;
  late final TextEditingController _controlsController;

  @override
  void initState() {
    super.initState();
    _descriptionController = TextEditingController(text: widget.entry.customDescription);
    _consequenceController = TextEditingController(text: widget.entry.customConsequence);
    _controlsController = TextEditingController(text: widget.entry.customControlsText);
  }

  @override
  void didUpdateWidget(OtherHazardCard oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.entry.customDescription != widget.entry.customDescription &&
        _descriptionController.text != widget.entry.customDescription) {
      _descriptionController.text = widget.entry.customDescription;
    }
    if (oldWidget.entry.customConsequence != widget.entry.customConsequence &&
        _consequenceController.text != widget.entry.customConsequence) {
      _consequenceController.text = widget.entry.customConsequence;
    }
    if (oldWidget.entry.customControlsText != widget.entry.customControlsText &&
        _controlsController.text != widget.entry.customControlsText) {
      _controlsController.text = widget.entry.customControlsText;
    }
  }

  @override
  void dispose() {
    _descriptionController.dispose();
    _consequenceController.dispose();
    _controlsController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final entry = widget.entry;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      clipBehavior: Clip.antiAlias,
      shape: widget.highlightError
          ? RoundedRectangleBorder(
              side: BorderSide(color: theme.colorScheme.error, width: 2),
              borderRadius: BorderRadius.circular(12),
            )
          : null,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          InkWell(
            onTap: () => widget.onSelectedChanged(!entry.isSelected),
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              child: Row(
                children: [
                  SizedBox(
                    width: 48,
                    height: 48,
                    child: Checkbox(
                      value: entry.isSelected,
                      onChanged: (v) => widget.onSelectedChanged(v ?? false),
                      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                  ),
                  Expanded(
                    child: Text(
                      'Other',
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
            Padding(
              padding: const EdgeInsets.all(kGloveHorizontalPadding),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextField(
                    controller: _descriptionController,
                    onChanged: widget.onDescriptionChanged,
                    decoration: const InputDecoration(
                      labelText: 'What can cause injury/harm/damage?',
                      hintText: 'Describe the hazard',
                    ),
                    maxLines: 2,
                  ),
                  const SizedBox(height: 12),
                  Text(
                    '2.1 — What can go wrong?',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _consequenceController,
                    onChanged: widget.onConsequenceChanged,
                    decoration: const InputDecoration(
                      hintText: 'Describe what can go wrong',
                    ),
                    maxLines: 3,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    '2.2 — What can I do about it?',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _controlsController,
                    onChanged: widget.onControlsChanged,
                    decoration: const InputDecoration(
                      hintText: 'List controls (one per line or separated by ;)',
                    ),
                    maxLines: 4,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'Hazard photo (optional)',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  if (entry.photoPaths.isNotEmpty)
                    SizedBox(
                      height: 100,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        itemCount: entry.photoPaths.length,
                        separatorBuilder: (_, __) => const SizedBox(width: 8),
                        itemBuilder: (context, index) {
                          return Stack(
                            children: [
                              ClipRRect(
                                borderRadius: BorderRadius.circular(8),
                                child: Image.file(
                                  File(entry.photoPaths[index]),
                                  width: 100,
                                  height: 100,
                                  fit: BoxFit.cover,
                                ),
                              ),
                              Positioned(
                                top: 0,
                                right: 0,
                                child: IconButton(
                                  icon: const Icon(Icons.close, color: Colors.white),
                                  style: IconButton.styleFrom(
                                    backgroundColor: Colors.black54,
                                  ),
                                  onPressed: () => widget.onRemovePhoto(index),
                                ),
                              ),
                            ],
                          );
                        },
                      ),
                    ),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: widget.onAddPhoto,
                    icon: const Icon(Icons.add_a_photo_outlined),
                    label: const Text('Add photo'),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
