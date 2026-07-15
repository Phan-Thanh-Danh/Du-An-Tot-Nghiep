SET QUOTED_IDENTIFIER ON;
GO

-- Xóa dữ liệu cũ (nếu có) để tránh trùng lặp
DELETE FROM BuoiHoc WHERE ghi_chu = N'Auto-generated for student P12';
DELETE FROM ThoiKhoaBieu WHERE ma_khoa_hoc IN (4, 10, 16) AND trang_thai = 'da_xuat_ban' AND ngay_bat_dau = '2026-07-06';
GO

-- Khai báo biến lưu mã TKB
DECLARE @tkb1 INT, @tkb2 INT, @tkb3 INT, @tkb4 INT, @tkb5 INT;

-- Thứ 2: "2 ca 1 môn" (Ca 1 & 2: Kỹ năng học tập - ma_khoa_hoc = 4)
INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, trang_thai)
VALUES (4, 1, 1, 2, '2026-07-06', 'da_xuat_ban');
SET @tkb1 = SCOPE_IDENTITY();

INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, trang_thai)
VALUES (4, 1, 2, 2, '2026-07-06', 'da_xuat_ban');
SET @tkb2 = SCOPE_IDENTITY();

-- Thứ 4: "2 ca 2 môn" (Ca 1: Tin học cơ bản - ma_khoa_hoc = 10; Ca 2: Nhập môn lập trình - ma_khoa_hoc = 16)
INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, trang_thai)
VALUES (10, 1, 1, 4, '2026-07-06', 'da_xuat_ban');
SET @tkb3 = SCOPE_IDENTITY();

INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, trang_thai)
VALUES (16, 1, 2, 4, '2026-07-06', 'da_xuat_ban');
SET @tkb4 = SCOPE_IDENTITY();

-- Thứ 6: Ca 3 Nhập môn lập trình
INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, trang_thai)
VALUES (16, 1, 3, 6, '2026-07-06', 'da_xuat_ban');
SET @tkb5 = SCOPE_IDENTITY();

-- Thêm Buổi Học cho tuần 13/07 - 19/07
INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
SELECT @tkb1, 4, 1, 1, k.ma_giao_vien, '2026-07-13', N'chua_mo', N'Auto-generated for student P12'
FROM KhoaHoc k WHERE k.ma_khoa_hoc = 4;

INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
SELECT @tkb2, 4, 1, 2, k.ma_giao_vien, '2026-07-13', N'chua_mo', N'Auto-generated for student P12'
FROM KhoaHoc k WHERE k.ma_khoa_hoc = 4;

INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
SELECT @tkb3, 10, 1, 1, k.ma_giao_vien, '2026-07-15', N'chua_mo', N'Auto-generated for student P12'
FROM KhoaHoc k WHERE k.ma_khoa_hoc = 10;

INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
SELECT @tkb4, 16, 1, 2, k.ma_giao_vien, '2026-07-15', N'chua_mo', N'Auto-generated for student P12'
FROM KhoaHoc k WHERE k.ma_khoa_hoc = 16;

INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
SELECT @tkb5, 16, 1, 3, k.ma_giao_vien, '2026-07-17', N'chua_mo', N'Auto-generated for student P12'
FROM KhoaHoc k WHERE k.ma_khoa_hoc = 16;
GO
