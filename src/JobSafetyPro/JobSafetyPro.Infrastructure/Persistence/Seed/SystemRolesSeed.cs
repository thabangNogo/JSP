using JobSafetyPro.Application.Constants;
using JobSafetyPro.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class SystemRolesSeed
{
    private static readonly (string Name, string Description)[] SystemRoles =
    {
        (AppRoles.Administrator, "System administrator"),
        (AppRoles.HseManager, "HSE manager"),
        (AppRoles.SafetyManager, "Safety manager"),
        (AppRoles.SafetyOfficer, "Safety officer"),
        (AppRoles.Supervisor, "Supervisor"),
        (AppRoles.Operator, "Operator"),
        (AppRoles.Auditor, "Auditor"),
    };

    public static async Task EnsureAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var existing = await context.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
        var added = 0;

        foreach (var (name, description) in SystemRoles)
        {
            if (existing.Contains(name)) continue;

            context.Roles.Add(new Role
            {
                Name = name,
                Description = description,
                IsSystemRole = true,
                CreatedBy = "system",
                CreatedDate = now,
            });
            added++;
        }

        if (added == 0) return;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Added {Count} missing system role(s).", added);
    }
}
