using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.Data.Database.Entities.Base;

namespace TuneTrail.Api.IoC.Context;

public class TuneTrailDbContext : DbContext
{
    public TuneTrailDbContext(DbContextOptions<TuneTrailDbContext> options)
        : base(options)
    {
    }

    public DbSet<Music> Music { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
