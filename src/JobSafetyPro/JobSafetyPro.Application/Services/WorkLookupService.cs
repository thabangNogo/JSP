using JobSafetyPro.Application.DTOs.MasterData;
using JobSafetyPro.Domain.Entities.MasterData;
using JobSafetyPro.Domain.Exceptions;
using JobSafetyPro.Domain.Interfaces;

namespace JobSafetyPro.Application.Services;

public interface IWorkLookupService
{
    Task<WorkLookupsDto> GetAllAsync(CancellationToken cancellationToken = default);

    Task<string> GetDepartmentNameAsync(Guid workDepartmentId, CancellationToken cancellationToken = default);

    Task<string> GetLocationNameAsync(Guid workLocationId, CancellationToken cancellationToken = default);

    Task<string> GetSectionNameAsync(Guid workSectionId, CancellationToken cancellationToken = default);
}

public class WorkLookupService : IWorkLookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkLookupService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<WorkLookupsDto> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _unitOfWork.WorkDepartments.GetAllAsync(cancellationToken);
        var locations = await _unitOfWork.WorkLocations.GetAllAsync(cancellationToken);
        var sections = await _unitOfWork.WorkSections.GetAllAsync(cancellationToken);

        return new WorkLookupsDto(
            MapDepartments(departments),
            MapLocations(locations),
            MapSections(sections));
    }

    public async Task<string> GetDepartmentNameAsync(Guid workDepartmentId, CancellationToken cancellationToken = default) =>
        (await _unitOfWork.WorkDepartments.GetByIdAsync(workDepartmentId, cancellationToken))?.Name
        ?? throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["WorkDepartmentId"] = new[] { "Department does not exist." }
        });

    public async Task<string> GetLocationNameAsync(Guid workLocationId, CancellationToken cancellationToken = default) =>
        (await _unitOfWork.WorkLocations.GetByIdAsync(workLocationId, cancellationToken))?.Name
        ?? throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["WorkLocationId"] = new[] { "Location does not exist." }
        });

    public async Task<string> GetSectionNameAsync(Guid workSectionId, CancellationToken cancellationToken = default) =>
        (await _unitOfWork.WorkSections.GetByIdAsync(workSectionId, cancellationToken))?.Name
        ?? throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["WorkSectionId"] = new[] { "Section does not exist." }
        });

    private static IReadOnlyList<WorkLookupItemDto> MapDepartments(IReadOnlyList<WorkDepartment> items) =>
        items.OrderBy(d => d.SortOrder).Select(d => new WorkLookupItemDto(d.Id, d.Name, d.SortOrder)).ToList();

    private static IReadOnlyList<WorkLookupItemDto> MapLocations(IReadOnlyList<WorkLocation> items) =>
        items.OrderBy(l => l.SortOrder).Select(l => new WorkLookupItemDto(l.Id, l.Name, l.SortOrder)).ToList();

    private static IReadOnlyList<WorkLookupItemDto> MapSections(IReadOnlyList<WorkSection> items) =>
        items.OrderBy(s => s.SortOrder).Select(s => new WorkLookupItemDto(s.Id, s.Name, s.SortOrder)).ToList();
}
