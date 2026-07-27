USE LMS;
GO

-- Open all CaThi and DeKiemTra for testing
UPDATE CaThi SET trang_thai = 'dang_thi';
UPDATE DeKiemTra SET trang_thai = 'dang_mo';
UPDATE ThiSinhCaThi SET trang_thai_du_thi = 'duoc_thi';

SELECT ma_ca_thi, ten_ca_thi, trang_thai FROM CaThi;
SELECT ma_de_kiem_tra, tieu_de, trang_thai FROM DeKiemTra;
GO
