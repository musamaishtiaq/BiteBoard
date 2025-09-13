using Microsoft.AspNetCore.Identity;
using BiteBoard.API.DTOs.Requests.Identity;
using BiteBoard.Data.Entities;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace BiteBoard.API.Helpers
{
	public static class ClaimsHelper
	{
		public static void HasRequiredClaims(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> permissions)
		{
			if (!claimsPrincipal.Identity.IsAuthenticated)
			{
				return;
			}
			var allClaims = claimsPrincipal.Claims.Select(a => a.Value).ToList();
			var success = allClaims.Intersect(permissions).Any();
			if (!success)
			{
				throw new Exception();
			}
			return;
		}

		public static void GetPermissions(this List<RoleClaimsDto> allPermissions, Type policy, string roleId, string area)
		{
			FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fi in fields)
			{
				allPermissions.Add(new RoleClaimsDto { Value = fi.GetValue(null).ToString(), Type = "Permissions", Area = area });
			}
		}

		public static async Task AddPermissionClaim(this RoleManager<ApplicationRole> roleManager, ApplicationRole role, string permission)
		{
			var allClaims = await roleManager.GetClaimsAsync(role);
			if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
			{
				await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
			}
		}
	}
}
