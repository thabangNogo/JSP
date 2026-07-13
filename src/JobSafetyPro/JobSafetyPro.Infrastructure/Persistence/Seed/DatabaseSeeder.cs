using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync();

        var now = DateTime.UtcNow;
        if (!await context.WorkDepartments.AnyAsync())
        {
            context.WorkDepartments.AddRange(WorkMasterDataSeed.CreateDepartments(now));
            context.WorkLocations.AddRange(WorkMasterDataSeed.CreateLocations(now));
            context.WorkSections.AddRange(WorkMasterDataSeed.CreateSections(now));
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded work department, location and section master data.");
        }

        if (!await context.PpeCatalogueItems.AnyAsync())
        {
            context.PpeCatalogueItems.AddRange(PpeCatalogueSeed.CreateDefaultItems(now));
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded PPE catalogue items.");
        }

        var legacyCompany = await context.Companies.FirstOrDefaultAsync(c => c.Name == "Demo Manufacturing Co");
        if (legacyCompany != null)
        {
            legacyCompany.Name = "Astec Industries";
            legacyCompany.Code = "ASTEC";
            legacyCompany.ModifiedDate = now;
            legacyCompany.ModifiedBy = "system";
            await context.SaveChangesAsync();
            logger.LogInformation("Renamed company from Demo Manufacturing Co to Astec Industries.");
        }

        if (!await context.Companies.AnyAsync())
        {
            await CreateInitialCompanyAsync(context, passwordHasher, logger, now);
        }

        await SystemRolesSeed.EnsureAsync(context, logger);
        await EmployeeSampleSeed.SeedIfNeededAsync(context, passwordHasher, logger);
        await SafetyLeadSampleSeed.SeedIfNeededAsync(context, passwordHasher, logger);
    }

    private static async Task CreateInitialCompanyAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        DateTime now)
    {
        var company = new Company
        {
            Name = "Astec Industries",
            Code = "ASTEC",
            CreatedBy = "system",
            CreatedDate = now
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var plant = new Plant
        {
            CompanyId = company.Id,
            Name = "Main Plant",
            Code = "PLT01",
            TimeZone = "UTC",
            CreatedBy = "system",
            CreatedDate = now
        };
        context.Plants.Add(plant);
        await context.SaveChangesAsync();

        var department = new Department
        {
            PlantId = plant.Id,
            Name = "Production",
            Code = "PROD",
            CreatedBy = "system",
            CreatedDate = now
        };
        context.Departments.Add(department);

        context.Roles.AddRange(
            new Role { Name = AppRoles.Administrator, Description = "System administrator", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.HseManager, Description = "HSE manager", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.SafetyManager, Description = "Safety manager", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.SafetyOfficer, Description = "Safety officer", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.Supervisor, Description = "Supervisor", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.Operator, Description = "Operator", IsSystemRole = true, CreatedBy = "system", CreatedDate = now },
            new Role { Name = AppRoles.Auditor, Description = "Auditor", IsSystemRole = true, CreatedBy = "system", CreatedDate = now });

        context.RiskLevels.AddRange(
            new RiskLevel { Code = "LOW", Name = "Low", NumericValue = 1, ColorHex = "#4CAF50", CreatedBy = "system", CreatedDate = now },
            new RiskLevel { Code = "MED", Name = "Medium", NumericValue = 2, ColorHex = "#FF9800", CreatedBy = "system", CreatedDate = now },
            new RiskLevel { Code = "HIGH", Name = "High", NumericValue = 3, ColorHex = "#F44336", CreatedBy = "system", CreatedDate = now },
            new RiskLevel { Code = "CRIT", Name = "Critical", NumericValue = 4, ColorHex = "#9C27B0", CreatedBy = "system", CreatedDate = now });

        await context.SaveChangesAsync();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.Administrator);
        var adminUser = new User
        {
            CompanyId = company.Id,
            PlantId = plant.Id,
            DepartmentId = department.Id,
            Email = "admin@jsp.demo",
            PasswordHash = passwordHasher.HashPassword("Admin@123"),
            FirstName = "System",
            LastName = "Administrator",
            EmployeeNumber = "EMP-001",
            CreatedBy = "system",
            CreatedDate = now
        };
        adminUser.UserRoles.Add(new UserRole
        {
            RoleId = adminRole.Id,
            UserId = adminUser.Id,
            CreatedBy = "system",
            CreatedDate = now
        });
        context.Users.Add(adminUser);

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded with demo company and admin user (admin@jsp.demo / Admin@123)");
    }
}
