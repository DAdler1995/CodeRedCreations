using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeRedCreations.Data.Migrations
{
    public partial class BrandModelGetPartIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Part_Brand_BrandModelBrandId",
                table: "Part");

            migrationBuilder.DropIndex(
                name: "IX_Part_BrandModelBrandId",
                table: "Part");

            migrationBuilder.DropColumn(
                name: "BrandModelBrandId",
                table: "Part");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandModelBrandId",
                table: "Part",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Part_BrandModelBrandId",
                table: "Part",
                column: "BrandModelBrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Part_Brand_BrandModelBrandId",
                table: "Part",
                column: "BrandModelBrandId",
                principalTable: "Brand",
                principalColumn: "BrandId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
