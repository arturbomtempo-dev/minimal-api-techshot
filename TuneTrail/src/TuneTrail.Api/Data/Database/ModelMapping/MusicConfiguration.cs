using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.Data.Database.ModelMapping.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static TuneTrail.Api.Shared.Constants;

namespace TuneTrail.Api.Data.Database.ModelMapping;

public class MusicConfiguration : BaseEntityTypeConfiguration<Music>
{
    public override void Configure(EntityTypeBuilder<Music> builder)
    {
        base.Configure(builder);

        builder.ToTable("Music");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Title)
            .HasMaxLength(CharacterLimits.TWO_HUNDRED)
            .IsRequired();

        builder.Property(e => e.Artist)
            .HasMaxLength(CharacterLimits.ONE_HUNDRED)
            .IsRequired();

        builder.Property(e => e.Genre)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.PersonalRating)
            .IsRequired(false);

        builder.Property(e => e.PlayCount)
            .IsRequired();

        builder.HasIndex(e => e.Title);
        builder.HasIndex(e => e.Artist);
    }
}
