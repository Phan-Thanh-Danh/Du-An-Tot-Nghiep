USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @Marker NVARCHAR(500) = N'Phase5_Test;CaThi=1';
DECLARE @ScheduleId INT;
DECLARE @ExamId INT;
DECLARE @CampusId INT;
DECLARE @RoomId INT;
DECLARE @SessionId INT;
DECLARE @TeacherId INT;
DECLARE @ExamDate DATETIME2 = DATEADD(DAY, 1, @CurrentDate);
DECLARE @StartTime DATETIME2 = DATEADD(HOUR, 8, CAST(CAST(@ExamDate AS DATE) AS DATETIME2));
DECLARE @EndTime DATETIME2 = DATEADD(HOUR, 2, @StartTime);

PRINT N'--- TẠO 1 CA THI TEST ---';
PRINT N'--- Ca thi sẽ ở trạng thái nhap, chưa mở ---';

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM CaThi WHERE ghi_chu = @Marker)
        THROW 53001, 'Ca thi test da ton tai. Khong tao trung.', 1;

    -- Chọn lịch thi đã có đề thi để sinh viên nhìn thấy bài thi.
    SELECT TOP 1
        @ScheduleId = l.ma_lich_thi_tong,
        @ExamId = l.ma_de_kiem_tra,
        @CampusId = h.ma_don_vi
    FROM LichThiTong l
    INNER JOIN DeKiemTra d ON d.ma_de_kiem_tra = l.ma_de_kiem_tra
    INNER JOIN HocKy h ON h.ma_hoc_ky = l.ma_ky_thi
    WHERE l.ma_de_kiem_tra IS NOT NULL
      AND l.trang_thai <> 'da_huy'
      AND d.trang_thai <> 'nhap'
    ORDER BY l.ma_lich_thi_tong;

    IF @ScheduleId IS NULL OR @ExamId IS NULL OR @CampusId IS NULL
        THROW 53002, 'Khong tim thay LichThiTong da co DeKiemTra de tao ca test.', 1;

    SELECT TOP 1 @RoomId = p.ma_phong
    FROM PhongHoc p
    WHERE p.ma_don_vi = @CampusId
      AND p.trang_thai_phong = 'hoat_dong'
      AND NOT EXISTS
      (
          SELECT 1
          FROM CaThi c
          WHERE c.ma_phong = p.ma_phong
            AND c.trang_thai <> 'da_huy'
            AND c.thoi_gian_bat_dau < @EndTime
            AND c.thoi_gian_ket_thuc > @StartTime
      )
    ORDER BY p.ma_phong;

    IF @RoomId IS NULL
        THROW 53003, 'Khong tim thay phong hoc trong khung gio test.', 1;

    SELECT TOP 1 @TeacherId = u.ma_nguoi_dung
    FROM NguoiDung u
    WHERE u.ma_don_vi = @CampusId
      AND u.vai_tro_chinh = 'giao_vien'
      AND u.trang_thai = 'hoat_dong'
    ORDER BY u.ma_nguoi_dung;

    IF @TeacherId IS NULL
        THROW 53004, 'Khong tim thay giang vien test.', 1;

    INSERT INTO CaThi
        (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi,
         thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi,
         trang_thai, ghi_chu, ngay_tao)
    SELECT
        @ScheduleId,
        LEFT(d.tieu_de + N' - Bài thi TEST', 100),
        @RoomId,
        @ExamDate,
        @StartTime,
        @EndTime,
        @CampusId,
        'nhap',
        @Marker,
        @CurrentDate
    FROM DeKiemTra d
    WHERE d.ma_de_kiem_tra = @ExamId;

    SET @SessionId = SCOPE_IDENTITY();

    -- Lấy tối đa 35 sinh viên cùng cơ sở làm dữ liệu test.
    INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
    SELECT TOP 35
        @SessionId,
        u.ma_nguoi_dung,
        'cho_thi',
        @CurrentDate
    FROM NguoiDung u
    WHERE u.ma_don_vi = @CampusId
      AND u.vai_tro_chinh = 'hoc_sinh'
      AND u.trang_thai = 'hoat_dong'
    ORDER BY u.ma_nguoi_dung;

    INSERT INTO PhanCongGiamThi
        (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
    VALUES
        (@SessionId, @TeacherId, 'giam_thi_chinh', 'du_kien', @CurrentDate);

    COMMIT TRANSACTION;

    SELECT
        @SessionId AS ma_ca_thi,
        @ScheduleId AS ma_lich_thi_tong,
        @ExamId AS ma_de_kiem_tra,
        @CampusId AS ma_don_vi,
        @RoomId AS ma_phong,
        @TeacherId AS ma_giam_thi,
        'nhap' AS trang_thai_ca_thi,
        'cho_thi' AS trang_thai_thi_sinh;

    PRINT N'--- ĐÃ TẠO CA THI TEST ---';
    PRINT N'--- Vào màn điểm danh, bấm Điểm danh tất cả có mặt, sau đó Bắt đầu canh thi ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI TẠO CA THI TEST !!!';
    THROW;
END CATCH;
GO
