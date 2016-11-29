using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coderedcreations.Data.Migrations
{
    public partial class ReplaceTrimWithYears : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrimLevel",
                table: "Car");

            migrationBuilder.AddColumn<string>(
                name: "Years",
                table: "Car",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Years",
                table: "Car");

            migrationBuilder.AddColumn<string>(
                name: "TrimLevel",
                table: "Car",
                nullable: true);
        }
    }
}
