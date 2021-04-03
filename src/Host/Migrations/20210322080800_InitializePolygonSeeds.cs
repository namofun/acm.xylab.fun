using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20210322080800_InitializePolygonSeeds")]
    public partial class InitializePolygonSeeds : Migration
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
