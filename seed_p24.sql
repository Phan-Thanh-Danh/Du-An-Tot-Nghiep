USE LMS;

DECLARE @StudentId INT;
DECLARE @ClassId INT;

SELECT @StudentId = ma_nguoi_dung, @ClassId = ma_lop 
FROM dbo.NguoiDung WHERE email = 'student.cntt01@lms.local';

IF @ClassId IS NULL
BEGIN
    PRINT 'Student does not have a class. Seeding aborted.';
    RETURN;
END

DECLARE @DonViId INT = (SELECT TOP 1 ma_don_vi FROM dbo.DonVi);

DECLARE @Today DATE = CAST(GETDATE() AT TIME ZONE 'UTC' AT TIME ZONE 'SE Asia Standard Time' AS DATE);
DECLARE @CurrentTermStart DATE = DATEADD(DAY, -30, @Today);
DECLARE @CurrentTermEnd DATE = DATEADD(DAY, 90, @Today);
DECLARE @UpcomingTermStart DATE = DATEADD(DAY, 5, @Today);
DECLARE @UpcomingTermEnd DATE = DATEADD(DAY, 125, @Today);
DECLARE @FarTermStart DATE = DATEADD(DAY, 10, @Today);
DECLARE @FarTermEnd DATE = DATEADD(DAY, 130, @Today);

-- Ensure terms exist
IF NOT EXISTS (SELECT 1 FROM dbo.HocKy WHERE ma_code_hoc_ky = 'SP24_CUR' AND ma_don_vi = @DonViId)
BEGIN
    INSERT INTO dbo.HocKy (ma_code_hoc_ky, ten_hoc_ky, nam_hoc, ngay_bat_dau, ngay_ket_thuc, ma_don_vi, thu_tu_trong_nam, da_khoa)
    VALUES ('SP24_CUR', N'Học kỳ SP24 (Current)', '2024-2025', @CurrentTermStart, @CurrentTermEnd, @DonViId, 1, 0);
END
ELSE
BEGIN
    UPDATE dbo.HocKy SET ngay_bat_dau = @CurrentTermStart, ngay_ket_thuc = @CurrentTermEnd WHERE ma_code_hoc_ky = 'SP24_CUR' AND ma_don_vi = @DonViId;
END

IF NOT EXISTS (SELECT 1 FROM dbo.HocKy WHERE ma_code_hoc_ky = 'SU24_UPC' AND ma_don_vi = @DonViId)
BEGIN
    INSERT INTO dbo.HocKy (ma_code_hoc_ky, ten_hoc_ky, nam_hoc, ngay_bat_dau, ngay_ket_thuc, ma_don_vi, thu_tu_trong_nam, da_khoa)
    VALUES ('SU24_UPC', N'Học kỳ SU24 (Upcoming 5 days)', '2024-2025', @UpcomingTermStart, @UpcomingTermEnd, @DonViId, 2, 0);
END
ELSE
BEGIN
    UPDATE dbo.HocKy SET ngay_bat_dau = @UpcomingTermStart, ngay_ket_thuc = @UpcomingTermEnd WHERE ma_code_hoc_ky = 'SU24_UPC' AND ma_don_vi = @DonViId;
END

IF NOT EXISTS (SELECT 1 FROM dbo.HocKy WHERE ma_code_hoc_ky = 'FA24_FAR' AND ma_don_vi = @DonViId)
BEGIN
    INSERT INTO dbo.HocKy (ma_code_hoc_ky, ten_hoc_ky, nam_hoc, ngay_bat_dau, ngay_ket_thuc, ma_don_vi, thu_tu_trong_nam, da_khoa)
    VALUES ('FA24_FAR', N'Học kỳ FA24 (Upcoming 10 days)', '2024-2025', @FarTermStart, @FarTermEnd, @DonViId, 3, 0);
END
ELSE
BEGIN
    UPDATE dbo.HocKy SET ngay_bat_dau = @FarTermStart, ngay_ket_thuc = @FarTermEnd WHERE ma_code_hoc_ky = 'FA24_FAR' AND ma_don_vi = @DonViId;
END

DECLARE @TermCur INT = (SELECT ma_hoc_ky FROM dbo.HocKy WHERE ma_code_hoc_ky = 'SP24_CUR' AND ma_don_vi = @DonViId);
DECLARE @TermUpc INT = (SELECT ma_hoc_ky FROM dbo.HocKy WHERE ma_code_hoc_ky = 'SU24_UPC' AND ma_don_vi = @DonViId);
DECLARE @TermFar INT = (SELECT ma_hoc_ky FROM dbo.HocKy WHERE ma_code_hoc_ky = 'FA24_FAR' AND ma_don_vi = @DonViId);

DECLARE @TeacherId INT = (SELECT TOP 1 ma_nguoi_dung FROM dbo.NguoiDung WHERE email = 'teacher.cntt@lms.local');
DECLARE @MonHocId INT = (SELECT TOP 1 ma_mon_hoc FROM dbo.DanhMucMonHoc WHERE ma_code_mon_hoc = 'CTDL101');
DECLARE @PhongId INT = (SELECT TOP 1 ma_phong FROM dbo.PhongHoc WHERE ma_code_phong = 'A101');
DECLARE @CaHocId INT = (SELECT TOP 1 ma_ca_hoc FROM dbo.CaHoc WHERE thu_tu = 1);
DECLARE @CaHocId2 INT = (SELECT TOP 1 ma_ca_hoc FROM dbo.CaHoc WHERE thu_tu = 2);

