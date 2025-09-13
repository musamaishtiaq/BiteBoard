using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BiteBoard.Data.Abstractions;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace BiteBoard.Data.Contexts
{
    public class ApplicationDbContext : AuditableContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMultiTenantContextAccessor tenantInfo, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options, tenantInfo)
        {
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantInfo tenantInfo, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) :
            base(options, tenantInfo)
        {
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {
        }

        

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (EntityEntry<AuditableEntity> entry in ChangeTracker.Entries<AuditableEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _authenticatedUser.UserId;
                        entry.Entity.CreatedAt = _dateTime.NowUtc;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                        entry.Entity.LastModifiedAt = _dateTime.NowUtc;
                        break;
                }
            }
            return _authenticatedUser.UserId == Guid.Empty
                ? await base.SaveChangesAsync(cancellationToken)
                : await base.SaveChangesAsync(_authenticatedUser.UserId);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.ConfigureMultiTenant();
        }
    }
}