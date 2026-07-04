import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/jsa_providers.dart';

class AssessmentHistoryScreen extends ConsumerWidget {
  const AssessmentHistoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final history = ref.watch(jsaHistoryProvider);
    return Scaffold(
      appBar: const AppNavigationBar(title: 'Assessment History'),
      body: history.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(jsaHistoryProvider),
        ),
        data: (items) => ListView.builder(
          itemCount: items.length,
          itemBuilder: (_, i) => ListTile(
            title: Text(items[i].title),
            subtitle: Text('${items[i].status} • ${items[i].jobDescription}'),
          ),
        ),
      ),
    );
  }
}
