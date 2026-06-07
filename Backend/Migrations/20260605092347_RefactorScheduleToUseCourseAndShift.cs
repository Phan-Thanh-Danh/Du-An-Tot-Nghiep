using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorScheduleToUseCourseAndShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_bu_cho_buoi__ThoiKhoaBieu",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_don_vi__DonVi",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_giao_vien_day_thay__NguoiDung",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_lop__LopHanhChinh",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_lop_hoc_phan__LopHocPhan",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_bu_cho_buoi",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_don_vi",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_giao_vien_day_thay",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_lop",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_lop_hoc_phan",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_mon_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "UQ_ThoiKhoaBieu_1",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "UQ_ThoiKhoaBieu_2",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_gio_ket_thuc_2",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_thu_trong_tuan_1",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_trang_thai_3",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_tkb",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_buoi_1",
                schema: "dbo",
                table: "BuoiHoc");

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

            migrationBuilder.CreateIndex(
                name: "UQ_CaHoc_ten_ca",
                schema: "dbo",
                table: "CaHoc",
                column: "ten_ca",
                unique: true);

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "CaHoc",
                columns: new[] { "ma_ca_hoc", "ten_ca", "buoi", "gio_bat_dau", "gio_ket_thuc", "thu_tu", "con_hoat_dong" },
                values: new object[,]
                {
                    { 1, "Ca 1", "sang", new TimeOnly(7, 30), new TimeOnly(9, 0), 1, true },
                    { 2, "Ca 2", "sang", new TimeOnly(9, 5), new TimeOnly(12, 0), 2, true },
                    { 3, "Ca 3", "chieu", new TimeOnly(13, 0), new TimeOnly(14, 30), 3, true },
                    { 4, "Ca 4", "chieu", new TimeOnly(14, 35), new TimeOnly(16, 5), 4, true },
                    { 5, "Ca 5", "chieu", new TimeOnly(16, 10), new TimeOnly(17, 40), 5, true },
                });

            migrationBuilder.AddColumn<int>(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_bat_dau",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_ket_thuc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.Sql(
                """
                UPDATE dbo.ThoiKhoaBieu
                SET trang_thai = N'da_xuat_ban'
                WHERE trang_thai = N'da_duyet';

                WITH SourceCourses AS
                (
                    SELECT
                        tkb.ma_don_vi,
                        MIN(tkb.ma_giao_vien) AS ma_giao_vien,
                        tkb.ma_mon_hoc,
                        lhp.ma_hoc_ky,
                        tkb.ma_lop,
                        MAX(tkb.ma_lop_hoc_phan) AS ma_lop_hoc_phan
                    FROM dbo.ThoiKhoaBieu AS tkb
                    LEFT JOIN dbo.LopHocPhan AS lhp ON lhp.ma_lop_hoc_phan = tkb.ma_lop_hoc_phan
                    GROUP BY tkb.ma_don_vi, tkb.ma_mon_hoc, lhp.ma_hoc_ky, tkb.ma_lop
                )
                INSERT INTO dbo.KhoaHoc
                    (ma_don_vi, ma_giao_vien, ma_mon_hoc, ma_hoc_ky, ma_lop, ma_lop_hoc_phan, tieu_de, mo_ta, trang_thai, url_anh_bia, ngay_tao)
                SELECT
                    source.ma_don_vi,
                    source.ma_giao_vien,
                    source.ma_mon_hoc,
                    source.ma_hoc_ky,
                    source.ma_lop,
                    source.ma_lop_hoc_phan,
                    CONCAT(N'Lịch học migrated - lớp ', source.ma_lop, N' - môn ', source.ma_mon_hoc),
                    N'Khóa học được tạo tự động khi chuyển ThoiKhoaBieu sang MaKhoaHoc.',
                    N'nhap',
                    NULL,
                    SYSUTCDATETIME()
                FROM SourceCourses AS source
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.KhoaHoc AS kh
                    WHERE kh.ma_don_vi = source.ma_don_vi
                        AND kh.ma_mon_hoc = source.ma_mon_hoc
                        AND kh.ma_lop = source.ma_lop
                        AND (
                            kh.ma_hoc_ky = source.ma_hoc_ky
                            OR (kh.ma_hoc_ky IS NULL AND source.ma_hoc_ky IS NULL)
                        )
                );

                UPDATE tkb
                SET
                    ma_khoa_hoc = kh.ma_khoa_hoc,
                    ma_ca_hoc = ca.ma_ca_hoc,
                    ngay_bat_dau = hk.ngay_bat_dau,
                    ngay_ket_thuc = hk.ngay_ket_thuc
                FROM dbo.ThoiKhoaBieu AS tkb
                LEFT JOIN dbo.LopHocPhan AS lhp ON lhp.ma_lop_hoc_phan = tkb.ma_lop_hoc_phan
                CROSS APPLY
                (
                    SELECT TOP (1) kh.ma_khoa_hoc, kh.ma_hoc_ky
                    FROM dbo.KhoaHoc AS kh
                    WHERE kh.ma_don_vi = tkb.ma_don_vi
                        AND kh.ma_mon_hoc = tkb.ma_mon_hoc
                        AND kh.ma_lop = tkb.ma_lop
                        AND (
                            kh.ma_hoc_ky = lhp.ma_hoc_ky
                            OR (kh.ma_hoc_ky IS NULL AND lhp.ma_hoc_ky IS NULL)
                        )
                    ORDER BY CASE WHEN kh.ma_giao_vien = tkb.ma_giao_vien THEN 0 ELSE 1 END, kh.ma_khoa_hoc
                ) AS kh
                LEFT JOIN dbo.HocKy AS hk ON hk.ma_hoc_ky = kh.ma_hoc_ky
                LEFT JOIN dbo.CaHoc AS ca
                    ON CONVERT(time(0), ca.gio_bat_dau) = CONVERT(time(0), tkb.gio_bat_dau)
                    AND CONVERT(time(0), ca.gio_ket_thuc) = CONVERT(time(0), tkb.gio_ket_thuc);

                IF EXISTS (SELECT 1 FROM dbo.ThoiKhoaBieu WHERE ma_khoa_hoc IS NULL OR ma_ca_hoc IS NULL)
                BEGIN
                    THROW 51000, N'Không thể backfill ThoiKhoaBieu sang MaKhoaHoc/MaCaHoc. Hãy kiểm tra dữ liệu lịch cũ hoặc reset database dev trước khi chạy migration.', 1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_buoi",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "du_kien",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "chua_xac_nhan");

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loai_thay_doi",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_thay_doi",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_giao_vien_day_thay",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_phong",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.Sql(
                """
                UPDATE dbo.BuoiHoc
                SET trang_thai_buoi = CASE trang_thai_buoi
                    WHEN N'da_xac_nhan' THEN N'da_dien_ra'
                    WHEN N'da_huy' THEN N'da_huy'
                    ELSE N'du_kien'
                END;

                UPDATE bh
                SET
                    ma_khoa_hoc = tkb.ma_khoa_hoc,
                    ma_ca_hoc = tkb.ma_ca_hoc,
                    ma_phong = tkb.ma_phong,
                    ma_giao_vien = kh.ma_giao_vien
                FROM dbo.BuoiHoc AS bh
                INNER JOIN dbo.ThoiKhoaBieu AS tkb ON tkb.ma_tkb = bh.ma_tkb
                INNER JOIN dbo.KhoaHoc AS kh ON kh.ma_khoa_hoc = tkb.ma_khoa_hoc;

                IF EXISTS
                (
                    SELECT 1
                    FROM dbo.BuoiHoc
                    WHERE ma_khoa_hoc IS NULL
                        OR ma_ca_hoc IS NULL
                        OR ma_phong IS NULL
                        OR ma_giao_vien IS NULL
                )
                BEGIN
                    THROW 51001, N'Không thể backfill BuoiHoc sang snapshot MaKhoaHoc/MaCaHoc/MaPhong/MaGiaoVien.', 1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ma_phong",
                schema: "dbo",
                table: "BuoiHoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "bu_cho_buoi",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "duong_dan_hop",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "gio_bat_dau",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "gio_ket_thuc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_giao_vien_day_thay",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_lop",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_lop_hoc_phan",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ma_mon_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "gio_bat_dau",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "gio_ket_thuc",
                schema: "dbo",
                table: "BuoiHoc");

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
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_ngay",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[ngay_ket_thuc] IS NULL OR [ngay_bat_dau] IS NULL OR [ngay_ket_thuc] >= [ngay_bat_dau]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_thu_trong_tuan",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[thu_trong_tuan] BETWEEN 1 AND 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_trang_thai",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'da_huy')");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuoiHoc_loai_thay_doi",
                schema: "dbo",
                table: "BuoiHoc",
                sql: "[loai_thay_doi] IS NULL OR [loai_thay_doi] IN (N'doi_giang_vien', N'doi_phong', N'doi_ca', N'huy_buoi', N'doi_lich')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_buoi",
                schema: "dbo",
                table: "BuoiHoc",
                sql: "[trang_thai_buoi] IN (N'du_kien', N'da_dien_ra', N'da_huy', N'doi_lich', N'day_thay')");

            migrationBuilder.AddForeignKey(
                name: "FK_BuoiHoc_ma_ca_hoc__CaHoc",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_ca_hoc",
                principalSchema: "dbo",
                principalTable: "CaHoc",
                principalColumn: "ma_ca_hoc");

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
                name: "FK_BuoiHoc_ma_phong__PhongHoc",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_phong",
                principalSchema: "dbo",
                principalTable: "PhongHoc",
                principalColumn: "ma_phong");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_ca_hoc__CaHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_ca_hoc",
                principalSchema: "dbo",
                principalTable: "CaHoc",
                principalColumn: "ma_ca_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_khoa_hoc",
                principalSchema: "dbo",
                principalTable: "KhoaHoc",
                principalColumn: "ma_khoa_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuoiHoc_ma_ca_hoc__CaHoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BuoiHoc_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BuoiHoc_ma_giao_vien_day_thay__NguoiDung",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BuoiHoc_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BuoiHoc_ma_phong__PhongHoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_ca_hoc__CaHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropTable(
                name: "CaHoc",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_phong",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_ngay",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_thu_trong_tuan",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThoiKhoaBieu_trang_thai",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_ca_hoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_giao_vien",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_giao_vien_day_thay",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_khoa_hoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_ma_phong",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_Ngay_Ca_GiaoVien",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_Ngay_Ca_Phong",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "UQ_BuoiHoc_Tkb_NgayHoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BuoiHoc_loai_thay_doi",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_buoi",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ngay_bat_dau",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ngay_ket_thuc",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "loai_thay_doi",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ly_do_thay_doi",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ma_giao_vien_day_thay",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ma_phong",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.RenameColumn(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                newName: "ma_mon_hoc");

            migrationBuilder.RenameColumn(
                name: "ma_ca_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                newName: "ma_lop");

            migrationBuilder.RenameIndex(
                name: "IX_ThoiKhoaBieu_ma_ca_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                newName: "IX_ThoiKhoaBieu_ma_lop");

            migrationBuilder.AddColumn<int>(
                name: "bu_cho_buoi",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "duong_dan_hop",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "gio_bat_dau",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "gio_ket_thuc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ma_giao_vien_day_thay",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_lop_hoc_phan",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_buoi",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "chua_xac_nhan",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "du_kien");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "gio_bat_dau",
                schema: "dbo",
                table: "BuoiHoc",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "gio_ket_thuc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_bu_cho_buoi",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "bu_cho_buoi");

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_don_vi",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_giao_vien_day_thay",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_giao_vien_day_thay");

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_lop_hoc_phan",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_lop_hoc_phan");

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_mon_hoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_ThoiKhoaBieu_1",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                columns: new[] { "ma_giao_vien", "thu_trong_tuan", "gio_bat_dau" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ThoiKhoaBieu_2",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                columns: new[] { "ma_phong", "thu_trong_tuan", "gio_bat_dau" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_gio_ket_thuc_2",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[gio_ket_thuc] > [gio_bat_dau]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_thu_trong_tuan_1",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[thu_trong_tuan] BETWEEN 1 AND 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThoiKhoaBieu_trang_thai_3",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                sql: "[trang_thai] IN (N'nhap', N'da_duyet', N'da_xuat_ban', N'da_huy')");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_ma_tkb",
                schema: "dbo",
                table: "BuoiHoc",
                column: "ma_tkb");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_buoi_1",
                schema: "dbo",
                table: "BuoiHoc",
                sql: "[trang_thai_buoi] IN (N'da_xac_nhan', N'chua_xac_nhan', N'yeu_cau_mo_khoa', N'da_huy')");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_bu_cho_buoi__ThoiKhoaBieu",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "bu_cho_buoi",
                principalSchema: "dbo",
                principalTable: "ThoiKhoaBieu",
                principalColumn: "ma_tkb");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_don_vi__DonVi",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_giao_vien_day_thay__NguoiDung",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_giao_vien_day_thay",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_lop__LopHanhChinh",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_lop",
                principalSchema: "dbo",
                principalTable: "LopHanhChinh",
                principalColumn: "ma_lop");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_lop_hoc_phan__LopHocPhan",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_lop_hoc_phan",
                principalSchema: "dbo",
                principalTable: "LopHocPhan",
                principalColumn: "ma_lop_hoc_phan");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_mon_hoc",
                principalSchema: "dbo",
                principalTable: "DanhMucMonHoc",
                principalColumn: "ma_mon_hoc");
        }
    }
}
