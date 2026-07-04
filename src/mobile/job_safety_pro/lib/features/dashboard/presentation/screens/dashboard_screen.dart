import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/router/app_routes.dart';
import '../../../../core/utils/connectivity_service.dart';
import '../../../../shared/enums/assessment_status.dart';
import '../../../auth/presentation/providers/auth_provider.dart';
import '../../../jsa/data/models/jsa_models.dart';
import '../../../jsa/domain/services/draft_auto_save_service.dart';
import '../../../jsa/presentation/providers/jsa_providers.dart';
import '../../../jsa/presentation/providers/workflow_form_reset.dart';
import '../../../safety/presentation/widgets/employee_kpi_section.dart';
import '../../../safety/presentation/widgets/injury_free_days_card.dart';
import '../../../safety/presentation/widgets/safety_actions_section.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authProvider).user;
    final isOnline = ref.watch(isOnlineProvider).maybeWhen(data: (v) => v, orElse: () => true);
    final drafts = ref.watch(assessmentDraftsProvider);
    final pendingSync = ref.watch(syncPendingCountProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Dashboard'),
        actions: [
          IconButton(
            icon: const Icon(Icons.person),
            onPressed: () => context.push(AppRoutes.profile),
          ),
          IconButton(
            icon: const Icon(Icons.settings),
            onPressed: () => context.push(AppRoutes.settings),
          ),
        ],
      ),
      body: Column(
        children: [
          OfflineBanner(isOnline: isOnline),
          Expanded(
            child: drafts.when(
              loading: () => _RefreshableScrollView(
                onRefresh: () => refreshAssessmentDrafts(ref),
                children: const [
                  SizedBox(height: 120),
                  Center(child: CircularProgressIndicator()),
                ],
              ),
              error: (e, _) => _RefreshableScrollView(
                onRefresh: () => refreshAssessmentDrafts(ref),
                children: [
                  ErrorView(
                    message: dioErrorMessage(e),
                    onRetry: () => refreshAssessmentDrafts(ref),
                    shrinkWrap: true,
                  ),
                ],
              ),
              data: (items) {
                final draftItems =
                    items.where((d) => d.status == AssessmentStatus.draft).toList();
                final submitted =
                    items.where((d) => d.status == AssessmentStatus.submitted).toList();
                final approved =
                    items.where((d) => d.status == AssessmentStatus.approved).toList();

                return _RefreshableScrollView(
                  onRefresh: () => refreshAssessmentDrafts(ref),
                  children: [
                    Text(
                      'Welcome, ${user?.firstName ?? 'User'}',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 16),
                    const InjuryFreeDaysCard(),
                    const EmployeeKpiSection(),
                    const SafetyActionsSection(),
                    _AssessmentSection(
                      title: 'Draft Assessments',
                      items: draftItems,
                      emptyMessage: 'No drafts. Start a new assessment below.',
                      onTap: (draft) => _openAssessmentDetail(context, draft),
                      onDelete: (draft) => _deleteDraft(context, ref, draft),
                      showDelete: true,
                    ),
                    _AssessmentSection(
                      title: 'Submitted Assessments',
                      items: submitted,
                      emptyMessage: 'No submitted assessments yet.',
                      onTap: (draft) => _openAssessmentDetail(context, draft),
                    ),
                    _AssessmentSection(
                      title: 'Approved Assessments',
                      items: approved,
                      emptyMessage: 'No approved assessments yet.',
                      onTap: (draft) => _openAssessmentDetail(context, draft),
                    ),
                    pendingSync.when(
                      data: (count) => count > 0
                          ? _DashboardCard(
                              icon: Icons.sync,
                              title: 'Pending Sync ($count)',
                              subtitle: 'Tap to view sync status',
                              onTap: () => context.push(AppRoutes.syncStatus),
                            )
                          : const SizedBox.shrink(),
                      loading: () => const SizedBox.shrink(),
                      error: (_, __) => const SizedBox.shrink(),
                    ),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  void _openAssessmentDetail(BuildContext context, AssessmentDraftModel draft) {
    context.push(AppRoutes.jsaDetailPath(draft.localId));
  }

  Future<void> _deleteDraft(
    BuildContext context,
    WidgetRef ref,
    AssessmentDraftModel draft,
  ) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete draft?'),
        content: Text('Delete "${draft.title}"? This cannot be undone.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Cancel')),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (confirm != true) return;

    final autoSave = ref.read(draftAutoSaveServiceProvider);
    if (autoSave.isTracking(draft.localId)) {
      autoSave.stop();
    }

    final activeDraft = ref.read(assessmentWorkflowProvider);
    if (activeDraft != null &&
        (activeDraft.localId == draft.localId ||
            activeDraft.remoteId == draft.remoteId)) {
      resetWorkflowFormProviders(ref);
    }

    await ref.read(jsaRepositoryProvider).deleteDraft(draft.localId);
    await refreshAssessmentDrafts(ref);
    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Draft deleted')),
      );
    }
  }
}

class _RefreshableScrollView extends StatelessWidget {
  const _RefreshableScrollView({
    required this.onRefresh,
    required this.children,
  });

  final Future<void> Function() onRefresh;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: onRefresh,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        children: children,
      ),
    );
  }
}

class _AssessmentSection extends StatelessWidget {
  const _AssessmentSection({
    required this.title,
    required this.items,
    required this.emptyMessage,
    this.onTap,
    this.onDelete,
    this.showDelete = false,
  });

  final String title;
  final List<AssessmentDraftModel> items;
  final String emptyMessage;
  final void Function(AssessmentDraftModel draft)? onTap;
  final void Function(AssessmentDraftModel draft)? onDelete;
  final bool showDelete;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 8, top: 8),
          child: Text(
            title,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
        ),
        if (items.isEmpty)
          Padding(
            padding: const EdgeInsets.only(bottom: 16),
            child: Text(emptyMessage, style: TextStyle(color: Colors.grey.shade700)),
          )
        else
          ...items.map((draft) {
            final updated = draft.updatedAt ?? DateTime.now();
            final subtitle = draft.status.isDraft
                ? 'Step ${draft.currentStep + 1} • Updated ${_formatDate(updated)}'
                : '${_statusLabel(draft.status)} • ${_formatDate(updated)}';
            return Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: ListTile(
                title: Text(draft.title.isEmpty ? 'Untitled' : draft.title),
                subtitle: Text(subtitle),
                trailing: showDelete
                    ? Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          IconButton(
                            icon: const Icon(Icons.delete_outline, color: Colors.red),
                            onPressed: onDelete == null ? null : () => onDelete!(draft),
                          ),
                          const Icon(Icons.chevron_right),
                        ],
                      )
                    : const Icon(Icons.chevron_right),
                onTap: onTap == null ? null : () => onTap!(draft),
              ),
            );
          }),
      ],
    );
  }

  static String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year} ${date.hour}:${date.minute.toString().padLeft(2, '0')}';
  }

  static String _statusLabel(AssessmentStatus status) {
    return switch (status) {
      AssessmentStatus.draft => 'Draft',
      AssessmentStatus.submitted => 'Submitted',
      AssessmentStatus.approved => 'Approved',
      AssessmentStatus.rejected => 'Rejected',
    };
  }
}

class _DashboardCard extends StatelessWidget {
  const _DashboardCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: ListTile(
        leading: CircleAvatar(child: Icon(icon)),
        title: Text(title),
        subtitle: Text(subtitle),
        trailing: const Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
