using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GoogleId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AlertEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "monitored_endpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FriendlyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CustomHeadersJson = table.Column<string>(type: "jsonb", nullable: true),
                    ExpectedStatusCode = table.Column<int>(type: "integer", nullable: false, defaultValue: 200),
                    JsonPropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    JsonPropertyExpectedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CurrentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Disabled"),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitored_endpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_monitored_endpoints_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_check_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    IsHealthy = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_check_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_health_check_logs_monitored_endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "monitored_endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_incidents_monitored_endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "monitored_endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_health_check_logs_EndpointId_CheckedAt",
                table: "health_check_logs",
                columns: new[] { "EndpointId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_incidents_EndpointId_ResolvedAt",
                table: "incidents",
                columns: new[] { "EndpointId", "ResolvedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_monitored_endpoints_UserId",
                table: "monitored_endpoints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_monitored_endpoints_UserId_IsEnabled",
                table: "monitored_endpoints",
                columns: new[] { "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_GoogleId",
                table: "users",
                column: "GoogleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_check_logs");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "monitored_endpoints");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
