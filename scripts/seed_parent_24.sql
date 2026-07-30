SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
USE LMS;

DELETE FROM DiemSo WHERE ma_hoc_sinh = 24;
INSERT INTO DiemSo (ma_don_vi, ma_hoc_sinh, ma_mon_hoc, ma_hoc_ky, diem_qua_trinh, diem_giua_ky, diem_cuoi_ky, gpa_mon_hoc, trang_thai, da_khoa, nam_nhap_hoc)
VALUES
  (3, 24, 50, 1, 6.0, 7.5, 7.5, 7.0, 'da_co_diem', 1, 2024),
  (3, 24, 50, 2, 8.0, 7.0, 8.5, 8.2, 'da_co_diem', 1, 2024),
  (3, 24, 50, 3, 9.5, 9.0, 9.0, 9.5, 'da_co_diem', 0, 2024);

DELETE FROM DiemDanh WHERE ma_hoc_sinh = 24;
INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
SELECT TOP 20 3, ma_buoi_hoc, 24,
  CASE
    WHEN ROW_NUMBER() OVER (ORDER BY ma_buoi_hoc) % 4 = 0 THEN 'vang'
    WHEN ROW_NUMBER() OVER (ORDER BY ma_buoi_hoc) % 9 = 0 THEN 'di_muon'
    ELSE 'co_mat'
  END,
  15, GETDATE(), 1.0
FROM BuoiHoc
WHERE ma_khoa_hoc IN (1,2,3,4,5);

DELETE FROM HoaDon WHERE ma_hoc_sinh = 24;
INSERT INTO HoaDon (ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code, loai_hoa_don, so_tien, giam_tru, da_thanh_toan, trang_thai, han_thanh_toan, ngay_tao, nguoi_tao)
VALUES
  (3, 24, 1, 'HD-2024-001-24', 'hoc_phi', 9000000, 0, 9000000, 'da_thanh_toan', '2024-09-30', GETDATE(), 1),
  (3, 24, 2, 'HD-2024-002-24', 'hoc_phi', 9500000, 500000, 4500000, 'chua_thanh_toan', '2025-03-15', GETDATE(), 1);

SELECT 'DiemSo' as bang, COUNT(*) as sl FROM DiemSo WHERE ma_hoc_sinh = 24
UNION ALL SELECT 'DiemDanh', COUNT(*) FROM DiemDanh WHERE ma_hoc_sinh = 24
UNION ALL SELECT 'HoaDon', COUNT(*) FROM HoaDon WHERE ma_hoc_sinh = 24
UNION ALL SELECT 'LienKetPhuHuynh', COUNT(*) FROM LienKetPhuHuynh WHERE ma_phu_huynh = 29 AND ma_hoc_sinh = 24;
