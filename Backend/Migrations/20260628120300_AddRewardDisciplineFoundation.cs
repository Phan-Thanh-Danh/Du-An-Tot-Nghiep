using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardDisciplineFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_ma_hoc_sinh",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_loai_khen_thuong_1",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_ma_don_vi",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_ma_hoc_sinh",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.AlterColumn<string>(
                name: "loai_khen_thuong",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "danh_hieu_snapshot",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "diem_xet",
                schema: "dbo",
                table: "KhenThuong",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ho_ten_snapshot",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_huy",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_dot_khen_thuong",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_mau_bang_khen",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mssv_snapshot",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "KhenThuong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_huy",
                schema: "dbo",
                table: "KhenThuong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_cap",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_duyet",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_huy",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ten_hoc_ky_snapshot",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "nhap");

            migrationBuilder.AddColumn<string>(
                name: "url_pdf_bang_khen",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "xep_hang",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chung_tu_json",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "da_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "hinh_thuc_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "nhac_nho");

            migrationBuilder.AddColumn<string>(
                name: "loai_doi_tuong_lien_ket",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_doi_tuong_lien_ket",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_hoc_ky",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "muc_do_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "nhe");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_duyet",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_het_hieu_luc",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_hieu_luc",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "date",
                nullable: false,
                defaultValueSql: "CONVERT(date, SYSUTCDATETIME())");

            migrationBuilder.AddColumn<int>(
                name: "nguoi_duyet",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "nhap");

            migrationBuilder.Sql(
                """
                UPDATE kt
                SET ma_don_vi = nd.ma_don_vi
                FROM dbo.KhenThuong AS kt
                INNER JOIN dbo.NguoiDung AS nd
                    ON kt.ma_hoc_sinh = nd.ma_nguoi_dung
                WHERE kt.ma_don_vi IS NULL;

                IF EXISTS (SELECT 1 FROM dbo.KhenThuong WHERE ma_don_vi IS NULL)
                BEGIN
                    THROW 51000, N'Không thể backfill ma_don_vi cho dữ liệu KhenThuong legacy.', 1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "KhenThuong",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MauBangKhen",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_bang_khen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_mau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    loai_mau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    file_nen_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    chieu_rong = table.Column<int>(type: "int", nullable: false),
                    chieu_cao = table.Column<int>(type: "int", nullable: false),
                    huong_giay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    cau_hinh_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauBangKhen", x => x.ma_mau_bang_khen);
                    table.CheckConstraint("CK_MauBangKhen_cau_hinh_json", "[cau_hinh_json] IS NULL OR ISJSON([cau_hinh_json]) = 1");
                    table.CheckConstraint("CK_MauBangKhen_huong_giay", "[huong_giay] IN (N'A4_NGANG', N'A4_DOC')");
                    table.CheckConstraint("CK_MauBangKhen_kich_thuoc", "[chieu_rong] > 0 AND [chieu_cao] > 0");
                    table.CheckConstraint("CK_MauBangKhen_loai_mau", "[loai_mau] IN (N'TOP_100_HOC_KY')");
                    table.ForeignKey(
                        name: "FK_MauBangKhen_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "DotKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_dot_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ten_dot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    loai_dot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "TOP_100_HOC_KY"),
                    so_luong_toi_da = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    tieu_chi_xet_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ma_mau_bang_khen = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "nhap"),
                    nguoi_tao = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    nguoi_duyet = table.Column<int>(type: "int", nullable: true),
                    ngay_duyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_cong_bo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotKhenThuong", x => x.ma_dot_khen_thuong);
                    table.CheckConstraint("CK_DotKhenThuong_loai_dot", "[loai_dot] IN (N'TOP_100_HOC_KY')");
                    table.CheckConstraint("CK_DotKhenThuong_so_luong_toi_da", "[so_luong_toi_da] > 0");
                    table.CheckConstraint("CK_DotKhenThuong_tieu_chi_xet_json", "[tieu_chi_xet_json] IS NULL OR ISJSON([tieu_chi_xet_json]) = 1");
                    table.CheckConstraint("CK_DotKhenThuong_trang_thai", "[trang_thai] IN (N'nhap', N'dang_xet', N'cho_duyet', N'da_duyet', N'da_cong_bo', N'da_huy')");
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
                    table.ForeignKey(
                        name: "FK_DotKhenThuong_ma_mau_bang_khen__MauBangKhen",
                        column: x => x.ma_mau_bang_khen,
                        principalSchema: "dbo",
                        principalTable: "MauBangKhen",
                        principalColumn: "ma_mau_bang_khen");
                    table.ForeignKey(
                        name: "FK_DotKhenThuong_nguoi_duyet__NguoiDung",
                        column: x => x.nguoi_duyet,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_DotKhenThuong_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_diem_xet",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[diem_xet] IS NULL OR [diem_xet] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_loai_khen_thuong_1",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[loai_khen_thuong] IN (N'hoc_luc', N'dac_biet', N'thi_dau', N'TOP_100_HOC_KY', N'KHAC')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_trang_thai",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[trang_thai] IN (N'nhap', N'cho_duyet', N'da_duyet', N'da_cap', N'da_sinh_pdf', N'loi_sinh_pdf', N'da_huy')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_xep_hang",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[xep_hang] IS NULL OR [xep_hang] > 0");

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
                name: "IX_HoSoKyLuat_nguoi_duyet",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_duyet");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_go_ky_luat");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoSoKyLuat_chung_tu_json",
                schema: "dbo",
                table: "HoSoKyLuat",
                sql: "[chung_tu_json] IS NULL OR ISJSON([chung_tu_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoSoKyLuat_hinh_thuc_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat",
                sql: "[hinh_thuc_xu_ly] IN (N'nhac_nho', N'khien_trach', N'canh_cao', N'dinh_chi', N'khac')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoSoKyLuat_muc_do_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat",
                sql: "[muc_do_vi_pham] IN (N'nhe', N'trung_binh', N'nghiem_trong')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoSoKyLuat_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat",
                sql: "[trang_thai] IN (N'nhap', N'cho_duyet', N'da_duyet', N'tu_choi', N'dang_hieu_luc', N'het_hieu_luc', N'da_go_hieu_luc', N'da_huy')");

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
                filter: "[trang_thai] <> N'da_huy'");

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

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_ma_hoc_ky__HocKy",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "ma_hoc_ky",
                principalSchema: "dbo",
                principalTable: "HocKy",
                principalColumn: "ma_hoc_ky");

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
                name: "FK_KhenThuong_ma_don_vi__DonVi",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_ma_dot_khen_thuong__DotKhenThuong",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_dot_khen_thuong",
                principalSchema: "dbo",
                principalTable: "DotKhenThuong",
                principalColumn: "ma_dot_khen_thuong");

            migrationBuilder.AddForeignKey(
                name: "FK_KhenThuong_ma_mau_bang_khen__MauBangKhen",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_mau_bang_khen",
                principalSchema: "dbo",
                principalTable: "MauBangKhen",
                principalColumn: "ma_mau_bang_khen");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKyLuat_ma_hoc_ky__HocKy",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKyLuat_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKyLuat_nguoi_go_ky_luat__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_ma_don_vi__DonVi",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_ma_dot_khen_thuong__DotKhenThuong",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_ma_mau_bang_khen__MauBangKhen",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_nguoi_cap__NguoiDung",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_nguoi_duyet__NguoiDung",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhenThuong_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropTable(
                name: "DotKhenThuong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MauBangKhen",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_don_vi_trang_thai",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_dot_xep_hang",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_hoc_sinh_hoc_ky_loai",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_ma_mau_bang_khen",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_nguoi_cap",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_nguoi_duyet",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_KhenThuong_nguoi_huy",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_diem_xet",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_loai_khen_thuong_1",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_trang_thai",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhenThuong_xep_hang",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_don_vi_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_hoc_ky_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_hoc_sinh_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_ngay_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_nguoi_duyet",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_nguoi_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoSoKyLuat_chung_tu_json",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoSoKyLuat_hinh_thuc_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoSoKyLuat_muc_do_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoSoKyLuat_trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "danh_hieu_snapshot",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "diem_xet",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ho_ten_snapshot",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ly_do_huy",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ma_dot_khen_thuong",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ma_mau_bang_khen",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "mssv_snapshot",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ngay_huy",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "nguoi_cap",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "nguoi_duyet",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "nguoi_huy",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ten_hoc_ky_snapshot",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "trang_thai",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "url_pdf_bang_khen",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "xep_hang",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "chung_tu_json",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "da_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "hinh_thuc_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "loai_doi_tuong_lien_ket",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ly_do_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ma_doi_tuong_lien_ket",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ma_hoc_ky",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "muc_do_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_duyet",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_het_hieu_luc",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_hieu_luc",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_vi_pham",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "nguoi_duyet",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "nguoi_go_ky_luat",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "trang_thai",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.AlterColumn<string>(
                name: "loai_khen_thuong",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuong_ma_hoc_sinh",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_hoc_sinh");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhenThuong_loai_khen_thuong_1",
                schema: "dbo",
                table: "KhenThuong",
                sql: "[loai_khen_thuong] IN (N'hoc_luc', N'dac_biet', N'thi_dau')");

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
        }
    }
}
