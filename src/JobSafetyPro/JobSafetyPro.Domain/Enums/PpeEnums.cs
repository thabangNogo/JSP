namespace JobSafetyPro.Domain.Enums;

public enum PpeCategory
{
    SafetyHelmet = 1,
    SafetyBoots = 2,
    SafetyGlasses = 3,
    EarPlugs = 4,
    EarMuffs = 5,
    Respirator = 6,
    DustMask = 7,
    FaceShield = 8,
    ReflectiveVest = 9,
    Overalls = 10,
    LeatherGloves = 11,
    RubberGloves = 12,
    FallArrestHarness = 13,
    WeldingHelmet = 14,
    FireResistantClothing = 15,
    SafetyGoggles = 16,
    Other = 99,
}

public enum PpeRequestPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4,
}

public enum PpeRequestStatus
{
    Requested = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Preparing = 5,
    Dispatched = 6,
    Collected = 7,
    Completed = 8,
    Archived = 9,
    Cancelled = 10,
}
