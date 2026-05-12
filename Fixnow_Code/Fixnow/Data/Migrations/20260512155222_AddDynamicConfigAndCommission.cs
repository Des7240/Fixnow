using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicConfigAndCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_commissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_commissions_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    ConfigKey = table.Column<string>(type: "text", nullable: false),
                    ConfigValue = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configs", x => x.ConfigKey);
                });

            migrationBuilder.InsertData(
                table: "system_configs",
                columns: new[] { "ConfigKey", "ConfigValue", "Description", "UpdatedAt" },
                values: new object[,]
                {
                    { "DAILY_WITHDRAW_LIMIT", "50000000", "Hạn mức rút tiền tối đa trong ngày", new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9804) },
                    { "MAX_WITHDRAW_AMOUNT", "20000000", "Số tiền rút tối đa một lần", new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9803) },
                    { "MIN_WITHDRAW_AMOUNT", "50000", "Số tiền rút tối thiểu", new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9799) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_commissions_ServiceId",
                table: "service_commissions",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_commissions");

            migrationBuilder.DropTable(
                name: "system_configs");
        }
    }
}
