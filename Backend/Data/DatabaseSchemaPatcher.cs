using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DatabaseSchemaPatcher
{
    public static async Task PatchMissingColumnsAsync(ApplicationDbContext context)
    {
        var sqlCommands = new[]
        {
            // PhienThiHocSinh missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'lan_thu')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [lan_thu] int NOT NULL CONSTRAINT DF_PhienThiHocSinh_lan_thu DEFAULT 1;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'han_nop_luc')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [han_nop_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'so_cau_dung')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [so_cau_dung] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'ket_qua_dat')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [ket_qua_dat] bit NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'de_thi_snapshot_json')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [de_thi_snapshot_json] nvarchar(max) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [ngay_cap_nhat] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'ma_ca_thi')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [ma_ca_thi] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'trang_thai_ky_ten')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [trang_thai_ky_ten] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'thoi_diem_ky')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [thoi_diem_ky] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'nguoi_xac_nhan_ky_ten')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [nguoi_xac_nhan_ky_ten] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PhienThiHocSinh]') AND name = N'trang_thai_cong_bo')
                ALTER TABLE [dbo].[PhienThiHocSinh] ADD [trang_thai_cong_bo] nvarchar(50) NULL;",

            // YeuCauMoKhoaDiemDanh missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauMoKhoaDiemDanh]') AND name = N'ghi_chu')
                ALTER TABLE [dbo].[YeuCauMoKhoaDiemDanh] ADD [ghi_chu] nvarchar(500) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauMoKhoaDiemDanh]') AND name = N'ly_do_tu_choi')
                ALTER TABLE [dbo].[YeuCauMoKhoaDiemDanh] ADD [ly_do_tu_choi] nvarchar(500) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauMoKhoaDiemDanh]') AND name = N'thoi_gian_xu_ly')
                ALTER TABLE [dbo].[YeuCauMoKhoaDiemDanh] ADD [thoi_gian_xu_ly] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauMoKhoaDiemDanh]') AND name = N'mo_khoa_den_luc')
                ALTER TABLE [dbo].[YeuCauMoKhoaDiemDanh] ADD [mo_khoa_den_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauMoKhoaDiemDanh]') AND name = N'nguoi_duyet')
                ALTER TABLE [dbo].[YeuCauMoKhoaDiemDanh] ADD [nguoi_duyet] int NULL;",

            // DeKiemTra missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'cau_hinh_de_thi')
                ALTER TABLE [dbo].[DeKiemTra] ADD [cau_hinh_de_thi] nvarchar(max) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'ty_le_trac_nghiem')
                ALTER TABLE [dbo].[DeKiemTra] ADD [ty_le_trac_nghiem] decimal(5,2) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'ty_le_tu_luan')
                ALTER TABLE [dbo].[DeKiemTra] ADD [ty_le_tu_luan] decimal(5,2) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'hinh_thuc_thi')
                ALTER TABLE [dbo].[DeKiemTra] ADD [hinh_thuc_thi] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'trang_thai_duyet')
                ALTER TABLE [dbo].[DeKiemTra] ADD [trang_thai_duyet] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DeKiemTra]') AND name = N'ma_nguoi_duyet')
                ALTER TABLE [dbo].[DeKiemTra] ADD [ma_nguoi_duyet] int NULL;",

            // KyThi missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KyThi]') AND name = N'ma_nganh')
                ALTER TABLE [dbo].[KyThi] ADD [ma_nganh] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KyThi]') AND name = N'loai_ky_thi')
                ALTER TABLE [dbo].[KyThi] ADD [loai_ky_thi] nvarchar(50) NULL;",

            // CauHinhDiemMonHoc
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHinhDiemMonHoc]') AND name = N'ti_le_chuyen_can_toi_thieu')
                ALTER TABLE [dbo].[CauHinhDiemMonHoc] ADD [ti_le_chuyen_can_toi_thieu] decimal(5,2) NULL;",

            // BuoiHoc missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'ma_giao_vien_day_thay')
                ALTER TABLE [dbo].[BuoiHoc] ADD [ma_giao_vien_day_thay] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'trang_thai_diem_danh')
                ALTER TABLE [dbo].[BuoiHoc] ADD [trang_thai_diem_danh] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'diem_danh_han_gui_luc')
                ALTER TABLE [dbo].[BuoiHoc] ADD [diem_danh_han_gui_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'diem_danh_han_chinh_sua_luc')
                ALTER TABLE [dbo].[BuoiHoc] ADD [diem_danh_han_chinh_sua_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'diem_danh_khoa_luc')
                ALTER TABLE [dbo].[BuoiHoc] ADD [diem_danh_khoa_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuoiHoc]') AND name = N'so_lan_mo_khoa_chinh_sua')
                ALTER TABLE [dbo].[BuoiHoc] ADD [so_lan_mo_khoa_chinh_sua] int NOT NULL CONSTRAINT DF_BuoiHoc_so_lan_mo_khoa DEFAULT 0;",

            // BaiTap missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BaiTap]') AND name = N'loai_tap_tin_cho_phep')
                ALTER TABLE [dbo].[BaiTap] ADD [loai_tap_tin_cho_phep] nvarchar(200) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BaiTap]') AND name = N'dung_luong_toi_da_mb')
                ALTER TABLE [dbo].[BaiTap] ADD [dung_luong_toi_da_mb] int NULL;",

            // ThietBiPhong missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThietBiPhong]') AND name = N'ma_code_thiet_bi')
                ALTER TABLE [dbo].[ThietBiPhong] ADD [ma_code_thiet_bi] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThietBiPhong]') AND name = N'chung_loai')
                ALTER TABLE [dbo].[ThietBiPhong] ADD [chung_loai] nvarchar(100) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThietBiPhong]') AND name = N'tinh_trang')
                ALTER TABLE [dbo].[ThietBiPhong] ADD [tinh_trang] nvarchar(50) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThietBiPhong]') AND name = N'ngay_kiem_dinh')
                ALTER TABLE [dbo].[ThietBiPhong] ADD [ngay_kiem_dinh] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThietBiPhong]') AND name = N'ghi_chu')
                ALTER TABLE [dbo].[ThietBiPhong] ADD [ghi_chu] nvarchar(500) NULL;",

            // TienDoBaiHoc missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TienDoBaiHoc]') AND name = N'ghi_chu')
                ALTER TABLE [dbo].[TienDoBaiHoc] ADD [ghi_chu] nvarchar(500) NULL;",

            // YeuCauXuatDuLieu table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'YeuCauXuatDuLieu')
                CREATE TABLE [dbo].[YeuCauXuatDuLieu] (
                    [ma_yeu_cau_xuat] int IDENTITY(1,1) NOT NULL,
                    [loai_du_lieu] nvarchar(50) NOT NULL,
                    [dinh_dang] nvarchar(10) NOT NULL,
                    [pham_vi_du_lieu] nvarchar(max) NULL,
                    [tham_so_loc_json] nvarchar(max) NULL,
                    [trang_thai] nvarchar(30) NOT NULL,
                    [tien_do] int NOT NULL DEFAULT 0,
                    [file_url] nvarchar(500) NULL,
                    [file_size] bigint NULL,
                    [tong_so_dong] int NOT NULL DEFAULT 0,
                    [ma_nguoi_yeu_cau] int NOT NULL,
                    [ma_don_vi] int NULL,
                    [thoi_gian_yeu_cau] datetime2 NOT NULL,
                    [thoi_gian_bat_dau] datetime2 NULL,
                    [thoi_gian_hoan_thanh] datetime2 NULL,
                    [thoi_gian_het_han] datetime2 NULL,
                    [thong_bao_loi] nvarchar(max) NULL,
                    [lan_thu_lai] int NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_YeuCauXuatDuLieu] PRIMARY KEY CLUSTERED ([ma_yeu_cau_xuat] ASC)
                );"
        };

        foreach (var sql in sqlCommands)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseSchemaPatcher] Warning executing patch: {ex.Message}");
            }
        }
    }
}
