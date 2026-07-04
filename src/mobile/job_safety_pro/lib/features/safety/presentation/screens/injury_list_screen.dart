import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/router/app_routes.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/safety_providers.dart';

final injuriesListProvider = FutureProvider<List<Map<String, dynamic>>>((ref) async {
  return ref.read(safetyRemoteDataSourceProvider).getInjuries();
});

class InjuryListScreen extends ConsumerWidget {
  const InjuryListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final injuries = ref.watch(injuriesListProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Injuries'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => context.push(AppRoutes.captureInjury),
          ),
        ],
      ),
      body: injuries.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(injuriesListProvider),
        ),
        data: (items) {
          if (items.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('No injuries recorded yet.'),
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: () => context.push(AppRoutes.captureInjury),
                    icon: const Icon(Icons.add),
                    label: const Text('Capture Injury'),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: () async => ref.invalidate(injuriesListProvider),
            child: ListView.separated(
              padding: const EdgeInsets.all(8),
              itemCount: items.length,
              separatorBuilder: (_, __) => const Divider(height: 1),
              itemBuilder: (context, index) {
                final injury = items[index];
                final id = injury['id'] as String;
                final employee = injury['employeeName'] as String? ?? 'Unknown';
                final type = injury['injuryType'] as String? ?? '';
                final status = injury['status'] as String? ?? '';
                final occurred = injury['injuryOccurredAt'] as String?;
                return ListTile(
                  leading: const CircleAvatar(child: Icon(Icons.local_hospital)),
                  title: Text(employee),
                  subtitle: Text('$type • $status${occurred != null ? '\n$occurred' : ''}'),
                  isThreeLine: occurred != null,
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push(AppRoutes.injuryDetailPath(id)),
                );
              },
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push(AppRoutes.captureInjury),
        child: const Icon(Icons.add),
      ),
    );
  }
}
