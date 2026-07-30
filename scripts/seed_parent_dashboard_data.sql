USE LMS;

DELETE FROM DiemSo WHERE ma_hoc_sinh = 23;
INSERT INTO DiemSo (ma_don_vi, ma_hoc_sinh, ma_mon_hoc, ma_hoc_ky, diem_qua_trinh, diem_giua_ky, diem_cuoi_ky, gpa_mon_hoc, trang_thai, da_khoa, nam_nhap_hoc)
VALUES
  (3, 23, 50, 1, 8.0, 7.5, 8.5, 8.0, 'da_co_diem', 1, 2024),
  (3, 23, 50, 2, 7.0, 7.0, 7.5, 7.2, 'da_co_diem', 1, 2024),
  (3, 23, 50, 3, 8.5, 9.0, 8.0, 8.5, 'da_co_diem', 0, 2024);

DELETE FROM DiemDanh WHERE ma_hoc_sinh = 23;
INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
SELECT TOP 20 3, ma_buoi_hoc, 23,
  CASE
    WHEN ROW_NUMBER() OVER (ORDER BY ma_buoi_hoc) % 10 = 0 THEN 'vang'
    WHEN ROW_NUMBER() OVER (ORDER BY ma_buoi_hoc) % 7 = 0 THEN 'di_muon'
    ELSE 'co_mat'
  END,
  15, GETDATE(), 1.0
FROM BuoiHoc
WHERE ma_khoa_hoc IN (1,2,3,4,5);

DELETE FROM HoaDon WHERE ma_hoc_sinh = 23;
INSERT INTO HoaDon (ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code, loai_hoa_don, so_tien, giam_tru, da_thanh_toan, trang_thai, han_thanh_toan, ngay_tao, nguoi_tao)
VALUES
  (3, 23, 1, 'HD-2024-001-23', 'hoc_phi', 8500000, 0, 8500000, 'da_thanh_toan', '2024-09-30', GETDATE(), 1),
  (3, 23, 2, 'HD-2024-002-23', 'hoc_phi', 8500000, 500000, 0, 'chua_thanh_toan', '2025-03-15', GETDATE(), 1);

INSERT INTO LienKetPhuHuynh (ma_phu_huynh, ma_hoc_sinh, quyen_xem, trang_thai, lien_ket_luc)
SELECT 29, nd.ma_nguoi_dung, 'grades,attendance,tuition,schedule,alerts', 'hoat_dong', GETDATE()
FROM NguoiDung nd
WHERE nd.email = 'student01@edulms.local'
  AND NOT EXISTS (SELECT 1 FROM LienKetPhuHuynh WHERE ma_phu_huynh = 29 AND ma_hoc_sinh = nd.ma_nguoi_dung);

SELECT 'DiemSo' as bang, COUNT(*) as sl FROM DiemSo WHERE ma_hoc_sinh = 23
UNION ALL SELECT 'DiemDanh', COUNT(*) FROM DiemDanh WHERE ma_hoc_sinh = 23
UNION ALL SELECT 'HoaDon', COUNT(*) FROM HoaDon WHERE ma_hoc_sinh = 23
UNION ALL SELECT 'LienKetPhuHuynh', COUNT(*) FROM LienKetPhuHuynh WHERE ma_phu_huynh = 29;
