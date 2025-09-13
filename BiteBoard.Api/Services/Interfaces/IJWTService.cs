using BiteBoard.Data.Entities;
using System.Security.Claims;

namespace BiteBoard.API.Services.Interfaces;

public interface IJWTService
{
    string GenerateToken(ApplicationUser user, IList<Claim> userClaims, string tenant);
}