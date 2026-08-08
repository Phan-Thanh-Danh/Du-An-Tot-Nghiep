using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddQuyDinhChuyenCan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuyDinhChuyenCan",
                schema: "dbo",
                columns: table => new
                {
                    ma_quy_dinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ngay_hieu_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quy_vang_toi_da = table.Column<int>(type: "int", nullable: false),
                    ti_le_canh_bao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_vang_khong_phep = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_vang_co_phep = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_di_muon = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    han_gui_phut = table.Column<int>(type: "int", nullable: false),
                    han_chinh_sua_phut = table.Column<int>(type: "int", nullable: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    tao_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyDinhChuyenCan", x => x.ma_quy_dinh);
                    table.ForeignKey(
                        name: "FK_QuyDinhChuyenCan_DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuyDinhChuyenCan_NguoiTao",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuyDinhChuyenCan_MaDonVi_NgayHieuLuc",
                schema: "dbo",
                table: "QuyDinhChuyenCan",
                columns: new[] { "ma_don_vi", "ngay_hieu_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuyDinhChuyenCan_nguoi_tao",
                schema: "dbo",
                table: "QuyDinhChuyenCan",
                column: "nguoi_tao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuyDinhChuyenCan",
                schema: "dbo");
        }
    }
}
