using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardCandidateApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_nguoi_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "nguoi_dieu_chinh");

            migrationBuilder.AddForeignKey(
                name: "FK_UngVienKhenThuong_nguoi_dieu_chinh__NguoiDung",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "nguoi_dieu_chinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UngVienKhenThuong_nguoi_dieu_chinh__NguoiDung",
                schema: "dbo",
                table: "UngVienKhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_UngVienKhenThuong_nguoi_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong");

            migrationBuilder.DropColumn(
                name: "ghi_chu_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong");

            migrationBuilder.DropColumn(
                name: "ngay_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong");

            migrationBuilder.DropColumn(
                name: "nguoi_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong");
        }
    }
}
