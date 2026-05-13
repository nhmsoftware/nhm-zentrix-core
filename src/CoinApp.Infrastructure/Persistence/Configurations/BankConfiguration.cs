using CoinApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinApp.Infrastructure.Persistence.Configurations;

public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("banks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Bin)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ShortName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.Bin)
            .IsUnique();

        builder.HasData(
            Create("11111111-1111-1111-1111-111111111111", "970436", "VCB", "Ngan hang TMCP Ngoai Thuong Viet Nam", "Vietcombank"),
            Create("22222222-2222-2222-2222-222222222222", "970415", "ICB", "Ngan hang TMCP Cong Thuong Viet Nam", "VietinBank"),
            Create("33333333-3333-3333-3333-333333333333", "970418", "BIDV", "Ngan hang TMCP Dau Tu va Phat Trien Viet Nam", "BIDV"),
            Create("44444444-4444-4444-4444-444444444444", "970405", "VBA", "Ngan hang Nong Nghiep va Phat Trien Nong Thon Viet Nam", "Agribank"),
            Create("55555555-5555-5555-5555-555555555555", "970422", "MB", "Ngan hang TMCP Quan Doi", "MBBank"),
            Create("66666666-6666-6666-6666-666666666666", "970407", "TCB", "Ngan hang TMCP Ky Thuong Viet Nam", "Techcombank"),
            Create("77777777-7777-7777-7777-777777777777", "970432", "VPB", "Ngan hang TMCP Viet Nam Thinh Vuong", "VPBank"),
            Create("88888888-8888-8888-8888-888888888888", "970416", "ACB", "Ngan hang TMCP A Chau", "ACB"));
    }

    private static Bank Create(string id, string bin, string code, string name, string shortName) =>
        new()
        {
            Id = Guid.Parse(id),
            Bin = bin,
            Code = code,
            Name = name,
            ShortName = shortName,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 5, 13, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 5, 13, 0, 0, 0, DateTimeKind.Utc)
        };
}
