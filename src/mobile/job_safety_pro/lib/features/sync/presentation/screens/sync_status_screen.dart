import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../jsa/presentation/providers/jsa_providers.dart';

class SyncStatusScreen extends ConsumerWidget {
  const SyncStatusScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final syncService = ref.watch(syncServiceProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Sync Status')),
      body: FutureBuilder(
        future: syncService.getPendingItems(),
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const Center(child: CircularProgressIndicator());
          }
          final items = snapshot.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('All assessments are synced.'));
          }
          return ListView.builder(
            itemCount: items.length,
            itemBuilder: (_, i) {
              final item = items[i];
              return ListTile(
                title: Text(item.action.name),
                subtitle: Text('Queued ${item.createdAt}'),
                trailing: Text('Retries: ${item.retryCount}'),
              );
            },
          );
        },
      ),
    );
  }
}
