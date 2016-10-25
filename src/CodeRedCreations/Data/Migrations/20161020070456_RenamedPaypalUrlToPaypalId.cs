using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeRedCreations.Data.Migrations
{
    public partial class RenamedPaypalUrlToPaypalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaypalUrl",
                table: "Part");

            migrationBuilder.AddColumn<string>(
                name: "PaypalId",
                table: "Part",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaypalId",
                table: "Part");

            migrationBuilder.AddColumn<string>(
                name: "PaypalUrl",
                table: "Part",
                nullable: true);
        }
    }
}
