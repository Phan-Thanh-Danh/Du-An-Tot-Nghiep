using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationFoundation : Migration
    {
        private const string TypeCheckSql =
            "[loai_don] IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc')";

        private const string StatusCheckSql =
            "[trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'yeu_cau_bo_sung', N'da_duyet', N'tu_choi', N'da_huy')";

        private const string ProcessingStatusCheckSql =
            "[trang_thai_xu_ly_nghiep_vu] IN (N'chua_xu_ly', N'cho_xu_ly', N'da_ghi_nhan', N'xu_ly_thanh_cong', N'xu_ly_that_bai', N'can_xu_ly_thu_cong')";

        private const string ActionCheckSql =
            "[hanh_dong] IN (N'tao_nhap', N'cap_nhat', N'nop', N'nop_lai', N'phan_cong', N'phan_cong_lai', N'tiep_nhan', N'yeu_cau_bo_sung', N'bo_sung', N'phe_duyet', N'tu_choi', N'leo_thang', N'huy', N'xu_ly_nghiep_vu')";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                IF EXISTS (
                    SELECT 1
                    FROM [dbo].[DonTu] AS d
                    LEFT JOIN [dbo].[NguoiDung] AS n ON n.[ma_nguoi_dung] = d.[ma_hoc_sinh]
                    WHERE n.[ma_nguoi_dung] IS NULL
                )
                    THROW 51000, N'P0-DT1 migration blocked: DonTu has orphan ma_hoc_sinh rows.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [loai_don] NOT IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc'))
                    THROW 51001, N'P0-DT1 migration blocked: DonTu has unsupported loai_don.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [trang_thai] NOT IN (N'nhap', N'da_nop', N'dang_xem_xet', N'yeu_cau_bo_sung', N'da_duyet', N'tu_choi', N'da_huy'))
                    THROW 51002, N'P0-DT1 migration blocked: DonTu has unsupported trang_thai.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [du_lieu_bieu_mau] IS NOT NULL AND ISJSON([du_lieu_bieu_mau]) <> 1)
                    THROW 51003, N'P0-DT1 migration blocked: DonTu has invalid du_lieu_bieu_mau JSON.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [nhat_ky_tu_dong] IS NOT NULL AND ISJSON([nhat_ky_tu_dong]) <> 1)
                    THROW 51004, N'P0-DT1 migration blocked: DonTu has invalid nhat_ky_tu_dong JSON.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[NhatKyDuyetDon] WHERE [hanh_dong] NOT IN (N'tao_nhap', N'cap_nhat', N'nop', N'nop_lai', N'phan_cong', N'phan_cong_lai', N'tiep_nhan', N'yeu_cau_bo_sung', N'bo_sung', N'phe_duyet', N'tu_choi', N'phan_cong', N'leo_thang', N'huy', N'xu_ly_nghiep_vu'))
                    THROW 51005, N'P0-DT1 migration blocked: NhatKyDuyetDon has unsupported hanh_dong.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_NhatKyDuyetDon_ma_don_tu",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhatKyDuyetDon_hanh_dong_1",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_ma_hoc_sinh",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_nguoi_duyet_hien_tai",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_loai_don_1",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_trang_thai_2",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.CreateTable(
                name: "MauDonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_don = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loai_don = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_mau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    phien_ban = table.Column<int>(type: "int", nullable: false),
                    cau_hinh_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bat_buoc_minh_chung = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    so_tep_toi_da = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    dung_luong_tep_toi_da_byte = table.Column<long>(type: "bigint", nullable: false, defaultValue: 10485760L),
                    tong_dung_luong_toi_da_byte = table.Column<long>(type: "bigint", nullable: false, defaultValue: 26214400L),
                    sla_gio = table.Column<int>(type: "int", nullable: true),
                    dang_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauDonTu", x => x.ma_mau_don);
                    table.CheckConstraint("CK_MauDonTu_cau_hinh_json_ISJSON", "ISJSON([cau_hinh_json]) = 1");
                    table.CheckConstraint("CK_MauDonTu_dung_luong_tep", "[dung_luong_tep_toi_da_byte] > 0");
                    table.CheckConstraint("CK_MauDonTu_loai_don", "[loai_don] IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc')");
                    table.CheckConstraint("CK_MauDonTu_phien_ban", "[phien_ban] > 0");
                    table.CheckConstraint("CK_MauDonTu_sla_gio", "[sla_gio] IS NULL OR [sla_gio] >= 0");
                    table.CheckConstraint("CK_MauDonTu_so_tep_toi_da", "[so_tep_toi_da] BETWEEN 0 AND 5");
                    table.CheckConstraint("CK_MauDonTu_tong_dung_luong", "[tong_dung_luong_toi_da_byte] >= [dung_luong_tep_toi_da_byte]");
                });

            SeedApplicationTemplates(migrationBuilder);

            migrationBuilder.AddColumn<DateTime>(
                name: "han_xu_ly_luc",
                schema: "dbo",
                table: "DonTu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ket_qua_xu_ly_json",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "DonTu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_mau_don",
                schema: "dbo",
                table: "DonTu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DonTu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_duyet",
                schema: "dbo",
                table: "DonTu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_nop",
                schema: "dbo",
                table: "DonTu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_xu_ly_cuoi",
                schema: "dbo",
                table: "DonTu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "noi_dung_yeu_cau_bo_sung",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tieu_de",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_xu_ly_nghiep_vu",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE d
                SET
                    [ma_don_vi] = n.[ma_don_vi],
                    [ma_mau_don] = m.[ma_mau_don],
                    [tieu_de] = CONCAT(N'Đơn ', d.[loai_don], N' #', CONVERT(nvarchar(20), d.[ma_don_tu])),
                    [ngay_cap_nhat] = d.[ngay_tao],
                    [trang_thai_xu_ly_nghiep_vu] = CASE WHEN d.[trang_thai] = N'da_duyet' THEN N'can_xu_ly_thu_cong' ELSE N'chua_xu_ly' END
                FROM [dbo].[DonTu] AS d
                INNER JOIN [dbo].[NguoiDung] AS n ON n.[ma_nguoi_dung] = d.[ma_hoc_sinh]
                LEFT JOIN [dbo].[MauDonTu] AS m ON m.[loai_don] = d.[loai_don] AND m.[phien_ban] = 1 AND m.[dang_hoat_dong] = 1;

                UPDATE d
                SET [ngay_nop] = COALESCE((
                    SELECT MIN(nk.[ngay_tao])
                    FROM [dbo].[NhatKyDuyetDon] AS nk
                    WHERE nk.[ma_don_tu] = d.[ma_don_tu] AND nk.[hanh_dong] = N'nop'
                ), CASE WHEN d.[trang_thai] <> N'nhap' THEN d.[ngay_tao] ELSE NULL END)
                FROM [dbo].[DonTu] AS d;

                UPDATE d
                SET [ngay_duyet] = (
                    SELECT MAX(nk.[ngay_tao])
                    FROM [dbo].[NhatKyDuyetDon] AS nk
                    WHERE nk.[ma_don_tu] = d.[ma_don_tu] AND nk.[hanh_dong] IN (N'phe_duyet', N'tu_choi')
                )
                FROM [dbo].[DonTu] AS d;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [ma_don_vi] IS NULL)
                    THROW 51006, N'P0-DT1 migration blocked: cannot backfill DonTu.ma_don_vi.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [tieu_de] IS NULL OR LTRIM(RTRIM([tieu_de])) = N'')
                    THROW 51007, N'P0-DT1 migration blocked: cannot backfill DonTu.tieu_de.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [ngay_cap_nhat] IS NULL)
                    THROW 51008, N'P0-DT1 migration blocked: cannot backfill DonTu.ngay_cap_nhat.', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[DonTu] WHERE [trang_thai_xu_ly_nghiep_vu] IS NULL)
                    THROW 51009, N'P0-DT1 migration blocked: cannot backfill DonTu.trang_thai_xu_ly_nghiep_vu.', 1;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "DonTu",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DonTu",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tieu_de",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_xu_ly_nghiep_vu",
                schema: "dbo",
                table: "DonTu",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "chua_xu_ly",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                schema: "dbo",
                table: "DonTu",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<int>(
                name: "ma_nguoi_duyet",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "hanh_dong",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_cong_khai",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_noi_bo",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "hien_thi_cho_hoc_sinh",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "nguon_thuc_hien",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "user");

            migrationBuilder.AddColumn<string>(
                name: "snapshot_json",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_cu",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_moi",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TepDinhKemDonTu",
                schema: "dbo",
                columns: table => new
                {
                    ma_tep = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_tu = table.Column<int>(type: "int", nullable: false),
                    storage_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ten_file_goc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ten_file_luu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: false),
                    file_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    nguoi_tai_len = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    da_xoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    nguoi_xoa = table.Column<int>(type: "int", nullable: true),
                    ngay_xoa = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKemDonTu", x => x.ma_tep);
                    table.CheckConstraint("CK_TepDinhKemDonTu_kich_thuoc", "[kich_thuoc_byte] > 0");
                    table.CheckConstraint("CK_TepDinhKemDonTu_soft_delete", "([da_xoa] = 0 AND [nguoi_xoa] IS NULL AND [ngay_xoa] IS NULL) OR ([da_xoa] = 1 AND [nguoi_xoa] IS NOT NULL AND [ngay_xoa] IS NOT NULL)");
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyDuyetDon_ma_don_tu_ngay_tao",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                columns: new[] { "ma_don_tu", "ngay_tao" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhatKyDuyetDon_hanh_dong_1",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                sql: "[hanh_dong] IN (N'tao_nhap', N'cap_nhat', N'nop', N'nop_lai', N'phan_cong', N'phan_cong_lai', N'tiep_nhan', N'yeu_cau_bo_sung', N'bo_sung', N'phe_duyet', N'tu_choi', N'leo_thang', N'huy', N'xu_ly_nghiep_vu')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhatKyDuyetDon_nguon_thuc_hien",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                sql: "[nguon_thuc_hien] IN (N'user', N'system')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhatKyDuyetDon_snapshot_json_ISJSON",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                sql: "[snapshot_json] IS NULL OR ISJSON([snapshot_json]) = 1");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_ket_qua_xu_ly_json_ISJSON",
                schema: "dbo",
                table: "DonTu",
                sql: "[ket_qua_xu_ly_json] IS NULL OR ISJSON([ket_qua_xu_ly_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_loai_don_1",
                schema: "dbo",
                table: "DonTu",
                sql: "[loai_don] IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_trang_thai_2",
                schema: "dbo",
                table: "DonTu",
                sql: "[trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'yeu_cau_bo_sung', N'da_duyet', N'tu_choi', N'da_huy')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_trang_thai_xu_ly_nghiep_vu",
                schema: "dbo",
                table: "DonTu",
                sql: "[trang_thai_xu_ly_nghiep_vu] IN (N'chua_xu_ly', N'cho_xu_ly', N'da_ghi_nhan', N'xu_ly_thanh_cong', N'xu_ly_that_bai', N'can_xu_ly_thu_cong')");

            migrationBuilder.CreateIndex(
                name: "UX_MauDonTu_loai_don_active",
                schema: "dbo",
                table: "MauDonTu",
                column: "loai_don",
                unique: true,
                filter: "[dang_hoat_dong] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_MauDonTu_loai_don_phien_ban",
                schema: "dbo",
                table: "MauDonTu",
                columns: new[] { "loai_don", "phien_ban" },
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

            migrationBuilder.AddForeignKey(
                name: "FK_DonTu_ma_don_vi__DonVi",
                schema: "dbo",
                table: "DonTu",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_DonTu_ma_mau_don__MauDonTu",
                schema: "dbo",
                table: "DonTu",
                column: "ma_mau_don",
                principalSchema: "dbo",
                principalTable: "MauDonTu",
                principalColumn: "ma_mau_don");

            migrationBuilder.AddForeignKey(
                name: "FK_DonTu_nguoi_xu_ly_cuoi__NguoiDung",
                schema: "dbo",
                table: "DonTu",
                column: "nguoi_xu_ly_cuoi",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        private static void SeedApplicationTemplates(MigrationBuilder migrationBuilder)
        {
            foreach (var template in GetApplicationTemplates())
            {
                migrationBuilder.Sql($"""
                    INSERT INTO [dbo].[MauDonTu]
                        ([loai_don], [ten_mau], [phien_ban], [cau_hinh_json], [bat_buoc_minh_chung], [so_tep_toi_da], [dung_luong_tep_toi_da_byte], [tong_dung_luong_toi_da_byte], [sla_gio], [dang_hoat_dong], [ngay_tao], [ngay_cap_nhat])
                    VALUES
                        (N'{EscapeSql(template.Type)}', N'{EscapeSql(template.Name)}', 1, N'{EscapeSql(template.Json)}', {(template.EvidenceRequired ? 1 : 0)}, 5, 10485760, 26214400, {template.SlaHours}, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                    """);
            }
        }

        private static IReadOnlyList<(string Type, string Name, string Json, bool EvidenceRequired, int SlaHours)> GetApplicationTemplates()
        {
            return
            [
                ("nghi_phep", "Mẫu đơn nghỉ phép", TemplateJson("""
                    [
                      {"key":"ngay_bat_dau","label":"Ngày bắt đầu nghỉ","type":"date","required":true},
                      {"key":"ngay_ket_thuc","label":"Ngày kết thúc nghỉ","type":"date","required":true},
                      {"key":"ly_do","label":"Lý do nghỉ phép","type":"textarea","required":true,"maxLength":1000,"evidenceRequired":false},
                      {"key":"lien_he_khan_cap","label":"Thông tin liên hệ khi cần","type":"text","required":false,"maxLength":200}
                    ]
                    """), false, 48),
                ("thi_lai", "Mẫu đơn đăng ký thi lại", TemplateJson("""
                    [
                      {"key":"ma_hoc_ky","label":"Học kỳ","type":"related_entity","required":true,"relatedEntity":"hoc_ky"},
                      {"key":"ma_mon_hoc","label":"Môn học","type":"related_entity","required":true,"relatedEntity":"mon_hoc"},
                      {"key":"ly_do","label":"Lý do đăng ký thi lại","type":"textarea","required":true,"maxLength":1000}
                    ]
                    """), false, 72),
                ("chuyen_truong", "Mẫu đơn chuyển trường", MinimalJson("Lý do chuyển trường"), true, 120),
                ("cap_chung_chi", "Mẫu đơn cấp chứng chỉ", MinimalJson("Nội dung yêu cầu cấp chứng chỉ"), false, 72),
                ("khac", "Mẫu đơn khác", MinimalJson("Nội dung đề nghị"), false, 72),
                ("phuc_tra_diem", "Mẫu đơn phúc tra điểm", TemplateJson("""
                    [
                      {"key":"ma_hoc_ky","label":"Học kỳ","type":"related_entity","required":true,"relatedEntity":"hoc_ky"},
                      {"key":"ma_mon_hoc","label":"Môn học","type":"related_entity","required":true,"relatedEntity":"mon_hoc"},
                      {"key":"ma_diem_so","label":"Dòng điểm cần phúc tra","type":"related_entity","required":false,"relatedEntity":"diem_so"},
                      {"key":"cot_diem","label":"Cột điểm","type":"select","required":true,"options":[{"value":"qua_trinh","label":"Quá trình"},{"value":"giua_ky","label":"Giữa kỳ"},{"value":"cuoi_ky","label":"Cuối kỳ"}]},
                      {"key":"ly_do","label":"Lý do phúc tra","type":"textarea","required":true,"maxLength":1200}
                    ]
                    """), true, 96),
                ("bao_luu", "Mẫu đơn bảo lưu", TemplateJson("""
                    [
                      {"key":"ma_hoc_ky_bat_dau","label":"Học kỳ bắt đầu bảo lưu","type":"related_entity","required":true,"relatedEntity":"hoc_ky"},
                      {"key":"thoi_luong_du_kien","label":"Thời lượng dự kiến theo tháng","type":"number","required":true},
                      {"key":"ly_do","label":"Lý do bảo lưu","type":"textarea","required":true,"maxLength":1500},
                      {"key":"cam_ket_lien_he","label":"Cam kết giữ liên hệ với nhà trường","type":"boolean","required":true}
                    ]
                    """), true, 120),
                ("chuyen_nganh", "Mẫu đơn chuyển ngành", MinimalJson("Lý do chuyển ngành"), true, 120),
                ("chuyen_co_so", "Mẫu đơn chuyển cơ sở", TemplateJson("""
                    [
                      {"key":"ma_don_vi_hien_tai","label":"Cơ sở hiện tại","type":"related_entity","required":true,"relatedEntity":"don_vi"},
                      {"key":"ma_don_vi_mong_muon","label":"Cơ sở mong muốn","type":"related_entity","required":true,"relatedEntity":"don_vi"},
                      {"key":"thoi_diem_mong_muon","label":"Thời điểm mong muốn chuyển","type":"date","required":true},
                      {"key":"ly_do","label":"Lý do chuyển cơ sở","type":"textarea","required":true,"maxLength":1500}
                    ]
                    """), true, 120),
                ("xac_nhan", "Mẫu đơn xác nhận", TemplateJson("""
                    [
                      {"key":"loai_xac_nhan","label":"Loại xác nhận","type":"select","required":true,"options":[{"value":"dang_hoc","label":"Đang học"},{"value":"hoan_thanh_mon","label":"Hoàn thành môn"},{"value":"khac","label":"Khác"}]},
                      {"key":"muc_dich_su_dung","label":"Mục đích sử dụng","type":"textarea","required":true,"maxLength":1000},
                      {"key":"so_ban","label":"Số bản cần cấp","type":"number","required":true}
                    ]
                    """), false, 72),
                ("rut_hoc", "Mẫu đơn rút học", TemplateJson("""
                    [
                      {"key":"ngay_du_kien_rut","label":"Ngày dự kiến rút học","type":"date","required":true},
                      {"key":"ly_do","label":"Lý do rút học","type":"textarea","required":true,"maxLength":2000},
                      {"key":"da_trao_doi_phu_huynh","label":"Đã trao đổi với phụ huynh/người bảo hộ","type":"boolean","required":true}
                    ]
                    """), true, 120)
            ];
        }

        private static string MinimalJson(string label)
        {
            return TemplateJson($$"""
                [
                  {"key":"noi_dung","label":"{{label}}","type":"textarea","required":true,"maxLength":1500}
                ]
                """);
        }

        private static string TemplateJson(string fieldsJson)
        {
            return $$"""{"fields":{{fieldsJson.ReplaceLineEndings("").Replace("  ", "")}}}""";
        }

        private static string EscapeSql(string value)
        {
            return value.Replace("'", "''", StringComparison.Ordinal);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonTu_ma_don_vi__DonVi",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropForeignKey(
                name: "FK_DonTu_ma_mau_don__MauDonTu",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropForeignKey(
                name: "FK_DonTu_nguoi_xu_ly_cuoi__NguoiDung",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropTable(
                name: "MauDonTu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TepDinhKemDonTu",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyDuyetDon_ma_don_tu_ngay_tao",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhatKyDuyetDon_hanh_dong_1",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhatKyDuyetDon_nguon_thuc_hien",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhatKyDuyetDon_snapshot_json_ISJSON",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_han_xu_ly_trang_thai",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_loai_don_trang_thai",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_ma_don_vi_trang_thai_ngay_nop",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_ma_hoc_sinh_ngay_tao",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_ma_mau_don",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_nguoi_duyet_trang_thai_ngay_nop",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropIndex(
                name: "IX_DonTu_nguoi_xu_ly_cuoi",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_ket_qua_xu_ly_json_ISJSON",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_loai_don_1",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_trang_thai_2",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonTu_trang_thai_xu_ly_nghiep_vu",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ghi_chu_cong_khai",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "ghi_chu_noi_bo",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "hien_thi_cho_hoc_sinh",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "nguon_thuc_hien",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "snapshot_json",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "trang_thai_cu",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "trang_thai_moi",
                schema: "dbo",
                table: "NhatKyDuyetDon");

            migrationBuilder.DropColumn(
                name: "han_xu_ly_luc",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ket_qua_xu_ly_json",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ma_mau_don",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ngay_duyet",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "ngay_nop",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "nguoi_xu_ly_cuoi",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "noi_dung_yeu_cau_bo_sung",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "row_version",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "tieu_de",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.DropColumn(
                name: "trang_thai_xu_ly_nghiep_vu",
                schema: "dbo",
                table: "DonTu");

            migrationBuilder.AlterColumn<int>(
                name: "ma_nguoi_duyet",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "hanh_dong",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyDuyetDon_ma_don_tu",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                column: "ma_don_tu");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhatKyDuyetDon_hanh_dong_1",
                schema: "dbo",
                table: "NhatKyDuyetDon",
                sql: "[hanh_dong] IN (N'nop', N'phe_duyet', N'tu_choi', N'phan_cong', N'leo_thang')");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_loai_don_1",
                schema: "dbo",
                table: "DonTu",
                sql: "[loai_don] IN ( N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc' )");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonTu_trang_thai_2",
                schema: "dbo",
                table: "DonTu",
                sql: "[trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'da_duyet', N'tu_choi')");
        }
    }
}
