using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20220424071721_RemoveOjUpdate")]
    public partial class RemoveOjUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantSolveRecord");

            migrationBuilder.DeleteData(
                table: "Configurations",
                keyColumn: "Name",
                keyValue: "oj_Codeforces_update_time");

            migrationBuilder.DeleteData(
                table: "Configurations",
                keyColumn: "Name",
                keyValue: "oj_Hdoj_update_time");

            migrationBuilder.DeleteData(
                table: "Configurations",
                keyColumn: "Name",
                keyValue: "oj_Vjudge_update_time");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantSolveRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Account = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSolveRecord", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Name", "Category", "Description", "DisplayPriority", "Public", "Type", "Value" },
                values: new object[] { "oj_Codeforces_update_time", "Tenant", "Last update time of Codeforces.", 0, false, "datetime", "null" });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Name", "Category", "Description", "DisplayPriority", "Public", "Type", "Value" },
                values: new object[] { "oj_Hdoj_update_time", "Tenant", "Last update time of HDOJ.", 2, false, "datetime", "null" });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Name", "Category", "Description", "DisplayPriority", "Public", "Type", "Value" },
                values: new object[] { "oj_Vjudge_update_time", "Tenant", "Last update time of Vjudge.", 1, false, "datetime", "null" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantSolveRecord_Category",
                table: "TenantSolveRecord",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSolveRecord_Category_Grade",
                table: "TenantSolveRecord",
                columns: new[] { "Category", "Grade" });
        }
    }
}
