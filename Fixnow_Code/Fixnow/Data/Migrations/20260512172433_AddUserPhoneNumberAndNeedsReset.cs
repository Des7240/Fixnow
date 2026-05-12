using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhoneNumberAndNeedsReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsPasswordReset",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "DAILY_WITHDRAW_LIMIT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 24, 32, 417, DateTimeKind.Utc).AddTicks(3275));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MAX_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 24, 32, 417, DateTimeKind.Utc).AddTicks(3274));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MIN_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 24, 32, 417, DateTimeKind.Utc).AddTicks(3270));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsPasswordReset",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "users");

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "DAILY_WITHDRAW_LIMIT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MAX_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9803));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MIN_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 15, 52, 21, 494, DateTimeKind.Utc).AddTicks(9799));
        }
    }
}
