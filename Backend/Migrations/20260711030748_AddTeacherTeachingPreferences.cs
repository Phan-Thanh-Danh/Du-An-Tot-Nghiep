using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherTeachingPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaoVienNguyenVongHocKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaoVien = table.Column<int>(type: "int", nullable: false),
                    MaHocKy = table.Column<int>(type: "int", nullable: false),
                    MaDonVi = table.Column<int>(type: "int", nullable: false),
                    SoLopToiDaMongMuon = table.Column<int>(type: "int", nullable: true),
                    SoCaToiDaMoiTuan = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienNguyenVongHocKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaoVienNguyenVongHocKy_DonVi_MaDonVi",
                        column: x => x.MaDonVi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoVienNguyenVongHocKy_HocKy_MaHocKy",
                        column: x => x.MaHocKy,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoVienNguyenVongHocKy_NguoiDung_MaGiaoVien",
                        column: x => x.MaGiaoVien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiaoVienNguyenVongCaDay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguyenVongId = table.Column<int>(type: "int", nullable: false),
                    ThuTrongTuan = table.Column<int>(type: "int", nullable: false),
                    MaCaHoc = table.Column<int>(type: "int", nullable: false),
                    MucDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienNguyenVongCaDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaoVienNguyenVongCaDay_CaHoc_MaCaHoc",
                        column: x => x.MaCaHoc,
                        principalSchema: "dbo",
                        principalTable: "CaHoc",
                        principalColumn: "ma_ca_hoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoVienNguyenVongCaDay_GiaoVienNguyenVongHocKy_NguyenVongId",
                        column: x => x.NguyenVongId,
                        principalTable: "GiaoVienNguyenVongHocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienNguyenVongCaDay_MaCaHoc",
                table: "GiaoVienNguyenVongCaDay",
                column: "MaCaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienNguyenVongCaDay_NguyenVongId_Thu_Ca",
                table: "GiaoVienNguyenVongCaDay",
                columns: new[] { "NguyenVongId", "ThuTrongTuan", "MaCaHoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienNguyenVongHocKy_MaDonVi",
                table: "GiaoVienNguyenVongHocKy",
                column: "MaDonVi");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienNguyenVongHocKy_MaGiaoVien_MaHocKy",
                table: "GiaoVienNguyenVongHocKy",
                columns: new[] { "MaGiaoVien", "MaHocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienNguyenVongHocKy_MaHocKy",
                table: "GiaoVienNguyenVongHocKy",
                column: "MaHocKy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaoVienNguyenVongCaDay");

            migrationBuilder.DropTable(
                name: "GiaoVienNguyenVongHocKy");
        }
    }
}
