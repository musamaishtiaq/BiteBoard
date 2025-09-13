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

public static class DefaultSuperAdminUser
{
    public static async Task AddPermissionClaim(this RoleManager<ApplicationRole> roleManager, ApplicationRole role, string module)
    {
        IList<Claim> allClaims = await roleManager.GetClaimsAsync(role);
        List<string> allPermissions = Permissions.GeneratePermissionsForModule(module);
        foreach (var permission in allPermissions)
        {
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
                await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
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

    private async static Task SeedClaimsForSuperAdmin(this RoleManager<ApplicationRole> roleManager, Tenant tenantInfo)
    {
        ApplicationRole superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        await roleManager.AddPermissionClaim(superAdminRole, "Dashboard");
        await roleManager.AddPermissionClaim(superAdminRole, "Users");
        await roleManager.AddPermissionClaim(superAdminRole, "Roles");

        await AddSpecificPermissionClaim(roleManager, superAdminRole, "Companies", new List<string> { "View", "Edit" });

        await roleManager.AddPermissionClaim(superAdminRole, "Products");
        await roleManager.AddPermissionClaim(superAdminRole, "Batches");
        await roleManager.AddPermissionClaim(superAdminRole, "Suppliers");
        await roleManager.AddPermissionClaim(superAdminRole, "MaterialCompositions");
        await roleManager.AddPermissionClaim(superAdminRole, "DigitalProductPassports");
        await roleManager.AddPermissionClaim(superAdminRole, "Widgets");
        await roleManager.AddPermissionClaim(superAdminRole, "Pages");
        await roleManager.AddPermissionClaim(superAdminRole, "Templates");

        // Only add Tenants permission for the default tenant
        if (tenantInfo.Id == "c82c98ae-21c2-4f0d-d119-8d6e4d9f32a1" && tenantInfo.Identifier == "default")
        {
            await roleManager.AddPermissionClaim(superAdminRole, "Tenants");
        }
        
    }

    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, Tenant tenantInfo)
    {
        ApplicationUser defaultSuperAdminUser = new()
        {
            Id = Guid.Parse("c82c98ae-21c2-4f0d-b119-8d6e4d9f32a1"),
            UserName = "superadmin",
            Email = "superadmin@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsActive = true
        };
        if (userManager.Users.All(u => u.Id != defaultSuperAdminUser.Id))
        {
            ApplicationUser user = await userManager.FindByEmailAsync(defaultSuperAdminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultSuperAdminUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(defaultSuperAdminUser, Roles.Basic.ToString());
                await userManager.AddToRoleAsync(defaultSuperAdminUser, Roles.Moderator.ToString());
                await userManager.AddToRoleAsync(defaultSuperAdminUser, Roles.Admin.ToString());
                await userManager.AddToRoleAsync(defaultSuperAdminUser, Roles.SuperAdmin.ToString());
            }
            await roleManager.SeedClaimsForSuperAdmin(tenantInfo);
        }
    }
}