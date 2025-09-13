using Microsoft.AspNetCore.Identity;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
using System.Threading.Tasks;

namespace BiteBoard.Data.Seeds;

public static class DefaultRoles
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = Roles.SuperAdmin.ToString()
        });
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = Roles.Admin.ToString()
        });
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = Roles.Moderator.ToString()
        });
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = Roles.Basic.ToString()
        });
    }
}