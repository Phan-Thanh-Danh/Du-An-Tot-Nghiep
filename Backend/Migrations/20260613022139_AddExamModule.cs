using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddExamModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_luong",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "dang_hoat_dong",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "dang_hoat_dong");

            migrationBuilder.AddColumn<int>(
                name: "ma_ca_thi",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_xac_nhan_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_diem_ky",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_cong_bo",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "DeKiemTra",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "nhap",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "nhap");

            migrationBuilder.AddColumn<string>(
                name: "hinh_thuc_thi",
                schema: "dbo",
                table: "DeKiemTra",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_nguoi_duyet",
                schema: "dbo",
                table: "DeKiemTra",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_nguoi_soan",
                schema: "dbo",
                table: "DeKiemTra",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DeKiemTra",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "DeKiemTra",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_duyet",
                schema: "dbo",
                table: "DeKiemTra",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ty_le_trac_nghiem",
                schema: "dbo",
                table: "DeKiemTra",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ty_le_tu_luan",
                schema: "dbo",
                table: "DeKiemTra",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KyThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_ky_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_ky_thi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyThi", x => x.ma_ky_thi);
                    table.CheckConstraint("CK_KyThi_trang_thai", "[trang_thai] IN (N'nhap', N'dang_dien_ra', N'da_ket_thuc')");
                    table.ForeignKey(
                        name: "FK_KyThi_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "LichThiTong",
                schema: "dbo",
                columns: table => new
                {
                    ma_lich_thi_tong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ky_thi = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: true),
                    hinh_thuc_thi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ngay_thi_du_kien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichThiTong", x => x.ma_lich_thi_tong);
                    table.CheckConstraint("CK_LichThiTong_hinh_thuc_thi", "[hinh_thuc_thi] IN (N'online_tap_trung', N'online_tu_do', N'van_dap')");
                    table.CheckConstraint("CK_LichThiTong_trang_thai", "[trang_thai] IN (N'nhap', N'da_gui_ve_co_so', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_LichThiTong_ma_de_kiem_tra__DeKiemTra",
                        column: x => x.ma_de_kiem_tra,
                        principalSchema: "dbo",
                        principalTable: "DeKiemTra",
                        principalColumn: "ma_de_kiem_tra");
                    table.ForeignKey(
                        name: "FK_LichThiTong_ma_ky_thi__KyThi",
                        column: x => x.ma_ky_thi,
                        principalSchema: "dbo",
                        principalTable: "KyThi",
                        principalColumn: "ma_ky_thi");
                    table.ForeignKey(
                        name: "FK_LichThiTong_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "CaThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_lich_thi_tong = table.Column<int>(type: "int", nullable: false),
                    ten_ca_thi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_phong = table.Column<int>(type: "int", nullable: true),
                    ngay_thi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_dieu_chinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaThi", x => x.ma_ca_thi);
                    table.CheckConstraint("CK_CaThi_thoi_gian", "[thoi_gian_ket_thuc] > [thoi_gian_bat_dau]");
                    table.CheckConstraint("CK_CaThi_trang_thai", "[trang_thai] IN (N'nhap', N'cho_phan_cong', N'da_san_sang', N'dang_diem_danh', N'dang_thi', N'da_ket_thuc', N'da_huy', N'su_co')");
                    table.ForeignKey(
                        name: "FK_CaThi_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_CaThi_ma_lich_thi_tong__LichThiTong",
                        column: x => x.ma_lich_thi_tong,
                        principalSchema: "dbo",
                        principalTable: "LichThiTong",
                        principalColumn: "ma_lich_thi_tong");
                    table.ForeignKey(
                        name: "FK_CaThi_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "BienBanThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_bien_ban = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_phien_thi = table.Column<int>(type: "int", nullable: true),
                    loai_bien_ban = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ma_nguoi_lap = table.Column<int>(type: "int", nullable: false),
                    thoi_diem_lap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai_xu_ly = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_xu_ly"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienBanThi", x => x.ma_bien_ban);
                    table.CheckConstraint("CK_BienBanThi_loai_bien_ban", "[loai_bien_ban] IN (N'gian_lan', N'su_co_diem_danh', N'quen_ky_ten', N'su_co_he_thong', N'khac')");
                    table.CheckConstraint("CK_BienBanThi_trang_thai_xu_ly", "[trang_thai_xu_ly] IN (N'cho_xu_ly', N'da_xu_ly', N'huy_bo')");
                    table.ForeignKey(
                        name: "FK_BienBanThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_BienBanThi_ma_nguoi_lap__NguoiDung",
                        column: x => x.ma_nguoi_lap,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_BienBanThi_ma_phien_thi__PhienThiHocSinh",
                        column: x => x.ma_phien_thi,
                        principalSchema: "dbo",
                        principalTable: "PhienThiHocSinh",
                        principalColumn: "ma_phien_thi");
                });

            migrationBuilder.CreateTable(
                name: "DiemDanhThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_danh_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai_diem_danh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "vang_mat"),
                    thoi_diem_diem_danh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ma_nguoi_diem_danh = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanhThi", x => x.ma_diem_danh_thi);
                    table.CheckConstraint("CK_DiemDanhThi_trang_thai", "[trang_thai_diem_danh] IN (N'co_mat', N'vang_mat', N'di_muon_qua_gio', N'su_co')");
                    table.ForeignKey(
                        name: "FK_DiemDanhThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_DiemDanhThi_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_DiemDanhThi_ma_nguoi_diem_danh__NguoiDung",
                        column: x => x.ma_nguoi_diem_danh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyViPhamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_vi_pham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phien_thi = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    loai_vi_pham = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    muc_do = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhac_nho"),
                    chi_tiet_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoi_diem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyViPhamThi", x => x.ma_vi_pham);
                    table.CheckConstraint("CK_NhatKyViPhamThi_chi_tiet_json_ISJSON", "[chi_tiet_json] IS NULL OR ISJSON([chi_tiet_json]) = 1");
                    table.CheckConstraint("CK_NhatKyViPhamThi_loai_vi_pham", "[loai_vi_pham] IN (N'chuyen_tab', N'mat_focus', N'mat_camera', N'tieng_on', N'khac')");
                    table.CheckConstraint("CK_NhatKyViPhamThi_muc_do", "[muc_do] IN (N'nhac_nho', N'nghiem_trong')");
                    table.ForeignKey(
                        name: "FK_NhatKyViPhamThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_NhatKyViPhamThi_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_NhatKyViPhamThi_ma_phien_thi__PhienThiHocSinh",
                        column: x => x.ma_phien_thi,
                        principalSchema: "dbo",
                        principalTable: "PhienThiHocSinh",
                        principalColumn: "ma_phien_thi");
                });

            migrationBuilder.CreateTable(
                name: "PhanCongGiamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_phan_cong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_giam_thi = table.Column<int>(type: "int", nullable: false),
                    vai_tro_giam_thi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "giam_thi_phu"),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "du_kien"),
                    ly_do_thay_doi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiamThi", x => x.ma_phan_cong);
                    table.CheckConstraint("CK_PhanCongGiamThi_trang_thai", "[trang_thai] IN (N'du_kien', N'da_xac_nhan', N'thay_the', N'huy_phan_cong')");
                    table.CheckConstraint("CK_PhanCongGiamThi_vai_tro", "[vai_tro_giam_thi] IN (N'giam_thi_chinh', N'giam_thi_phu', N'ho_tro_ky_thuat')");
                    table.ForeignKey(
                        name: "FK_PhanCongGiamThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_PhanCongGiamThi_ma_giam_thi__NguoiDung",
                        column: x => x.ma_giam_thi,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "ThiSinhCaThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_thi_sinh_ca_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai_du_thi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_thi"),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThiSinhCaThi", x => x.ma_thi_sinh_ca_thi);
                    table.CheckConstraint("CK_ThiSinhCaThi_trang_thai_du_thi", "[trang_thai_du_thi] IN (N'cho_thi', N'duoc_thi', N'khong_duoc_thi', N'dinh_chi', N'vang_thi')");
                    table.ForeignKey(
                        name: "FK_ThiSinhCaThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_ThiSinhCaThi_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "XuLyViPhamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_xu_ly = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_vi_pham = table.Column<int>(type: "int", nullable: false),
                    hanh_dong_xu_ly = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    lan_nhac_nho = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_xu_ly = table.Column<int>(type: "int", nullable: false),
                    thoi_diem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuLyViPhamThi", x => x.ma_xu_ly);
                    table.CheckConstraint("CK_XuLyViPhamThi_hanh_dong", "[hanh_dong_xu_ly] IN (N'nhac_nho_he_thong', N'canh_bao_truc_tiep', N'dinh_chi_thi', N'bo_qua')");
                    table.CheckConstraint("CK_XuLyViPhamThi_lan_nhac_nho", "[lan_nhac_nho] >= 0");
                    table.ForeignKey(
                        name: "FK_XuLyViPhamThi_ma_nguoi_xu_ly__NguoiDung",
                        column: x => x.ma_nguoi_xu_ly,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_XuLyViPhamThi_ma_vi_pham__NhatKyViPhamThi",
                        column: x => x.ma_vi_pham,
                        principalSchema: "dbo",
                        principalTable: "NhatKyViPhamThi",
                        principalColumn: "ma_vi_pham");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhienThiHocSinh_nguoi_xac_nhan_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh",
                column: "nguoi_xac_nhan_ky_ten");

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_CaThi_HocSinh",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" },
                unique: true,
                filter: "[ma_ca_thi] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_cuoi_cung",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[diem_cuoi_cung] IS NULL OR [diem_cuoi_cung] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_tu_dong",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[diem_tu_dong] IS NULL OR [diem_tu_dong] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[diem_tu_luan_ai_goi_y] IS NULL OR [diem_tu_luan_ai_goi_y] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_trang_thai_cong_bo",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[trang_thai_cong_bo] IS NULL OR [trang_thai_cong_bo] IN (N'chua_co_diem', N'da_cham_xong', N'da_doc_diem', N'da_cong_bo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_trang_thai_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[trang_thai_ky_ten] IS NULL OR [trang_thai_ky_ten] IN (N'chua_ky', N'da_ky', N'quen_ky', N'su_co')");

            migrationBuilder.CreateIndex(
                name: "IX_DeKiemTra_ma_nguoi_duyet",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_DeKiemTra_ma_nguoi_soan",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_nguoi_soan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_hinh_thuc_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[hinh_thuc_thi] IS NULL OR [hinh_thuc_thi] IN (N'online_tap_trung', N'online_tu_do', N'van_dap')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_thoi_gian_phut",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[thoi_gian_phut] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_trang_thai_duyet",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[trang_thai_duyet] IS NULL OR [trang_thai_duyet] IN (N'nhap', N'cho_duyet', N'da_duyet', N'tu_choi')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_ty_le_trac_nghiem",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[ty_le_trac_nghiem] IS NULL OR [ty_le_trac_nghiem] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_ty_le_tu_luan",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[ty_le_tu_luan] IS NULL OR [ty_le_tu_luan] BETWEEN 0 AND 100");

            migrationBuilder.CreateIndex(
                name: "IX_BienBanThi_ma_ca_thi",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_ca_thi");

            migrationBuilder.CreateIndex(
                name: "IX_BienBanThi_ma_nguoi_lap",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_nguoi_lap");

            migrationBuilder.CreateIndex(
                name: "IX_BienBanThi_ma_phien_thi",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_phien_thi");

            migrationBuilder.CreateIndex(
                name: "IX_CaThi_ma_don_vi",
                schema: "dbo",
                table: "CaThi",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_CaThi_ma_lich_thi_tong",
                schema: "dbo",
                table: "CaThi",
                column: "ma_lich_thi_tong");

            migrationBuilder.CreateIndex(
                name: "IX_CaThi_ma_phong",
                schema: "dbo",
                table: "CaThi",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhThi_ma_hoc_sinh",
                schema: "dbo",
                table: "DiemDanhThi",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhThi_ma_nguoi_diem_danh",
                schema: "dbo",
                table: "DiemDanhThi",
                column: "ma_nguoi_diem_danh");

            migrationBuilder.CreateIndex(
                name: "UQ_DiemDanhThi_CaThi_HocSinh",
                schema: "dbo",
                table: "DiemDanhThi",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KyThi_ma_hoc_ky",
                schema: "dbo",
                table: "KyThi",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_LichThiTong_ma_de_kiem_tra",
                schema: "dbo",
                table: "LichThiTong",
                column: "ma_de_kiem_tra");

            migrationBuilder.CreateIndex(
                name: "IX_LichThiTong_ma_ky_thi",
                schema: "dbo",
                table: "LichThiTong",
                column: "ma_ky_thi");

            migrationBuilder.CreateIndex(
                name: "IX_LichThiTong_ma_mon_hoc",
                schema: "dbo",
                table: "LichThiTong",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyViPhamThi_CaThi_HocSinh",
                schema: "dbo",
                table: "NhatKyViPhamThi",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyViPhamThi_ma_hoc_sinh",
                schema: "dbo",
                table: "NhatKyViPhamThi",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyViPhamThi_ma_phien_thi",
                schema: "dbo",
                table: "NhatKyViPhamThi",
                column: "ma_phien_thi");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiamThi_ma_giam_thi",
                schema: "dbo",
                table: "PhanCongGiamThi",
                column: "ma_giam_thi");

            migrationBuilder.CreateIndex(
                name: "UQ_PhanCongGiamThi_CaThi_GiamThi",
                schema: "dbo",
                table: "PhanCongGiamThi",
                columns: new[] { "ma_ca_thi", "ma_giam_thi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThiSinhCaThi_ma_hoc_sinh",
                schema: "dbo",
                table: "ThiSinhCaThi",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "UQ_ThiSinhCaThi_CaThi_HocSinh",
                schema: "dbo",
                table: "ThiSinhCaThi",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XuLyViPhamThi_ma_nguoi_xu_ly",
                schema: "dbo",
                table: "XuLyViPhamThi",
                column: "ma_nguoi_xu_ly");

            migrationBuilder.CreateIndex(
                name: "IX_XuLyViPhamThi_ma_vi_pham",
                schema: "dbo",
                table: "XuLyViPhamThi",
                column: "ma_vi_pham");

            migrationBuilder.AddForeignKey(
                name: "FK_DeKiemTra_ma_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_nguoi_duyet",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DeKiemTra_ma_nguoi_soan__NguoiDung",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_nguoi_soan",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_PhienThiHocSinh_ma_ca_thi__CaThi",
                schema: "dbo",
                table: "PhienThiHocSinh",
                column: "ma_ca_thi",
                principalSchema: "dbo",
                principalTable: "CaThi",
                principalColumn: "ma_ca_thi");

            migrationBuilder.AddForeignKey(
                name: "FK_PhienThiHocSinh_nguoi_xac_nhan_ky_ten__NguoiDung",
                schema: "dbo",
                table: "PhienThiHocSinh",
                column: "nguoi_xac_nhan_ky_ten",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeKiemTra_ma_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropForeignKey(
                name: "FK_DeKiemTra_ma_nguoi_soan__NguoiDung",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropForeignKey(
                name: "FK_PhienThiHocSinh_ma_ca_thi__CaThi",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropForeignKey(
                name: "FK_PhienThiHocSinh_nguoi_xac_nhan_ky_ten__NguoiDung",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropTable(
                name: "BienBanThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiemDanhThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhanCongGiamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThiSinhCaThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XuLyViPhamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyViPhamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CaThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LichThiTong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KyThi",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_PhienThiHocSinh_nguoi_xac_nhan_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropIndex(
                name: "UQ_PhienThiHocSinh_CaThi_HocSinh",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_cuoi_cung",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_tu_dong",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_trang_thai_cong_bo",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_trang_thai_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropIndex(
                name: "IX_DeKiemTra_ma_nguoi_duyet",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropIndex(
                name: "IX_DeKiemTra_ma_nguoi_soan",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_hinh_thuc_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_thoi_gian_phut",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_trang_thai_duyet",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_ty_le_trac_nghiem",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_ty_le_tu_luan",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ma_ca_thi",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "nguoi_xac_nhan_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "thoi_diem_ky",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "trang_thai_cong_bo",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "trang_thai_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "hinh_thuc_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ma_nguoi_duyet",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ma_nguoi_soan",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "trang_thai_duyet",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ty_le_trac_nghiem",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "ty_le_tu_luan",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_luong",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "dang_hoat_dong",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "dang_hoat_dong");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "DeKiemTra",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "nhap",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "nhap");
        }
    }
}
