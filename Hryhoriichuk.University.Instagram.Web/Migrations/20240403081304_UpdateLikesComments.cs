﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hryhoriichuk.University.Instagram.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLikesComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comment",
                newName: "Text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Comment",
                newName: "Content");
        }
    }
}
