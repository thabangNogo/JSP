using AutoMapper;
using JobSafetyPro.Application.DTOs.Incidents;
using JobSafetyPro.Application.DTOs.Organization;
using JobSafetyPro.Application.DTOs.Safety;
using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;

namespace JobSafetyPro.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>();

        CreateMap<Plant, PlantDto>();
        CreateMap<CreatePlantDto, Plant>();

        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentDto, Department>();

        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<Role, RoleDto>();

        CreateMap<JobSafetyAssessment, JobSafetyAssessmentDto>();
        CreateMap<CreateJobSafetyAssessmentDto, JobSafetyAssessment>()
            .ForMember(d => d.Hazards, opt => opt.Ignore())
            .ForMember(d => d.ControlMeasures, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.Department, opt => opt.Ignore())
            .ForMember(d => d.Location, opt => opt.Ignore())
            .ForMember(d => d.Section, opt => opt.Ignore());

        CreateMap<Hazard, HazardDto>();
        CreateMap<CreateHazardDto, Hazard>();

        CreateMap<RiskLevel, RiskLevelDto>();

        CreateMap<ControlMeasure, ControlMeasureDto>();
        CreateMap<CreateControlMeasureDto, ControlMeasure>();

        CreateMap<Incident, IncidentDto>();
        CreateMap<CreateIncidentDto, Incident>();

        CreateMap<NearMiss, NearMissDto>();
        CreateMap<CreateNearMissDto, NearMiss>();

        CreateMap<CorrectiveAction, CorrectiveActionDto>();
        CreateMap<CreateCorrectiveActionDto, CorrectiveAction>();

        CreateMap<Attachment, AttachmentDto>();
        CreateMap<CreateAttachmentDto, Attachment>();
    }
}
