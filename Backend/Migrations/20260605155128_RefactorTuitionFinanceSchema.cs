using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTuitionFinanceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_ma_hoc_sinh",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "UQ_HoaDon_1",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_da_thanh_toan_3",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_giam_tru_2",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_phuong_thuc_tt_5",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_so_tien_1",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_trang_thai_4",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_GiaoDich_ma_hoa_don",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_du_lieu_phan_hoi_ISJSON",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_loai_giao_dich_2",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_so_tien_1",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_trang_thai_3",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "HoaDon",
                type: "datetime2",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "du_lieu_phan_hoi",
                schema: "dbo",
                table: "GiaoDich",
                newName: "response_payload_json");

            migrationBuilder.RenameIndex(
                name: "UQ_GiaoDich_1",
                schema: "dbo",
                table: "GiaoDich",
                newName: "UQ_GiaoDich_ma_tham_chieu_cong");

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[HoaDon]
                SET [giam_tru] = 0
                WHERE [giam_tru] IS NULL;

                UPDATE [dbo].[HoaDon]
                SET [trang_thai] = CASE [trang_thai]
                    WHEN N'mot_phan' THEN N'thanh_toan_mot_phan'
                    WHEN N'dang_xu_ly' THEN N'chua_thanh_toan'
                    WHEN N'that_bai' THEN N'chua_thanh_toan'
                    ELSE [trang_thai]
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "chua_thanh_toan",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "chua_thanh_toan");

            migrationBuilder.AlterColumn<decimal>(
                name: "giam_tru",
                schema: "dbo",
                table: "HoaDon",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loai_hoa_don",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "hoc_phi");

            migrationBuilder.AddColumn<string>(
                name: "ma_hoa_don_code",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "HoaDon",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<int>(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "HoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_tao",
                schema: "dbo",
                table: "HoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[HoaDon]
                SET [ma_hoa_don_code] = CONCAT(
                    N'HD',
                    DATEPART(year, [han_thanh_toan]),
                    N'-',
                    RIGHT(CONCAT(N'000000', CONVERT(nvarchar(20), [ma_hoa_don])), 6))
                WHERE [ma_hoa_don_code] IS NULL OR [ma_hoa_don_code] = N'';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ma_hoa_don_code",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "loai_giao_dich",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "callback_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "checkout_url",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chu_thich",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_tai_khoan_nhan_tien",
                schema: "dbo",
                table: "GiaoDich",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "GiaoDich",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_het_han",
                schema: "dbo",
                table: "GiaoDich",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_thanh_toan",
                schema: "dbo",
                table: "GiaoDich",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nha_cung_cap_thanh_toan",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "noi_dung_chuyen_khoan",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_payload",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_url",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[GiaoDich]
                SET
                    [so_tien] = CASE
                        WHEN [loai_giao_dich] = N'ghi_no' AND [so_tien] > 0 THEN -[so_tien]
                        ELSE [so_tien]
                    END,
                    [loai_giao_dich] = CASE [loai_giao_dich]
                        WHEN N'thanh_toan' THEN N'thanh_toan_hoc_phi'
                        WHEN N'ghi_no' THEN N'phat_sinh_hoc_phi'
                        WHEN N'ghi_co' THEN N'dieu_chinh_cong_no'
                        ELSE [loai_giao_dich]
                    END;
                """);

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
                    table.CheckConstraint("CK_TaiKhoanNhanTien_provider", "[nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr', N'casso', N'sepay', N'mb_bank')");
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung",
                sql: "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh', N'sieu_quan_tri', N'quan_tri_co_so', N'quan_tri_co_so_con', N'chu_tich', N'hoidong_quanly_noidung', N'admin_tai_chinh', N'ke_toan_co_so', N'ke_toan_truong_co_so')");

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'admin_tai_chinh')
                BEGIN
                    INSERT INTO [dbo].[VaiTro] ([ma_vai_tro], [ma_code_vai_tro], [ten_vai_tro])
                    SELECT ISNULL(MAX([ma_vai_tro]), 0) + 1, N'admin_tai_chinh', N'Admin tài chính'
                    FROM [dbo].[VaiTro];
                END;

                IF NOT EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'ke_toan_co_so')
                BEGIN
                    INSERT INTO [dbo].[VaiTro] ([ma_vai_tro], [ma_code_vai_tro], [ten_vai_tro])
                    SELECT ISNULL(MAX([ma_vai_tro]), 0) + 1, N'ke_toan_co_so', N'Kế toán cơ sở'
                    FROM [dbo].[VaiTro];
                END;

                IF NOT EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'ke_toan_truong_co_so')
                BEGIN
                    INSERT INTO [dbo].[VaiTro] ([ma_vai_tro], [ma_code_vai_tro], [ten_vai_tro])
                    SELECT ISNULL(MAX([ma_vai_tro]), 0) + 1, N'ke_toan_truong_co_so', N'Kế toán trưởng cơ sở'
                    FROM [dbo].[VaiTro];
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_nguoi_cap_nhat",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_cap_nhat");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_da_thanh_toan",
                schema: "dbo",
                table: "HoaDon",
                sql: "[da_thanh_toan] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_giam_tru",
                schema: "dbo",
                table: "HoaDon",
                sql: "[giam_tru] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_loai_hoa_don",
                schema: "dbo",
                table: "HoaDon",
                sql: "[loai_hoa_don] IN (N'hoc_phi', N'le_phi', N'tai_lieu', N'khac')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_so_tien",
                schema: "dbo",
                table: "HoaDon",
                sql: "[so_tien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_trang_thai",
                schema: "dbo",
                table: "HoaDon",
                sql: "[trang_thai] IN (N'chua_thanh_toan', N'thanh_toan_mot_phan', N'da_thanh_toan', N'qua_han', N'da_huy')");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_HoaDon_TrangThai",
                schema: "dbo",
                table: "GiaoDich",
                columns: new[] { "ma_hoa_don", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_ma_tai_khoan_nhan_tien",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tai_khoan_nhan_tien");

            migrationBuilder.CreateIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tham_chieu_noi_bo",
                unique: true,
                filter: "[ma_tham_chieu_noi_bo] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_callback_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[callback_payload_json] IS NULL OR ISJSON([callback_payload_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_loai_giao_dich",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[loai_giao_dich] IN (N'phat_sinh_hoc_phi', N'thanh_toan_hoc_phi', N'dieu_chinh_cong_no', N'hoan_tien', N'huy_hoa_don')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[nha_cung_cap_thanh_toan] IS NULL OR [nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr', N'casso', N'sepay', N'mb_bank')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_request_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[request_payload_json] IS NULL OR ISJSON([request_payload_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_response_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[response_payload_json] IS NULL OR ISJSON([response_payload_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_trang_thai",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[trang_thai] IN (N'phat_sinh', N'cho_thanh_toan', N'dang_xu_ly', N'thanh_cong', N'that_bai', N'het_han', N'da_huy', N'sai_so_tien', N'cho_xu_ly_thu_cong')");

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

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDich_ma_tai_khoan_nhan_tien__TaiKhoanNhanTien",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_tai_khoan_nhan_tien",
                principalSchema: "dbo",
                principalTable: "TaiKhoanNhanTien",
                principalColumn: "ma_tai_khoan_nhan_tien");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_cap_nhat",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiaoDich_ma_tai_khoan_nhan_tien__TaiKhoanNhanTien",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropTable(
                name: "TaiKhoanNhanTien",
                schema: "dbo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_nguoi_cap_nhat",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_nguoi_tao",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "UQ_HoaDon_HocSinh_HocKy_LoaiHoaDon",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "UQ_HoaDon_ma_hoa_don_code",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_da_thanh_toan",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_giam_tru",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_loai_hoa_don",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_so_tien",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HoaDon_trang_thai",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_GiaoDich_HoaDon_TrangThai",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropIndex(
                name: "IX_GiaoDich_ma_tai_khoan_nhan_tien",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_callback_payload_json",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_loai_giao_dich",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_request_payload_json",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_response_payload_json",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_trang_thai",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "loai_hoa_don",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "ma_hoa_don_code",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "nguoi_tao",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "callback_payload_json",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "checkout_url",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "chu_thich",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ma_tai_khoan_nhan_tien",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ma_tham_chieu_noi_bo",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ngay_het_han",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ngay_thanh_toan",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "nha_cung_cap_thanh_toan",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "noi_dung_chuyen_khoan",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "qr_payload",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "qr_url",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "request_payload_json",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.RenameColumn(
                name: "response_payload_json",
                schema: "dbo",
                table: "GiaoDich",
                newName: "du_lieu_phan_hoi");

            migrationBuilder.RenameIndex(
                name: "UQ_GiaoDich_ma_tham_chieu_cong",
                schema: "dbo",
                table: "GiaoDich",
                newName: "UQ_GiaoDich_1");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "chua_thanh_toan",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "chua_thanh_toan");

            migrationBuilder.AlterColumn<decimal>(
                name: "giam_tru",
                schema: "dbo",
                table: "HoaDon",
                type: "decimal(15,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "loai_giao_dich",
                schema: "dbo",
                table: "GiaoDich",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung",
                sql: "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh', N'sieu_quan_tri', N'quan_tri_co_so', N'quan_tri_co_so_con', N'chu_tich', N'hoidong_quanly_noidung')");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_ma_hoc_sinh",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "UQ_HoaDon_1",
                schema: "dbo",
                table: "HoaDon",
                column: "ma_giao_dich_cong",
                unique: true,
                filter: "[ma_giao_dich_cong] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_da_thanh_toan_3",
                schema: "dbo",
                table: "HoaDon",
                sql: "[da_thanh_toan] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_giam_tru_2",
                schema: "dbo",
                table: "HoaDon",
                sql: "[giam_tru] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_phuong_thuc_tt_5",
                schema: "dbo",
                table: "HoaDon",
                sql: "[phuong_thuc_tt] IN (N'vnpay', N'momo', N'chuyen_khoan')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_so_tien_1",
                schema: "dbo",
                table: "HoaDon",
                sql: "[so_tien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoaDon_trang_thai_4",
                schema: "dbo",
                table: "HoaDon",
                sql: "[trang_thai] IN (N'chua_thanh_toan', N'mot_phan', N'da_thanh_toan', N'qua_han', N'dang_xu_ly', N'that_bai')");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_ma_hoa_don",
                schema: "dbo",
                table: "GiaoDich",
                column: "ma_hoa_don");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_du_lieu_phan_hoi_ISJSON",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[du_lieu_phan_hoi] IS NULL OR ISJSON([du_lieu_phan_hoi]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_loai_giao_dich_2",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[loai_giao_dich] IN (N'thanh_toan', N'ghi_co', N'ghi_no', N'hoan_tien')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_so_tien_1",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[so_tien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_trang_thai_3",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[trang_thai] IN (N'dang_xu_ly', N'thanh_cong', N'that_bai', N'da_huy')");
        }
    }
}
