using FluentValidation;
using JobSafetyPro.Application.DTOs.Auth;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Organization;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.DTOs.Profile;
using JobSafetyPro.Application.DTOs.Safety;

namespace JobSafetyPro.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class CreateJobSafetyAssessmentValidator : AbstractValidator<CreateJobSafetyAssessmentDto>
{
    public CreateJobSafetyAssessmentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobDescription).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PlantId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.WorkLocationId).NotEmpty();
        RuleFor(x => x.WorkSectionId).NotEmpty();

        RuleForEach(x => x.Hazards).ChildRules(hazard =>
        {
            hazard.RuleFor(h => h.Description).NotEmpty().MaximumLength(2000);
            hazard.RuleFor(h => h.RiskLevelId).NotEmpty();
        }).When(x => x.Hazards != null && x.Hazards.Count > 0);

        RuleForEach(x => x.Controls).ChildRules(control =>
        {
            control.RuleFor(c => c.Description).NotEmpty().MaximumLength(2000);
            control.RuleFor(c => c.HierarchyOfControl).NotEmpty().MaximumLength(100);
        }).When(x => x.Controls != null && x.Controls.Count > 0);
    }
}

public class SaveEmployeeProfileValidator : AbstractValidator<SaveEmployeeProfileDto>
{
    public SaveEmployeeProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CompanyNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Occupation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkDepartmentId).NotEmpty();
    }
}

public class SaveAssessmentDraftValidator : AbstractValidator<SaveAssessmentDraftDto>
{
    public SaveAssessmentDraftValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobDescription).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PlantId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.CurrentStep).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WorkLocationId).NotEmpty();
        RuleFor(x => x.WorkSectionId).NotEmpty();
    }
}

public class CreateIncidentValidator : AbstractValidator<CreateIncidentDto>
{
    public CreateIncidentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.OccurredAt).LessThanOrEqualTo(DateTime.UtcNow.AddHours(1));
    }
}

public class CreateCorrectiveActionValidator : AbstractValidator<CreateCorrectiveActionDto>
{
    public CreateCorrectiveActionValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.AssignedToUserId).NotEmpty();
        RuleFor(x => x)
            .Must(x => (x.IncidentId.HasValue && !x.NearMissId.HasValue) ||
                       (!x.IncidentId.HasValue && x.NearMissId.HasValue))
            .WithMessage("Corrective action must be linked to either an incident or a near miss.");
    }
}

public class CreateNearMissReportValidator : AbstractValidator<CreateNearMissReportDto>
{
    public CreateNearMissReportValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PlantId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.OccurredAt).LessThanOrEqualTo(DateTime.UtcNow.AddHours(1));
        // Photos are optional — no rules on PhotoStoragePaths.
    }
}

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CompanyNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Occupation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkDepartmentId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class CreateInjuryValidator : AbstractValidator<CreateInjuryDto>
{
    public CreateInjuryValidator()
    {
        RuleFor(x => x.EmployeeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IncidentDescription).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.InjuryOccurredAt).LessThanOrEqualTo(DateTime.UtcNow.AddHours(1));
        RuleFor(x => x.LostTimeDays).GreaterThanOrEqualTo(0).When(x => x.LostTimeDays.HasValue);
    }
}

public class UpdateInjuryValidator : AbstractValidator<UpdateInjuryDto>
{
    public UpdateInjuryValidator()
    {
        RuleFor(x => x.EmployeeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IncidentDescription).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.LostTimeDays).GreaterThanOrEqualTo(0).When(x => x.LostTimeDays.HasValue);
    }
}
