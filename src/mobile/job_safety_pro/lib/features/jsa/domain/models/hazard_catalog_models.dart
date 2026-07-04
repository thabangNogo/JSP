/// QJSA hazard catalog: Hazard → Consequence → Controls.

class PredefinedControl {
  const PredefinedControl({
    required this.id,
    required this.description,
    required this.hierarchyOfControl,
  });

  final String id;
  final String description;
  final String hierarchyOfControl;

  Map<String, dynamic> toJson() => {
        'id': id,
        'description': description,
        'hierarchyOfControl': hierarchyOfControl,
      };

  factory PredefinedControl.fromJson(Map<String, dynamic> json) => PredefinedControl(
        id: json['id'] as String,
        description: json['description'] as String,
        hierarchyOfControl: json['hierarchyOfControl'] as String,
      );
}

class HazardConsequence {
  const HazardConsequence({required this.description});

  final String description;

  Map<String, dynamic> toJson() => {'description': description};

  factory HazardConsequence.fromJson(Map<String, dynamic> json) =>
      HazardConsequence(description: json['description'] as String);
}

class CatalogHazard {
  const CatalogHazard({
    required this.id,
    required this.name,
    required this.consequence,
    required this.controls,
  });

  final String id;
  final String name;
  final HazardConsequence consequence;
  final List<PredefinedControl> controls;

  String get consequenceDescription => consequence.description;

  Set<String> get allControlIds => controls.map((c) => c.id).toSet();

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'consequence': consequence.toJson(),
        'controls': controls.map((c) => c.toJson()).toList(),
      };

  factory CatalogHazard.fromJson(Map<String, dynamic> json) => CatalogHazard(
        id: json['id'] as String,
        name: json['name'] as String,
        consequence: HazardConsequence.fromJson(
          Map<String, dynamic>.from(json['consequence'] as Map),
        ),
        controls: (json['controls'] as List<dynamic>)
            .map((e) => PredefinedControl.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
      );
}

/// User selection for one catalog hazard on the Identify Hazards step.
class SelectedHazardEntry {
  const SelectedHazardEntry({
    required this.catalogHazardId,
    this.isSelected = false,
    this.selectedControlIds = const {},
    this.customDescription = '',
    this.customConsequence = '',
    this.customControlsText = '',
    this.photoPaths = const [],
  });

  static const otherCatalogId = 'other';

  final String catalogHazardId;
  final bool isSelected;
  final Set<String> selectedControlIds;
  final String customDescription;
  final String customConsequence;
  final String customControlsText;
  final List<String> photoPaths;

  bool get isOther => catalogHazardId == otherCatalogId;

  SelectedHazardEntry copyWith({
    bool? isSelected,
    Set<String>? selectedControlIds,
    String? customDescription,
    String? customConsequence,
    String? customControlsText,
    List<String>? photoPaths,
    bool clearCustom = false,
  }) {
    return SelectedHazardEntry(
      catalogHazardId: catalogHazardId,
      isSelected: isSelected ?? this.isSelected,
      selectedControlIds: selectedControlIds ?? this.selectedControlIds,
      customDescription:
          clearCustom ? '' : (customDescription ?? this.customDescription),
      customConsequence:
          clearCustom ? '' : (customConsequence ?? this.customConsequence),
      customControlsText:
          clearCustom ? '' : (customControlsText ?? this.customControlsText),
      photoPaths: clearCustom ? const [] : (photoPaths ?? this.photoPaths),
    );
  }

  Map<String, dynamic> toJson() => {
        'catalogHazardId': catalogHazardId,
        'isSelected': isSelected,
        'selectedControlIds': selectedControlIds.toList(),
        if (customDescription.isNotEmpty) 'customDescription': customDescription,
        if (customConsequence.isNotEmpty) 'customConsequence': customConsequence,
        if (customControlsText.isNotEmpty) 'customControlsText': customControlsText,
        if (photoPaths.isNotEmpty) 'photoPaths': photoPaths,
      };

  factory SelectedHazardEntry.fromJson(Map<String, dynamic> json) => SelectedHazardEntry(
        catalogHazardId: json['catalogHazardId'] as String,
        isSelected: json['isSelected'] as bool? ?? false,
        selectedControlIds:
            (json['selectedControlIds'] as List<dynamic>? ?? []).cast<String>().toSet(),
        customDescription: json['customDescription'] as String? ?? '',
        customConsequence: json['customConsequence'] as String? ?? '',
        customControlsText: json['customControlsText'] as String? ?? '',
        photoPaths: (json['photoPaths'] as List<dynamic>? ?? []).cast<String>(),
      );
}
