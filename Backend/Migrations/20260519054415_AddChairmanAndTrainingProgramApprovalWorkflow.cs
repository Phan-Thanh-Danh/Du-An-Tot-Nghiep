using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddChairmanAndTrainingProgramApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'chu_tich')
                BEGIN
                    DECLARE @ChairmanRoleId int = 10;

                    IF EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_vai_tro] = @ChairmanRoleId)
                    BEGIN
                        SELECT @ChairmanRoleId = ISNULL(MAX([ma_vai_tro]), 0) + 1 FROM [dbo].[VaiTro];
                    END

                    INSERT INTO [dbo].[VaiTro] ([ma_vai_tro], [ma_code_vai_tro], [ten_vai_tro])
                    VALUES (@ChairmanRoleId, N'chu_tich', N'Chủ tịch');
                END
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_trang_thai",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_gui_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_tu_choi_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_gian_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_gian_gui_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_gian_tu_choi",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_trang_thai",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                sql: "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'rejected', N'active', N'inactive', N'archived')");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'chu_tich')
                BEGIN
                    DECLARE @ChairmanRoleId int;
                    SELECT @ChairmanRoleId = [ma_vai_tro] FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'chu_tich';

                    IF NOT EXISTS (SELECT 1 FROM [dbo].[NguoiDung] WHERE [vai_tro_chinh] = N'chu_tich')
                        AND NOT EXISTS (SELECT 1 FROM [dbo].[PhanQuyenNguoiDung] WHERE [ma_vai_tro] = @ChairmanRoleId)
                    BEGIN
                        DELETE FROM [dbo].[VaiTro] WHERE [ma_code_vai_tro] = N'chu_tich';
                    END
                END
                """);

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

            migrationBuilder.DropIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_gui_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropIndex(
                name: "IX_ChuongTrinhDaoTao_nguoi_tu_choi_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_trang_thai",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "ghi_chu_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "nguoi_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "nguoi_gui_duyet_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "nguoi_tu_choi_id",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "thoi_gian_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "thoi_gian_gui_duyet",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "thoi_gian_tu_choi",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_trang_thai",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                sql: "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");
        }
    }
}
