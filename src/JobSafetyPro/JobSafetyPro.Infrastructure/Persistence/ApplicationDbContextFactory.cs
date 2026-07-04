using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Infrastructure.Persistence.Interceptors;
using JobSafetyPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JobSafetyPro.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        var interceptor = new AuditableEntityInterceptor(
            new DesignTimeCurrentUserService(),
            new DateTimeService());

        return new ApplicationDbContext(optionsBuilder.Options, interceptor);
    }

    private static IConfiguration BuildConfiguration()
    {
        var apiProjectPath = ResolveApiProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveApiProjectPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory != null)
        {
            if (directory.Name == "JobSafetyPro.API"
                && File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
            {
                return directory.FullName;
            }

            var siblingApiPath = Path.Combine(directory.FullName, "JobSafetyPro.API");
            if (File.Exists(Path.Combine(siblingApiPath, "appsettings.json")))
            {
                return siblingApiPath;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate JobSafetyPro.API/appsettings.json. Run migrations from the solution folder or JobSafetyPro.API.");
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public string? Email => "system";
        public Guid? CompanyId => null;
        public Guid? PlantId => null;
        public IReadOnlyList<string> Roles => Array.Empty<string>();
        public bool IsAuthenticated => false;
    }
}
