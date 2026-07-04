import 'package:shared_preferences/shared_preferences.dart';
import '../../../../core/constants/api_constants.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class SettingsState {
  const SettingsState({
    this.notificationsEnabled = true,
    this.offlineModePreferred = false,
  });

  final bool notificationsEnabled;
  final bool offlineModePreferred;

  SettingsState copyWith({bool? notificationsEnabled, bool? offlineModePreferred}) =>
      SettingsState(
        notificationsEnabled: notificationsEnabled ?? this.notificationsEnabled,
        offlineModePreferred: offlineModePreferred ?? this.offlineModePreferred,
      );
}

class SettingsNotifier extends AsyncNotifier<SettingsState> {
  @override
  Future<SettingsState> build() async {
    final prefs = await SharedPreferences.getInstance();
    return SettingsState(
      notificationsEnabled: prefs.getBool(StorageKeys.notificationsEnabled) ?? true,
      offlineModePreferred: prefs.getBool(StorageKeys.offlineMode) ?? false,
    );
  }

  Future<void> setNotifications(bool enabled) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(StorageKeys.notificationsEnabled, enabled);
    state = AsyncData(state.requireValue.copyWith(notificationsEnabled: enabled));
  }

  Future<void> setOfflineMode(bool enabled) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(StorageKeys.offlineMode, enabled);
    state = AsyncData(state.requireValue.copyWith(offlineModePreferred: enabled));
  }
}

final settingsProvider = AsyncNotifierProvider<SettingsNotifier, SettingsState>(
  SettingsNotifier.new,
);
