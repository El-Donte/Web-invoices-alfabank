using AbsIntegrationService.Infrastructure.Entities;
using AbsIntegrationService.Models;
using AutoMapper;

namespace AbsIntegrationService.Infrastructure.MappingProfiles;

public class InvoiceDraftMapperProfile : Profile
{
    public InvoiceDraftMapperProfile()
    {
        CreateMap<InvoiceDraft, InvoiceDraftEntity>();
        CreateMap<InvoiceDraftEntity, InvoiceDraft>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}