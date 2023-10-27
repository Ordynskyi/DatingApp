using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore.Migrations;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhotosToModerateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModerationPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationPhotos_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationPhotos_AppUserId",
            table: "ModerationPhotos",
            column: "AppUserId");

            migrationBuilder.Sql(
                "INSERT INTO ModerationPhotos(Url, PublicId, AppUserId) " +
                "SELECT Url, PublicId, AppUserId " +
                "FROM Photos; " +

                "DELETE FROM Photos;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
                "INSERT INTO Photos(Url, PublicId, AppUserId) " +
                "SELECT Url, PublicId, AppUserId " +
                "FROM Photos; " +

                "DELETE FROM ModerationPhotos;");

            migrationBuilder.DropTable(
                name: "ModerationPhotos");
        }
    }
}
