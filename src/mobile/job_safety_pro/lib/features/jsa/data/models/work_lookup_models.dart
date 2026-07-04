class WorkLookupItem {
  const WorkLookupItem({required this.id, required this.name, required this.sortOrder});

  factory WorkLookupItem.fromJson(Map<String, dynamic> json) => WorkLookupItem(
        id: json['id']?.toString() ?? '',
        name: json['name'] as String? ?? '',
        sortOrder: json['sortOrder'] as int? ?? 0,
      );

  final String id;
  final String name;
  final int sortOrder;
}

class WorkLookups {
  const WorkLookups({
    required this.departments,
    required this.locations,
    required this.sections,
  });

  final List<WorkLookupItem> departments;
  final List<WorkLookupItem> locations;
  final List<WorkLookupItem> sections;
}

/// Resolves a lookup id from a saved id or display name (e.g. after server sync).
String? resolveWorkLookupId(
  List<WorkLookupItem> items, {
  String? id,
  String name = '',
}) {
  if (id != null && id.isNotEmpty) return id;
  final trimmed = name.trim();
  if (trimmed.isEmpty) return null;
  for (final item in items) {
    if (item.name == trimmed) return item.id;
  }
  return null;
}
