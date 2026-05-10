using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fixnow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchRadiusToWorkerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SearchRadius",
                table: "worker_profiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchRadius",
                table: "worker_profiles");
        }
    }
}
