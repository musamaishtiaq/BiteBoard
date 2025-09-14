using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class OrderItemProfile : Profile
    {
        public OrderItemProfile()
        {
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToStringFromGuid()))
                .ForMember(dest => dest.MenuItemId, opt => opt.MapFrom(src => src.MenuItemId.ToStringFromGuid()))
                .ForMember(dest => dest.DealId, opt => opt.MapFrom(src => src.DealId.HasValue ? src.DealId.Value.ToStringFromGuid() : null))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToGuidFromString()))
                .ForMember(dest => dest.MenuItemId, opt => opt.MapFrom(src => src.MenuItemId.ToGuidFromString()))
                .ForMember(dest => dest.DealId, opt => opt.MapFrom(src => src.DealId != null ? src.DealId.ToGuidFromString() : (Guid?)null));
        }
    }
}
