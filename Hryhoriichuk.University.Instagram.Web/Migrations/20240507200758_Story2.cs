using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hryhoriichuk.University.Instagram.Web.Migrations
{
    /// <inheritdoc />
    public partial class Story2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileId",
                table: "Stories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_ProfileId",
                table: "Stories",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Profile_ProfileId",
                table: "Stories",
                column: "ProfileId",
                principalTable: "Profile",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Profile_ProfileId",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_ProfileId",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Stories");
        }
    }
}
