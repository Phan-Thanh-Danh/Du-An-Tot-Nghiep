using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingNotificationRecipientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[ThongBaoNguoiNhan]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ThongBaoNguoiNhan]
                    (
                        [ma_thong_bao_nguoi_nhan] int IDENTITY(1,1) NOT NULL,
                        [ma_thong_bao] int NOT NULL,
                        [ma_nguoi_nhan] int NOT NULL,
                        [ma_don_vi] int NOT NULL,
                        [da_doc] bit NOT NULL CONSTRAINT [DF_ThongBaoNguoiNhan_da_doc] DEFAULT CAST(0 AS bit),
                        [doc_luc] datetime2 NULL,
                        [da_an] bit NOT NULL CONSTRAINT [DF_ThongBaoNguoiNhan_da_an] DEFAULT CAST(0 AS bit),
                        [an_luc] datetime2 NULL,
                        [nhan_luc] datetime2 NOT NULL CONSTRAINT [DF_ThongBaoNguoiNhan_nhan_luc] DEFAULT SYSUTCDATETIME(),
                        [ngay_tao] datetime2 NOT NULL CONSTRAINT [DF_ThongBaoNguoiNhan_ngay_tao] DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT [PK_ThongBaoNguoiNhan] PRIMARY KEY ([ma_thong_bao_nguoi_nhan]),
                        CONSTRAINT [FK_ThongBaoNguoiNhan_ma_thong_bao__ThongBao]
                            FOREIGN KEY ([ma_thong_bao]) REFERENCES [dbo].[ThongBao] ([ma_thong_bao]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ThongBaoNguoiNhan_ma_nguoi_nhan__NguoiDung]
                            FOREIGN KEY ([ma_nguoi_nhan]) REFERENCES [dbo].[NguoiDung] ([ma_nguoi_dung]),
                        CONSTRAINT [FK_ThongBaoNguoiNhan_ma_don_vi__DonVi]
                            FOREIGN KEY ([ma_don_vi]) REFERENCES [dbo].[DonVi] ([ma_don_vi])
                    );

                    CREATE UNIQUE INDEX [UQ_ThongBaoNguoiNhan_ThongBao_NguoiNhan]
                        ON [dbo].[ThongBaoNguoiNhan] ([ma_thong_bao], [ma_nguoi_nhan]);
                    CREATE INDEX [IX_ThongBaoNguoiNhan_NguoiNhan_DaDoc_DaAn_NhanLuc]
                        ON [dbo].[ThongBaoNguoiNhan] ([ma_nguoi_nhan], [da_doc], [da_an], [nhan_luc]);
                    CREATE INDEX [IX_ThongBaoNguoiNhan_DonVi_NhanLuc]
                        ON [dbo].[ThongBaoNguoiNhan] ([ma_don_vi], [nhan_luc]);
                    CREATE INDEX [IX_ThongBaoNguoiNhan_MaThongBao]
                        ON [dbo].[ThongBaoNguoiNhan] ([ma_thong_bao]);

                    INSERT INTO [dbo].[ThongBaoNguoiNhan]
                        ([ma_thong_bao], [ma_nguoi_nhan], [ma_don_vi], [da_doc], [doc_luc], [da_an], [an_luc], [nhan_luc], [ngay_tao])
                    SELECT
                        [ma_thong_bao], [ma_nguoi_nhan], [ma_don_vi], [da_doc], NULL,
                        CAST(0 AS bit), NULL, [ngay_tao], [ngay_tao]
                    FROM [dbo].[ThongBao];
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không drop bảng trong migration sửa drift: bảng có thể đã tồn tại
            // hợp lệ trước khi Up chạy trên một môi trường khác.
        }
    }
}
