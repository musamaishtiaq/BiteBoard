using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class DeliveryAddressProfile : Profile
    {
        public DeliveryAddressProfile()
        {
            CreateMap<DeliveryAddress, DeliveryAddressDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToGuidFromString()));
        }
    }
}
