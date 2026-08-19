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
            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `lan_thu`  int NOT NULL DEFAULT 1",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `han_nop_luc`  datetime NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `so_cau_dung`  int NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `ket_qua_dat`  bit NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `de_thi_snapshot_json`  longtext NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `ngay_cap_nhat`  datetime NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `ma_ca_thi`  int NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `trang_thai_ky_ten`  varchar(50) NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `thoi_diem_ky`  datetime NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `nguoi_xac_nhan_ky_ten`  int NULL",

            @"ALTER TABLE `PhienThiHocSinh` ADD IF NOT EXISTS `trang_thai_cong_bo`  varchar(50) NULL",

            // YeuCauMoKhoaDiemDanh missing columns
            @"ALTER TABLE `YeuCauMoKhoaDiemDanh` ADD IF NOT EXISTS `ghi_chu`  varchar(500) NULL",

            @"ALTER TABLE `YeuCauMoKhoaDiemDanh` ADD IF NOT EXISTS `ly_do_tu_choi`  varchar(500) NULL",

            @"ALTER TABLE `YeuCauMoKhoaDiemDanh` ADD IF NOT EXISTS `thoi_gian_xu_ly`  datetime NULL",

            @"ALTER TABLE `YeuCauMoKhoaDiemDanh` ADD IF NOT EXISTS `mo_khoa_den_luc`  datetime NULL",

            @"ALTER TABLE `YeuCauMoKhoaDiemDanh` ADD IF NOT EXISTS `nguoi_duyet`  int NULL",

            // DeKiemTra missing columns
            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `cau_hinh_de_thi`  longtext NULL",

            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `ty_le_trac_nghiem`  decimal(5,2) NULL",

            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `ty_le_tu_luan`  decimal(5,2) NULL",

            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `hinh_thuc_thi`  varchar(50) NULL",

            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `trang_thai_duyet`  varchar(50) NULL",

            @"ALTER TABLE `DeKiemTra` ADD IF NOT EXISTS `ma_nguoi_duyet`  int NULL",

            // KyThi missing columns
            @"ALTER TABLE `KyThi` ADD IF NOT EXISTS `ma_nganh`  int NULL",

            @"ALTER TABLE `KyThi` ADD IF NOT EXISTS `loai_ky_thi`  varchar(50) NULL",

            // CauHinhDiemMonHoc
            @"ALTER TABLE `CauHinhDiemMonHoc` ADD IF NOT EXISTS `ti_le_chuyen_can_toi_thieu`  decimal(5,2) NULL",

            // BuoiHoc missing columns
            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `ma_giao_vien_day_thay`  int NULL",

            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `trang_thai_diem_danh`  varchar(50) NULL",

            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `diem_danh_han_gui_luc`  datetime NULL",

            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `diem_danh_han_chinh_sua_luc`  datetime NULL",

            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `diem_danh_khoa_luc`  datetime NULL",

            @"ALTER TABLE `BuoiHoc` ADD IF NOT EXISTS `so_lan_mo_khoa_chinh_sua`  int NOT NULL DEFAULT 0",

            // BaiTap missing columns
            @"ALTER TABLE `BaiTap` ADD IF NOT EXISTS `loai_tap_tin_cho_phep`  varchar(200) NULL",

            @"ALTER TABLE `BaiTap` ADD IF NOT EXISTS `dung_luong_toi_da_mb`  int NULL"
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
