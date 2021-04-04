using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class PolygonSeeds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Polygon.Storages.SeedMigrationV1.Up(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
