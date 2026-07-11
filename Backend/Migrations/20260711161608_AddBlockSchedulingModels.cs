using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockSchedulingModels : Migration
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
                    so_tin_chi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    so_block_hoc = table.Column<int>(type: "int", nullable: false),
                    so_buoi_tren_tuan = table.Column<int>(type: "int", nullable: false),
                    so_ca_tren_buoi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyDoiTinChi", x => x.so_tin_chi);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "QuyDoiTinChi",
                columns: new[] { "so_tin_chi", "so_block_hoc", "so_buoi_tren_tuan", "so_ca_tren_buoi" },
                values: new object[,]
                {
                    { 1, 1, 2, 1 },
                    { 2, 1, 4, 1 },
                    { 3, 2, 3, 1 },
                    { 4, 2, 4, 1 },
                    { 5, 5, 1, 2 },
                    { 6, 5, 1, 2 },
                    { 7, 5, 1, 2 }
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
                name: "ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc");
        }
    }
}
