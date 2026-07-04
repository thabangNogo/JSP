import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/router/app_routes.dart';
import '../../../auth/presentation/providers/auth_provider.dart';
import '../../../jsa/presentation/providers/work_lookups_provider.dart';
import '../providers/employee_profile_provider.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authProvider).user;
    final profileState = ref.watch(employeeProfileProvider);
    final profile = profileState.profile;
    final lookupsAsync = ref.watch(workLookupsProvider);
    final departmentLabel = lookupsAsync.maybeWhen(
      data: (lookups) => profile?.resolveDepartmentName(lookups.departments) ?? '',
      orElse: () => profile?.workDepartmentName ?? '',
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text('Employee Profile'),
        actions: [
          IconButton(
            icon: const Icon(Icons.edit),
            tooltip: 'Edit profile',
            onPressed: () => context.push(AppRoutes.profileEdit),
          ),
        ],
      ),
      body: profileState.isLoading
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                CircleAvatar(
                  radius: 40,
                  child: Text(
                    (profile?.name.isNotEmpty == true ? profile!.name[0] : null) ??
                        (user?.firstName.isNotEmpty == true ? user!.firstName[0] : '?'),
                    style: const TextStyle(fontSize: 32),
                  ),
                ),
                const SizedBox(height: 16),
                if (profile != null && profile.isComplete) ...[
                  _ProfileRow(label: 'Name', value: profile.name),
                  _ProfileRow(label: 'Surname', value: profile.surname),
                  _ProfileRow(label: 'Company Number', value: profile.companyNumber),
                  _ProfileRow(
                    label: 'Department',
                    value: departmentLabel.isNotEmpty ? departmentLabel : '—',
                  ),
                  _ProfileRow(label: 'Occupation', value: profile.occupation),
                ] else
                  const Text(
                    'Your employee profile is incomplete. Complete it for sign-off on assessments.',
                    style: TextStyle(fontSize: 16),
                  ),
                const Divider(height: 32),
                if (user != null) ...[
                  Text('Account: ${user.email}'),
                  Text('Roles: ${user.roles.join(', ')}'),
                ],
                const SizedBox(height: 24),
                ElevatedButton.icon(
                  onPressed: () => context.push(AppRoutes.profileEdit),
                  icon: const Icon(Icons.edit),
                  label: Text(profile?.isComplete == true ? 'Edit Profile' : 'Complete Profile'),
                ),
                const SizedBox(height: 12),
                ElevatedButton(
                  onPressed: () async {
                    await ref.read(authProvider.notifier).logout();
                    if (context.mounted) context.go(AppRoutes.login);
                  },
                  style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                  child: const Text('Sign Out'),
                ),
              ],
            ),
    );
  }
}

class _ProfileRow extends StatelessWidget {
  const _ProfileRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 140,
            child: Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}
