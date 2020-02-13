using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Server.Migrations.ApplicationDb
{
    public partial class SimplerUserProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsNavMinified",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsNavOpen",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastPageVisited",
                table: "UserProfiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsNavMinified",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNavOpen",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastPageVisited",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
