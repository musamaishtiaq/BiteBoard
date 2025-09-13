using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BiteBoard.Data.Constants;
using BiteBoard.API.Settings;
using BiteBoard.Data.Entities;

namespace BiteBoard.API.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JWTSettings _jwtSetting;

    public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, JWTSettings jwtSetting)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSetting = jwtSetting;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
            return;
        var permissions = context.User.Claims
            .Where(x => x.Type == CustomClaimTypes.Permission
            && x.Value == requirement.Permission
            && (x.Issuer == "LOCAL AUTHORITY" || x.Issuer == _jwtSetting.Issuer));
        if (permissions.Any())
        {
            context.Succeed(requirement);
            return;
        }
        await Task.CompletedTask;
    }
}