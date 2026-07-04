import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/router/app_routes.dart';
import '../../../../shared/enums/assessment_workflow_step.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/jsa_providers.dart';
import '../providers/workflow_form_reset.dart';

void _openDraftWorkflow(
  BuildContext context,
  WidgetRef ref,
  int currentStep,
  String localId,
) {
  resetWorkflowFormProviders(ref);
  final routes = [
    '/jsas/workflow/$localId/job-information',
    '/jsas/workflow/$localId/quick-assessment',
    '/jsas/workflow/$localId/hazards',
    '/jsas/workflow/$localId/risks',
    '/jsas/workflow/$localId/sign-off',
  ];
  context.go(routes[currentStep.clamp(0, routes.length - 1)]);
}

class JsaListScreen extends ConsumerWidget {
  const JsaListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final drafts = ref.watch(assessmentDraftsProvider);

    return Scaffold(
      appBar: const AppNavigationBar(title: 'Assessments'),
      body: drafts.when(
        loading: () => RefreshIndicator(
          onRefresh: () => refreshAssessmentDrafts(ref),
          child: ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            children: const [
              SizedBox(height: 120),
              Center(child: CircularProgressIndicator()),
            ],
          ),
        ),
        error: (e, _) => RefreshIndicator(
          onRefresh: () => refreshAssessmentDrafts(ref),
          child: ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            children: [
              SizedBox(height: MediaQuery.sizeOf(context).height * 0.25),
              ErrorView(
                message: dioErrorMessage(e),
                onRetry: () => refreshAssessmentDrafts(ref),
                shrinkWrap: true,
              ),
            ],
          ),
        ),
        data: (items) => RefreshIndicator(
          onRefresh: () => refreshAssessmentDrafts(ref),
          child: items.isEmpty
              ? ListView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  children: [
                    SizedBox(height: MediaQuery.sizeOf(context).height * 0.3),
                    const Center(
                      child: Text('No drafts yet. Start a new assessment.'),
                    ),
                  ],
                )
              : ListView.builder(
                  physics: const AlwaysScrollableScrollPhysics(),
                  itemCount: items.length,
                  itemBuilder: (context, index) {
                    final draft = items[index];
                    return Card(
                      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      child: ListTile(
                        title: Text(draft.title),
                        subtitle: Text(
                          'Step ${draft.currentStep + 1}/${AssessmentWorkflowStep.totalSteps} • ${draft.isSynced ? 'Synced' : 'Draft'}',
                        ),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () => _openDraftWorkflow(
                          context,
                          ref,
                          draft.currentStep,
                          draft.localId,
                        ),
                      ),
                    );
                  },
                ),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push(AppRoutes.jsaNew),
        child: const Icon(Icons.add),
      ),
    );
  }
}
