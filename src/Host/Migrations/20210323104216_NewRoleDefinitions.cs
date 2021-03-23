using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class NewRoleDefinitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "ShortName" },
                values: new object[] { -30, "40100c40-6ca5-7bcb-48bc-41f2a939cbee", "CDS API user", "CDS", "CDS", "cds_api" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "ShortName" },
                values: new object[] { -31, "8f8c37e1-a309-bd2d-6708-0519df89139b", "Contest Creator", "ContestCreator", "CONTESTCREATOR", "cont" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -31);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -30);
        }
    }
}
