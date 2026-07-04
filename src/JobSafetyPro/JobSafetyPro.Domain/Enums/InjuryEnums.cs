namespace JobSafetyPro.Domain.Enums;

public enum InjuryType
{
    FirstAidInjury = 1,
    MedicalTreatmentInjury = 2,
    LostTimeInjury = 3,
    Fatality = 4,
}

public enum BodyPartInjured
{
    Head = 1,
    Eye = 2,
    Face = 3,
    Neck = 4,
    Shoulder = 5,
    Arm = 6,
    Hand = 7,
    Finger = 8,
    Chest = 9,
    Back = 10,
    Leg = 11,
    Knee = 12,
    Foot = 13,
    Multiple = 14,
    Other = 15,
}

public enum InjuryStatus
{
    Open = 1,
    Closed = 2,
    UnderInvestigation = 3,
    MedicalTreatment = 4,
    ReturnToWork = 5,
}
