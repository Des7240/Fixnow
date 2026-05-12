using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUserPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "DAILY_WITHDRAW_LIMIT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 36, 33, 753, DateTimeKind.Utc).AddTicks(7532));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MAX_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 36, 33, 753, DateTimeKind.Utc).AddTicks(7531));

            migrationBuilder.UpdateData(
                table: "system_configs",
                keyColumn: "ConfigKey",
                keyValue: "MIN_WITHDRAW_AMOUNT",
                column: "UpdatedAt",
                value: new DateTime(2026, 5, 12, 17, 36, 33, 753, DateTimeKind.Utc).AddTicks(7526));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");

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
    }
}
