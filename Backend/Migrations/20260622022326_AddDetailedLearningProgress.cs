using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedLearningProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhienHocNoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_phien_hoc = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nhip_tim_cuoi_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    so_giay_hoat_dong_da_xac_nhan = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    vi_tri_video_cuoi_giay = table.Column<int>(type: "int", nullable: true),
                    phan_tram_cuon_lon_nhat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    so_thu_tu_nhip_tim_cuoi = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "dang_hoat_dong"),
                    user_agent_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    dia_chi_ip_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienHocNoiDung", x => x.ma_phien_hoc);
                    table.CheckConstraint("CK_PhienHocNoiDung_TrangThai", "[trang_thai] IN (N'dang_hoat_dong', N'da_ket_thuc', N'het_han', N'bi_thay_the')");
                    table.ForeignKey(
                        name: "FK_PhienHocNoiDung_MaHocSinh_NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_PhienHocNoiDung_MaNoiDung_BaiHocNoiDung",
                        column: x => x.ma_noi_dung,
                        principalSchema: "dbo",
                        principalTable: "BaiHocNoiDung",
                        principalColumn: "ma_noi_dung");
                });

            migrationBuilder.CreateTable(
                name: "TienDoNoiDungHocTap",
                schema: "dbo",
                columns: table => new
                {
                    ma_tien_do_noi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false),
                    loai_noi_dung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_bat_dau"),
                    phan_tram_tien_do = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    so_giay_da_xac_nhan = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    vi_tri_video_cuoi_giay = table.Column<int>(type: "int", nullable: true),
                    phan_tram_cuon_lon_nhat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    chi_so_muc_cuoi = table.Column<int>(type: "int", nullable: true),
                    so_muc_da_xem = table.Column<int>(type: "int", nullable: true),
                    tong_so_muc = table.Column<int>(type: "int", nullable: true),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lan_tuong_tac_cuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hoan_thanh_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    chi_tiet_tien_do_json = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienDoNoiDungHocTap", x => x.ma_tien_do_noi_dung);
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_PhanTramCuonLonNhat", "[phan_tram_cuon_lon_nhat] IS NULL OR [phan_tram_cuon_lon_nhat] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_PhanTramTienDo", "[phan_tram_tien_do] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_SoGiayDaXacNhan", "[so_giay_da_xac_nhan] >= 0");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_TrangThai", "[trang_thai] IN (N'chua_bat_dau', N'dang_hoc', N'hoan_thanh')");
                    table.ForeignKey(
                        name: "FK_TienDoNoiDungHocTap_MaHocSinh_NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_TienDoNoiDungHocTap_MaNoiDung_BaiHocNoiDung",
                        column: x => x.ma_noi_dung,
                        principalSchema: "dbo",
                        principalTable: "BaiHocNoiDung",
                        principalColumn: "ma_noi_dung");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhienHocNoiDung_HocSinh_NoiDung_TrangThai",
                schema: "dbo",
                table: "PhienHocNoiDung",
                columns: new[] { "ma_hoc_sinh", "ma_noi_dung", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_PhienHocNoiDung_ma_noi_dung",
                schema: "dbo",
                table: "PhienHocNoiDung",
                column: "ma_noi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_PhienHocNoiDung_NhipTimCuoiLuc",
                schema: "dbo",
                table: "PhienHocNoiDung",
                column: "nhip_tim_cuoi_luc");

            migrationBuilder.CreateIndex(
                name: "UQ_PhienHocNoiDung_SessionToken",
                schema: "dbo",
                table: "PhienHocNoiDung",
                column: "session_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TienDoNoiDungHocTap_LanTuongTacCuoi",
                schema: "dbo",
                table: "TienDoNoiDungHocTap",
                column: "lan_tuong_tac_cuoi");

            migrationBuilder.CreateIndex(
                name: "IX_TienDoNoiDungHocTap_MaHocSinh",
                schema: "dbo",
                table: "TienDoNoiDungHocTap",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_TienDoNoiDungHocTap_MaNoiDung",
                schema: "dbo",
                table: "TienDoNoiDungHocTap",
                column: "ma_noi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_TienDoNoiDungHocTap_TrangThai",
                schema: "dbo",
                table: "TienDoNoiDungHocTap",
                column: "trang_thai");

            migrationBuilder.CreateIndex(
                name: "UQ_TienDoNoiDungHocTap_HocSinh_NoiDung",
                schema: "dbo",
                table: "TienDoNoiDungHocTap",
                columns: new[] { "ma_hoc_sinh", "ma_noi_dung" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhienHocNoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TienDoNoiDungHocTap",
                schema: "dbo");
        }
    }
}
