import 'dart:async';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../safety/data/datasources/safety_remote_datasource.dart';
import '../services/notification_service.dart';

/// Handles PPE push alerts via FCM token registration (when configured) and
/// foreground polling against the notifications API.
class FcmPushService {
  FcmPushService({
    required SafetyRemoteDataSource remote,
    NotificationService? notifications,
  })  : _remote = remote,
        _notifications = notifications ?? NotificationService();

  final SafetyRemoteDataSource _remote;
  final NotificationService _notifications;
  Timer? _pollTimer;
  static const _lastSeenKey = 'ppe_notification_last_seen_ids';

  static const _ppeTypes = {
    'PpeRequested',
    'PpeApproved',
    'PpeRejected',
    'PpePreparing',
    'PpeDispatched',
    'PpeReadyForCollection',
    'PpeCollected',
    'PpeStockLow',
    'PpeOverdue',
    'PpeWaitingEscalation',
  };

  Future<void> init() async {
    await _registerDeviceIfPossible();
    _pollTimer?.cancel();
    _pollTimer = Timer.periodic(const Duration(minutes: 2), (_) => _pollPpeNotifications());
    await _pollPpeNotifications();
  }

  void dispose() => _pollTimer?.cancel();

  Future<void> _registerDeviceIfPossible() async {
    try {
      final token = await _resolveFcmToken();
      if (token == null || token.isEmpty) return;
      final platform = Platform.isIOS
          ? 'ios'
          : Platform.isAndroid
              ? 'android'
              : 'unknown';
      await _remote.registerDevice(token, platform);
    } catch (e, st) {
      debugPrint('FCM device registration skipped: $e\n$st');
    }
  }

  /// Override via Firebase Messaging when `google-services.json` is configured.
  Future<String?> _resolveFcmToken() async {
    return null;
  }

  Future<void> _pollPpeNotifications() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final seen = prefs.getStringList(_lastSeenKey)?.toSet() ?? {};
      final items = await _remote.getNotifications();
      final updatedSeen = {...seen};

      for (final n in items) {
        final id = n['id']?.toString();
        final type = n['notificationType']?.toString() ?? '';
        final isRead = n['isRead'] == true;
        if (id == null || isRead || !_ppeTypes.contains(type) || seen.contains(id)) {
          continue;
        }
        final title = n['title']?.toString() ?? 'PPE Update';
        final message = n['message']?.toString() ?? '';
        await _notifications.showPpeNotification(title, message);
        updatedSeen.add(id);
      }

      await prefs.setStringList(_lastSeenKey, updatedSeen.take(200).toList());
    } catch (e) {
      debugPrint('PPE notification poll failed: $e');
    }
  }
}
