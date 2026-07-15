SET QUOTED_IDENTIFIER ON;
GO

DECLARE @StudentId INT = 24;

-- Xóa dữ liệu điểm danh cũ
DELETE FROM DiemDanh WHERE ma_hoc_sinh = @StudentId;

-- Helper table cho các trạng thái điểm danh
DECLARE @Statuses TABLE (ID INT, status_val NVARCHAR(50));
INSERT INTO @Statuses (ID, status_val) VALUES (1, N'co_mat'), (2, N'co_mat'), (3, N'co_mat'), (4, N'co_mat'), (5, N'co_mat'), (6, N'di_muon'), (7, N'vang'), (8, N'co_phep');

DECLARE @StartDate DATE = '2026-06-01';
DECLARE @EndDate DATE = '2026-07-15';
DECLARE @CurrDate DATE = @StartDate;

WHILE @CurrDate <= @EndDate
BEGIN
    DECLARE @DayOfWeek INT = DATEPART(dw, @CurrDate);
    DECLARE @RandId INT;
    DECLARE @Status NVARCHAR(50);
    
    -- Xử lý Thứ 2
    IF @DayOfWeek = 2
    BEGIN
        -- Ca 1
        DECLARE @Tkb1 INT = (SELECT TOP 1 ma_tkb FROM ThoiKhoaBieu WHERE ma_khoa_hoc = 4 AND ma_ca_hoc = 1);
        IF @Tkb1 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM BuoiHoc WHERE ma_tkb = @Tkb1 AND ngay_hoc = @CurrDate)
                INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
                VALUES (@Tkb1, 4, 1, 1, 4, @CurrDate, N'da_khoa', N'Seed quá khứ');
            
            DECLARE @Bh1 INT = (SELECT ma_buoi_hoc FROM BuoiHoc WHERE ma_tkb = @Tkb1 AND ngay_hoc = @CurrDate);
            SET @RandId = (ABS(CHECKSUM(NEWID())) % 8) + 1;
            SET @Status = (SELECT status_val FROM @Statuses WHERE ID = @RandId);
            IF @Status IS NULL SET @Status = N'co_mat';
            
            INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
            VALUES (1, @Bh1, @StudentId, @Status, 4, GETDATE(), CASE WHEN @Status = N'vang' THEN 1 ELSE 0 END);
        END

        -- Ca 2
        DECLARE @Tkb2 INT = (SELECT TOP 1 ma_tkb FROM ThoiKhoaBieu WHERE ma_khoa_hoc = 4 AND ma_ca_hoc = 2);
        IF @Tkb2 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM BuoiHoc WHERE ma_tkb = @Tkb2 AND ngay_hoc = @CurrDate)
                INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
                VALUES (@Tkb2, 4, 1, 2, 4, @CurrDate, N'da_khoa', N'Seed quá khứ');
            
            DECLARE @Bh2 INT = (SELECT ma_buoi_hoc FROM BuoiHoc WHERE ma_tkb = @Tkb2 AND ngay_hoc = @CurrDate);
            SET @RandId = (ABS(CHECKSUM(NEWID())) % 8) + 1;
            SET @Status = (SELECT status_val FROM @Statuses WHERE ID = @RandId);
            IF @Status IS NULL SET @Status = N'co_mat';
            
            INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
            VALUES (1, @Bh2, @StudentId, @Status, 4, GETDATE(), CASE WHEN @Status = N'vang' THEN 1 ELSE 0 END);
        END
    END
    
    -- Xử lý Thứ 4
    IF @DayOfWeek = 4
    BEGIN
        DECLARE @Tkb3 INT = (SELECT TOP 1 ma_tkb FROM ThoiKhoaBieu WHERE ma_khoa_hoc = 10 AND ma_ca_hoc = 1);
        IF @Tkb3 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM BuoiHoc WHERE ma_tkb = @Tkb3 AND ngay_hoc = @CurrDate)
                INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
                VALUES (@Tkb3, 10, 1, 1, 4, @CurrDate, N'da_khoa', N'Seed quá khứ');
            
            DECLARE @Bh3 INT = (SELECT ma_buoi_hoc FROM BuoiHoc WHERE ma_tkb = @Tkb3 AND ngay_hoc = @CurrDate);
            SET @RandId = (ABS(CHECKSUM(NEWID())) % 8) + 1;
            SET @Status = (SELECT status_val FROM @Statuses WHERE ID = @RandId);
            IF @Status IS NULL SET @Status = N'co_mat';
            
            INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
            VALUES (1, @Bh3, @StudentId, @Status, 4, GETDATE(), CASE WHEN @Status = N'vang' THEN 1 ELSE 0 END);
        END

        DECLARE @Tkb4 INT = (SELECT TOP 1 ma_tkb FROM ThoiKhoaBieu WHERE ma_khoa_hoc = 16 AND ma_ca_hoc = 2);
        IF @Tkb4 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM BuoiHoc WHERE ma_tkb = @Tkb4 AND ngay_hoc = @CurrDate)
                INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
                VALUES (@Tkb4, 16, 1, 2, 5, @CurrDate, N'da_khoa', N'Seed quá khứ');
            
            DECLARE @Bh4 INT = (SELECT ma_buoi_hoc FROM BuoiHoc WHERE ma_tkb = @Tkb4 AND ngay_hoc = @CurrDate);
            SET @RandId = (ABS(CHECKSUM(NEWID())) % 8) + 1;
            SET @Status = (SELECT status_val FROM @Statuses WHERE ID = @RandId);
            IF @Status IS NULL SET @Status = N'co_mat';
            
            INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
            VALUES (1, @Bh4, @StudentId, @Status, 5, GETDATE(), CASE WHEN @Status = N'vang' THEN 1 ELSE 0 END);
        END
    END
    
    -- Xử lý Thứ 6
    IF @DayOfWeek = 6
    BEGIN
        DECLARE @Tkb5 INT = (SELECT TOP 1 ma_tkb FROM ThoiKhoaBieu WHERE ma_khoa_hoc = 16 AND ma_ca_hoc = 3);
        IF @Tkb5 IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM BuoiHoc WHERE ma_tkb = @Tkb5 AND ngay_hoc = @CurrDate)
                INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_diem_danh, ghi_chu)
                VALUES (@Tkb5, 16, 1, 3, 5, @CurrDate, N'da_khoa', N'Seed quá khứ');
            
            DECLARE @Bh5 INT = (SELECT ma_buoi_hoc FROM BuoiHoc WHERE ma_tkb = @Tkb5 AND ngay_hoc = @CurrDate);
            SET @RandId = (ABS(CHECKSUM(NEWID())) % 8) + 1;
            SET @Status = (SELECT status_val FROM @Statuses WHERE ID = @RandId);
            IF @Status IS NULL SET @Status = N'co_mat';
            
            INSERT INTO DiemDanh (ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
            VALUES (1, @Bh5, @StudentId, @Status, 5, GETDATE(), CASE WHEN @Status = N'vang' THEN 1 ELSE 0 END);
        END
    END

    SET @CurrDate = DATEADD(day, 1, @CurrDate);
END
GO
