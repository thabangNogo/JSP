using JSP.Domain.Common;
using JSP.Domain.Entities.Organization;

namespace JSP.Domain.Entities.Safety;

public class GoldenRule : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid? PlantId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;

    public Plant? Plant { get; set; }
}
