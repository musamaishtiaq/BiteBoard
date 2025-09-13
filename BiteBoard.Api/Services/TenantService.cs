using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.API.Helpers;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Interfaces;
using BiteBoard.Data.Seeds;
using Serilog;
using System;
using Npgsql;

namespace BiteBoard.API.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TenantService> _logger;

        public TenantService(
            TenantDbContext context,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<TenantService> logger)
        {
            _context = context;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<Tenant> GetTenantById(string id)
        {
            return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants.ToListAsync();
        }

        public async Task<Tenant> CreateTenantAsync(TenantDto request)
        {
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == request.Identifier);

            if (existingTenant != null)
                throw new InvalidOperationException($"Tenant with identifier '{request.Identifier}' already exists.");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid().ToString(),
                Identifier = request.Identifier,
                Name = request.Name,
                ConnectionString = request.ConnectionString,
                ValidTill = request.ValidTill,
                IsEnabled = true
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            await CreateTenantDbAsync(tenant);
            _logger.LogInformation($"Tenant '{request.Identifier}' created successfully.");

            return tenant;
        }

        public async Task<Tenant> UpdateTenantAsync(string id, TenantDto request)
        {
            var tenantInD = await _context.Tenants.FindAsync(id);
            if (tenantInD == null)
                throw new InvalidOperationException($"Tenant with id: {id} not found.");

            var alreadyExists = await _context.Tenants.AnyAsync(t => t.Identifier == request.Identifier && t.Id != request.Id);
            if (alreadyExists)
                throw new InvalidOperationException($"Tenant identifier '{request.Identifier}' already taken.");

            tenantInD.ValidTill = request.ValidTill;
            tenantInD.IsEnabled = true;
            _context.Tenants.Update(tenantInD);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Tenant '{request.Identifier}' updated successfully.");

            return tenantInD;
        }

        public async Task<Tenant> EnableTenantAsync(string id, bool isEnabled)
        {
            var tenantInD = await _context.Tenants.FindAsync(id);
            if (tenantInD == null)
                throw new InvalidOperationException($"Tenant with id: {id} not found.");

            tenantInD.IsEnabled = isEnabled;
            _context.Tenants.Update(tenantInD);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Tenant '{tenantInD.Identifier}' " + (isEnabled ? "enabled" : "disabled") + " successfully.");

            return tenantInD;
        }

        public async Task CreateTenantDataBaseAsync(Tenant request)
        {
            await CreateTenantDbAsync(request);
            _logger.LogInformation($"Tenant '{request.Identifier}' database created successfully.");
        }

        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            tenant.IsEnabled = false;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task CreateTenantDbAsync(Tenant tenantInfo)
        {
            try
            {
                // Create Application Database
                await CreateApplicationDatabaseAsync(tenantInfo);

                // Create Identity Database (same database but different schema)
                await CreateIdentityDatabaseAsync(tenantInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create databases for tenant '{tenantInfo.Identifier}'");
                throw;
            }
        }

        private async Task CreateApplicationDatabaseAsync(Tenant tenantInfo)
        {
            var applicationOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(tenantInfo.ConnectionString)
                .Options;

            using var scope = _serviceProvider.CreateScope();
            // Get required services
            var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();
            // Create a temporary authenticated user service for database creation
            var tempUserService = new TempAuthenticatedUserService();

            using (var appContext = new ApplicationDbContext(applicationOptions, tenantInfo, dateTimeService, tempUserService))
            {
                await appContext.Database.EnsureCreatedAsync();
                await SeedApplicationDataAsync(appContext, tenantInfo);
            }
        }

        private async Task CreateIdentityDatabaseAsync(Tenant tenantInfo)
        {
            var identityOptions = new DbContextOptionsBuilder<IdentityContext>()
                .UseNpgsql(tenantInfo.ConnectionString)
                .Options;

            using (var identityContext = new IdentityContext(identityOptions, tenantInfo))
            {
                await identityContext.Database.MigrateAsync();
                var userManager = CreateUserManager(identityContext);
                var roleManager = CreateRoleManager(identityContext);

                await SeedIdentityDataAsync(identityContext, userManager, roleManager, tenantInfo);
            }
        }

        private UserManager<ApplicationUser> CreateUserManager(IdentityContext context)
        {
            var userStore = new UserStore<ApplicationUser, ApplicationRole, IdentityContext, Guid>(context);

            var options = _serviceProvider.GetRequiredService<IOptions<IdentityOptions>>();
            var passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            var userValidators = _serviceProvider.GetServices<IUserValidator<ApplicationUser>>();
            var passwordValidators = _serviceProvider.GetServices<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = _serviceProvider.GetRequiredService<ILookupNormalizer>();
            var errors = _serviceProvider.GetRequiredService<IdentityErrorDescriber>();
            var logger = _serviceProvider.GetRequiredService<ILogger<UserManager<ApplicationUser>>>();

            return new UserManager<ApplicationUser>(
                userStore,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                _serviceProvider,
                logger);
        }

        private RoleManager<ApplicationRole> CreateRoleManager(IdentityContext context)
        {
            var roleStore = new RoleStore<ApplicationRole, IdentityContext, Guid>(context);

            var roleValidators = _serviceProvider.GetServices<IRoleValidator<ApplicationRole>>();
            var keyNormalizer = _serviceProvider.GetRequiredService<ILookupNormalizer>();
            var errors = _serviceProvider.GetRequiredService<IdentityErrorDescriber>();
            var logger = _serviceProvider.GetRequiredService<ILogger<RoleManager<ApplicationRole>>>();

            return new RoleManager<ApplicationRole>(
                roleStore,
                roleValidators,
                keyNormalizer,
                errors,
                logger);
        }

        private async Task SeedApplicationDataAsync(ApplicationDbContext context, Tenant tenantInfo)
        {
            // Add any default application data here
            if (tenantInfo.Id == "c82c98ae-21c2-4f0d-d119-8d6e4d9f32a1" && tenantInfo.Identifier == "default")
            {

            }
            else
            {

            }

            await context.SaveChangesAsync();
        }

        private async Task SeedIdentityDataAsync(IdentityContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, Tenant tenantInfo)
        {
            await DefaultRoles.SeedAsync(userManager, roleManager);
            await DefaultSuperAdminUser.SeedAsync(userManager, roleManager, tenantInfo);
            await DefaultBasicUser.SeedAsync(userManager, roleManager);

            await Task.CompletedTask;
        }
    }

    public class TempAuthenticatedUserService : IAuthenticatedUserService
    {
        public Guid UserId => Guid.Empty; // System user for initial setup
        public string Username => "System";
    }
}
