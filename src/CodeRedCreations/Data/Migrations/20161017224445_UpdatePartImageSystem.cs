using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeRedCreations.Data.Migrations
{
    public partial class UpdatePartImageSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "Part");

            migrationBuilder.AddColumn<string>(
                name: "ImageStrings",
                table: "Part",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageStrings",
                table: "Part");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "Part",
                nullable: true);
        }
    }
}
