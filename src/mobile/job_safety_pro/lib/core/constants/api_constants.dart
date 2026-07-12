class ApiConstants {
  static const authLogin = '/auth/login';
  static const authRefresh = '/auth/refresh';
  static const authLogout = '/auth/logout';
  static const authMe = '/auth/me';
  static const jsas = '/jsas';
  static const jsaDrafts = '/jsas/drafts';
  static const jsaSummaries = '/jsas/summaries';
  static const jsaReports = '/jsas/reports';
  static const employeeProfileMe = '/employee-profiles/me';
  static const riskLevels = '/risk-levels';
  static const incidents = '/incidents';
  static const nearMisses = '/near-misses';
  static const stopUnsafeWork = '/stop-unsafe-work';
  static const notifications = '/notifications';
  static const employeeSafetyKpis = '/safety-kpis/employee';
  static const managerSafetyKpis = '/safety-kpis/manager';
  static const injuryFreeDays = '/dashboard/injury-free-days';
  static const injuries = '/injuries';
  static const ppeMy = '/ppe/requests/my';
  static const workLookups = '/work-lookups';
  static const attachments = '/attachments';
}

class HiveBoxes {
  static const assessmentDrafts = 'assessment_drafts';
  static const deletedAssessmentIds = 'deleted_assessment_ids';
  static const syncQueue = 'sync_queue';
  static const cachedJsas = 'cached_jsas';
  static const userProfile = 'user_profile';
}

class StorageKeys {
  static const accessToken = 'access_token';
  static const refreshToken = 'refresh_token';
  static const tokenExpiry = 'token_expiry';
  static const userEmail = 'user_email';
  static const notificationsEnabled = 'notifications_enabled';
  static const offlineMode = 'offline_mode';
}
