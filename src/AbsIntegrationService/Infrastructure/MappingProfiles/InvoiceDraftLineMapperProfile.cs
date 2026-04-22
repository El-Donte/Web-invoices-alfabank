using AbsIntegrationService.Infrastructure.Entities;
using AbsIntegrationService.Models;
using AutoMapper;

namespace AbsIntegrationService.Infrastructure.MappingProfiles;

public class InvoiceDraftLineMapperProfile :  Profile
{
    public InvoiceDraftLineMapperProfile()
    {
        CreateMap<InvoiceDraftLineEntity, InvoiceDraftLine>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        CreateMap<InvoiceDraftLine, InvoiceDraftLineEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}