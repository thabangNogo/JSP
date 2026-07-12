import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/router/app_routes.dart';
import '../providers/ppe_providers.dart';

class MyPpeScreen extends ConsumerWidget {
  const MyPpeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(myPpeProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('My PPE')),
      body: async.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Failed to load PPE: $e')),
        data: (data) {
          final current = (data['currentRequests'] as List<dynamic>? ?? []);
          final history = (data['history'] as List<dynamic>? ?? []);
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text('Current Requests', style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: 8),
              ...current.map((r) => _RequestCard(map: r as Map<String, dynamic>)),
              if (current.isEmpty) const Text('No active PPE requests.'),
              const SizedBox(height: 24),
              Text('History', style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: 8),
              ...history.map((h) => _HistoryTile(map: h as Map<String, dynamic>)),
            ],
          );
        },
      ),
    );
  }
}

class MyPpeDashboardCard extends ConsumerWidget {
  const MyPpeDashboardCard({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(myPpeProvider);
    return Card(
      child: InkWell(
        onTap: () => context.push(AppRoutes.myPpe),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: async.when(
            loading: () => const ListTile(title: Text('My PPE'), subtitle: Text('Loading...')),
            error: (_, __) => const ListTile(title: Text('My PPE'), subtitle: Text('Tap to view')),
            data: (data) {
              final current = data['currentRequests'] as List<dynamic>? ?? [];
              final first = current.isNotEmpty ? current.first as Map<String, dynamic> : null;
              return ListTile(
                leading: const Icon(Icons.health_and_safety),
                title: const Text('My PPE'),
                subtitle: Text(first != null
                    ? '${first['ppeItemName']} · ${first['status']}'
                    : '${current.length} active request(s)'),
              );
            },
          ),
        ),
      ),
    );
  }
}

class _RequestCard extends StatelessWidget {
  const _RequestCard({required this.map});
  final Map<String, dynamic> map;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        title: Text('${map['ppeItemName'] ?? 'PPE'}'),
        subtitle: Text('${map['status']} · ${map['requestNumber']}'),
      ),
    );
  }
}

class _HistoryTile extends StatelessWidget {
  const _HistoryTile({required this.map});
  final Map<String, dynamic> map;

  @override
  Widget build(BuildContext context) {
    final timeline = map['timeline'] as List<dynamic>? ?? [];
    return ExpansionTile(
      title: Text('${map['ppeItemName']} · ${map['status']}'),
      subtitle: Text('Requested ${map['requestedDate']}'),
      children: timeline.map((t) {
        final step = t as Map<String, dynamic>;
        return ListTile(
          dense: true,
          title: Text('${step['newStatus']}'),
          subtitle: Text('${step['actionDate']}'),
        );
      }).toList(),
    );
  }
}
