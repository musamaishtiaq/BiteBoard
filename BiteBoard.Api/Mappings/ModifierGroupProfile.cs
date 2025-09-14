using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.Api.Mappings
{
    public class ModifierGroupProfile : Profile
    {
        public ModifierGroupProfile()
        {
            CreateMap<ModifierGroup, ModifierGroupDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
