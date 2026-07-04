import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../shared/enums/assessment_status.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../../data/models/jsa_models.dart';
import '../../domain/services/assessment_summary_builder.dart';
import '../providers/jsa_providers.dart';
import '../providers/workflow_form_reset.dart';
import 'workflow/workflow_routes.dart';

class AssessmentDetailScreen extends ConsumerWidget {
  const AssessmentDetailScreen({super.key, required this.localId});

  final String localId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailAsync = ref.watch(assessmentDetailProvider(localId));
    final riskLevelsAsync = ref.watch(riskLevelsProvider);

    return Scaffold(
      appBar: const AppNavigationBar(title: 'Assessment Details'),
      body: detailAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(assessmentDetailProvider(localId)),
        ),
        data: (detail) {
          final riskLevelNames = riskLevelsAsync.maybeWhen(
            data: (levels) => {
              for (final level in levels) level.id: level.name,
            },
            orElse: () => <String, String>{},
          );

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              _StatusHeader(detail: detail),
              const SizedBox(height: 16),
              _DetailSection(
                title: 'Overview',
                children: [
                  _DetailRow(
                    label: 'Date & time',
                    value: _formatDateTime(detail.lastUpdated),
                  ),
                  if (detail.status.isDraft)
                    _DetailRow(
                      label: 'Progress',
                      value: 'Step ${detail.currentStep + 1}',
                    ),
                  if (detail.jobDescription.isNotEmpty)
                    _DetailRow(label: 'Job description', value: detail.jobDescription),
                ],
              ),
              _DetailSection(
                title: 'Job information',
                children: [
                  _DetailRow(label: 'Department', value: detail.department),
                  _DetailRow(label: 'Location', value: detail.location),
                  _DetailRow(label: 'Section', value: detail.section),
                ],
              ),
              if (detail.quickAssessmentSummary != null &&
                  detail.quickAssessmentSummary!.trim().isNotEmpty)
                _DetailSection(
                  title: 'Quick Assessment',
                  children: [
                    Text(detail.quickAssessmentSummary!),
                  ],
                ),
              if (detail.observeStopNotes.trim().isNotEmpty)
                _DetailSection(
                  title: 'Observe & Stop',
                  children: [Text(detail.observeStopNotes)],
                ),
              if (detail.conversationNotes.trim().isNotEmpty)
                _DetailSection(
                  title: 'Conversation',
                  children: [Text(detail.conversationNotes)],
                ),
              _DetailSection(
                title: 'Sign-off',
                children: [
                  _DetailRow(label: 'Name', value: detail.signOffName),
                  _DetailRow(label: 'Surname', value: detail.signOffSurname),
                  _DetailRow(label: 'Company number', value: detail.signOffCompanyNumber),
                  _DetailRow(label: 'Occupation', value: detail.signOffOccupation),
                ],
              ),
              if (detail.hazards.isNotEmpty)
                _DetailSection(
                  title: 'Hazards (${detail.hazards.length})',
                  children: detail.hazards.map((hazard) {
                    final riskName = _riskLevelLabel(hazard.riskLevelId, riskLevelNames);
                    final residualName =
                        _riskLevelLabel(hazard.residualRiskLevelId, riskLevelNames);
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            hazard.description,
                            style: Theme.of(context).textTheme.bodyLarge,
                          ),
                          if (hazard.consequence.isNotEmpty) ...[
                            const SizedBox(height: 4),
                            Text(
                              'Consequence: ${hazard.consequence}',
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                          ],
                          if (riskName != null) ...[
                            const SizedBox(height: 4),
                            Text('Risk level: $riskName'),
                          ],
                          if (residualName != null && residualName != riskName)
                            Text('Residual risk: $residualName'),
                        ],
                      ),
                    );
                  }).toList(),
                ),
              if (detail.controls.isNotEmpty)
                _DetailSection(
                  title: 'Controls (${detail.controls.length})',
                  children: detail.controls.map((control) {
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            control.description,
                            style: Theme.of(context).textTheme.bodyLarge,
                          ),
                          const SizedBox(height: 4),
                          Text('Hierarchy: ${control.hierarchyOfControl}'),
                          Text(
                            control.isImplemented ? 'Implemented' : 'Not yet implemented',
                            style: TextStyle(
                              color: control.isImplemented ? Colors.green.shade700 : Colors.orange.shade800,
                            ),
                          ),
                        ],
                      ),
                    );
                  }).toList(),
                ),
              if (detail.signaturePath != null && detail.signaturePath!.trim().isNotEmpty)
                _DetailSection(
                  title: 'Signature',
                  children: [_SignaturePreview(path: detail.signaturePath!)],
                ),
              _DetailSection(
                title: 'Assessment summary',
                children: [
                  SelectableText(
                    AssessmentSummaryBuilder.build(detail),
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                ],
              ),
              const SizedBox(height: 80),
            ],
          );
        },
      ),
      floatingActionButton: detailAsync.maybeWhen(
        data: (detail) => detail.canContinueEditing
            ? FloatingActionButton.extended(
                onPressed: () => _continueEditing(context, ref, detail),
                icon: const Icon(Icons.edit),
                label: const Text('Continue'),
              )
            : null,
        orElse: () => null,
      ),
    );
  }

  void _continueEditing(BuildContext context, WidgetRef ref, AssessmentDetailModel detail) {
    resetWorkflowFormProviders(ref);
    ref.read(assessmentWorkflowProvider.notifier).loadDraft(detail.localId);
    context.go(WorkflowRoutes.path(detail.localId, detail.currentStep));
  }

  static String? _riskLevelLabel(String? id, Map<String, String> names) {
    if (id == null || id.isEmpty) return null;
    return names[id] ?? id;
  }

  static String _formatDateTime(DateTime? date) {
    if (date == null) return '—';
    final d = date.toLocal();
    final hour = d.hour.toString().padLeft(2, '0');
    final minute = d.minute.toString().padLeft(2, '0');
    return '${d.day}/${d.month}/${d.year} $hour:$minute';
  }
}

