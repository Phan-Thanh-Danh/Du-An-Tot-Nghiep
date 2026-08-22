-- ============================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
-- Dọn dữ liệu seed nhầm trước đó (hóa đơn HK3-2026 đã tạo + giao dịch kèm theo)
-- ============================================================
DELETE g
FROM dbo.GiaoDich g
JOIN dbo.HoaDon h ON h.ma_hoa_don = g.ma_hoa_don
WHERE h.ma_hoc_ky = 12;

DELETE FROM dbo.HoaDon WHERE ma_hoc_ky = 12;

-- ============================================================
-- Tạo 2 hóa đơn học phí mẫu HK3-2026: học phí gốc 30.000.000đ
-- giảm còn 10.000đ (GiamTru = 30.000.000 - 10.000 = 29.990.000)
-- Trạng thái: chưa thanh toán
-- ============================================================
DECLARE @MaHocKy INT = 12; -- HK3_2026 - CS HCM
DECLARE @MaDonVi INT = 13; -- FPT Polytechnic HCM

-- Lấy 2 sinh viên đầu tiên thuộc chương trình có học phí gốc 30.000.000đ (CT 1)
IF NOT EXISTS (SELECT 1 FROM dbo.HoaDon WHERE ma_hoc_ky = @MaHocKy)
BEGIN
    INSERT INTO dbo.HoaDon
        (ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code, loai_hoa_don,
         so_tien, giam_tru, da_thanh_toan, trang_thai, han_thanh_toan,
         ghi_chu, ngay_tao, nguoi_tao)
    SELECT TOP 2
        n.ma_don_vi,
        n.ma_nguoi_dung,
        @MaHocKy,
        'HP-HK3-2026-' + CAST(n.ma_nguoi_dung AS VARCHAR(10)),
        'hoc_phi',
        30000000.00,              -- so_tien: học phí gốc 30 triệu
        29990000.00,              -- giam_tru: giảm còn 10.000đ
        0.00,                     -- da_thanh_toan
        'chua_thanh_toan',
        DATEADD(DAY, 30, CAST(SYSUTCDATETIME() AS DATE)),
        N'Học phí HK3-2026 giảm từ 30.000.000đ xuống còn 10.000đ',
        SYSUTCDATETIME(),
        NULL
    FROM dbo.NguoiDung n
    JOIN dbo.LopHanhChinh l ON l.ma_lop = n.ma_lop AND l.ma_chuong_trinh = 1
    WHERE n.vai_tro_chinh = 'hoc_sinh'
      AND n.ma_don_vi = @MaDonVi
    ORDER BY n.ma_nguoi_dung;
END

-- Kiểm tra kết quả
SELECT h.ma_hoa_don, h.ma_hoa_don_code, h.ma_hoc_sinh, n.ho_ten, n.email,
       h.so_tien, h.giam_tru, h.da_thanh_toan,
       h.so_tien - h.giam_tru - h.da_thanh_toan AS con_phai_dong,
       h.trang_thai, h.han_thanh_toan
FROM dbo.HoaDon h
JOIN dbo.NguoiDung n ON n.ma_nguoi_dung = h.ma_hoc_sinh
WHERE h.ma_hoc_ky = @MaHocKy;
