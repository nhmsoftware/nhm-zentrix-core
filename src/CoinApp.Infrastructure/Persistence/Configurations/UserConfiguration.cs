using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinApp.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.DateOfBirth)
            .HasColumnType("date");

        builder.Property(x => x.Gender)
            .HasConversion<int>()
            .HasDefaultValue(UserGender.Unspecified);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.VerificationStatus)
            .HasConversion<int>()
            .HasDefaultValue(AccountVerificationStatus.Unverified);

        builder.Property(x => x.ReferralCode)
            .HasMaxLength(32)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.MoneyBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.BinBank)
            .HasMaxLength(20);

        builder.Property(x => x.AccountBank)
            .HasMaxLength(50);

        builder.Property(x => x.AccountBankName)
            .HasMaxLength(150);

        builder.Property(x => x.IdentityFrontImagePath)
            .HasMaxLength(500);

        builder.Property(x => x.IdentityBackImagePath)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");
    }
}
