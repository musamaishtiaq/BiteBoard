using AutoMapper;
using BiteBoard.API.DTOs.Requests.Identity;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.API.Mappings
{
	public class RoleProfile : Profile
	{
		public RoleProfile()
		{
			//CreateMap<Source, Destination>();
			CreateMap<ApplicationRole, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
		}
	}
}
