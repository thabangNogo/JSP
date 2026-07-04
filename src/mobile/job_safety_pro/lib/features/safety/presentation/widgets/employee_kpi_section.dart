import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/safety_providers.dart';

class EmployeeKpiSection extends ConsumerWidget {
  const EmployeeKpiSection({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final kpis = ref.watch(employeeKpiProvider);

    if (kpis.isLoading) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 16),
        child: Center(child: CircularProgressIndicator()),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Padding(
          padding: const EdgeInsets.only(top: 8, bottom: 8),
          child: Text(
            'Safety KPIs',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
          ),
        ),
        Row(
          children: [
            Expanded(
              child: _KpiTile(
                label: 'Near Misses\n(this month)',
                value: '${kpis.nearMissesThisMonth}',
                color: Colors.orange,
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: _KpiTile(
                label: 'Draft\nAssessments',
                value: '${kpis.draftAssessments}',
                color: Colors.blue,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: _KpiTile(
                label: 'Submitted\nAssessments',
                value: '${kpis.submittedAssessments}',
                color: Colors.amber,
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: _KpiTile(
                label: 'Approved\nAssessments',
                value: '${kpis.approvedAssessments}',
                color: Colors.green,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: _KpiTile(
                label: 'Participation\nScore',
                value: '${kpis.participationScore}',
                color: Colors.teal,
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class _KpiTile extends StatelessWidget {
  const _KpiTile({required this.label, required this.value, required this.color});

  final String label;
  final String value;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          children: [
            Text(value, style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: color)),
            const SizedBox(height: 4),
            Text(label, textAlign: TextAlign.center, style: const TextStyle(fontSize: 12)),
          ],
        ),
      ),
    );
  }
}
