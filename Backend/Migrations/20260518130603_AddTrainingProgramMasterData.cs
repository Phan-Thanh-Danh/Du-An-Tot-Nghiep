using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgramMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuyen_nganh_co_so = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_tuyen_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_code_chuong_trinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ten_chuong_trinh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    so_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_dao_tao_thang = table.Column<int>(type: "int", nullable: false),
                    tong_tin_chi_yeu_cau = table.Column<int>(type: "int", nullable: true),
                    so_tin_chi_toi_thieu_moi_ky = table.Column<int>(type: "int", nullable: true),
                    so_tin_chi_toi_da_moi_ky = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguon_chuong_trinh_id = table.Column<int>(type: "int", nullable: true),
                    ghi_chu_thay_doi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_het_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTao", x => x.ma_chuong_trinh);
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_so_hoc_ky", "[so_hoc_ky] > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_thoi_gian_dao_tao_thang", "[thoi_gian_dao_tao_thang] > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tin_chi_toi_da_moi_ky", "[so_tin_chi_toi_da_moi_ky] IS NULL OR [so_tin_chi_toi_da_moi_ky] > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tin_chi_toi_thieu_moi_ky", "[so_tin_chi_toi_thieu_moi_ky] IS NULL OR [so_tin_chi_toi_thieu_moi_ky] >= 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tong_tin_chi_yeu_cau", "[tong_tin_chi_yeu_cau] IS NULL OR [tong_tin_chi_yeu_cau] > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_trang_thai", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh_co_so__ChuyenNganhTheoCoSo",
                        column: x => x.ma_chuyen_nganh_co_so,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganhTheoCoSo",
                        principalColumn: "ma_chuyen_nganh_co_so",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTao_ma_khoa_tuyen_sinh__KhoaTuyenSinh",
                        column: x => x.ma_khoa_tuyen_sinh,
                        principalSchema: "dbo",
                        principalTable: "KhoaTuyenSinh",
                        principalColumn: "ma_khoa_tuyen_sinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTao_nguon_chuong_trinh_id__ChuongTrinhDaoTao",
                        column: x => x.nguon_chuong_trinh_id,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_ma_khoa_tuyen_sinh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_khoa_tuyen_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_nguon_chuong_trinh_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguon_chuong_trinh_id");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_co_so_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                columns: new[] { "ma_chuyen_nganh_co_so", "ma_khoa_tuyen_sinh", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_ma_code_chuong_trinh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_code_chuong_trinh",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTao",
                schema: "dbo");
        }
    }
}
