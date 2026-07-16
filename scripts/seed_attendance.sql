INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
SELECT 
    1,
    41, 
    n.ma_nguoi_dung, 
    'co_mat', 
    15, 
    GETDATE(), 
    0
FROM NguoiDung n
WHERE n.ma_lop = 4 
AND NOT EXISTS (
    SELECT 1 FROM DiemDanh d WHERE d.ma_buoi_hoc = 41 AND d.ma_hoc_sinh = n.ma_nguoi_dung
);