-- Insert KhoaHoc for each term
IF NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermCur AND ma_mon_hoc = @MonHocId)
BEGIN
    INSERT INTO dbo.KhoaHoc (tieu_de, ma_hoc_ky, ma_lop, ma_mon_hoc, ma_giao_vien, ma_don_vi, trang_thai, ngay_tao)
    VALUES (N'Khoá học CTDL101 (Cur)', @TermCur, @ClassId, @MonHocId, @TeacherId, @DonViId, 'da_xuat_ban', GETDATE());
END

DECLARE @KhCur INT = (SELECT TOP 1 ma_khoa_hoc FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermCur AND ma_mon_hoc = @MonHocId);

IF NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermUpc AND ma_mon_hoc = @MonHocId)
BEGIN
    INSERT INTO dbo.KhoaHoc (tieu_de, ma_hoc_ky, ma_lop, ma_mon_hoc, ma_giao_vien, ma_don_vi, trang_thai, ngay_tao)
    VALUES (N'Khoá học CTDL101 (Upc)', @TermUpc, @ClassId, @MonHocId, @TeacherId, @DonViId, 'da_xuat_ban', GETDATE());
END

DECLARE @KhUpc INT = (SELECT TOP 1 ma_khoa_hoc FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermUpc AND ma_mon_hoc = @MonHocId);

IF NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermFar AND ma_mon_hoc = @MonHocId)
BEGIN
    INSERT INTO dbo.KhoaHoc (tieu_de, ma_hoc_ky, ma_lop, ma_mon_hoc, ma_giao_vien, ma_don_vi, trang_thai, ngay_tao)
    VALUES (N'Khoá học CTDL101 (Far)', @TermFar, @ClassId, @MonHocId, @TeacherId, @DonViId, 'da_xuat_ban', GETDATE());
END

DECLARE @KhFar INT = (SELECT TOP 1 ma_khoa_hoc FROM dbo.KhoaHoc WHERE ma_lop = @ClassId AND ma_hoc_ky = @TermFar AND ma_mon_hoc = @MonHocId);

-- Insert ThoiKhoaBieu
IF NOT EXISTS (SELECT 1 FROM dbo.ThoiKhoaBieu WHERE ma_khoa_hoc = @KhCur)
BEGIN
    INSERT INTO dbo.ThoiKhoaBieu (ma_khoa_hoc, thu_trong_tuan, ma_ca_hoc, ma_phong, ngay_bat_dau, ngay_ket_thuc, trang_thai)
    VALUES (@KhCur, 2, @CaHocId, @PhongId, @CurrentTermStart, @CurrentTermEnd, 'da_xuat_ban');
END
DECLARE @TkbCur INT = (SELECT TOP 1 ma_tkb FROM dbo.ThoiKhoaBieu WHERE ma_khoa_hoc = @KhCur);

IF NOT EXISTS (SELECT 1 FROM dbo.ThoiKhoaBieu WHERE ma_khoa_hoc = @KhUpc)
BEGIN
    INSERT INTO dbo.ThoiKhoaBieu (ma_khoa_hoc, thu_trong_tuan, ma_ca_hoc, ma_phong, ngay_bat_dau, ngay_ket_thuc, trang_thai)
    VALUES (@KhUpc, 3, @CaHocId, @PhongId, @UpcomingTermStart, @UpcomingTermEnd, 'da_xuat_ban');
END

IF NOT EXISTS (SELECT 1 FROM dbo.ThoiKhoaBieu WHERE ma_khoa_hoc = @KhFar)
BEGIN
    INSERT INTO dbo.ThoiKhoaBieu (ma_khoa_hoc, thu_trong_tuan, ma_ca_hoc, ma_phong, ngay_bat_dau, ngay_ket_thuc, trang_thai)
    VALUES (@KhFar, 4, @CaHocId, @PhongId, @FarTermStart, @FarTermEnd, 'da_xuat_ban');
END

-- Delete existing BuoiHoc for these KhoaHoc to reset
DELETE FROM dbo.BuoiHoc WHERE ma_khoa_hoc IN (@KhCur, @KhUpc, @KhFar);

-- Insert BuoiHoc (Today, Tomorrow, Yesterday, Cancelled)
INSERT INTO dbo.BuoiHoc (ma_khoa_hoc, ma_tkb, ma_phong, ma_giao_vien, ma_ca_hoc, ngay_hoc, ghi_chu, trang_thai_buoi, trang_thai_diem_danh)
VALUES 
(@KhCur, @TkbCur, @PhongId, @TeacherId, @CaHocId, @Today, N'Buổi học hôm nay', 'du_kien', 'chua_mo'),
(@KhCur, @TkbCur, @PhongId, @TeacherId, @CaHocId2, DATEADD(DAY, 2, @Today), N'Buổi học đổi ca', 'du_kien', 'chua_mo'),
(@KhCur, @TkbCur, @PhongId, @TeacherId, @CaHocId, DATEADD(DAY, 1, @Today), N'Buổi ngày mai', 'du_kien', 'chua_mo'),
(@KhCur, @TkbCur, @PhongId, @TeacherId, @CaHocId, DATEADD(DAY, -1, @Today), N'Buổi hủy', 'da_huy', 'chua_mo');

PRINT 'P24 seed data inserted successfully.';
