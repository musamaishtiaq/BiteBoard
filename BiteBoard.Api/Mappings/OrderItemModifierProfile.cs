using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class OrderItemModifierProfile : Profile
    {
        public OrderItemModifierProfile()
        {
            CreateMap<OrderItemModifier, OrderItemModifierDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.OrderItemId.ToStringFromGuid()))
                .ForMember(dest => dest.ModifierOptionId, opt => opt.MapFrom(src => src.ModifierOptionId.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.OrderItemId.ToGuidFromString()))
                .ForMember(dest => dest.ModifierOptionId, opt => opt.MapFrom(src => src.ModifierOptionId.ToGuidFromString()));
        }
    }
}
