using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace coderedcreations.Data.Migrations
{
    public partial class UserReferral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReferral",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Earnings = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    PayPalAccount = table.Column<string>(nullable: true),
                    PayoutPercent = table.Column<int>(nullable: false),
                    ReferralCode = table.Column<string>(nullable: true),
                    RequestedPayout = table.Column<bool>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReferral", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReferral");
        }
    }
}