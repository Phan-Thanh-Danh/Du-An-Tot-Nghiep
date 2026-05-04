/*
    LMS - SQL Server 2022 DDL script - BAN DAY DU DA SUA THEO REVIEW
    Nguon chinh: LMS_SQL_Server_DDL.sql goc.
    Da tich hop cac yeu cau tu 2 file review HTML:
      - lms_sql_deep_review: 6 loi can fix + 11 diem can bo sung.
      - lms_sql_review: 4 loi can fix + 8 diem can bo sung.

    Cach dung:
      - Chay MOT FILE NAY tu dau tren SQL Server 2022/SSMS 22.
      - File tu tao database LMS_Internal_DB neu chua co.
      - Phan cuoi file tu dong sua/bo sung schema de ket qua cuoi cung dung voi review.
*/
SET NOCOUNT ON;
GO

IF DB_ID(N'LMS_Internal_DB') IS NULL
BEGIN
    CREATE DATABASE [LMS_Internal_DB];
END
GO

USE [LMS_Internal_DB];
GO

-- Tạo schema mặc định
IF SCHEMA_ID(N'dbo') IS NULL EXEC(N'CREATE SCHEMA dbo');
GO

/* XÓA DB nếu cần chạy lại từ đầu:
USE master;
ALTER DATABASE [LMS_Internal_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [LMS_Internal_DB];
GO
*/


-- =========================================================
-- 1. TẠO BẢNG
-- =========================================================

-- 1. Đơn Vị (DonVi)
CREATE TABLE dbo.[DonVi] (
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DonVi_ma_don_vi] DEFAULT NEWID(),
    [ma_don_vi_cha] UNIQUEIDENTIFIER NULL,
    [ten_don_vi] NVARCHAR(255) NOT NULL,
    [cap_don_vi] NVARCHAR(20) NOT NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_DonVi_con_hoat_dong] DEFAULT 1,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_DonVi_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_DonVi] PRIMARY KEY ([ma_don_vi]),
    CONSTRAINT [CK_DonVi_cap_don_vi_1] CHECK ([cap_don_vi] IN (N'root', N'co_so', N'co_so_con'))
);
GO

-- 2. Vai Trò (VaiTro)
CREATE TABLE dbo.[VaiTro] (
    [ma_vai_tro] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_VaiTro_ma_vai_tro] DEFAULT NEWID(),
    [ma_code_vai_tro] NVARCHAR(50) NOT NULL,
    [ten_vai_tro] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_VaiTro] PRIMARY KEY ([ma_vai_tro]),
    CONSTRAINT [UQ_VaiTro_1] UNIQUE ([ma_code_vai_tro])
);
GO

-- 3. Người Dùng (NguoiDung)
CREATE TABLE dbo.[NguoiDung] (
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NguoiDung_ma_nguoi_dung] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [ho_ten] NVARCHAR(255) NOT NULL,
    [vai_tro_chinh] NVARCHAR(50) NOT NULL,
    [ma_lop] UNIQUEIDENTIFIER NULL,
    [so_dien_thoai] NVARCHAR(15) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_NguoiDung_trang_thai] DEFAULT N'dang_nhap_lan_dau',
    [nam_nhap_hoc] INT NULL,
    [mat_khau_hash] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_NguoiDung_ngay_tao] DEFAULT SYSUTCDATETIME(),
    [lan_dang_nhap_cuoi] DATETIME2 NULL,
    CONSTRAINT [PK_NguoiDung] PRIMARY KEY ([ma_nguoi_dung]),
    CONSTRAINT [UQ_NguoiDung_1] UNIQUE ([email]),
    CONSTRAINT [CK_NguoiDung_vai_tro_chinh_1] CHECK ([vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh')),
    CONSTRAINT [CK_NguoiDung_trang_thai_2] CHECK ([trang_thai] IN (N'hoat_dong', N'bi_khoa', N'dang_nhap_lan_dau'))
);
GO

-- 4. Phân Quyền Người Dùng (PhanQuyenNguoiDung)
CREATE TABLE dbo.[PhanQuyenNguoiDung] (
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL,
    [ma_vai_tro] UNIQUEIDENTIFIER NOT NULL,
    [ngay_gan] DATETIME2 NOT NULL CONSTRAINT [DF_PhanQuyenNguoiDung_ngay_gan] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_PhanQuyenNguoiDung] PRIMARY KEY ([ma_nguoi_dung], [ma_vai_tro])
);
GO

-- 5. Token Làm Mới (TokenLamMoi)
CREATE TABLE dbo.[TokenLamMoi] (
    [ma_token_lam_moi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_TokenLamMoi_ma_token_lam_moi] DEFAULT NEWID(),
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL,
    [token_hash] NVARCHAR(128) NOT NULL,
    [het_han_luc] DATETIME2 NOT NULL,
    [thu_hoi_luc] DATETIME2 NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_TokenLamMoi_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_TokenLamMoi] PRIMARY KEY ([ma_token_lam_moi]),
    CONSTRAINT [UQ_TokenLamMoi_1] UNIQUE ([token_hash])
);
GO

-- 6. Nhật Ký Kiểm Toán (NhatKyKiemToan)
CREATE TABLE dbo.[NhatKyKiemToan] (
    [ma_kiem_toan] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NhatKyKiemToan_ma_kiem_toan] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [loai_doi_tuong] NVARCHAR(100) NOT NULL,
    [ma_doi_tuong] UNIQUEIDENTIFIER NOT NULL,
    [hanh_dong] NVARCHAR(50) NOT NULL,
    [gia_tri_cu] NVARCHAR(MAX) NULL,
    [gia_tri_moi] NVARCHAR(MAX) NULL,
    [nguoi_thay_doi] UNIQUEIDENTIFIER NULL,
    [thoi_diem_thay_doi] DATETIME2 NOT NULL CONSTRAINT [DF_NhatKyKiemToan_thoi_diem_thay_doi] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_NhatKyKiemToan] PRIMARY KEY ([ma_kiem_toan])
);
GO

-- 7. Cảnh Báo Bảo Mật (CanhBaoBaoMat)
CREATE TABLE dbo.[CanhBaoBaoMat] (
    [ma_canh_bao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CanhBaoBaoMat_ma_canh_bao] DEFAULT NEWID(),
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL,
    [diem_rui_ro] DECIMAL(5,2) NOT NULL,
    [dia_chi_ip] NVARCHAR(45) NULL,
    [thong_tin_trinh_duyet] NVARCHAR(500) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_CanhBaoBaoMat_trang_thai] DEFAULT N'mo',
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_CanhBaoBaoMat_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_CanhBaoBaoMat] PRIMARY KEY ([ma_canh_bao]),
    CONSTRAINT [CK_CanhBaoBaoMat_diem_rui_ro_1] CHECK ([diem_rui_ro] BETWEEN 0 AND 1),
    CONSTRAINT [CK_CanhBaoBaoMat_trang_thai_2] CHECK ([trang_thai] IN (N'mo', N'da_xem', N'bo_qua'))
);
GO

-- 8. Học Kỳ (HocKy)
CREATE TABLE dbo.[HocKy] (
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_HocKy_ma_hoc_ky] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_code_hoc_ky] NVARCHAR(30) NOT NULL,
    [ten_hoc_ky] NVARCHAR(100) NOT NULL,
    [ngay_bat_dau] DATE NOT NULL,
    [ngay_ket_thuc] DATE NOT NULL,
    [da_khoa] BIT NOT NULL CONSTRAINT [DF_HocKy_da_khoa] DEFAULT 0,
    [so_tin_chi_toi_da] INT NULL,
    [han_rut_mon] DATE NULL,
    CONSTRAINT [PK_HocKy] PRIMARY KEY ([ma_hoc_ky])
);
GO

-- 9. Danh Mục Môn Học (DanhMucMonHoc)
CREATE TABLE dbo.[DanhMucMonHoc] (
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DanhMucMonHoc_ma_mon_hoc] DEFAULT NEWID(),
    [ma_code_mon_hoc] NVARCHAR(50) NOT NULL,
    [ten_mon_hoc] NVARCHAR(255) NOT NULL,
    [so_tin_chi] INT NOT NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_DanhMucMonHoc_con_hoat_dong] DEFAULT 1,
    CONSTRAINT [PK_DanhMucMonHoc] PRIMARY KEY ([ma_mon_hoc]),
    CONSTRAINT [UQ_DanhMucMonHoc_1] UNIQUE ([ma_code_mon_hoc]),
    CONSTRAINT [CK_DanhMucMonHoc_so_tin_chi_1] CHECK ([so_tin_chi] > 0)
);
GO

-- 10. Môn Học Tiên Quyết (MonHocTienQuyet)
CREATE TABLE dbo.[MonHocTienQuyet] (
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_tien_quyet] UNIQUEIDENTIFIER NOT NULL,
    [diem_toi_thieu] DECIMAL(5,2) NULL,
    CONSTRAINT [PK_MonHocTienQuyet] PRIMARY KEY ([ma_mon_hoc], [ma_mon_tien_quyet]),
    CONSTRAINT [CK_MonHocTienQuyet_diem_toi_thieu_1] CHECK ([diem_toi_thieu] BETWEEN 0 AND 10)
);
GO

-- 11. Lớp Hành Chính (LopHanhChinh)
CREATE TABLE dbo.[LopHanhChinh] (
    [ma_lop] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_LopHanhChinh_ma_lop] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_code_lop] NVARCHAR(50) NOT NULL,
    [ten_lop] NVARCHAR(255) NOT NULL,
    [ma_giao_vien_chu_nhiem] UNIQUEIDENTIFIER NULL,
    [nam_nhap_hoc] INT NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_LopHanhChinh_con_hoat_dong] DEFAULT 1,
    CONSTRAINT [PK_LopHanhChinh] PRIMARY KEY ([ma_lop]),
    CONSTRAINT [UQ_LopHanhChinh_1] UNIQUE ([ma_code_lop])
);
GO

-- 12. Lớp Học Phần (LopHocPhan)
CREATE TABLE dbo.[LopHocPhan] (
    [ma_lop_hoc_phan] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_LopHocPhan_ma_lop_hoc_phan] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [ma_code_lop_hoc_phan] NVARCHAR(50) NOT NULL,
    [suc_chua] INT NOT NULL,
    [so_dang_ky_toi_thieu] INT NULL,
    [so_da_dang_ky] INT NOT NULL CONSTRAINT [DF_LopHocPhan_so_da_dang_ky] DEFAULT 0,
    [trang_thai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_LopHocPhan_trang_thai] DEFAULT N'mo',
    CONSTRAINT [PK_LopHocPhan] PRIMARY KEY ([ma_lop_hoc_phan]),
    CONSTRAINT [UQ_LopHocPhan_1] UNIQUE ([ma_code_lop_hoc_phan]),
    CONSTRAINT [CK_LopHocPhan_suc_chua_1] CHECK ([suc_chua] > 0),
    CONSTRAINT [CK_LopHocPhan_trang_thai_2] CHECK ([trang_thai] IN (N'mo', N'dong', N'cho_huy', N'da_huy'))
);
GO

-- 13. Khóa Học (KhoaHoc)
CREATE TABLE dbo.[KhoaHoc] (
    [ma_khoa_hoc] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_KhoaHoc_ma_khoa_hoc] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_giao_vien] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [mo_ta] NVARCHAR(MAX) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_KhoaHoc_trang_thai] DEFAULT N'nhap',
    [url_anh_bia] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_KhoaHoc_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_KhoaHoc] PRIMARY KEY ([ma_khoa_hoc]),
    CONSTRAINT [CK_KhoaHoc_trang_thai_1] CHECK ([trang_thai] IN (N'nhap', N'da_xuat_ban', N'luu_tru'))
);
GO

-- 14. Chương (Chuong)
CREATE TABLE dbo.[Chuong] (
    [ma_chuong] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Chuong_ma_chuong] DEFAULT NEWID(),
    [ma_khoa_hoc] UNIQUEIDENTIFIER NOT NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [thu_tu] INT NOT NULL CONSTRAINT [DF_Chuong_thu_tu] DEFAULT 0,
    [da_an] BIT NOT NULL CONSTRAINT [DF_Chuong_da_an] DEFAULT 0,
    CONSTRAINT [PK_Chuong] PRIMARY KEY ([ma_chuong])
);
GO

-- 15. Bài Học (BaiHoc)
CREATE TABLE dbo.[BaiHoc] (
    [ma_bai_hoc] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaiHoc_ma_bai_hoc] DEFAULT NEWID(),
    [ma_chuong] UNIQUEIDENTIFIER NOT NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [loai_bai_hoc] NVARCHAR(20) NOT NULL,
    [url_tap_tin] NVARCHAR(MAX) NULL,
    [thoi_luong_giay] INT NULL,
    [noi_dung_van_ban] NVARCHAR(MAX) NULL,
    [dieu_kien_mo_khoa] NVARCHAR(MAX) NULL,
    [tom_tat_ai] NVARCHAR(MAX) NULL,
    [thu_tu] INT NOT NULL CONSTRAINT [DF_BaiHoc_thu_tu] DEFAULT 0,
    [da_an] BIT NOT NULL CONSTRAINT [DF_BaiHoc_da_an] DEFAULT 0,
    CONSTRAINT [PK_BaiHoc] PRIMARY KEY ([ma_bai_hoc]),
    CONSTRAINT [CK_BaiHoc_loai_bai_hoc_1] CHECK ([loai_bai_hoc] IN (N'video', N'pdf', N'van_ban', N'trac_nghiem')),
    CONSTRAINT [CK_BaiHoc_thoi_luong_giay_2] CHECK ([thoi_luong_giay] >= 0)
);
GO

-- 16. Tiến Độ Bài Học (TienDoBaiHoc)
CREATE TABLE dbo.[TienDoBaiHoc] (
    [ma_tien_do] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_TienDoBaiHoc_ma_tien_do] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_bai_hoc] UNIQUEIDENTIFIER NOT NULL,
    [phan_tram_tien_do] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_TienDoBaiHoc_phan_tram_tien_do] DEFAULT 0,
    [lan_gui_nhip_tim_cuoi] DATETIME2 NULL,
    [hoan_thanh_luc] DATETIME2 NULL,
    CONSTRAINT [PK_TienDoBaiHoc] PRIMARY KEY ([ma_tien_do]),
    CONSTRAINT [UQ_TienDoBaiHoc_1] UNIQUE ([ma_hoc_sinh], [ma_bai_hoc]),
    CONSTRAINT [CK_TienDoBaiHoc_phan_tram_tien_do_1] CHECK ([phan_tram_tien_do] BETWEEN 0 AND 100)
);
GO

