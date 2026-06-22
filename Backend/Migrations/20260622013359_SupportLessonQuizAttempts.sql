BEGIN TRANSACTION;
DROP INDEX [UQ_PhienThiHocSinh_1] ON [dbo].[PhienThiHocSinh];

ALTER TABLE [dbo].[DeKiemTra] DROP CONSTRAINT [CK_DeKiemTra_loai_de_thi];

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [de_thi_snapshot_json] nvarchar(max) NULL;

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [han_nop_luc] datetime2 NULL;

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [ket_qua_dat] bit NULL;

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [lan_thu] int NOT NULL DEFAULT 1;

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [ngay_cap_nhat] datetime2 NULL;

ALTER TABLE [dbo].[PhienThiHocSinh] ADD [so_cau_dung] int NULL;

CREATE UNIQUE INDEX [UQ_PhienThiHocSinh_De_HocSinh_LanThu] ON [dbo].[PhienThiHocSinh] ([ma_de_kiem_tra], [ma_hoc_sinh], [lan_thu]);

ALTER TABLE [dbo].[PhienThiHocSinh] ADD CONSTRAINT [CK_PhienThiHocSinh_de_thi_snapshot_json_ISJSON] CHECK ([de_thi_snapshot_json] IS NULL OR ISJSON([de_thi_snapshot_json]) = 1);

ALTER TABLE [dbo].[PhienThiHocSinh] ADD CONSTRAINT [CK_PhienThiHocSinh_lan_thu] CHECK ([lan_thu] > 0);

ALTER TABLE [dbo].[PhienThiHocSinh] ADD CONSTRAINT [CK_PhienThiHocSinh_so_cau_dung] CHECK ([so_cau_dung] IS NULL OR [so_cau_dung] >= 0);

ALTER TABLE [dbo].[DeKiemTra] ADD CONSTRAINT [CK_DeKiemTra_loai_de_thi] CHECK ([loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop', N'quiz_bai_hoc'));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622013359_SupportLessonQuizAttempts', N'10.0.6');

COMMIT;
GO

