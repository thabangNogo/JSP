import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/jsa_providers.dart';

mixin WorkflowStepMixin<T extends ConsumerStatefulWidget> on ConsumerState<T> {
  String get draftId;

  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(assessmentWorkflowProvider.notifier).loadDraft(draftId));
  }
}