-- 17. Bài Tập (BaiTap)
CREATE TABLE dbo.[BaiTap] (
    [ma_bai_tap] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaiTap_ma_bai_tap] DEFAULT NEWID(),
    [ma_khoa_hoc] UNIQUEIDENTIFIER NOT NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [mo_ta] NVARCHAR(MAX) NULL,
    [han_nop] DATETIME2 NOT NULL,
    [so_lan_nop_toi_da] INT NOT NULL CONSTRAINT [DF_BaiTap_so_lan_nop_toi_da] DEFAULT 3,
    [dinh_dang_cho_phep] NVARCHAR(200) NOT NULL,
    [huong_dan_cham_diem] NVARCHAR(MAX) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_BaiTap_trang_thai] DEFAULT N'nhap',
    CONSTRAINT [PK_BaiTap] PRIMARY KEY ([ma_bai_tap]),
    CONSTRAINT [CK_BaiTap_so_lan_nop_toi_da_1] CHECK ([so_lan_nop_toi_da] > 0),
    CONSTRAINT [CK_BaiTap_trang_thai_2] CHECK ([trang_thai] IN (N'nhap', N'da_xuat_ban', N'da_dong'))
);
GO

-- 18. Bài Nộp (BaiNop)
CREATE TABLE dbo.[BaiNop] (
    [ma_bai_nop] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaiNop_ma_bai_nop] DEFAULT NEWID(),
    [ma_bai_tap] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [url_tap_tin] NVARCHAR(MAX) NOT NULL,
    [so_lan_nop] INT NOT NULL,
    [nop_tre] BIT NOT NULL CONSTRAINT [DF_BaiNop_nop_tre] DEFAULT 0,
    [diem_dao_van] DECIMAL(5,2) NULL,
    [diem_so] DECIMAL(5,2) NULL,
    [diem_ai_de_xuat] DECIMAL(5,2) NULL,
    [nhan_xet] NVARCHAR(MAX) NULL,
    [thoi_diem_nop] DATETIME2 NOT NULL CONSTRAINT [DF_BaiNop_thoi_diem_nop] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BaiNop] PRIMARY KEY ([ma_bai_nop]),
    CONSTRAINT [UQ_BaiNop_1] UNIQUE ([ma_bai_tap], [ma_hoc_sinh], [so_lan_nop]),
    CONSTRAINT [CK_BaiNop_so_lan_nop_1] CHECK ([so_lan_nop] > 0),
    CONSTRAINT [CK_BaiNop_diem_dao_van_2] CHECK ([diem_dao_van] BETWEEN 0 AND 100),
    CONSTRAINT [CK_BaiNop_diem_so_3] CHECK ([diem_so] BETWEEN 0 AND 10),
    CONSTRAINT [CK_BaiNop_diem_ai_de_xuat_4] CHECK ([diem_ai_de_xuat] BETWEEN 0 AND 10)
);
GO

-- 19. Cảnh Báo Đạo Văn (CanhBaoDaoVan)
CREATE TABLE dbo.[CanhBaoDaoVan] (
    [ma_canh_bao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CanhBaoDaoVan_ma_canh_bao] DEFAULT NEWID(),
    [ma_bai_nop] UNIQUEIDENTIFIER NOT NULL,
    [diem_dao_van] DECIMAL(5,2) NOT NULL,
    [chi_tiet] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_CanhBaoDaoVan_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_CanhBaoDaoVan] PRIMARY KEY ([ma_canh_bao]),
    CONSTRAINT [CK_CanhBaoDaoVan_diem_dao_van_1] CHECK ([diem_dao_van] BETWEEN 0 AND 100)
);
GO

-- 20. Câu Hỏi (CauHoi)
CREATE TABLE dbo.[CauHoi] (
    [ma_cau_hoi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CauHoi_ma_cau_hoi] DEFAULT NEWID(),
    [ma_mon_hoc] UNIQUEIDENTIFIER NULL,
    [nguoi_tao] UNIQUEIDENTIFIER NULL,
    [loai_cau_hoi] NVARCHAR(20) NOT NULL,
    [noi_dung] NVARCHAR(MAX) NOT NULL,
    [lua_chon] NVARCHAR(MAX) NULL,
    [dap_an_dung] NVARCHAR(MAX) NULL,
    [do_kho] NVARCHAR(10) NOT NULL,
    CONSTRAINT [PK_CauHoi] PRIMARY KEY ([ma_cau_hoi]),
    CONSTRAINT [CK_CauHoi_loai_cau_hoi_1] CHECK ([loai_cau_hoi] IN (N'trac_nghiem', N'tu_luan')),
    CONSTRAINT [CK_CauHoi_do_kho_2] CHECK ([do_kho] IN (N'de', N'trung_binh', N'kho'))
);
GO

-- 21. Đề Kiểm Tra (DeKiemTra)
CREATE TABLE dbo.[DeKiemTra] (
    [ma_de_kiem_tra] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DeKiemTra_ma_de_kiem_tra] DEFAULT NEWID(),
    [ma_mon_hoc] UNIQUEIDENTIFIER NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [thoi_gian_phut] INT NOT NULL,
    [cau_hinh_de_thi] NVARCHAR(MAX) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_DeKiemTra_trang_thai] DEFAULT N'nhap',
    CONSTRAINT [PK_DeKiemTra] PRIMARY KEY ([ma_de_kiem_tra]),
    CONSTRAINT [CK_DeKiemTra_thoi_gian_phut_1] CHECK ([thoi_gian_phut] BETWEEN 1 AND 240),
    CONSTRAINT [CK_DeKiemTra_trang_thai_2] CHECK ([trang_thai] IN (N'nhap', N'da_len_lich', N'dang_mo', N'da_dong', N'da_cong_bo'))
);
GO

-- 22. Câu Hỏi Đề Kiểm Tra (CauHoiDeKiemTra)
CREATE TABLE dbo.[CauHoiDeKiemTra] (
    [ma_de_kiem_tra] UNIQUEIDENTIFIER NOT NULL,
    [ma_cau_hoi] UNIQUEIDENTIFIER NOT NULL,
    [diem_so] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_CauHoiDeKiemTra_diem_so] DEFAULT 1,
    [thu_tu] INT NULL,
    CONSTRAINT [PK_CauHoiDeKiemTra] PRIMARY KEY ([ma_de_kiem_tra], [ma_cau_hoi])
);
GO

-- 23. Phiên Thi Học Sinh (PhienThiHocSinh)
CREATE TABLE dbo.[PhienThiHocSinh] (
    [ma_phien_thi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_PhienThiHocSinh_ma_phien_thi] DEFAULT NEWID(),
    [ma_de_kiem_tra] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [bat_dau_luc] DATETIME2 NULL,
    [nop_luc] DATETIME2 NULL,
    [cau_tra_loi_json] NVARCHAR(MAX) NULL,
    [nhat_ky_vi_pham] NVARCHAR(MAX) NULL,
    [sao_luu_cuc_bo] NVARCHAR(MAX) NULL,
    [trang_thai_luong] NVARCHAR(20) NOT NULL CONSTRAINT [DF_PhienThiHocSinh_trang_thai_luong] DEFAULT N'dang_hoat_dong',
    [diem_tu_dong] DECIMAL(5,2) NULL,
    [diem_cuoi_cung] DECIMAL(5,2) NULL,
    CONSTRAINT [PK_PhienThiHocSinh] PRIMARY KEY ([ma_phien_thi]),
    CONSTRAINT [UQ_PhienThiHocSinh_1] UNIQUE ([ma_de_kiem_tra], [ma_hoc_sinh]),
    CONSTRAINT [CK_PhienThiHocSinh_trang_thai_luong_1] CHECK ([trang_thai_luong] IN (N'dang_hoat_dong', N'bi_gian_doan', N'da_dung'))
);
GO

-- 24. Phòng Học (PhongHoc)
CREATE TABLE dbo.[PhongHoc] (
    [ma_phong] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_PhongHoc_ma_phong] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_code_phong] NVARCHAR(50) NOT NULL,
    [ten_phong] NVARCHAR(255) NOT NULL,
    [suc_chua] INT NOT NULL,
    [loai_phong] NVARCHAR(30) NOT NULL,
    [trang_thai_phong] NVARCHAR(20) NOT NULL CONSTRAINT [DF_PhongHoc_trang_thai_phong] DEFAULT N'hoat_dong',
    CONSTRAINT [PK_PhongHoc] PRIMARY KEY ([ma_phong]),
    CONSTRAINT [UQ_PhongHoc_1] UNIQUE ([ma_code_phong]),
    CONSTRAINT [CK_PhongHoc_suc_chua_1] CHECK ([suc_chua] > 0),
    CONSTRAINT [CK_PhongHoc_loai_phong_2] CHECK ([loai_phong] IN (N'ly_thuyet', N'phong_thi_nghiem', N'hoi_truong', N'truc_tuyen', N'khac')),
    CONSTRAINT [CK_PhongHoc_trang_thai_phong_3] CHECK ([trang_thai_phong] IN (N'hoat_dong', N'bao_tri', N'ngung_hoat_dong'))
);
GO

-- 25. Thiết Bị Phòng (ThietBiPhong)
CREATE TABLE dbo.[ThietBiPhong] (
    [ma_thiet_bi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ThietBiPhong_ma_thiet_bi] DEFAULT NEWID(),
    [ma_phong] UNIQUEIDENTIFIER NOT NULL,
    [ten_thiet_bi] NVARCHAR(255) NOT NULL,
    [so_luong] INT NOT NULL CONSTRAINT [DF_ThietBiPhong_so_luong] DEFAULT 1,
    CONSTRAINT [PK_ThietBiPhong] PRIMARY KEY ([ma_thiet_bi]),
    CONSTRAINT [CK_ThietBiPhong_so_luong_1] CHECK ([so_luong] >= 0)
);
GO

-- 26. Thời Khóa Biểu (ThoiKhoaBieu)
CREATE TABLE dbo.[ThoiKhoaBieu] (
    [ma_tkb] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ThoiKhoaBieu_ma_tkb] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_giao_vien] UNIQUEIDENTIFIER NOT NULL,
    [ma_giao_vien_day_thay] UNIQUEIDENTIFIER NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_phong] UNIQUEIDENTIFIER NOT NULL,
    [ma_lop] UNIQUEIDENTIFIER NOT NULL,
    [ma_lop_hoc_phan] UNIQUEIDENTIFIER NULL,
    [thu_trong_tuan] INT NOT NULL,
    [gio_bat_dau] TIME NOT NULL,
    [gio_ket_thuc] TIME NOT NULL,
    [duong_dan_hop] NVARCHAR(MAX) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ThoiKhoaBieu_trang_thai] DEFAULT N'nhap',
    [bu_cho_buoi] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_ThoiKhoaBieu] PRIMARY KEY ([ma_tkb]),
    CONSTRAINT [UQ_ThoiKhoaBieu_1] UNIQUE ([ma_giao_vien], [thu_trong_tuan], [gio_bat_dau]),
    CONSTRAINT [UQ_ThoiKhoaBieu_2] UNIQUE ([ma_phong], [thu_trong_tuan], [gio_bat_dau]),
    CONSTRAINT [CK_ThoiKhoaBieu_thu_trong_tuan_1] CHECK ([thu_trong_tuan] BETWEEN 1 AND 7),
    CONSTRAINT [CK_ThoiKhoaBieu_gio_ket_thuc_2] CHECK ([gio_ket_thuc] > [gio_bat_dau]),
    CONSTRAINT [CK_ThoiKhoaBieu_trang_thai_3] CHECK ([trang_thai] IN (N'nhap', N'da_duyet', N'da_xuat_ban', N'da_huy'))
);
GO

-- 27. Buổi Học (BuoiHoc)
CREATE TABLE dbo.[BuoiHoc] (
    [ma_buoi_hoc] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BuoiHoc_ma_buoi_hoc] DEFAULT NEWID(),
    [ma_tkb] UNIQUEIDENTIFIER NOT NULL,
    [ngay_hoc] DATE NOT NULL,
    [gio_bat_dau] TIME NOT NULL,
    [gio_ket_thuc] TIME NOT NULL,
    [trang_thai_buoi] NVARCHAR(20) NOT NULL CONSTRAINT [DF_BuoiHoc_trang_thai_buoi] DEFAULT N'chua_xac_nhan',
    [khoa_luc] DATETIME2 NULL,
    CONSTRAINT [PK_BuoiHoc] PRIMARY KEY ([ma_buoi_hoc]),
    CONSTRAINT [CK_BuoiHoc_trang_thai_buoi_1] CHECK ([trang_thai_buoi] IN (N'da_xac_nhan', N'chua_xac_nhan', N'yeu_cau_mo_khoa', N'da_huy'))
);
GO

-- 28. Điểm Danh (DiemDanh)
CREATE TABLE dbo.[DiemDanh] (
    [ma_diem_danh] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DiemDanh_ma_diem_danh] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_buoi_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL,
    [nguoi_ghi_nhan] UNIQUEIDENTIFIER NOT NULL,
    [ghi_nhan_luc] DATETIME2 NOT NULL CONSTRAINT [DF_DiemDanh_ghi_nhan_luc] DEFAULT SYSUTCDATETIME(),
    [khoa_luc] DATETIME2 NULL,
    [he_so_vang] INT NOT NULL,
    [ma_yc_mo_khoa] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_DiemDanh] PRIMARY KEY ([ma_diem_danh]),
    CONSTRAINT [UQ_DiemDanh_1] UNIQUE ([ma_buoi_hoc], [ma_hoc_sinh]),
    CONSTRAINT [CK_DiemDanh_trang_thai_1] CHECK ([trang_thai] IN (N'co_mat', N'vang', N'di_muon', N'co_phep')),
    CONSTRAINT [CK_DiemDanh_he_so_vang_2] CHECK ([he_so_vang] >= 0)
);
GO

