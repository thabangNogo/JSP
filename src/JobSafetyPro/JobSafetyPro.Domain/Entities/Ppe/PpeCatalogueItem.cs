using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Enums;

namespace JobSafetyPro.Domain.Entities.Ppe;

public class PpeCatalogueItem : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;

    public PpeCategory Category { get; set; }

    public int QuantityInStock { get; set; }

    public int MinimumStockLevel { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PpeRequest> Requests { get; set; } = new List<PpeRequest>();
}
