namespace JobSafetyPro.Domain.Enums;

public enum RootCauseCategory
{
    HumanError = 0,
    ProcedureFailure = 1,
    EquipmentFailure = 2,
    TrainingGap = 3,
    Housekeeping = 4,
    Environmental = 5,
    DesignDeficiency = 6,
    CommunicationFailure = 7,
    Other = 8,
}
