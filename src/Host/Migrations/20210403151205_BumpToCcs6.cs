using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20210403151205_BumpToCcs6")]
    public partial class BumpToCcs6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ComapreArguments",
                table: "PolygonProblems",
                newName: "CompareArguments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationTime",
                table: "TenantTeachingClasses",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TenantTeachingClasses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "TenantTeachingClasses",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "PolygonAuthors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [PolygonAuthors] SET [Level] = 1");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ContestJury",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [ContestJury] SET [Level] = 10");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -11,
                columns: new[] { "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "f25ae969-433c-3f4a-04ca-7ec12d2583cc", "ProblemCreator", "PROBLEMCREATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName", "ShortName" },
                values: new object[] { -17, "89679840-61ae-1966-f3cc-56e0d6eb43a3", "Team Leader", "TeamLeader", "TEAMLEADER", "leader" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantTeachingClasses_UserId",
                table: "TenantTeachingClasses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_TenantStudents_StudentId",
                table: "AspNetUsers",
                column: "StudentId",
                principalTable: "TenantStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantTeachingClasses_AspNetUsers_UserId",
                table: "TenantTeachingClasses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_TenantStudents_StudentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantTeachingClasses_AspNetUsers_UserId",
                table: "TenantTeachingClasses");

            migrationBuilder.DropIndex(
                name: "IX_TenantTeachingClasses_UserId",
                table: "TenantTeachingClasses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "TenantTeachingClasses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TenantTeachingClasses");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "TenantTeachingClasses");

            migrationBuilder.RenameColumn(
                name: "CompareArguments",
                table: "PolygonProblems",
                newName: "ComapreArguments");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "PolygonAuthors");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ContestJury");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: -11,
                columns: new[] { "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "215260d1-ee7b-b826-38c4-f76639a9a354", "Problem", "PROBLEM" });
        }
    }
}
