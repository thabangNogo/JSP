namespace JobSafetyPro.Application.Constants;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string HseManager = "HSE Manager";
    public const string SafetyManager = "Safety Manager";
    public const string SafetyOfficer = "Safety Officer";
    public const string Supervisor = "Supervisor";
    public const string Operator = "Operator";
    public const string Auditor = "Auditor";
}

public static class RelatedEntityTypes
{
    public const string Incident = "Incident";
    public const string NearMiss = "NearMiss";
    public const string JobSafetyAssessment = "JobSafetyAssessment";
    public const string StopUnsafeWork = "StopUnsafeWork";
    public const string Injury = "Injury";
    public const string CorrectiveAction = "CorrectiveAction";
}
