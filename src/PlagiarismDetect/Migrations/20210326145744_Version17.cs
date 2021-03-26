using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class Version17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Justification",
                table: "PlagiarismReports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Justification",
                table: "PlagiarismReports");
        }
    }
}
