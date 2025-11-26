using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoEnricher.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoUrl = table.Column<string>(type: "text", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoMetadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoMetadata_SongId",
                table: "VideoMetadata",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoMetadata");
        }
    }
}
