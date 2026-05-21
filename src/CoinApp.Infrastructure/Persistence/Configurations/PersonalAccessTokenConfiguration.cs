using CoinApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinApp.Infrastructure.Persistence.Configurations;

public sealed class PersonalAccessTokenConfiguration : IEntityTypeConfiguration<PersonalAccessToken>
{
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.ToTable("personal_access_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.LastUsedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
