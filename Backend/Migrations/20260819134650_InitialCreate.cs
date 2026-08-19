using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CaHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten_ca = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buoi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time", nullable: false),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    thu_tu = table.Column<int>(type: "int", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaHoc", x => x.ma_ca_hoc);
                    table.CheckConstraint("CK_CaHoc_buoi", "`buoi` IN ('sang', 'chieu', 'toi')");
                    table.CheckConstraint("CK_CaHoc_gio", "`gio_ket_thuc` > `gio_bat_dau`");
                    table.CheckConstraint("CK_CaHoc_thu_tu", "`thu_tu` > 0");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHinhCanhBaoAi",
                schema: "dbo",
                columns: table => new
                {
                    MaCauHinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenQuyTac = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DieuKienKichHoat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NguongTriSo = table.Column<int>(type: "int", nullable: false),
                    KenhNhan = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhCanhBaoAi", x => x.MaCauHinh);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHoiDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi_dg = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    noi_dung_cau_hoi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiDanhGia", x => x.ma_cau_hoi_dg);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHoiThuongGap",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi_faq = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    danh_muc = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cau_hoi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tra_loi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiThuongGap", x => x.ma_cau_hoi_faq);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DonVi",
                schema: "dbo",
                columns: table => new
                {
                    ma_don_vi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi_cha = table.Column<int>(type: "int", nullable: true),
                    ten_don_vi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cap_don_vi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVi", x => x.ma_don_vi);
                    table.CheckConstraint("CK_DonVi_cap_don_vi_1", "`cap_don_vi` IN ('root', 'co_so', 'co_so_con')");
                    table.ForeignKey(
                        name: "FK_DonVi_ma_don_vi_cha__DonVi",
                        column: x => x.ma_don_vi_cha,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KhoaTuyenSinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_khoa_tuyen_sinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_code_khoa = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_khoa = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: false),
                    nam_ket_thuc_du_kien = table.Column<int>(type: "int", nullable: true),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaTuyenSinh", x => x.ma_khoa_tuyen_sinh);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoaiDauDiemQuaTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_loai_dau_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_loai = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu_hien_thi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiDauDiemQuaTrinh", x => x.ma_loai_dau_diem);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MauDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_danh_gia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten_mau = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cau_hinh_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dang_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauDanhGia", x => x.ma_mau_danh_gia);
                    table.CheckConstraint("CK_MauDanhGia_cau_hinh_json_ISJSON", "JSON_VALID(`cau_hinh_json`) = 1");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MauDonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_don = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loai_don = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_mau = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phien_ban = table.Column<int>(type: "int", nullable: false),
                    cau_hinh_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bat_buoc_minh_chung = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    so_tep_toi_da = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    dung_luong_tep_toi_da_byte = table.Column<long>(type: "bigint", nullable: false, defaultValue: 10485760L),
                    tong_dung_luong_toi_da_byte = table.Column<long>(type: "bigint", nullable: false, defaultValue: 26214400L),
                    sla_gio = table.Column<int>(type: "int", nullable: true),
                    dang_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauDonTu", x => x.ma_mau_don);
                    table.CheckConstraint("CK_MauDonTu_cau_hinh_json_ISJSON", "JSON_VALID(`cau_hinh_json`) = 1");
                    table.CheckConstraint("CK_MauDonTu_dung_luong_tep", "`dung_luong_tep_toi_da_byte` > 0");
                    table.CheckConstraint("CK_MauDonTu_phien_ban", "`phien_ban` > 0");
                    table.CheckConstraint("CK_MauDonTu_sla_gio", "`sla_gio` IS NULL OR `sla_gio` >= 0");
                    table.CheckConstraint("CK_MauDonTu_so_tep_toi_da", "`so_tep_toi_da` BETWEEN 0 AND 5");
                    table.CheckConstraint("CK_MauDonTu_tong_dung_luong", "`tong_dung_luong_toi_da_byte` >= `dung_luong_tep_toi_da_byte`");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NganhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_code_nganh = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_nganh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhDaoTao", x => x.ma_nganh);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OtpCode = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiredAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetOtps", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuyDoiTinChi",
                schema: "dbo",
                columns: table => new
                {
                    ma_quy_doi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    so_block_hoc = table.Column<int>(type: "int", nullable: false),
                    so_buoi_moi_tuan = table.Column<int>(type: "int", nullable: false),
                    so_ca_moi_buoi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyDoiTinChi", x => x.ma_quy_doi);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuyenHan",
                schema: "dbo",
                columns: table => new
                {
                    ma_quyen_han = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_quyen_han = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    module = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyenHan", x => x.ma_quyen_han);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuyTrinhDonTu",
                columns: table => new
                {
                    MaQuyTrinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LoaiDon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenQuyTrinh = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SlaKhoangThoiGian = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyTrinhDonTu", x => x.MaQuyTrinh);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VaiTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ma_code_vai_tro = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_vai_tro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.ma_vai_tro);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "yeu_cau_xuat_du_lieu",
                columns: table => new
                {
                    ma_yeu_cau = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_bao_cao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_bao_cao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_ky = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cap_don_vi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dinh_dang = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duong_dan_file = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_yeu_cau = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_gian_yeu_cau = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    thoi_gian_hoan_thanh = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yeu_cau_xuat_du_lieu", x => x.ma_yeu_cau);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHinhKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_kt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_khen_thuong = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gpa_toi_thieu = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhKhenThuong", x => x.ma_cau_hinh_kt);
                    table.CheckConstraint("CK_CauHinhKhenThuong_gpa_toi_thieu_1", "`gpa_toi_thieu` BETWEEN 0 AND 10");
                    table.ForeignKey(
                        name: "FK_CauHinhKhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HocKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_hoc_ky = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_hoc_ky = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: false),
                    nam_hoc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu_trong_nam = table.Column<int>(type: "int", nullable: false),
                    ngay_ket_thuc_block5 = table.Column<DateOnly>(type: "date", nullable: true),
                    da_khoa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    so_tin_chi_toi_da = table.Column<int>(type: "int", nullable: true),
                    han_rut_mon = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocKy", x => x.ma_hoc_ky);
                    table.CheckConstraint("CK_HocKy_thu_tu_trong_nam_1", "`thu_tu_trong_nam` IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_HocKy_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ToaNha",
                schema: "dbo",
                columns: table => new
                {
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_toa_nha = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_toa_nha = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_chi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tang = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToaNha", x => x.ma_toa_nha);
                    table.CheckConstraint("CK_ToaNha_so_tang_1", "`so_tang` IS NULL OR `so_tang` > 0");
                    table.ForeignKey(
                        name: "FK_ToaNha_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChuyenNganh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_nganh = table.Column<int>(type: "int", nullable: false),
                    ten_chuyen_nganh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BuocQuyTrinh",
                columns: table => new
                {
                    MaBuoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaQuyTrinh = table.Column<int>(type: "int", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TenBuoc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VaiTroXuLy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KieuBuoc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SlaKhoangThoiGian = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuocQuyTrinh", x => x.MaBuoc);
                    table.ForeignKey(
                        name: "FK_BuocQuyTrinh_QuyTrinhDonTu_MaQuyTrinh",
                        column: x => x.MaQuyTrinh,
                        principalTable: "QuyTrinhDonTu",
                        principalColumn: "MaQuyTrinh",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AnhChupPhanTich",
                schema: "dbo",
                columns: table => new
                {
                    ma_anh_chup = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ngay_anh_chup = table.Column<DateOnly>(type: "date", nullable: false),
                    loai_chi_so = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gia_tri_chi_so = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    chieu_loc_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhChupPhanTich", x => x.ma_anh_chup);
                    table.CheckConstraint("CK_AnhChupPhanTich_chieu_loc_json_ISJSON", "`chieu_loc_json` IS NULL OR JSON_VALID(`chieu_loc_json`) = 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Block",
                schema: "dbo",
                columns: table => new
                {
                    ma_block = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten_block = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaiDoanDangKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_giai_doan_dk = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tin_chi_toi_da = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaiDoanDangKy", x => x.ma_giai_doan_dk);
                    table.CheckConstraint("CK_GiaiDoanDangKy_trang_thai_1", "`trang_thai` IN ('nhap', 'dang_mo', 'da_dong')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KyThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_ky_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten_ky_thi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_nganh = table.Column<int>(type: "int", nullable: true),
                    loai_ky_thi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cuoi_ky")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyThi", x => x.ma_ky_thi);
                    table.CheckConstraint("CK_KyThi_LoaiKyThi", "loai_ky_thi IN ('giua_ky', 'cuoi_ky')");
                    table.CheckConstraint("CK_KyThi_trang_thai", "`trang_thai` IN ('nhap', 'dang_dien_ra', 'da_ket_thuc')");
                    table.ForeignKey(
                        name: "FK_KyThi_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_KyThi_ma_nganh__NganhDaoTao",
                        column: x => x.ma_nganh,
                        principalSchema: "dbo",
                        principalTable: "NganhDaoTao",
                        principalColumn: "ma_nganh");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tang",
                schema: "dbo",
                columns: table => new
                {
                    ma_tang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false),
                    ten_tang = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu_tang = table.Column<int>(type: "int", nullable: false),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChuyenNganhTheoCoSo",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh_co_so = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: true),
                    chi_tieu_du_kien = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganhTheoCoSo", x => x.ma_chuyen_nganh_co_so);
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_chi_tieu_du_kien_1", "`chi_tieu_du_kien` IS NULL OR `chi_tieu_du_kien` >= 0");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_nam_bat_dau_1", "`nam_bat_dau` IS NULL OR `nam_bat_dau` >= 2000");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_trang_thai_1", "`trang_thai` IN ('draft', 'pending_approval', 'approved', 'active', 'inactive', 'rejected')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DanhMucMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_code_mon_hoc = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_mon_hoc = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ma_nganh = table.Column<int>(type: "int", nullable: true),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucMonHoc", x => x.ma_mon_hoc);
                    table.CheckConstraint("CK_DanhMucMonHoc_so_tin_chi_1", "`so_tin_chi` > 0");
                    table.ForeignKey(
                        name: "FK_DanhMucMonHoc_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_DanhMucMonHoc_ma_nganh__NganhDaoTao",
                        column: x => x.ma_nganh,
                        principalSchema: "dbo",
                        principalTable: "NganhDaoTao",
                        principalColumn: "ma_nganh");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhongHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_toa_nha = table.Column<int>(type: "int", nullable: true),
                    ma_tang = table.Column<int>(type: "int", nullable: true),
                    ma_code_phong = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_phong = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    suc_chua = table.Column<int>(type: "int", nullable: false),
                    loai_phong = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_phong = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "hoat_dong")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongHoc", x => x.ma_phong);
                    table.CheckConstraint("CK_PhongHoc_loai_phong_2", "`loai_phong` IN ('ly_thuyet', 'phong_thi_nghiem', 'thuc_hanh', 'lab', 'hoi_truong', 'truc_tuyen', 'khac')");
                    table.CheckConstraint("CK_PhongHoc_suc_chua_1", "`suc_chua` > 0");
                    table.CheckConstraint("CK_PhongHoc_trang_thai_phong_3", "`trang_thai_phong` IN ('hoat_dong', 'bao_tri', 'ngung_hoat_dong')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHinhDauDiemQuaTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_dau_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_loai_dau_diem = table.Column<int>(type: "int", nullable: false),
                    so_luong_cot = table.Column<int>(type: "int", nullable: false),
                    trong_so_noi_bo = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhDauDiemQuaTrinh", x => x.ma_cau_hinh_dau_diem);
                    table.CheckConstraint("CK_CauHinhDauDiemQuaTrinh_so_luong_cot", "`so_luong_cot` > 0");
                    table.CheckConstraint("CK_CauHinhDauDiemQuaTrinh_trong_so_noi_bo", "`trong_so_noi_bo` BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_loai_dau_diem__LoaiDauDiemQuaTrinh",
                        column: x => x.ma_loai_dau_diem,
                        principalSchema: "dbo",
                        principalTable: "LoaiDauDiemQuaTrinh",
                        principalColumn: "ma_loai_dau_diem");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Chuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    da_an = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LopHocPhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_code_lop_hoc_phan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    suc_chua = table.Column<int>(type: "int", nullable: false),
                    so_dang_ky_toi_thieu = table.Column<int>(type: "int", nullable: true),
                    so_da_dang_ky = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "mo")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quota_vang_toi_da = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocPhan", x => x.ma_lop_hoc_phan);
                    table.CheckConstraint("CK_LopHocPhan_suc_chua_1", "`suc_chua` > 0");
                    table.CheckConstraint("CK_LopHocPhan_trang_thai_2", "`trang_thai` IN ('mo', 'dong', 'cho_huy', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                    table.CheckConstraint("CK_MonHocTienQuyet_diem_toi_thieu_1", "`diem_toi_thieu` BETWEEN 0 AND 10");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaoCaoSuDungPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_bc_su_dung_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    tu_ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    den_ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    so_gio_su_dung = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    ti_le_su_dung = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    tao_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoSuDungPhong", x => x.ma_bc_su_dung_phong);
                    table.CheckConstraint("CK_BaoCaoSuDungPhong_ti_le_su_dung_1", "`ti_le_su_dung` BETWEEN 0 AND 100");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThietBiPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_thiet_bi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ten_thiet_bi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_luong = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBiPhong", x => x.ma_thiet_bi);
                    table.CheckConstraint("CK_ThietBiPhong_so_luong_1", "`so_luong` >= 0");
                    table.ForeignKey(
                        name: "FK_ThietBiPhong_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaiTap",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_tap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    han_nop = table.Column<DateTime>(type: "datetime", nullable: false),
                    so_lan_nop_toi_da = table.Column<int>(type: "int", nullable: false, defaultValueSql: "3"),
                    dinh_dang_cho_phep = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    huong_dan_cham_diem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DungLuongToiThieuKB = table.Column<int>(type: "int", nullable: false),
                    DungLuongToiDaMB = table.Column<int>(type: "int", nullable: false),
                    MaCauHinhDauDiem = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiTap", x => x.ma_bai_tap);
                    table.CheckConstraint("CK_BaiTap_dinh_dang_cho_phep_ISJSON", "`dinh_dang_cho_phep` IS NULL OR JSON_VALID(`dinh_dang_cho_phep`) = 1");
                    table.CheckConstraint("CK_BaiTap_so_lan_nop_toi_da_1", "`so_lan_nop_toi_da` > 0");
                    table.CheckConstraint("CK_BaiTap_trang_thai_2", "`trang_thai` IN ('nhap', 'da_xuat_ban', 'da_dong')");
                    table.ForeignKey(
                        name: "FK_BaiTap_ma_cau_hinh_dau_diem__CauHinhDauDiemQuaTrinh",
                        column: x => x.MaCauHinhDauDiem,
                        principalSchema: "dbo",
                        principalTable: "CauHinhDauDiemQuaTrinh",
                        principalColumn: "ma_cau_hinh_dau_diem");
                    table.ForeignKey(
                        name: "FK_BaiTap_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_chuong = table.Column<int>(type: "int", nullable: false),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_bai_hoc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_tap_tin = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_luong_giay = table.Column<int>(type: "int", nullable: true),
                    noi_dung_van_ban = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dieu_kien_mo_khoa = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tom_tat_ai = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    da_an = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHoc", x => x.ma_bai_hoc);
                    table.CheckConstraint("CK_BaiHoc_dieu_kien_mo_khoa_ISJSON", "`dieu_kien_mo_khoa` IS NULL OR JSON_VALID(`dieu_kien_mo_khoa`) = 1");
                    table.CheckConstraint("CK_BaiHoc_loai_bai_hoc_1", "`loai_bai_hoc` IN ('video', 'pdf', 'van_ban', 'trac_nghiem', 'slide_html')");
                    table.CheckConstraint("CK_BaiHoc_thoi_luong_giay_2", "`thoi_luong_giay` >= 0");
                    table.CheckConstraint("CK_BaiHoc_trang_thai", "`trang_thai` IS NULL OR `trang_thai` IN ('nhap', 'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_BaiHoc_ma_chuong__Chuong",
                        column: x => x.ma_chuong,
                        principalSchema: "dbo",
                        principalTable: "Chuong",
                        principalColumn: "ma_chuong");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaiHocNoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    loai_noi_dung = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_html = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_tap_tin = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    storage_key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: true),
                    thoi_luong_giay = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHocNoiDung", x => x.ma_noi_dung);
                    table.CheckConstraint("CK_BaiHocNoiDung_loai_noi_dung", "`loai_noi_dung` IN ('video', 'slide_html', 'tai_lieu', 'quiz', 'van_ban')");
                    table.CheckConstraint("CK_BaiHocNoiDung_noi_dung_json_ISJSON", "`noi_dung_json` IS NULL OR JSON_VALID(`noi_dung_json`) = 1");
                    table.CheckConstraint("CK_BaiHocNoiDung_thoi_luong_giay", "`thoi_luong_giay` IS NULL OR `thoi_luong_giay` >= 0");
                    table.CheckConstraint("CK_BaiHocNoiDung_trang_thai", "`trang_thai` IS NULL OR `trang_thai` IN ('nhap', 'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_BaiHocNoiDung_ma_bai_hoc__BaiHoc",
                        column: x => x.ma_bai_hoc,
                        principalSchema: "dbo",
                        principalTable: "BaiHoc",
                        principalColumn: "ma_bai_hoc",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaiNop",
                schema: "dbo",
                columns: table => new
                {
                    ma_bai_nop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_bai_tap = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    url_tap_tin = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_lan_nop = table.Column<int>(type: "int", nullable: false),
                    nop_tre = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    diem_dao_van = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_so = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_ai_de_xuat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    nhan_xet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem_nop = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    da_cong_bo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiNop", x => x.ma_bai_nop);
                    table.CheckConstraint("CK_BaiNop_diem_ai_de_xuat_4", "`diem_ai_de_xuat` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_BaiNop_diem_dao_van_2", "`diem_dao_van` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_BaiNop_diem_so_3", "`diem_so` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_BaiNop_so_lan_nop_1", "`so_lan_nop` > 0");
                    table.ForeignKey(
                        name: "FK_BaiNop_ma_bai_tap__BaiTap",
                        column: x => x.ma_bai_tap,
                        principalSchema: "dbo",
                        principalTable: "BaiTap",
                        principalColumn: "ma_bai_tap");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CanhBaoDaoVan",
                schema: "dbo",
                columns: table => new
                {
                    ma_canh_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_bai_nop = table.Column<int>(type: "int", nullable: false),
                    diem_dao_van = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    chi_tiet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanhBaoDaoVan", x => x.ma_canh_bao);
                    table.CheckConstraint("CK_CanhBaoDaoVan_chi_tiet_ISJSON", "`chi_tiet` IS NULL OR JSON_VALID(`chi_tiet`) = 1");
                    table.CheckConstraint("CK_CanhBaoDaoVan_diem_dao_van_1", "`diem_dao_van` BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_CanhBaoDaoVan_ma_bai_nop__BaiNop",
                        column: x => x.ma_bai_nop,
                        principalSchema: "dbo",
                        principalTable: "BaiNop",
                        principalColumn: "ma_bai_nop");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaoCaoRuiRoRotMon",
                schema: "dbo",
                columns: table => new
                {
                    ma_bao_cao_rot = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    xac_suat_rot_mon = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dac_trung_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tao_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoRuiRoRotMon", x => x.ma_bao_cao_rot);
                    table.CheckConstraint("CK_BaoCaoRuiRoRotMon_dac_trung_json_ISJSON", "`dac_trung_json` IS NULL OR JSON_VALID(`dac_trung_json`) = 1");
                    table.CheckConstraint("CK_BaoCaoRuiRoRotMon_xac_suat_rot_mon_1", "`xac_suat_rot_mon` BETWEEN 0 AND 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BaoCaoRuiRoVang",
                schema: "dbo",
                columns: table => new
                {
                    ma_bao_cao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    diem_rui_ro = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dac_trung_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tao_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoRuiRoVang", x => x.ma_bao_cao);
                    table.CheckConstraint("CK_BaoCaoRuiRoVang_dac_trung_json_ISJSON", "`dac_trung_json` IS NULL OR JSON_VALID(`dac_trung_json`) = 1");
                    table.CheckConstraint("CK_BaoCaoRuiRoVang_diem_rui_ro_1", "`diem_rui_ro` BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_BaoCaoRuiRoVang_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BienBanThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_bien_ban = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_phien_thi = table.Column<int>(type: "int", nullable: true),
                    loai_bien_ban = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_nguoi_lap = table.Column<int>(type: "int", nullable: false),
                    thoi_diem_lap = table.Column<DateTime>(type: "datetime", nullable: false),
                    trang_thai_xu_ly = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_xu_ly")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienBanThi", x => x.ma_bien_ban);
                    table.CheckConstraint("CK_BienBanThi_loai_bien_ban", "`loai_bien_ban` IN ('gian_lan', 'su_co_diem_danh', 'quen_ky_ten', 'su_co_he_thong', 'khac')");
                    table.CheckConstraint("CK_BienBanThi_trang_thai_xu_ly", "`trang_thai_xu_ly` IN ('cho_xu_ly', 'da_xu_ly', 'huy_bo')");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                schema: "dbo",
                columns: table => new
                {
                    ma_binh_luan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    giay_trong_video = table.Column<int>(type: "int", nullable: true),
                    so_trang_pdf = table.Column<int>(type: "int", nullable: true),
                    ma_binh_luan_cha = table.Column<int>(type: "int", nullable: true),
                    da_ghim = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuan", x => x.ma_binh_luan);
                    table.CheckConstraint("CK_BinhLuan_giay_trong_video_1", "`giay_trong_video` >= 0");
                    table.CheckConstraint("CK_BinhLuan_so_trang_pdf_2", "`so_trang_pdf` > 0");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BuoiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_tkb = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    ngay_hoc = table.Column<DateOnly>(type: "date", nullable: false),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien_day_thay = table.Column<int>(type: "int", nullable: true),
                    trang_thai_buoi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "du_kien")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_thay_doi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_thay_doi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    khoa_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    diem_danh_bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    diem_danh_han_gui_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    diem_danh_da_gui_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    diem_danh_han_chinh_sua_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    diem_danh_khoa_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    trang_thai_diem_danh = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_mo")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuoiHoc", x => x.ma_buoi_hoc);
                    table.CheckConstraint("CK_BuoiHoc_loai_thay_doi", "`loai_thay_doi` IS NULL OR `loai_thay_doi` IN ('doi_giang_vien', 'doi_phong', 'doi_ca', 'huy_buoi', 'doi_lich')");
                    table.CheckConstraint("CK_BuoiHoc_trang_thai_buoi", "`trang_thai_buoi` IN ('du_kien', 'da_dien_ra', 'da_huy', 'doi_lich', 'day_thay')");
                    table.CheckConstraint("CK_BuoiHoc_trang_thai_diem_danh", "`trang_thai_diem_danh` IN ('chua_mo', 'dang_diem_danh', 'da_gui', 'da_khoa')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CanhBaoBaoMat",
                schema: "dbo",
                columns: table => new
                {
                    ma_canh_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    diem_rui_ro = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    dia_chi_ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thong_tin_trinh_duyet = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "mo")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanhBaoBaoMat", x => x.ma_canh_bao);
                    table.CheckConstraint("CK_CanhBaoBaoMat_diem_rui_ro_1", "`diem_rui_ro` BETWEEN 0 AND 1");
                    table.CheckConstraint("CK_CanhBaoBaoMat_trang_thai_2", "`trang_thai` IN ('mo', 'da_xem', 'bo_qua')");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CaThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_lich_thi_tong = table.Column<int>(type: "int", nullable: false),
                    ten_ca_thi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_phong = table.Column<int>(type: "int", nullable: true),
                    ngay_thi = table.Column<DateTime>(type: "datetime", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_dieu_chinh = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaThi", x => x.ma_ca_thi);
                    table.CheckConstraint("CK_CaThi_thoi_gian", "`thoi_gian_ket_thuc` > `thoi_gian_bat_dau`");
                    table.CheckConstraint("CK_CaThi_trang_thai", "`trang_thai` IN ('nhap', 'cho_phan_cong', 'da_san_sang', 'dang_diem_danh', 'dang_thi', 'da_ket_thuc', 'da_huy', 'su_co')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHinhDiemMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    trong_so_qua_trinh = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    trong_so_giua_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    trong_so_cuoi_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    nguong_dat = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValueSql: "5"),
                    ti_le_chuyen_can_toi_thieu = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValueSql: "0"),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhDiemMonHoc", x => x.ma_cau_hinh_diem);
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_nguong_dat_4", "`nguong_dat` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_ti_le_chuyen_can_toi_thieu_5", "`ti_le_chuyen_can_toi_thieu` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_cuoi_ky_3", "`trong_so_cuoi_ky` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_giua_ky_2", "`trong_so_giua_ky` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_CauHinhDiemMonHoc_trong_so_qua_trinh_1", "`trong_so_qua_trinh` BETWEEN 0 AND 100");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHinhHocPhiChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_hoc_phi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_chuong_trinh_dao_tao = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    nam_hoc_trong_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_trong_nam = table.Column<int>(type: "int", nullable: false),
                    so_thu_tu_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    loai_cach_tinh_hoc_phi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "co_dinh_theo_hoc_ky")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tien_hoc_phi = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    tien_hoc_lieu = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    tong_tien_du_kien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHocPhiChuongTrinh", x => x.ma_cau_hinh_hoc_phi);
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_hoc_ky_trong_nam", "`hoc_ky_trong_nam` IN (1, 2, 3)");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_loai_cach_tinh", "`loai_cach_tinh_hoc_phi` IN ('co_dinh_theo_hoc_ky', 'theo_tin_chi', 'theo_mon_hoc')");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_nam_hoc", "`nam_hoc_trong_chuong_trinh` >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_thu_tu", "`so_thu_tu_hoc_ky` >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_tien_hoc_phi", "`so_tien_hoc_phi` >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tien_hoc_lieu", "`tien_hoc_lieu` >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tong_tien", "`tong_tien_du_kien` = `so_tien_hoc_phi` + `tien_hoc_lieu`");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CauHoi",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    loai_cau_hoi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kieu_lua_chon = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lua_chon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dap_an_dung = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    giai_thich_dap_an = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    do_kho = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.ma_cau_hoi);
                    table.CheckConstraint("CK_CauHoi_dap_an_dung_ISJSON", "`dap_an_dung` IS NULL OR JSON_VALID(`dap_an_dung`) = 1");
                    table.CheckConstraint("CK_CauHoi_do_kho_2", "`do_kho` IN ('de', 'trung_binh', 'kho')");
                    table.CheckConstraint("CK_CauHoi_kieu_lua_chon", "`kieu_lua_chon` IS NULL OR `kieu_lua_chon` IN ('chon_mot', 'chon_nhieu')");
                    table.CheckConstraint("CK_CauHoi_loai_cau_hoi_1", "`loai_cau_hoi` IN ('trac_nghiem', 'tu_luan')");
                    table.CheckConstraint("CK_CauHoi_lua_chon_ISJSON", "`lua_chon` IS NULL OR JSON_VALID(`lua_chon`) = 1");
                    table.ForeignKey(
                        name: "FK_CauHoi_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_tuyen_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_code_chuong_trinh = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_chuong_trinh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_dao_tao_thang = table.Column<int>(type: "int", nullable: false),
                    tong_tin_chi_yeu_cau = table.Column<int>(type: "int", nullable: true),
                    so_tin_chi_toi_thieu_moi_ky = table.Column<int>(type: "int", nullable: true),
                    so_tin_chi_toi_da_moi_ky = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguon_chuong_trinh_id = table.Column<int>(type: "int", nullable: true),
                    ghi_chu_thay_doi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_het_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    nguoi_gui_duyet_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_gui_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_duyet_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu_duyet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tu_choi_id = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_tu_choi = table.Column<DateTime>(type: "datetime", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTao", x => x.ma_chuong_trinh);
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_so_hoc_ky", "`so_hoc_ky` > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_thoi_gian_dao_tao_thang", "`thoi_gian_dao_tao_thang` > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tin_chi_toi_da_moi_ky", "`so_tin_chi_toi_da_moi_ky` IS NULL OR `so_tin_chi_toi_da_moi_ky` > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tin_chi_toi_thieu_moi_ky", "`so_tin_chi_toi_thieu_moi_ky` IS NULL OR `so_tin_chi_toi_thieu_moi_ky` >= 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_tong_tin_chi_yeu_cau", "`tong_tin_chi_yeu_cau` IS NULL OR `tong_tin_chi_yeu_cau` > 0");
                    table.CheckConstraint("CK_ChuongTrinhDaoTao_trang_thai", "`trang_thai` IN ('draft', 'pending_approval', 'approved', 'rejected', 'active', 'inactive', 'archived')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChuongTrinhHocKy",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh_hoc_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    thu_tu_hoc_ky = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhHocKy", x => x.ma_chuong_trinh_hoc_ky);
                    table.CheckConstraint("CK_ChuongTrinhHocKy_thu_tu_hoc_ky_1", "`thu_tu_hoc_ky` > 0");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MonHocTrongChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh_mon_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: false),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    loai_mon_hoc = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bat_buoc = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTrongChuongTrinh", x => x.ma_chuong_trinh_mon_hoc);
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_hoc_ky_du_kien", "`hoc_ky_du_kien` > 0");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_loai_mon_hoc", "`loai_mon_hoc` IN ('bat_buoc', 'tu_chon', 'thay_the')");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_so_tin_chi", "`so_tin_chi` > 0");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeCuongMonHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_syllabus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ma_chuong_trinh_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ten_syllabus = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: true),
                    bat_buoc = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeCuongMonHoc", x => x.ma_syllabus);
                    table.CheckConstraint("CK_DeCuongMonHoc_hoc_ky_du_kien_1", "`hoc_ky_du_kien` IS NULL OR `hoc_ky_du_kien` > 0");
                    table.CheckConstraint("CK_DeCuongMonHoc_trang_thai_1", "`trang_thai` IN ('draft', 'pending_approval', 'approved', 'active', 'inactive', 'archived')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DangKyHocPhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_dang_ky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vi_tri_cho = table.Column<int>(type: "int", nullable: true),
                    la_hoc_lai = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    kiem_tra_tien_quyet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    da_kiem_tra_tien_quyet = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyHocPhan", x => x.ma_dang_ky);
                    table.CheckConstraint("CK_DangKyHocPhan_trang_thai_1", "`trang_thai` IN ('da_dang_ky', 'danh_sach_cho', 'da_rut', 'lop_bi_huy')");
                    table.CheckConstraint("CK_DangKyHocPhan_vi_tri_cho_2", "`vi_tri_cho` > 0");
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_ma_lop_hoc_phan__LopHocPhan",
                        column: x => x.ma_lop_hoc_phan,
                        principalSchema: "dbo",
                        principalTable: "LopHocPhan",
                        principalColumn: "ma_lop_hoc_phan");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DanhGiaGiaoVien",
                schema: "dbo",
                columns: table => new
                {
                    ma_danh_gia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_cau_hoi_dg = table.Column<int>(type: "int", nullable: false),
                    diem_so = table.Column<int>(type: "int", nullable: false),
                    nhan_xet_tu_do = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ai_cam_xuc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ai_chu_de = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    cohort_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaGiaoVien", x => x.ma_danh_gia);
                    table.CheckConstraint("CK_DanhGiaGiaoVien_ai_cam_xuc_2", "`ai_cam_xuc` IN ('tich_cuc', 'trung_tinh', 'tieu_cuc')");
                    table.CheckConstraint("CK_DanhGiaGiaoVien_ai_chu_de_ISJSON", "`ai_chu_de` IS NULL OR JSON_VALID(`ai_chu_de`) = 1");
                    table.CheckConstraint("CK_DanhGiaGiaoVien_diem_so_1", "`diem_so` BETWEEN 1 AND 5");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DanhSachRuiRoRotMon",
                schema: "dbo",
                columns: table => new
                {
                    ma_rui_ro_rot = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    xac_suat_rot_mon = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    tao_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachRuiRoRotMon", x => x.ma_rui_ro_rot);
                    table.CheckConstraint("CK_DanhSachRuiRoRotMon_xac_suat_rot_mon_1", "`xac_suat_rot_mon` BETWEEN 0 AND 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DatPhong",
                schema: "dbo",
                columns: table => new
                {
                    ma_dat_phong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    muc_dich = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    so_nguoi_tham_du = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_duyet")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatPhong", x => x.ma_dat_phong);
                    table.CheckConstraint("CK_DatPhong_ket_thuc_luc_1", "`ket_thuc_luc` > `bat_dau_luc`");
                    table.CheckConstraint("CK_DatPhong_so_nguoi_tham_du_2", "`so_nguoi_tham_du` >= 0");
                    table.CheckConstraint("CK_DatPhong_trang_thai_3", "`trang_thai` IN ('cho_duyet', 'da_xac_nhan', 'tu_choi', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeKiemTra",
                schema: "dbo",
                columns: table => new
                {
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_gian_phut = table.Column<int>(type: "int", nullable: false),
                    cau_hinh_de_thi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_de_thi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hinh_thuc_thi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ty_le_trac_nghiem = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ty_le_tu_luan = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ma_nguoi_soan = table.Column<int>(type: "int", nullable: true),
                    ma_nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    trang_thai_duyet = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeKiemTra", x => x.ma_de_kiem_tra);
                    table.CheckConstraint("CK_DeKiemTra_cau_hinh_de_thi_ISJSON", "`cau_hinh_de_thi` IS NULL OR JSON_VALID(`cau_hinh_de_thi`) = 1");
                    table.CheckConstraint("CK_DeKiemTra_hinh_thuc_thi", "`hinh_thuc_thi` IS NULL OR `hinh_thuc_thi` IN ('online_tap_trung', 'online_tu_do', 'van_dap')");
                    table.CheckConstraint("CK_DeKiemTra_loai_de_thi", "`loai_de_thi` IS NULL OR `loai_de_thi` IN ('trac_nghiem', 'tu_luan', 'ket_hop', 'quiz_bai_hoc', 'progress_test')");
                    table.CheckConstraint("CK_DeKiemTra_thoi_gian_phut", "`thoi_gian_phut` > 0");
                    table.CheckConstraint("CK_DeKiemTra_thoi_gian_phut_1", "`thoi_gian_phut` BETWEEN 1 AND 240");
                    table.CheckConstraint("CK_DeKiemTra_trang_thai_2", "`trang_thai` IN ('nhap', 'da_len_lich', 'dang_mo', 'da_dong', 'da_cong_bo')");
                    table.CheckConstraint("CK_DeKiemTra_trang_thai_duyet", "`trang_thai_duyet` IS NULL OR `trang_thai_duyet` IN ('nhap', 'cho_duyet', 'da_duyet', 'tu_choi')");
                    table.CheckConstraint("CK_DeKiemTra_ty_le_trac_nghiem", "`ty_le_trac_nghiem` IS NULL OR `ty_le_trac_nghiem` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_DeKiemTra_ty_le_tu_luan", "`ty_le_tu_luan` IS NULL OR `ty_le_tu_luan` BETWEEN 0 AND 100");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LichThiTong",
                schema: "dbo",
                columns: table => new
                {
                    ma_lich_thi_tong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ky_thi = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: true),
                    hinh_thuc_thi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_thi_du_kien = table.Column<DateTime>(type: "datetime", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichThiTong", x => x.ma_lich_thi_tong);
                    table.CheckConstraint("CK_LichThiTong_hinh_thuc_thi", "`hinh_thuc_thi` IN ('online_tap_trung', 'online_tu_do', 'van_dap')");
                    table.CheckConstraint("CK_LichThiTong_trang_thai", "`trang_thai` IN ('nhap', 'da_gui_ve_co_so', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_danh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_ghi_nhan = table.Column<int>(type: "int", nullable: false),
                    ghi_nhan_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    khoa_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    he_so_vang = table.Column<int>(type: "int", nullable: false),
                    ma_yc_mo_khoa = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.ma_diem_danh);
                    table.CheckConstraint("CK_DiemDanh_he_so_vang_2", "`he_so_vang` >= 0");
                    table.CheckConstraint("CK_DiemDanh_trang_thai_1", "`trang_thai` IN ('co_mat', 'vang', 'di_muon', 'co_phep')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiemDanhThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_danh_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai_diem_danh = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "vang_mat")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem_diem_danh = table.Column<DateTime>(type: "datetime", nullable: true),
                    ma_nguoi_diem_danh = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanhThi", x => x.ma_diem_danh_thi);
                    table.CheckConstraint("CK_DiemDanhThi_trang_thai", "`trang_thai_diem_danh` IN ('co_mat', 'vang_mat', 'di_muon_qua_gio', 'su_co')");
                    table.ForeignKey(
                        name: "FK_DiemDanhThi_ma_ca_thi__CaThi",
                        column: x => x.ma_ca_thi,
                        principalSchema: "dbo",
                        principalTable: "CaThi",
                        principalColumn: "ma_ca_thi");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiemSo",
                schema: "dbo",
                columns: table => new
                {
                    ma_diem_so = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    diem_qua_trinh = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_giua_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_cuoi_ky = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    gpa_mon_hoc = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "chua_hoan_thanh")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    da_khoa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ly_do_rot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemSo", x => x.ma_diem_so);
                    table.CheckConstraint("CK_DiemSo_diem_cuoi_ky_3", "`diem_cuoi_ky` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_diem_giua_ky_2", "`diem_giua_ky` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_diem_qua_trinh_1", "`diem_qua_trinh` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_gpa_mon_hoc_4", "`gpa_mon_hoc` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_DiemSo_ly_do_rot_ISJSON", "`ly_do_rot` IS NULL OR JSON_VALID(`ly_do_rot`) = 1");
                    table.CheckConstraint("CK_DiemSo_trang_thai_5", "`trang_thai` IN ('dat', 'rot', 'chua_hoan_thanh', 'cho_hoan_thanh_bo_sung')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_don_tu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_mau_don = table.Column<int>(type: "int", nullable: true),
                    loai_don = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_xu_ly_nghiep_vu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "chua_xu_ly")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet_hien_tai = table.Column<int>(type: "int", nullable: true),
                    nguoi_xu_ly_cuoi = table.Column<int>(type: "int", nullable: true),
                    du_lieu_bieu_mau = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_bang_chung = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_yeu_cau_bo_sung = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_qua_xu_ly_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nhat_ky_tu_dong = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_nop = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    han_xu_ly_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    row_version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonTu", x => x.ma_don_tu);
                    table.CheckConstraint("CK_DonTu_du_lieu_bieu_mau_ISJSON", "`du_lieu_bieu_mau` IS NULL OR JSON_VALID(`du_lieu_bieu_mau`) = 1");
                    table.CheckConstraint("CK_DonTu_ket_qua_xu_ly_json_ISJSON", "`ket_qua_xu_ly_json` IS NULL OR JSON_VALID(`ket_qua_xu_ly_json`) = 1");
                    table.CheckConstraint("CK_DonTu_loai_don_1", "`loai_don` IN ('nghi_phep', 'thi_lai', 'chuyen_truong', 'cap_chung_chi', 'khac', 'phuc_tra_diem', 'bao_luu', 'chuyen_nganh', 'chuyen_co_so', 'xac_nhan', 'rut_hoc')");
                    table.CheckConstraint("CK_DonTu_nhat_ky_tu_dong_ISJSON", "`nhat_ky_tu_dong` IS NULL OR JSON_VALID(`nhat_ky_tu_dong`) = 1");
                    table.CheckConstraint("CK_DonTu_trang_thai_2", "`trang_thai` IN ('nhap', 'da_nop', 'dang_xem_xet', 'yeu_cau_bo_sung', 'da_duyet', 'tu_choi', 'da_huy')");
                    table.CheckConstraint("CK_DonTu_trang_thai_xu_ly_nghiep_vu", "`trang_thai_xu_ly_nghiep_vu` IN ('chua_xu_ly', 'cho_xu_ly', 'da_ghi_nhan', 'xu_ly_thanh_cong', 'xu_ly_that_bai', 'can_xu_ly_thu_cong')");
                    table.ForeignKey(
                        name: "FK_DonTu_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_DonTu_ma_mau_don__MauDonTu",
                        column: x => x.ma_mau_don,
                        principalSchema: "dbo",
                        principalTable: "MauDonTu",
                        principalColumn: "ma_mau_don");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DotKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_dot_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ten_dot = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_dot = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "TOP_100_HOC_KY")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_luong_toi_da = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    tieu_chi_xet_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_mau_bang_khen = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_cong_bo = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotKhenThuong", x => x.ma_dot_khen_thuong);
                    table.CheckConstraint("CK_DotKhenThuong_loai_dot", "`loai_dot` IN ('TOP_100_HOC_KY')");
                    table.CheckConstraint("CK_DotKhenThuong_so_luong_toi_da", "`so_luong_toi_da` > 0");
                    table.CheckConstraint("CK_DotKhenThuong_tieu_chi_xet_json", "`tieu_chi_xet_json` IS NULL OR JSON_VALID(`tieu_chi_xet_json`) = 1");
                    table.CheckConstraint("CK_DotKhenThuong_trang_thai", "`trang_thai` IN ('nhap', 'dang_xet', 'cho_duyet', 'da_duyet', 'da_cong_bo', 'da_huy')");
                    table.ForeignKey(
                        name: "FK_DotKhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_DotKhenThuong_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaoDich",
                schema: "dbo",
                columns: table => new
                {
                    ma_giao_dich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false),
                    ma_tai_khoan_nhan_tien = table.Column<int>(type: "int", nullable: true),
                    ma_tham_chieu_noi_bo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_tham_chieu_cong = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    loai_giao_dich = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nha_cung_cap_thanh_toan = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_chuyen_khoan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    qr_payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    qr_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    checkout_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    request_payload_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    response_payload_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    callback_payload_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_het_han = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_thanh_toan = table.Column<DateTime>(type: "datetime", nullable: true),
                    ma_nguoi_thuc_hien = table.Column<int>(type: "int", nullable: true),
                    chu_thich = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDich", x => x.ma_giao_dich);
                    table.CheckConstraint("CK_GiaoDich_callback_payload_json", "`callback_payload_json` IS NULL OR JSON_VALID(`callback_payload_json`) = 1");
                    table.CheckConstraint("CK_GiaoDich_loai_giao_dich", "`loai_giao_dich` IN ('phat_sinh_hoc_phi', 'thanh_toan_hoc_phi', 'dieu_chinh_cong_no', 'hoan_tien', 'huy_hoa_don')");
                    table.CheckConstraint("CK_GiaoDich_provider", "`nha_cung_cap_thanh_toan` IS NULL OR `nha_cung_cap_thanh_toan` IN ('payos', 'vietqr')");
                    table.CheckConstraint("CK_GiaoDich_request_payload_json", "`request_payload_json` IS NULL OR JSON_VALID(`request_payload_json`) = 1");
                    table.CheckConstraint("CK_GiaoDich_response_payload_json", "`response_payload_json` IS NULL OR JSON_VALID(`response_payload_json`) = 1");
                    table.CheckConstraint("CK_GiaoDich_trang_thai", "`trang_thai` IN ('phat_sinh', 'cho_thanh_toan', 'dang_xu_ly', 'thanh_cong', 'that_bai', 'het_han', 'da_huy', 'sai_so_tien', 'cho_xu_ly_thu_cong')");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaoVienChuyenNganh",
                columns: table => new
                {
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    la_chuyen_mon_chinh = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    muc_do_phu_hop = table.Column<int>(type: "int", nullable: false),
                    so_nam_kinh_nghiem = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienChuyenNganh", x => new { x.ma_giao_vien, x.ma_chuyen_nganh });
                    table.CheckConstraint("CK_GiaoVienChuyenNganh_muc_do_phu_hop", "`muc_do_phu_hop` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_GiaoVienChuyenNganh_so_nam_kinh_nghiem", "`so_nam_kinh_nghiem` IS NULL OR `so_nam_kinh_nghiem` >= 0");
                    table.ForeignKey(
                        name: "FK_GiaoVienChuyenNganh_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaoVienMonHoc",
                columns: table => new
                {
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    muc_do_phu_hop = table.Column<int>(type: "int", nullable: false),
                    so_lan_da_day = table.Column<int>(type: "int", nullable: false),
                    so_nam_kinh_nghiem = table.Column<int>(type: "int", nullable: true),
                    la_mon_chinh = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienMonHoc", x => new { x.ma_giao_vien, x.ma_mon_hoc });
                    table.CheckConstraint("CK_GiaoVienMonHoc_muc_do_phu_hop", "`muc_do_phu_hop` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_GiaoVienMonHoc_so_lan_da_day", "`so_lan_da_day` >= 0");
                    table.CheckConstraint("CK_GiaoVienMonHoc_so_nam_kinh_nghiem", "`so_nam_kinh_nghiem` IS NULL OR `so_nam_kinh_nghiem` >= 0");
                    table.ForeignKey(
                        name: "FK_GiaoVienMonHoc_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaoVienNguyenVongCaDay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NguyenVongId = table.Column<int>(type: "int", nullable: false),
                    ThuTrongTuan = table.Column<int>(type: "int", nullable: false),
                    MaCaHoc = table.Column<int>(type: "int", nullable: false),
                    MucDo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiaoVienNguyenVongHocKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaGiaoVien = table.Column<int>(type: "int", nullable: false),
                    MaHocKy = table.Column<int>(type: "int", nullable: false),
                    MaDonVi = table.Column<int>(type: "int", nullable: false),
                    SoLopToiDaMongMuon = table.Column<int>(type: "int", nullable: true),
                    SoCaToiDaMoiTuan = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrangThai = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HoaDon",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ma_hoa_don_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_hoa_don = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "hoc_phi")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    giam_tru = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    da_thanh_toan = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_thanh_toan")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    han_thanh_toan = table.Column<DateOnly>(type: "date", nullable: false),
                    url_hoa_don_pdf = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_huy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_huy = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    nguoi_huy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.ma_hoa_don);
                    table.CheckConstraint("CK_HoaDon_da_thanh_toan", "`da_thanh_toan` >= 0");
                    table.CheckConstraint("CK_HoaDon_giam_tru", "`giam_tru` >= 0");
                    table.CheckConstraint("CK_HoaDon_loai_hoa_don", "`loai_hoa_don` IN ('hoc_phi', 'le_phi', 'tai_lieu', 'khac')");
                    table.CheckConstraint("CK_HoaDon_so_tien", "`so_tien` >= 0");
                    table.CheckConstraint("CK_HoaDon_trang_thai", "`trang_thai` IN ('chua_thanh_toan', 'thanh_toan_mot_phan', 'da_thanh_toan', 'qua_han', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HoSoKyLuat",
                schema: "dbo",
                columns: table => new
                {
                    ma_ky_luat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_ky_luat = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_do_vi_pham = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhe")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hinh_thuc_xu_ly = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhac_nho")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_vi_pham = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "DATE(UTC_TIMESTAMP())"),
                    can_cu_xu_ly = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_noi_bo = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_huy = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_huy = table.Column<int>(type: "int", nullable: true),
                    ngay_huy = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_het_hieu_luc = table.Column<DateOnly>(type: "date", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_duyet = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chung_tu_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_ap_dung = table.Column<int>(type: "int", nullable: true),
                    ngay_ap_dung = table.Column<DateTime>(type: "datetime", nullable: true),
                    da_go_ky_luat = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ly_do_go_ky_luat = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_go_ky_luat = table.Column<int>(type: "int", nullable: true),
                    ngay_go_ky_luat = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    loai_doi_tuong_lien_ket = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_doi_tuong_lien_ket = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoKyLuat", x => x.ma_ky_luat);
                    table.CheckConstraint("CK_HoSoKyLuat_chung_tu_json", "`chung_tu_json` IS NULL OR JSON_VALID(`chung_tu_json`) = 1");
                    table.CheckConstraint("CK_HoSoKyLuat_hinh_thuc_xu_ly", "`hinh_thuc_xu_ly` IN ('nhac_nho', 'khien_trach', 'canh_cao', 'dinh_chi', 'khac')");
                    table.CheckConstraint("CK_HoSoKyLuat_muc_do_vi_pham", "`muc_do_vi_pham` IN ('nhe', 'trung_binh', 'nghiem_trong')");
                    table.CheckConstraint("CK_HoSoKyLuat_trang_thai", "`trang_thai` IN ('nhap', 'cho_duyet', 'da_duyet', 'tu_choi', 'dang_hieu_luc', 'het_hieu_luc', 'da_go_hieu_luc', 'da_huy')");
                    table.ForeignKey(
                        name: "FK_HoSoKyLuat_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_HoSoKyLuat_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_dot_khen_thuong = table.Column<int>(type: "int", nullable: true),
                    ma_mau_bang_khen = table.Column<int>(type: "int", nullable: true),
                    loai_khen_thuong = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gpa_dat_duoc = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_xet = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    xep_hang = table.Column<int>(type: "int", nullable: true),
                    url_chung_tu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_pdf_bang_khen = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_sinh_pdf = table.Column<DateTime>(type: "datetime", nullable: true),
                    loi_sinh_pdf = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_lan_sinh_pdf = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ho_ten_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mssv_snapshot = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_hoc_ky_snapshot = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    danh_hieu_snapshot = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cap_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_cap = table.Column<int>(type: "int", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    da_huy = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ly_do_huy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_huy = table.Column<int>(type: "int", nullable: true),
                    ngay_huy = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu_huy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_cap = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu_vong_doi = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhenThuong", x => x.ma_khen_thuong);
                    table.CheckConstraint("CK_KhenThuong_diem_xet", "`diem_xet` IS NULL OR `diem_xet` >= 0");
                    table.CheckConstraint("CK_KhenThuong_gpa_dat_duoc_2", "`gpa_dat_duoc` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_KhenThuong_loai_khen_thuong_1", "`loai_khen_thuong` IN ('hoc_luc', 'dac_biet', 'thi_dau', 'TOP_100_HOC_KY', 'KHAC')");
                    table.CheckConstraint("CK_KhenThuong_so_lan_sinh_pdf", "`so_lan_sinh_pdf` >= 0");
                    table.CheckConstraint("CK_KhenThuong_trang_thai", "`trang_thai` IN ('nhap', 'cho_duyet', 'da_duyet', 'da_cap', 'da_sinh_pdf', 'loi_sinh_pdf', 'da_huy')");
                    table.CheckConstraint("CK_KhenThuong_xep_hang", "`xep_hang` IS NULL OR `xep_hang` > 0");
                    table.ForeignKey(
                        name: "FK_KhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_KhenThuong_ma_dot_khen_thuong__DotKhenThuong",
                        column: x => x.ma_dot_khen_thuong,
                        principalSchema: "dbo",
                        principalTable: "DotKhenThuong",
                        principalColumn: "ma_dot_khen_thuong");
                    table.ForeignKey(
                        name: "FK_KhenThuong_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KhieuNaiKyLuat",
                schema: "dbo",
                columns: table => new
                {
                    ma_khieu_nai_ky_luat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ho_so_ky_luat = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ly_do_khieu_nai = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chung_tu_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_xu_ly = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_xu_ly = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_xu_ly = table.Column<int>(type: "int", nullable: true),
                    ngay_xu_ly = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhieuNaiKyLuat", x => x.ma_khieu_nai_ky_luat);
                    table.CheckConstraint("CK_KhieuNaiKyLuat_chung_tu_json_ISJSON", "`chung_tu_json` IS NULL OR JSON_VALID(`chung_tu_json`) = 1");
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_ma_ho_so_ky_luat__HoSoKyLuat",
                        column: x => x.ma_ho_so_ky_luat,
                        principalSchema: "dbo",
                        principalTable: "HoSoKyLuat",
                        principalColumn: "ma_ky_luat",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: true),
                    ma_block_bat_dau = table.Column<int>(type: "int", nullable: true),
                    ma_lop = table.Column<int>(type: "int", nullable: false),
                    SoBlockHoc = table.Column<int>(type: "int", nullable: false),
                    ma_lop_hoc_phan = table.Column<int>(type: "int", nullable: true),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_anh_bia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHoc", x => x.ma_khoa_hoc);
                    table.CheckConstraint("CK_KhoaHoc_trang_thai_1", "`trang_thai` IN ('nhap', 'da_xuat_ban', 'luu_tru')");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_ma_block_bat_dau__Block",
                        column: x => x.ma_block_bat_dau,
                        principalSchema: "dbo",
                        principalTable: "Block",
                        principalColumn: "ma_block");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThoiKhoaBieu",
                schema: "dbo",
                columns: table => new
                {
                    ma_tkb = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_phong = table.Column<int>(type: "int", nullable: false),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: false),
                    thu_trong_tuan = table.Column<int>(type: "int", nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThoiKhoaBieu", x => x.ma_tkb);
                    table.CheckConstraint("CK_ThoiKhoaBieu_ngay", "`ngay_ket_thuc` IS NULL OR `ngay_bat_dau` IS NULL OR `ngay_ket_thuc` >= `ngay_bat_dau`");
                    table.CheckConstraint("CK_ThoiKhoaBieu_thu_trong_tuan", "`thu_trong_tuan` BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_ThoiKhoaBieu_trang_thai", "`trang_thai` IN ('nhap', 'da_xuat_ban', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LienKetPhuHuynh",
                schema: "dbo",
                columns: table => new
                {
                    ma_lien_ket_ph = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phu_huynh = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    quyen_xem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lien_ket_luc = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LienKetPhuHuynh", x => x.ma_lien_ket_ph);
                    table.CheckConstraint("CK_LienKetPhuHuynh_quyen_xem_ISJSON", "`quyen_xem` IS NULL OR JSON_VALID(`quyen_xem`) = 1");
                    table.CheckConstraint("CK_LienKetPhuHuynh_trang_thai_1", "`trang_thai` IN ('cho_duyet', 'hoat_dong', 'da_thu_hoi')");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LopHanhChinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_lop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_lop = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_lop = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_giao_vien_chu_nhiem = table.Column<int>(type: "int", nullable: true),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: true),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: true),
                    si_so_du_kien = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ho_ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vai_tro_chinh = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_lop = table.Column<int>(type: "int", nullable: true),
                    so_dien_thoai = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "dang_nhap_lan_dau")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nam_nhap_hoc = table.Column<int>(type: "int", nullable: true),
                    mat_khau_hash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    lan_dang_nhap_cuoi = table.Column<DateTime>(type: "datetime", nullable: true),
                    so_lan_sai_mat_khau = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    dang_nhap_lan_dau = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.ma_nguoi_dung);
                    table.CheckConstraint("CK_NguoiDung_trang_thai_2", "`trang_thai` IN ('hoat_dong', 'bi_khoa', 'dang_nhap_lan_dau')");
                    table.CheckConstraint("CK_NguoiDung_vai_tro_chinh_1", "`vai_tro_chinh` IN ('quan_tri', 'giao_vien', 'hoc_sinh', 'nhan_vien', 'hieu_truong', 'phu_huynh', 'sieu_quan_tri', 'quan_tri_co_so', 'quan_tri_co_so_con', 'chu_tich', 'hoidong_quanly_noidung', 'admin_tai_chinh', 'ke_toan_co_so', 'ke_toan_truong_co_so')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MauBangKhen",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_bang_khen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten_mau = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_mau = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_nen_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chieu_rong = table.Column<int>(type: "int", nullable: false),
                    chieu_cao = table.Column<int>(type: "int", nullable: false),
                    huong_giay = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cau_hinh_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauBangKhen", x => x.ma_mau_bang_khen);
                    table.CheckConstraint("CK_MauBangKhen_cau_hinh_json", "`cau_hinh_json` IS NULL OR JSON_VALID(`cau_hinh_json`) = 1");
                    table.CheckConstraint("CK_MauBangKhen_huong_giay", "`huong_giay` IN ('A4_NGANG', 'A4_DOC')");
                    table.CheckConstraint("CK_MauBangKhen_kich_thuoc", "`chieu_rong` > 0 AND `chieu_cao` > 0");
                    table.CheckConstraint("CK_MauBangKhen_loai_mau", "`loai_mau` IN ('TOP_100_HOC_KY')");
                    table.ForeignKey(
                        name: "FK_MauBangKhen_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MauThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_tb = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loai_su_kien = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kenh_gui = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mau_tieu_de = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mau_noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ten_mau = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_mau = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_thong_bao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_do_uu_tien = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_mac_dinh = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bien_cho_phep_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dang_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    la_he_thong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauThongBao", x => x.ma_mau_tb);
                    table.CheckConstraint("CK_MauThongBao_kenh_gui_1", "`kenh_gui` IN ('email', 'thong_bao_day', 'sms', 'in_app')");
                    table.ForeignKey(
                        name: "FK_MauThongBao_DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_MauThongBao_NguoiCapNhat",
                        column: x => x.nguoi_cap_nhat,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_MauThongBao_NguoiTao",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NhatKyDuyetDon",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_duyet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_tu = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    nguon_thuc_hien = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hanh_dong = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_cu = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_moi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_cong_khai = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_noi_bo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    snapshot_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hien_thi_cho_hoc_sinh = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyDuyetDon", x => x.ma_nk_duyet);
                    table.CheckConstraint("CK_NhatKyDuyetDon_hanh_dong_1", "`hanh_dong` IN ('tao_nhap', 'cap_nhat', 'nop', 'nop_lai', 'phan_cong', 'phan_cong_lai', 'tiep_nhan', 'yeu_cau_bo_sung', 'bo_sung', 'phe_duyet', 'tu_choi', 'leo_thang', 'huy', 'xu_ly_nghiep_vu')");
                    table.CheckConstraint("CK_NhatKyDuyetDon_nguon_thuc_hien", "`nguon_thuc_hien` IN ('user', 'system')");
                    table.CheckConstraint("CK_NhatKyDuyetDon_snapshot_json_ISJSON", "`snapshot_json` IS NULL OR JSON_VALID(`snapshot_json`) = 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NhatKyKiemToan",
                schema: "dbo",
                columns: table => new
                {
                    ma_kiem_toan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    loai_doi_tuong = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_doi_tuong = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hanh_dong = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gia_tri_cu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gia_tri_moi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_thay_doi = table.Column<int>(type: "int", nullable: true),
                    thoi_diem_thay_doi = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    dia_chi_ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trace_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyKiemToan", x => x.ma_kiem_toan);
                    table.CheckConstraint("CK_NhatKyKiemToan_gia_tri_cu_ISJSON", "`gia_tri_cu` IS NULL OR JSON_VALID(`gia_tri_cu`) = 1");
                    table.CheckConstraint("CK_NhatKyKiemToan_gia_tri_moi_ISJSON", "`gia_tri_moi` IS NULL OR JSON_VALID(`gia_tri_moi`) = 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NhatKyThayDoiDiem",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_thay_doi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_diem_so = table.Column<int>(type: "int", nullable: false),
                    nguoi_thay_doi = table.Column<int>(type: "int", nullable: false),
                    gia_tri_cu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gia_tri_moi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    thay_doi_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyThayDoiDiem", x => x.ma_nk_thay_doi);
                    table.CheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_cu_ISJSON", "`gia_tri_cu` IS NULL OR JSON_VALID(`gia_tri_cu`) = 1");
                    table.CheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_moi_ISJSON", "`gia_tri_moi` IS NULL OR JSON_VALID(`gia_tri_moi`) = 1");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NopBaiDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_nop_dg = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    so_lan_nop = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    so_lan_sua = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NopBaiDanhGia", x => x.ma_nop_dg);
                    table.CheckConstraint("CK_NopBaiDanhGia_so_lan_nop_1", "`so_lan_nop` BETWEEN 0 AND 2");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhanCongGiamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_phan_cong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_giam_thi = table.Column<int>(type: "int", nullable: false),
                    vai_tro_giam_thi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "giam_thi_phu")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "du_kien")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_thay_doi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiamThi", x => x.ma_phan_cong);
                    table.CheckConstraint("CK_PhanCongGiamThi_trang_thai", "`trang_thai` IN ('du_kien', 'da_xac_nhan', 'thay_the', 'huy_phan_cong')");
                    table.CheckConstraint("CK_PhanCongGiamThi_vai_tro", "`vai_tro_giam_thi` IN ('giam_thi_chinh', 'giam_thi_phu', 'ho_tro_ky_thuat')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhanQuyenNguoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ngay_gan = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhienHocNoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_phien_hoc = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    session_token = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    nhip_tim_cuoi_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ket_thuc_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    so_giay_hoat_dong_da_xac_nhan = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    vi_tri_video_cuoi_giay = table.Column<int>(type: "int", nullable: true),
                    phan_tram_cuon_lon_nhat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    so_thu_tu_nhip_tim_cuoi = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "dang_hoat_dong")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_chi_ip_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienHocNoiDung", x => x.ma_phien_hoc);
                    table.CheckConstraint("CK_PhienHocNoiDung_TrangThai", "`trang_thai` IN ('dang_hoat_dong', 'da_ket_thuc', 'het_ha', 'bi_thay_the')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhienThiHocSinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_phien_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_de_kiem_tra = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    nop_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    cau_tra_loi_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nhat_ky_vi_pham = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sao_luu_cuc_bo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_luong = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "dang_hoat_dong")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    diem_tu_dong = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_cuoi_cung = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    diem_tu_luan_ai_goi_y = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    lan_thu = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    han_nop_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    so_cau_dung = table.Column<int>(type: "int", nullable: true),
                    ket_qua_dat = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    de_thi_snapshot_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: true),
                    trang_thai_ky_ten = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem_ky = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_xac_nhan_ky_ten = table.Column<int>(type: "int", nullable: true),
                    trang_thai_cong_bo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienThiHocSinh", x => x.ma_phien_thi);
                    table.CheckConstraint("CK_PhienThiHocSinh_cau_tra_loi_json_ISJSON", "`cau_tra_loi_json` IS NULL OR JSON_VALID(`cau_tra_loi_json`) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_de_thi_snapshot_json_ISJSON", "`de_thi_snapshot_json` IS NULL OR JSON_VALID(`de_thi_snapshot_json`) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_cuoi_cung", "`diem_cuoi_cung` IS NULL OR `diem_cuoi_cung` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_tu_dong", "`diem_tu_dong` IS NULL OR `diem_tu_dong` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y", "`diem_tu_luan_ai_goi_y` IS NULL OR `diem_tu_luan_ai_goi_y` BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_PhienThiHocSinh_lan_thu", "`lan_thu` > 0");
                    table.CheckConstraint("CK_PhienThiHocSinh_nhat_ky_vi_pham_ISJSON", "`nhat_ky_vi_pham` IS NULL OR JSON_VALID(`nhat_ky_vi_pham`) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_sao_luu_cuc_bo_ISJSON", "`sao_luu_cuc_bo` IS NULL OR JSON_VALID(`sao_luu_cuc_bo`) = 1");
                    table.CheckConstraint("CK_PhienThiHocSinh_so_cau_dung", "`so_cau_dung` IS NULL OR `so_cau_dung` >= 0");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_cong_bo", "`trang_thai_cong_bo` IS NULL OR `trang_thai_cong_bo` IN ('chua_co_diem', 'da_cham_xong', 'da_doc_diem', 'da_cong_bo')");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_ky_ten", "`trang_thai_ky_ten` IS NULL OR `trang_thai_ky_ten` IN ('chua_ky', 'da_ky', 'quen_ky', 'su_co')");
                    table.CheckConstraint("CK_PhienThiHocSinh_trang_thai_luong_1", "`trang_thai_luong` IN ('dang_hoat_dong', 'bi_gian_doan', 'da_dung')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhieuHoTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_phieu_ht = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    danh_muc = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "mo")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phan_cong_cho = table.Column<int>(type: "int", nullable: true),
                    han_xu_ly = table.Column<DateTime>(type: "datetime", nullable: true),
                    danh_gia_hai_long = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    do_uu_tien = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "medium")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlDinhKem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuHoTro", x => x.ma_phieu_ht);
                    table.CheckConstraint("CK_PhieuHoTro_danh_gia_hai_long_3", "`danh_gia_hai_long` BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_PhieuHoTro_danh_muc_1", "`danh_muc` IN ('ky_thuat', 'hoc_vu', 'tai_chinh', 'khac')");
                    table.CheckConstraint("CK_PhieuHoTro_trang_thai_2", "`trang_thai` IN ('mo', 'dang_xu_ly', 'da_giai_quyet', 'da_dong')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuyDinhChuyenCan",
                schema: "dbo",
                columns: table => new
                {
                    ma_quy_dinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ngay_hieu_luc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    quy_vang_toi_da = table.Column<int>(type: "int", nullable: false),
                    ti_le_canh_bao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_vang_khong_phep = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_vang_co_phep = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    he_so_di_muon = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    han_gui_phut = table.Column<int>(type: "int", nullable: false),
                    han_chinh_sua_phut = table.Column<int>(type: "int", nullable: false),
                    ghi_chu = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    tao_luc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduleGenerationJob",
                schema: "dbo",
                columns: table => new
                {
                    ma_job = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    draft_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "draft")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tong_course = table.Column<int>(type: "int", nullable: true),
                    so_xep_duoc = table.Column<int>(type: "int", nullable: true),
                    so_khong_xep_duoc = table.Column<int>(type: "int", nullable: true),
                    score = table.Column<float>(type: "float", nullable: true),
                    tom_tat_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_xuat_ban = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleGenerationJob", x => x.ma_job);
                    table.CheckConstraint("CK_ScheduleGenerationJob_trang_thai", "`trang_thai` IN ('draft', 'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_nguoi_yeu_cau__NguoiDung",
                        column: x => x.nguoi_yeu_cau,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaiKhoanNhanTien",
                schema: "dbo",
                columns: table => new
                {
                    ma_tai_khoan_nhan_tien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ten_ngan_hang = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_ngan_hang = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tai_khoan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_chu_tai_khoan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chi_nhanh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nha_cung_cap_thanh_toan = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "payos")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_duyet = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cau_hinh_provider_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    la_mac_dinh = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    con_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime", nullable: true),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNhanTien", x => x.ma_tai_khoan_nhan_tien);
                    table.CheckConstraint("CK_TaiKhoanNhanTien_cau_hinh_provider_json", "`cau_hinh_provider_json` IS NULL OR JSON_VALID(`cau_hinh_provider_json`) = 1");
                    table.CheckConstraint("CK_TaiKhoanNhanTien_provider", "`nha_cung_cap_thanh_toan` IN ('payos', 'vietqr')");
                    table.CheckConstraint("CK_TaiKhoanNhanTien_trang_thai_duyet", "`trang_thai_duyet` IN ('nhap', 'cho_duyet', 'da_duyet', 'tu_choi', 'ngung_hoat_dong')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TepDinhKemDonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_tep = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_tu = table.Column<int>(type: "int", nullable: false),
                    storage_key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_file_goc = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_file_luu = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: false),
                    file_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tai_len = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    da_xoa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    nguoi_xoa = table.Column<int>(type: "int", nullable: true),
                    ngay_xoa = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKemDonTu", x => x.ma_tep);
                    table.CheckConstraint("CK_TepDinhKemDonTu_kich_thuoc", "`kich_thuoc_byte` > 0");
                    table.CheckConstraint("CK_TepDinhKemDonTu_soft_delete", "(`da_xoa` = 0 AND `nguoi_xoa` IS NULL AND `ngay_xoa` IS NULL) OR (`da_xoa` = 1 AND `nguoi_xoa` IS NOT NULL AND `ngay_xoa` IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_TepDinhKemDonTu_ma_don_tu__DonTu",
                        column: x => x.ma_don_tu,
                        principalSchema: "dbo",
                        principalTable: "DonTu",
                        principalColumn: "ma_don_tu");
                    table.ForeignKey(
                        name: "FK_TepDinhKemDonTu_nguoi_tai_len__NguoiDung",
                        column: x => x.nguoi_tai_len,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_TepDinhKemDonTu_nguoi_xoa__NguoiDung",
                        column: x => x.nguoi_xoa,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThiSinhCaThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_thi_sinh_ca_thi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    trang_thai_du_thi = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_thi")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThiSinhCaThi", x => x.ma_thi_sinh_ca_thi);
                    table.CheckConstraint("CK_ThiSinhCaThi_trang_thai_du_thi", "`trang_thai_du_thi` IN ('cho_thi', 'duoc_thi', 'khong_duoc_thi', 'dinh_chi', 'vang_thi')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_thong_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_nhom_thong_bao = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "UUID()", collation: "ascii_general_ci"),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_su_kien = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_thong_bao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "manual")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_de = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tom_tat = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tom_tat_noi_dung = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung_text = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_do = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "info")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_lien_ket = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_doi_tuong_lien_ket = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_doi_tuong_lien_ket = table.Column<int>(type: "int", nullable: true),
                    pham_vi_gui = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "nguoi_dung")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duong_dan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "da_gui")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    da_doc = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    doc_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    gui_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.ma_thong_bao);
                    table.CheckConstraint("CK_ThongBao_loai_thong_bao", "`loai_thong_bao` IN ('thong_bao_chung', 'hoc_phi', 'bao_tri', 'co_so_vat_chat', 'hoc_vu', 'khan_cap', 'system', 'manual', 'schedule_changed', 'session_cancelled', 'attendance_unlock_approved', 'attendance_unlock_rejected')");
                    table.CheckConstraint("CK_ThongBao_muc_do", "`muc_do` IN ('thong_tin', 'quan_trong', 'khan_cap', 'info', 'warning', 'important')");
                    table.CheckConstraint("CK_ThongBao_noi_dung_json_ISJSON", "`noi_dung_json` IS NULL OR JSON_VALID(`noi_dung_json`) = 1");
                    table.CheckConstraint("CK_ThongBao_pham_vi_gui", "`pham_vi_gui` IN ('toan_he_thong', 'don_vi', 'lop_hanh_chinh', 'vai_tro', 'nguoi_dung', 'khoa_hoc', 'users', 'class', 'course', 'campus')");
                    table.CheckConstraint("CK_ThongBao_trang_thai", "`trang_thai` IN ('nhap', 'da_gui', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThongBaoHenGio",
                schema: "dbo",
                columns: table => new
                {
                    ma_tb_hen_gio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    loai_su_kien = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bo_loc_nguoi_nhan = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gui_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "da_len_lich")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoHenGio", x => x.ma_tb_hen_gio);
                    table.CheckConstraint("CK_ThongBaoHenGio_bo_loc_nguoi_nhan_ISJSON", "`bo_loc_nguoi_nhan` IS NULL OR JSON_VALID(`bo_loc_nguoi_nhan`) = 1");
                    table.CheckConstraint("CK_ThongBaoHenGio_trang_thai_1", "`trang_thai` IN ('da_len_lich', 'dang_cho', 'da_huy', 'hoan_thanh')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TienDoBaiHoc",
                schema: "dbo",
                columns: table => new
                {
                    ma_tien_do = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    phan_tram_tien_do = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    lan_gui_nhip_tim_cuoi = table.Column<DateTime>(type: "datetime", nullable: true),
                    hoan_thanh_luc = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienDoBaiHoc", x => x.ma_tien_do);
                    table.CheckConstraint("CK_TienDoBaiHoc_phan_tram_tien_do_1", "`phan_tram_tien_do` BETWEEN 0 AND 100");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TienDoNoiDungHocTap",
                schema: "dbo",
                columns: table => new
                {
                    ma_tien_do_noi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false),
                    loai_noi_dung = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "chua_bat_dau")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phan_tram_tien_do = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    so_giay_da_xac_nhan = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    vi_tri_video_cuoi_giay = table.Column<int>(type: "int", nullable: true),
                    phan_tram_cuon_lon_nhat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    chi_so_muc_cuoi = table.Column<int>(type: "int", nullable: true),
                    so_muc_da_xem = table.Column<int>(type: "int", nullable: true),
                    tong_so_muc = table.Column<int>(type: "int", nullable: true),
                    bat_dau_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    lan_tuong_tac_cuoi = table.Column<DateTime>(type: "datetime", nullable: true),
                    hoan_thanh_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    chi_tiet_tien_do_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienDoNoiDungHocTap", x => x.ma_tien_do_noi_dung);
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_PhanTramCuonLonNhat", "`phan_tram_cuon_lon_nhat` IS NULL OR `phan_tram_cuon_lon_nhat` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_PhanTramTienDo", "`phan_tram_tien_do` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_SoGiayDaXacNhan", "`so_giay_da_xac_nhan` >= 0");
                    table.CheckConstraint("CK_TienDoNoiDungHocTap_TrangThai", "`trang_thai` IN ('chua_bat_dau', 'dang_hoc', 'hoan_thanh')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TokenLamMoi",
                schema: "dbo",
                columns: table => new
                {
                    ma_token_lam_moi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    het_han_luc = table.Column<DateTime>(type: "datetime", nullable: false),
                    thu_hoi_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TuyChonThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "int", nullable: false),
                    nhan_email = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    nhan_push = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    nhan_sms = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    cap_nhat_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UngVienKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_ung_vien_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_dot_khen_thuong = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    xep_hang = table.Column<int>(type: "int", nullable: true),
                    diem_xet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gpa_hoc_ky = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tong_tin_chi = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_loai = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_loai_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_chi_snapshot_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ho_ten_snapshot = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mssv_snapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_hoc_ky_snapshot = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu_dieu_chinh = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_dieu_chinh = table.Column<int>(type: "int", nullable: true),
                    ngay_dieu_chinh = table.Column<DateTime>(type: "datetime", nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UngVienKhenThuong", x => x.ma_ung_vien_khen_thuong);
                    table.CheckConstraint("CK_UngVienKhenThuong_diem_xet", "`diem_xet` >= 0");
                    table.CheckConstraint("CK_UngVienKhenThuong_ly_do_loai_json_ISJSON", "`ly_do_loai_json` IS NULL OR JSON_VALID(`ly_do_loai_json`) = 1");
                    table.CheckConstraint("CK_UngVienKhenThuong_tieu_chi_snapshot_json_ISJSON", "`tieu_chi_snapshot_json` IS NULL OR JSON_VALID(`tieu_chi_snapshot_json`) = 1");
                    table.CheckConstraint("CK_UngVienKhenThuong_xep_hang", "`xep_hang` IS NULL OR `xep_hang` > 0");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_dot_khen_thuong__DotKhenThuong",
                        column: x => x.ma_dot_khen_thuong,
                        principalSchema: "dbo",
                        principalTable: "DotKhenThuong",
                        principalColumn: "ma_dot_khen_thuong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_nguoi_dieu_chinh__NguoiDung",
                        column: x => x.nguoi_dieu_chinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VaiTroQuyenHan",
                schema: "dbo",
                columns: table => new
                {
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ma_quyen_han = table.Column<int>(type: "int", nullable: false),
                    ngay_cap = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    nguoi_cap = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTroQuyenHan", x => new { x.ma_vai_tro, x.ma_quyen_han });
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_NguoiCap",
                        column: x => x.nguoi_cap,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_QuyenHan",
                        column: x => x.ma_quyen_han,
                        principalSchema: "dbo",
                        principalTable: "QuyenHan",
                        principalColumn: "ma_quyen_han",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_VaiTro",
                        column: x => x.ma_vai_tro,
                        principalSchema: "dbo",
                        principalTable: "VaiTro",
                        principalColumn: "ma_vai_tro",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "XuatBaoCao",
                schema: "dbo",
                columns: table => new
                {
                    ma_xuat_bao_cao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    loai_bao_cao = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tham_so_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_tap_tin = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_xu_ly")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatBaoCao", x => x.ma_xuat_bao_cao);
                    table.CheckConstraint("CK_XuatBaoCao_tham_so_json_ISJSON", "`tham_so_json` IS NULL OR JSON_VALID(`tham_so_json`) = 1");
                    table.CheckConstraint("CK_XuatBaoCao_trang_thai_1", "`trang_thai` IN ('cho_xu_ly', 'dang_xu_ly', 'hoan_thanh', 'that_bai')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YeuCauDoiLich",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_doi_lich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_tkb = table.Column<int>(type: "int", nullable: false),
                    giao_vien_de_xuat = table.Column<int>(type: "int", nullable: false),
                    giao_vien_nhan_doi = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "cho_gv_nhan_dong_y")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    gv_nhan_phan_hoi_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    admin_duyet_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauDoiLich", x => x.ma_yc_doi_lich);
                    table.CheckConstraint("CK_YeuCauDoiLich_trang_thai_1", "`trang_thai` IN ('cho_gv_nhan_dong_y', 'cho_admin_duyet', 'da_hoan_doi', 'tu_choi', 'da_huy')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YeuCauHoanPhi",
                schema: "dbo",
                columns: table => new
                {
                    ma_hoan_phi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_hoa_don = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    so_tien_yeu_cau = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    loai_hoan_phi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_yeu_cau = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    nguoi_cap_nhat = table.Column<int>(type: "int", nullable: true),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime", nullable: true),
                    xu_ly_luc = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoanPhi", x => x.ma_hoan_phi);
                    table.CheckConstraint("CK_YeuCauHoanPhi_loai_hoan_phi_2", "`loai_hoan_phi` IN ('toan_phan', 'mot_phan', 'ghi_co')");
                    table.CheckConstraint("CK_YeuCauHoanPhi_so_tien_yeu_cau_1", "`so_tien_yeu_cau` >= 0");
                    table.CheckConstraint("CK_YeuCauHoanPhi_trang_thai_3", "`trang_thai` IN ('cho_duyet', 'da_duyet', 'tu_choi', 'da_xu_ly')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YeuCauMoKhoaDiemDanh",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_mo_khoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_buoi_hoc = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    mo_khoa_den_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_tu_choi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_gian_xu_ly = table.Column<DateTime>(type: "datetime", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauMoKhoaDiemDanh", x => x.ma_yc_mo_khoa);
                    table.CheckConstraint("CK_YeuCauMoKhoaDiemDanh_trang_thai_1", "`trang_thai` IN ('cho_duyet', 'da_duyet', 'tu_choi', 'het_han')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YeuCauSuaDiem",
                schema: "dbo",
                columns: table => new
                {
                    ma_yc_sua_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_diem_so = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    ly_do = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_bang_chung = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "cho_duyet")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    mo_den_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    loai_yeu_cau = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "sua_sau_submit")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    unlock_expires_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    cot_diem_duoc_mo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauSuaDiem", x => x.ma_yc_sua_diem);
                    table.CheckConstraint("CK_YeuCauSuaDiem_trang_thai_1", "`trang_thai` IN ('cho_duyet', 'da_duyet', 'tu_choi', 'het_han')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NhatKyViPhamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_vi_pham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phien_thi = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_ca_thi = table.Column<int>(type: "int", nullable: false),
                    loai_vi_pham = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_do = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "nhac_nho")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chi_tiet_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem = table.Column<DateTime>(type: "datetime", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyViPhamThi", x => x.ma_vi_pham);
                    table.CheckConstraint("CK_NhatKyViPhamThi_chi_tiet_json_ISJSON", "`chi_tiet_json` IS NULL OR JSON_VALID(`chi_tiet_json`) = 1");
                    table.CheckConstraint("CK_NhatKyViPhamThi_loai_vi_pham", "`loai_vi_pham` IN ('chuyen_tab', 'mat_focus', 'mat_camera', 'tieng_on', 'khac')");
                    table.CheckConstraint("CK_NhatKyViPhamThi_muc_do", "`muc_do` IN ('nhac_nho', 'nghiem_trong')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TinNhanHoTro",
                schema: "dbo",
                columns: table => new
                {
                    ma_tin_nhan_ht = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_phieu_ht = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_gui = table.Column<int>(type: "int", nullable: false),
                    noi_dung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url_dinh_kem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduleDraftItem",
                schema: "dbo",
                columns: table => new
                {
                    ma_draft_item = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_job = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    thu_trong_tuan = table.Column<int>(type: "int", nullable: true),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_phong = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<float>(type: "float", nullable: true),
                    canh_bao_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loi_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScoreBreakdownJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LyDoGoiYJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDraftItem", x => x.ma_draft_item);
                    table.CheckConstraint("CK_ScheduleDraftItem_thu_trong_tuan", "`thu_trong_tuan` IS NULL OR `thu_trong_tuan` BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_ScheduleDraftItem_trang_thai", "`trang_thai` IN ('pending', 'xep_duoc', 'khong_xep_duoc')");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_ca_hoc__CaHoc",
                        column: x => x.ma_ca_hoc,
                        principalSchema: "dbo",
                        principalTable: "CaHoc",
                        principalColumn: "ma_ca_hoc");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_job__ScheduleGenerationJob",
                        column: x => x.ma_job,
                        principalSchema: "dbo",
                        principalTable: "ScheduleGenerationJob",
                        principalColumn: "ma_job",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_khoa_hoc__KhoaHoc",
                        column: x => x.ma_khoa_hoc,
                        principalSchema: "dbo",
                        principalTable: "KhoaHoc",
                        principalColumn: "ma_khoa_hoc");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NhatKyThongBao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nk_thong_bao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_thong_bao = table.Column<int>(type: "int", nullable: true),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kenh_gui = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gui_luc = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyThongBao", x => x.ma_nk_thong_bao);
                    table.CheckConstraint("CK_NhatKyThongBao_kenh_gui_2", "`kenh_gui` IN ('email', 'thong_bao_day', 'sms')");
                    table.CheckConstraint("CK_NhatKyThongBao_trang_thai_1", "`trang_thai` IN ('cho_gui', 'da_gui', 'da_nhan', 'that_bai', 'bo_qua')");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThongBaoNguoiNhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_thong_bao_nguoi_nhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_thong_bao = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    da_doc = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    doc_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    da_an = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    an_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    nhan_luc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoNguoiNhan", x => x.ma_thong_bao_nguoi_nhan);
                    table.ForeignKey(
                        name: "FK_ThongBaoNguoiNhan_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_ThongBaoNguoiNhan_ma_nguoi_nhan__NguoiDung",
                        column: x => x.ma_nguoi_nhan,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_ThongBaoNguoiNhan_ma_thong_bao__ThongBao",
                        column: x => x.ma_thong_bao,
                        principalSchema: "dbo",
                        principalTable: "ThongBao",
                        principalColumn: "ma_thong_bao",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "XuLyViPhamThi",
                schema: "dbo",
                columns: table => new
                {
                    ma_xu_ly = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ma_vi_pham = table.Column<int>(type: "int", nullable: false),
                    hanh_dong_xu_ly = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lan_nhac_nho = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_xu_ly = table.Column<int>(type: "int", nullable: false),
                    thoi_diem = table.Column<DateTime>(type: "datetime", nullable: false),
                    ly_do = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuLyViPhamThi", x => x.ma_xu_ly);
                    table.CheckConstraint("CK_XuLyViPhamThi_hanh_dong", "`hanh_dong_xu_ly` IN ('nhac_nho_he_thong', 'canh_bao_truc_tiep', 'dinh_chi_thi', 'bo_qua')");
                    table.CheckConstraint("CK_XuLyViPhamThi_lan_nhac_nho", "`lan_nhac_nho` >= 0");
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "LoaiDauDiemQuaTrinh",
                columns: new[] { "ma_loai_dau_diem", "ma_code", "ten_loai", "thu_tu_hien_thi" },
                values: new object[,]
                {
                    { 1, "chuyen_can", "Chuyên cần", 1 },
                    { 2, "quiz", "Quiz", 2 },
                    { 3, "lab", "Lab", 3 },
                    { 4, "progress_test", "Progress Test", 4 },
                    { 5, "assignment", "Assignment", 5 }
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
                name: "IX_AnhChupPhanTich_ma_hoc_ky",
                schema: "dbo",
                table: "AnhChupPhanTich",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_AnhChupPhanTich_1",
                schema: "dbo",
                table: "AnhChupPhanTich",
                columns: new[] { "ma_don_vi", "ma_hoc_ky", "ngay_anh_chup", "loai_chi_so" },
                unique: true);

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
                name: "IX_BaiTap_MaCauHinhDauDiem",
                schema: "dbo",
                table: "BaiTap",
                column: "MaCauHinhDauDiem");

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
                name: "IX_Block_ma_hoc_ky",
                schema: "dbo",
                table: "Block",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_BuocQuyTrinh_MaQuyTrinh",
                table: "BuocQuyTrinh",
                column: "MaQuyTrinh");

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
                name: "IX_CauHinhDauDiemQuaTrinh_ma_hoc_ky",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDauDiemQuaTrinh_ma_loai_dau_diem",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_loai_dau_diem");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDauDiemQuaTrinh_ma_mon_hoc",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_mon_hoc");

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
                name: "IX_CauHinhDiemMonHoc_nguoi_cap_nhat",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "nguoi_cap_nhat");

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
                filter: "`con_hoat_dong` = 1");

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
                name: "IX_DanhMucMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucMonHoc_ma_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_nganh");

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
                unique: true);

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
                name: "IX_DonTu_han_xu_ly_trang_thai",
                schema: "dbo",
                table: "DonTu",
                columns: new[] { "han_xu_ly_luc", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_loai_don_trang_thai",
                schema: "dbo",
                table: "DonTu",
                columns: new[] { "loai_don", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_ma_don_vi_trang_thai_ngay_nop",
                schema: "dbo",
                table: "DonTu",
                columns: new[] { "ma_don_vi", "trang_thai", "ngay_nop" });

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_ma_hoc_sinh_ngay_tao",
                schema: "dbo",
                table: "DonTu",
                columns: new[] { "ma_hoc_sinh", "ngay_tao" });

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_ma_mau_don",
                schema: "dbo",
                table: "DonTu",
                column: "ma_mau_don");

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_nguoi_duyet_trang_thai_ngay_nop",
                schema: "dbo",
                table: "DonTu",
                columns: new[] { "nguoi_duyet_hien_tai", "trang_thai", "ngay_nop" });

            migrationBuilder.CreateIndex(
                name: "IX_DonTu_nguoi_xu_ly_cuoi",
                schema: "dbo",
                table: "DonTu",
                column: "nguoi_xu_ly_cuoi");

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
                name: "IX_DotKhenThuong_hoc_ky_don_vi_loai_trang_thai",
                schema: "dbo",
                table: "DotKhenThuong",
                columns: new[] { "ma_hoc_ky", "ma_don_vi", "loai_dot", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_DotKhenThuong_ma_don_vi",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_DotKhenThuong_ma_mau_bang_khen",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "ma_mau_bang_khen");

            migrationBuilder.CreateIndex(
                name: "IX_DotKhenThuong_nguoi_duyet",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_DotKhenThuong_nguoi_tao",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UX_DotKhenThuong_active_hoc_ky_don_vi_loai",
                schema: "dbo",
                table: "DotKhenThuong",
                columns: new[] { "ma_hoc_ky", "ma_don_vi", "loai_dot" },
                unique: true,
                filter: "`trang_thai` <> 'da_huy'");

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
                filter: "`ma_tham_chieu_cong` IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tham_chieu_noi_bo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienChuyenNganh_MaChuyenNganh",
                table: "GiaoVienChuyenNganh",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienChuyenNganh_MaGiaoVien",
                table: "GiaoVienChuyenNganh",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienMonHoc_MaGiaoVien",
                table: "GiaoVienMonHoc",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienMonHoc_MaMonHoc",
                table: "GiaoVienMonHoc",
                column: "ma_mon_hoc");

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
                filter: "`ma_hoc_ky` IS NOT NULL");

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
                name: "IX_HoSoKyLuat_don_vi_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat",
                columns: new[] { "ma_don_vi", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_hoc_ky_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat",
                columns: new[] { "ma_hoc_ky", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_hoc_sinh_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat",
                columns: new[] { "ma_hoc_sinh", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_ngay_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "ngay_vi_pham");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_ap_dung");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_duyet",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_go_ky_luat");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_huy",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_huy");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_tao",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_don_vi_trang_thai",
                schema: "dbo",
                table: "KhenThuong",
                columns: new[] { "ma_don_vi", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_dot_xep_hang",
                schema: "dbo",
                table: "KhenThuong",
                columns: new[] { "ma_dot_khen_thuong", "xep_hang" });

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_hoc_sinh_hoc_ky_loai",
                schema: "dbo",
                table: "KhenThuong",
                columns: new[] { "ma_hoc_sinh", "ma_hoc_ky", "loai_khen_thuong" });

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_ma_hoc_ky",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_ma_mau_bang_khen",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_mau_bang_khen");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_nguoi_cap",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_cap");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_nguoi_duyet",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_nguoi_huy",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_huy");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaDonVi",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaHocSinh",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaHoSoKyLuat",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_ho_so_ky_luat");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_nguoi_xu_ly",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "nguoi_xu_ly");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_TrangThai",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "trang_thai");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_block_bat_dau",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_block_bat_dau");

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
                name: "IX_KyThi_ma_nganh",
                schema: "dbo",
                table: "KyThi",
                column: "ma_nganh");

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
                name: "IX_MauBangKhen_loai_mau_con_hoat_dong",
                schema: "dbo",
                table: "MauBangKhen",
                columns: new[] { "loai_mau", "con_hoat_dong" });

            migrationBuilder.CreateIndex(
                name: "IX_MauBangKhen_nguoi_tao",
                schema: "dbo",
                table: "MauBangKhen",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "UX_MauDonTu_loai_don_active",
                schema: "dbo",
                table: "MauDonTu",
                column: "loai_don",
                unique: true,
                filter: "`dang_hoat_dong` = 1");

            migrationBuilder.CreateIndex(
                name: "UX_MauDonTu_loai_don_phien_ban",
                schema: "dbo",
                table: "MauDonTu",
                columns: new[] { "loai_don", "phien_ban" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_DangHoatDong",
                schema: "dbo",
                table: "MauThongBao",
                column: "dang_hoat_dong");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_LoaiThongBao",
                schema: "dbo",
                table: "MauThongBao",
                column: "loai_thong_bao");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_MaDonVi",
                schema: "dbo",
                table: "MauThongBao",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_MaMau",
                schema: "dbo",
                table: "MauThongBao",
                column: "ma_mau");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_nguoi_cap_nhat",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_nguoi_tao",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_tao");

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
                name: "IX_NhatKyDuyetDon_ma_don_tu_ngay_tao",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                columns: new[] { "ma_don_tu", "ngay_tao" });

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
                name: "UQ_PhienThiHocSinh_CaThi_HocSinh",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_ca_thi", "ma_hoc_sinh" },
                unique: true,
                filter: "`ma_ca_thi` IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_De_HocSinh_LanThu",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_de_kiem_tra", "ma_hoc_sinh", "lan_thu" },
                unique: true);

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
                name: "IX_QuyDinhChuyenCan_MaDonVi_NgayHieuLuc",
                schema: "dbo",
                table: "QuyDinhChuyenCan",
                columns: new[] { "ma_don_vi", "ngay_hieu_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuyDinhChuyenCan_nguoi_tao",
                schema: "dbo",
                table: "QuyDinhChuyenCan",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_QuyDoiTinChi_SoTinChi",
                schema: "dbo",
                table: "QuyDoiTinChi",
                column: "so_tin_chi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuyenHan_MaCode",
                schema: "dbo",
                table: "QuyenHan",
                column: "ma_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_Job_KhoaHoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                columns: new[] { "ma_job", "ma_khoa_hoc" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_ca_hoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_ca_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_khoa_hoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_phong",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_DonVi_HocKy",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                columns: new[] { "ma_don_vi", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_ma_hoc_ky",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_nguoi_yeu_cau",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "nguoi_yeu_cau");

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleGenerationJob_DraftId",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "draft_id",
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
                filter: "`la_mac_dinh` = 1 AND `con_hoat_dong` = 1");

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
                name: "IX_TepDinhKemDonTu_ma_don_tu_da_xoa",
                schema: "dbo",
                table: "TepDinhKemDonTu",
                columns: new[] { "ma_don_tu", "da_xoa" });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKemDonTu_nguoi_tai_len",
                schema: "dbo",
                table: "TepDinhKemDonTu",
                column: "nguoi_tai_len");

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKemDonTu_nguoi_xoa",
                schema: "dbo",
                table: "TepDinhKemDonTu",
                column: "nguoi_xoa");

            migrationBuilder.CreateIndex(
                name: "UX_TepDinhKemDonTu_storage_key",
                schema: "dbo",
                table: "TepDinhKemDonTu",
                column: "storage_key",
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
                filter: "`trang_thai` <> 'da_huy'");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_DonVi_Loai_GuiLuc",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_don_vi", "loai_thong_bao", "gui_luc" });

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
                name: "IX_ThongBaoNguoiNhan_DonVi_NhanLuc",
                schema: "dbo",
                table: "ThongBaoNguoiNhan",
                columns: new[] { "ma_don_vi", "nhan_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoNguoiNhan_MaThongBao",
                schema: "dbo",
                table: "ThongBaoNguoiNhan",
                column: "ma_thong_bao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoNguoiNhan_NguoiNhan_DaDoc_DaAn_NhanLuc",
                schema: "dbo",
                table: "ThongBaoNguoiNhan",
                columns: new[] { "ma_nguoi_nhan", "da_doc", "da_an", "nhan_luc" });

            migrationBuilder.CreateIndex(
                name: "UQ_ThongBaoNguoiNhan_ThongBao_NguoiNhan",
                schema: "dbo",
                table: "ThongBaoNguoiNhan",
                columns: new[] { "ma_thong_bao", "ma_nguoi_nhan" },
                unique: true);

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
                name: "IX_UngVienKhenThuong_CampaignRank",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_dot_khen_thuong", "trang_thai", "xep_hang" });

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_ma_hoc_ky",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_nguoi_dieu_chinh",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "nguoi_dieu_chinh");

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_nguoi_tao",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_OrgStatus",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_don_vi", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_StudentTerm",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_hoc_sinh", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "UQ_UngVienKhenThuong_CampaignStudent",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_dot_khen_thuong", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_VaiTro_1",
                schema: "dbo",
                table: "VaiTro",
                column: "ma_code_vai_tro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroQuyenHan_ma_quyen_han",
                schema: "dbo",
                table: "VaiTroQuyenHan",
                column: "ma_quyen_han");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroQuyenHan_nguoi_cap",
                schema: "dbo",
                table: "VaiTroQuyenHan",
                column: "nguoi_cap");

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
                filter: "`trang_thai` = 'cho_duyet'");

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
                name: "FK_CauHinhDiemMonHoc_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "nguoi_cap_nhat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

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
                name: "FK_DonTu_nguoi_xu_ly_cuoi__NguoiDung",
                schema: "dbo",
                table: "DonTu",
                column: "nguoi_xu_ly_cuoi",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DotKhenThuong_ma_mau_bang_khen__MauBangKhen",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "ma_mau_bang_khen",
                principalSchema: "dbo",
                principalTable: "MauBangKhen",
                principalColumn: "ma_mau_bang_khen");

            migrationBuilder.AddForeignKey(
                name: "FK_DotKhenThuong_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "nguoi_duyet",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_DotKhenThuong_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "DotKhenThuong",
                column: "nguoi_tao",
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
                name: "FK_GiaoVienChuyenNganh_ma_giao_vien__NguoiDung",
                table: "GiaoVienChuyenNganh",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoVienMonHoc_ma_giao_vien__NguoiDung",
                table: "GiaoVienMonHoc",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoVienNguyenVongCaDay_GiaoVienNguyenVongHocKy_NguyenVongId",
                table: "GiaoVienNguyenVongCaDay",
                column: "NguyenVongId",
                principalTable: "GiaoVienNguyenVongHocKy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoVienNguyenVongHocKy_NguoiDung_MaGiaoVien",
                table: "GiaoVienNguyenVongHocKy",
                column: "MaGiaoVien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_HoSoKyLuat_nguoi_ap_dung__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_ap_dung",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_duyet",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_go_ky_luat__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_go_ky_luat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_huy",
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
                name: "FK_KhenThuong_nguoi_cap__NguoiDung",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_cap",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_duyet",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "KhenThuong",
                column: "nguoi_huy",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_ma_mau_bang_khen__MauBangKhen",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_mau_bang_khen",
                principalSchema: "dbo",
                principalTable: "MauBangKhen",
                principalColumn: "ma_mau_bang_khen");

            migrationBuilder.AddForeignKey(
                name: "FK_KhieuNaiKyLuat_ma_hoc_sinh__NguoiDung",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_hoc_sinh",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_KhieuNaiKyLuat_nguoi_xu_ly__NguoiDung",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "nguoi_xu_ly",
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
                name: "BuocQuyTrinh");

            migrationBuilder.DropTable(
                name: "CanhBaoBaoMat",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CanhBaoDaoVan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHinhCanhBaoAi",
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
                name: "GiaoVienChuyenNganh");

            migrationBuilder.DropTable(
                name: "GiaoVienMonHoc");

            migrationBuilder.DropTable(
                name: "GiaoVienNguyenVongCaDay");

            migrationBuilder.DropTable(
                name: "KhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhieuNaiKyLuat",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LienKetPhuHuynh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MauDanhGia",
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
                name: "PhienHocNoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyDinhChuyenCan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyDoiTinChi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScheduleDraftItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TepDinhKemDonTu",
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
                name: "ThongBaoNguoiNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TienDoBaiHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TienDoNoiDungHocTap",
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
                name: "UngVienKhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VaiTroQuyenHan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XuatBaoCao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XuLyViPhamThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "yeu_cau_xuat_du_lieu");

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
                name: "QuyTrinhDonTu");

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
                name: "GiaoVienNguyenVongHocKy");

            migrationBuilder.DropTable(
                name: "HoSoKyLuat",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScheduleGenerationJob",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DonTu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiHocNoiDung",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhieuHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DotKhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyenHan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VaiTro",
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
                name: "MauDonTu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BaiHoc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MauBangKhen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhienThiHocSinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CauHinhDauDiemQuaTrinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThoiKhoaBieu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Chuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CaThi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LoaiDauDiemQuaTrinh",
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
                name: "Block",
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
