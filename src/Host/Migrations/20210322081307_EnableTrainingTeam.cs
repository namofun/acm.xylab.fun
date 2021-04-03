using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20210322081307_EnableTrainingTeam")]
    public partial class EnableTrainingTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantTrainingTeams",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(maxLength: 128, nullable: false),
                    AffiliationId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Time = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTrainingTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantTrainingTeams_TenantAffiliation_AffiliationId",
                        column: x => x.AffiliationId,
                        principalTable: "TenantAffiliation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantTrainingTeams_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantTrainingUsers",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Accepted = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTrainingUsers", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TenantTrainingUsers_TenantTrainingTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TenantTrainingTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantTrainingUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantTrainingTeams_AffiliationId",
                table: "TenantTrainingTeams",
                column: "AffiliationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantTrainingTeams_UserId",
                table: "TenantTrainingTeams",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantTrainingUsers_UserId",
                table: "TenantTrainingUsers",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantTrainingUsers");

            migrationBuilder.DropTable(
                name: "TenantTrainingTeams");
        }
    }
}
