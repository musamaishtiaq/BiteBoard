using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BiteBoard.Data.Entities;
using System;

namespace BiteBoard.Data.EntityConfigurations
{
	public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
	{
		public void Configure(EntityTypeBuilder<Tenant> builder)
		{
			// Property Configurations
			builder.Property(ct => ct.Identifier)
				.IsRequired()
				.HasMaxLength(50)
				.IsUnicode(false);
			builder.Property(ct => ct.Name)
				.IsRequired()
				.HasMaxLength(100)
				.IsUnicode(false);
			builder.Property(ct => ct.ConnectionString)
				.IsRequired()
				.IsUnicode(false);
			builder.Property(ct => ct.IsEnabled)
				.IsRequired();
			builder.Property(ct => ct.CreatedOn)
				.IsRequired()
				.IsUnicode(false);
			// Key Configurations
			builder.HasKey(ct => ct.Id);
			builder.Property(ct => ct.Id)
				.ValueGeneratedOnAdd();
		}
	}
}