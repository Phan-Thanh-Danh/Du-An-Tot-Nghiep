using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardCertificatePdfFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "loi_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "so_lan_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_so_lan_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[so_lan_sinh_pdf] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_so_lan_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "loi_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ngay_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "so_lan_sinh_pdf",
                schema: "dbo",
                table: "KhenThuong");
        }
    }
}
