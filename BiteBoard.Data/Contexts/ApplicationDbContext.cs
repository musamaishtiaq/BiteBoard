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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantInfo tenantInfo, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) :
            base(options, tenantInfo)
        {
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMultiTenantContextAccessor tenantInfo, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options, tenantInfo)
        {
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<DealItem> DealItems { get; set; }
        public DbSet<ModifierGroup> ModifierGroups { get; set; }
        public DbSet<ModifierOption> ModifierOptions { get; set; }
        public DbSet<MenuItemModifier> MenuItemModifiers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemModifier> OrderItemModifiers { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public DbSet<DeliveryAssignment> DeliveryAssignments { get; set; }

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
            // Configure composite keys for junction tables
            builder.Entity<MenuItemModifier>()
                .HasKey(mm => new { mm.MenuItemId, mm.ModifierGroupId });

            // Configure relationships
            builder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId);

            builder.Entity<Deal>()
                .HasMany(d => d.DealItems)
                .WithOne(di => di.Deal)
                .HasForeignKey(di => di.DealId);

            builder.Entity<DealItem>()
                .HasOne(di => di.MenuItem)
                .WithMany()
                .HasForeignKey(di => di.MenuItemId);

            builder.Entity<OrderItem>()
            .HasOne(oi => oi.Deal)
            .WithMany()
            .HasForeignKey(oi => oi.DealId)
            .IsRequired(false);

            builder.Entity<DeliveryAddress>()
            .HasOne(da => da.Order)
            .WithOne(o => o.DeliveryAddress)
            .HasForeignKey<DeliveryAddress>(da => da.OrderId);

            builder.Entity<DeliveryAssignment>()
            .HasOne(da => da.Order)
            .WithOne(o => o.DeliveryAssignment)
            .HasForeignKey<DeliveryAssignment>(da => da.OrderId);

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.ConfigureMultiTenant();
        }
    }
}