-- 29. Yêu Cầu Mở Khóa Điểm Danh (YeuCauMoKhoaDiemDanh)
CREATE TABLE dbo.[YeuCauMoKhoaDiemDanh] (
    [ma_yc_mo_khoa] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_YeuCauMoKhoaDiemDanh_ma_yc_mo_khoa] DEFAULT NEWID(),
    [ma_buoi_hoc] UNIQUEIDENTIFIER NOT NULL,
    [nguoi_yeu_cau] UNIQUEIDENTIFIER NOT NULL,
    [ly_do] NVARCHAR(MAX) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_YeuCauMoKhoaDiemDanh_trang_thai] DEFAULT N'cho_duyet',
    [nguoi_duyet] UNIQUEIDENTIFIER NULL,
    [mo_khoa_den_luc] DATETIME2 NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_YeuCauMoKhoaDiemDanh_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_YeuCauMoKhoaDiemDanh] PRIMARY KEY ([ma_yc_mo_khoa]),
    CONSTRAINT [UQ_YeuCauMoKhoaDiemDanh_1] UNIQUE ([ma_buoi_hoc]),
    CONSTRAINT [CK_YeuCauMoKhoaDiemDanh_trang_thai_1] CHECK ([trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han'))
);
GO

-- 30. Báo Cáo Rủi Ro Vắng (BaoCaoRuiRoVang)
CREATE TABLE dbo.[BaoCaoRuiRoVang] (
    [ma_bao_cao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaoCaoRuiRoVang_ma_bao_cao] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NULL,
    [diem_rui_ro] DECIMAL(5,2) NOT NULL,
    [dac_trung_json] NVARCHAR(MAX) NULL,
    [tao_luc] DATETIME2 NOT NULL CONSTRAINT [DF_BaoCaoRuiRoVang_tao_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BaoCaoRuiRoVang] PRIMARY KEY ([ma_bao_cao]),
    CONSTRAINT [CK_BaoCaoRuiRoVang_diem_rui_ro_1] CHECK ([diem_rui_ro] BETWEEN 0 AND 1)
);
GO

-- 31. Cấu Hình Điểm Môn Học (CauHinhDiemMonHoc)
CREATE TABLE dbo.[CauHinhDiemMonHoc] (
    [ma_cau_hinh_diem] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CauHinhDiemMonHoc_ma_cau_hinh_diem] DEFAULT NEWID(),
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [trong_so_qua_trinh] DECIMAL(5,2) NOT NULL,
    [trong_so_giua_ky] DECIMAL(5,2) NOT NULL,
    [trong_so_cuoi_ky] DECIMAL(5,2) NOT NULL,
    [nguong_dat] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_CauHinhDiemMonHoc_nguong_dat] DEFAULT 5,
    CONSTRAINT [PK_CauHinhDiemMonHoc] PRIMARY KEY ([ma_cau_hinh_diem]),
    CONSTRAINT [CK_CauHinhDiemMonHoc_trong_so_qua_trinh_1] CHECK ([trong_so_qua_trinh] BETWEEN 0 AND 100),
    CONSTRAINT [CK_CauHinhDiemMonHoc_trong_so_giua_ky_2] CHECK ([trong_so_giua_ky] BETWEEN 0 AND 100),
    CONSTRAINT [CK_CauHinhDiemMonHoc_trong_so_cuoi_ky_3] CHECK ([trong_so_cuoi_ky] BETWEEN 0 AND 100),
    CONSTRAINT [CK_CauHinhDiemMonHoc_nguong_dat_4] CHECK ([nguong_dat] BETWEEN 0 AND 10)
);
GO

-- 32. Điểm Số (DiemSo)
CREATE TABLE dbo.[DiemSo] (
    [ma_diem_so] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DiemSo_ma_diem_so] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [diem_qua_trinh] DECIMAL(5,2) NULL,
    [diem_giua_ky] DECIMAL(5,2) NULL,
    [diem_cuoi_ky] DECIMAL(5,2) NULL,
    [gpa_mon_hoc] DECIMAL(5,2) NOT NULL CONSTRAINT [DF_DiemSo_gpa_mon_hoc] DEFAULT 0,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_DiemSo_trang_thai] DEFAULT N'chua_hoan_thanh',
    [da_khoa] BIT NOT NULL CONSTRAINT [DF_DiemSo_da_khoa] DEFAULT 0,
    [ly_do_rot] NVARCHAR(MAX) NULL,
    [nam_nhap_hoc] INT NOT NULL,
    CONSTRAINT [PK_DiemSo] PRIMARY KEY ([ma_diem_so]),
    CONSTRAINT [UQ_DiemSo_1] UNIQUE ([ma_hoc_sinh], [ma_mon_hoc], [ma_hoc_ky]),
    CONSTRAINT [CK_DiemSo_diem_qua_trinh_1] CHECK ([diem_qua_trinh] BETWEEN 0 AND 10),
    CONSTRAINT [CK_DiemSo_diem_giua_ky_2] CHECK ([diem_giua_ky] BETWEEN 0 AND 10),
    CONSTRAINT [CK_DiemSo_diem_cuoi_ky_3] CHECK ([diem_cuoi_ky] BETWEEN 0 AND 10),
    CONSTRAINT [CK_DiemSo_gpa_mon_hoc_4] CHECK ([gpa_mon_hoc] BETWEEN 0 AND 10),
    CONSTRAINT [CK_DiemSo_trang_thai_5] CHECK ([trang_thai] IN (N'dat', N'rot', N'chua_hoan_thanh', N'cho_hoan_thanh_bo_sung'))
);
GO

-- 33. Nhật Ký Thay Đổi Điểm (NhatKyThayDoiDiem)
CREATE TABLE dbo.[NhatKyThayDoiDiem] (
    [ma_nk_thay_doi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NhatKyThayDoiDiem_ma_nk_thay_doi] DEFAULT NEWID(),
    [ma_diem_so] UNIQUEIDENTIFIER NOT NULL,
    [nguoi_thay_doi] UNIQUEIDENTIFIER NOT NULL,
    [gia_tri_cu] NVARCHAR(MAX) NOT NULL,
    [gia_tri_moi] NVARCHAR(MAX) NOT NULL,
    [ly_do] NVARCHAR(MAX) NOT NULL,
    [nguoi_duyet] UNIQUEIDENTIFIER NULL,
    [thay_doi_luc] DATETIME2 NOT NULL CONSTRAINT [DF_NhatKyThayDoiDiem_thay_doi_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_NhatKyThayDoiDiem] PRIMARY KEY ([ma_nk_thay_doi])
);
GO

-- 34. Yêu Cầu Sửa Điểm (YeuCauSuaDiem)
CREATE TABLE dbo.[YeuCauSuaDiem] (
    [ma_yc_sua_diem] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_YeuCauSuaDiem_ma_yc_sua_diem] DEFAULT NEWID(),
    [ma_diem_so] UNIQUEIDENTIFIER NOT NULL,
    [nguoi_yeu_cau] UNIQUEIDENTIFIER NOT NULL,
    [ly_do] NVARCHAR(MAX) NOT NULL,
    [url_bang_chung] NVARCHAR(MAX) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_YeuCauSuaDiem_trang_thai] DEFAULT N'cho_duyet',
    [nguoi_duyet] UNIQUEIDENTIFIER NULL,
    [mo_den_luc] DATETIME2 NULL,
    CONSTRAINT [PK_YeuCauSuaDiem] PRIMARY KEY ([ma_yc_sua_diem]),
    CONSTRAINT [CK_YeuCauSuaDiem_trang_thai_1] CHECK ([trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han'))
);
GO

-- 35. Báo Cáo Rủi Ro Rớt Môn (BaoCaoRuiRoRotMon)
CREATE TABLE dbo.[BaoCaoRuiRoRotMon] (
    [ma_bao_cao_rot] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaoCaoRuiRoRotMon_ma_bao_cao_rot] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [xac_suat_rot_mon] DECIMAL(5,2) NOT NULL,
    [dac_trung_json] NVARCHAR(MAX) NULL,
    [tao_luc] DATETIME2 NOT NULL CONSTRAINT [DF_BaoCaoRuiRoRotMon_tao_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BaoCaoRuiRoRotMon] PRIMARY KEY ([ma_bao_cao_rot]),
    CONSTRAINT [CK_BaoCaoRuiRoRotMon_xac_suat_rot_mon_1] CHECK ([xac_suat_rot_mon] BETWEEN 0 AND 1)
);
GO

-- 36. Bình Luận (BinhLuan)
CREATE TABLE dbo.[BinhLuan] (
    [ma_binh_luan] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BinhLuan_ma_binh_luan] DEFAULT NEWID(),
    [ma_bai_hoc] UNIQUEIDENTIFIER NOT NULL,
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL,
    [noi_dung] NVARCHAR(MAX) NOT NULL,
    [giay_trong_video] INT NULL,
    [so_trang_pdf] INT NULL,
    [ma_binh_luan_cha] UNIQUEIDENTIFIER NULL,
    [da_ghim] BIT NOT NULL CONSTRAINT [DF_BinhLuan_da_ghim] DEFAULT 0,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_BinhLuan_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BinhLuan] PRIMARY KEY ([ma_binh_luan]),
    CONSTRAINT [CK_BinhLuan_giay_trong_video_1] CHECK ([giay_trong_video] >= 0),
    CONSTRAINT [CK_BinhLuan_so_trang_pdf_2] CHECK ([so_trang_pdf] > 0)
);
GO

-- 37. Khen Thưởng (KhenThuong)
CREATE TABLE dbo.[KhenThuong] (
    [ma_khen_thuong] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_KhenThuong_ma_khen_thuong] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [loai_khen_thuong] NVARCHAR(30) NOT NULL,
    [gpa_dat_duoc] DECIMAL(5,2) NULL,
    [url_chung_tu] NVARCHAR(MAX) NOT NULL,
    [cap_luc] DATETIME2 NOT NULL CONSTRAINT [DF_KhenThuong_cap_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_KhenThuong] PRIMARY KEY ([ma_khen_thuong]),
    CONSTRAINT [CK_KhenThuong_loai_khen_thuong_1] CHECK ([loai_khen_thuong] IN (N'hoc_luc', N'dac_biet', N'thi_dau')),
    CONSTRAINT [CK_KhenThuong_gpa_dat_duoc_2] CHECK ([gpa_dat_duoc] BETWEEN 0 AND 10)
);
GO

-- 38. Cấu Hình Khen Thưởng (CauHinhKhenThuong)
CREATE TABLE dbo.[CauHinhKhenThuong] (
    [ma_cau_hinh_kt] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CauHinhKhenThuong_ma_cau_hinh_kt] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [loai_khen_thuong] NVARCHAR(30) NOT NULL,
    [gpa_toi_thieu] DECIMAL(5,2) NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_CauHinhKhenThuong_con_hoat_dong] DEFAULT 1,
    CONSTRAINT [PK_CauHinhKhenThuong] PRIMARY KEY ([ma_cau_hinh_kt]),
    CONSTRAINT [CK_CauHinhKhenThuong_gpa_toi_thieu_1] CHECK ([gpa_toi_thieu] BETWEEN 0 AND 10)
);
GO

-- 39. Hồ Sơ Kỷ Luật (HoSoKyLuat)
CREATE TABLE dbo.[HoSoKyLuat] (
    [ma_ky_luat] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_HoSoKyLuat_ma_ky_luat] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [loai_ky_luat] NVARCHAR(50) NOT NULL,
    [mo_ta] NVARCHAR(MAX) NOT NULL,
    [nguoi_tao] UNIQUEIDENTIFIER NOT NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_HoSoKyLuat_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_HoSoKyLuat] PRIMARY KEY ([ma_ky_luat])
);
GO

-- 40. Hóa Đơn (HoaDon)
CREATE TABLE dbo.[HoaDon] (
    [ma_hoa_don] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_HoaDon_ma_hoa_don] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NULL,
    [so_tien] DECIMAL(15,2) NOT NULL,
    [giam_tru] DECIMAL(15,2) NULL CONSTRAINT [DF_HoaDon_giam_tru] DEFAULT 0,
    [da_thanh_toan] DECIMAL(15,2) NOT NULL CONSTRAINT [DF_HoaDon_da_thanh_toan] DEFAULT 0,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_HoaDon_trang_thai] DEFAULT N'chua_thanh_toan',
    [phuong_thuc_tt] NVARCHAR(20) NULL,
    [ma_giao_dich_cong] NVARCHAR(100) NULL,
    [bat_dau_tt_luc] DATETIME2 NULL,
    [het_han_tt_luc] DATETIME2 NULL,
    [han_thanh_toan] DATE NOT NULL,
    [url_hoa_don_pdf] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_HoaDon] PRIMARY KEY ([ma_hoa_don]),
    CONSTRAINT [UQ_HoaDon_1] UNIQUE ([ma_giao_dich_cong]),
    CONSTRAINT [CK_HoaDon_so_tien_1] CHECK ([so_tien] >= 0),
    CONSTRAINT [CK_HoaDon_giam_tru_2] CHECK ([giam_tru] >= 0),
    CONSTRAINT [CK_HoaDon_da_thanh_toan_3] CHECK ([da_thanh_toan] >= 0),
    CONSTRAINT [CK_HoaDon_trang_thai_4] CHECK ([trang_thai] IN (N'chua_thanh_toan', N'mot_phan', N'da_thanh_toan', N'qua_han', N'dang_xu_ly', N'that_bai')),
    CONSTRAINT [CK_HoaDon_phuong_thuc_tt_5] CHECK ([phuong_thuc_tt] IN (N'vnpay', N'momo', N'chuyen_khoan'))
);
GO

-- 41. Giao Dịch (GiaoDich)
CREATE TABLE dbo.[GiaoDich] (
    [ma_giao_dich] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_GiaoDich_ma_giao_dich] DEFAULT NEWID(),
    [ma_hoa_don] UNIQUEIDENTIFIER NOT NULL,
    [ma_tham_chieu_cong] NVARCHAR(100) NULL,
    [so_tien] DECIMAL(15,2) NOT NULL,
    [loai_giao_dich] NVARCHAR(20) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL,
    [du_lieu_phan_hoi] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_GiaoDich_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_GiaoDich] PRIMARY KEY ([ma_giao_dich]),
    CONSTRAINT [UQ_GiaoDich_1] UNIQUE ([ma_tham_chieu_cong]),
    CONSTRAINT [CK_GiaoDich_so_tien_1] CHECK ([so_tien] >= 0),
    CONSTRAINT [CK_GiaoDich_loai_giao_dich_2] CHECK ([loai_giao_dich] IN (N'thanh_toan', N'ghi_co', N'ghi_no', N'hoan_tien')),
    CONSTRAINT [CK_GiaoDich_trang_thai_3] CHECK ([trang_thai] IN (N'dang_xu_ly', N'thanh_cong', N'that_bai', N'da_huy'))
);
GO

-- 42. Yêu Cầu Hoàn Phí (YeuCauHoanPhi)
CREATE TABLE dbo.[YeuCauHoanPhi] (
    [ma_hoan_phi] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_YeuCauHoanPhi_ma_hoan_phi] DEFAULT NEWID(),
    [ma_hoa_don] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [so_tien_yeu_cau] DECIMAL(15,2) NOT NULL,
    [loai_hoan_phi] NVARCHAR(20) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_YeuCauHoanPhi_trang_thai] DEFAULT N'cho_duyet',
    [nguoi_duyet] UNIQUEIDENTIFIER NULL,
    [xu_ly_luc] DATETIME2 NULL,
    CONSTRAINT [PK_YeuCauHoanPhi] PRIMARY KEY ([ma_hoan_phi]),
    CONSTRAINT [CK_YeuCauHoanPhi_so_tien_yeu_cau_1] CHECK ([so_tien_yeu_cau] >= 0),
    CONSTRAINT [CK_YeuCauHoanPhi_loai_hoan_phi_2] CHECK ([loai_hoan_phi] IN (N'toan_phan', N'mot_phan', N'ghi_co')),
    CONSTRAINT [CK_YeuCauHoanPhi_trang_thai_3] CHECK ([trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'da_xu_ly'))
);
GO

