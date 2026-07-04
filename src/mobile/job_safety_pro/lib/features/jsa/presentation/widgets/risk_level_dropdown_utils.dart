import 'package:flutter/material.dart';
import '../../data/models/jsa_models.dart';

/// Builds dropdown items with unique [RiskLevelModel.id] values only.
List<DropdownMenuItem<String>> buildUniqueRiskLevelItems(List<RiskLevelModel> levels) {
  final seen = <String>{};
  final items = <DropdownMenuItem<String>>[];

  for (final level in levels) {
    final id = level.id.trim();
    if (id.isEmpty || !seen.add(id)) continue;
    items.add(DropdownMenuItem(value: id, child: Text(level.name)));
  }

  return items;
}

/// Returns a [value] that exists in exactly one dropdown item, or null.
String? resolveRiskLevelDropdownValue(
  String? savedId,
  List<RiskLevelModel> levels,
) {
  if (levels.isEmpty) return null;

  final items = buildUniqueRiskLevelItems(levels);
  if (items.isEmpty) return null;

  final validIds = items.map((item) => item.value).whereType<String>().toSet();
  if (savedId == null || savedId.trim().isEmpty) return null;

  final normalized = savedId.trim();
  if (validIds.contains(normalized)) return normalized;

  const offlineCodeById = <String, String>{
    'offline-low': 'LOW',
    'offline-med': 'MED',
    'offline-high': 'HIGH',
    'offline-crit': 'CRIT',
  };
  final offlineCode = offlineCodeById[normalized];
  if (offlineCode != null) {
    final matches = levels.where((l) => l.code.toUpperCase() == offlineCode).toList();
    if (matches.length == 1 && validIds.contains(matches.first.id)) {
      return matches.first.id;
    }
  }

  // Legacy offline placeholder ids ('1', '2', '3') from early app versions.
  final legacyNumeric = int.tryParse(normalized);
  if (legacyNumeric != null) {
    final matches = levels.where((l) => l.numericValue == legacyNumeric).toList();
    if (matches.length == 1 && validIds.contains(matches.first.id)) {
      return matches.first.id;
    }
  }

  final codeMatches = levels
      .where((l) => l.code.toUpperCase() == normalized.toUpperCase())
      .toList();
  if (codeMatches.length == 1 && validIds.contains(codeMatches.first.id)) {
    return codeMatches.first.id;
  }

  return null;
}

/// Dropdown for initial or residual risk on the Assess Risks step.
class RiskLevelDropdownField extends StatelessWidget {
  const RiskLevelDropdownField({
    super.key,
    required this.label,
    required this.levels,
    required this.selectedId,
    required this.onChanged,
  });

  final String label;
  final List<RiskLevelModel> levels;
  final String? selectedId;
  final ValueChanged<String?> onChanged;

  @override
  Widget build(BuildContext context) {
    final items = buildUniqueRiskLevelItems(levels);
    final value = resolveRiskLevelDropdownValue(selectedId, levels);

    return DropdownButtonFormField<String>(
      key: ValueKey('$label-${selectedId ?? 'none'}-${items.length}'),
      initialValue: value,
      decoration: InputDecoration(labelText: label),
      hint: const Text('Select risk level'),
      items: items,
      onChanged: items.isEmpty ? null : onChanged,
    );
  }
}
