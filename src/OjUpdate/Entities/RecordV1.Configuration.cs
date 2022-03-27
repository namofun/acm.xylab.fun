using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SatelliteSite.OjUpdateModule.Entities;

namespace SatelliteSite.OjUpdateModule
{
    public class RecordV1EntityConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<RecordV1>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<RecordV1> entity)
        {
            entity.ToTable("TenantSolveRecord");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Category);

            entity.HasIndex(e => new { e.Category, e.Grade });

            entity.Property(e => e.Account)
                .IsRequired();

            entity.Property(e => e.NickName)
                .IsRequired();
        }
    }
}
