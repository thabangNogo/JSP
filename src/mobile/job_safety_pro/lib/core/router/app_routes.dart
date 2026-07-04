class AppRoutes {
  static const splash = '/';
  static const login = '/login';
  static const dashboard = '/dashboard';
  static const jsaList = '/jsas';
  static const jsaDetail = '/jsas/detail/:localId';
  static const jsaHistory = '/jsas/history';

  static String jsaDetailPath(String localId) => '/jsas/detail/$localId';
  static const jsaNew = '/jsas/new';
  static const jsaWorkflow = '/jsas/workflow/:draftId';
  static const jsaStepQuickAssessment = '/jsas/workflow/:draftId/quick-assessment';
  static const jsaStepJobInformation = '/jsas/workflow/:draftId/job-information';
  static const jsaStepHazards = '/jsas/workflow/:draftId/hazards';
  static const jsaStepRisks = '/jsas/workflow/:draftId/risks';
  static const jsaStepSignOff = '/jsas/workflow/:draftId/sign-off';
  static const qrScanner = '/scanner';
  static const profile = '/profile';
  static const profileEdit = '/profile/edit';
  static const settings = '/settings';
  static const syncStatus = '/sync';
  static const stopUnsafeWork = '/safety/stop-unsafe-work';
  static const reportNearMiss = '/safety/near-miss';
  static const notifications = '/safety/notifications';
  static const injuries = '/safety/injuries';
  static const captureInjury = '/safety/injuries/capture';
  static const injuryDetail = '/safety/injuries/:id';

  static String injuryDetailPath(String id) => '/safety/injuries/$id';
}
