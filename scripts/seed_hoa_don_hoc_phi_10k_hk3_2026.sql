-- ============================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
-- Tạo hóa đơn học phí HK3-2026 (ma_hoc_ky = 12) cho TOÀN BỘ sinh viên CS FPT Polytechnic HCM (ma_don_vi = 13)
-- Chương trình giảm học phí: sinh viên chỉ còn phải đóng 10.000 đồng
--   + SV thuộc chương trình có cấu hình (CT 1,2,3): SoTien = học phí gốc (tong_tien_du_kien), GiamTru = gốc - 10.000
--   + SV thuộc chương trình không có cấu hình: SoTien = 10.000, GiamTru = 0
-- Hóa đơn đã thanh toán đủ 10.000đ (trang_thai = da_thanh_toan)
-- Kèm lịch sử giao dịch thanh toán (GiaoDich loai = thanh_toan_hoc_phi, trang_thai = thanh_cong)
-- Script idempotent: chạy lại không tạo trùng hóa đơn/giao dịch
-- ============================================================

DECLARE @MaHocKy INT = 12; -- HK3_2026 - CS HCM
DECLARE @MaDonVi INT = 13; -- FPT Polytechnic HCM
DECLARE @TkNhanTien INT = 2; -- Tài khoản nhận tiền MB Bank của CS 13

-- 1) Tạo hóa đơn cho từng sinh viên
INSERT INTO dbo.HoaDon
    (ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code, loai_hoa_don,
     so_tien, giam_tru, da_thanh_toan, trang_thai, han_thanh_toan,
     ghi_chu, ngay_tao, nguoi_tao)
SELECT
    n.ma_don_vi,
    n.ma_nguoi_dung,
    @MaHocKy,
    'HP-HK3-2026-' + CAST(n.ma_nguoi_dung AS VARCHAR(10)),
    'hoc_phi',
    ISNULL(c.tong_tien_du_kien, 10000),                                -- so_tien
    CASE WHEN c.tong_tien_du_kien IS NOT NULL
         THEN c.tong_tien_du_kien - 10000 ELSE 0 END,                  -- giam_tru
    10000,                                                             -- da_thanh_toan
    'da_thanh_toan',
    CAST(SYSUTCDATETIME() AS DATE),
    CASE WHEN c.tong_tien_du_kien IS NOT NULL
         THEN N'Học phí HK3-2026 giảm còn 10.000 đồng'
         ELSE N'Học phí HK3-2026: 10.000 đồng (không có cấu hình học phí)' END,
    SYSUTCDATETIME(),
    NULL
FROM dbo.NguoiDung n
LEFT JOIN dbo.LopHanhChinh l ON l.ma_lop = n.ma_lop
LEFT JOIN dbo.CauHinhHocPhiChuongTrinh c
       ON c.ma_don_vi = n.ma_don_vi
      AND c.ma_hoc_ky = @MaHocKy
      AND c.ma_chuong_trinh_dao_tao = l.ma_chuong_trinh
      AND c.con_hoat_dong = 1
WHERE n.vai_tro_chinh = 'hoc_sinh'
  AND n.ma_don_vi = @MaDonVi
  AND NOT EXISTS (
      SELECT 1 FROM dbo.HoaDon h
      WHERE h.ma_hoc_sinh = n.ma_nguoi_dung AND h.ma_hoc_ky = @MaHocKy
  );

-- 2) Lịch sử giao dịch: mỗi hóa đơn có 1 giao dịch thanh toán 10.000đ thành công
INSERT INTO dbo.GiaoDich
    (ma_hoa_don, ma_tai_khoan_nhan_tien, ma_tham_chieu_noi_bo,
     so_tien, loai_giao_dich, trang_thai, nha_cung_cap_thanh_toan,
     noi_dung_chuyen_khoan, ngay_tao, ngay_thanh_toan,
     ma_nguoi_thuc_hien, chu_thich)
SELECT
    h.ma_hoa_don,
    @TkNhanTien,
    'GD-' + CAST(h.ma_hoa_don AS VARCHAR(10)),
    10000,
    'thanh_toan_hoc_phi',
    'thanh_cong',
    'payos',
    N'Đóng học phí 10.000đ - Học kỳ 3 năm 2026',
    SYSUTCDATETIME(),
    SYSUTCDATETIME(),
    h.ma_hoc_sinh,
    N'Chương trình giảm học phí còn 10.000 đồng cho toàn cơ sở'
FROM dbo.HoaDon h
WHERE h.ma_hoc_ky = @MaHocKy
  AND NOT EXISTS (
      SELECT 1 FROM dbo.GiaoDich g
      WHERE g.ma_hoa_don = h.ma_hoa_don
  );

-- 3) Báo cáo kết quả
SELECT
    COUNT(*)                                                      AS so_hoa_don_tao,
    SUM(CASE WHEN giam_tru > 0 THEN 1 ELSE 0 END)                 AS hoa_don_co_giam_tru,
    SUM(CASE WHEN giam_tru = 0 AND so_tien = 10000 THEN 1 ELSE 0 END) AS hoa_don_10k_khong_cau_hinh,
    SUM(so_tien)                                                  AS tong_goc,
    SUM(giam_tru)                                                 AS tong_giam_tru,
    SUM(da_thanh_toan)                                            AS tong_da_thu,
    COUNT(DISTINCT ma_don_vi)                                     AS so_cs
FROM dbo.HoaDon
WHERE ma_hoc_ky = @MaHocKy;

SELECT COUNT(*) AS so_giao_dich_tao
FROM dbo.GiaoDich g
JOIN dbo.HoaDon h ON h.ma_hoa_don = g.ma_hoa_don
WHERE h.ma_hoc_ky = @MaHocKy;

-- 4) Mẫu kiểm tra
SELECT TOP 5 h.ma_hoa_don, h.ma_hoa_don_code, h.ma_hoc_sinh, h.so_tien, h.giam_tru,
       h.da_thanh_toan, h.trang_thai, g.ma_giao_dich, g.so_tien AS gd_tien,
       g.trang_thai AS gd_trang_thai, g.loai_giao_dich
FROM dbo.HoaDon h
LEFT JOIN dbo.GiaoDich g ON g.ma_hoa_don = h.ma_hoa_don
WHERE h.ma_hoc_ky = @MaHocKy
ORDER BY h.ma_hoa_don;