-- 43. Phiếu Hỗ Trợ (PhieuHoTro)
CREATE TABLE dbo.[PhieuHoTro] (
    [ma_phieu_ht] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_PhieuHoTro_ma_phieu_ht] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [danh_muc] NVARCHAR(30) NOT NULL,
    [tieu_de] NVARCHAR(255) NOT NULL,
    [mo_ta] NVARCHAR(MAX) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_PhieuHoTro_trang_thai] DEFAULT N'mo',
    [phan_cong_cho] UNIQUEIDENTIFIER NULL,
    [han_xu_ly] DATETIME2 NULL,
    [danh_gia_hai_long] INT NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_PhieuHoTro_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_PhieuHoTro] PRIMARY KEY ([ma_phieu_ht]),
    CONSTRAINT [CK_PhieuHoTro_danh_muc_1] CHECK ([danh_muc] IN (N'ky_thuat', N'hoc_vu', N'tai_chinh', N'khac')),
    CONSTRAINT [CK_PhieuHoTro_trang_thai_2] CHECK ([trang_thai] IN (N'mo', N'dang_xu_ly', N'da_giai_quyet', N'da_dong')),
    CONSTRAINT [CK_PhieuHoTro_danh_gia_hai_long_3] CHECK ([danh_gia_hai_long] BETWEEN 1 AND 5)
);
GO

-- 44. Tin Nhắn Hỗ Trợ (TinNhanHoTro)
CREATE TABLE dbo.[TinNhanHoTro] (
    [ma_tin_nhan_ht] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_TinNhanHoTro_ma_tin_nhan_ht] DEFAULT NEWID(),
    [ma_phieu_ht] UNIQUEIDENTIFIER NOT NULL,
    [ma_nguoi_gui] UNIQUEIDENTIFIER NOT NULL,
    [noi_dung] NVARCHAR(MAX) NOT NULL,
    [url_dinh_kem] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_TinNhanHoTro_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_TinNhanHoTro] PRIMARY KEY ([ma_tin_nhan_ht])
);
GO

-- 45. Câu Hỏi Thường Gặp (CauHoiThuongGap)
CREATE TABLE dbo.[CauHoiThuongGap] (
    [ma_cau_hoi_faq] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CauHoiThuongGap_ma_cau_hoi_faq] DEFAULT NEWID(),
    [danh_muc] NVARCHAR(30) NOT NULL,
    [cau_hoi] NVARCHAR(500) NOT NULL,
    [tra_loi] NVARCHAR(MAX) NOT NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_CauHoiThuongGap_con_hoat_dong] DEFAULT 1,
    CONSTRAINT [PK_CauHoiThuongGap] PRIMARY KEY ([ma_cau_hoi_faq])
);
GO

-- 46. Giai Đoạn Đăng Ký (GiaiDoanDangKy)
CREATE TABLE dbo.[GiaiDoanDangKy] (
    [ma_giai_doan_dk] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_GiaiDoanDangKy_ma_giai_doan_dk] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [bat_dau_luc] DATETIME2 NOT NULL,
    [ket_thuc_luc] DATETIME2 NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_GiaiDoanDangKy_trang_thai] DEFAULT N'nhap',
    [so_tin_chi_toi_da] INT NOT NULL,
    CONSTRAINT [PK_GiaiDoanDangKy] PRIMARY KEY ([ma_giai_doan_dk]),
    CONSTRAINT [CK_GiaiDoanDangKy_trang_thai_1] CHECK ([trang_thai] IN (N'nhap', N'dang_mo', N'da_dong'))
);
GO

-- 47. Đăng Ký Học Phần (DangKyHocPhan)
CREATE TABLE dbo.[DangKyHocPhan] (
    [ma_dang_ky] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DangKyHocPhan_ma_dang_ky] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_lop_hoc_phan] UNIQUEIDENTIFIER NOT NULL,
    [trang_thai] NVARCHAR(30) NOT NULL,
    [vi_tri_cho] INT NULL,
    [la_hoc_lai] BIT NOT NULL CONSTRAINT [DF_DangKyHocPhan_la_hoc_lai] DEFAULT 0,
    [kiem_tra_tien_quyet] BIT NOT NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_DangKyHocPhan_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_DangKyHocPhan] PRIMARY KEY ([ma_dang_ky]),
    CONSTRAINT [UQ_DangKyHocPhan_1] UNIQUE ([ma_hoc_sinh], [ma_lop_hoc_phan]),
    CONSTRAINT [CK_DangKyHocPhan_trang_thai_1] CHECK ([trang_thai] IN (N'da_dang_ky', N'danh_sach_cho', N'da_rut', N'lop_bi_huy')),
    CONSTRAINT [CK_DangKyHocPhan_vi_tri_cho_2] CHECK ([vi_tri_cho] > 0)
);
GO

-- 48. Đơn Từ (DonTu)
CREATE TABLE dbo.[DonTu] (
    [ma_don_tu] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DonTu_ma_don_tu] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [loai_don] NVARCHAR(50) NOT NULL,
    [trang_thai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_DonTu_trang_thai] DEFAULT N'nhap',
    [nguoi_duyet_hien_tai] UNIQUEIDENTIFIER NULL,
    [du_lieu_bieu_mau] NVARCHAR(MAX) NULL,
    [url_bang_chung] NVARCHAR(MAX) NULL,
    [ly_do_tu_choi] NVARCHAR(MAX) NULL,
    [nhat_ky_tu_dong] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_DonTu_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_DonTu] PRIMARY KEY ([ma_don_tu]),
    CONSTRAINT [CK_DonTu_loai_don_1] CHECK ([loai_don] IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac')),
    CONSTRAINT [CK_DonTu_trang_thai_2] CHECK ([trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'da_duyet', N'tu_choi'))
);
GO

-- 49. Nhật Ký Duyệt Đơn (NhatKyDuyetDon)
CREATE TABLE dbo.[NhatKyDuyetDon] (
    [ma_nk_duyet] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NhatKyDuyetDon_ma_nk_duyet] DEFAULT NEWID(),
    [ma_don_tu] UNIQUEIDENTIFIER NOT NULL,
    [ma_nguoi_duyet] UNIQUEIDENTIFIER NOT NULL,
    [hanh_dong] NVARCHAR(20) NOT NULL,
    [ghi_chu] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_NhatKyDuyetDon_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_NhatKyDuyetDon] PRIMARY KEY ([ma_nk_duyet]),
    CONSTRAINT [CK_NhatKyDuyetDon_hanh_dong_1] CHECK ([hanh_dong] IN (N'nop', N'phe_duyet', N'tu_choi', N'phan_cong', N'leo_thang'))
);
GO

-- 50. Câu Hỏi Đánh Giá (CauHoiDanhGia)
CREATE TABLE dbo.[CauHoiDanhGia] (
    [ma_cau_hoi_dg] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CauHoiDanhGia_ma_cau_hoi_dg] DEFAULT NEWID(),
    [noi_dung_cau_hoi] NVARCHAR(500) NOT NULL,
    [con_hoat_dong] BIT NOT NULL CONSTRAINT [DF_CauHoiDanhGia_con_hoat_dong] DEFAULT 1,
    CONSTRAINT [PK_CauHoiDanhGia] PRIMARY KEY ([ma_cau_hoi_dg])
);
GO

-- 51. Đánh Giá Giáo Viên (DanhGiaGiaoVien)
CREATE TABLE dbo.[DanhGiaGiaoVien] (
    [ma_danh_gia] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DanhGiaGiaoVien_ma_danh_gia] DEFAULT NEWID(),
    [ma_giao_vien] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [ma_cau_hoi_dg] UNIQUEIDENTIFIER NOT NULL,
    [diem_so] INT NOT NULL,
    [nhan_xet_tu_do] NVARCHAR(MAX) NULL,
    [ai_cam_xuc] NVARCHAR(20) NULL,
    [ai_chu_de] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_DanhGiaGiaoVien] PRIMARY KEY ([ma_danh_gia]),
    CONSTRAINT [CK_DanhGiaGiaoVien_diem_so_1] CHECK ([diem_so] BETWEEN 1 AND 5),
    CONSTRAINT [CK_DanhGiaGiaoVien_ai_cam_xuc_2] CHECK ([ai_cam_xuc] IN (N'tich_cuc', N'trung_tinh', N'tieu_cuc'))
);
GO

-- 52. Nộp Bài Đánh Giá (NopBaiDanhGia)
CREATE TABLE dbo.[NopBaiDanhGia] (
    [ma_nop_dg] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NopBaiDanhGia_ma_nop_dg] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_giao_vien] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NOT NULL,
    [so_lan_nop] INT NOT NULL CONSTRAINT [DF_NopBaiDanhGia_so_lan_nop] DEFAULT 1,
    [cap_nhat_luc] DATETIME2 NOT NULL CONSTRAINT [DF_NopBaiDanhGia_cap_nhat_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_NopBaiDanhGia] PRIMARY KEY ([ma_nop_dg]),
    CONSTRAINT [UQ_NopBaiDanhGia_1] UNIQUE ([ma_hoc_sinh], [ma_giao_vien], [ma_hoc_ky]),
    CONSTRAINT [CK_NopBaiDanhGia_so_lan_nop_1] CHECK ([so_lan_nop] BETWEEN 0 AND 2)
);
GO

-- 53. Liên Kết Phụ Huynh (LienKetPhuHuynh)
CREATE TABLE dbo.[LienKetPhuHuynh] (
    [ma_lien_ket_ph] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_LienKetPhuHuynh_ma_lien_ket_ph] DEFAULT NEWID(),
    [ma_phu_huynh] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [quyen_xem] NVARCHAR(MAX) NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_LienKetPhuHuynh_trang_thai] DEFAULT N'cho_duyet',
    [lien_ket_luc] DATETIME2 NULL,
    CONSTRAINT [PK_LienKetPhuHuynh] PRIMARY KEY ([ma_lien_ket_ph]),
    CONSTRAINT [UQ_LienKetPhuHuynh_1] UNIQUE ([ma_phu_huynh], [ma_hoc_sinh]),
    CONSTRAINT [CK_LienKetPhuHuynh_trang_thai_1] CHECK ([trang_thai] IN (N'cho_duyet', N'hoat_dong', N'da_thu_hoi'))
);
GO

-- 54. Mẫu Thông Báo (MauThongBao)
CREATE TABLE dbo.[MauThongBao] (
    [ma_mau_tb] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_MauThongBao_ma_mau_tb] DEFAULT NEWID(),
    [loai_su_kien] NVARCHAR(100) NOT NULL,
    [kenh_gui] NVARCHAR(20) NOT NULL,
    [mau_tieu_de] NVARCHAR(500) NULL,
    [mau_noi_dung] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_MauThongBao] PRIMARY KEY ([ma_mau_tb]),
    CONSTRAINT [UQ_MauThongBao_1] UNIQUE ([loai_su_kien], [kenh_gui]),
    CONSTRAINT [CK_MauThongBao_kenh_gui_1] CHECK ([kenh_gui] IN (N'email', N'thong_bao_day', N'sms'))
);
GO

-- 55. Thông Báo (ThongBao)
CREATE TABLE dbo.[ThongBao] (
    [ma_thong_bao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ThongBao_ma_thong_bao] DEFAULT NEWID(),
    [ma_nguoi_nhan] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [loai_su_kien] NVARCHAR(100) NOT NULL,
    [tieu_de] NVARCHAR(500) NULL,
    [noi_dung] NVARCHAR(MAX) NOT NULL,
    [da_doc] BIT NOT NULL CONSTRAINT [DF_ThongBao_da_doc] DEFAULT 0,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_ThongBao_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_ThongBao] PRIMARY KEY ([ma_thong_bao])
);
GO

-- 56. Nhật Ký Thông Báo (NhatKyThongBao)
CREATE TABLE dbo.[NhatKyThongBao] (
    [ma_nk_thong_bao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_NhatKyThongBao_ma_nk_thong_bao] DEFAULT NEWID(),
    [ma_thong_bao] UNIQUEIDENTIFIER NULL,
    [ma_nguoi_nhan] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL,
    [kenh_gui] NVARCHAR(20) NOT NULL,
    [gui_luc] DATETIME2 NULL,
    CONSTRAINT [PK_NhatKyThongBao] PRIMARY KEY ([ma_nk_thong_bao]),
    CONSTRAINT [CK_NhatKyThongBao_trang_thai_1] CHECK ([trang_thai] IN (N'cho_gui', N'da_gui', N'da_nhan', N'that_bai', N'bo_qua')),
    CONSTRAINT [CK_NhatKyThongBao_kenh_gui_2] CHECK ([kenh_gui] IN (N'email', N'thong_bao_day', N'sms'))
);
GO

-- 57. Tùy Chọn Thông Báo (TuyChonThongBao)
CREATE TABLE dbo.[TuyChonThongBao] (
    [ma_nguoi_dung] UNIQUEIDENTIFIER NOT NULL,
    [nhan_email] BIT NOT NULL CONSTRAINT [DF_TuyChonThongBao_nhan_email] DEFAULT 1,
    [nhan_push] BIT NOT NULL CONSTRAINT [DF_TuyChonThongBao_nhan_push] DEFAULT 1,
    [nhan_sms] BIT NOT NULL CONSTRAINT [DF_TuyChonThongBao_nhan_sms] DEFAULT 0,
    [cap_nhat_luc] DATETIME2 NOT NULL CONSTRAINT [DF_TuyChonThongBao_cap_nhat_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_TuyChonThongBao] PRIMARY KEY ([ma_nguoi_dung])
);
GO

-- 58. Thông Báo Hẹn Giờ (ThongBaoHenGio)
CREATE TABLE dbo.[ThongBaoHenGio] (
    [ma_tb_hen_gio] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ThongBaoHenGio_ma_tb_hen_gio] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [nguoi_tao] UNIQUEIDENTIFIER NOT NULL,
    [loai_su_kien] NVARCHAR(100) NOT NULL,
    [bo_loc_nguoi_nhan] NVARCHAR(MAX) NOT NULL,
    [gui_luc] DATETIME2 NOT NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ThongBaoHenGio_trang_thai] DEFAULT N'da_len_lich',
    CONSTRAINT [PK_ThongBaoHenGio] PRIMARY KEY ([ma_tb_hen_gio]),
    CONSTRAINT [CK_ThongBaoHenGio_trang_thai_1] CHECK ([trang_thai] IN (N'da_len_lich', N'dang_cho', N'da_huy', N'hoan_thanh'))
);
GO

-- 59. Đặt Phòng (DatPhong)
CREATE TABLE dbo.[DatPhong] (
    [ma_dat_phong] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DatPhong_ma_dat_phong] DEFAULT NEWID(),
    [ma_phong] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [nguoi_yeu_cau] UNIQUEIDENTIFIER NOT NULL,
    [muc_dich] NVARCHAR(500) NOT NULL,
    [bat_dau_luc] DATETIME2 NOT NULL,
    [ket_thuc_luc] DATETIME2 NOT NULL,
    [so_nguoi_tham_du] INT NULL,
    [trang_thai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_DatPhong_trang_thai] DEFAULT N'cho_duyet',
    [nguoi_duyet] UNIQUEIDENTIFIER NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_DatPhong_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_DatPhong] PRIMARY KEY ([ma_dat_phong]),
    CONSTRAINT [CK_DatPhong_ket_thuc_luc_1] CHECK ([ket_thuc_luc] > [bat_dau_luc]),
    CONSTRAINT [CK_DatPhong_so_nguoi_tham_du_2] CHECK ([so_nguoi_tham_du] >= 0),
    CONSTRAINT [CK_DatPhong_trang_thai_3] CHECK ([trang_thai] IN (N'cho_duyet', N'da_xac_nhan', N'tu_choi', N'da_huy'))
);
GO

