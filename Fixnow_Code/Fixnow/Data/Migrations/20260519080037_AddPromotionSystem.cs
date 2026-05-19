using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionId",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "numeric", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxUsageLimit = table.Column<int>(type: "integer", nullable: false),
                    CurrentUsageCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicableServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promotions_services_ApplicableServiceId",
                        column: x => x.ApplicableServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_promotion_usages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_promotion_usages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_promotion_usages_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_promotion_usages_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_bookings_PromotionId",
                table: "bookings",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_promotions_ApplicableServiceId",
                table: "promotions",
                column: "ApplicableServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_promotions_Code",
                table: "promotions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_promotion_usages_PromotionId",
                table: "user_promotion_usages",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_promotion_usages_UserId_PromotionId",
                table: "user_promotion_usages",
                columns: new[] { "UserId", "PromotionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_promotions_PromotionId",
                table: "bookings",
                column: "PromotionId",
                principalTable: "promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_promotions_PromotionId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "user_promotion_usages");

            migrationBuilder.DropTable(
                name: "promotions");

            migrationBuilder.DropIndex(
                name: "IX_bookings_PromotionId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                table: "bookings");

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
    }
}
