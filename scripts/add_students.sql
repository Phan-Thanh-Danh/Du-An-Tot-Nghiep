BEGIN TRAN;
DELETE FROM ThiSinhCaThi WHERE ma_ca_thi IN (6, 7);

INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
SELECT 6, ma_nguoi_dung, 'cho_thi', GETDATE()
FROM NguoiDung WHERE ma_lop = 4;

INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
SELECT 7, ma_nguoi_dung, 'cho_thi', GETDATE()
FROM NguoiDung WHERE ma_lop = 4;

COMMIT TRAN;
