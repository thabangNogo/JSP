import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/safety_providers.dart';

final injuryDetailProvider = FutureProvider.family<Map<String, dynamic>, String>((ref, id) async {
  return ref.read(safetyRemoteDataSourceProvider).getInjuryById(id);
});

class InjuryDetailScreen extends ConsumerWidget {
  const InjuryDetailScreen({super.key, required this.injuryId});

  final String injuryId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detail = ref.watch(injuryDetailProvider(injuryId));

    return Scaffold(
      appBar: AppBar(title: const Text('Injury Details')),
      body: detail.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(injuryDetailProvider(injuryId)),
        ),
        data: (injury) {
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              _SectionHeader(title: injury['employeeName'] as String? ?? 'Injury'),
              _DetailRow('Status', injury['status'] as String? ?? ''),
              _DetailRow('Type', injury['injuryType'] as String? ?? ''),
              _DetailRow('Body Part', injury['bodyPartInjured'] as String? ?? ''),
              _DetailRow('Department', injury['department'] as String? ?? ''),
              _DetailRow('Location', injury['location'] as String? ?? ''),
              _DetailRow('Section', injury['section'] as String? ?? ''),
              _DetailRow('Occurred', injury['injuryOccurredAt']?.toString() ?? ''),
              _DetailRow('Submitted', injury['submittedAt']?.toString() ?? ''),
              const Divider(height: 32),
              const Text('Incident Description', style: TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              Text(injury['incidentDescription'] as String? ?? ''),
              if (injury['immediateActionTaken'] != null) ...[
                const SizedBox(height: 16),
                const Text('Immediate Action', style: TextStyle(fontWeight: FontWeight.bold)),
                Text(injury['immediateActionTaken'] as String),
              ],
              if (injury['rootCause'] != null) ...[
                const SizedBox(height: 16),
                const Text('Root Cause', style: TextStyle(fontWeight: FontWeight.bold)),
                Text(injury['rootCause'] as String),
              ],
              if (injury['correctiveAction'] != null) ...[
                const SizedBox(height: 16),
                const Text('Corrective Action', style: TextStyle(fontWeight: FontWeight.bold)),
                Text(injury['correctiveAction'] as String),
              ],
              if (injury['lostTimeDays'] != null)
                _DetailRow('Lost Time Days', '${injury['lostTimeDays']}'),
              if (injury['witnesses'] != null) ...[
                const SizedBox(height: 16),
                const Text('Witnesses', style: TextStyle(fontWeight: FontWeight.bold)),
                Text(injury['witnesses'] as String),
              ],
            ],
          );
        },
      ),
    );
  }
}

class _SectionHeader extends StatelessWidget {
  const _SectionHeader({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Text(title, style: Theme.of(context).textTheme.headlineSmall),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow(this.label, this.value);

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
            width: 120,
            child: Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}
