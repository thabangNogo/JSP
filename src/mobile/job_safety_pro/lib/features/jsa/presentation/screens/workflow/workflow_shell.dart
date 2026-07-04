import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../../core/router/app_routes.dart';
import '../../../../../shared/enums/assessment_workflow_step.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../providers/jsa_providers.dart';
import 'workflow_routes.dart';

class WorkflowShell extends ConsumerStatefulWidget {
  const WorkflowShell({super.key, required this.draftId, required this.child});

  final String draftId;
  final Widget child;

  @override
  ConsumerState<WorkflowShell> createState() => _WorkflowShellState();
}

class _WorkflowShellState extends ConsumerState<WorkflowShell> with WidgetsBindingObserver {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(draftAutoSaveServiceProvider).start(widget.draftId);
    });
  }

  @override
  void dispose() {
    ref.read(draftAutoSaveServiceProvider).saveDraft(showSnackBar: false);
    ref.read(draftAutoSaveServiceProvider).stop();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.paused ||
        state == AppLifecycleState.inactive ||
        state == AppLifecycleState.detached) {
      ref.read(draftAutoSaveServiceProvider).saveDraft(showSnackBar: false);
    }
  }

  Future<void> _goBack(int current) async {
    if (current <= 0) {
      await ref.read(draftAutoSaveServiceProvider).saveDraft(
            showSnackBar: true,
            context: context,
          );
      if (!mounted) return;
      context.go(AppRoutes.jsaList);
      return;
    }
    await ref.read(assessmentWorkflowProvider.notifier).previousStep();
    await ref.read(draftAutoSaveServiceProvider).saveDraft(
          showSnackBar: true,
          context: context,
        );
    if (!mounted) return;
    context.go(WorkflowRoutes.path(widget.draftId, current - 1));
  }

  @override
  Widget build(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    final current = WorkflowRoutes.indexFromPath(location);
    final canGoBack = current > 0;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        await _goBack(current);
      },
      child: Scaffold(
        appBar: AppBar(
          title: Text(AssessmentWorkflowStep.fromIndex(current).title),
          leading: IconButton(
            icon: Icon(canGoBack ? Icons.arrow_back : Icons.close),
            tooltip: canGoBack ? 'Previous step' : 'Exit workflow',
            onPressed: () => _goBack(current),
          ),
          actions: [
            IconButton(
              icon: const Icon(Icons.dashboard_outlined),
              tooltip: 'Dashboard',
              onPressed: () async {
                await ref.read(draftAutoSaveServiceProvider).saveDraft(
                      showSnackBar: true,
                      context: context,
                    );
                ref.read(draftAutoSaveServiceProvider).stop();
                if (context.mounted) context.go(AppRoutes.dashboard);
              },
            ),
          ],
        ),
        body: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
              child: Text(
                'QJSA Step ${current + 1} of ${AssessmentWorkflowStep.totalSteps}',
                style: Theme.of(context).textTheme.labelLarge?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: List.generate(AssessmentWorkflowStep.totalSteps, (index) {
                  return Expanded(
                    child: Container(
                      height: 6,
                      margin: EdgeInsets.only(
                        right: index == AssessmentWorkflowStep.totalSteps - 1 ? 0 : 4,
                      ),
                      decoration: BoxDecoration(
                        color: index <= current
                            ? Theme.of(context).colorScheme.primary
                            : Colors.grey.shade300,
                        borderRadius: BorderRadius.circular(3),
                      ),
                    ),
                  );
                }),
              ),
            ),
            Expanded(child: widget.child),
          ],
        ),
      ),
    );
  }
}
