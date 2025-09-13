using Finbuckle.MultiTenant;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.Data.Entities;

namespace BiteBoard.API.Services.Interfaces
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenantAsync(TenantDto request);
        Task<Tenant> UpdateTenantAsync(string id, TenantDto request);
        Task CreateTenantDataBaseAsync(Tenant request);
        Task<Tenant> GetTenantById(string id);
        Task<bool> DeleteTenantAsync(string tenantId);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<Tenant> EnableTenantAsync(string id, bool isEnabled);
    }
}
