using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coderedcreations.Data.Migrations
{
    public partial class ProductSales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnSale",
                table: "Products",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SaleAmount",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleExpiration",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalePercent",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnSale",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleExpiration",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalePercent",
                table: "Products");
        }
    }
}
