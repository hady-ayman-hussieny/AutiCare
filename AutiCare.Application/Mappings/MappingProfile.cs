using AutoMapper;
using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Child, ChildResponse>()
            .ForMember(d => d.AgeInYears, o => o.MapFrom(s => DateTime.Today.Year - s.DateOfBirth.Year));

        CreateMap<Specialist, SpecialistResponse>();

        CreateMap<Session, SessionResponse>();

        CreateMap<AIResult, AIResultResponse>()
            .ForMember(d => d.ChildId, o => o.Ignore())
            .ForMember(d => d.ChildName, o => o.Ignore());
    }
}
