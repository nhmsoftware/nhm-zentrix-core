using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAtUtc",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferrerId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_verification_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_verification_codes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_ReferralCode",
                table: "users",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ReferrerId",
                table: "users",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_codes_Email",
                table: "email_verification_codes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_codes_UserId",
                table: "email_verification_codes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_verification_codes");

            migrationBuilder.DropIndex(
                name: "IX_users_ReferralCode",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_ReferrerId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAtUtc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ReferrerId",
                table: "users");
        }
    }
}