class _StatusHeader extends StatelessWidget {
  const _StatusHeader({required this.detail});

  final AssessmentDetailModel detail;

  @override
  Widget build(BuildContext context) {
    final statusColor = switch (detail.status) {
      AssessmentStatus.draft => Colors.grey.shade700,
      AssessmentStatus.submitted => Colors.blue.shade700,
      AssessmentStatus.approved => Colors.green.shade700,
      AssessmentStatus.rejected => Colors.red.shade700,
    };

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          detail.title.isEmpty ? 'Untitled assessment' : detail.title,
          style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
          decoration: BoxDecoration(
            color: statusColor.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Text(
            _statusLabel(detail.status),
            style: TextStyle(color: statusColor, fontWeight: FontWeight.w600),
          ),
        ),
      ],
    );
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

class _DetailSection extends StatelessWidget {
  const _DetailSection({required this.title, required this.children});

  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 12),
            ...children,
          ],
        ),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow({required this.label, this.value});

  final String label;
  final String? value;

  @override
  Widget build(BuildContext context) {
    final display = (value == null || value!.trim().isEmpty) ? '—' : value!.trim();
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: Theme.of(context).textTheme.labelMedium?.copyWith(
                  color: Colors.grey.shade700,
                ),
          ),
          const SizedBox(height: 2),
          Text(display),
        ],
      ),
    );
  }
}

class _SignaturePreview extends StatelessWidget {
  const _SignaturePreview({required this.path});

  final String path;

  @override
  Widget build(BuildContext context) {
    final file = File(path);
    if (file.existsSync()) {
      return ClipRRect(
        borderRadius: BorderRadius.circular(8),
        child: Image.file(file, height: 120, fit: BoxFit.contain),
      );
    }
    return Text(path, style: TextStyle(color: Colors.grey.shade700, fontSize: 12));
  }
}
