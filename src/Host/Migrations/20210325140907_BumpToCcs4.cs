using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class BumpToCcs4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousRating",
                table: "ContestMembers");

            migrationBuilder.DropColumn(
                name: "RatingDelta",
                table: "ContestMembers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "ShortName" },
                values: new object[] { -32, "d68c9040-7762-ab0b-06eb-19ce1b5a5120", "Temporary Team Account", "TemporaryTeamAccount", "TEMPORARYTEAMACCOUNT", "temp_team" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -32);

            migrationBuilder.AddColumn<int>(
                name: "PreviousRating",
                table: "ContestMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingDelta",
                table: "ContestMembers",
                type: "int",
                nullable: true);
        }
    }
}
