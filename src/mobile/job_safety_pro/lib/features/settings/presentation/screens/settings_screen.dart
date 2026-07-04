import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../../shared/widgets/common_widgets.dart';
import '../providers/settings_provider.dart';

class SettingsScreen extends ConsumerWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final settings = ref.watch(settingsProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Settings')),
      body: settings.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => ErrorView(
          message: dioErrorMessage(e),
          onRetry: () => ref.invalidate(settingsProvider),
        ),
        data: (state) => ListView(
          children: [
            SwitchListTile(
              title: const Text('Push Notifications'),
              subtitle: const Text('Receive assessment and sync alerts'),
              value: state.notificationsEnabled,
              onChanged: (v) => ref.read(settingsProvider.notifier).setNotifications(v),
            ),
            SwitchListTile(
              title: const Text('Prefer Offline Mode'),
              subtitle: const Text('Save assessments locally first'),
              value: state.offlineModePreferred,
              onChanged: (v) => ref.read(settingsProvider.notifier).setOfflineMode(v),
            ),
            const ListTile(
              title: Text('App Version'),
              subtitle: Text('1.0.0'),
            ),
          ],
        ),
      ),
    );
  }
}
