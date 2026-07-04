import '../../domain/models/hazard_catalog_models.dart';

/// QJSA booklet Step 2 / 2.1 / 2.2 — one row per hazard, consequence, and controls.
class QjsaHazardCatalog {
  static List<PredefinedControl> _splitControls(String hazardId, String controlLine) {
    final parts = controlLine
        .split(';')
        .map((s) => s.trim())
        .where((s) => s.isNotEmpty);
    var index = 0;
    return parts
        .map(
          (description) => PredefinedControl(
            id: '$hazardId-c${++index}',
            description: description,
            hierarchyOfControl: 'Administrative',
          ),
        )
        .toList();
  }

  static final List<CatalogHazard> hazards = [
    CatalogHazard(
      id: 'moving-machinery',
      name: 'i.e. Moving Machinery',
      consequence: HazardConsequence(
        description: 'Bump a person; run over and kill',
      ),
      controls: _splitControls(
        'moving-machinery',
        'People to stay clear from machine route; Barricade; Prevent access; Always be on look out',
      ),
    ),
    CatalogHazard(
      id: 'bad-roof-highwall',
      name: 'Bad Roof / Highwall',
      consequence: HazardConsequence(
        description: 'Fall on person/machinery',
      ),
      controls: _splitControls(
        'bad-roof-highwall',
        'Barring; Support; Barricade',
      ),
    ),
    CatalogHazard(
      id: 'electrical-cables',
      name: 'Electrical Cables',
      consequence: HazardConsequence(
        description: 'Electrical shock/Electrocution',
      ),
      controls: _splitControls(
        'electrical-cables',
        'Switch Off; Isolate, Lock Out',
      ),
    ),
    CatalogHazard(
      id: 'working-at-heights',
      name: 'Working at Heights',
      consequence: HazardConsequence(
        description: 'Fall to below',
      ),
      controls: _splitControls(
        'working-at-heights',
        'Fall restraint; harness; 2 x lanyards',
      ),
    ),
    CatalogHazard(
      id: 'falling-objects',
      name: 'Falling Objects',
      consequence: HazardConsequence(
        description: 'Fall on person/machinery',
      ),
      controls: _splitControls(
        'falling-objects',
        'Barricade are below',
      ),
    ),
    CatalogHazard(
      id: 'uneven-ground',
      name: 'Uneven Ground',
      consequence: HazardConsequence(
        description: 'Trip & Fall',
      ),
      controls: _splitControls(
        'uneven-ground',
        'Good Housekeeping; Awareness; Floor leveling; Signage',
      ),
    ),
    CatalogHazard(
      id: 'dust-noise',
      name: 'Dust; Noise',
      consequence: HazardConsequence(
        description: 'Poor visibility; Lung disease; hearing loss',
      ),
      controls: _splitControls(
        'dust-noise',
        'Water down; ventilation; correct PPE',
      ),
    ),
    CatalogHazard(
      id: 'oil-spills',
      name: 'Oil Spills',
      consequence: HazardConsequence(
        description: 'Slip & Fall; Soil pollution',
      ),
      controls: _splitControls(
        'oil-spills',
        'Clean up spoils; Drip Trays; Procedures',
      ),
    ),
    CatalogHazard(
      id: 'moving-conveyors',
      name: 'Moving Conveyors',
      consequence: HazardConsequence(
        description: 'Pulled into conveyor; loss of limb; death',
      ),
      controls: _splitControls(
        'moving-conveyors',
        'Guards; Switch Off; Isolate; Look Out; Procedure',
      ),
    ),
    CatalogHazard(
      id: 'moving-machine-parts',
      name: 'Moving Machine Parts',
      consequence: HazardConsequence(
        description: 'Nip points; articulation point',
      ),
      controls: _splitControls(
        'moving-machine-parts',
        'Guards; Isolation bars',
      ),
    ),
    CatalogHazard(
      id: 'wet-floors',
      name: 'Wet Floors',
      consequence: HazardConsequence(
        description: 'Slip & Fall',
      ),
      controls: _splitControls(
        'wet-floors',
        'Signage; clean up spillage',
      ),
    ),
    CatalogHazard(
      id: 'poor-stacking-storage',
      name: 'Poor Stacking & Storage',
      consequence: HazardConsequence(
        description: 'Fall over/onto',
      ),
      controls: _splitControls(
        'poor-stacking-storage',
        'Housekeeping; Heavy Items below',
      ),
    ),
    CatalogHazard(
      id: 'other',
      name: 'Other',
      consequence: const HazardConsequence(description: ''),
      controls: const [],
    ),
  ];

  static CatalogHazard? byId(String id) {
    for (final hazard in hazards) {
      if (hazard.id == id) return hazard;
    }
    return null;
  }

  static Map<String, CatalogHazard> get byIdMap =>
      {for (final h in hazards) h.id: h};
}
