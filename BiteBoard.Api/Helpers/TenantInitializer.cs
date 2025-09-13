using Microsoft.EntityFrameworkCore;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.API.Helpers
{
    public class TenantInitializer
    {
        private readonly TenantDbContext _context;
        private readonly ITenantService _tenantService;

        public TenantInitializer(
            TenantDbContext context,
            ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task CreateDefaultTenantAsync()
        {
            var tenantIdentifier = "default";
            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(ct => ct.Identifier == tenantIdentifier);
            if (existingTenant == null)
            {
                existingTenant = new Tenant
                {
                    Id = "c82c98ae-21c2-4f0d-d119-8d6e4d9f32a1",
                    Identifier = "default",
                    Name = "Default Tenant",
                    ConnectionString = "Server=localhost;port=5432;Database=BiteBoard;User Id=postgres;Password=123;",
                    IsEnabled = true,
                    ValidTill = new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    CreatedOn = new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                };
                _context.Tenants.Add(existingTenant);
                await _context.SaveChangesAsync();
            }

            await _tenantService.CreateTenantDataBaseAsync(existingTenant);
        }
    }
}
