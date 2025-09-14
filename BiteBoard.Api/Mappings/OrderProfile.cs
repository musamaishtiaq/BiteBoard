using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.TableId.HasValue ? src.TableId.Value.ToStringFromGuid() : null))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.TableId != null ? src.TableId.ToGuidFromString() : (Guid?)null));
        }
    }
}
