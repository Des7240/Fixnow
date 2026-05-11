using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenJobModerationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedRepairTimeMinutes",
                table: "worker_offers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "worker_offers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SpamScore",
                table: "worker_offers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyDays",
                table: "worker_offers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedReason",
                table: "open_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "open_jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxBudget",
                table: "open_jobs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinBudget",
                table: "open_jobs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "open_jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedBy",
                table: "open_jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationReason",
                table: "open_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "open_jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReportCount",
                table: "open_jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UrgencyLevel",
                table: "open_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "saved_open_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_open_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_saved_open_jobs_open_jobs_OpenJobId",
                        column: x => x.OpenJobId,
                        principalTable: "open_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saved_open_jobs_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_saved_open_jobs_OpenJobId",
                table: "saved_open_jobs",
                column: "OpenJobId");

            migrationBuilder.CreateIndex(
                name: "IX_saved_open_jobs_WorkerId_OpenJobId",
                table: "saved_open_jobs",
                columns: new[] { "WorkerId", "OpenJobId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_open_jobs");

            migrationBuilder.DropColumn(
                name: "EstimatedRepairTimeMinutes",
                table: "worker_offers");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "worker_offers");

            migrationBuilder.DropColumn(
                name: "SpamScore",
                table: "worker_offers");

            migrationBuilder.DropColumn(
                name: "WarrantyDays",
                table: "worker_offers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ClosedReason",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "MaxBudget",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "MinBudget",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ModeratedBy",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ModerationReason",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "ReportCount",
                table: "open_jobs");

            migrationBuilder.DropColumn(
                name: "UrgencyLevel",
                table: "open_jobs");
        }
    }
}
