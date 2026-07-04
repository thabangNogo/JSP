enum SyncActionType {
  createJsa,
  updateJsa,
  uploadAttachment,
}

class SyncQueueItem {
  SyncQueueItem({
    required this.id,
    required this.action,
    required this.payload,
    required this.createdAt,
    this.retryCount = 0,
  });

  factory SyncQueueItem.fromJson(Map<String, dynamic> json) => SyncQueueItem(
        id: json['id'] as String,
        action: SyncActionType.values.byName(json['action'] as String),
        payload: Map<String, dynamic>.from(json['payload'] as Map),
        createdAt: DateTime.parse(json['createdAt'] as String),
        retryCount: json['retryCount'] as int? ?? 0,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'action': action.name,
        'payload': payload,
        'createdAt': createdAt.toIso8601String(),
        'retryCount': retryCount,
      };

  final String id;
  final SyncActionType action;
  final Map<String, dynamic> payload;
  final DateTime createdAt;
  final int retryCount;
}
