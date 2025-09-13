using AutoMapper;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.Data.Entities;

namespace BiteBoard.API.Mappings
{
    public class TenantProfile : Profile
    {
        public TenantProfile()
        {
            //CreateMap<Source, Destination>()
            CreateMap<Tenant, TenantDto>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
