namespace JobSafetyPro.Application.DTOs.MasterData;

public record WorkLookupItemDto(Guid Id, string Name, int SortOrder);

public record WorkLookupsDto(
    IReadOnlyList<WorkLookupItemDto> Departments,
    IReadOnlyList<WorkLookupItemDto> Locations,
    IReadOnlyList<WorkLookupItemDto> Sections);
