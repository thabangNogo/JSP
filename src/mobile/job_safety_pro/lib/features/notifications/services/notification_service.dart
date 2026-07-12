import 'package:flutter_local_notifications/flutter_local_notifications.dart';

class NotificationService {
  NotificationService() : _plugin = FlutterLocalNotificationsPlugin();

  final FlutterLocalNotificationsPlugin _plugin;

  Future<void> init() async {
    const android = AndroidInitializationSettings('@mipmap/ic_launcher');
    const ios = DarwinInitializationSettings();
    await _plugin.initialize(
      const InitializationSettings(android: android, iOS: ios),
    );
  }

  Future<void> showAssessmentReminder(String title, String body) async {
    const details = NotificationDetails(
      android: AndroidNotificationDetails(
        'jsp_assessments',
        'Assessment Reminders',
        importance: Importance.high,
        priority: Priority.high,
      ),
      iOS: DarwinNotificationDetails(),
    );
    await _plugin.show(
      DateTime.now().millisecondsSinceEpoch ~/ 1000,
      title,
      body,
      details,
    );
  }

  Future<void> showSyncComplete(int count) async {
    await showAssessmentReminder(
      'Sync Complete',
      '$count assessment(s) synced successfully.',
    );
  }

  Future<void> showPpeNotification(String title, String body) async {
    const details = NotificationDetails(
      android: AndroidNotificationDetails(
        'jsp_ppe',
        'PPE Updates',
        importance: Importance.high,
        priority: Priority.high,
      ),
      iOS: DarwinNotificationDetails(),
    );
    await _plugin.show(
      DateTime.now().millisecondsSinceEpoch ~/ 1000,
      title,
      body,
      details,
    );
  }
}
