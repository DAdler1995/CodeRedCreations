using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coderedcreations.Data.Migrations
{
    public partial class CarProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Car_Products_ProductPartId",
                table: "Car");

            migrationBuilder.DropIndex(
                name: "IX_Car_ProductPartId",
                table: "Car");

            migrationBuilder.DropColumn(
                name: "ProductPartId",
                table: "Car");

            migrationBuilder.CreateTable(
                name: "CarProduct",
                columns: table => new
                {
                    ProductId = table.Column<int>(nullable: false),
                    CarId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarProduct", x => new { x.ProductId, x.CarId });
                    table.ForeignKey(
                        name: "FK_CarProduct_Car_CarId",
                        column: x => x.CarId,
                        principalTable: "Car",
                        principalColumn: "CarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarProduct_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarProduct_CarId",
                table: "CarProduct",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarProduct_ProductId",
                table: "CarProduct",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarProduct");

            migrationBuilder.AddColumn<int>(
                name: "ProductPartId",
                table: "Car",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Car_ProductPartId",
                table: "Car",
                column: "ProductPartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Car_Products_ProductPartId",
                table: "Car",
                column: "ProductPartId",
                principalTable: "Products",
                principalColumn: "PartId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
