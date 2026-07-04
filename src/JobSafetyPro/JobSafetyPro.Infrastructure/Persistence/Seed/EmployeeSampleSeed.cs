using JobSafetyPro.Application.Constants;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Infrastructure.Persistence;
using JobSafetyPro.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class EmployeeSampleSeed
{
    private static readonly (string Email, string First, string Last, string EmpNo, string Dept, string Occupation)[] Samples =
    {
        ("john.smith@jsp.demo", "John", "Smith", "CN-1001", "Machine Shop", "Artisan"),
        ("mary.jones@jsp.demo", "Mary", "Jones", "CN-1002", "Fabrication (Welding & Boilermaking)", "Welder"),
        ("peter.brown@jsp.demo", "Peter", "Brown", "CN-1003", "Fabrication (Welding & Boilermaking)", "Boilermaker"),
        ("lisa.wilson@jsp.demo", "Lisa", "Wilson", "CN-1004", "Fitting", "Fitter"),
        ("david.taylor@jsp.demo", "David", "Taylor", "CN-1005", "Maintenance", "Electrician"),
        ("susan.anderson@jsp.demo", "Susan", "Anderson", "CN-1006", "Warehouse", "Storeman"),
        ("michael.thomas@jsp.demo", "Michael", "Thomas", "CN-1007", "Stripping", "Operator"),
        ("2jane.martin@jsp.demo", "Jane", "Martin", "CN-1008", "Machine Shop", "Supervisor"),
    };

    public static async Task SeedIfNeededAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var company = await context.Companies.FirstOrDefaultAsync(cancellationToken);
        if (company == null) return;

        var existingCount = await context.Users.CountAsync(u => u.CompanyId == company.Id, cancellationToken);
        if (existingCount > 5) return;

        var plant = await context.Plants.FirstAsync(p => p.CompanyId == company.Id, cancellationToken);
        var orgDept = await context.Departments.FirstAsync(d => d.PlantId == plant.Id, cancellationToken);
        var operatorRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.Operator, cancellationToken);
        var supervisorRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.Supervisor, cancellationToken);
        var workDepts = await context.WorkDepartments.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var password = passwordHasher.HashPassword("Employee@123");

        foreach (var sample in Samples)
        {
            if (await context.Users.AnyAsync(u => u.Email == sample.Email, cancellationToken))
            {
                continue;
            }

            var workDept = workDepts.First(d => d.Name == sample.Dept);
            var user = new User
            {
                CompanyId = company.Id,
                PlantId = plant.Id,
                DepartmentId = orgDept.Id,
                Email = sample.Email,
                PasswordHash = password,
                FirstName = sample.First,
                LastName = sample.Last,
                EmployeeNumber = sample.EmpNo,
                IsActive = sample.Occupation != "Operator" || sample.Email != "michael.thomas@jsp.demo",
                CreatedBy = "system",
                CreatedDate = now.AddMonths(-Random.Shared.Next(6, 36)),
            };

            var role = sample.Occupation == "Supervisor" ? supervisorRole : operatorRole;
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
                WorkDepartmentId = workDept.Id,
                Name = sample.First,
                Surname = sample.Last,
                CompanyNumber = sample.EmpNo,
                Occupation = sample.Occupation,
                CreatedBy = "system",
                CreatedDate = user.CreatedDate,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded sample employees for demo portal.");
    }
}
