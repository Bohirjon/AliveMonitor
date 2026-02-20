using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliveMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSslCertificateChecking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SslCertificateExpiresAt",
                table: "monitored_endpoints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SslCheckEnabled",
                table: "monitored_endpoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SslLastAlertedThresholdDays",
                table: "monitored_endpoints",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SslLastCheckedAt",
                table: "monitored_endpoints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ssl_certificate_check_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    SubjectName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IssuerName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysUntilExpiry = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ssl_certificate_check_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ssl_certificate_check_logs_monitored_endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "monitored_endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ssl_certificate_check_logs_EndpointId_CheckedAt",
                table: "ssl_certificate_check_logs",
                columns: new[] { "EndpointId", "CheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ssl_certificate_check_logs");

            migrationBuilder.DropColumn(
                name: "SslCertificateExpiresAt",
                table: "monitored_endpoints");

            migrationBuilder.DropColumn(
                name: "SslCheckEnabled",
                table: "monitored_endpoints");

            migrationBuilder.DropColumn(
                name: "SslLastAlertedThresholdDays",
                table: "monitored_endpoints");

            migrationBuilder.DropColumn(
                name: "SslLastCheckedAt",
                table: "monitored_endpoints");
        }
    }
}
