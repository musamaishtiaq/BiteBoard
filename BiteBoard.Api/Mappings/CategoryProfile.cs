using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.API.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            //CreateMap<Source, Destination>()
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
