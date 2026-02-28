using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "teams",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "telegram_link_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_link_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telegram_link_codes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telegram_link_codes_Code",
                table: "telegram_link_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telegram_link_codes_ExpiresAt",
                table: "telegram_link_codes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_link_codes_UserId",
                table: "telegram_link_codes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telegram_link_codes");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "teams");
        }
    }
}
