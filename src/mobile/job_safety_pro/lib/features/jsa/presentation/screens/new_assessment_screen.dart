import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../../../auth/presentation/providers/auth_provider.dart';
import '../providers/jsa_providers.dart';
import '../providers/workflow_form_reset.dart';

class NewAssessmentScreen extends ConsumerStatefulWidget {
  const NewAssessmentScreen({super.key});

  @override
  ConsumerState<NewAssessmentScreen> createState() => _NewAssessmentScreenState();
}

class _NewAssessmentScreenState extends ConsumerState<NewAssessmentScreen> {
  static const _worksOrderPrefix = 'WO-';

  final _titleController = TextEditingController(text: _worksOrderPrefix);
  final _descController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _clearFormFields();
      resetWorkflowFormProviders(ref);
    });
  }

  void _clearFormFields() {
    _titleController.text = _worksOrderPrefix;
    _titleController.selection = TextSelection.collapsed(offset: _worksOrderPrefix.length);
    _descController.clear();
  }

  @override
  void dispose() {
    _titleController.dispose();
    _descController.dispose();
    super.dispose();
  }

  Future<void> _create() async {
    final user = ref.read(authProvider).user;
    if (user == null) return;

    final plantId = user.plantId;
    final departmentId = user.departmentId;
    if (plantId == null || departmentId == null) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Your profile is missing plant or department. Please contact an administrator.',
          ),
        ),
      );
      return;
    }

    resetWorkflowFormProviders(ref);

    await ref.read(assessmentWorkflowProvider.notifier).startNewDraft(
          companyId: user.companyId,
          plantId: plantId,
          departmentId: departmentId,
          title: _titleController.text.trim(),
          jobDescription: _descController.text.trim(),
        );

    final draft = ref.read(assessmentWorkflowProvider);
    if (draft != null && mounted) {
      context.go('/jsas/workflow/${draft.localId}/job-information');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: const AppNavigationBar(title: 'New Assessment'),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            TextField(
              controller: _titleController,
              decoration: const InputDecoration(
                labelText: 'Works order number',
                hintText: '12345',
              ),
              textCapitalization: TextCapitalization.characters,
              inputFormatters: const [_WorksOrderPrefixFormatter()],
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _descController,
              decoration: const InputDecoration(
                labelText: 'Short description of the task',
              ),
              maxLines: 4,
            ),
            const Spacer(),
            ElevatedButton(onPressed: _create, child: const Text('Start Workflow')),
          ],
        ),
      ),
    );
  }
}

class _WorksOrderPrefixFormatter extends TextInputFormatter {
  const _WorksOrderPrefixFormatter();

  static const _prefix = 'WO-';

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    var text = newValue.text.toUpperCase();

    if (!text.startsWith(_prefix)) {
      final suffix = text.replaceFirst(RegExp(r'^WO-?'), '');
      text = '$_prefix$suffix';
    }

    var selection = newValue.selection;
    if (selection.start < _prefix.length) {
      selection = TextSelection.collapsed(offset: _prefix.length);
    } else if (selection.end < _prefix.length) {
      selection = TextSelection.collapsed(offset: _prefix.length);
    }

    return TextEditingValue(text: text, selection: selection);
  }
}
