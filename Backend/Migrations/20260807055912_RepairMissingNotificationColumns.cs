using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingNotificationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.ThongBao', N'ma_nhom_thong_bao') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [ma_nhom_thong_bao] uniqueidentifier NOT NULL
                        CONSTRAINT [DF_ThongBao_ma_nhom_thong_bao_Repair] DEFAULT NEWID() WITH VALUES;
                IF COL_LENGTH(N'dbo.ThongBao', N'loai_thong_bao') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [loai_thong_bao] nvarchar(100) NOT NULL
                        CONSTRAINT [DF_ThongBao_loai_thong_bao_Repair] DEFAULT N'manual' WITH VALUES;
                IF COL_LENGTH(N'dbo.ThongBao', N'tom_tat') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [tom_tat] nvarchar(1000) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'tom_tat_noi_dung') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [tom_tat_noi_dung] nvarchar(1000) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'noi_dung_json') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [noi_dung_json] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'noi_dung_text') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [noi_dung_text] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'muc_do') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [muc_do] nvarchar(30) NOT NULL
                        CONSTRAINT [DF_ThongBao_muc_do_Repair] DEFAULT N'info' WITH VALUES;
                IF COL_LENGTH(N'dbo.ThongBao', N'doi_tuong_lien_ket') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [doi_tuong_lien_ket] nvarchar(100) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'loai_doi_tuong_lien_ket') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [loai_doi_tuong_lien_ket] nvarchar(100) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'ma_doi_tuong_lien_ket') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [ma_doi_tuong_lien_ket] int NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'pham_vi_gui') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [pham_vi_gui] nvarchar(50) NOT NULL
                        CONSTRAINT [DF_ThongBao_pham_vi_gui_Repair] DEFAULT N'nguoi_dung' WITH VALUES;
                IF COL_LENGTH(N'dbo.ThongBao', N'duong_dan') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [duong_dan] nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'nguoi_tao') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [nguoi_tao] int NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'trang_thai') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [trang_thai] nvarchar(30) NOT NULL
                        CONSTRAINT [DF_ThongBao_trang_thai_Repair] DEFAULT N'da_gui' WITH VALUES;
                IF COL_LENGTH(N'dbo.ThongBao', N'doc_luc') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [doc_luc] datetime2 NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'gui_luc') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [gui_luc] datetime2 NULL;
                IF COL_LENGTH(N'dbo.ThongBao', N'ngay_cap_nhat') IS NULL
                    ALTER TABLE [dbo].[ThongBao] ADD [ngay_cap_nhat] datetime2 NULL;

                EXEC(N'UPDATE [dbo].[ThongBao]
                    SET [gui_luc] = COALESCE([gui_luc], [ngay_tao]),
                        [noi_dung_text] = COALESCE([noi_dung_text], [noi_dung]),
                        [tom_tat_noi_dung] = COALESCE([tom_tat_noi_dung], [tom_tat]);');

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ThongBao]') AND name = N'IX_ThongBao_MaNhomThongBao')
                    EXEC(N'CREATE INDEX [IX_ThongBao_MaNhomThongBao] ON [dbo].[ThongBao] ([ma_nhom_thong_bao]);');
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ThongBao]') AND name = N'IX_ThongBao_DonVi_Loai_GuiLuc')
                    EXEC(N'CREATE INDEX [IX_ThongBao_DonVi_Loai_GuiLuc] ON [dbo].[ThongBao] ([ma_don_vi], [loai_thong_bao], [gui_luc]);');
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[ThongBao]') AND name = N'FK_ThongBao_nguoi_tao__NguoiDung')
                    EXEC(N'ALTER TABLE [dbo].[ThongBao] ADD CONSTRAINT [FK_ThongBao_nguoi_tao__NguoiDung]
                        FOREIGN KEY ([nguoi_tao]) REFERENCES [dbo].[NguoiDung] ([ma_nguoi_dung]);');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không xóa các cột trong migration sửa drift vì trên môi trường đúng
            // chúng là schema hợp lệ đã tồn tại từ các migration trước.
        }
    }
}
