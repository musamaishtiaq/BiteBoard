using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class DealItemProfile : Profile
    {
        public DealItemProfile()
        {
            CreateMap<DealItem, DealItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.MenuItemId, opt => opt.MapFrom(src => src.MenuItemId.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MenuItemId, opt => opt.MapFrom(src => src.MenuItemId.ToGuidFromString()));
        }
    }
}
