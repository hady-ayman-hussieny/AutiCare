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
    }
}
