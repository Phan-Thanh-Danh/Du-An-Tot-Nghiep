using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "CaHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_ca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    buoi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time", nullable: false),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    thu_tu = table.Column<int>(type: "int", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaHoc", x => x.ma_ca_hoc);
                    table.CheckConstraint("CK_CaHoc_buoi", "[buoi] IN (N'sang', N'chieu', N'toi')");
                    table.CheckConstraint("CK_CaHoc_gio", "[gio_ket_thuc] > [gio_bat_dau]");
                    table.CheckConstraint("CK_CaHoc_thu_tu", "[thu_tu] > 0");
                });

            migrationBuilder.CreateTable(
                name: "CauHoiDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi_dg = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    noi_dung_cau_hoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiDanhGia", x => x.ma_cau_hoi_dg);
                });

            migrationBuilder.CreateTable(
                name: "CauHoiThuongGap",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi_faq = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    danh_muc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    cau_hoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tra_loi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiThuongGap", x => x.ma_cau_hoi_faq);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code_mon_hoc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_mon_hoc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucMonHoc", x => x.ma_mon_hoc);
                    table.CheckConstraint("CK_DanhMucMonHoc_so_tin_chi_1", "[so_tin_chi] > 0");
                });

            migrationBuilder.CreateTable(
                name: "DonVi",
                schema: "dbo",
                columns: table => new
                {
                    ma_don_vi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi_cha = table.Column<int>(type: "int", nullable: true),
                    ten_don_vi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cap_don_vi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVi", x => x.ma_don_vi);
                    table.CheckConstraint("CK_DonVi_cap_don_vi_1", "[cap_don_vi] IN (N'root', N'co_so', N'co_so_con')");
                    table.ForeignKey(
                        name: "FK_DonVi_ma_don_vi_cha__DonVi",
                        column: x => x.ma_don_vi_cha,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "KhoaTuyenSinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_khoa_tuyen_sinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code_khoa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_khoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: false),
                    nam_ket_thuc_du_kien = table.Column<int>(type: "int", nullable: true),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaTuyenSinh", x => x.ma_khoa_tuyen_sinh);
                });

            migrationBuilder.CreateTable(
                name: "MauThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_tb = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loai_su_kien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    kenh_gui = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    mau_tieu_de = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    mau_noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauThongBao", x => x.ma_mau_tb);
                    table.CheckConstraint("CK_MauThongBao_kenh_gui_1", "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms')");
                });

            migrationBuilder.CreateTable(
                name: "NganhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code_nganh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_nganh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhDaoTao", x => x.ma_nganh);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ma_code_vai_tro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_vai_tro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.ma_vai_tro);
                });

            migrationBuilder.CreateTable(
                name: "BaiTap",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_tap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    han_nop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_lan_nop_toi_da = table.Column<int>(type: "int", nullable: false, defaultValueSql: "3"),
                    dinh_dang_cho_phep = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    huong_dan_cham_diem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiTap", x => x.ma_bai_tap);
                    table.CheckConstraint("CK_BaiTap_dinh_dang_cho_phep_ISJSON", "[dinh_dang_cho_phep] IS NULL OR ISJSON([dinh_dang_cho_phep]) = 1");
                    table.CheckConstraint("CK_BaiTap_so_lan_nop_toi_da_1", "[so_lan_nop_toi_da] > 0");
                    table.CheckConstraint("CK_BaiTap_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'da_dong')");
                    table.ForeignKey(
                        name: "FK_BaiTap_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "Chuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    da_an = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chuong", x => x.ma_chuong);
                    table.ForeignKey(
                        name: "FK_Chuong_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "MonHocTienQuyet",
                schema: "dbo",
                columns: table => new
                {
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_mon_tien_quyet = table.Column<int>(type: "int", nullable: false),
                    diem_toi_thieu = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTienQuyet", x => new { x.ma_mon_hoc, x.ma_mon_tien_quyet });
                    table.CheckConstraint("CK_MonHocTienQuyet_diem_toi_thieu_1", "[diem_toi_thieu] BETWEEN 0 AND 10");
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_ma_mon_tien_quyet__DanhMucMonHoc",
                        column: x => x.ma_mon_tien_quyet,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "CauHinhKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_kt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_khen_thuong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    gpa_toi_thieu = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhKhenThuong", x => x.ma_cau_hinh_kt);
                    table.CheckConstraint("CK_CauHinhKhenThuong_gpa_toi_thieu_1", "[gpa_toi_thieu] BETWEEN 0 AND 10");
                    table.ForeignKey(
                        name: "FK_CauHinhKhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "HocKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_hoc_ky = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ten_hoc_ky = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: false),
                    nam_hoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    thu_tu_trong_nam = table.Column<int>(type: "int", nullable: false),
                    ngay_ket_thuc_block5 = table.Column<DateOnly>(type: "date", nullable: true),
                    da_khoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    so_tin_chi_toi_da = table.Column<int>(type: "int", nullable: true),
                    han_rut_mon = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocKy", x => x.ma_hoc_ky);
                    table.CheckConstraint("CK_HocKy_thu_tu_trong_nam_1", "[thu_tu_trong_nam] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_HocKy_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "ToaNha",
                schema: "dbo",
                columns: table => new
                {
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_toa_nha = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_toa_nha = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    dia_chi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    so_tang = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToaNha", x => x.ma_toa_nha);
                    table.CheckConstraint("CK_ToaNha_so_tang_1", "[so_tang] IS NULL OR [so_tang] > 0");
                    table.ForeignKey(
                        name: "FK_ToaNha_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nganh = table.Column<int>(type: "int", nullable: false),
                    ten_chuyen_nganh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganh", x => x.ma_chuyen_nganh);
                    table.ForeignKey(
                        name: "FK_ChuyenNganh_ma_nganh__NganhDaoTao",
                        column: x => x.ma_nganh,
                        principalSchema: "dbo",
                        principalTable: "NganhDaoTao",
                        principalColumn: "ma_nganh");
                });

            migrationBuilder.CreateTable(
                name: "BaiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuong = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    loai_bai_hoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    url_tap_tin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoi_luong_giay = table.Column<int>(type: "int", nullable: true),
                    noi_dung_van_ban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dieu_kien_mo_khoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tom_tat_ai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    da_an = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "nhap"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHoc", x => x.ma_bai_hoc);
                    table.CheckConstraint("CK_BaiHoc_dieu_kien_mo_khoa_ISJSON", "[dieu_kien_mo_khoa] IS NULL OR ISJSON([dieu_kien_mo_khoa]) = 1");
                    table.CheckConstraint("CK_BaiHoc_loai_bai_hoc_1", "[loai_bai_hoc] IN (N'video', N'pdf', N'van_ban', N'trac_nghiem', N'slide_html')");
                    table.CheckConstraint("CK_BaiHoc_thoi_luong_giay_2", "[thoi_luong_giay] >= 0");
                    table.CheckConstraint("CK_BaiHoc_trang_thai", "[trang_thai] IS NULL OR [trang_thai] IN (N'nhap', N'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_BaiHoc_ma_chuong__Chuong",
                        column: x => x.ma_chuong,
                        principalSchema: "dbo",
                        principalTable: "Chuong",
                        principalColumn: "ma_chuong");
                });

            migrationBuilder.CreateTable(
                name: "AnhChupPhanTich",
                schema: "dbo",
                columns: table => new
                {
                    ma_anh_chup = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ngay_anh_chup = table.Column<DateOnly>(type: "date", nullable: false),
                    loai_chi_so = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    gia_tri_chi_so = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    chieu_loc_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhChupPhanTich", x => x.ma_anh_chup);
                    table.CheckConstraint("CK_AnhChupPhanTich_chieu_loc_json_ISJSON", "[chieu_loc_json] IS NULL OR ISJSON([chieu_loc_json]) = 1");
                    table.ForeignKey(
                        name: "FK_AnhChupPhanTich_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_AnhChupPhanTich_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "CauHinhDiemMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    trong_so_qua_trinh = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    trong_so_giua_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    trong_so_cuoi_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    nguong_dat = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValueSql: "5")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhDiemMonHoc", x => x.ma_cau_hinh_diem);
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_nguong_dat_4", "[nguong_dat] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_cuoi_ky_3", "[trong_so_cuoi_ky] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_giua_ky_2", "[trong_so_giua_ky] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_qua_trinh_1", "[trong_so_qua_trinh] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_CauHinhDiemMonHoc_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_CauHinhDiemMonHoc_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "GiaiDoanDangKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_giai_doan_dk = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap"),
                    so_tin_chi_toi_da = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaiDoanDangKy", x => x.ma_giai_doan_dk);
                    table.CheckConstraint("CK_GiaiDoanDangKy_trang_thai_1", "[trang_thai] IN (N'nhap', N'dang_mo', N'da_dong')");
                    table.ForeignKey(
                        name: "FK_GiaiDoanDangKy_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_GiaiDoanDangKy_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

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
                name: "LopHocPhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_code_lop_hoc_phan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    suc_chua = table.Column<int>(type: "int", nullable: false),
                    so_dang_ky_toi_thieu = table.Column<int>(type: "int", nullable: true),
                    so_da_dang_ky = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "mo"),
                    quota_vang_toi_da = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocPhan", x => x.ma_lop_hoc_phan);
                    table.CheckConstraint("CK_LopHocPhan_suc_chua_1", "[suc_chua] > 0");
                    table.CheckConstraint("CK_LopHocPhan_trang_thai_2", "[trang_thai] IN (N'mo', N'dong', N'cho_huy', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_LopHocPhan_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_LopHocPhan_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_LopHocPhan_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "Tang",
                schema: "dbo",
                columns: table => new
                {
                    ma_tang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false),
                    ten_tang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    thu_tu_tang = table.Column<int>(type: "int", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tang", x => x.ma_tang);
                    table.ForeignKey(
                        name: "FK_Tang_ma_toa_nha__ToaNha",
                        column: x => x.ma_toa_nha,
                        principalSchema: "dbo",
                        principalTable: "ToaNha",
                        principalColumn: "ma_toa_nha");
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganhTheoCoSo",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh_co_so = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: true),
                    chi_tieu_du_kien = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganhTheoCoSo", x => x.ma_chuyen_nganh_co_so);
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_chi_tieu_du_kien_1", "[chi_tieu_du_kien] IS NULL OR [chi_tieu_du_kien] >= 0");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_nam_bat_dau_1", "[nam_bat_dau] IS NULL OR [nam_bat_dau] >= 2000");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_trang_thai_1", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'rejected')");
                    table.ForeignKey(
                        name: "FK_ChuyenNganhTheoCoSo_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_ChuyenNganhTheoCoSo_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "PhongHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_toa_nha = table.Column<int>(type: "int", nullable: true),
                    ma_tang = table.Column<int>(type: "int", nullable: true),
                    ma_code_phong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_phong = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    suc_chua = table.Column<int>(type: "int", nullable: false),
                    loai_phong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    trang_thai_phong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "hoat_dong"),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongHoc", x => x.ma_phong);
                    table.CheckConstraint("CK_PhongHoc_loai_phong_2", "[loai_phong] IN (N'ly_thuyet', N'phong_thi_nghiem', N'thuc_hanh', N'lab', N'hoi_truong', N'truc_tuyen', N'khac')");
                    table.CheckConstraint("CK_PhongHoc_suc_chua_1", "[suc_chua] > 0");
                    table.CheckConstraint("CK_PhongHoc_trang_thai_phong_3", "[trang_thai_phong] IN (N'hoat_dong', N'bao_tri', N'ngung_hoat_dong')");
                    table.ForeignKey(
                        name: "FK_PhongHoc_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_PhongHoc_ma_tang__Tang",
                        column: x => x.ma_tang,
                        principalSchema: "dbo",
                        principalTable: "Tang",
                        principalColumn: "ma_tang");
                    table.ForeignKey(
                        name: "FK_PhongHoc_ma_toa_nha__ToaNha",
                        column: x => x.ma_toa_nha,
                        principalSchema: "dbo",
                        principalTable: "ToaNha",
                        principalColumn: "ma_toa_nha");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoSuDungPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_bc_su_dung_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    tu_ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    den_ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    so_gio_su_dung = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    ti_le_su_dung = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    tao_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoSuDungPhong", x => x.ma_bc_su_dung_phong);
                    table.CheckConstraint("CK_BaoCaoSuDungPhong_ti_le_su_dung_1", "[ti_le_su_dung] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_BaoCaoSuDungPhong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_BaoCaoSuDungPhong_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "ThietBiPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_thiet_bi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ten_thiet_bi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBiPhong", x => x.ma_thiet_bi);
                    table.CheckConstraint("CK_ThietBiPhong_so_luong_1", "[so_luong] >= 0");
                    table.ForeignKey(
                        name: "FK_ThietBiPhong_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "BaiHocNoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    loai_noi_dung = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    noi_dung_html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noi_dung_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_tap_tin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    storage_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: true),
                    thoi_luong_giay = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "nhap"),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHocNoiDung", x => x.ma_noi_dung);
                    table.CheckConstraint("CK_BaiHocNoiDung_loai_noi_dung", "[loai_noi_dung] IN (N'video', N'slide_html', N'tai_lieu', N'quiz', N'van_ban')");
                    table.CheckConstraint("CK_BaiHocNoiDung_noi_dung_json_ISJSON", "[noi_dung_json] IS NULL OR ISJSON([noi_dung_json]) = 1");
                    table.CheckConstraint("CK_BaiHocNoiDung_thoi_luong_giay", "[thoi_luong_giay] IS NULL OR [thoi_luong_giay] >= 0");
                    table.CheckConstraint("CK_BaiHocNoiDung_trang_thai", "[trang_thai] IS NULL OR [trang_thai] IN (N'nhap', N'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_BaiHocNoiDung_ma_bai_hoc__BaiHoc",
                        column: x => x.ma_bai_hoc,
                        principalSchema: "dbo",
                        principalTable: "BaiHoc",
                        principalColumn: "ma_bai_hoc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaiNop",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_nop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bai_tap = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    url_tap_tin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_lan_nop = table.Column<int>(type: "int", nullable: false),
                    nop_tre = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    diem_dao_van = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_so = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_ai_de_xuat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    nhan_xet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoi_diem_nop = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    da_cong_bo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiNop", x => x.ma_bai_nop);
                    table.CheckConstraint("CK_BaiNop_diem_ai_de_xuat_4", "[diem_ai_de_xuat] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_BaiNop_diem_dao_van_2", "[diem_dao_van] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_BaiNop_diem_so_3", "[diem_so] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_BaiNop_so_lan_nop_1", "[so_lan_nop] > 0");
                    table.ForeignKey(
                        name: "FK_BaiNop_ma_bai_tap__BaiTap",
                        column: x => x.ma_bai_tap,
                        principalSchema: "dbo",
                        principalTable: "BaiTap",
                        principalColumn: "ma_bai_tap");
                });

            migrationBuilder.CreateTable(
                name: "CanhBaoDaoVan",
                schema: "dbo",
                columns: table => new
                {
                    ma_canh_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bai_nop = table.Column<int>(type: "int", nullable: false),
                    diem_dao_van = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    chi_tiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanhBaoDaoVan", x => x.ma_canh_bao);
                    table.CheckConstraint("CK_CanhBaoDaoVan_chi_tiet_ISJSON", "[chi_tiet] IS NULL OR ISJSON([chi_tiet]) = 1");
                    table.CheckConstraint("CK_CanhBaoDaoVan_diem_dao_van_1", "[diem_dao_van] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_CanhBaoDaoVan_ma_bai_nop__BaiNop",
                        column: x => x.ma_bai_nop,
                        principalSchema: "dbo",
                        principalTable: "BaiNop",
                        principalColumn: "ma_bai_nop");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoRuiRoRotMon",
                schema: "dbo",
                columns: table => new
                {
                    ma_bao_cao_rot = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    xac_suat_rot_mon = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dac_trung_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tao_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoRuiRoRotMon", x => x.ma_bao_cao_rot);
                    table.CheckConstraint("CK_BaoCaoRuiRoRotMon_dac_trung_json_ISJSON", "[dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1");
                    table.CheckConstraint("CK_BaoCaoRuiRoRotMon_xac_suat_rot_mon_1", "[xac_suat_rot_mon] BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_BaoCaoRuiRoRotMon_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_BaoCaoRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoRuiRoVang",
                schema: "dbo",
                columns: table => new
                {
                    ma_bao_cao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    diem_rui_ro = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dac_trung_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tao_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoRuiRoVang", x => x.ma_bao_cao);
                    table.CheckConstraint("CK_BaoCaoRuiRoVang_dac_trung_json_ISJSON", "[dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1");
                    table.CheckConstraint("CK_BaoCaoRuiRoVang_diem_rui_ro_1", "[diem_rui_ro] BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_BaoCaoRuiRoVang_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
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
                });

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                schema: "dbo",
                columns: table => new
                {
                    ma_binh_luan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    giay_trong_video = table.Column<int>(type: "int", nullable: true),
                    so_trang_pdf = table.Column<int>(type: "int", nullable: true),
                    ma_binh_luan_cha = table.Column<int>(type: "int", nullable: true),
                    da_ghim = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuan", x => x.ma_binh_luan);
                    table.CheckConstraint("CK_BinhLuan_giay_trong_video_1", "[giay_trong_video] >= 0");
                    table.CheckConstraint("CK_BinhLuan_so_trang_pdf_2", "[so_trang_pdf] > 0");
                    table.ForeignKey(
                        name: "FK_BinhLuan_ma_bai_hoc__BaiHoc",
                        column: x => x.ma_bai_hoc,
                        principalSchema: "dbo",
                        principalTable: "BaiHoc",
                        principalColumn: "ma_bai_hoc");
                    table.ForeignKey(
                        name: "FK_BinhLuan_ma_binh_luan_cha__BinhLuan",
                        column: x => x.ma_binh_luan_cha,
                        principalSchema: "dbo",
                        principalTable: "BinhLuan",
                        principalColumn: "ma_binh_luan");
                });

            migrationBuilder.CreateTable(
                name: "BuoiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_tkb = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    ngay_hoc = table.Column<DateOnly>(type: "date", nullable: false),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien_day_thay = table.Column<int>(type: "int", nullable: true),
                    trang_thai_buoi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "du_kien"),
                    loai_thay_doi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ly_do_thay_doi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    khoa_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diem_danh_bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diem_danh_han_gui_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diem_danh_da_gui_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diem_danh_han_chinh_sua_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diem_danh_khoa_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    trang_thai_diem_danh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_mo"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuoiHoc", x => x.ma_buoi_hoc);
                    table.CheckConstraint("CK_BuoiHoc_loai_thay_doi", "[loai_thay_doi] IS NULL OR [loai_thay_doi] IN (N'doi_giang_vien', N'doi_phong', N'doi_ca', N'huy_buoi', N'doi_lich')");
                    table.CheckConstraint("CK_BuoiHoc_trang_thai_buoi", "[trang_thai_buoi] IN (N'du_kien', N'da_dien_ra', N'da_huy', N'doi_lich', N'day_thay')");
                    table.CheckConstraint("CK_BuoiHoc_trang_thai_diem_danh", "[trang_thai_diem_danh] IN (N'chua_mo', N'dang_diem_danh', N'da_gui', N'da_khoa')");
                    table.ForeignKey(
                        name: "FK_BuoiHoc_ma_ca_hoc__CaHoc",
                        column: x => x.ma_ca_hoc,
                        principalSchema: "dbo",
                        principalTable: "CaHoc",
                        principalColumn: "ma_ca_hoc");
                    table.ForeignKey(
                        name: "FK_BuoiHoc_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "CanhBaoBaoMat",
                schema: "dbo",
                columns: table => new
                {
                    ma_canh_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    diem_rui_ro = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dia_chi_ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    thong_tin_trinh_duyet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "mo"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanhBaoBaoMat", x => x.ma_canh_bao);
                    table.CheckConstraint("CK_CanhBaoBaoMat_diem_rui_ro_1", "[diem_rui_ro] BETWEEN 0 AND 1");
                    table.CheckConstraint("CK_CanhBaoBaoMat_trang_thai_2", "[trang_thai] IN (N'mo', N'da_xem', N'bo_qua')");
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
                        name: "FK_CaThi_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "CauHinhHocPhiChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_hoc_phi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_chuong_trinh_dao_tao = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    nam_hoc_trong_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_trong_nam = table.Column<int>(type: "int", nullable: false),
                    so_thu_tu_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    loai_cach_tinh_hoc_phi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "co_dinh_theo_hoc_ky"),
                    so_tien_hoc_phi = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    tien_hoc_lieu = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    tong_tien_du_kien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHocPhiChuongTrinh", x => x.ma_cau_hinh_hoc_phi);
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_hoc_ky_trong_nam", "[hoc_ky_trong_nam] IN (1, 2, 3)");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_loai_cach_tinh", "[loai_cach_tinh_hoc_phi] IN (N'co_dinh_theo_hoc_ky', N'theo_tin_chi', N'theo_mon_hoc')");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_nam_hoc", "[nam_hoc_trong_chuong_trinh] >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_thu_tu", "[so_thu_tu_hoc_ky] >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_tien_hoc_phi", "[so_tien_hoc_phi] >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tien_hoc_lieu", "[tien_hoc_lieu] >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tong_tien", "[tong_tien_du_kien] = [so_tien_hoc_phi] + [tien_hoc_lieu]");
                    table.ForeignKey(
                        name: "FK_CauHinhHocPhiChuongTrinh_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_CauHinhHocPhiChuongTrinh_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "CauHoi",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    loai_cau_hoi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lua_chon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dap_an_dung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    do_kho = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.ma_cau_hoi);
                    table.CheckConstraint("CK_CauHoi_dap_an_dung_ISJSON", "[dap_an_dung] IS NULL OR ISJSON([dap_an_dung]) = 1");
                    table.CheckConstraint("CK_CauHoi_do_kho_2", "[do_kho] IN (N'de', N'trung_binh', N'kho')");
                    table.CheckConstraint("CK_CauHoi_loai_cau_hoi_1", "[loai_cau_hoi] IN (N'trac_nghiem', N'tu_luan')");
                    table.CheckConstraint("CK_CauHoi_lua_chon_ISJSON", "[lua_chon] IS NULL OR ISJSON([lua_chon]) = 1");
                    table.ForeignKey(
                        name: "FK_CauHoi_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "CauHoiDeKiemTra",
                schema: "dbo",
                columns: table => new
                {
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: false),
                    ma_cau_hoi = table.Column<int>(type: "int", nullable: false),
                    diem_so = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1m),
                    thu_tu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiDeKiemTra", x => new { x.ma_de_kiem_tra, x.ma_cau_hoi });
                    table.ForeignKey(
                        name: "FK_CauHoiDeKiemTra_ma_cau_hoi__CauHoi",
                        column: x => x.ma_cau_hoi,
                        principalSchema: "dbo",
                        principalTable: "CauHoi",
                        principalColumn: "ma_cau_hoi");
                });

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
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
                    nguoi_gui_duyet_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_gui_duyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_duyet_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_duyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ghi_chu_duyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoi_tu_choi_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_tu_choi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_trang_thai", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'rejected', N'active', N'inactive', N'archived')");
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh",
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

            migrationBuilder.CreateTable(
                name: "ChuongTrinhHocKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh_hoc_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    thu_tu_hoc_ky = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhHocKy", x => x.ma_chuong_trinh_hoc_ky);
                    table.CheckConstraint("CK_ChuongTrinhHocKy_thu_tu_hoc_ky_1", "[thu_tu_hoc_ky] > 0");
                    table.ForeignKey(
                        name: "FK_ChuongTrinhHocKy_ma_chuong_trinh__ChuongTrinhDaoTao",
                        column: x => x.ma_chuong_trinh,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh");
                    table.ForeignKey(
                        name: "FK_ChuongTrinhHocKy_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "MonHocTrongChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh_mon_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: false),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    loai_mon_hoc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    bat_buoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTrongChuongTrinh", x => x.ma_chuong_trinh_mon_hoc);
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_hoc_ky_du_kien", "[hoc_ky_du_kien] > 0");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_loai_mon_hoc", "[loai_mon_hoc] IN (N'bat_buoc', N'tu_chon', N'thay_the')");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_so_tin_chi", "[so_tin_chi] > 0");
                    table.ForeignKey(
                        name: "FK_MonHocTrongChuongTrinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                        column: x => x.ma_chuong_trinh,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHocTrongChuongTrinh_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeCuongMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_syllabus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ma_chuong_trinh_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ten_syllabus = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: true),
                    bat_buoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeCuongMonHoc", x => x.ma_syllabus);
                    table.CheckConstraint("CK_DeCuongMonHoc_hoc_ky_du_kien_1", "[hoc_ky_du_kien] IS NULL OR [hoc_ky_du_kien] > 0");
                    table.CheckConstraint("CK_DeCuongMonHoc_trang_thai_1", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");
                    table.ForeignKey(
                        name: "FK_DeCuongMonHoc_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                        column: x => x.ma_chuong_trinh_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "MonHocTrongChuongTrinh",
                        principalColumn: "ma_chuong_trinh_mon_hoc");
                    table.ForeignKey(
                        name: "FK_DeCuongMonHoc_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_DeCuongMonHoc_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_DeCuongMonHoc_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "DangKyHocPhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_dang_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    vi_tri_cho = table.Column<int>(type: "int", nullable: true),
                    la_hoc_lai = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    kiem_tra_tien_quyet = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    da_kiem_tra_tien_quyet = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyHocPhan", x => x.ma_dang_ky);
                    table.CheckConstraint("CK_DangKyHocPhan_trang_thai_1", "[trang_thai] IN (N'da_dang_ky', N'danh_sach_cho', N'da_rut', N'lop_bi_huy')");
                    table.CheckConstraint("CK_DangKyHocPhan_vi_tri_cho_2", "[vi_tri_cho] > 0");
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_ma_lop_hoc_phan__LopHocPhan",
                        column: x => x.ma_lop_hoc_phan,
                        principalSchema: "dbo",
                        principalTable: "LopHocPhan",
                        principalColumn: "ma_lop_hoc_phan");
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaGiaoVien",
                schema: "dbo",
                columns: table => new
                {
                    ma_danh_gia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_cau_hoi_dg = table.Column<int>(type: "int", nullable: false),
                    diem_so = table.Column<int>(type: "int", nullable: false),
                    nhan_xet_tu_do = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ai_cam_xuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ai_chu_de = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    cohort_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaGiaoVien", x => x.ma_danh_gia);
                    table.CheckConstraint("CK_DanhGiaGiaoVien_ai_cam_xuc_2", "[ai_cam_xuc] IN (N'tich_cuc', N'trung_tinh', N'tieu_cuc')");
                    table.CheckConstraint("CK_DanhGiaGiaoVien_ai_chu_de_ISJSON", "[ai_chu_de] IS NULL OR ISJSON([ai_chu_de]) = 1");
                    table.CheckConstraint("CK_DanhGiaGiaoVien_diem_so_1", "[diem_so] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_DanhGiaGiaoVien_ma_cau_hoi_dg__CauHoiDanhGia",
                        column: x => x.ma_cau_hoi_dg,
                        principalSchema: "dbo",
                        principalTable: "CauHoiDanhGia",
                        principalColumn: "ma_cau_hoi_dg");
                    table.ForeignKey(
                        name: "FK_DanhGiaGiaoVien_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "DanhSachRuiRoRotMon",
                schema: "dbo",
                columns: table => new
                {
                    ma_rui_ro_rot = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    xac_suat_rot_mon = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    tao_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachRuiRoRotMon", x => x.ma_rui_ro_rot);
                    table.CheckConstraint("CK_DanhSachRuiRoRotMon_xac_suat_rot_mon_1", "[xac_suat_rot_mon] BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_DanhSachRuiRoRotMon_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_DanhSachRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "DatPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_dat_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    muc_dich = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_nguoi_tham_du = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_duyet"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatPhong", x => x.ma_dat_phong);
                    table.CheckConstraint("CK_DatPhong_ket_thuc_luc_1", "[ket_thuc_luc] > [bat_dau_luc]");
                    table.CheckConstraint("CK_DatPhong_so_nguoi_tham_du_2", "[so_nguoi_tham_du] >= 0");
                    table.CheckConstraint("CK_DatPhong_trang_thai_3", "[trang_thai] IN (N'cho_duyet', N'da_xac_nhan', N'tu_choi', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_DatPhong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_DatPhong_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "DeKiemTra",
                schema: "dbo",
                columns: table => new
                {
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    thoi_gian_phut = table.Column<int>(type: "int", nullable: false),
                    cau_hinh_de_thi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    loai_de_thi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    hinh_thuc_thi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ty_le_trac_nghiem = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ty_le_tu_luan = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ma_nguoi_soan = table.Column<int>(type: "int", nullable: true),
                    ma_nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    trang_thai_duyet = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeKiemTra", x => x.ma_de_kiem_tra);
                    table.CheckConstraint("CK_DeKiemTra_cau_hinh_de_thi_ISJSON", "[cau_hinh_de_thi] IS NULL OR ISJSON([cau_hinh_de_thi]) = 1");
                    table.CheckConstraint("CK_DeKiemTra_hinh_thuc_thi", "[hinh_thuc_thi] IS NULL OR [hinh_thuc_thi] IN (N'online_tap_trung', N'online_tu_do', N'van_dap')");
                    table.CheckConstraint("CK_DeKiemTra_loai_de_thi", "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop')");
                    table.CheckConstraint("CK_DeKiemTra_thoi_gian_phut", "[thoi_gian_phut] > 0");
                    table.CheckConstraint("CK_DeKiemTra_thoi_gian_phut_1", "[thoi_gian_phut] BETWEEN 1 AND 240");
                    table.CheckConstraint("CK_DeKiemTra_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_len_lich', N'dang_mo', N'da_dong', N'da_cong_bo')");
                    table.CheckConstraint("CK_DeKiemTra_trang_thai_duyet", "[trang_thai_duyet] IS NULL OR [trang_thai_duyet] IN (N'nhap', N'cho_duyet', N'da_duyet', N'tu_choi')");
                    table.CheckConstraint("CK_DeKiemTra_ty_le_trac_nghiem", "[ty_le_trac_nghiem] IS NULL OR [ty_le_trac_nghiem] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_DeKiemTra_ty_le_tu_luan", "[ty_le_tu_luan] IS NULL OR [ty_le_tu_luan] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_DeKiemTra_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_DeKiemTra_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
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
                name: "DiemDanh",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_danh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    nguoi_ghi_nhan = table.Column<int>(type: "int", nullable: false),
                    ghi_nhan_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    khoa_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    he_so_vang = table.Column<int>(type: "int", nullable: false),
                    ma_yc_mo_khoa = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.ma_diem_danh);
                    table.CheckConstraint("CK_DiemDanh_he_so_vang_2", "[he_so_vang] >= 0");
                    table.CheckConstraint("CK_DiemDanh_trang_thai_1", "[trang_thai] IN (N'co_mat', N'vang', N'di_muon', N'co_phep')");
                    table.ForeignKey(
                        name: "FK_DiemDanh_ma_buoi_hoc__BuoiHoc",
                        column: x => x.ma_buoi_hoc,
                        principalSchema: "dbo",
                        principalTable: "BuoiHoc",
                        principalColumn: "ma_buoi_hoc");
                    table.ForeignKey(
                        name: "FK_DiemDanh_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
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
                });

            migrationBuilder.CreateTable(
                name: "DiemSo",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_so = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    diem_qua_trinh = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_giua_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_cuoi_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    gpa_mon_hoc = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "chua_hoan_thanh"),
                    da_khoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ly_do_rot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemSo", x => x.ma_diem_so);
                    table.CheckConstraint("CK_DiemSo_diem_cuoi_ky_3", "[diem_cuoi_ky] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_diem_giua_ky_2", "[diem_giua_ky] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_diem_qua_trinh_1", "[diem_qua_trinh] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_gpa_mon_hoc_4", "[gpa_mon_hoc] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_ly_do_rot_ISJSON", "[ly_do_rot] IS NULL OR ISJSON([ly_do_rot]) = 1");
                    table.CheckConstraint("CK_DiemSo_trang_thai_5", "[trang_thai] IN (N'dat', N'rot', N'chua_hoan_thanh', N'cho_hoan_thanh_bo_sung')");
                    table.ForeignKey(
                        name: "FK_DiemSo_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_DiemSo_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_DiemSo_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "DonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_don_tu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    loai_don = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    nguoi_duyet_hien_tai = table.Column<int>(type: "int", nullable: true),
                    du_lieu_bieu_mau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_bang_chung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhat_ky_tu_dong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonTu", x => x.ma_don_tu);
                    table.CheckConstraint("CK_DonTu_du_lieu_bieu_mau_ISJSON", "[du_lieu_bieu_mau] IS NULL OR ISJSON([du_lieu_bieu_mau]) = 1");
                    table.CheckConstraint("CK_DonTu_loai_don_1", "[loai_don] IN ( N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc' )");
                    table.CheckConstraint("CK_DonTu_nhat_ky_tu_dong_ISJSON", "[nhat_ky_tu_dong] IS NULL OR ISJSON([nhat_ky_tu_dong]) = 1");
                    table.CheckConstraint("CK_DonTu_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'da_duyet', N'tu_choi')");
                });

            migrationBuilder.CreateTable(
                name: "GiaoDich",
                schema: "dbo",
                columns: table => new
                {
                    ma_giao_dich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false),
                    ma_tai_khoan_nhan_tien = table.Column<int>(type: "int", nullable: true),
                    ma_tham_chieu_noi_bo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ma_tham_chieu_cong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    so_tien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    loai_giao_dich = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nha_cung_cap_thanh_toan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    noi_dung_chuyen_khoan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    qr_payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qr_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    checkout_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    request_payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    response_payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    callback_payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_het_han = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_thanh_toan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ma_nguoi_thuc_hien = table.Column<int>(type: "int", nullable: true),
                    chu_thich = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDich", x => x.ma_giao_dich);
                    table.CheckConstraint("CK_GiaoDich_callback_payload_json", "[callback_payload_json] IS NULL OR ISJSON([callback_payload_json]) = 1");
                    table.CheckConstraint("CK_GiaoDich_loai_giao_dich", "[loai_giao_dich] IN (N'phat_sinh_hoc_phi', N'thanh_toan_hoc_phi', N'dieu_chinh_cong_no', N'hoan_tien', N'huy_hoa_don')");
                    table.CheckConstraint("CK_GiaoDich_provider", "[nha_cung_cap_thanh_toan] IS NULL OR [nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr')");
                    table.CheckConstraint("CK_GiaoDich_request_payload_json", "[request_payload_json] IS NULL OR ISJSON([request_payload_json]) = 1");
                    table.CheckConstraint("CK_GiaoDich_response_payload_json", "[response_payload_json] IS NULL OR ISJSON([response_payload_json]) = 1");
                    table.CheckConstraint("CK_GiaoDich_trang_thai", "[trang_thai] IN (N'phat_sinh', N'cho_thanh_toan', N'dang_xu_ly', N'thanh_cong', N'that_bai', N'het_han', N'da_huy', N'sai_so_tien', N'cho_xu_ly_thu_cong')");
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ma_hoa_don_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    loai_hoa_don = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "hoc_phi"),
                    so_tien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    giam_tru = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    da_thanh_toan = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_thanh_toan"),
                    han_thanh_toan = table.Column<DateOnly>(type: "date", nullable: false),
                    url_hoa_don_pdf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_huy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_huy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    nguoi_huy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.ma_hoa_don);
                    table.CheckConstraint("CK_HoaDon_da_thanh_toan", "[da_thanh_toan] >= 0");
                    table.CheckConstraint("CK_HoaDon_giam_tru", "[giam_tru] >= 0");
                    table.CheckConstraint("CK_HoaDon_loai_hoa_don", "[loai_hoa_don] IN (N'hoc_phi', N'le_phi', N'tai_lieu', N'khac')");
                    table.CheckConstraint("CK_HoaDon_so_tien", "[so_tien] >= 0");
                    table.CheckConstraint("CK_HoaDon_trang_thai", "[trang_thai] IN (N'chua_thanh_toan', N'thanh_toan_mot_phan', N'da_thanh_toan', N'qua_han', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_HoaDon_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_HoaDon_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "HoSoKyLuat",
                schema: "dbo",
                columns: table => new
                {
                    ma_ky_luat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_ky_luat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoKyLuat", x => x.ma_ky_luat);
                    table.ForeignKey(
                        name: "FK_HoSoKyLuat_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "KhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    loai_khen_thuong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    gpa_dat_duoc = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    url_chung_tu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cap_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    da_huy = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ghi_chu_huy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhenThuong", x => x.ma_khen_thuong);
                    table.CheckConstraint("CK_KhenThuong_gpa_dat_duoc_2", "[gpa_dat_duoc] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_KhenThuong_loai_khen_thuong_1", "[loai_khen_thuong] IN (N'hoc_luc', N'dac_biet', N'thi_dau')");
                    table.ForeignKey(
                        name: "FK_KhenThuong_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ma_lop = table.Column<int>(type: "int", nullable: false),
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap"),
                    url_anh_bia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHoc", x => x.ma_khoa_hoc);
                    table.CheckConstraint("CK_KhoaHoc_trang_thai_1", "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'luu_tru')");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_ma_lop_hoc_phan__LopHocPhan",
                        column: x => x.ma_lop_hoc_phan,
                        principalSchema: "dbo",
                        principalTable: "LopHocPhan",
                        principalColumn: "ma_lop_hoc_phan");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateTable(
                name: "ThoiKhoaBieu",
                schema: "dbo",
                columns: table => new
                {
                    ma_tkb = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false),
                    thu_trong_tuan = table.Column<int>(type: "int", nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThoiKhoaBieu", x => x.ma_tkb);
                    table.CheckConstraint("CK_ThoiKhoaBieu_ngay", "[ngay_ket_thuc] IS NULL OR [ngay_bat_dau] IS NULL OR [ngay_ket_thuc] >= [ngay_bat_dau]");
                    table.CheckConstraint("CK_ThoiKhoaBieu_thu_trong_tuan", "[thu_trong_tuan] BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_ThoiKhoaBieu_trang_thai", "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_ThoiKhoaBieu_ma_ca_hoc__CaHoc",
                        column: x => x.ma_ca_hoc,
                        principalSchema: "dbo",
                        principalTable: "CaHoc",
                        principalColumn: "ma_ca_hoc");
                    table.ForeignKey(
                        name: "FK_ThoiKhoaBieu_ma_khoa_hoc__KhoaHoc",
                        column: x => x.ma_khoa_hoc,
                        principalSchema: "dbo",
                        principalTable: "KhoaHoc",
                        principalColumn: "ma_khoa_hoc");
                    table.ForeignKey(
                        name: "FK_ThoiKhoaBieu_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateTable(
                name: "LienKetPhuHuynh",
                schema: "dbo",
                columns: table => new
                {
                    ma_lien_ket_ph = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phu_huynh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    quyen_xem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet"),
                    lien_ket_luc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LienKetPhuHuynh", x => x.ma_lien_ket_ph);
                    table.CheckConstraint("CK_LienKetPhuHuynh_quyen_xem_ISJSON", "[quyen_xem] IS NULL OR ISJSON([quyen_xem]) = 1");
                    table.CheckConstraint("CK_LienKetPhuHuynh_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'hoat_dong', N'da_thu_hoi')");
                });

            migrationBuilder.CreateTable(
                name: "LopHanhChinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_lop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_lop = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_lop = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ma_giao_vien_chu_nhiem = table.Column<int>(type: "int", nullable: true),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: true),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHanhChinh", x => x.ma_lop);
                    table.ForeignKey(
                        name: "FK_LopHanhChinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                        column: x => x.ma_chuong_trinh,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh");
                    table.ForeignKey(
                        name: "FK_LopHanhChinh_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ho_ten = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    vai_tro_chinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ma_lop = table.Column<int>(type: "int", nullable: true),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "dang_nhap_lan_dau"),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: true),
                    mat_khau_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    lan_dang_nhap_cuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    so_lan_sai_mat_khau = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    dang_nhap_lan_dau = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.ma_nguoi_dung);
                    table.CheckConstraint("CK_NguoiDung_trang_thai_2", "[trang_thai] IN (N'hoat_dong', N'bi_khoa', N'dang_nhap_lan_dau')");
                    table.CheckConstraint("CK_NguoiDung_vai_tro_chinh_1", "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh', N'sieu_quan_tri', N'quan_tri_co_so', N'quan_tri_co_so_con', N'chu_tich', N'hoidong_quanly_noidung', N'admin_tai_chinh', N'ke_toan_co_so', N'ke_toan_truong_co_so')");
                    table.ForeignKey(
                        name: "FK_NguoiDung_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_NguoiDung_ma_lop__LopHanhChinh",
                        column: x => x.ma_lop,
                        principalSchema: "dbo",
                        principalTable: "LopHanhChinh",
                        principalColumn: "ma_lop");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyDuyetDon",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_duyet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_tu = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_duyet = table.Column<int>(type: "int", nullable: false),
                    hanh_dong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyDuyetDon", x => x.ma_nk_duyet);
                    table.CheckConstraint("CK_NhatKyDuyetDon_hanh_dong_1", "[hanh_dong] IN (N'nop', N'phe_duyet', N'tu_choi', N'phan_cong', N'leo_thang')");
                    table.ForeignKey(
                        name: "FK_NhatKyDuyetDon_ma_don_tu__DonTu",
                        column: x => x.ma_don_tu,
                        principalSchema: "dbo",
                        principalTable: "DonTu",
                        principalColumn: "ma_don_tu");
                    table.ForeignKey(
                        name: "FK_NhatKyDuyetDon_ma_nguoi_duyet__NguoiDung",
                        column: x => x.ma_nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyKiemToan",
                schema: "dbo",
                columns: table => new
                {
                    ma_kiem_toan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    loai_doi_tuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_doi_tuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    hanh_dong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    gia_tri_cu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gia_tri_moi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoi_thay_doi = table.Column<int>(type: "int", nullable: true),
                    thoi_diem_thay_doi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    dia_chi_ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    mo_ta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    trace_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyKiemToan", x => x.ma_kiem_toan);
                    table.CheckConstraint("CK_NhatKyKiemToan_gia_tri_cu_ISJSON", "[gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1");
                    table.CheckConstraint("CK_NhatKyKiemToan_gia_tri_moi_ISJSON", "[gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1");
                    table.ForeignKey(
                        name: "FK_NhatKyKiemToan_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_NhatKyKiemToan_nguoi_thay_doi__NguoiDung",
                        column: x => x.nguoi_thay_doi,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyThayDoiDiem",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_thay_doi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_diem_so = table.Column<int>(type: "int", nullable: false),
                    nguoi_thay_doi = table.Column<int>(type: "int", nullable: false),
                    gia_tri_cu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gia_tri_moi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    thay_doi_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyThayDoiDiem", x => x.ma_nk_thay_doi);
                    table.CheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_cu_ISJSON", "[gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1");
                    table.CheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_moi_ISJSON", "[gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1");
                    table.ForeignKey(
                        name: "FK_NhatKyThayDoiDiem_ma_diem_so__DiemSo",
                        column: x => x.ma_diem_so,
                        principalSchema: "dbo",
                        principalTable: "DiemSo",
                        principalColumn: "ma_diem_so");
                    table.ForeignKey(
                        name: "FK_NhatKyThayDoiDiem_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_NhatKyThayDoiDiem_nguoi_thay_doi__NguoiDung",
                        column: x => x.nguoi_thay_doi,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "NopBaiDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_nop_dg = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    so_lan_nop = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    so_lan_sua = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NopBaiDanhGia", x => x.ma_nop_dg);
                    table.CheckConstraint("CK_NopBaiDanhGia_so_lan_nop_1", "[so_lan_nop] BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_NopBaiDanhGia_ma_giao_vien__NguoiDung",
                        column: x => x.ma_giao_vien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_NopBaiDanhGia_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_NopBaiDanhGia_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
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
                name: "PhanQuyenNguoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ngay_gan = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyenNguoiDung", x => new { x.ma_nguoi_dung, x.ma_vai_tro });
                    table.ForeignKey(
                        name: "FK_PhanQuyenNguoiDung_ma_nguoi_dung__NguoiDung",
                        column: x => x.ma_nguoi_dung,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_PhanQuyenNguoiDung_ma_vai_tro__VaiTro",
                        column: x => x.ma_vai_tro,
                        principalSchema: "dbo",
                        principalTable: "VaiTro",
                        principalColumn: "ma_vai_tro");
                });

            migrationBuilder.CreateTable(
                name: "PhienThiHocSinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_phien_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nop_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cau_tra_loi_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhat_ky_vi_pham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sao_luu_cuc_bo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai_luong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "dang_hoat_dong"),
                    diem_tu_dong = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_cuoi_cung = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_tu_luan_ai_goi_y = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: true),
                    trang_thai_ky_ten = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    thoi_diem_ky = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nguoi_xac_nhan_ky_ten = table.Column<int>(type: "int", nullable: true),
                    trang_thai_cong_bo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienThiHocSinh", x => x.ma_phien_thi);
                    table.CheckConstraint("CK_PhienThiHocSinh_cau_tra_loi_json_ISJSON", "[cau_tra_loi_json] IS NULL OR ISJSON([cau_tra_loi_json]) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_cuoi_cung", "[diem_cuoi_cung] IS NULL OR [diem_cuoi_cung] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_tu_dong", "[diem_tu_dong] IS NULL OR [diem_tu_dong] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y", "[diem_tu_luan_ai_goi_y] IS NULL OR [diem_tu_luan_ai_goi_y] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_nhat_ky_vi_pham_ISJSON", "[nhat_ky_vi_pham] IS NULL OR ISJSON([nhat_ky_vi_pham]) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_sao_luu_cuc_bo_ISJSON", "[sao_luu_cuc_bo] IS NULL OR ISJSON([sao_luu_cuc_bo]) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_cong_bo", "[trang_thai_cong_bo] IS NULL OR [trang_thai_cong_bo] IN (N'chua_co_diem', N'da_cham_xong', N'da_doc_diem', N'da_cong_bo')");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_ky_ten", "[trang_thai_ky_ten] IS NULL OR [trang_thai_ky_ten] IN (N'chua_ky', N'da_ky', N'quen_ky', N'su_co')");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_luong_1", "[trang_thai_luong] IN (N'dang_hoat_dong', N'bi_gian_doan', N'da_dung')");
                    table.ForeignKey(
                        name: "FK_PhienThiHocSinh_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                    table.ForeignKey(
                        name: "FK_PhienThiHocSinh_ma_de_kiem_tra__DeKiemTra",
                        column: x => x.ma_de_kiem_tra,
                        principalSchema: "dbo",
                        principalTable: "DeKiemTra",
                        principalColumn: "ma_de_kiem_tra");
                    table.ForeignKey(
                        name: "FK_PhienThiHocSinh_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_PhienThiHocSinh_nguoi_xac_nhan_ky_ten__NguoiDung",
                        column: x => x.nguoi_xac_nhan_ky_ten,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "PhieuHoTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_phieu_ht = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    danh_muc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "mo"),
                    phan_cong_cho = table.Column<int>(type: "int", nullable: true),
                    han_xu_ly = table.Column<DateTime>(type: "datetime2", nullable: true),
                    danh_gia_hai_long = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    do_uu_tien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "medium")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuHoTro", x => x.ma_phieu_ht);
                    table.CheckConstraint("CK_PhieuHoTro_danh_gia_hai_long_3", "[danh_gia_hai_long] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_PhieuHoTro_danh_muc_1", "[danh_muc] IN (N'ky_thuat', N'hoc_vu', N'tai_chinh', N'khac')");
                    table.CheckConstraint("CK_PhieuHoTro_trang_thai_2", "[trang_thai] IN (N'mo', N'dang_xu_ly', N'da_giai_quyet', N'da_dong')");
                    table.ForeignKey(
                        name: "FK_PhieuHoTro_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_PhieuHoTro_phan_cong_cho__NguoiDung",
                        column: x => x.phan_cong_cho,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoanNhanTien",
                schema: "dbo",
                columns: table => new
                {
                    ma_tai_khoan_nhan_tien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ten_ngan_hang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_ngan_hang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    so_tai_khoan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_chu_tai_khoan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    chi_nhanh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    nha_cung_cap_thanh_toan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "payos"),
                    trang_thai_duyet = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    cau_hinh_provider_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    la_mac_dinh = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNhanTien", x => x.ma_tai_khoan_nhan_tien);
                    table.CheckConstraint("CK_TaiKhoanNhanTien_cau_hinh_provider_json", "[cau_hinh_provider_json] IS NULL OR ISJSON([cau_hinh_provider_json]) = 1");
                    table.CheckConstraint("CK_TaiKhoanNhanTien_provider", "[nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr')");
                    table.CheckConstraint("CK_TaiKhoanNhanTien_trang_thai_duyet", "[trang_thai_duyet] IN (N'nhap', N'cho_duyet', N'da_duyet', N'tu_choi', N'ngung_hoat_dong')");
                    table.ForeignKey(
                        name: "FK_TaiKhoanNhanTien_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_TaiKhoanNhanTien_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_TaiKhoanNhanTien_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
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
                name: "ThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_thong_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nhom_thong_bao = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_su_kien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    tom_tat = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    noi_dung_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noi_dung_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    muc_do = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "info"),
                    doi_tuong_lien_ket = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ma_doi_tuong_lien_ket = table.Column<int>(type: "int", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "da_gui"),
                    da_doc = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    doc_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.ma_thong_bao);
                    table.CheckConstraint("CK_ThongBao_muc_do", "[muc_do] IN (N'info', N'warning', N'important')");
                    table.CheckConstraint("CK_ThongBao_noi_dung_json_ISJSON", "[noi_dung_json] IS NULL OR ISJSON([noi_dung_json]) = 1");
                    table.CheckConstraint("CK_ThongBao_trang_thai", "[trang_thai] IN (N'nhap', N'da_gui', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_ThongBao_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_ThongBao_ma_nguoi_nhan__NguoiDung",
                        column: x => x.ma_nguoi_nhan,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_ThongBao_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "ThongBaoHenGio",
                schema: "dbo",
                columns: table => new
                {
                    ma_tb_hen_gio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    loai_su_kien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    bo_loc_nguoi_nhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gui_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "da_len_lich")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoHenGio", x => x.ma_tb_hen_gio);
                    table.CheckConstraint("CK_ThongBaoHenGio_bo_loc_nguoi_nhan_ISJSON", "[bo_loc_nguoi_nhan] IS NULL OR ISJSON([bo_loc_nguoi_nhan]) = 1");
                    table.CheckConstraint("CK_ThongBaoHenGio_trang_thai_1", "[trang_thai] IN (N'da_len_lich', N'dang_cho', N'da_huy', N'hoan_thanh')");
                    table.ForeignKey(
                        name: "FK_ThongBaoHenGio_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_ThongBaoHenGio_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "TienDoBaiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_tien_do = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    phan_tram_tien_do = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    lan_gui_nhip_tim_cuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hoan_thanh_luc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienDoBaiHoc", x => x.ma_tien_do);
                    table.CheckConstraint("CK_TienDoBaiHoc_phan_tram_tien_do_1", "[phan_tram_tien_do] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_TienDoBaiHoc_ma_bai_hoc__BaiHoc",
                        column: x => x.ma_bai_hoc,
                        principalSchema: "dbo",
                        principalTable: "BaiHoc",
                        principalColumn: "ma_bai_hoc");
                    table.ForeignKey(
                        name: "FK_TienDoBaiHoc_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "TokenLamMoi",
                schema: "dbo",
                columns: table => new
                {
                    ma_token_lam_moi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    het_han_luc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thu_hoi_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenLamMoi", x => x.ma_token_lam_moi);
                    table.ForeignKey(
                        name: "FK_TokenLamMoi_ma_nguoi_dung__NguoiDung",
                        column: x => x.ma_nguoi_dung,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "TuyChonThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    nhan_email = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    nhan_push = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    nhan_sms = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuyChonThongBao", x => x.ma_nguoi_dung);
                    table.ForeignKey(
                        name: "FK_TuyChonThongBao_ma_nguoi_dung__NguoiDung",
                        column: x => x.ma_nguoi_dung,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "XuatBaoCao",
                schema: "dbo",
                columns: table => new
                {
                    ma_xuat_bao_cao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_bao_cao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tham_so_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_tap_tin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_xu_ly"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatBaoCao", x => x.ma_xuat_bao_cao);
                    table.CheckConstraint("CK_XuatBaoCao_tham_so_json_ISJSON", "[tham_so_json] IS NULL OR ISJSON([tham_so_json]) = 1");
                    table.CheckConstraint("CK_XuatBaoCao_trang_thai_1", "[trang_thai] IN (N'cho_xu_ly', N'dang_xu_ly', N'hoan_thanh', N'that_bai')");
                    table.ForeignKey(
                        name: "FK_XuatBaoCao_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_XuatBaoCao_nguoi_yeu_cau__NguoiDung",
                        column: x => x.nguoi_yeu_cau,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauDoiLich",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_doi_lich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_tkb = table.Column<int>(type: "int", nullable: false),
                    giao_vien_de_xuat = table.Column<int>(type: "int", nullable: false),
                    giao_vien_nhan_doi = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_gv_nhan_dong_y"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    gv_nhan_phan_hoi_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    admin_duyet_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauDoiLich", x => x.ma_yc_doi_lich);
                    table.CheckConstraint("CK_YeuCauDoiLich_trang_thai_1", "[trang_thai] IN (N'cho_gv_nhan_dong_y', N'cho_admin_duyet', N'da_hoan_doi', N'tu_choi', N'da_huy')");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiLich_gv_de_xuat__NguoiDung",
                        column: x => x.giao_vien_de_xuat,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiLich_gv_nhan_doi__NguoiDung",
                        column: x => x.giao_vien_nhan_doi,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiLich_ma_tkb__ThoiKhoaBieu",
                        column: x => x.ma_tkb,
                        principalSchema: "dbo",
                        principalTable: "ThoiKhoaBieu",
                        principalColumn: "ma_tkb");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiLich_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauHoanPhi",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoan_phi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    so_tien_yeu_cau = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    loai_hoan_phi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet"),
                    ly_do_yeu_cau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    xu_ly_luc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoanPhi", x => x.ma_hoan_phi);
                    table.CheckConstraint("CK_YeuCauHoanPhi_loai_hoan_phi_2", "[loai_hoan_phi] IN (N'toan_phan', N'mot_phan', N'ghi_co')");
                    table.CheckConstraint("CK_YeuCauHoanPhi_so_tien_yeu_cau_1", "[so_tien_yeu_cau] >= 0");
                    table.CheckConstraint("CK_YeuCauHoanPhi_trang_thai_3", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'da_xu_ly')");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_ma_hoa_don__HoaDon",
                        column: x => x.ma_hoa_don,
                        principalSchema: "dbo",
                        principalTable: "HoaDon",
                        principalColumn: "ma_hoa_don");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_nguoi_cap_nhat__NguoiDung",
                        column: x => x.nguoi_cap_nhat,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauHoanPhi_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauMoKhoaDiemDanh",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_mo_khoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    mo_khoa_den_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoi_gian_xu_ly = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauMoKhoaDiemDanh", x => x.ma_yc_mo_khoa);
                    table.CheckConstraint("CK_YeuCauMoKhoaDiemDanh_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han')");
                    table.ForeignKey(
                        name: "FK_YeuCauMoKhoaDiemDanh_ma_buoi_hoc__BuoiHoc",
                        column: x => x.ma_buoi_hoc,
                        principalSchema: "dbo",
                        principalTable: "BuoiHoc",
                        principalColumn: "ma_buoi_hoc");
                    table.ForeignKey(
                        name: "FK_YeuCauMoKhoaDiemDanh_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauMoKhoaDiemDanh_nguoi_yeu_cau__NguoiDung",
                        column: x => x.nguoi_yeu_cau,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauSuaDiem",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_sua_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_diem_so = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_bang_chung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    mo_den_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    loai_yeu_cau = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "sua_sau_submit"),
                    unlock_expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cot_diem_duoc_mo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauSuaDiem", x => x.ma_yc_sua_diem);
                    table.CheckConstraint("CK_YeuCauSuaDiem_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han')");
                    table.ForeignKey(
                        name: "FK_YeuCauSuaDiem_ma_diem_so__DiemSo",
                        column: x => x.ma_diem_so,
                        principalSchema: "dbo",
                        principalTable: "DiemSo",
                        principalColumn: "ma_diem_so");
                    table.ForeignKey(
                        name: "FK_YeuCauSuaDiem_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_YeuCauSuaDiem_nguoi_yeu_cau__NguoiDung",
                        column: x => x.nguoi_yeu_cau,
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
                name: "TinNhanHoTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_tin_nhan_ht = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_phieu_ht = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_gui = table.Column<int>(type: "int", nullable: false),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_dinh_kem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhanHoTro", x => x.ma_tin_nhan_ht);
                    table.ForeignKey(
                        name: "FK_TinNhanHoTro_ma_nguoi_gui__NguoiDung",
                        column: x => x.ma_nguoi_gui,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_TinNhanHoTro_ma_phieu_ht__PhieuHoTro",
                        column: x => x.ma_phieu_ht,
                        principalSchema: "dbo",
                        principalTable: "PhieuHoTro",
                        principalColumn: "ma_phieu_ht");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_thong_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_thong_bao = table.Column<int>(type: "int", nullable: true),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    kenh_gui = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gui_luc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyThongBao", x => x.ma_nk_thong_bao);
                    table.CheckConstraint("CK_NhatKyThongBao_kenh_gui_2", "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms')");
                    table.CheckConstraint("CK_NhatKyThongBao_trang_thai_1", "[trang_thai] IN (N'cho_gui', N'da_gui', N'da_nhan', N'that_bai', N'bo_qua')");
                    table.ForeignKey(
                        name: "FK_NhatKyThongBao_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_NhatKyThongBao_ma_nguoi_nhan__NguoiDung",
                        column: x => x.ma_nguoi_nhan,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_NhatKyThongBao_ma_thong_bao__ThongBao",
                        column: x => x.ma_thong_bao,
                        principalSchema: "dbo",
                        principalTable: "ThongBao",
                        principalColumn: "ma_thong_bao");
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
                name: "IX_AnhChupPhanTich_ma_hoc_ky",
                schema: "dbo",
                table: "AnhChupPhanTich",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_AnhChupPhanTich_1",
                schema: "dbo",
                table: "AnhChupPhanTich",
                columns: new[] { "ma_don_vi", "ma_hoc_ky", "ngay_anh_chup", "loai_chi_so" },
                unique: true,
                filter: "[ma_hoc_ky] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BaiHoc_ma_chuong",
                schema: "dbo",
                table: "BaiHoc",
                column: "ma_chuong");

            migrationBuilder.CreateIndex(
                name: "IX_BaiHocNoiDung_ma_bai_hoc",
                schema: "dbo",
                table: "BaiHocNoiDung",
                column: "ma_bai_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BaiHocNoiDung_ma_de_kiem_tra",
                schema: "dbo",
                table: "BaiHocNoiDung",
                column: "ma_de_kiem_tra");

            migrationBuilder.CreateIndex(
                name: "IX_BaiNop_BaiTap_HocSinh",
                schema: "dbo",
                table: "BaiNop",
                columns: new[] { "ma_bai_tap", "ma_hoc_sinh", "so_lan_nop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiNop_ma_hoc_sinh",
                schema: "dbo",
                table: "BaiNop",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_ma_mon_hoc",
                schema: "dbo",
                table: "BaiTap",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoRuiRoRotMon_ma_hoc_ky",
                schema: "dbo",
                table: "BaoCaoRuiRoRotMon",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoRuiRoRotMon_ma_hoc_sinh",
                schema: "dbo",
                table: "BaoCaoRuiRoRotMon",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoRuiRoRotMon_ma_mon_hoc",
                schema: "dbo",
                table: "BaoCaoRuiRoRotMon",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoRuiRoVang_ma_hoc_sinh",
                schema: "dbo",
                table: "BaoCaoRuiRoVang",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoRuiRoVang_ma_mon_hoc",
                schema: "dbo",
                table: "BaoCaoRuiRoVang",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoSuDungPhong_ma_don_vi",
                schema: "dbo",
                table: "BaoCaoSuDungPhong",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoSuDungPhong_ma_phong",
                schema: "dbo",
                table: "BaoCaoSuDungPhong",
                column: "ma_phong");

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
                name: "IX_BinhLuan_ma_bai_hoc",
                schema: "dbo",
                table: "BinhLuan",
                column: "ma_bai_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_ma_binh_luan_cha",
                schema: "dbo",
                table: "BinhLuan",
                column: "ma_binh_luan_cha");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_ma_nguoi_dung",
                schema: "dbo",
                table: "BinhLuan",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_DiemDanh_HanChinhSua",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "trang_thai_diem_danh", "diem_danh_han_chinh_sua_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_DiemDanh_HanGui",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "trang_thai_diem_danh", "diem_danh_han_gui_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_ca_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_ca_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_giao_vien",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_giao_vien_day_thay",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_giao_vien_day_thay");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_khoa_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_phong",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_Ngay_Ca_GiaoVien",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "ngay_hoc", "ma_ca_hoc", "ma_giao_vien" });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_Ngay_Ca_Phong",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "ngay_hoc", "ma_ca_hoc", "ma_phong" });

            migrationBuilder.CreateIndex(
                name: "UQ_BuoiHoc_Tkb_NgayHoc",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "ma_tkb", "ngay_hoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CaHoc_ten_ca",
                schema: "dbo",
                table: "CaHoc",
                column: "ten_ca",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CanhBaoBaoMat_ma_nguoi_dung",
                schema: "dbo",
                table: "CanhBaoBaoMat",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_CanhBaoDaoVan_ma_bai_nop",
                schema: "dbo",
                table: "CanhBaoDaoVan",
                column: "ma_bai_nop");

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
                name: "IX_CauHinhDiemMonHoc_ma_hoc_ky",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDiemMonHoc_ma_mon_hoc",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHocPhiChuongTrinh_ma_chuong_trinh_dao_tao",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                column: "ma_chuong_trinh_dao_tao");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHocPhiChuongTrinh_ma_hoc_ky",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_CauHinhHocPhiChuongTrinh_active_scope",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                columns: new[] { "ma_don_vi", "ma_chuong_trinh_dao_tao", "ma_hoc_ky" },
                unique: true,
                filter: "[con_hoat_dong] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhKhenThuong_ma_don_vi",
                schema: "dbo",
                table: "CauHinhKhenThuong",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_ma_mon_hoc",
                schema: "dbo",
                table: "CauHoi",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_nguoi_tao",
                schema: "dbo",
                table: "CauHoi",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoiDeKiemTra_ma_cau_hoi",
                schema: "dbo",
                table: "CauHoiDeKiemTra",
                column: "ma_cau_hoi");

            migrationBuilder.CreateIndex(
                name: "IX_Chuong_ma_mon_hoc",
                schema: "dbo",
                table: "Chuong",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_ma_khoa_tuyen_sinh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_khoa_tuyen_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_duyet_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_gui_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_gui_duyet_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_tu_choi_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_tu_choi_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_nguon_chuong_trinh_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguon_chuong_trinh_id");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                columns: new[] { "ma_chuyen_nganh", "ma_khoa_tuyen_sinh", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_ma_code_chuong_trinh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_code_chuong_trinh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhHocKy_ma_hoc_ky",
                schema: "dbo",
                table: "ChuongTrinhHocKy",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhHocKy_1",
                schema: "dbo",
                table: "ChuongTrinhHocKy",
                columns: new[] { "ma_chuong_trinh", "thu_tu_hoc_ky" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhHocKy_2",
                schema: "dbo",
                table: "ChuongTrinhHocKy",
                columns: new[] { "ma_chuong_trinh", "ma_hoc_ky" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganh_nganh_ten",
                schema: "dbo",
                table: "ChuyenNganh",
                columns: new[] { "ma_nganh", "ten_chuyen_nganh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganhTheoCoSo_ma_don_vi",
                schema: "dbo",
                table: "ChuyenNganhTheoCoSo",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganhTheoCoSo_1",
                schema: "dbo",
                table: "ChuyenNganhTheoCoSo",
                columns: new[] { "ma_chuyen_nganh", "ma_don_vi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHocPhan_LopHocPhan",
                schema: "dbo",
                table: "DangKyHocPhan",
                columns: new[] { "ma_lop_hoc_phan", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "UQ_DangKyHocPhan_1",
                schema: "dbo",
                table: "DangKyHocPhan",
                columns: new[] { "ma_hoc_sinh", "ma_lop_hoc_phan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaGiaoVien_ma_cau_hoi_dg",
                schema: "dbo",
                table: "DanhGiaGiaoVien",
                column: "ma_cau_hoi_dg");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaGiaoVien_ma_giao_vien",
                schema: "dbo",
                table: "DanhGiaGiaoVien",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaGiaoVien_ma_hoc_ky",
                schema: "dbo",
                table: "DanhGiaGiaoVien",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_DanhMucMonHoc_1",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_code_mon_hoc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachRuiRoRotMon_ma_hoc_ky",
                schema: "dbo",
                table: "DanhSachRuiRoRotMon",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachRuiRoRotMon_ma_hoc_sinh",
                schema: "dbo",
                table: "DanhSachRuiRoRotMon",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_DanhSachRuiRoRotMon_ma_mon_hoc",
                schema: "dbo",
                table: "DanhSachRuiRoRotMon",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_ma_don_vi",
                schema: "dbo",
                table: "DatPhong",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_ma_phong",
                schema: "dbo",
                table: "DatPhong",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_nguoi_duyet",
                schema: "dbo",
                table: "DatPhong",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_nguoi_yeu_cau",
                schema: "dbo",
                table: "DatPhong",
                column: "nguoi_yeu_cau");

            migrationBuilder.CreateIndex(
                name: "IX_DeCuongMonHoc_ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_chuong_trinh_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_DeCuongMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_DeCuongMonHoc_ma_don_vi",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_DeCuongMonHoc_1",
                schema: "dbo",
                table: "DeCuongMonHoc",
                columns: new[] { "ma_mon_hoc", "ma_chuyen_nganh", "ma_don_vi", "version" },
                unique: true,
                filter: "[ma_don_vi] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeKiemTra_ma_hoc_ky",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_DeKiemTra_ma_mon_hoc",
                schema: "dbo",
                table: "DeKiemTra",
                column: "ma_mon_hoc");

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

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_BuoiHoc_HocSinh",
                schema: "dbo",
                table: "DiemDanh",
                columns: new[] { "ma_buoi_hoc", "ma_hoc_sinh", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_DonVi_HocSinh",
                schema: "dbo",
                table: "DiemDanh",
                columns: new[] { "ma_don_vi", "ma_hoc_sinh" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_ma_don_vi_ma_buoi_hoc",
                schema: "dbo",
                table: "DiemDanh",
                columns: new[] { "ma_don_vi", "ma_buoi_hoc" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_ma_hoc_sinh",
                schema: "dbo",
                table: "DiemDanh",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_ma_yc_mo_khoa",
                schema: "dbo",
                table: "DiemDanh",
                column: "ma_yc_mo_khoa");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_nguoi_ghi_nhan",
                schema: "dbo",
                table: "DiemDanh",
                column: "nguoi_ghi_nhan");

            migrationBuilder.CreateIndex(
                name: "UQ_DiemDanh_1",
                schema: "dbo",
                table: "DiemDanh",
                columns: new[] { "ma_buoi_hoc", "ma_hoc_sinh" },
                unique: true);

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
                name: "IX_DiemSo_HocSinh_HocKy",
                schema: "dbo",
                table: "DiemSo",
                columns: new[] { "ma_hoc_sinh", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemSo_ma_don_vi_ma_hoc_ky",
                schema: "dbo",
                table: "DiemSo",
                columns: new[] { "ma_don_vi", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemSo_ma_hoc_ky",
                schema: "dbo",
                table: "DiemSo",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_DiemSo_ma_mon_hoc",
                schema: "dbo",
                table: "DiemSo",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_DiemSo_1",
                schema: "dbo",
                table: "DiemSo",
                columns: new[] { "ma_hoc_sinh", "ma_mon_hoc", "ma_hoc_ky" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_ma_hoc_sinh",
                schema: "dbo",
                table: "DonTu",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_nguoi_duyet_hien_tai",
                schema: "dbo",
                table: "DonTu",
                column: "nguoi_duyet_hien_tai");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_cap_don_vi",
                schema: "dbo",
                table: "DonVi",
                column: "cap_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_con_hoat_dong",
                schema: "dbo",
                table: "DonVi",
                column: "con_hoat_dong");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_ma_don_vi_cha",
                schema: "dbo",
                table: "DonVi",
                column: "ma_don_vi_cha");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDoanDangKy_ma_don_vi",
                schema: "dbo",
                table: "GiaiDoanDangKy",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDoanDangKy_ma_hoc_ky",
                schema: "dbo",
                table: "GiaiDoanDangKy",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_HoaDon_TrangThai",
                schema: "dbo",
                table: "GiaoDich",
                columns: new[] { "ma_hoa_don", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_ma_nguoi_thuc_hien",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_nguoi_thuc_hien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_ma_tai_khoan_nhan_tien",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tai_khoan_nhan_tien");

            migrationBuilder.CreateIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_cong",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tham_chieu_cong",
                unique: true,
                filter: "[ma_tham_chieu_cong] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tham_chieu_noi_bo",
                unique: true,
                filter: "[ma_tham_chieu_noi_bo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_ma_don_vi",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_ma_hoc_ky",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_nguoi_cap_nhat",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_nguoi_huy",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_huy");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_nguoi_tao",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UQ_HoaDon_HocSinh_HocKy_LoaiHoaDon",
                schema: "dbo",
                table: "HoaDon",
                columns: new[] { "ma_hoc_sinh", "ma_hoc_ky", "loai_hoa_don" },
                unique: true,
                filter: "[ma_hoc_ky] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_HoaDon_ma_hoa_don_code",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_hoa_don_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_HocKy_1",
                schema: "dbo",
                table: "HocKy",
                columns: new[] { "ma_don_vi", "nam_hoc", "thu_tu_trong_nam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_ma_don_vi",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_ma_hoc_sinh",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_tao",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_ma_hoc_ky",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_ma_hoc_sinh",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_giao_vien",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_hoc_ky",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_lop",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_lop_hoc_phan",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop_hoc_phan");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_mon_hoc",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_KhoaHoc_DonVi_MonHoc_HocKy_Lop",
                schema: "dbo",
                table: "KhoaHoc",
                columns: new[] { "ma_don_vi", "ma_mon_hoc", "ma_hoc_ky", "ma_lop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_KhoaTuyenSinh_1",
                schema: "dbo",
                table: "KhoaTuyenSinh",
                column: "ma_code_khoa",
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
                name: "IX_LienKetPhuHuynh_ma_hoc_sinh",
                schema: "dbo",
                table: "LienKetPhuHuynh",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "UQ_LienKetPhuHuynh_1",
                schema: "dbo",
                table: "LienKetPhuHuynh",
                columns: new[] { "ma_phu_huynh", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_ma_chuong_trinh",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_chuong_trinh");

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_ma_don_vi",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_ma_giao_vien_chu_nhiem",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_giao_vien_chu_nhiem");

            migrationBuilder.CreateIndex(
                name: "UQ_LopHanhChinh_1",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_code_lop",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_ma_don_vi",
                schema: "dbo",
                table: "LopHocPhan",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_ma_hoc_ky",
                schema: "dbo",
                table: "LopHocPhan",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_ma_mon_hoc",
                schema: "dbo",
                table: "LopHocPhan",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_LopHocPhan_1",
                schema: "dbo",
                table: "LopHocPhan",
                column: "ma_code_lop_hoc_phan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MauThongBao_1",
                schema: "dbo",
                table: "MauThongBao",
                columns: new[] { "loai_su_kien", "kenh_gui" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTienQuyet_ma_mon_tien_quyet",
                schema: "dbo",
                table: "MonHocTienQuyet",
                column: "ma_mon_tien_quyet");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTrongChuongTrinh_chuong_trinh_hoc_ky",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                columns: new[] { "ma_chuong_trinh", "hoc_ky_du_kien" });

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTrongChuongTrinh_ma_mon_hoc",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_MonHocTrongChuongTrinh_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                columns: new[] { "ma_chuong_trinh", "ma_mon_hoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NganhDaoTao_1",
                schema: "dbo",
                table: "NganhDaoTao",
                column: "ma_code_nganh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_ma_don_vi",
                schema: "dbo",
                table: "NguoiDung",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_ma_lop",
                schema: "dbo",
                table: "NguoiDung",
                column: "ma_lop");

            migrationBuilder.CreateIndex(
                name: "UQ_NguoiDung_1",
                schema: "dbo",
                table: "NguoiDung",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyDuyetDon_ma_don_tu",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                column: "ma_don_tu");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyDuyetDon_ma_nguoi_duyet",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                column: "ma_nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyKiemToan_ma_don_vi_thoi_diem",
                schema: "dbo",
                table: "NhatKyKiemToan",
                columns: new[] { "ma_don_vi", "thoi_diem_thay_doi" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyKiemToan_nguoi_thay_doi",
                schema: "dbo",
                table: "NhatKyKiemToan",
                column: "nguoi_thay_doi");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThayDoiDiem_ma_diem_so",
                schema: "dbo",
                table: "NhatKyThayDoiDiem",
                column: "ma_diem_so");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThayDoiDiem_nguoi_duyet",
                schema: "dbo",
                table: "NhatKyThayDoiDiem",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThayDoiDiem_nguoi_thay_doi",
                schema: "dbo",
                table: "NhatKyThayDoiDiem",
                column: "nguoi_thay_doi");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThongBao_ma_don_vi_gui_luc",
                schema: "dbo",
                table: "NhatKyThongBao",
                columns: new[] { "ma_don_vi", "gui_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThongBao_ma_nguoi_nhan",
                schema: "dbo",
                table: "NhatKyThongBao",
                column: "ma_nguoi_nhan");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThongBao_ma_thong_bao",
                schema: "dbo",
                table: "NhatKyThongBao",
                column: "ma_thong_bao");

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
                name: "IX_NopBaiDanhGia_ma_giao_vien",
                schema: "dbo",
                table: "NopBaiDanhGia",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_NopBaiDanhGia_ma_hoc_ky",
                schema: "dbo",
                table: "NopBaiDanhGia",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_NopBaiDanhGia_1",
                schema: "dbo",
                table: "NopBaiDanhGia",
                columns: new[] { "ma_hoc_sinh", "ma_giao_vien", "ma_hoc_ky" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetOtps_Email",
                schema: "dbo",
                table: "PasswordResetOtps",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetOtps_Email_IsUsed_CreatedAt",
                schema: "dbo",
                table: "PasswordResetOtps",
                columns: new[] { "Email", "IsUsed", "CreatedAt" });

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
                name: "IX_PhanQuyenNguoiDung_ma_vai_tro",
                schema: "dbo",
                table: "PhanQuyenNguoiDung",
                column: "ma_vai_tro");

            migrationBuilder.CreateIndex(
                name: "IX_PhienThiHocSinh_ma_hoc_sinh",
                schema: "dbo",
                table: "PhienThiHocSinh",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_PhienThiHocSinh_nguoi_xac_nhan_ky_ten",
                schema: "dbo",
                table: "PhienThiHocSinh",
                column: "nguoi_xac_nhan_ky_ten");

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_1",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_de_kiem_tra", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_CaThi_HocSinh",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" },
                unique: true,
                filter: "[ma_ca_thi] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuHoTro_ma_hoc_sinh",
                schema: "dbo",
                table: "PhieuHoTro",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuHoTro_phan_cong_cho",
                schema: "dbo",
                table: "PhieuHoTro",
                column: "phan_cong_cho");

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_ma_tang",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_tang");

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_ma_toa_nha",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_toa_nha");

            migrationBuilder.CreateIndex(
                name: "UQ_PhongHoc_DonVi_Code",
                schema: "dbo",
                table: "PhongHoc",
                columns: new[] { "ma_don_vi", "ma_code_phong" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNhanTien_nguoi_duyet",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNhanTien_nguoi_tao",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UQ_TaiKhoanNhanTien_DonVi_NganHang_SoTaiKhoan",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                columns: new[] { "ma_don_vi", "ma_ngan_hang", "so_tai_khoan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TaiKhoanNhanTien_DonVi_DefaultActive",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                column: "ma_don_vi",
                unique: true,
                filter: "[la_mac_dinh] = 1 AND [con_hoat_dong] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tang_ma_toa_nha",
                schema: "dbo",
                table: "Tang",
                column: "ma_toa_nha");

            migrationBuilder.CreateIndex(
                name: "UQ_Tang_ToaNha_ThuTu",
                schema: "dbo",
                table: "Tang",
                columns: new[] { "ma_toa_nha", "thu_tu_tang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThietBiPhong_ma_phong",
                schema: "dbo",
                table: "ThietBiPhong",
                column: "ma_phong");

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
                name: "IX_ThoiKhoaBieu_ma_ca_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_ca_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_phong",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                columns: new[] { "ma_khoa_hoc", "thu_trong_tuan", "ma_ca_hoc" },
                unique: true,
                filter: "[trang_thai] <> N'da_huy'");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_DonVi_NgayTao",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_don_vi", "ngay_tao" });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_ma_don_vi",
                schema: "dbo",
                table: "ThongBao",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaNhomThongBao",
                schema: "dbo",
                table: "ThongBao",
                column: "ma_nhom_thong_bao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_nguoi_tao",
                schema: "dbo",
                table: "ThongBao",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiNhan_DaDoc",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_nguoi_nhan", "da_doc" });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiNhan_DaDoc_NgayTao",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_nguoi_nhan", "da_doc", "ngay_tao" });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoHenGio_ma_don_vi",
                schema: "dbo",
                table: "ThongBaoHenGio",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoHenGio_nguoi_tao",
                schema: "dbo",
                table: "ThongBaoHenGio",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_TienDoBaiHoc_ma_bai_hoc",
                schema: "dbo",
                table: "TienDoBaiHoc",
                column: "ma_bai_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_TienDoBaiHoc_1",
                schema: "dbo",
                table: "TienDoBaiHoc",
                columns: new[] { "ma_hoc_sinh", "ma_bai_hoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanHoTro_ma_nguoi_gui",
                schema: "dbo",
                table: "TinNhanHoTro",
                column: "ma_nguoi_gui");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanHoTro_ma_phieu_ht",
                schema: "dbo",
                table: "TinNhanHoTro",
                column: "ma_phieu_ht");

            migrationBuilder.CreateIndex(
                name: "IX_ToaNha_ma_don_vi",
                schema: "dbo",
                table: "ToaNha",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_ToaNha_DonVi_Code",
                schema: "dbo",
                table: "ToaNha",
                columns: new[] { "ma_don_vi", "ma_code_toa_nha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenLamMoi_ma_nguoi_dung",
                schema: "dbo",
                table: "TokenLamMoi",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "UQ_TokenLamMoi_1",
                schema: "dbo",
                table: "TokenLamMoi",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_VaiTro_1",
                schema: "dbo",
                table: "VaiTro",
                column: "ma_code_vai_tro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XuatBaoCao_ma_don_vi",
                schema: "dbo",
                table: "XuatBaoCao",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_XuatBaoCao_nguoi_yeu_cau",
                schema: "dbo",
                table: "XuatBaoCao",
                column: "nguoi_yeu_cau");

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

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiLich_giao_vien_de_xuat",
                schema: "dbo",
                table: "YeuCauDoiLich",
                column: "giao_vien_de_xuat");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiLich_giao_vien_nhan_doi",
                schema: "dbo",
                table: "YeuCauDoiLich",
                column: "giao_vien_nhan_doi");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiLich_ma_tkb",
                schema: "dbo",
                table: "YeuCauDoiLich",
                column: "ma_tkb");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiLich_nguoi_duyet",
                schema: "dbo",
                table: "YeuCauDoiLich",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_ma_don_vi",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_ma_hoa_don",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "ma_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_ma_hoc_sinh",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_nguoi_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_nguoi_duyet",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_nguoi_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMoKhoaDiemDanh_nguoi_duyet",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMoKhoaDiemDanh_nguoi_yeu_cau",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                column: "nguoi_yeu_cau");

            migrationBuilder.CreateIndex(
                name: "UX_YeuCauMoKhoaDiemDanh_ChoDuyet",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                column: "ma_buoi_hoc",
                unique: true,
                filter: "[trang_thai] = N'cho_duyet'");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauSuaDiem_ma_diem_so",
                schema: "dbo",
                table: "YeuCauSuaDiem",
                column: "ma_diem_so");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauSuaDiem_nguoi_duyet",
                schema: "dbo",
                table: "YeuCauSuaDiem",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauSuaDiem_nguoi_yeu_cau",
                schema: "dbo",
                table: "YeuCauSuaDiem",
                column: "nguoi_yeu_cau");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiHocNoiDung_ma_de_kiem_tra__DeKiemTra",
                schema: "dbo",
                table: "BaiHocNoiDung",
                column: "ma_de_kiem_tra",
                principalSchema: "dbo",
                principalTable: "DeKiemTra",
                principalColumn: "ma_de_kiem_tra");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiNop_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "BaiNop",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaoRuiRoRotMon_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "BaoCaoRuiRoRotMon",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaoRuiRoVang_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "BaoCaoRuiRoVang",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BienBanThi_ma_ca_thi__CaThi",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_ca_thi",
                principalSchema: "dbo",
                principalTable: "CaThi",
                principalColumn: "ma_ca_thi");

            migrationBuilder.AddForeignKey(
                name: "FK_BienBanThi_ma_nguoi_lap__NguoiDung",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_nguoi_lap",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BienBanThi_ma_phien_thi__PhienThiHocSinh",
                schema: "dbo",
                table: "BienBanThi",
                column: "ma_phien_thi",
                principalSchema: "dbo",
                principalTable: "PhienThiHocSinh",
                principalColumn: "ma_phien_thi");

            migrationBuilder.AddForeignKey(
                name: "FK_BinhLuan_ma_nguoi_dung__NguoiDung",
                schema: "dbo",
                table: "BinhLuan",
                column: "ma_nguoi_dung",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BuoiHoc_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BuoiHoc_ma_giao_vien_day_thay__NguoiDung",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_giao_vien_day_thay",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_BuoiHoc_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_khoa_hoc",
                principalSchema: "dbo",
                principalTable: "KhoaHoc",
                principalColumn: "ma_khoa_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_BuoiHoc_ma_tkb__ThoiKhoaBieu",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_tkb",
                principalSchema: "dbo",
                principalTable: "ThoiKhoaBieu",
                principalColumn: "ma_tkb");

            migrationBuilder.AddForeignKey(
                name: "FK_CanhBaoBaoMat_ma_nguoi_dung__NguoiDung",
                schema: "dbo",
                table: "CanhBaoBaoMat",
                column: "ma_nguoi_dung",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_CaThi_ma_lich_thi_tong__LichThiTong",
                schema: "dbo",
                table: "CaThi",
                column: "ma_lich_thi_tong",
                principalSchema: "dbo",
                principalTable: "LichThiTong",
                principalColumn: "ma_lich_thi_tong");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHinhHocPhiChuongTrinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                column: "ma_chuong_trinh_dao_tao",
                principalSchema: "dbo",
                principalTable: "ChuongTrinhDaoTao",
                principalColumn: "ma_chuong_trinh");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoi_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "CauHoi",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoiDeKiemTra_ma_de_kiem_tra__DeKiemTra",
                schema: "dbo",
                table: "CauHoiDeKiemTra",
                column: "ma_de_kiem_tra",
                principalSchema: "dbo",
                principalTable: "DeKiemTra",
                principalColumn: "ma_de_kiem_tra");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_duyet_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_duyet_id",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_gui_duyet_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_gui_duyet_id",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_tu_choi_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "nguoi_tu_choi_id",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHocPhan_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DangKyHocPhan",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaGiaoVien_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "DanhGiaGiaoVien",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhSachRuiRoRotMon_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DanhSachRuiRoRotMon",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "DatPhong",
                column: "nguoi_duyet",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_nguoi_yeu_cau__NguoiDung",
                schema: "dbo",
                table: "DatPhong",
                column: "nguoi_yeu_cau",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

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
                name: "FK_DiemDanh_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DiemDanh",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanh_nguoi_ghi_nhan__NguoiDung",
                schema: "dbo",
                table: "DiemDanh",
                column: "nguoi_ghi_nhan",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanh_ma_yc_mo_khoa__YeuCauMoKhoaDiemDanh",
                schema: "dbo",
                table: "DiemDanh",
                column: "ma_yc_mo_khoa",
                principalSchema: "dbo",
                principalTable: "YeuCauMoKhoaDiemDanh",
                principalColumn: "ma_yc_mo_khoa");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhThi_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DiemDanhThi",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhThi_ma_nguoi_diem_danh__NguoiDung",
                schema: "dbo",
                table: "DiemDanhThi",
                column: "ma_nguoi_diem_danh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemSo_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DiemSo",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DonTu_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "DonTu",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DonTu_nguoi_duyet_hien_tai__NguoiDung",
                schema: "dbo",
                table: "DonTu",
                column: "nguoi_duyet_hien_tai",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDich_ma_hoa_don__HoaDon",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_hoa_don",
                principalSchema: "dbo",
                principalTable: "HoaDon",
                principalColumn: "ma_hoa_don");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDich_ma_nguoi_thuc_hien__NguoiDung",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_nguoi_thuc_hien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDich_ma_tai_khoan_nhan_tien__TaiKhoanNhanTien",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tai_khoan_nhan_tien",
                principalSchema: "dbo",
                principalTable: "TaiKhoanNhanTien",
                principalColumn: "ma_tai_khoan_nhan_tien");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_cap_nhat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_huy",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_lop__LopHanhChinh",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop",
                principalSchema: "dbo",
                principalTable: "LopHanhChinh",
                principalColumn: "ma_lop");

            migrationBuilder.AddForeignKey(
                name: "FK_LienKetPhuHuynh_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "LienKetPhuHuynh",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_LienKetPhuHuynh_ma_phu_huynh__NguoiDung",
                schema: "dbo",
                table: "LienKetPhuHuynh",
                column: "ma_phu_huynh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHanhChinh_ma_giao_vien_chu_nhiem__NguoiDung",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_giao_vien_chu_nhiem",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHanhChinh_ma_don_vi__DonVi",
                schema: "dbo",
                table: "LopHanhChinh");

            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_ma_don_vi__DonVi",
                schema: "dbo",
                table: "NguoiDung");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_duyet_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_gui_duyet_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTao_nguoi_tu_choi_id__NguoiDung",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHanhChinh_ma_giao_vien_chu_nhiem__NguoiDung",
                schema: "dbo",
                table: "LopHanhChinh");

            migrationBuilder.DropTable(
                name: "AnhChupPhanTich",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiHocNoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaoCaoRuiRoRotMon",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaoCaoRuiRoVang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaoCaoSuDungPhong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BienBanThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BinhLuan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CanhBaoBaoMat",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CanhBaoDaoVan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHinhDiemMonHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHinhHocPhiChuongTrinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHinhKhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHoiDeKiemTra",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHoiThuongGap",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChuongTrinhHocKy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChuyenNganhTheoCoSo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DangKyHocPhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhGiaGiaoVien",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhSachRuiRoRotMon",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DatPhong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeCuongMonHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiemDanh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiemDanhThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GiaiDoanDangKy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GiaoDich",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "HoSoKyLuat",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LienKetPhuHuynh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MauThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MonHocTienQuyet",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyDuyetDon",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyKiemToan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyThayDoiDiem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NopBaiDanhGia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PasswordResetOtps",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhanCongGiamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhanQuyenNguoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThietBiPhong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThiSinhCaThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThongBaoHenGio",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TienDoBaiHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TinNhanHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TokenLamMoi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TuyChonThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XuatBaoCao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XuLyViPhamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauDoiLich",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauHoanPhi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauSuaDiem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiNop",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHoi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHoiDanhGia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MonHocTrongChuongTrinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauMoKhoaDiemDanh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TaiKhoanNhanTien",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DonTu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VaiTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhieuHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyViPhamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "HoaDon",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiemSo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiTap",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BuoiHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Chuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhienThiHocSinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThoiKhoaBieu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CaThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CaHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhoaHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LichThiTong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhongHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LopHocPhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeKiemTra",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KyThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Tang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhMucMonHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "HocKy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ToaNha",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DonVi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NguoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LopHanhChinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChuyenNganh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhoaTuyenSinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NganhDaoTao",
                schema: "dbo");
        }
    }
}
