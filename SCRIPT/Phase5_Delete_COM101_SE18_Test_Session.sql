USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SessionMarker NVARCHAR(500) = N'Phase5_COM101_SE18_TEST';
DECLARE @SessionId INT;
DECLARE @ScheduleId INT;
DECLARE @SessionCount INT;
DECLARE @ScheduleDeleted INT = 0;

PRINT N'--- XÓA CA THI TEST COM101 - SE18 ---';

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT TOP 1
        @SessionId = ma_ca_thi,
        @ScheduleId = ma_lich_thi_tong
    FROM CaThi
    WHERE ghi_chu = @SessionMarker;

    SELECT @SessionCount = COUNT(*)
    FROM CaThi
    WHERE ghi_chu = @SessionMarker;

    IF @SessionId IS NULL
    BEGIN
        PRINT N'Không tìm thấy ca test Phase5_COM101_SE18_TEST.';
        COMMIT TRANSACTION;
        RETURN;
    END

    DELETE xv
    FROM XuLyViPhamThi xv
    INNER JOIN NhatKyViPhamThi vp ON vp.ma_vi_pham = xv.ma_vi_pham
    WHERE vp.ma_ca_thi = @SessionId;

    DELETE FROM NhatKyViPhamThi
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM BienBanThi
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM DiemDanhThi
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM PhanCongGiamThi
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM ThiSinhCaThi
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM PhienThiHocSinh
    WHERE ma_ca_thi = @SessionId;

    DELETE FROM CaThi
    WHERE ma_ca_thi = @SessionId;

    IF NOT EXISTS
    (
        SELECT 1
        FROM CaThi
        WHERE ma_lich_thi_tong = @ScheduleId
    )
    BEGIN
        DELETE FROM LichThiTong
        WHERE ma_lich_thi_tong = @ScheduleId;
        SET @ScheduleDeleted = @@ROWCOUNT;
    END

    COMMIT TRANSACTION;

    SELECT
        @SessionCount AS ca_test_da_xoa,
        @ScheduleDeleted AS lich_thi_tong_da_xoa,
        N'Đề thi và câu hỏi được giữ lại' AS ghi_chu;

    PRINT N'--- ĐÃ XÓA CA TEST COM101 - SE18 ---';
    PRINT N'--- Có thể chạy lại Phase5_Add_COM101_SE18_Test_Session.sql ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI XÓA CA TEST COM101 - SE18 !!!';
    THROW;
END CATCH;
GO
