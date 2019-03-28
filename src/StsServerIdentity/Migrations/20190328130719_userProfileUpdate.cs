using Microsoft.EntityFrameworkCore.Migrations;

namespace StsServerIdentity.Migrations
{
    public partial class userProfileUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEventRecordsRole",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SecuredFilesRole",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataEventRecordsRole",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecuredFilesRole",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
