USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT N'--- XÓA DỮ LIỆU CA THI/LỊCH THI DO PHASE 5 TẠO ---';
PRINT N'--- Chỉ xóa bản ghi có marker Phase5_AllCourses ---';

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('tempdb..#Phase5Sessions') IS NOT NULL
        DROP TABLE #Phase5Sessions;

    IF OBJECT_ID('tempdb..#Phase5Schedules') IS NOT NULL
        DROP TABLE #Phase5Schedules;

    CREATE TABLE #Phase5Sessions
    (
        ma_ca_thi INT NOT NULL PRIMARY KEY
    );

    CREATE TABLE #Phase5Schedules
    (
        ma_lich_thi_tong INT NOT NULL PRIMARY KEY
    );

    DECLARE @SessionCount INT;
    DECLARE @ScheduleCount INT;
    DECLARE @DeletedSchedules INT;

    INSERT INTO #Phase5Sessions (ma_ca_thi)
    SELECT ma_ca_thi
    FROM CaThi
    WHERE ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%';

    INSERT INTO #Phase5Schedules (ma_lich_thi_tong)
    SELECT DISTINCT c.ma_lich_thi_tong
    FROM CaThi c
    INNER JOIN #Phase5Sessions s ON s.ma_ca_thi = c.ma_ca_thi;

    SELECT @SessionCount = COUNT(*) FROM #Phase5Sessions;
    SELECT @ScheduleCount = COUNT(*) FROM #Phase5Schedules;

    PRINT N'Số ca thi sẽ xóa: ' + CAST(@SessionCount AS NVARCHAR(20));
    PRINT N'Số lịch thi tổng sẽ xóa: ' + CAST(@ScheduleCount AS NVARCHAR(20));

    -- Xóa bảng con trước vì các FK đều dùng DeleteBehavior.NoAction.
    DELETE xv
    FROM XuLyViPhamThi xv
    INNER JOIN NhatKyViPhamThi vp ON vp.ma_vi_pham = xv.ma_vi_pham
    INNER JOIN #Phase5Sessions s ON s.ma_ca_thi = vp.ma_ca_thi;

    DELETE FROM NhatKyViPhamThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM BienBanThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM DiemDanhThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM PhanCongGiamThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM ThiSinhCaThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM PhienThiHocSinh
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    DELETE FROM CaThi
    WHERE ma_ca_thi IN (SELECT ma_ca_thi FROM #Phase5Sessions);

    -- Chỉ xóa lịch không còn ca thi nào tham chiếu.
    DELETE l
    FROM LichThiTong l
    INNER JOIN #Phase5Schedules s ON s.ma_lich_thi_tong = l.ma_lich_thi_tong
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM CaThi c
        WHERE c.ma_lich_thi_tong = l.ma_lich_thi_tong
    );
    SET @DeletedSchedules = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @SessionCount AS ca_thi_da_xoa,
        @DeletedSchedules AS lich_thi_tong_da_xoa;

    PRINT N'--- ĐÃ XÓA CA THI VÀ LỊCH THI PHASE 5 ---';
    PRINT N'--- Không xóa DeKiemTra, KyThi hoặc dữ liệu Phase 1-4 ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT N'!!! LỖI KHI XÓA DỮ LIỆU PHASE 5 !!!';
    THROW;
END CATCH;
GO
