using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenJobFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OpenJobId",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "open_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    Lng = table.Column<double>(type: "double precision", nullable: false),
                    Location = table.Column<Point>(type: "geography(Point, 4326)", nullable: false),
                    RadiusKm = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_open_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_open_jobs_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_open_jobs_users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "open_job_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_open_job_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_open_job_attachments_open_jobs_OpenJobId",
                        column: x => x.OpenJobId,
                        principalTable: "open_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_open_job_attachments_uploaded_files_FileId",
                        column: x => x.FileId,
                        principalTable: "uploaded_files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "worker_offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimatedPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Analysis = table.Column<string>(type: "text", nullable: false),
                    EstimatedArrivalMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_worker_offers_open_jobs_OpenJobId",
                        column: x => x.OpenJobId,
                        principalTable: "open_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_worker_offers_users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "offer_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offer_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offer_attachments_uploaded_files_FileId",
                        column: x => x.FileId,
                        principalTable: "uploaded_files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_offer_attachments_worker_offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "worker_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_OpenJobId",
                table: "bookings",
                column: "OpenJobId");

            migrationBuilder.CreateIndex(
                name: "IX_offer_attachments_FileId",
                table: "offer_attachments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_offer_attachments_OfferId",
                table: "offer_attachments",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_open_job_attachments_FileId",
                table: "open_job_attachments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_open_job_attachments_OpenJobId",
                table: "open_job_attachments",
                column: "OpenJobId");

            migrationBuilder.CreateIndex(
                name: "IX_open_jobs_CustomerId",
                table: "open_jobs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_open_jobs_ServiceId",
                table: "open_jobs",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_offers_OpenJobId",
                table: "worker_offers",
                column: "OpenJobId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_offers_WorkerId",
                table: "worker_offers",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_open_jobs_OpenJobId",
                table: "bookings",
                column: "OpenJobId",
                principalTable: "open_jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_open_jobs_OpenJobId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "offer_attachments");

            migrationBuilder.DropTable(
                name: "open_job_attachments");

            migrationBuilder.DropTable(
                name: "worker_offers");

            migrationBuilder.DropTable(
                name: "open_jobs");

            migrationBuilder.DropIndex(
                name: "IX_bookings_OpenJobId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "OpenJobId",
                table: "bookings");
        }
    }
}
