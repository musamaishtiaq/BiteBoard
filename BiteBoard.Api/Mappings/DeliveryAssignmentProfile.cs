using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class DeliveryAssignmentProfile : Profile
    {
        public DeliveryAssignmentProfile()
        {
            CreateMap<DeliveryAssignment, DeliveryAssignmentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToStringFromGuid()))
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.ToGuidFromString()))
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId.ToGuidFromString()));
        }
    }
}
