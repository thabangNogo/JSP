import '../models/jsa_models.dart';

final _guidPattern = RegExp(
  r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$',
);

bool isValidGuid(String value) => _guidPattern.hasMatch(value.trim());

/// Maps placeholder/offline risk level ids to API [RiskLevelModel.id] values (GUIDs).
String? resolveRiskLevelId(String? savedId, List<RiskLevelModel> apiLevels) {
  if (savedId == null || savedId.trim().isEmpty) return null;
  final normalized = savedId.trim();

  if (isValidGuid(normalized)) {
    final exists = apiLevels.any((level) => level.id == normalized);
    return exists ? normalized : null;
  }

  const offlineCodeById = <String, String>{
    'offline-low': 'LOW',
    'offline-med': 'MED',
    'offline-high': 'HIGH',
    'offline-crit': 'CRIT',
    '1': 'LOW',
    '2': 'MED',
    '3': 'HIGH',
    '4': 'CRIT',
  };

  final code = offlineCodeById[normalized];
  if (code != null) {
    final matches = apiLevels.where((l) => l.code.toUpperCase() == code).toList();
    if (matches.length == 1) return matches.first.id;
  }

  final numeric = int.tryParse(normalized);
  if (numeric != null) {
    final matches = apiLevels.where((l) => l.numericValue == numeric).toList();
    if (matches.length == 1) return matches.first.id;
  }

  final byCode = apiLevels
      .where((l) => l.code.toUpperCase() == normalized.toUpperCase())
      .toList();
  if (byCode.length == 1) return byCode.first.id;

  return null;
}

AssessmentDraftModel normalizeDraftRiskLevelIds(
  AssessmentDraftModel draft,
  List<RiskLevelModel> apiLevels,
) {
  final hazards = draft.hazards
      .map(
        (hazard) {
          final resolved = resolveRiskLevelId(hazard.riskLevelId, apiLevels);
          return hazard.copyWith(
            riskLevelId: resolved,
            residualRiskLevelId: resolved,
          );
        },
      )
      .toList();

  return draft.copyWith(hazards: hazards);
}
