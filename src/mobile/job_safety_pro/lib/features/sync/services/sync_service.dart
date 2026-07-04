import 'package:uuid/uuid.dart';
import '../../../../core/storage/hive_service.dart';
import '../data/models/sync_queue_item.dart';

class SyncService {
  SyncService(this._hiveService);

  final HiveService _hiveService;
  final _uuid = const Uuid();

  Future<void> enqueue(SyncQueueItem item) async {
    await _hiveService.syncQueueBox.put(item.id, item.toJson());
  }

  Future<List<SyncQueueItem>> getPendingItems() async {
    return _hiveService.syncQueueBox.values
        .map((e) => SyncQueueItem.fromJson(Map<String, dynamic>.from(e)))
        .toList()
      ..sort((a, b) => a.createdAt.compareTo(b.createdAt));
  }

  Future<void> removeItem(String id) async {
    await _hiveService.syncQueueBox.delete(id);
  }

  Future<int> get pendingCount async => _hiveService.syncQueueBox.length;

  String generateId() => _uuid.v4();
}
