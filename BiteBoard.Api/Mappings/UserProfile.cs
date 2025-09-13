using AutoMapper;
using BiteBoard.API.DTOs.Requests.Identity;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;

namespace BiteBoard.API.Mappings
{
	public class UserProfile : Profile
	{
		public UserProfile()
		{
			//CreateMap<Source, Destination>();
			CreateMap<ApplicationUser, ApplicationUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToStringFromGuid()))
				.ReverseMap()
				.ForMember(c => c.Id, option => option.Ignore());
		}
	}
}
