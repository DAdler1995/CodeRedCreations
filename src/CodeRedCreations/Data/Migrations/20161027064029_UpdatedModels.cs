using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeRedCreations.Data.Migrations
{
    public partial class UpdatedModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Promos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: false),
                    DiscountAmount = table.Column<decimal>(nullable: true),
                    DiscountPercentage = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promos", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "PromoModelId",
                table: "Part",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Part_PromoModelId",
                table: "Part",
                column: "PromoModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Part_Promos_PromoModelId",
                table: "Part",
                column: "PromoModelId",
                principalTable: "Promos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Part_Promos_PromoModelId",
                table: "Part");

            migrationBuilder.DropIndex(
                name: "IX_Part_PromoModelId",
                table: "Part");

            migrationBuilder.DropColumn(
                name: "PromoModelId",
                table: "Part");

            migrationBuilder.DropTable(
                name: "Promos");
        }
    }
}
