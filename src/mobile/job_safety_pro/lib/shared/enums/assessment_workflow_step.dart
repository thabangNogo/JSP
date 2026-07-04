enum AssessmentWorkflowStep {
  jobInformation(1, 'Job Information'),
  quickAssessment(2, 'Quick Assessment'),
  identifyHazards(3, 'Identify Hazards'),
  assessRisks(4, 'Assess Risks'),
  signOff(5, 'Sign Off');

  const AssessmentWorkflowStep(this.stepNumber, this.title);

  final int stepNumber;
  final String title;

  static const int totalSteps = 5;

  static AssessmentWorkflowStep fromIndex(int index) =>
      AssessmentWorkflowStep.values[index.clamp(0, values.length - 1)];
}
