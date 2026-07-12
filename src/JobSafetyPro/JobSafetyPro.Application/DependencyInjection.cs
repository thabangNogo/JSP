using FluentValidation;
using JobSafetyPro.Application.Interfaces;
using JobSafetyPro.Application.Mapping;
using JobSafetyPro.Application.Services;
using JobSafetyPro.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace JobSafetyPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJobSafetyAssessmentService, JobSafetyAssessmentService>();
        services.AddScoped<IEmployeeProfileService, EmployeeProfileService>();
        services.AddScoped<IWorkLookupService, WorkLookupService>();
        services.AddScoped<IJsaDraftWorkflowService, JsaDraftWorkflowService>();
        services.AddScoped<IRiskLevelService, RiskLevelService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<INearMissService, NearMissService>();
        services.AddScoped<IStopUnsafeWorkService, StopUnsafeWorkService>();
        services.AddScoped<ISafetyNotificationDispatcher, SafetyNotificationDispatcher>();
        services.AddScoped<ISafetyNotificationService, SafetyNotificationService>();
        services.AddScoped<ISafetyKpiService, SafetyKpiService>();
        services.AddScoped<ISafetyEscalationService, SafetyEscalationService>();
        services.AddScoped<ICorrectiveActionService, CorrectiveActionService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IInjuryFreeDaysService, InjuryFreeDaysService>();
        services.AddScoped<IInjuryDashboardService, InjuryDashboardService>();
        services.AddScoped<IInjuryService, InjuryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IJsaWorkflowService, JsaWorkflowService>();
        services.AddScoped<IInjuryWorkflowService, InjuryWorkflowService>();
        services.AddScoped<IManagerDashboardService, ManagerDashboardService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();

        services.AddScoped<IPpeCatalogueService, PpeCatalogueService>();
        services.AddScoped<IPpeRequestService, PpeRequestService>();
        services.AddScoped<IPpeDashboardService, PpeDashboardService>();
        services.AddScoped<IPpeReportService, PpeReportService>();
        services.AddScoped<IPpeEscalationService, PpeEscalationService>();

        return services;
    }
}
