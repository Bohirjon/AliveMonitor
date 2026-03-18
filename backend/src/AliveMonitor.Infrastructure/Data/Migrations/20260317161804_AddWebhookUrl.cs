using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "teams",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "teams");
        }
    }
}
