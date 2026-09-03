USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @CurrentUserId INT;

PRINT N'--- CẬP NHẬT TRẠNG THÁI CA THI PHASE 5 ---';
PRINT N'--- Không tạo thêm dữ liệu mới ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- Chọn một giám thị Phase 5 làm người điểm danh hệ thống.
    SELECT TOP 1 @CurrentUserId = p.ma_giam_thi
    FROM PhanCongGiamThi p
    INNER JOIN CaThi c ON c.ma_ca_thi = p.ma_ca_thi
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
      AND p.trang_thai <> 'huy_phan_cong'
    ORDER BY p.ma_phan_cong;

    IF @CurrentUserId IS NULL
        THROW 52001, 'Khong tim thay giam thi cho ca Phase 5.', 1;

    -- Đề thi đã được lên lịch để sinh viên nhận diện đúng bài thi.
    UPDATE d
    SET d.trang_thai = CASE WHEN d.trang_thai = 'nhap' THEN 'da_len_lich' ELSE d.trang_thai END,
        d.trang_thai_duyet = CASE WHEN d.trang_thai_duyet IS NULL THEN 'da_duyet' ELSE d.trang_thai_duyet END,
        d.ngay_cap_nhat = @CurrentDate
    FROM DeKiemTra d
    INNER JOIN LichThiTong l ON l.ma_de_kiem_tra = d.ma_de_kiem_tra
    INNER JOIN CaThi c ON c.ma_lich_thi_tong = l.ma_lich_thi_tong
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%';

    -- Đây là bước làm bài thực tế: ca phải dang_thi thì API StartExam mới cho vào.
    UPDATE c
    SET c.trang_thai = 'dang_thi',
        c.ngay_cap_nhat = @CurrentDate
    FROM CaThi c
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
      AND c.trang_thai NOT IN ('da_ket_thuc', 'da_huy');

    -- Đánh có mặt toàn bộ thí sinh, trừ người đã đình chỉ.
    UPDATE t
    SET t.trang_thai_du_thi = 'duoc_thi'
    FROM ThiSinhCaThi t
    INNER JOIN CaThi c ON c.ma_ca_thi = t.ma_ca_thi
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
      AND c.trang_thai = 'dang_thi'
      AND t.trang_thai_du_thi <> 'dinh_chi';

    UPDATE d
    SET d.trang_thai_diem_danh = 'co_mat',
        d.thoi_diem_diem_danh = @CurrentDate,
        d.ma_nguoi_diem_danh = COALESCE(d.ma_nguoi_diem_danh, @CurrentUserId),
        d.ghi_chu = N'Cập nhật trạng thái Phase 5: có mặt'
    FROM DiemDanhThi d
    INNER JOIN CaThi c ON c.ma_ca_thi = d.ma_ca_thi
    INNER JOIN ThiSinhCaThi t ON t.ma_ca_thi = d.ma_ca_thi AND t.ma_hoc_sinh = d.ma_hoc_sinh
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
      AND c.trang_thai = 'dang_thi'
      AND t.trang_thai_du_thi = 'duoc_thi';

    INSERT INTO DiemDanhThi
        (ma_ca_thi, ma_hoc_sinh, trang_thai_diem_danh,
         thoi_diem_diem_danh, ma_nguoi_diem_danh, ghi_chu, ngay_tao)
    SELECT
        t.ma_ca_thi,
        t.ma_hoc_sinh,
        'co_mat',
        @CurrentDate,
        @CurrentUserId,
        N'Cập nhật trạng thái Phase 5: có mặt',
        @CurrentDate
    FROM ThiSinhCaThi t
    INNER JOIN CaThi c ON c.ma_ca_thi = t.ma_ca_thi
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
      AND c.trang_thai = 'dang_thi'
      AND t.trang_thai_du_thi = 'duoc_thi'
      AND NOT EXISTS
      (
          SELECT 1
          FROM DiemDanhThi d
          WHERE d.ma_ca_thi = t.ma_ca_thi
            AND d.ma_hoc_sinh = t.ma_hoc_sinh
      );

    COMMIT TRANSACTION;

    SELECT
        COUNT(*) AS tong_ca_thi,
        SUM(CASE WHEN c.trang_thai = 'dang_thi' THEN 1 ELSE 0 END) AS ca_dang_thi,
        SUM(CASE WHEN c.trang_thai <> 'dang_thi' THEN 1 ELSE 0 END) AS ca_chua_dang_thi
    FROM CaThi c
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%';

    SELECT
        COUNT(*) AS tong_thi_sinh,
        SUM(CASE WHEN t.trang_thai_du_thi = 'duoc_thi' THEN 1 ELSE 0 END) AS thi_sinh_duoc_thi,
        SUM(CASE WHEN t.trang_thai_du_thi = 'dinh_chi' THEN 1 ELSE 0 END) AS thi_sinh_dinh_chi
    FROM ThiSinhCaThi t
    INNER JOIN CaThi c ON c.ma_ca_thi = t.ma_ca_thi
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%';

    PRINT N'--- ĐÃ CẬP NHẬT: CA dang_thi, THÍ SINH duoc_thi, ĐIỂM DANH co_mat ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI CẬP NHẬT TRẠNG THÁI PHASE 5 !!!';
    THROW;
END CATCH;
GO
