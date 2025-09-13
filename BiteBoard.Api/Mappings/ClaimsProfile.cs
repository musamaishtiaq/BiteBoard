using AutoMapper;
using BiteBoard.API.DTOs.Requests.Identity;
using System.Security.Claims;

namespace BiteBoard.API.Mappings
{
	public class ClaimsProfile : Profile
	{
		public ClaimsProfile()
		{
			//CreateMap<Source, Destination>();
			CreateMap<Claim, RoleClaimsDto>()
				.ReverseMap();
		}
	}
}
