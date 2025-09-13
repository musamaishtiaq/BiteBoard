using BiteBoard.Data.Interfaces;
using System.Security.Claims;

namespace BiteBoard.API.Services;

public class AuthenticatedUserService : IAuthenticatedUserService
{
    public Guid UserId { get; }
    public string Username { get; }

    public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) != null)
        {
            bool isSuccess = Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
            if (isSuccess)
                UserId = userId;
            Username = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name) == null ? null : httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name).Value;
        }
    }
}