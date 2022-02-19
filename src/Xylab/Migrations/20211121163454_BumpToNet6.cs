using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20211121163454_BumpToNet6")]
    public partial class BumpToNet6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Name", "Category", "Description", "DisplayPriority", "Public", "Type", "Value" },
                values: new object[] { "contest_last_rating_change_time", "Contest", "Last rating update time.", 0, false, "datetime", "null" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configurations",
                keyColumn: "Name",
                keyValue: "contest_last_rating_change_time");
        }
    }
}
