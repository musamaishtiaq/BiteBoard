using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Contexts
{
    public abstract class AuditableContext : MultiTenantDbContext
    {
        public DbSet<AuditTrail> AuditTrails { get; set; }

        public AuditableContext(DbContextOptions options, IMultiTenantContextAccessor tenantInfo) : base(tenantInfo, options)
        {
        }

        public AuditableContext(DbContextOptions options, ITenantInfo tenantInfo) : base(tenantInfo, options)
        {
        }

        public AuditableContext(DbContextOptions options) : base((ITenantInfo)null, options)
        {
        }

        public virtual async Task<int> SaveChangesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return await base.SaveChangesAsync();
            else
            {
                List<AuditTrailEntry> auditTrailEntries = OnBeforeSaveChanges(userId);
                int result = await base.SaveChangesAsync();
                await OnAfterSaveChanges(auditTrailEntries);
                return result;
            }
        }

        private List<AuditTrailEntry> OnBeforeSaveChanges(Guid userId)
        {
            ChangeTracker.DetectChanges();
            List<AuditTrailEntry> auditTrailEntries = new();
            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditTrail || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                AuditTrailEntry auditTrailEntry = new(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditTrailEntries.Add(auditTrailEntry);
                foreach (PropertyEntry property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditTrailEntry.TemporaryProperties.Add(property);
                        continue;
                    }
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditTrailEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditTrailEntry.AuditType = AuditTrailType.Create;
                            auditTrailEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditTrailEntry.AuditType = AuditTrailType.Delete;
                            auditTrailEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditTrailEntry.ChangedColumns.Add(propertyName);
                                auditTrailEntry.AuditType = AuditTrailType.Update;
                                auditTrailEntry.OldValues[propertyName] = property.OriginalValue;
                                auditTrailEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (AuditTrailEntry auditTrailEntry in auditTrailEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditTrails.Add(auditTrailEntry.ToAuditTrail());
            }
            return auditTrailEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditTrailEntry> auditTrailEntries)
        {
            if (auditTrailEntries == null || auditTrailEntries.Count == 0)
                return Task.CompletedTask;
            foreach (AuditTrailEntry auditTrailEntry in auditTrailEntries)
            {
                foreach (PropertyEntry prop in auditTrailEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                        auditTrailEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    else
                        auditTrailEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                AuditTrails.Add(auditTrailEntry.ToAuditTrail());
            }
            return SaveChangesAsync();
        }
    }
}
