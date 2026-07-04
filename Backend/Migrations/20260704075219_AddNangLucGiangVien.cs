using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNangLucGiangVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NangLucGiangVien",
                columns: table => new
                {
                    MaNangLuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaoVien = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    MucDoPhuHop = table.Column<int>(type: "int", nullable: false),
                    SoLanDaDay = table.Column<int>(type: "int", nullable: false),
                    UuTien = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NangLucGiangVien", x => x.MaNangLuc);
                    table.ForeignKey(
                        name: "FK_NangLucGiangVien_DanhMucMonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NangLucGiangVien_NguoiDung_MaGiaoVien",
                        column: x => x.MaGiaoVien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NangLucGiangVien_MaGiaoVien",
                table: "NangLucGiangVien",
                column: "MaGiaoVien");

            migrationBuilder.CreateIndex(
                name: "IX_NangLucGiangVien_MaMonHoc",
                table: "NangLucGiangVien",
                column: "MaMonHoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NangLucGiangVien");
        }
    }
}
