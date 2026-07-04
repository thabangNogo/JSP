import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/safety_providers.dart';

final notificationsListProvider = FutureProvider<List<Map<String, dynamic>>>((ref) async {
  return ref.read(safetyRemoteDataSourceProvider).getNotifications();
});

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notifications = ref.watch(notificationsListProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Notifications')),
      body: notifications.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(notificationsListProvider),
        ),
        data: (items) {
          if (items.isEmpty) {
            return const Center(child: Text('No notifications yet.'));
          }
          return ListView.separated(
            padding: const EdgeInsets.all(8),
            itemCount: items.length,
            separatorBuilder: (_, __) => const Divider(height: 1),
            itemBuilder: (context, index) {
              final n = items[index];
              final isRead = n['isRead'] as bool? ?? false;
              final priority = n['priority'] as String? ?? 'Normal';
              return ListTile(
                leading: Icon(
                  priority == 'Critical'
                      ? Icons.warning_amber_rounded
                      : Icons.notifications_outlined,
                  color: priority == 'Critical' ? Colors.red : null,
                ),
                title: Text(
                  n['title'] as String? ?? '',
                  style: TextStyle(fontWeight: isRead ? FontWeight.normal : FontWeight.bold),
                ),
                subtitle: Text(n['message'] as String? ?? ''),
                onTap: () async {
                  final id = n['id']?.toString();
                  if (id != null) {
                    await ref.read(safetyRemoteDataSourceProvider).markNotificationRead(id);
                    ref.invalidate(notificationsListProvider);
                  }
                },
              );
            },
          );
        },
      ),
    );
  }
}
