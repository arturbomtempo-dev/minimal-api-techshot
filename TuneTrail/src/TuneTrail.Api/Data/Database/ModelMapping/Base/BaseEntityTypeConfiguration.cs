using TuneTrail.Api.Data.Database.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TuneTrail.Api.Data.Database.ModelMapping.Base;

public abstract class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(BaseEntity.CreatedAt)).IsRequired();
            builder.Property(nameof(BaseEntity.UpdatedAt)).IsRequired(false);
            builder.Property(nameof(BaseEntity.Deleted)).IsRequired();
        }
    }
}
