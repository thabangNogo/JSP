using FluentValidation;
using JobSafetyPro.Application.DTOs.Ppe;

namespace JobSafetyPro.Application.Validators;

public class CreatePpeCatalogueItemValidator : AbstractValidator<CreatePpeCatalogueItemDto>
{
    public CreatePpeCatalogueItemValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStockLevel).GreaterThanOrEqualTo(0);
    }
}

public class CreatePpeRequestValidator : AbstractValidator<CreatePpeRequestDto>
{
    public CreatePpeRequestValidator()
    {
        RuleFor(x => x.EmployeeUserId).NotEmpty();
        RuleFor(x => x.Department).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PpeCatalogueItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RequiredByDate).NotEmpty();
    }
}

public class DispatchPpeRequestValidator : AbstractValidator<DispatchPpeRequestDto>
{
    public DispatchPpeRequestValidator()
    {
        RuleFor(x => x.DispatchDate).NotEmpty();
        RuleFor(x => x.CollectedByEmployee).NotEmpty().MaximumLength(200);
    }
}
