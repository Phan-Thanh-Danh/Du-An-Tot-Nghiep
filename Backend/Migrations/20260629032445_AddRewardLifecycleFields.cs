using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardLifecycleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_vong_doi",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap",
                schema: "dbo",
                table: "KhenThuong",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ghi_chu_vong_doi",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ngay_cap",
                schema: "dbo",
                table: "KhenThuong");
        }
    }
}
