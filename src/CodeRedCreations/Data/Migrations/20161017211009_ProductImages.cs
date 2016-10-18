using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeRedCreations.Data.Migrations
{
    public partial class ProductImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgurAlbumUrl",
                table: "Part");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImgurAlbumUrl",
                table: "Part",
                nullable: true);
        }
    }
}
