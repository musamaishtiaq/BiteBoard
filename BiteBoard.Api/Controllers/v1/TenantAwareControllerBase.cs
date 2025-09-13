using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.ResultWrapper;
using System.Security.Claims;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.API.Controllers.v1
{
    [ApiController]
    public abstract class TenantAwareControllerBase : ControllerBase
    {
        protected readonly TenantDbContext _tenantContext;

        protected TenantAwareControllerBase(TenantDbContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        protected async Task<Tenant> GetCurrentTenant(string identifier)
        {
            var tenant = await _tenantContext.Tenants.FirstOrDefaultAsync(t => t.Identifier == identifier);
            if (tenant == null)
            {
                return null;
            }

            return tenant;
        }

        protected async Task<bool> ValidateTenant(string identifier)
        {
            var tenant = await GetCurrentTenant(identifier);
            if (tenant == null)
            {
                return false;
            }

            return tenant.IsEnabled;
        }

        protected string GetTenant()
        {
            var tenant = "";
            if (Request.Headers.TryGetValue("X-Tenant", out var tenantHeader))
            {
                tenant = tenantHeader.ToString();
            }

            return tenant;
        }
    }
}
