using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20220320133917_RemoveOneSetting")]
    public partial class RemoveOneSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configurations",
                keyColumn: "Name",
                keyValue: "email_sender_url");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Name", "Category", "Description", "DisplayPriority", "Public", "Type", "Value" },
                values: new object[] { "email_sender_url", "Identity", "The HTTP URL of logic app to send the email.", 1, true, "string", "\"\"" });
        }
    }
}
