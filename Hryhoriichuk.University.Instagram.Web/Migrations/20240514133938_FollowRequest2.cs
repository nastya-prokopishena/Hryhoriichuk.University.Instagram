using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hryhoriichuk.University.Instagram.Web.Migrations
{
    /// <inheritdoc />
    public partial class FollowRequest2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowRequest_Profile_ProfileId",
                table: "FollowRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FollowRequest",
                table: "FollowRequest");

            migrationBuilder.RenameTable(
                name: "FollowRequest",
                newName: "FollowRequests");

            migrationBuilder.RenameIndex(
                name: "IX_FollowRequest_ProfileId",
                table: "FollowRequests",
                newName: "IX_FollowRequests_ProfileId");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FollowRequests",
                table: "FollowRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequests_Profile_ProfileId",
                table: "FollowRequests",
                column: "ProfileId",
                principalTable: "Profile",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowRequests_Profile_ProfileId",
                table: "FollowRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FollowRequests",
                table: "FollowRequests");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "FollowRequests",
                newName: "FollowRequest");

            migrationBuilder.RenameIndex(
                name: "IX_FollowRequests_ProfileId",
                table: "FollowRequest",
                newName: "IX_FollowRequest_ProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FollowRequest",
                table: "FollowRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequest_Profile_ProfileId",
                table: "FollowRequest",
                column: "ProfileId",
                principalTable: "Profile",
                principalColumn: "Id");
        }
    }
}
