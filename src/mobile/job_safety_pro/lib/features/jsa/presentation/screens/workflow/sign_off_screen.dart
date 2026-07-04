import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:signature/signature.dart';
import '../../../../../core/router/app_routes.dart';
import '../../../data/utils/signature_storage.dart';
import '../../../domain/services/assessment_validation_service.dart';
import '../../../domain/services/draft_auto_save_service.dart';
import '../../../../profile/presentation/providers/employee_profile_provider.dart';
import '../../providers/jsa_providers.dart';
import '../../providers/workflow_form_reset.dart';
import 'workflow_step_mixin.dart';

class SignOffScreen extends ConsumerStatefulWidget {
  const SignOffScreen({super.key, required this.draftId});
  final String draftId;

  @override
  ConsumerState<SignOffScreen> createState() => _SignOffScreenState();
}

class _SignOffScreenState extends ConsumerState<SignOffScreen> with WorkflowStepMixin {
  final _signatureController = SignatureController(
    penStrokeWidth: 3,
    penColor: Colors.black,
    exportBackgroundColor: Colors.white,
  );

  @override
  String get draftId => widget.draftId;

  @override
  void dispose() {
    _signatureController.dispose();
    super.dispose();
  }

  Future<String?> _exportSignature() async {
    if (_signatureController.isEmpty) return null;

    try {
      final bytes = await _signatureController.toPngBytes();
      if (bytes == null || bytes.isEmpty) return null;
      return SignatureStorage.savePng(widget.draftId, bytes);
    } catch (e, st) {
      debugPrint('Signature export failed: $e\n$st');
      return null;
    }
  }

  Future<void> _submit() async {
    final draft = ref.read(assessmentWorkflowProvider);
    if (draft == null) return;

    final profile = ref.read(employeeProfileProvider).profile;
    final hasSignature = !_signatureController.isEmpty ||
        (draft.signaturePath != null &&
            await SignatureStorage.exists(draft.signaturePath!));

    final validation = ref.read(assessmentValidationServiceProvider).validateSignOff(
          hasSignature: hasSignature,
          profile: profile,
        );
    if (!validation.isValid) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(validation.message ?? 'Validation failed')),
        );
      }
      return;
    }

    String? signaturePath = await _exportSignature();
    if (signaturePath == null && _signatureController.isEmpty) {
      if (draft.signaturePath != null && await SignatureStorage.exists(draft.signaturePath!)) {
        signaturePath = draft.signaturePath;
      }
    }

    if (signaturePath == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Could not save signature. Please sign again and retry.'),
          ),
        );
      }
      return;
    }

    ref.read(draftAutoSaveServiceProvider).stop();

    await ref.read(assessmentWorkflowProvider.notifier).updateDraft(
          draft.copyWith(
            signaturePath: signaturePath,
            currentStep: 4,
            signOffName: profile?.name,
            signOffSurname: profile?.surname,
            signOffCompanyNumber: profile?.companyNumber,
            signOffOccupation: profile?.occupation,
          ),
        );

    try {
      await ref.read(assessmentWorkflowProvider.notifier).submit();

      if (mounted) {
        ref.read(draftAutoSaveServiceProvider).stop();
        resetWorkflowFormProviders(ref);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Assessment submitted successfully')),
        );
        context.go(AppRoutes.dashboard);
      }
    } on StateError catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.message)),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Submit failed: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final draft = ref.watch(assessmentWorkflowProvider);
    final profile = ref.watch(employeeProfileProvider).profile;

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text('Step 5: Review and sign off on the assessment.'),
          const SizedBox(height: 12),
          if (draft != null) ...[
            Text('Job: ${draft.title}', style: const TextStyle(fontWeight: FontWeight.bold)),
            if (draft.department.isNotEmpty ||
                draft.location.isNotEmpty ||
                draft.section.isNotEmpty) ...[
              const SizedBox(height: 8),
              const Text('Job information', style: TextStyle(fontWeight: FontWeight.w600)),
              if (draft.department.isNotEmpty) Text('Department: ${draft.department}'),
              if (draft.location.isNotEmpty) Text('Location: ${draft.location}'),
              if (draft.section.isNotEmpty) Text('Section: ${draft.section}'),
            ],
            Text('Hazards: ${draft.hazards.length} • Controls: ${draft.controls.length}'),
          ],
          if (profile != null && profile.isComplete) ...[
            const SizedBox(height: 12),
            const Text('Sign-off (from your profile)', style: TextStyle(fontWeight: FontWeight.w600)),
            Text('${profile.name} ${profile.surname}'),
            Text('Company #: ${profile.companyNumber}'),
            Text('Occupation: ${profile.occupation}'),
          ] else ...[
            const SizedBox(height: 12),
            const Text(
              'Complete your employee profile before submitting.',
              style: TextStyle(color: Colors.orange),
            ),
          ],
          const SizedBox(height: 16),
          Expanded(
            child: Container(
              decoration: BoxDecoration(
                border: Border.all(color: Colors.grey),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Signature(controller: _signatureController, backgroundColor: Colors.white),
            ),
          ),
          const SizedBox(height: 8),
          OutlinedButton(
            onPressed: () => _signatureController.clear(),
            child: const Text('Clear Signature'),
          ),
          const SizedBox(height: 8),
          ElevatedButton(onPressed: _submit, child: const Text('Sign Off & Submit')),
        ],
      ),
    );
  }
}
