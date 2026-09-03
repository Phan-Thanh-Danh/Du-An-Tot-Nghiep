using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DatabaseSchemaPatcher
{
    public static async Task PatchMissingColumnsAsync(ApplicationDbContext context)
    {
        var sqlCommands = new[]
        {
            // DanhMucMonHoc missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DanhMucMonHoc]') AND name = N'ma_chuyen_nganh')
                ALTER TABLE [dbo].[DanhMucMonHoc] ADD [ma_chuyen_nganh] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DanhMucMonHoc]') AND name = N'ma_nganh')
                ALTER TABLE [dbo].[DanhMucMonHoc] ADD [ma_nganh] int NULL;",

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

            // BaiHocNoiDung missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BaiHocNoiDung]') AND name = N'ma_de_kiem_tra')
                ALTER TABLE [dbo].[BaiHocNoiDung] ADD [ma_de_kiem_tra] int NULL;",

            // CauHoi missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHoi]') AND name = N'kieu_lua_chon')
                ALTER TABLE [dbo].[CauHoi] ADD [kieu_lua_chon] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHoi]') AND name = N'giai_thich_dap_an')
                ALTER TABLE [dbo].[CauHoi] ADD [giai_thich_dap_an] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHoi]') AND name = N'con_hoat_dong')
                ALTER TABLE [dbo].[CauHoi] ADD [con_hoat_dong] bit NOT NULL CONSTRAINT DF_CauHoi_con_hoat_dong DEFAULT 1;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHoi]') AND name = N'ngay_tao')
                ALTER TABLE [dbo].[CauHoi] ADD [ngay_tao] datetime2 NOT NULL CONSTRAINT DF_CauHoi_ngay_tao DEFAULT SYSUTCDATETIME();",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CauHoi]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[CauHoi] ADD [ngay_cap_nhat] datetime2 NULL;",

            // DonTu row_version
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'row_version')
                ALTER TABLE [dbo].[DonTu] ADD [row_version] rowversion;",

            // HoSoKyLuat missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ma_hoc_ky')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ma_hoc_ky] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'tieu_de')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [tieu_de] nvarchar(255) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'muc_do_vi_pham')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [muc_do_vi_pham] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'hinh_thuc_xu_ly')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [hinh_thuc_xu_ly] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'trang_thai')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [trang_thai] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'can_cu_xu_ly')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [can_cu_xu_ly] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ghi_chu_noi_bo')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ghi_chu_noi_bo] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ly_do_huy')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ly_do_huy] nvarchar(500) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'nguoi_huy')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [nguoi_huy] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_huy')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_huy] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_vi_pham')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_vi_pham] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_hieu_luc')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_hieu_luc] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_het_hieu_luc')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_het_hieu_luc] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'nguoi_duyet')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [nguoi_duyet] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_duyet')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_duyet] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ly_do_tu_choi')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ly_do_tu_choi] nvarchar(500) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ghi_chu_duyet')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ghi_chu_duyet] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'chung_tu_json')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [chung_tu_json] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'da_go_ky_luat')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [da_go_ky_luat] bit NOT NULL CONSTRAINT DF_HoSoKyLuat_da_go DEFAULT 0;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ly_do_go_ky_luat')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ly_do_go_ky_luat] nvarchar(500) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'nguoi_go_ky_luat')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [nguoi_go_ky_luat] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_go_ky_luat')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_go_ky_luat] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'nguoi_ap_dung')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [nguoi_ap_dung] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_ap_dung')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_ap_dung] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ngay_cap_nhat] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'loai_doi_tuong_lien_ket')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [loai_doi_tuong_lien_ket] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoSoKyLuat]') AND name = N'ma_doi_tuong_lien_ket')
                ALTER TABLE [dbo].[HoSoKyLuat] ADD [ma_doi_tuong_lien_ket] int NULL;",

            // HoaDon missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoaDon]') AND name = N'ly_do_huy')
                ALTER TABLE [dbo].[HoaDon] ADD [ly_do_huy] nvarchar(500) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoaDon]') AND name = N'ngay_huy')
                ALTER TABLE [dbo].[HoaDon] ADD [ngay_huy] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HoaDon]') AND name = N'nguoi_huy')
                ALTER TABLE [dbo].[HoaDon] ADD [nguoi_huy] int NULL;",

            // KhenThuong missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ma_don_vi')
                ALTER TABLE [dbo].[KhenThuong] ADD [ma_don_vi] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ma_dot_khen_thuong')
                ALTER TABLE [dbo].[KhenThuong] ADD [ma_dot_khen_thuong] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ma_mau_bang_khen')
                ALTER TABLE [dbo].[KhenThuong] ADD [ma_mau_bang_khen] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'trang_thai')
                ALTER TABLE [dbo].[KhenThuong] ADD [trang_thai] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'diem_xet')
                ALTER TABLE [dbo].[KhenThuong] ADD [diem_xet] decimal(5,2) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'xep_hang')
                ALTER TABLE [dbo].[KhenThuong] ADD [xep_hang] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'url_pdf_bang_khen')
                ALTER TABLE [dbo].[KhenThuong] ADD [url_pdf_bang_khen] nvarchar(1000) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ngay_sinh_pdf')
                ALTER TABLE [dbo].[KhenThuong] ADD [ngay_sinh_pdf] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'loi_sinh_pdf')
                ALTER TABLE [dbo].[KhenThuong] ADD [loi_sinh_pdf] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'so_lan_sinh_pdf')
                ALTER TABLE [dbo].[KhenThuong] ADD [so_lan_sinh_pdf] int NOT NULL CONSTRAINT DF_KhenThuong_so_lan_pdf DEFAULT 0;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ho_ten_snapshot')
                ALTER TABLE [dbo].[KhenThuong] ADD [ho_ten_snapshot] nvarchar(255) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'mssv_snapshot')
                ALTER TABLE [dbo].[KhenThuong] ADD [mssv_snapshot] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ten_hoc_ky_snapshot')
                ALTER TABLE [dbo].[KhenThuong] ADD [ten_hoc_ky_snapshot] nvarchar(100) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'danh_hieu_snapshot')
                ALTER TABLE [dbo].[KhenThuong] ADD [danh_hieu_snapshot] nvarchar(255) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[KhenThuong] ADD [ngay_cap_nhat] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'nguoi_cap')
                ALTER TABLE [dbo].[KhenThuong] ADD [nguoi_cap] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'nguoi_duyet')
                ALTER TABLE [dbo].[KhenThuong] ADD [nguoi_duyet] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ly_do_huy')
                ALTER TABLE [dbo].[KhenThuong] ADD [ly_do_huy] nvarchar(500) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'nguoi_huy')
                ALTER TABLE [dbo].[KhenThuong] ADD [nguoi_huy] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ngay_huy')
                ALTER TABLE [dbo].[KhenThuong] ADD [ngay_huy] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ngay_cap')
                ALTER TABLE [dbo].[KhenThuong] ADD [ngay_cap] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ghi_chu_vong_doi')
                ALTER TABLE [dbo].[KhenThuong] ADD [ghi_chu_vong_doi] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'ma_code_xac_thuc')
                ALTER TABLE [dbo].[KhenThuong] ADD [ma_code_xac_thuc] nvarchar(100) NULL;",

            // MauBangKhen missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauBangKhen]') AND name = N'file_nen_url')
                ALTER TABLE [dbo].[MauBangKhen] ADD [file_nen_url] nvarchar(1000) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauBangKhen]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[MauBangKhen] ADD [ngay_cap_nhat] datetime2 NULL;",

            // MauThongBao missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'ma_don_vi')
                ALTER TABLE [dbo].[MauThongBao] ADD [ma_don_vi] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'ten_mau')
                ALTER TABLE [dbo].[MauThongBao] ADD [ten_mau] nvarchar(255) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'ma_mau')
                ALTER TABLE [dbo].[MauThongBao] ADD [ma_mau] nvarchar(100) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'loai_thong_bao')
                ALTER TABLE [dbo].[MauThongBao] ADD [loai_thong_bao] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'muc_do_uu_tien')
                ALTER TABLE [dbo].[MauThongBao] ADD [muc_do_uu_tien] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'doi_tuong_mac_dinh')
                ALTER TABLE [dbo].[MauThongBao] ADD [doi_tuong_mac_dinh] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'bien_cho_phep_json')
                ALTER TABLE [dbo].[MauThongBao] ADD [bien_cho_phep_json] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'dang_hoat_dong')
                ALTER TABLE [dbo].[MauThongBao] ADD [dang_hoat_dong] bit NOT NULL CONSTRAINT DF_MauThongBao_dang_hd DEFAULT 1;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'la_he_thong')
                ALTER TABLE [dbo].[MauThongBao] ADD [la_he_thong] bit NOT NULL CONSTRAINT DF_MauThongBao_la_ht DEFAULT 0;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'ngay_tao')
                ALTER TABLE [dbo].[MauThongBao] ADD [ngay_tao] datetime2 NOT NULL CONSTRAINT DF_MauThongBao_ngay_tao DEFAULT SYSUTCDATETIME();",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'nguoi_tao')
                ALTER TABLE [dbo].[MauThongBao] ADD [nguoi_tao] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[MauThongBao] ADD [ngay_cap_nhat] datetime2 NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MauThongBao]') AND name = N'nguoi_cap_nhat')
                ALTER TABLE [dbo].[MauThongBao] ADD [nguoi_cap_nhat] int NULL;",

            // NhatKyDuyetDon missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'nguon_thuc_hien')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [nguon_thuc_hien] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'trang_thai_cu')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [trang_thai_cu] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'trang_thai_moi')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [trang_thai_moi] nvarchar(50) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'ghi_chu_cong_khai')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [ghi_chu_cong_khai] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'ghi_chu_noi_bo')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [ghi_chu_noi_bo] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'snapshot_json')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [snapshot_json] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NhatKyDuyetDon]') AND name = N'hien_thi_cho_hoc_sinh')
                ALTER TABLE [dbo].[NhatKyDuyetDon] ADD [hien_thi_cho_hoc_sinh] bit NOT NULL CONSTRAINT DF_NhatKyDuyetDon_hien_thi DEFAULT 1;",

            // YeuCauHoanPhi missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'ly_do_yeu_cau')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [ly_do_yeu_cau] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'ly_do_tu_choi')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [ly_do_tu_choi] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'ghi_chu')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [ghi_chu] nvarchar(max) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'nguoi_tao')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [nguoi_tao] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'nguoi_cap_nhat')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [nguoi_cap_nhat] int NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'ngay_tao')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [ngay_tao] datetime2 NOT NULL CONSTRAINT DF_YCHoanPhi_ngay_tao DEFAULT SYSUTCDATETIME();",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[YeuCauHoanPhi]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[YeuCauHoanPhi] ADD [ngay_cap_nhat] datetime2 NULL;",

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
                );",

            // MonHocChuyenNganh table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MonHocChuyenNganh')
            BEGIN
                CREATE TABLE [dbo].[MonHocChuyenNganh] (
                    [ma_mon_hoc] int NOT NULL,
                    [ma_chuyen_nganh] int NOT NULL,
                    CONSTRAINT [PK_MonHocChuyenNganh] PRIMARY KEY CLUSTERED ([ma_mon_hoc], [ma_chuyen_nganh]),
                    CONSTRAINT [FK_MonHocChuyenNganh_ma_mon_hoc__DanhMucMonHoc] FOREIGN KEY ([ma_mon_hoc]) REFERENCES [dbo].[DanhMucMonHoc] ([ma_mon_hoc]) ON DELETE CASCADE,
                    CONSTRAINT [FK_MonHocChuyenNganh_ma_chuyen_nganh__ChuyenNganh] FOREIGN KEY ([ma_chuyen_nganh]) REFERENCES [dbo].[ChuyenNganh] ([ma_chuyen_nganh]) ON DELETE CASCADE
                );
                CREATE NONCLUSTERED INDEX [IX_MonHocChuyenNganh_MaMonHoc] ON [dbo].[MonHocChuyenNganh] ([ma_mon_hoc]);
                CREATE NONCLUSTERED INDEX [IX_MonHocChuyenNganh_MaChuyenNganh] ON [dbo].[MonHocChuyenNganh] ([ma_chuyen_nganh]);
            END;",

            // ScheduleGenerationJob table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScheduleGenerationJob')
            BEGIN
                CREATE TABLE [dbo].[ScheduleGenerationJob] (
                    [ma_job] int IDENTITY(1,1) NOT NULL,
                    [draft_id] uniqueidentifier NOT NULL,
                    [ma_don_vi] int NOT NULL,
                    [ma_hoc_ky] int NOT NULL,
                    [nguoi_yeu_cau] int NOT NULL,
                    [trang_thai] nvarchar(30) NOT NULL CONSTRAINT DF_ScheduleGenerationJob_trang_thai DEFAULT N'draft',
                    [tong_course] int NULL,
                    [so_xep_duoc] int NULL,
                    [so_khong_xep_duoc] int NULL,
                    [so_xung_dot_cung] int NULL,
                    [score] float NULL,
                    [tom_tat_json] nvarchar(max) NULL,
                    [ngay_tao] datetime2 NOT NULL CONSTRAINT DF_ScheduleGenerationJob_ngay_tao DEFAULT SYSUTCDATETIME(),
                    [ngay_xuat_ban] datetime2 NULL,
                    CONSTRAINT [PK_ScheduleGenerationJob] PRIMARY KEY CLUSTERED ([ma_job] ASC),
                    CONSTRAINT [CK_ScheduleGenerationJob_trang_thai] CHECK ([trang_thai] IN (N'draft', N'da_xuat_ban')),
                    CONSTRAINT [FK_ScheduleGenerationJob_ma_don_vi__DonVi] FOREIGN KEY ([ma_don_vi]) REFERENCES [dbo].[DonVi] ([ma_don_vi]),
                    CONSTRAINT [FK_ScheduleGenerationJob_ma_hoc_ky__HocKy] FOREIGN KEY ([ma_hoc_ky]) REFERENCES [dbo].[HocKy] ([ma_hoc_ky]),
                    CONSTRAINT [FK_ScheduleGenerationJob_nguoi_yeu_cau__NguoiDung] FOREIGN KEY ([nguoi_yeu_cau]) REFERENCES [dbo].[NguoiDung] ([ma_nguoi_dung])
                );
                CREATE UNIQUE NONCLUSTERED INDEX [UQ_ScheduleGenerationJob_DraftId] ON [dbo].[ScheduleGenerationJob] ([draft_id]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleGenerationJob_DonVi_HocKy] ON [dbo].[ScheduleGenerationJob] ([ma_don_vi], [ma_hoc_ky]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleGenerationJob_ma_hoc_ky] ON [dbo].[ScheduleGenerationJob] ([ma_hoc_ky]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleGenerationJob_nguoi_yeu_cau] ON [dbo].[ScheduleGenerationJob] ([nguoi_yeu_cau]);
            END;",

            // ScheduleDraftItem table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScheduleDraftItem')
            BEGIN
                CREATE TABLE [dbo].[ScheduleDraftItem] (
                    [ma_draft_item] int IDENTITY(1,1) NOT NULL,
                    [ma_job] int NOT NULL,
                    [ma_khoa_hoc] int NOT NULL,
                    [ma_giao_vien] int NULL,
                    [muc_do_phu_hop] int NULL,
                    [thu_trong_tuan] int NULL,
                    [ma_ca_hoc] int NULL,
                    [ma_phong] int NULL,
                    [trang_thai] nvarchar(30) NOT NULL CONSTRAINT DF_ScheduleDraftItem_trang_thai DEFAULT N'pending',
                    [score] float NULL,
                    [canh_bao_json] nvarchar(max) NULL,
                    [loi_json] nvarchar(max) NULL,
                    [ScoreBreakdownJson] nvarchar(max) NULL,
                    [LyDoGoiYJson] nvarchar(max) NULL,
                    CONSTRAINT [PK_ScheduleDraftItem] PRIMARY KEY CLUSTERED ([ma_draft_item] ASC),
                    CONSTRAINT [CK_ScheduleDraftItem_thu_trong_tuan] CHECK ([thu_trong_tuan] IS NULL OR [thu_trong_tuan] BETWEEN 1 AND 7),
                    CONSTRAINT [CK_ScheduleDraftItem_trang_thai] CHECK ([trang_thai] IN (N'pending', N'xep_duoc', N'khong_xep_duoc')),
                    CONSTRAINT [FK_ScheduleDraftItem_ma_ca_hoc__CaHoc] FOREIGN KEY ([ma_ca_hoc]) REFERENCES [dbo].[CaHoc] ([ma_ca_hoc]),
                    CONSTRAINT [FK_ScheduleDraftItem_ma_job__ScheduleGenerationJob] FOREIGN KEY ([ma_job]) REFERENCES [dbo].[ScheduleGenerationJob] ([ma_job]) ON DELETE CASCADE,
                    CONSTRAINT [FK_ScheduleDraftItem_ma_khoa_hoc__KhoaHoc] FOREIGN KEY ([ma_khoa_hoc]) REFERENCES [dbo].[KhoaHoc] ([ma_khoa_hoc]),
                    CONSTRAINT [FK_ScheduleDraftItem_ma_phong__PhongHoc] FOREIGN KEY ([ma_phong]) REFERENCES [dbo].[PhongHoc] ([ma_phong]),
                    CONSTRAINT [FK_ScheduleDraftItem_ma_giao_vien__NguoiDung] FOREIGN KEY ([ma_giao_vien]) REFERENCES [dbo].[NguoiDung] ([ma_nguoi_dung])
                );
                CREATE NONCLUSTERED INDEX [IX_ScheduleDraftItem_Job_KhoaHoc] ON [dbo].[ScheduleDraftItem] ([ma_job], [ma_khoa_hoc]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleDraftItem_ma_ca_hoc] ON [dbo].[ScheduleDraftItem] ([ma_ca_hoc]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleDraftItem_ma_khoa_hoc] ON [dbo].[ScheduleDraftItem] ([ma_khoa_hoc]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleDraftItem_ma_phong] ON [dbo].[ScheduleDraftItem] ([ma_phong]);
                CREATE NONCLUSTERED INDEX [IX_ScheduleDraftItem_ma_giao_vien] ON [dbo].[ScheduleDraftItem] ([ma_giao_vien]);
            END;",

            // ThoiKhoaBieu ma_job_nguon column
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ThoiKhoaBieu]') AND name = N'ma_job_nguon')
            BEGIN
                ALTER TABLE [dbo].[ThoiKhoaBieu] ADD [ma_job_nguon] int NULL;
                ALTER TABLE [dbo].[ThoiKhoaBieu] ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_job_nguon__ScheduleGenerationJob]
                    FOREIGN KEY ([ma_job_nguon]) REFERENCES [dbo].[ScheduleGenerationJob] ([ma_job]);
                CREATE NONCLUSTERED INDEX [IX_ThoiKhoaBieu_ma_job_nguon] ON [dbo].[ThoiKhoaBieu] ([ma_job_nguon]);
            END;",

            // MauBangKhen table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MauBangKhen')
            BEGIN
                CREATE TABLE [dbo].[MauBangKhen] (
                    [ma_mau_bang_khen] INT IDENTITY(1,1) NOT NULL,
                    [ma_don_vi] INT NULL,
                    [ten_mau] NVARCHAR(200) NOT NULL,
                    [loai_mau] NVARCHAR(50) NOT NULL,
                    [huong_giay] NVARCHAR(20) NOT NULL,
                    [chieu_rong] INT NOT NULL,
                    [chieu_cao] INT NOT NULL,
                    [cau_hinh_json] NVARCHAR(MAX) NULL,
                    [con_hoat_dong] BIT NOT NULL CONSTRAINT DF_MauBangKhen_con_hoat_dong DEFAULT 1,
                    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT DF_MauBangKhen_ngay_tao DEFAULT SYSUTCDATETIME(),
                    [nguoi_tao] INT NOT NULL,
                    CONSTRAINT [PK_MauBangKhen] PRIMARY KEY CLUSTERED ([ma_mau_bang_khen] ASC),
                    CONSTRAINT [FK_MauBangKhen_ma_don_vi__DonVi] FOREIGN KEY ([ma_don_vi]) REFERENCES [dbo].[DonVi]([ma_don_vi]),
                    CONSTRAINT [FK_MauBangKhen_nguoi_tao__NguoiDung] FOREIGN KEY ([nguoi_tao]) REFERENCES [dbo].[NguoiDung]([ma_nguoi_dung])
                );
            END;",

            // QuyDoiTinChi table
            @"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuyDoiTinChi')
            BEGIN
                CREATE TABLE [dbo].[QuyDoiTinChi] (
                    [ma_quy_doi] INT IDENTITY(1,1) NOT NULL,
                    [so_tin_chi] INT NOT NULL,
                    [so_block_hoc] INT NOT NULL,
                    [so_buoi_moi_tuan] INT NOT NULL,
                    [so_ca_moi_buoi] INT NOT NULL,
                    CONSTRAINT [PK_QuyDoiTinChi] PRIMARY KEY CLUSTERED ([ma_quy_doi] ASC)
                );
                CREATE UNIQUE NONCLUSTERED INDEX [IX_QuyDoiTinChi_SoTinChi] ON [dbo].[QuyDoiTinChi] ([so_tin_chi]);

                SET IDENTITY_INSERT [dbo].[QuyDoiTinChi] ON;
                INSERT INTO [dbo].[QuyDoiTinChi] ([ma_quy_doi], [so_tin_chi], [so_block_hoc], [so_buoi_moi_tuan], [so_ca_moi_buoi]) VALUES
                (1, 2, 1, 2, 1),
                (2, 3, 1, 3, 1),
                (3, 4, 2, 2, 1),
                (4, 5, 2, 3, 1);
                SET IDENTITY_INSERT [dbo].[QuyDoiTinChi] OFF;
            END;",

            // DonTu missing columns
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'tieu_de')
                ALTER TABLE [dbo].[DonTu] ADD [tieu_de] nvarchar(255) NOT NULL CONSTRAINT DF_DonTu_tieu_de DEFAULT N'Đơn từ';",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ma_don_vi')
                ALTER TABLE [dbo].[DonTu] ADD [ma_don_vi] int NOT NULL CONSTRAINT DF_DonTu_ma_don_vi DEFAULT 164;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ma_mau_don')
                ALTER TABLE [dbo].[DonTu] ADD [ma_mau_don] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'trang_thai_xu_ly_nghiep_vu')
                ALTER TABLE [dbo].[DonTu] ADD [trang_thai_xu_ly_nghiep_vu] nvarchar(50) NOT NULL CONSTRAINT DF_DonTu_trang_thai_xu_ly_nghiep_vu DEFAULT N'chua_xu_ly';",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'nguoi_xu_ly_cuoi')
                ALTER TABLE [dbo].[DonTu] ADD [nguoi_xu_ly_cuoi] int NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'noi_dung_yeu_cau_bo_sung')
                ALTER TABLE [dbo].[DonTu] ADD [noi_dung_yeu_cau_bo_sung] nvarchar(max) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ket_qua_xu_ly_json')
                ALTER TABLE [dbo].[DonTu] ADD [ket_qua_xu_ly_json] nvarchar(max) NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'han_xu_ly_luc')
                ALTER TABLE [dbo].[DonTu] ADD [han_xu_ly_luc] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ngay_cap_nhat')
                ALTER TABLE [dbo].[DonTu] ADD [ngay_cap_nhat] datetime2 NOT NULL CONSTRAINT DF_DonTu_ngay_cap_nhat DEFAULT SYSUTCDATETIME();",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ngay_duyet')
                ALTER TABLE [dbo].[DonTu] ADD [ngay_duyet] datetime2 NULL;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DonTu]') AND name = N'ngay_nop')
                ALTER TABLE [dbo].[DonTu] ADD [ngay_nop] datetime2 NULL;"
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
