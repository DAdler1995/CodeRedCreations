using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeRedCreations.Data.Migrations
{
    public partial class CarBrandPartMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrandModel",
                columns: table => new
                {
                    BrandId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandModel", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "PartModel",
                columns: table => new
                {
                    PartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BrandModelBrandId = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    OnSale = table.Column<bool>(nullable: false),
                    PartType = table.Column<int>(nullable: false),
                    PaypalUrl = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    Shipping = table.Column<decimal>(nullable: false),
                    Stock = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartModel", x => x.PartId);
                    table.ForeignKey(
                        name: "FK_PartModel_BrandModel_BrandModelBrandId",
                        column: x => x.BrandModelBrandId,
                        principalTable: "BrandModel",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarModel",
                columns: table => new
                {
                    CarId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Make = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    PartModelPartId = table.Column<int>(nullable: true),
                    TrimLevel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModel", x => x.CarId);
                    table.ForeignKey(
                        name: "FK_CarModel_PartModel_PartModelPartId",
                        column: x => x.PartModelPartId,
                        principalTable: "PartModel",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarModel_PartModelPartId",
                table: "CarModel",
                column: "PartModelPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartModel_BrandModelBrandId",
                table: "PartModel",
                column: "BrandModelBrandId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarModel");

            migrationBuilder.DropTable(
                name: "PartModel");

            migrationBuilder.DropTable(
                name: "BrandModel");
        }
    }
}
