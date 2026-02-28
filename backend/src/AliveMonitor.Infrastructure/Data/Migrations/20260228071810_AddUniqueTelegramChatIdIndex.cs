using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTelegramChatIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_TelegramChatId",
                table: "users",
                column: "TelegramChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telegram_link_codes_TeamId",
                table: "telegram_link_codes",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_telegram_link_codes_teams_TeamId",
                table: "telegram_link_codes",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_telegram_link_codes_teams_TeamId",
                table: "telegram_link_codes");

            migrationBuilder.DropIndex(
                name: "IX_users_TelegramChatId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_telegram_link_codes_TeamId",
                table: "telegram_link_codes");
        }
    }
}