-- 60. Báo Cáo Sử Dụng Phòng (BaoCaoSuDungPhong)
CREATE TABLE dbo.[BaoCaoSuDungPhong] (
    [ma_bc_su_dung_phong] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_BaoCaoSuDungPhong_ma_bc_su_dung_phong] DEFAULT NEWID(),
    [ma_phong] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [tu_ngay] DATE NOT NULL,
    [den_ngay] DATE NOT NULL,
    [so_gio_su_dung] DECIMAL(10,2) NOT NULL CONSTRAINT [DF_BaoCaoSuDungPhong_so_gio_su_dung] DEFAULT 0,
    [ti_le_su_dung] DECIMAL(5,2) NULL,
    [tao_luc] DATETIME2 NOT NULL CONSTRAINT [DF_BaoCaoSuDungPhong_tao_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BaoCaoSuDungPhong] PRIMARY KEY ([ma_bc_su_dung_phong]),
    CONSTRAINT [CK_BaoCaoSuDungPhong_ti_le_su_dung_1] CHECK ([ti_le_su_dung] BETWEEN 0 AND 100)
);
GO

-- 61. Ảnh Chụp Phân Tích (AnhChupPhanTich)
CREATE TABLE dbo.[AnhChupPhanTich] (
    [ma_anh_chup] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_AnhChupPhanTich_ma_anh_chup] DEFAULT NEWID(),
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NULL,
    [ngay_anh_chup] DATE NOT NULL,
    [loai_chi_so] NVARCHAR(50) NOT NULL,
    [gia_tri_chi_so] DECIMAL(18,4) NOT NULL,
    [chieu_loc_json] NVARCHAR(MAX) NULL,
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_AnhChupPhanTich_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_AnhChupPhanTich] PRIMARY KEY ([ma_anh_chup]),
    CONSTRAINT [UQ_AnhChupPhanTich_1] UNIQUE ([ma_don_vi], [ma_hoc_ky], [ngay_anh_chup], [loai_chi_so])
);
GO

-- 62. Danh Sách Rủi Ro Rớt Môn (DanhSachRuiRoRotMon)
CREATE TABLE dbo.[DanhSachRuiRoRotMon] (
    [ma_rui_ro_rot] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_DanhSachRuiRoRotMon_ma_rui_ro_rot] DEFAULT NEWID(),
    [ma_hoc_sinh] UNIQUEIDENTIFIER NOT NULL,
    [ma_mon_hoc] UNIQUEIDENTIFIER NULL,
    [ma_hoc_ky] UNIQUEIDENTIFIER NULL,
    [xac_suat_rot_mon] DECIMAL(5,2) NOT NULL,
    [tao_luc] DATETIME2 NOT NULL CONSTRAINT [DF_DanhSachRuiRoRotMon_tao_luc] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_DanhSachRuiRoRotMon] PRIMARY KEY ([ma_rui_ro_rot]),
    CONSTRAINT [CK_DanhSachRuiRoRotMon_xac_suat_rot_mon_1] CHECK ([xac_suat_rot_mon] BETWEEN 0 AND 1)
);
GO

-- 63. Xuất Báo Cáo (XuatBaoCao)
CREATE TABLE dbo.[XuatBaoCao] (
    [ma_xuat_bao_cao] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_XuatBaoCao_ma_xuat_bao_cao] DEFAULT NEWID(),
    [nguoi_yeu_cau] UNIQUEIDENTIFIER NOT NULL,
    [ma_don_vi] UNIQUEIDENTIFIER NOT NULL,
    [loai_bao_cao] NVARCHAR(50) NOT NULL,
    [tham_so_json] NVARCHAR(MAX) NULL,
    [url_tap_tin] NVARCHAR(MAX) NULL,
    [trang_thai] NVARCHAR(20) NOT NULL CONSTRAINT [DF_XuatBaoCao_trang_thai] DEFAULT N'cho_xu_ly',
    [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_XuatBaoCao_ngay_tao] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_XuatBaoCao] PRIMARY KEY ([ma_xuat_bao_cao]),
    CONSTRAINT [CK_XuatBaoCao_trang_thai_1] CHECK ([trang_thai] IN (N'cho_xu_ly', N'dang_xu_ly', N'hoan_thanh', N'that_bai'))
);
GO

-- =========================================================
-- 2. KHÓA NGOẠI
-- =========================================================

