import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../providers/jsa_providers.dart';

mixin WorkflowStepMixin<T extends ConsumerStatefulWidget> on ConsumerState<T> {
  String get draftId;

  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(assessmentWorkflowProvider.notifier).loadDraft(draftId));
  }

  void goNext(String nextRoute) {
    ref.read(assessmentWorkflowProvider.notifier).nextStep();
    context.go(nextRoute);
  }
}

class ObserveStopScreen extends ConsumerStatefulWidget {
  const ObserveStopScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<ObserveStopScreen> createState() => _ObserveStopScreenState();
}

class _ObserveStopScreenState extends ConsumerState<ObserveStopScreen> with WorkflowStepMixin {
  final _controller = TextEditingController();

  @override
  String get draftId => widget.draftId;

  @override
  Widget build(BuildContext context) {
    final draft = ref.watch(assessmentWorkflowProvider);
    if (draft != null && _controller.text.isEmpty) {
      _controller.text = draft.observeStopNotes;
    }

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text('Step 4: Observe the task and stop if unsafe conditions exist.'),
          const SizedBox(height: 16),
          TextField(
            controller: _controller,
            maxLines: 5,
            decoration: const InputDecoration(
              labelText: 'Observation notes',
              hintText: 'Describe what you observed...',
            ),
          ),
          const Spacer(),
          ElevatedButton(
            onPressed: () async {
              if (draft == null) return;
              await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
                    draft.copyWith(observeStopNotes: _controller.text),
                  );
              goNext('/jsas/workflow/${widget.draftId}/conversation');
            },
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }
}

class StartConversationScreen extends ConsumerStatefulWidget {
  const StartConversationScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<StartConversationScreen> createState() => _StartConversationScreenState();
}

class _StartConversationScreenState extends ConsumerState<StartConversationScreen> with WorkflowStepMixin {
  final _controller = TextEditingController();

  @override
  String get draftId => widget.draftId;

  @override
  Widget build(BuildContext context) {
    final draft = ref.watch(assessmentWorkflowProvider);
    if (draft != null && _controller.text.isEmpty) {
      _controller.text = draft.conversationNotes;
    }

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text('Step 5: Start a safety conversation with the work team.'),
          const SizedBox(height: 16),
          TextField(
            controller: _controller,
            maxLines: 5,
            decoration: const InputDecoration(labelText: 'Conversation notes'),
          ),
          const Spacer(),
          ElevatedButton(
            onPressed: () async {
              if (draft == null) return;
              await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
                    draft.copyWith(conversationNotes: _controller.text),
                  );
              goNext('/jsas/workflow/${widget.draftId}/sign-off');
            },
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }
}
