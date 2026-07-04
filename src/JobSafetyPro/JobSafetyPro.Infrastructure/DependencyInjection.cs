using JobSafetyPro.Infrastructure.Hubs;
using JobSafetyPro.Infrastructure.Persistence;
using JobSafetyPro.Infrastructure.Persistence.Interceptors;
using JobSafetyPro.Infrastructure.Persistence.Repositories;
using JobSafetyPro.Infrastructure.Persistence.Seed;
using JobSafetyPro.Infrastructure.Services;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JobSafetyPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdministrator", policy => policy.RequireRole("Administrator"));
            options.AddPolicy("RequireHseManager", policy => policy.RequireRole("Administrator", "HSE Manager"));
            options.AddPolicy("RequireSupervisor", policy => policy.RequireRole("Administrator", "HSE Manager", "Supervisor"));
            options.AddPolicy("RequireSafetyLead", policy => policy.RequireRole(
                "Administrator",
                "HSE Manager",
                "Safety Manager",
                "Safety Officer"));
        });

        services.AddSignalR();
        services.AddScoped<JobSafetyPro.Application.Interfaces.INotificationHubService, NotificationHubService>();
        services.AddHostedService<Background.SafetyEscalationHostedService>();

        return services;
    }

    public static async Task InitialiseDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await DatabaseSeeder.SeedAsync(serviceProvider);
    }
}
