import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/auth/role_utils.dart';
import '../../../../core/router/app_routes.dart';
import '../../../auth/presentation/providers/auth_provider.dart';

class SafetyActionsSection extends ConsumerWidget {
  const SafetyActionsSection({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authProvider).user;
    final canManageInjuries = isSafetyLead(user);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Padding(
          padding: const EdgeInsets.only(top: 8, bottom: 8),
          child: Text(
            'Safety Actions',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
          ),
        ),
        _ActionCard(
          icon: Icons.assignment_outlined,
          title: 'Create Job Safety Assessment',
          color: Colors.blue,
          onTap: () => context.push(AppRoutes.jsaNew),
        ),
        _ActionCard(
          icon: Icons.report_outlined,
          title: 'Report Near Miss',
          color: Colors.orange,
          onTap: () => context.push(AppRoutes.reportNearMiss),
        ),
        _ActionCard(
          icon: Icons.pan_tool_outlined,
          title: 'Stop Unsafe Work',
          color: Colors.red,
          onTap: () => context.push(AppRoutes.stopUnsafeWork),
        ),
        if (canManageInjuries) ...[
          _ActionCard(
            icon: Icons.local_hospital_outlined,
            title: 'Injuries',
            color: Colors.deepPurple,
            onTap: () => context.push(AppRoutes.injuries),
          ),
        ],
        _ActionCard(
          icon: Icons.notifications_outlined,
          title: 'Notifications',
          color: Colors.teal,
          onTap: () => context.push(AppRoutes.notifications),
        ),
      ],
    );
  }
}

class _ActionCard extends StatelessWidget {
  const _ActionCard({
    required this.icon,
    required this.title,
    required this.color,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: color.withValues(alpha: 0.15),
          child: Icon(icon, color: color),
        ),
        title: Text(title, style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600)),
        trailing: const Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
