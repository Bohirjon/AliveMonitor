using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "monitored_endpoints",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MemberEmails = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teams_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitored_endpoints_TeamId",
                table: "monitored_endpoints",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_teams_UserId",
                table: "teams",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_monitored_endpoints_teams_TeamId",
                table: "monitored_endpoints",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monitored_endpoints_teams_TeamId",
                table: "monitored_endpoints");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropIndex(
                name: "IX_monitored_endpoints_TeamId",
                table: "monitored_endpoints");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "monitored_endpoints");
        }
    }
}
