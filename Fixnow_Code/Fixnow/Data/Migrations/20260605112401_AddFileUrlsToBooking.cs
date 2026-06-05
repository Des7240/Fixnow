using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUrlsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "FileUrls",
                table: "bookings",
                type: "text[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::text[]");

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "DAILY_WITHDRAW_LIMIT",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 5, 11, 23, 58, 762, DateTimeKind.Utc).AddTicks(3670));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MAX_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 5, 11, 23, 58, 762, DateTimeKind.Utc).AddTicks(3669));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MIN_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 5, 11, 23, 58, 762, DateTimeKind.Utc).AddTicks(3663));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrls",
                table: "bookings");

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "DAILY_WITHDRAW_LIMIT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 19, 8, 0, 34, 494, DateTimeKind.Utc).AddTicks(5470));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MAX_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 19, 8, 0, 34, 494, DateTimeKind.Utc).AddTicks(5468));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MIN_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 19, 8, 0, 34, 494, DateTimeKind.Utc).AddTicks(5461));
        }
    }
}
