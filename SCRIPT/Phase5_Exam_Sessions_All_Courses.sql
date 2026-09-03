USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @InvigilatorsPerSession INT = 14;
DECLARE @RoomsRequired INT = 48;
DECLARE @SlotsPerDay INT = 2;

PRINT N'--- BẮT ĐẦU PHASE 5: TẠO CA THI CHO TẤT CẢ KHÓA HỌC ---';
PRINT N'--- Trạng thái: ca thi nhap, thí sinh cho_thi, giám thị du_kien ---';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CampusId INT;
    DECLARE @CampusIndex INT = 0;
    DECLARE curCampus CURSOR LOCAL FAST_FORWARD FOR
        SELECT ma_don_vi
        FROM DonVi
        WHERE cap_don_vi = 'co_so'
        ORDER BY ma_don_vi;

    OPEN curCampus;
    FETCH NEXT FROM curCampus INTO @CampusId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @CampusIndex += 1;

        DECLARE @TermId INT;
        DECLARE @TermEnd DATETIME2;
        SELECT TOP 1
            @TermId = ma_hoc_ky,
            @TermEnd = CAST(ngay_ket_thuc AS DATETIME2)
        FROM HocKy
        WHERE ma_don_vi = @CampusId
          AND nam_hoc = '2026'
          AND thu_tu_trong_nam = 1
        ORDER BY ma_hoc_ky;

        IF @TermId IS NULL
            THROW 51001, 'Khong tim thay HocKy Thu 2026 cho co so.', 1;

        DECLARE @ExamId INT;
        SELECT @ExamId = ma_ky_thi
        FROM KyThi
        WHERE ma_hoc_ky = @TermId
          AND loai_ky_thi = 'cuoi_ky';

        IF @ExamId IS NULL
        BEGIN
            INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, loai_ky_thi, trang_thai, ngay_tao)
            VALUES (
                N'Kỳ thi cuối kỳ Thu 2026 - Cơ sở ' + CAST(@CampusIndex AS NVARCHAR(10)),
                @TermId,
                'cuoi_ky',
                'nhap',
                @CurrentDate
            );
            SET @ExamId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE KyThi
            SET trang_thai = 'nhap',
                ngay_cap_nhat = @CurrentDate
            WHERE ma_ky_thi = @ExamId
              AND trang_thai <> 'da_ket_thuc';
        END

        DECLARE @RoomCount INT;
        SELECT @RoomCount = COUNT(*)
        FROM (
            SELECT TOP (@RoomsRequired) ma_phong
            FROM PhongHoc
            WHERE ma_don_vi = @CampusId
              AND trang_thai_phong = 'hoat_dong'
            ORDER BY ma_phong
        ) AS ActiveRooms;

        IF @RoomCount < @RoomsRequired
            THROW 51002, 'Co so khong du 48 phong hoc dang hoat dong.', 1;

        DECLARE @TeacherCount INT;
        SELECT @TeacherCount = COUNT(*)
        FROM NguoiDung
        WHERE ma_don_vi = @CampusId
          AND vai_tro_chinh = 'giao_vien'
          AND trang_thai = 'hoat_dong';

        IF @TeacherCount < 1000
            THROW 51003, 'Co so khong du 1000 giang vien dang hoat dong.', 1;

        DECLARE @CourseId INT;
        DECLARE @CourseTitle NVARCHAR(500);
        DECLARE @SubjectId INT;
        DECLARE @ClassId INT;
        DECLARE @CourseIndex INT = 0;
        DECLARE @TeacherStart INT = 0;
        DECLARE @BaseExamDate DATETIME2 = DATEADD(DAY, 7, @TermEnd);

        DECLARE curCourse CURSOR LOCAL FAST_FORWARD FOR
            SELECT k.ma_khoa_hoc, k.tieu_de, k.ma_mon_hoc, k.ma_lop
            FROM KhoaHoc k
            WHERE k.ma_don_vi = @CampusId
              AND k.ma_hoc_ky = @TermId
              AND k.trang_thai IN ('da_xuat_ban', 'dang_dien_ra', 'hoat_dong')
            ORDER BY k.ma_khoa_hoc;

        OPEN curCourse;
        FETCH NEXT FROM curCourse INTO @CourseId, @CourseTitle, @SubjectId, @ClassId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @CourseIndex += 1;

            DECLARE @Marker NVARCHAR(500) =
                N'Phase5_AllCourses;MaKhoaHoc=' + CAST(@CourseId AS NVARCHAR(20));
            DECLARE @SessionId INT;
            DECLARE @ScheduleId INT;
            DECLARE @ExamDefinitionId INT;
            DECLARE @ExamTitle NVARCHAR(255) = LEFT(
                COALESCE(@CourseTitle, N'Khóa học ' + CAST(@CourseId AS NVARCHAR(20))) + N' - Bài thi cuối kỳ',
                255);

            SELECT TOP 1 @SessionId = ma_ca_thi
            FROM CaThi
            WHERE ma_don_vi = @CampusId
                            AND ghi_chu LIKE @Marker + N';%';

            IF @SessionId IS NULL
            BEGIN
                SELECT TOP 1 @ExamDefinitionId = ma_de_kiem_tra
                FROM DeKiemTra
                WHERE ma_mon_hoc = @SubjectId
                  AND ma_hoc_ky = @TermId
                  AND JSON_VALUE(cau_hinh_de_thi, '$.MaKhoaHoc') = CAST(@CourseId AS NVARCHAR(20))
                ORDER BY ma_de_kiem_tra;

                IF @ExamDefinitionId IS NULL
                BEGIN
                    INSERT INTO DeKiemTra
                        (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut,
                         cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi,
                         trang_thai_duyet, ngay_tao)
                    VALUES
                        (@SubjectId,
                         @TermId,
                         @ExamTitle,
                         120,
                         N'{"MaKhoaHoc":' + CAST(@CourseId AS NVARCHAR(20)) + N',"ChoPhepLamTruoc":false,"HienThiKetQua":false}',
                         'da_len_lich',
                         'trac_nghiem',
                         'online_tap_trung',
                         'da_duyet',
                         @CurrentDate);
                    SET @ExamDefinitionId = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    UPDATE DeKiemTra
                    SET trang_thai = CASE WHEN trang_thai = 'nhap' THEN 'da_len_lich' ELSE trang_thai END,
                        trang_thai_duyet = CASE WHEN trang_thai_duyet IS NULL THEN 'da_duyet' ELSE trang_thai_duyet END,
                        ngay_cap_nhat = @CurrentDate
                    WHERE ma_de_kiem_tra = @ExamDefinitionId;
                END

                SELECT TOP 1 @ScheduleId = ma_lich_thi_tong
                FROM LichThiTong
                WHERE ma_ky_thi = @ExamId
                  AND ma_mon_hoc = @SubjectId
                  AND trang_thai <> 'da_huy'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM CaThi c
                      WHERE c.ma_lich_thi_tong = LichThiTong.ma_lich_thi_tong
                        AND c.ma_don_vi = @CampusId
                  )
                ORDER BY ma_lich_thi_tong;

                IF @ScheduleId IS NULL
                BEGIN
                    INSERT INTO LichThiTong
                        (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi,
                         ngay_thi_du_kien, trang_thai, ngay_tao)
                    VALUES
                        (@ExamId, @SubjectId, @ExamDefinitionId, 'online_tap_trung',
                         @BaseExamDate, 'nhap', @CurrentDate);
                    SET @ScheduleId = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    UPDATE LichThiTong
                    SET trang_thai = 'nhap',
                        ngay_cap_nhat = @CurrentDate
                    WHERE ma_lich_thi_tong = @ScheduleId
                      AND trang_thai <> 'da_huy';
                END

                DECLARE @RoomId INT;
                DECLARE @RoomRank INT = ((@CourseIndex - 1) % @RoomCount) + 1;
                SELECT @RoomId = ma_phong
                FROM (
                    SELECT TOP (@RoomsRequired)
                        ma_phong,
                        ROW_NUMBER() OVER (ORDER BY ma_phong) AS room_rank
                    FROM PhongHoc
                    WHERE ma_don_vi = @CampusId
                      AND trang_thai_phong = 'hoat_dong'
                    ORDER BY ma_phong
                ) AS Rooms
                WHERE room_rank = @RoomRank;

                DECLARE @DayOffset INT = (@CourseIndex - 1) / (@RoomCount * @SlotsPerDay);
                DECLARE @SlotIndex INT = ((@CourseIndex - 1) / @RoomCount) % @SlotsPerDay;
                DECLARE @ExamDate DATETIME2 = DATEADD(DAY, @DayOffset, @BaseExamDate);
                DECLARE @StartTime DATETIME2 = DATEADD(MINUTE, CASE WHEN @SlotIndex = 0 THEN 450 ELSE 810 END, CAST(CAST(@ExamDate AS DATE) AS DATETIME2));
                DECLARE @EndTime DATETIME2 = DATEADD(HOUR, 2, @StartTime);

                IF EXISTS (
                    SELECT 1
                    FROM CaThi
                    WHERE ma_phong = @RoomId
                      AND trang_thai <> 'da_huy'
                      AND thoi_gian_bat_dau < @EndTime
                      AND thoi_gian_ket_thuc > @StartTime
                )
                    THROW 51004, 'Phong thi bi trung lich voi ca thi dang ton tai.', 1;

                INSERT INTO CaThi
                    (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi,
                     thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi,
                     trang_thai, ghi_chu, ngay_tao)
                VALUES
                    (@ScheduleId,
                     LEFT(N'Bài thi - ' + COALESCE(@CourseTitle, N'Khóa học ' + CAST(@CourseId AS NVARCHAR(20))), 100),
                     @RoomId,
                     @ExamDate,
                     @StartTime,
                     @EndTime,
                     @CampusId,
                     'nhap',
                     @Marker + N';TrangThai=chua_mo',
                     @CurrentDate);
                SET @SessionId = SCOPE_IDENTITY();

                INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
                SELECT @SessionId, u.ma_nguoi_dung, 'cho_thi', @CurrentDate
                FROM NguoiDung u
                WHERE u.ma_don_vi = @CampusId
                  AND u.ma_lop = @ClassId
                  AND u.vai_tro_chinh = 'hoc_sinh'
                  AND u.trang_thai = 'hoat_dong'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM ThiSinhCaThi t
                      WHERE t.ma_ca_thi = @SessionId
                        AND t.ma_hoc_sinh = u.ma_nguoi_dung
                  );

                ;WITH TeacherPool AS
                (
                    SELECT
                        ma_nguoi_dung,
                        ROW_NUMBER() OVER (ORDER BY ma_nguoi_dung) AS teacher_rank
                    FROM NguoiDung
                    WHERE ma_don_vi = @CampusId
                      AND vai_tro_chinh = 'giao_vien'
                      AND trang_thai = 'hoat_dong'
                )
                INSERT INTO PhanCongGiamThi
                    (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
                SELECT
                    @SessionId,
                    tp.ma_nguoi_dung,
                    CASE
                        WHEN ((tp.teacher_rank - 1 - (@TeacherStart % @TeacherCount) + @TeacherCount) % @TeacherCount) = 0
                            THEN 'giam_thi_chinh'
                        ELSE 'giam_thi_phu'
                    END,
                    'du_kien',
                    @CurrentDate
                FROM TeacherPool tp
                WHERE ((tp.teacher_rank - 1 - (@TeacherStart % @TeacherCount) + @TeacherCount) % @TeacherCount) < @InvigilatorsPerSession;

                SET @TeacherStart = (@TeacherStart + @InvigilatorsPerSession) % @TeacherCount;
            END

            IF @SessionId IS NOT NULL
            BEGIN
                SELECT @ScheduleId = ma_lich_thi_tong
                FROM CaThi
                WHERE ma_ca_thi = @SessionId;

                SELECT @ExamDefinitionId = ma_de_kiem_tra
                FROM LichThiTong
                WHERE ma_lich_thi_tong = @ScheduleId;

                IF @ExamDefinitionId IS NULL
                BEGIN
                    SELECT TOP 1 @ExamDefinitionId = ma_de_kiem_tra
                    FROM DeKiemTra
                    WHERE ma_mon_hoc = @SubjectId
                      AND ma_hoc_ky = @TermId
                      AND JSON_VALUE(cau_hinh_de_thi, '$.MaKhoaHoc') = CAST(@CourseId AS NVARCHAR(20))
                    ORDER BY ma_de_kiem_tra;

                    IF @ExamDefinitionId IS NULL
                    BEGIN
                        INSERT INTO DeKiemTra
                            (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut,
                             cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi,
                             trang_thai_duyet, ngay_tao)
                        VALUES
                            (@SubjectId,
                             @TermId,
                             @ExamTitle,
                             120,
                             N'{"MaKhoaHoc":' + CAST(@CourseId AS NVARCHAR(20)) + N',"ChoPhepLamTruoc":false,"HienThiKetQua":false}',
                             'da_len_lich',
                             'trac_nghiem',
                             'online_tap_trung',
                             'da_duyet',
                             @CurrentDate);
                        SET @ExamDefinitionId = SCOPE_IDENTITY();
                    END
                END

                UPDATE LichThiTong
                SET ma_de_kiem_tra = @ExamDefinitionId,
                    trang_thai = CASE WHEN trang_thai = 'da_huy' THEN trang_thai ELSE 'nhap' END,
                    ngay_cap_nhat = @CurrentDate
                WHERE ma_lich_thi_tong = @ScheduleId;

                UPDATE DeKiemTra
                SET trang_thai = CASE WHEN trang_thai = 'nhap' THEN 'da_len_lich' ELSE trang_thai END,
                    trang_thai_duyet = CASE WHEN trang_thai_duyet IS NULL THEN 'da_duyet' ELSE trang_thai_duyet END,
                    ngay_cap_nhat = @CurrentDate
                WHERE ma_de_kiem_tra = @ExamDefinitionId;
            END

            FETCH NEXT FROM curCourse INTO @CourseId, @CourseTitle, @SubjectId, @ClassId;
        END

        CLOSE curCourse;
        DEALLOCATE curCourse;

        PRINT N'[OK] Co so ' + CAST(@CampusIndex AS NVARCHAR(10))
            + N': da tao ca thi cho cac khoa hoc, phan bo phong va giám thị.';

        FETCH NEXT FROM curCampus INTO @CampusId;
    END

    CLOSE curCampus;
    DEALLOCATE curCampus;

        -- Đồng bộ điểm danh cho toàn bộ thí sinh của các ca Phase 5 đã mở.
        -- Ca chưa dang_thi không bị tự động điểm danh để giữ đúng nghiệp vụ.
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
                d.ghi_chu = N'Tự động đồng bộ điểm danh Phase 5'
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
                pc.ma_giam_thi,
                N'Tự động đồng bộ điểm danh Phase 5',
                @CurrentDate
        FROM ThiSinhCaThi t
        INNER JOIN CaThi c ON c.ma_ca_thi = t.ma_ca_thi
        OUTER APPLY
        (
                SELECT TOP 1 p.ma_giam_thi
                FROM PhanCongGiamThi p
                WHERE p.ma_ca_thi = t.ma_ca_thi
                    AND p.trang_thai <> 'huy_phan_cong'
                ORDER BY CASE WHEN p.vai_tro_giam_thi = 'giam_thi_chinh' THEN 0 ELSE 1 END,
                                 p.ma_phan_cong
        ) pc
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
        c.ma_don_vi,
        COUNT(DISTINCT c.ma_ca_thi) AS so_ca_thi,
        COUNT(DISTINCT t.ma_hoc_sinh) AS so_luot_thi_sinh,
        COUNT(DISTINCT p.ma_giam_thi) AS so_giam_thi_da_phan_bo,
        SUM(CASE WHEN c.trang_thai = 'dang_thi' THEN 1 ELSE 0 END) AS so_ca_dang_mo
    FROM CaThi c
    LEFT JOIN ThiSinhCaThi t ON t.ma_ca_thi = c.ma_ca_thi
    LEFT JOIN PhanCongGiamThi p ON p.ma_ca_thi = c.ma_ca_thi
    WHERE c.ghi_chu LIKE N'Phase5_AllCourses;MaKhoaHoc=%'
    GROUP BY c.ma_don_vi
    ORDER BY c.ma_don_vi;

    PRINT N'--- HOÀN THÀNH PHASE 5 THÀNH CÔNG ---';
    PRINT N'--- Không có ca thi nào được chuyển sang dang_thi ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    IF CURSOR_STATUS('local', 'curCourse') >= -1
    BEGIN
        IF CURSOR_STATUS('local', 'curCourse') > -1 CLOSE curCourse;
        DEALLOCATE curCourse;
    END

    IF CURSOR_STATUS('local', 'curCampus') >= -1
    BEGIN
        IF CURSOR_STATUS('local', 'curCampus') > -1 CLOSE curCampus;
        DEALLOCATE curCampus;
    END

    PRINT N'!!! LỖI TRONG PHASE 5 !!!';
    THROW;
END CATCH;
GO
