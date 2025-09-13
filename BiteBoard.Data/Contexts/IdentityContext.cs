using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BiteBoard.Data.Entities;
using System;

namespace BiteBoard.Data.Contexts
{
    public class IdentityContext : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base((ITenantInfo)null, options)
        {
        }

        public IdentityContext(DbContextOptions<IdentityContext> options, ITenantInfo tenantInfo) : base(tenantInfo, options)
        {
        }
        public IdentityContext(DbContextOptions<IdentityContext> options, IMultiTenantContextAccessor tenantInfo) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Identity");
            builder.Entity<ApplicationUser>(entity => entity.ToTable(name: "Users"));
            builder.Entity<ApplicationRole>(entity => entity.ToTable(name: "Roles"));
            builder.Entity<IdentityUserRole<Guid>>(entity => entity.ToTable("UserRoles"));
            builder.Entity<IdentityUserClaim<Guid>>(entity => entity.ToTable("UserClaims"));
            builder.Entity<IdentityUserLogin<Guid>>(entity => entity.ToTable("UserLogins"));
            builder.Entity<IdentityRoleClaim<Guid>>(entity => entity.ToTable("RoleClaims"));
            builder.Entity<IdentityUserToken<Guid>>(entity => entity.ToTable("UserTokens"));
            builder.ConfigureMultiTenant();
        }
    }
}