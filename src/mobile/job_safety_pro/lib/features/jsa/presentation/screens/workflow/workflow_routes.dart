/// QJSA workflow step paths (5 steps).
class WorkflowRoutes {
  static const segments = [
    'job-information',
    'quick-assessment',
    'hazards',
    'risks',
    'sign-off',
  ];

  static int indexFromPath(String path) {
    for (var i = 0; i < segments.length; i++) {
      if (path.contains('/${segments[i]}')) return i;
    }
    return 0;
  }

  static String path(String draftId, int stepIndex) {
    final index = stepIndex.clamp(0, segments.length - 1);
    return '/jsas/workflow/$draftId/${segments[index]}';
  }
}
