using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRecipientState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.AddColumn<string>(
                name: "duong_dan",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "gui_luc",
                schema: "dbo",
                table: "ThongBao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loai_doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loai_thong_bao",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "manual");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "ThongBao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pham_vi_gui",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "nguoi_dung");

            migrationBuilder.AddColumn<string>(
                name: "tom_tat_noi_dung",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThongBaoNguoiNhan",
                schema: "dbo",
                columns: table => new
                {
                    ma_thong_bao_nguoi_nhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_thong_bao = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_nhan = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    da_doc = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    doc_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    da_an = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    an_luc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nhan_luc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
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
                });

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[ThongBao]
                SET
                    [loai_thong_bao] = CASE
                        WHEN [loai_su_kien] IN (
                            N'thong_bao_chung', N'hoc_phi', N'bao_tri', N'co_so_vat_chat', N'hoc_vu', N'khan_cap',
                            N'system', N'manual', N'schedule_changed', N'session_cancelled',
                            N'attendance_unlock_approved', N'attendance_unlock_rejected'
                        ) THEN [loai_su_kien]
                        ELSE N'system'
                    END,
                    [tom_tat_noi_dung] = [tom_tat],
                    [loai_doi_tuong_lien_ket] = [doi_tuong_lien_ket],
                    [gui_luc] = [ngay_tao],
                    [pham_vi_gui] = N'nguoi_dung'
                WHERE [loai_thong_bao] = N'manual'
                   OR [tom_tat_noi_dung] IS NULL
                   OR [gui_luc] IS NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [dbo].[ThongBaoNguoiNhan]
                    ([ma_thong_bao], [ma_nguoi_nhan], [ma_don_vi], [da_doc], [doc_luc], [da_an], [an_luc], [nhan_luc], [ngay_tao])
                SELECT
                    tb.[ma_thong_bao],
                    tb.[ma_nguoi_nhan],
                    tb.[ma_don_vi],
                    tb.[da_doc],
                    tb.[doc_luc],
                    CAST(0 AS bit),
                    NULL,
                    tb.[ngay_tao],
                    tb.[ngay_tao]
                FROM [dbo].[ThongBao] tb
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [dbo].[ThongBaoNguoiNhan] tbn
                    WHERE tbn.[ma_thong_bao] = tb.[ma_thong_bao]
                      AND tbn.[ma_nguoi_nhan] = tb.[ma_nguoi_nhan]
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_DonVi_Loai_GuiLuc",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_don_vi", "loai_thong_bao", "gui_luc" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_loai_thong_bao",
                schema: "dbo",
                table: "ThongBao",
                sql: "[loai_thong_bao] IN (N'thong_bao_chung', N'hoc_phi', N'bao_tri', N'co_so_vat_chat', N'hoc_vu', N'khan_cap', N'system', N'manual', N'schedule_changed', N'session_cancelled', N'attendance_unlock_approved', N'attendance_unlock_rejected')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao",
                sql: "[muc_do] IN (N'thong_tin', N'quan_trong', N'khan_cap', N'info', N'warning', N'important')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_pham_vi_gui",
                schema: "dbo",
                table: "ThongBao",
                sql: "[pham_vi_gui] IN (N'toan_he_thong', N'don_vi', N'lop_hanh_chinh', N'vai_tro', N'nguoi_dung', N'khoa_hoc', N'users', N'class', N'course', N'campus')");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBaoNguoiNhan",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_DonVi_Loai_GuiLuc",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_loai_thong_bao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_pham_vi_gui",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "duong_dan",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "gui_luc",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "loai_doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "loai_thong_bao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "pham_vi_gui",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "tom_tat_noi_dung",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao",
                sql: "[muc_do] IN (N'info', N'warning', N'important')");
        }
    }
}
