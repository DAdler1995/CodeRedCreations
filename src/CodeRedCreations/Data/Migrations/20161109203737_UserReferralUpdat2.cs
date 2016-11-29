using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coderedcreations.Data.Migrations
{
    public partial class UserReferralUpdat2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalAccount",
                table: "UserReferral",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequestedPayout",
                table: "UserReferral",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalAccount",
                table: "UserReferral");

            migrationBuilder.DropColumn(
                name: "RequestedPayout",
                table: "UserReferral");
        }
    }
}