ALTER TABLE dbo.[DonVi] WITH CHECK
ADD CONSTRAINT [FK_DonVi_ma_don_vi_cha__DonVi]
FOREIGN KEY ([ma_don_vi_cha]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NguoiDung] WITH CHECK
ADD CONSTRAINT [FK_NguoiDung_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NguoiDung] WITH CHECK
ADD CONSTRAINT [FK_NguoiDung_ma_lop__LopHanhChinh]
FOREIGN KEY ([ma_lop]) REFERENCES dbo.[LopHanhChinh]([ma_lop])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhanQuyenNguoiDung] WITH CHECK
ADD CONSTRAINT [FK_PhanQuyenNguoiDung_ma_nguoi_dung__NguoiDung]
FOREIGN KEY ([ma_nguoi_dung]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhanQuyenNguoiDung] WITH CHECK
ADD CONSTRAINT [FK_PhanQuyenNguoiDung_ma_vai_tro__VaiTro]
FOREIGN KEY ([ma_vai_tro]) REFERENCES dbo.[VaiTro]([ma_vai_tro])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TokenLamMoi] WITH CHECK
ADD CONSTRAINT [FK_TokenLamMoi_ma_nguoi_dung__NguoiDung]
FOREIGN KEY ([ma_nguoi_dung]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyKiemToan] WITH CHECK
ADD CONSTRAINT [FK_NhatKyKiemToan_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyKiemToan] WITH CHECK
ADD CONSTRAINT [FK_NhatKyKiemToan_nguoi_thay_doi__NguoiDung]
FOREIGN KEY ([nguoi_thay_doi]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CanhBaoBaoMat] WITH CHECK
ADD CONSTRAINT [FK_CanhBaoBaoMat_ma_nguoi_dung__NguoiDung]
FOREIGN KEY ([ma_nguoi_dung]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HocKy] WITH CHECK
ADD CONSTRAINT [FK_HocKy_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[MonHocTienQuyet] WITH CHECK
ADD CONSTRAINT [FK_MonHocTienQuyet_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[MonHocTienQuyet] WITH CHECK
ADD CONSTRAINT [FK_MonHocTienQuyet_ma_mon_tien_quyet__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_tien_quyet]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LopHanhChinh] WITH CHECK
ADD CONSTRAINT [FK_LopHanhChinh_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LopHanhChinh] WITH CHECK
ADD CONSTRAINT [FK_LopHanhChinh_ma_giao_vien_chu_nhiem__NguoiDung]
FOREIGN KEY ([ma_giao_vien_chu_nhiem]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LopHocPhan] WITH CHECK
ADD CONSTRAINT [FK_LopHocPhan_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LopHocPhan] WITH CHECK
ADD CONSTRAINT [FK_LopHocPhan_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LopHocPhan] WITH CHECK
ADD CONSTRAINT [FK_LopHocPhan_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[KhoaHoc] WITH CHECK
ADD CONSTRAINT [FK_KhoaHoc_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[KhoaHoc] WITH CHECK
ADD CONSTRAINT [FK_KhoaHoc_ma_giao_vien__NguoiDung]
FOREIGN KEY ([ma_giao_vien]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[KhoaHoc] WITH CHECK
ADD CONSTRAINT [FK_KhoaHoc_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[Chuong] WITH CHECK
ADD CONSTRAINT [FK_Chuong_ma_khoa_hoc__KhoaHoc]
FOREIGN KEY ([ma_khoa_hoc]) REFERENCES dbo.[KhoaHoc]([ma_khoa_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaiHoc] WITH CHECK
ADD CONSTRAINT [FK_BaiHoc_ma_chuong__Chuong]
FOREIGN KEY ([ma_chuong]) REFERENCES dbo.[Chuong]([ma_chuong])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TienDoBaiHoc] WITH CHECK
ADD CONSTRAINT [FK_TienDoBaiHoc_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TienDoBaiHoc] WITH CHECK
ADD CONSTRAINT [FK_TienDoBaiHoc_ma_bai_hoc__BaiHoc]
FOREIGN KEY ([ma_bai_hoc]) REFERENCES dbo.[BaiHoc]([ma_bai_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaiTap] WITH CHECK
ADD CONSTRAINT [FK_BaiTap_ma_khoa_hoc__KhoaHoc]
FOREIGN KEY ([ma_khoa_hoc]) REFERENCES dbo.[KhoaHoc]([ma_khoa_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaiNop] WITH CHECK
ADD CONSTRAINT [FK_BaiNop_ma_bai_tap__BaiTap]
FOREIGN KEY ([ma_bai_tap]) REFERENCES dbo.[BaiTap]([ma_bai_tap])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaiNop] WITH CHECK
ADD CONSTRAINT [FK_BaiNop_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CanhBaoDaoVan] WITH CHECK
ADD CONSTRAINT [FK_CanhBaoDaoVan_ma_bai_nop__BaiNop]
FOREIGN KEY ([ma_bai_nop]) REFERENCES dbo.[BaiNop]([ma_bai_nop])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHoi] WITH CHECK
ADD CONSTRAINT [FK_CauHoi_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHoi] WITH CHECK
ADD CONSTRAINT [FK_CauHoi_nguoi_tao__NguoiDung]
FOREIGN KEY ([nguoi_tao]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DeKiemTra] WITH CHECK
ADD CONSTRAINT [FK_DeKiemTra_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DeKiemTra] WITH CHECK
ADD CONSTRAINT [FK_DeKiemTra_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHoiDeKiemTra] WITH CHECK
ADD CONSTRAINT [FK_CauHoiDeKiemTra_ma_de_kiem_tra__DeKiemTra]
FOREIGN KEY ([ma_de_kiem_tra]) REFERENCES dbo.[DeKiemTra]([ma_de_kiem_tra])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHoiDeKiemTra] WITH CHECK
ADD CONSTRAINT [FK_CauHoiDeKiemTra_ma_cau_hoi__CauHoi]
FOREIGN KEY ([ma_cau_hoi]) REFERENCES dbo.[CauHoi]([ma_cau_hoi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
ADD CONSTRAINT [FK_PhienThiHocSinh_ma_de_kiem_tra__DeKiemTra]
FOREIGN KEY ([ma_de_kiem_tra]) REFERENCES dbo.[DeKiemTra]([ma_de_kiem_tra])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
ADD CONSTRAINT [FK_PhienThiHocSinh_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhongHoc] WITH CHECK
ADD CONSTRAINT [FK_PhongHoc_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThietBiPhong] WITH CHECK
ADD CONSTRAINT [FK_ThietBiPhong_ma_phong__PhongHoc]
FOREIGN KEY ([ma_phong]) REFERENCES dbo.[PhongHoc]([ma_phong])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_giao_vien__NguoiDung]
FOREIGN KEY ([ma_giao_vien]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_giao_vien_day_thay__NguoiDung]
FOREIGN KEY ([ma_giao_vien_day_thay]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_phong__PhongHoc]
FOREIGN KEY ([ma_phong]) REFERENCES dbo.[PhongHoc]([ma_phong])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_lop__LopHanhChinh]
FOREIGN KEY ([ma_lop]) REFERENCES dbo.[LopHanhChinh]([ma_lop])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_ma_lop_hoc_phan__LopHocPhan]
FOREIGN KEY ([ma_lop_hoc_phan]) REFERENCES dbo.[LopHocPhan]([ma_lop_hoc_phan])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
ADD CONSTRAINT [FK_ThoiKhoaBieu_bu_cho_buoi__ThoiKhoaBieu]
FOREIGN KEY ([bu_cho_buoi]) REFERENCES dbo.[ThoiKhoaBieu]([ma_tkb])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BuoiHoc] WITH CHECK
ADD CONSTRAINT [FK_BuoiHoc_ma_tkb__ThoiKhoaBieu]
FOREIGN KEY ([ma_tkb]) REFERENCES dbo.[ThoiKhoaBieu]([ma_tkb])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemDanh] WITH CHECK
ADD CONSTRAINT [FK_DiemDanh_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemDanh] WITH CHECK
ADD CONSTRAINT [FK_DiemDanh_ma_buoi_hoc__BuoiHoc]
FOREIGN KEY ([ma_buoi_hoc]) REFERENCES dbo.[BuoiHoc]([ma_buoi_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemDanh] WITH CHECK
ADD CONSTRAINT [FK_DiemDanh_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemDanh] WITH CHECK
ADD CONSTRAINT [FK_DiemDanh_nguoi_ghi_nhan__NguoiDung]
FOREIGN KEY ([nguoi_ghi_nhan]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemDanh] WITH CHECK
ADD CONSTRAINT [FK_DiemDanh_ma_yc_mo_khoa__YeuCauMoKhoaDiemDanh]
FOREIGN KEY ([ma_yc_mo_khoa]) REFERENCES dbo.[YeuCauMoKhoaDiemDanh]([ma_yc_mo_khoa])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauMoKhoaDiemDanh] WITH CHECK
ADD CONSTRAINT [FK_YeuCauMoKhoaDiemDanh_ma_buoi_hoc__BuoiHoc]
FOREIGN KEY ([ma_buoi_hoc]) REFERENCES dbo.[BuoiHoc]([ma_buoi_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauMoKhoaDiemDanh] WITH CHECK
ADD CONSTRAINT [FK_YeuCauMoKhoaDiemDanh_nguoi_yeu_cau__NguoiDung]
FOREIGN KEY ([nguoi_yeu_cau]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauMoKhoaDiemDanh] WITH CHECK
ADD CONSTRAINT [FK_YeuCauMoKhoaDiemDanh_nguoi_duyet__NguoiDung]
FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoRuiRoVang] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoRuiRoVang_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoRuiRoVang] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoRuiRoVang_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHinhDiemMonHoc] WITH CHECK
ADD CONSTRAINT [FK_CauHinhDiemMonHoc_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHinhDiemMonHoc] WITH CHECK
ADD CONSTRAINT [FK_CauHinhDiemMonHoc_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemSo] WITH CHECK
ADD CONSTRAINT [FK_DiemSo_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemSo] WITH CHECK
ADD CONSTRAINT [FK_DiemSo_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemSo] WITH CHECK
ADD CONSTRAINT [FK_DiemSo_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DiemSo] WITH CHECK
ADD CONSTRAINT [FK_DiemSo_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThayDoiDiem] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThayDoiDiem_ma_diem_so__DiemSo]
FOREIGN KEY ([ma_diem_so]) REFERENCES dbo.[DiemSo]([ma_diem_so])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThayDoiDiem] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThayDoiDiem_nguoi_thay_doi__NguoiDung]
FOREIGN KEY ([nguoi_thay_doi]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThayDoiDiem] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThayDoiDiem_nguoi_duyet__NguoiDung]
FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauSuaDiem] WITH CHECK
ADD CONSTRAINT [FK_YeuCauSuaDiem_ma_diem_so__DiemSo]
FOREIGN KEY ([ma_diem_so]) REFERENCES dbo.[DiemSo]([ma_diem_so])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauSuaDiem] WITH CHECK
ADD CONSTRAINT [FK_YeuCauSuaDiem_nguoi_yeu_cau__NguoiDung]
FOREIGN KEY ([nguoi_yeu_cau]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauSuaDiem] WITH CHECK
ADD CONSTRAINT [FK_YeuCauSuaDiem_nguoi_duyet__NguoiDung]
FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoRuiRoRotMon_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoRuiRoRotMon_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BinhLuan] WITH CHECK
ADD CONSTRAINT [FK_BinhLuan_ma_bai_hoc__BaiHoc]
FOREIGN KEY ([ma_bai_hoc]) REFERENCES dbo.[BaiHoc]([ma_bai_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BinhLuan] WITH CHECK
ADD CONSTRAINT [FK_BinhLuan_ma_nguoi_dung__NguoiDung]
FOREIGN KEY ([ma_nguoi_dung]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BinhLuan] WITH CHECK
ADD CONSTRAINT [FK_BinhLuan_ma_binh_luan_cha__BinhLuan]
FOREIGN KEY ([ma_binh_luan_cha]) REFERENCES dbo.[BinhLuan]([ma_binh_luan])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[KhenThuong] WITH CHECK
ADD CONSTRAINT [FK_KhenThuong_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[KhenThuong] WITH CHECK
ADD CONSTRAINT [FK_KhenThuong_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[CauHinhKhenThuong] WITH CHECK
ADD CONSTRAINT [FK_CauHinhKhenThuong_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoSoKyLuat] WITH CHECK
ADD CONSTRAINT [FK_HoSoKyLuat_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoSoKyLuat] WITH CHECK
ADD CONSTRAINT [FK_HoSoKyLuat_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoSoKyLuat] WITH CHECK
ADD CONSTRAINT [FK_HoSoKyLuat_nguoi_tao__NguoiDung]
FOREIGN KEY ([nguoi_tao]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoaDon] WITH CHECK
ADD CONSTRAINT [FK_HoaDon_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoaDon] WITH CHECK
ADD CONSTRAINT [FK_HoaDon_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[HoaDon] WITH CHECK
ADD CONSTRAINT [FK_HoaDon_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[GiaoDich] WITH CHECK
ADD CONSTRAINT [FK_GiaoDich_ma_hoa_don__HoaDon]
FOREIGN KEY ([ma_hoa_don]) REFERENCES dbo.[HoaDon]([ma_hoa_don])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauHoanPhi] WITH CHECK
ADD CONSTRAINT [FK_YeuCauHoanPhi_ma_hoa_don__HoaDon]
FOREIGN KEY ([ma_hoa_don]) REFERENCES dbo.[HoaDon]([ma_hoa_don])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauHoanPhi] WITH CHECK
ADD CONSTRAINT [FK_YeuCauHoanPhi_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauHoanPhi] WITH CHECK
ADD CONSTRAINT [FK_YeuCauHoanPhi_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[YeuCauHoanPhi] WITH CHECK
ADD CONSTRAINT [FK_YeuCauHoanPhi_nguoi_duyet__NguoiDung]
FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhieuHoTro] WITH CHECK
ADD CONSTRAINT [FK_PhieuHoTro_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[PhieuHoTro] WITH CHECK
ADD CONSTRAINT [FK_PhieuHoTro_phan_cong_cho__NguoiDung]
FOREIGN KEY ([phan_cong_cho]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TinNhanHoTro] WITH CHECK
ADD CONSTRAINT [FK_TinNhanHoTro_ma_phieu_ht__PhieuHoTro]
FOREIGN KEY ([ma_phieu_ht]) REFERENCES dbo.[PhieuHoTro]([ma_phieu_ht])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TinNhanHoTro] WITH CHECK
ADD CONSTRAINT [FK_TinNhanHoTro_ma_nguoi_gui__NguoiDung]
FOREIGN KEY ([ma_nguoi_gui]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[GiaiDoanDangKy] WITH CHECK
ADD CONSTRAINT [FK_GiaiDoanDangKy_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[GiaiDoanDangKy] WITH CHECK
ADD CONSTRAINT [FK_GiaiDoanDangKy_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DangKyHocPhan] WITH CHECK
ADD CONSTRAINT [FK_DangKyHocPhan_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DangKyHocPhan] WITH CHECK
ADD CONSTRAINT [FK_DangKyHocPhan_ma_lop_hoc_phan__LopHocPhan]
FOREIGN KEY ([ma_lop_hoc_phan]) REFERENCES dbo.[LopHocPhan]([ma_lop_hoc_phan])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DonTu] WITH CHECK
ADD CONSTRAINT [FK_DonTu_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DonTu] WITH CHECK
ADD CONSTRAINT [FK_DonTu_nguoi_duyet_hien_tai__NguoiDung]
FOREIGN KEY ([nguoi_duyet_hien_tai]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyDuyetDon] WITH CHECK
ADD CONSTRAINT [FK_NhatKyDuyetDon_ma_don_tu__DonTu]
FOREIGN KEY ([ma_don_tu]) REFERENCES dbo.[DonTu]([ma_don_tu])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyDuyetDon] WITH CHECK
ADD CONSTRAINT [FK_NhatKyDuyetDon_ma_nguoi_duyet__NguoiDung]
FOREIGN KEY ([ma_nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhGiaGiaoVien] WITH CHECK
ADD CONSTRAINT [FK_DanhGiaGiaoVien_ma_giao_vien__NguoiDung]
FOREIGN KEY ([ma_giao_vien]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhGiaGiaoVien] WITH CHECK
ADD CONSTRAINT [FK_DanhGiaGiaoVien_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhGiaGiaoVien] WITH CHECK
ADD CONSTRAINT [FK_DanhGiaGiaoVien_ma_cau_hoi_dg__CauHoiDanhGia]
FOREIGN KEY ([ma_cau_hoi_dg]) REFERENCES dbo.[CauHoiDanhGia]([ma_cau_hoi_dg])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NopBaiDanhGia] WITH CHECK
ADD CONSTRAINT [FK_NopBaiDanhGia_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NopBaiDanhGia] WITH CHECK
ADD CONSTRAINT [FK_NopBaiDanhGia_ma_giao_vien__NguoiDung]
FOREIGN KEY ([ma_giao_vien]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NopBaiDanhGia] WITH CHECK
ADD CONSTRAINT [FK_NopBaiDanhGia_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LienKetPhuHuynh] WITH CHECK
ADD CONSTRAINT [FK_LienKetPhuHuynh_ma_phu_huynh__NguoiDung]
FOREIGN KEY ([ma_phu_huynh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[LienKetPhuHuynh] WITH CHECK
ADD CONSTRAINT [FK_LienKetPhuHuynh_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThongBao] WITH CHECK
ADD CONSTRAINT [FK_ThongBao_ma_nguoi_nhan__NguoiDung]
FOREIGN KEY ([ma_nguoi_nhan]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThongBao] WITH CHECK
ADD CONSTRAINT [FK_ThongBao_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThongBao] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThongBao_ma_thong_bao__ThongBao]
FOREIGN KEY ([ma_thong_bao]) REFERENCES dbo.[ThongBao]([ma_thong_bao])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThongBao] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThongBao_ma_nguoi_nhan__NguoiDung]
FOREIGN KEY ([ma_nguoi_nhan]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[NhatKyThongBao] WITH CHECK
ADD CONSTRAINT [FK_NhatKyThongBao_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[TuyChonThongBao] WITH CHECK
ADD CONSTRAINT [FK_TuyChonThongBao_ma_nguoi_dung__NguoiDung]
FOREIGN KEY ([ma_nguoi_dung]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThongBaoHenGio] WITH CHECK
ADD CONSTRAINT [FK_ThongBaoHenGio_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[ThongBaoHenGio] WITH CHECK
ADD CONSTRAINT [FK_ThongBaoHenGio_nguoi_tao__NguoiDung]
FOREIGN KEY ([nguoi_tao]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DatPhong] WITH CHECK
ADD CONSTRAINT [FK_DatPhong_ma_phong__PhongHoc]
FOREIGN KEY ([ma_phong]) REFERENCES dbo.[PhongHoc]([ma_phong])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DatPhong] WITH CHECK
ADD CONSTRAINT [FK_DatPhong_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DatPhong] WITH CHECK
ADD CONSTRAINT [FK_DatPhong_nguoi_yeu_cau__NguoiDung]
FOREIGN KEY ([nguoi_yeu_cau]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DatPhong] WITH CHECK
ADD CONSTRAINT [FK_DatPhong_nguoi_duyet__NguoiDung]
FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoSuDungPhong] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoSuDungPhong_ma_phong__PhongHoc]
FOREIGN KEY ([ma_phong]) REFERENCES dbo.[PhongHoc]([ma_phong])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[BaoCaoSuDungPhong] WITH CHECK
ADD CONSTRAINT [FK_BaoCaoSuDungPhong_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[AnhChupPhanTich] WITH CHECK
ADD CONSTRAINT [FK_AnhChupPhanTich_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[AnhChupPhanTich] WITH CHECK
ADD CONSTRAINT [FK_AnhChupPhanTich_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhSachRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_DanhSachRuiRoRotMon_ma_hoc_sinh__NguoiDung]
FOREIGN KEY ([ma_hoc_sinh]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhSachRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_DanhSachRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc]
FOREIGN KEY ([ma_mon_hoc]) REFERENCES dbo.[DanhMucMonHoc]([ma_mon_hoc])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[DanhSachRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [FK_DanhSachRuiRoRotMon_ma_hoc_ky__HocKy]
FOREIGN KEY ([ma_hoc_ky]) REFERENCES dbo.[HocKy]([ma_hoc_ky])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[XuatBaoCao] WITH CHECK
ADD CONSTRAINT [FK_XuatBaoCao_nguoi_yeu_cau__NguoiDung]
FOREIGN KEY ([nguoi_yeu_cau]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE dbo.[XuatBaoCao] WITH CHECK
ADD CONSTRAINT [FK_XuatBaoCao_ma_don_vi__DonVi]
FOREIGN KEY ([ma_don_vi]) REFERENCES dbo.[DonVi]([ma_don_vi])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- =========================================================
-- 3. CHECK JSON
-- =========================================================

ALTER TABLE dbo.[AnhChupPhanTich] WITH CHECK
ADD CONSTRAINT [CK_AnhChupPhanTich_chieu_loc_json_ISJSON]
CHECK ([chieu_loc_json] IS NULL OR ISJSON([chieu_loc_json]) = 1);
GO
ALTER TABLE dbo.[BaiHoc] WITH CHECK
ADD CONSTRAINT [CK_BaiHoc_dieu_kien_mo_khoa_ISJSON]
CHECK ([dieu_kien_mo_khoa] IS NULL OR ISJSON([dieu_kien_mo_khoa]) = 1);
GO
ALTER TABLE dbo.[BaiTap] WITH CHECK
ADD CONSTRAINT [CK_BaiTap_dinh_dang_cho_phep_ISJSON]
CHECK ([dinh_dang_cho_phep] IS NULL OR ISJSON([dinh_dang_cho_phep]) = 1);
GO
ALTER TABLE dbo.[BaoCaoRuiRoRotMon] WITH CHECK
ADD CONSTRAINT [CK_BaoCaoRuiRoRotMon_dac_trung_json_ISJSON]
CHECK ([dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1);
GO
ALTER TABLE dbo.[BaoCaoRuiRoVang] WITH CHECK
ADD CONSTRAINT [CK_BaoCaoRuiRoVang_dac_trung_json_ISJSON]
CHECK ([dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1);
GO
ALTER TABLE dbo.[CanhBaoDaoVan] WITH CHECK
ADD CONSTRAINT [CK_CanhBaoDaoVan_chi_tiet_ISJSON]
CHECK ([chi_tiet] IS NULL OR ISJSON([chi_tiet]) = 1);
GO
ALTER TABLE dbo.[CauHoi] WITH CHECK
ADD CONSTRAINT [CK_CauHoi_dap_an_dung_ISJSON]
CHECK ([dap_an_dung] IS NULL OR ISJSON([dap_an_dung]) = 1);
GO
ALTER TABLE dbo.[CauHoi] WITH CHECK
ADD CONSTRAINT [CK_CauHoi_lua_chon_ISJSON]
CHECK ([lua_chon] IS NULL OR ISJSON([lua_chon]) = 1);
GO
ALTER TABLE dbo.[DanhGiaGiaoVien] WITH CHECK
ADD CONSTRAINT [CK_DanhGiaGiaoVien_ai_chu_de_ISJSON]
CHECK ([ai_chu_de] IS NULL OR ISJSON([ai_chu_de]) = 1);
GO
ALTER TABLE dbo.[DeKiemTra] WITH CHECK
ADD CONSTRAINT [CK_DeKiemTra_cau_hinh_de_thi_ISJSON]
CHECK ([cau_hinh_de_thi] IS NULL OR ISJSON([cau_hinh_de_thi]) = 1);
GO
ALTER TABLE dbo.[DiemSo] WITH CHECK
ADD CONSTRAINT [CK_DiemSo_ly_do_rot_ISJSON]
CHECK ([ly_do_rot] IS NULL OR ISJSON([ly_do_rot]) = 1);
GO
ALTER TABLE dbo.[DonTu] WITH CHECK
ADD CONSTRAINT [CK_DonTu_du_lieu_bieu_mau_ISJSON]
CHECK ([du_lieu_bieu_mau] IS NULL OR ISJSON([du_lieu_bieu_mau]) = 1);
GO
ALTER TABLE dbo.[DonTu] WITH CHECK
ADD CONSTRAINT [CK_DonTu_nhat_ky_tu_dong_ISJSON]
CHECK ([nhat_ky_tu_dong] IS NULL OR ISJSON([nhat_ky_tu_dong]) = 1);
GO
ALTER TABLE dbo.[GiaoDich] WITH CHECK
ADD CONSTRAINT [CK_GiaoDich_du_lieu_phan_hoi_ISJSON]
CHECK ([du_lieu_phan_hoi] IS NULL OR ISJSON([du_lieu_phan_hoi]) = 1);
GO
ALTER TABLE dbo.[LienKetPhuHuynh] WITH CHECK
ADD CONSTRAINT [CK_LienKetPhuHuynh_quyen_xem_ISJSON]
CHECK ([quyen_xem] IS NULL OR ISJSON([quyen_xem]) = 1);
GO
ALTER TABLE dbo.[NhatKyKiemToan] WITH CHECK
ADD CONSTRAINT [CK_NhatKyKiemToan_gia_tri_cu_ISJSON]
CHECK ([gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1);
GO
ALTER TABLE dbo.[NhatKyKiemToan] WITH CHECK
ADD CONSTRAINT [CK_NhatKyKiemToan_gia_tri_moi_ISJSON]
CHECK ([gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1);
GO
ALTER TABLE dbo.[NhatKyThayDoiDiem] WITH CHECK
ADD CONSTRAINT [CK_NhatKyThayDoiDiem_gia_tri_cu_ISJSON]
CHECK ([gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1);
GO
ALTER TABLE dbo.[NhatKyThayDoiDiem] WITH CHECK
ADD CONSTRAINT [CK_NhatKyThayDoiDiem_gia_tri_moi_ISJSON]
CHECK ([gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1);
GO
ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
ADD CONSTRAINT [CK_PhienThiHocSinh_cau_tra_loi_json_ISJSON]
CHECK ([cau_tra_loi_json] IS NULL OR ISJSON([cau_tra_loi_json]) = 1);
GO
ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
ADD CONSTRAINT [CK_PhienThiHocSinh_nhat_ky_vi_pham_ISJSON]
CHECK ([nhat_ky_vi_pham] IS NULL OR ISJSON([nhat_ky_vi_pham]) = 1);
GO
ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
ADD CONSTRAINT [CK_PhienThiHocSinh_sao_luu_cuc_bo_ISJSON]
CHECK ([sao_luu_cuc_bo] IS NULL OR ISJSON([sao_luu_cuc_bo]) = 1);
GO
ALTER TABLE dbo.[ThongBaoHenGio] WITH CHECK
ADD CONSTRAINT [CK_ThongBaoHenGio_bo_loc_nguoi_nhan_ISJSON]
CHECK ([bo_loc_nguoi_nhan] IS NULL OR ISJSON([bo_loc_nguoi_nhan]) = 1);
GO
ALTER TABLE dbo.[XuatBaoCao] WITH CHECK
ADD CONSTRAINT [CK_XuatBaoCao_tham_so_json_ISJSON]
CHECK ([tham_so_json] IS NULL OR ISJSON([tham_so_json]) = 1);
GO

-- =========================================================
-- 4. INDEX, FUNCTION, TRIGGER, STORED PROCEDURE, SEED
-- =========================================================


-- Index khuyến nghị theo phạm vi cơ sở và truy vấn nghiệp vụ
CREATE INDEX IX_NguoiDung_ma_don_vi ON dbo.[NguoiDung]([ma_don_vi]);
GO
CREATE INDEX IX_DiemSo_ma_don_vi_ma_hoc_ky ON dbo.[DiemSo]([ma_don_vi], [ma_hoc_ky]);
GO
CREATE INDEX IX_DiemDanh_ma_don_vi_ma_buoi_hoc ON dbo.[DiemDanh]([ma_don_vi], [ma_buoi_hoc]);
GO
CREATE INDEX IX_NhatKyThongBao_ma_don_vi_gui_luc ON dbo.[NhatKyThongBao]([ma_don_vi], [gui_luc]);
GO
CREATE INDEX IX_NhatKyKiemToan_ma_don_vi_thoi_diem ON dbo.[NhatKyKiemToan]([ma_don_vi], [thoi_diem_thay_doi]);
GO

-- Một bình luận được ghim tối đa 1 lần cho mỗi bài học
CREATE UNIQUE INDEX UX_BinhLuan_MotGhimMoiBaiHoc
ON dbo.[BinhLuan]([ma_bai_hoc])
WHERE [da_ghim] = 1;
GO

-- Hàm lấy danh sách đơn vị con phục vụ lọc dữ liệu đa cơ sở
CREATE OR ALTER FUNCTION dbo.fn_LayDanhSachCoSoCon(@maDonVi UNIQUEIDENTIFIER)
RETURNS TABLE
AS
RETURN
(
    WITH CayDonVi AS
    (
        SELECT [ma_don_vi], [ma_don_vi_cha], [ten_don_vi], [cap_don_vi]
        FROM dbo.[DonVi]
        WHERE [ma_don_vi] = @maDonVi

        UNION ALL

        SELECT dv.[ma_don_vi], dv.[ma_don_vi_cha], dv.[ten_don_vi], dv.[cap_don_vi]
        FROM dbo.[DonVi] dv
        INNER JOIN CayDonVi c ON dv.[ma_don_vi_cha] = c.[ma_don_vi]
    )
    SELECT [ma_don_vi] FROM CayDonVi
);
GO

-- Trigger bảo vệ điểm đã khóa: không cho sửa các cột điểm/GPA/trạng thái khi da_khoa = 1
CREATE OR ALTER TRIGGER dbo.trg_DiemSo_ChanSuaKhiDaKhoa
ON dbo.[DiemSo]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.[ma_diem_so] = d.[ma_diem_so]
        WHERE d.[da_khoa] = 1
          AND (
                ISNULL(i.[diem_qua_trinh], -1) <> ISNULL(d.[diem_qua_trinh], -1)
             OR ISNULL(i.[diem_giua_ky], -1) <> ISNULL(d.[diem_giua_ky], -1)
             OR ISNULL(i.[diem_cuoi_ky], -1) <> ISNULL(d.[diem_cuoi_ky], -1)
             OR ISNULL(i.[gpa_mon_hoc], -1) <> ISNULL(d.[gpa_mon_hoc], -1)
             OR ISNULL(i.[trang_thai], N'') <> ISNULL(d.[trang_thai], N'')
          )
    )
    BEGIN
        THROW 50001, N'Không được sửa điểm khi bản ghi DiemSo đã khóa.', 1;
    END
END;
GO

-- Stored procedure tính lại GPA môn theo cấu hình trọng số
CREATE OR ALTER PROCEDURE dbo.sp_TinhLaiGpaMonHoc
    @maDiemSo UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ds
    SET [gpa_mon_hoc] =
        CAST((
            ISNULL(ds.[diem_qua_trinh], 0) * ch.[trong_so_qua_trinh] +
            ISNULL(ds.[diem_giua_ky], 0) * ch.[trong_so_giua_ky] +
            ISNULL(ds.[diem_cuoi_ky], 0) * ch.[trong_so_cuoi_ky]
        ) / 100.0 AS DECIMAL(5,2)),
        [trang_thai] =
            CASE
                WHEN (
                    ISNULL(ds.[diem_qua_trinh], 0) * ch.[trong_so_qua_trinh] +
                    ISNULL(ds.[diem_giua_ky], 0) * ch.[trong_so_giua_ky] +
                    ISNULL(ds.[diem_cuoi_ky], 0) * ch.[trong_so_cuoi_ky]
                ) / 100.0 >= ch.[nguong_dat]
                THEN N'dat'
                ELSE N'rot'
            END
    FROM dbo.[DiemSo] ds
    JOIN dbo.[CauHinhDiemMonHoc] ch
      ON ch.[ma_mon_hoc] = ds.[ma_mon_hoc]
     AND ch.[ma_hoc_ky] = ds.[ma_hoc_ky]
    WHERE ds.[ma_diem_so] = @maDiemSo
      AND ds.[da_khoa] = 0;
END;
GO

-- Trigger tính lại GPA tự động sau khi nhập/sửa điểm
CREATE OR ALTER TRIGGER dbo.trg_DiemSo_TinhGpaSauCapNhat
ON dbo.[DiemSo]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ds
    SET [gpa_mon_hoc] =
        CAST((
            ISNULL(ds.[diem_qua_trinh], 0) * ch.[trong_so_qua_trinh] +
            ISNULL(ds.[diem_giua_ky], 0) * ch.[trong_so_giua_ky] +
            ISNULL(ds.[diem_cuoi_ky], 0) * ch.[trong_so_cuoi_ky]
        ) / 100.0 AS DECIMAL(5,2)),
        [trang_thai] =
            CASE
                WHEN (
                    ISNULL(ds.[diem_qua_trinh], 0) * ch.[trong_so_qua_trinh] +
                    ISNULL(ds.[diem_giua_ky], 0) * ch.[trong_so_giua_ky] +
                    ISNULL(ds.[diem_cuoi_ky], 0) * ch.[trong_so_cuoi_ky]
                ) / 100.0 >= ch.[nguong_dat]
                THEN N'dat'
                ELSE N'rot'
            END
    FROM dbo.[DiemSo] ds
    JOIN inserted i ON i.[ma_diem_so] = ds.[ma_diem_so]
    JOIN dbo.[CauHinhDiemMonHoc] ch
      ON ch.[ma_mon_hoc] = ds.[ma_mon_hoc]
     AND ch.[ma_hoc_ky] = ds.[ma_hoc_ky]
    WHERE ds.[da_khoa] = 0;
END;
GO

-- Seed vai trò cơ bản
INSERT INTO dbo.[VaiTro]([ma_code_vai_tro], [ten_vai_tro])
SELECT v.[ma_code_vai_tro], v.[ten_vai_tro]
FROM (VALUES
    (N'quan_tri', N'Quản trị'),
    (N'giao_vien', N'Giáo viên'),
    (N'hoc_sinh', N'Học sinh'),
    (N'nhan_vien', N'Nhân viên/Giáo vụ'),
    (N'hieu_truong', N'Hiệu trưởng/BGH'),
    (N'phu_huynh', N'Phụ huynh'),
    (N'sieu_quan_tri', N'Siêu quản trị'),
    (N'quan_tri_co_so', N'Quản trị cơ sở')
) v([ma_code_vai_tro], [ten_vai_tro])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.[VaiTro] vt WHERE vt.[ma_code_vai_tro] = v.[ma_code_vai_tro]
);
GO


-- =========================================================
-- 5. BO SUNG & SUA LOI THEO 2 FILE REVIEW HTML
--    (Day la phan tich hop trong file day du, khong can chay patch rieng)
-- =========================================================

/* =========================================================
   M1 - NguoiDung: them dem sai mat khau + co dang nhap lan dau
   ========================================================= */
IF COL_LENGTH('dbo.NguoiDung', 'so_lan_sai_mat_khau') IS NULL
BEGIN
    ALTER TABLE dbo.[NguoiDung]
    ADD [so_lan_sai_mat_khau] INT NOT NULL
        CONSTRAINT [DF_NguoiDung_so_lan_sai_mat_khau] DEFAULT 0;
END;
GO

IF COL_LENGTH('dbo.NguoiDung', 'dang_nhap_lan_dau') IS NULL
BEGIN
    ALTER TABLE dbo.[NguoiDung]
    ADD [dang_nhap_lan_dau] BIT NOT NULL
        CONSTRAINT [DF_NguoiDung_dang_nhap_lan_dau] DEFAULT 1;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_NguoiDung_so_lan_sai_mat_khau')
BEGIN
    ALTER TABLE dbo.[NguoiDung] WITH CHECK
    ADD CONSTRAINT [CK_NguoiDung_so_lan_sai_mat_khau]
    CHECK ([so_lan_sai_mat_khau] BETWEEN 0 AND 5);
END;
GO

/* =========================================================
   M7 - ThoiKhoaBieu: sua CHECK thu_trong_tuan tu 1-7 thanh 2-7
   ========================================================= */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ThoiKhoaBieu_thu_trong_tuan_1')
BEGIN
    ALTER TABLE dbo.[ThoiKhoaBieu]
    DROP CONSTRAINT [CK_ThoiKhoaBieu_thu_trong_tuan_1];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ThoiKhoaBieu_thu_trong_tuan_1')
BEGIN
    ALTER TABLE dbo.[ThoiKhoaBieu] WITH CHECK
    ADD CONSTRAINT [CK_ThoiKhoaBieu_thu_trong_tuan_1]
    CHECK ([thu_trong_tuan] BETWEEN 2 AND 7);
END;
GO

/* =========================================================
   M5 - YeuCauMoKhoaDiemDanh: UNIQUE chi ap dung cho yeu cau dang cho duyet
   ========================================================= */
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_YeuCauMoKhoaDiemDanh_1')
BEGIN
    ALTER TABLE dbo.[YeuCauMoKhoaDiemDanh]
    DROP CONSTRAINT [UQ_YeuCauMoKhoaDiemDanh_1];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_YeuCauMoKhoaDiemDanh_ChoDuyet' AND object_id = OBJECT_ID(N'dbo.YeuCauMoKhoaDiemDanh'))
BEGIN
    CREATE UNIQUE INDEX [UX_YeuCauMoKhoaDiemDanh_ChoDuyet]
    ON dbo.[YeuCauMoKhoaDiemDanh]([ma_buoi_hoc])
    WHERE [trang_thai] = N'cho_duyet';
END;
GO

/* =========================================================
   M14 - DanhGiaGiaoVien: tang bao ve an danh bang ngay_tao/cohort_hash + view threshold >= 5
   ========================================================= */
IF COL_LENGTH('dbo.DanhGiaGiaoVien', 'ngay_tao') IS NULL
BEGIN
    ALTER TABLE dbo.[DanhGiaGiaoVien]
    ADD [ngay_tao] DATETIME2 NOT NULL
        CONSTRAINT [DF_DanhGiaGiaoVien_ngay_tao] DEFAULT SYSUTCDATETIME();
END;
GO

IF COL_LENGTH('dbo.DanhGiaGiaoVien', 'cohort_hash') IS NULL
BEGIN
    ALTER TABLE dbo.[DanhGiaGiaoVien]
    ADD [cohort_hash] NVARCHAR(128) NULL;
END;
GO

CREATE OR ALTER VIEW dbo.[vw_TongHopDanhGiaGiaoVien_AnDanh]
AS
SELECT
    [ma_giao_vien],
    [ma_hoc_ky],
    [ma_cau_hoi_dg],
    COUNT_BIG(*) AS [so_luong_danh_gia],
    CAST(AVG(CAST([diem_so] AS DECIMAL(10,2))) AS DECIMAL(5,2)) AS [diem_trung_binh]
FROM dbo.[DanhGiaGiaoVien]
GROUP BY [ma_giao_vien], [ma_hoc_ky], [ma_cau_hoi_dg]
HAVING COUNT_BIG(*) >= 5;
GO

/* =========================================================
   M13 - DonTu: bo sung loai don phuc_tra_diem, bao_luu, chuyen_nganh, chuyen_co_so, xac_nhan, rut_hoc
   ========================================================= */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_DonTu_loai_don_1')
BEGIN
    ALTER TABLE dbo.[DonTu]
    DROP CONSTRAINT [CK_DonTu_loai_don_1];
END;
GO

ALTER TABLE dbo.[DonTu] WITH CHECK
ADD CONSTRAINT [CK_DonTu_loai_don_1]
CHECK ([loai_don] IN (
    N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac',
    N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc'
));
GO

/* =========================================================
   M6 - CauHinhDiemMonHoc: tong trong so phai bang 100
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CauHinhDiemMonHoc_tong_trong_so_100')
BEGIN
    ALTER TABLE dbo.[CauHinhDiemMonHoc] WITH CHECK
    ADD CONSTRAINT [CK_CauHinhDiemMonHoc_tong_trong_so_100]
    CHECK ([trong_so_qua_trinh] + [trong_so_giua_ky] + [trong_so_cuoi_ky] = 100);
END;
GO

/* =========================================================
   M5 - LopHocPhan: quota vang toi da + view tong vang theo mon/lop hoc phan
   ========================================================= */
IF COL_LENGTH('dbo.LopHocPhan', 'quota_vang_toi_da') IS NULL
BEGIN
    ALTER TABLE dbo.[LopHocPhan]
    ADD [quota_vang_toi_da] INT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_LopHocPhan_quota_vang_toi_da')
BEGIN
    ALTER TABLE dbo.[LopHocPhan] WITH CHECK
    ADD CONSTRAINT [CK_LopHocPhan_quota_vang_toi_da]
    CHECK ([quota_vang_toi_da] IS NULL OR [quota_vang_toi_da] >= 0);
END;
GO

CREATE OR ALTER VIEW dbo.[vw_TongVangTheoMonHoc]
AS
SELECT
    dd.[ma_hoc_sinh],
    tkb.[ma_lop_hoc_phan],
    tkb.[ma_mon_hoc],
    COUNT_BIG(*) AS [so_ban_ghi_diem_danh],
    SUM(CASE WHEN dd.[trang_thai] = N'vang' THEN dd.[he_so_vang] ELSE 0 END) AS [tong_he_so_vang]
FROM dbo.[DiemDanh] dd
JOIN dbo.[BuoiHoc] bh ON bh.[ma_buoi_hoc] = dd.[ma_buoi_hoc]
JOIN dbo.[ThoiKhoaBieu] tkb ON tkb.[ma_tkb] = bh.[ma_tkb]
GROUP BY dd.[ma_hoc_sinh], tkb.[ma_lop_hoc_phan], tkb.[ma_mon_hoc];
GO

/* =========================================================
   M6 - YeuCauSuaDiem: phan biet sua sau submit vs incomplete, thoi han unlock
   ========================================================= */
IF COL_LENGTH('dbo.YeuCauSuaDiem', 'loai_yeu_cau') IS NULL
BEGIN
    ALTER TABLE dbo.[YeuCauSuaDiem]
    ADD [loai_yeu_cau] NVARCHAR(30) NOT NULL
        CONSTRAINT [DF_YeuCauSuaDiem_loai_yeu_cau] DEFAULT N'sua_sau_submit';
END;
GO

IF COL_LENGTH('dbo.YeuCauSuaDiem', 'unlock_expires_at') IS NULL
BEGIN
    ALTER TABLE dbo.[YeuCauSuaDiem]
    ADD [unlock_expires_at] DATETIME2 NULL;
END;
GO

IF COL_LENGTH('dbo.YeuCauSuaDiem', 'cot_diem_duoc_mo') IS NULL
BEGIN
    ALTER TABLE dbo.[YeuCauSuaDiem]
    ADD [cot_diem_duoc_mo] NVARCHAR(30) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_YeuCauSuaDiem_loai_yeu_cau')
BEGIN
    ALTER TABLE dbo.[YeuCauSuaDiem] WITH CHECK
    ADD CONSTRAINT [CK_YeuCauSuaDiem_loai_yeu_cau]
    CHECK ([loai_yeu_cau] IN (N'sua_sau_submit', N'incomplete'));
END;
GO

/* =========================================================
   M7 - YeuCauDoiLich: bang luu quy trinh 2 GV doi lich + admin duyet
   ========================================================= */
IF OBJECT_ID(N'dbo.YeuCauDoiLich', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[YeuCauDoiLich] (
        [ma_yc_doi_lich] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_YeuCauDoiLich_ma_yc_doi_lich] DEFAULT NEWID(),
        [ma_tkb] UNIQUEIDENTIFIER NOT NULL,
        [giao_vien_de_xuat] UNIQUEIDENTIFIER NOT NULL,
        [giao_vien_nhan_doi] UNIQUEIDENTIFIER NOT NULL,
        [ly_do] NVARCHAR(MAX) NOT NULL,
        [trang_thai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_YeuCauDoiLich_trang_thai] DEFAULT N'cho_gv_nhan_dong_y',
        [nguoi_duyet] UNIQUEIDENTIFIER NULL,
        [gv_nhan_phan_hoi_luc] DATETIME2 NULL,
        [admin_duyet_luc] DATETIME2 NULL,
        [ngay_tao] DATETIME2 NOT NULL CONSTRAINT [DF_YeuCauDoiLich_ngay_tao] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_YeuCauDoiLich] PRIMARY KEY ([ma_yc_doi_lich]),
        CONSTRAINT [CK_YeuCauDoiLich_trang_thai_1] CHECK ([trang_thai] IN (N'cho_gv_nhan_dong_y', N'cho_admin_duyet', N'da_hoan_doi', N'tu_choi', N'da_huy')),
        CONSTRAINT [FK_YeuCauDoiLich_ma_tkb__ThoiKhoaBieu] FOREIGN KEY ([ma_tkb]) REFERENCES dbo.[ThoiKhoaBieu]([ma_tkb]),
        CONSTRAINT [FK_YeuCauDoiLich_gv_de_xuat__NguoiDung] FOREIGN KEY ([giao_vien_de_xuat]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung]),
        CONSTRAINT [FK_YeuCauDoiLich_gv_nhan_doi__NguoiDung] FOREIGN KEY ([giao_vien_nhan_doi]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung]),
        CONSTRAINT [FK_YeuCauDoiLich_nguoi_duyet__NguoiDung] FOREIGN KEY ([nguoi_duyet]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung])
    );
END;
GO

/* =========================================================
   M3 - BaiNop: co cong bo diem/nhan xet cho HS xem
   ========================================================= */
IF COL_LENGTH('dbo.BaiNop', 'da_cong_bo') IS NULL
BEGIN
    ALTER TABLE dbo.[BaiNop]
    ADD [da_cong_bo] BIT NOT NULL
        CONSTRAINT [DF_BaiNop_da_cong_bo] DEFAULT 0;
END;
GO

/* =========================================================
   M4 - PhienThiHocSinh: diem AI goi y cho tu luan
   ========================================================= */
IF COL_LENGTH('dbo.PhienThiHocSinh', 'diem_tu_luan_ai_goi_y') IS NULL
BEGIN
    ALTER TABLE dbo.[PhienThiHocSinh]
    ADD [diem_tu_luan_ai_goi_y] DECIMAL(5,2) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y')
BEGIN
    ALTER TABLE dbo.[PhienThiHocSinh] WITH CHECK
    ADD CONSTRAINT [CK_PhienThiHocSinh_diem_tu_luan_ai_goi_y]
    CHECK ([diem_tu_luan_ai_goi_y] IS NULL OR [diem_tu_luan_ai_goi_y] BETWEEN 0 AND 10);
END;
GO

/* =========================================================
   M9 - KhenThuong: soft-cancel, khong xoa/thu hoi vat ly
   ========================================================= */
IF COL_LENGTH('dbo.KhenThuong', 'da_huy') IS NULL
BEGIN
    ALTER TABLE dbo.[KhenThuong]
    ADD [da_huy] BIT NOT NULL
        CONSTRAINT [DF_KhenThuong_da_huy] DEFAULT 0;
END;
GO

IF COL_LENGTH('dbo.KhenThuong', 'ghi_chu_huy') IS NULL
BEGIN
    ALTER TABLE dbo.[KhenThuong]
    ADD [ghi_chu_huy] NVARCHAR(MAX) NULL;
END;
GO

/* =========================================================
   M10 - HoaDon/GiaoDich: rollback timeout + actor audit
   ========================================================= */
IF COL_LENGTH('dbo.HoaDon', 'thoi_diem_khoi_tao_tt') IS NULL
BEGIN
    ALTER TABLE dbo.[HoaDon]
    ADD [thoi_diem_khoi_tao_tt] DATETIME2 NULL;
END;
GO

IF COL_LENGTH('dbo.HoaDon', 'het_han_tt') IS NULL
BEGIN
    ALTER TABLE dbo.[HoaDon]
    ADD [het_han_tt] DATETIME2 NULL;
END;
GO

IF COL_LENGTH('dbo.GiaoDich', 'ma_nguoi_thuc_hien') IS NULL
BEGIN
    ALTER TABLE dbo.[GiaoDich]
    ADD [ma_nguoi_thuc_hien] UNIQUEIDENTIFIER NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_GiaoDich_ma_nguoi_thuc_hien__NguoiDung')
BEGIN
    ALTER TABLE dbo.[GiaoDich] WITH CHECK
    ADD CONSTRAINT [FK_GiaoDich_ma_nguoi_thuc_hien__NguoiDung]
    FOREIGN KEY ([ma_nguoi_thuc_hien]) REFERENCES dbo.[NguoiDung]([ma_nguoi_dung]);
END;
GO

/* =========================================================
   M11 - PhieuHoTro: priority de tinh SLA
   ========================================================= */
IF COL_LENGTH('dbo.PhieuHoTro', 'do_uu_tien') IS NULL
BEGIN
    ALTER TABLE dbo.[PhieuHoTro]
    ADD [do_uu_tien] NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_PhieuHoTro_do_uu_tien] DEFAULT N'medium';
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PhieuHoTro_do_uu_tien')
BEGIN
    ALTER TABLE dbo.[PhieuHoTro] WITH CHECK
    ADD CONSTRAINT [CK_PhieuHoTro_do_uu_tien]
    CHECK ([do_uu_tien] IN (N'low', N'medium', N'high', N'urgent'));
END;
GO

/* =========================================================
   M12 - DangKyHocPhan: co kiem tra tien quyet ro rang
   ========================================================= */
IF COL_LENGTH('dbo.DangKyHocPhan', 'da_kiem_tra_tien_quyet') IS NULL
BEGIN
    ALTER TABLE dbo.[DangKyHocPhan]
    ADD [da_kiem_tra_tien_quyet] BIT NOT NULL
        CONSTRAINT [DF_DangKyHocPhan_da_kiem_tra_tien_quyet] DEFAULT 0;
END;
GO

/* =========================================================
   M14 - NopBaiDanhGia: so lan sua toi da 2
   ========================================================= */
IF COL_LENGTH('dbo.NopBaiDanhGia', 'so_lan_sua') IS NULL
BEGIN
    ALTER TABLE dbo.[NopBaiDanhGia]
    ADD [so_lan_sua] INT NOT NULL
        CONSTRAINT [DF_NopBaiDanhGia_so_lan_sua] DEFAULT 0;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_NopBaiDanhGia_so_lan_sua')
BEGIN
    ALTER TABLE dbo.[NopBaiDanhGia] WITH CHECK
    ADD CONSTRAINT [CK_NopBaiDanhGia_so_lan_sua]
    CHECK ([so_lan_sua] BETWEEN 0 AND 2);
END;
GO

/* =========================================================
   M18 - AnalyticsSnapshots: lam ro AnhChupPhanTich bang view/alias tong hop
   ========================================================= */
IF OBJECT_ID(N'dbo.BaoCaoTongHopPhanTich', N'V') IS NULL AND OBJECT_ID(N'dbo.AnhChupPhanTich', N'U') IS NOT NULL
BEGIN
    EXEC(N'CREATE VIEW dbo.[BaoCaoTongHopPhanTich] AS SELECT * FROM dbo.[AnhChupPhanTich];');
END;
GO

/* =========================================================
   M8 - BinhLuan: bo gioi han 1 comment ghim/bai hoc neu index ton tai
   ========================================================= */
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BinhLuan_MotGhimMoiBaiHoc' AND object_id = OBJECT_ID(N'dbo.BinhLuan'))
BEGIN
    DROP INDEX [UX_BinhLuan_MotGhimMoiBaiHoc] ON dbo.[BinhLuan];
END;
GO

/* =========================================================
   NFR Security - cam xoa vat ly cac bang quan trong
   ========================================================= */
CREATE OR ALTER TRIGGER dbo.[trg_NhatKyKiemToan_CamXoaVatLy]
ON dbo.[NhatKyKiemToan]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50002, N'NhatKyKiemToan la audit log bat bien; khong duoc xoa vat ly.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.[trg_DiemSo_CamXoaVatLy]
ON dbo.[DiemSo]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50003, N'DiemSo khong duoc xoa vat ly; hay dung trang thai/nhat ky thay doi diem.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.[trg_HoaDon_CamXoaVatLy]
ON dbo.[HoaDon]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50004, N'HoaDon khong duoc xoa vat ly; hay cap nhat trang_thai hoac tao giao dich dieu chinh.', 1;
END;
GO

/* =========================================================
   NFR Performance - bo sung index cho bang nghiep vu nang
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiemDanh_BuoiHoc_HocSinh' AND object_id = OBJECT_ID(N'dbo.DiemDanh'))
BEGIN
    CREATE INDEX [IX_DiemDanh_BuoiHoc_HocSinh]
    ON dbo.[DiemDanh]([ma_buoi_hoc], [ma_hoc_sinh], [trang_thai]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiemDanh_DonVi_HocSinh' AND object_id = OBJECT_ID(N'dbo.DiemDanh'))
BEGIN
    CREATE INDEX [IX_DiemDanh_DonVi_HocSinh]
    ON dbo.[DiemDanh]([ma_don_vi], [ma_hoc_sinh]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BaiNop_BaiTap_HocSinh' AND object_id = OBJECT_ID(N'dbo.BaiNop'))
BEGIN
    CREATE INDEX [IX_BaiNop_BaiTap_HocSinh]
    ON dbo.[BaiNop]([ma_bai_tap], [ma_hoc_sinh], [so_lan_nop]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DangKyHocPhan_LopHocPhan' AND object_id = OBJECT_ID(N'dbo.DangKyHocPhan'))
BEGIN
    CREATE INDEX [IX_DangKyHocPhan_LopHocPhan]
    ON dbo.[DangKyHocPhan]([ma_lop_hoc_phan], [trang_thai]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiemSo_HocSinh_HocKy' AND object_id = OBJECT_ID(N'dbo.DiemSo'))
BEGIN
    CREATE INDEX [IX_DiemSo_HocSinh_HocKy]
    ON dbo.[DiemSo]([ma_hoc_sinh], [ma_hoc_ky]);
END;
GO

IF OBJECT_ID(N'dbo.ThongBao', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ThongBao_NguoiNhan_DaDoc' AND object_id = OBJECT_ID(N'dbo.ThongBao'))
AND COL_LENGTH('dbo.ThongBao', 'ma_nguoi_nhan') IS NOT NULL
AND COL_LENGTH('dbo.ThongBao', 'da_doc') IS NOT NULL
BEGIN
    CREATE INDEX [IX_ThongBao_NguoiNhan_DaDoc]
    ON dbo.[ThongBao]([ma_nguoi_nhan], [da_doc]);
END;
GO

PRINT N'Hoan tat LMS SQL FULL: da tich hop loi va yeu cau bo sung tu 2 file review HTML.';
GO
