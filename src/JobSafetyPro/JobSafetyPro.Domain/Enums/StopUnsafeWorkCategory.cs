namespace JobSafetyPro.Domain.Enums;

public enum StopUnsafeWorkCategory
{
    UnsafeCondition = 0,
    UnsafeAct = 1,
    EquipmentFailure = 2,
    ElectricalHazard = 3,
    WorkingAtHeight = 4,
    FallingObject = 5,
    FireHazard = 6,
    ChemicalHazard = 7,
    EnvironmentalHazard = 8,
    Other = 9,
}
