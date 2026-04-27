using AbsIntegrationService.Infrastructure.Entities;
using AbsIntegrationService.Models;
using AutoMapper;

namespace AbsIntegrationService.Infrastructure.MappingProfiles;

public class DraftOperationLinkMapperProlife: Profile
{
    public DraftOperationLinkMapperProlife()
    {
        CreateMap<DraftOperationLinkEntity, DraftOperationLink>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        CreateMap<DraftOperationLink,DraftOperationLinkEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}