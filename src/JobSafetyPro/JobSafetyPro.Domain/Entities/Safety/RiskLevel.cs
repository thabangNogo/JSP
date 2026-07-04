using JobSafetyPro.Domain.Common;

namespace JobSafetyPro.Domain.Entities.Safety;

public class RiskLevel : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int NumericValue { get; set; }

    public string ColorHex { get; set; } = string.Empty;

    public ICollection<Hazard> Hazards { get; set; } = new List<Hazard>();
}
