using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class SafetyLeadSampleSeed
{
    private const string DemoPassword = "Admin@123";

    private static readonly (string Email, string First, string Last, string EmpNo, string Role, string Occupation)[] Samples =
    {
        ("hse.manager@jsp.demo", "Henry", "Mbeki", "CN-HSE-001", AppRoles.HseManager, "HSE Manager"),
        ("safety.manager@jsp.demo", "Sarah", "Naidoo", "CN-SM-001", AppRoles.SafetyManager, "Safety Manager"),
        ("safety.officer@jsp.demo", "Paul", "Dlamini", "CN-SO-001", AppRoles.SafetyOfficer, "Safety Officer"),
    };

    public static async Task SeedIfNeededAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var company = await context.Companies.FirstOrDefaultAsync(cancellationToken);
        if (company == null) return;

        var plant = await context.Plants.FirstOrDefaultAsync(p => p.CompanyId == company.Id, cancellationToken);
        if (plant == null) return;

        var orgDept = await context.Departments.FirstOrDefaultAsync(d => d.PlantId == plant.Id, cancellationToken);
        if (orgDept == null) return;

        var maintenanceDept = await context.WorkDepartments
            .FirstOrDefaultAsync(d => d.Name == "Maintenance", cancellationToken);
        if (maintenanceDept == null)
        {
            maintenanceDept = await context.WorkDepartments.FirstOrDefaultAsync(cancellationToken);
            if (maintenanceDept == null) return;
        }

        var roles = await context.Roles.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var passwordHash = passwordHasher.HashPassword(DemoPassword);
        var seeded = 0;

        foreach (var sample in Samples)
        {
            if (await context.Users.AnyAsync(u => u.Email == sample.Email, cancellationToken))
            {
                continue;
            }

            var role = roles.FirstOrDefault(r => r.Name == sample.Role);
            if (role == null)
            {
                logger.LogWarning("Safety lead seed skipped {Email}: role {Role} not found.", sample.Email, sample.Role);
                continue;
            }

            var user = new User
            {
                CompanyId = company.Id,
                PlantId = plant.Id,
                DepartmentId = orgDept.Id,
                Email = sample.Email,
                PasswordHash = passwordHash,
                FirstName = sample.First,
                LastName = sample.Last,
                EmployeeNumber = sample.EmpNo,
                IsActive = true,
                CreatedBy = "system",
                CreatedDate = now,
            };

            user.UserRoles.Add(new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id,
                CreatedBy = "system",
                CreatedDate = now,
            });

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            context.EmployeeProfiles.Add(new EmployeeProfile
            {
                UserId = user.Id,
                WorkDepartmentId = maintenanceDept.Id,
                Name = sample.First,
                Surname = sample.Last,
                CompanyNumber = sample.EmpNo,
                Occupation = sample.Occupation,
                CreatedBy = "system",
                CreatedDate = now,
            });

            seeded++;
        }

        if (seeded > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Seeded {Count} safety lead demo users ({Password}): HSE Manager, Safety Manager, Safety Officer.",
                seeded,
                DemoPassword);
        }
    }
}
