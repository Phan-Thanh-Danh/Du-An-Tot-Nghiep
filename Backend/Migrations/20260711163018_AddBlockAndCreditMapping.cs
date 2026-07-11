using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockAndCreditMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "si_so_du_kien",
                schema: "dbo",
                table: "LopHanhChinh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoBlockHoc",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Block",
                schema: "dbo",
                columns: table => new
                {
                    ma_block = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_block = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    thu_tu_block = table.Column<int>(type: "int", nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.ma_block);
                    table.ForeignKey(
                        name: "FK_Block_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyDoiTinChi",
                schema: "dbo",
                columns: table => new
                {
                    ma_quy_doi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    so_block_hoc = table.Column<int>(type: "int", nullable: false),
                    so_buoi_moi_tuan = table.Column<int>(type: "int", nullable: false),
                    so_ca_moi_buoi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyDoiTinChi", x => x.ma_quy_doi);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "QuyDoiTinChi",
                columns: new[] { "ma_quy_doi", "so_block_hoc", "so_buoi_moi_tuan", "so_ca_moi_buoi", "so_tin_chi" },
                values: new object[,]
                {
                    { 1, 1, 2, 1, 2 },
                    { 2, 1, 3, 1, 3 },
                    { 3, 2, 2, 1, 4 },
                    { 4, 2, 3, 1, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_block_bat_dau");

            migrationBuilder.CreateIndex(
                name: "IX_Block_ma_hoc_ky",
                schema: "dbo",
                table: "Block",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_QuyDoiTinChi_SoTinChi",
                schema: "dbo",
                table: "QuyDoiTinChi",
                column: "so_tin_chi",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_block_bat_dau__Block",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_block_bat_dau",
                principalSchema: "dbo",
                principalTable: "Block",
                principalColumn: "ma_block");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhoaHoc_ma_block_bat_dau__Block",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropTable(
                name: "Block",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyDoiTinChi",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_KhoaHoc_ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "si_so_du_kien",
                schema: "dbo",
                table: "LopHanhChinh");

            migrationBuilder.DropColumn(
                name: "SoBlockHoc",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc");
        }
    }
}
