using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hryhoriichuk.University.Instagram.Web.Migrations
{
    /// <inheritdoc />
    public partial class Privacy33 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FollowerId",
                table: "FollowRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "PrivacySettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    CommentPrivacy = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacySettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_PrivacySettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FollowerId",
                table: "FollowRequests",
                column: "FollowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequests_AspNetUsers_FollowerId",
                table: "FollowRequests",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowRequests_AspNetUsers_FollowerId",
                table: "FollowRequests");

            migrationBuilder.DropTable(
                name: "PrivacySettings");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_FollowerId",
                table: "FollowRequests");

            migrationBuilder.AlterColumn<string>(
                name: "FollowerId",
                table: "FollowRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
