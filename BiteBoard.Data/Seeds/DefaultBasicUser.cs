using Microsoft.AspNetCore.Identity;
using BiteBoard.Data.Constants;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BiteBoard.Data.Seeds;

public static class DefaultBasicUser
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        ApplicationUser defaultBasicUser = new()
        {
            Id = Guid.Parse("4eab7589-9cfc-4e4b-9c37-5138c674a5fb"),
            UserName = "basicuser",
            Email = "basicuser@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsActive = true
        };
        if (userManager.Users.All(u => u.Id != defaultBasicUser.Id))
        {
            ApplicationUser user = await userManager.FindByEmailAsync(defaultBasicUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultBasicUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(defaultBasicUser, Roles.Basic.ToString());

                var basicUserRole = await roleManager.FindByNameAsync(Roles.Basic.ToString());
                if (basicUserRole != null)
                {
                    // Assign all permissions from Products
                    await AddAllPermissionClaims(roleManager, basicUserRole, "Dashboard");
                }
            }
        }
    }

    private static async Task AddAllPermissionClaims(RoleManager<ApplicationRole> roleManager, ApplicationRole role, string module)
    {
        IList<Claim> allClaims = await roleManager.GetClaimsAsync(role);
        List<string> allPermissions = Permissions.GeneratePermissionsForModule(module);

        foreach (var permission in allPermissions)
        {
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }
        }
    }

    private static async Task AddSpecificPermissionClaim(RoleManager<ApplicationRole> roleManager, ApplicationRole role, string module, List<string> specificPermissions)
    {
        IList<Claim> allClaims = await roleManager.GetClaimsAsync(role);
        foreach (var permission in specificPermissions)
        {
            var permissionInDb = Permissions.GeneratePermissionsForModule(module).FirstOrDefault(p => p.EndsWith(permission));

            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permissionInDb))
            {
                await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permissionInDb));
            }
        }
    }
}