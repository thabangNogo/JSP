using JobSafetyPro.Domain.Entities.Ppe;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class PpeCatalogueSeed
{
    public static IReadOnlyList<PpeCatalogueItem> CreateDefaultItems(DateTime now) => new List<PpeCatalogueItem>
    {
        Item("Safety Helmet", PpeCategory.SafetyHelmet, 50, 10, now),
        Item("Safety Boots", PpeCategory.SafetyBoots, 80, 15, now),
        Item("Safety Glasses", PpeCategory.SafetyGlasses, 120, 20, now),
        Item("Ear Plugs", PpeCategory.EarPlugs, 200, 40, now),
        Item("Ear Muffs", PpeCategory.EarMuffs, 40, 8, now),
        Item("Respirator", PpeCategory.Respirator, 30, 6, now),
        Item("Dust Mask", PpeCategory.DustMask, 150, 30, now),
        Item("Face Shield", PpeCategory.FaceShield, 35, 7, now),
        Item("Reflective Vest", PpeCategory.ReflectiveVest, 60, 12, now),
        Item("Overalls", PpeCategory.Overalls, 45, 10, now),
        Item("Leather Gloves", PpeCategory.LeatherGloves, 100, 20, now),
        Item("Rubber Gloves", PpeCategory.RubberGloves, 90, 18, now),
        Item("Fall Arrest Harness", PpeCategory.FallArrestHarness, 25, 5, now),
        Item("Welding Helmet", PpeCategory.WeldingHelmet, 20, 4, now),
        Item("Fire Resistant Clothing", PpeCategory.FireResistantClothing, 30, 6, now),
        Item("Safety Goggles", PpeCategory.SafetyGoggles, 70, 14, now),
    };

    private static PpeCatalogueItem Item(string name, PpeCategory category, int stock, int min, DateTime now) =>
        new()
        {
            ItemName = name,
            Category = category,
            QuantityInStock = stock,
            MinimumStockLevel = min,
            Description = $"{name} for workplace safety compliance.",
            IsActive = true,
            CreatedDate = now,
            CreatedBy = "system",
        };
}
