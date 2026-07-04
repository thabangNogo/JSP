using System.Security.Cryptography;
using System.Text;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

/// <summary>
/// Deterministic GUIDs so migrations and runtime seeding insert the same master-data rows.
/// </summary>
public static class WorkMasterDataIds
{
    private static readonly Guid Namespace = new("7c4f2a8e-3b1d-4e9f-a6c5-8d2e1f0a9b3c");

    public static Guid Department(string name) => CreateId("department", name);

    public static Guid Location(string name) => CreateId("location", name);

    public static Guid Section(string name) => CreateId("section", name);

    private static Guid CreateId(string kind, string name)
    {
        var input = $"{Namespace:N}:{kind}:{name}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(hash);
    }
}
