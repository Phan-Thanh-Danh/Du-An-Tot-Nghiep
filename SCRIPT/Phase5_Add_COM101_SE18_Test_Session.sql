USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @CampusId INT;
DECLARE @TermId INT;
DECLARE @SubjectId INT;
DECLARE @ClassId INT;
DECLARE @TeacherId INT;
DECLARE @RoomId INT;
DECLARE @ExamId INT;
DECLARE @ScheduleId INT;
DECLARE @SessionId INT;
DECLARE @ExamDate DATETIME2 = DATEADD(DAY, 1, @CurrentDate);
DECLARE @StartTime DATETIME2 = DATEADD(HOUR, 8, CAST(CAST(@ExamDate AS DATE) AS DATETIME2));
DECLARE @EndTime DATETIME2 = DATEADD(HOUR, 2, @StartTime);
DECLARE @ExamTitle NVARCHAR(255) = N'COM101 - Lớp SE18 - Bài thi cuối kỳ TEST';
DECLARE @SessionMarker NVARCHAR(500) = N'Phase5_COM101_SE18_TEST';

PRINT N'--- TẠO CA THI TEST COM101 - LỚP SE18 ---';
PRINT N'--- Ca thi ban đầu: nhap | Thí sinh: cho_thi | Giám thị: du_kien ---';

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT @SubjectId = ma_mon_hoc
    FROM DanhMucMonHoc
    WHERE ma_code_mon_hoc = 'COM101';

    SELECT TOP 1
        @ClassId = l.ma_lop,
        @CampusId = l.ma_don_vi
    FROM LopHanhChinh l
    -- Phase 2 tạo mã theo mẫu SE + CampusIndex + số lớp: SE118 là lớp SE18 ở cơ sở 1.
    WHERE l.ma_code_lop = 'SE118'
      AND l.ten_lop = N'Lớp SE18 - Trường AET Cơ sở TP.HCM'
      AND l.con_hoat_dong = 1;

    SELECT TOP 1 @TermId = ma_hoc_ky
    FROM HocKy
    WHERE ma_don_vi = @CampusId
      AND nam_hoc = '2026'
      AND thu_tu_trong_nam = 1
    ORDER BY ma_hoc_ky;

    SELECT @TeacherId = ma_nguoi_dung
    FROM NguoiDung
    WHERE email = 'gv1.cs1@aet.local'
      AND vai_tro_chinh = 'giao_vien'
      AND trang_thai = 'hoat_dong';

    IF @SubjectId IS NULL
        THROW 54001, 'Khong tim thay mon COM101.', 1;
    IF @ClassId IS NULL OR @CampusId IS NULL
        THROW 54002, 'Khong tim thay lop SE118 (ten SE18) tai co so TP.HCM.', 1;
    IF @TermId IS NULL
        THROW 54003, 'Khong tim thay hoc ky Thu 2026 cua lop SE18.', 1;
    IF @TeacherId IS NULL
        THROW 54004, 'Khong tim thay giang vien gv1.cs1@aet.local.', 1;

    IF EXISTS (SELECT 1 FROM CaThi WHERE ghi_chu = @SessionMarker)
        THROW 54005, 'Ca thi test COM101-SE18 da ton tai.', 1;

    IF (SELECT COUNT(*) FROM NguoiDung
        WHERE ma_lop = @ClassId
          AND ma_don_vi = @CampusId
          AND vai_tro_chinh = 'hoc_sinh'
          AND trang_thai = 'hoat_dong') < 40
        THROW 54006, 'Lop SE18 khong du 40 sinh vien hoat dong.', 1;

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
        THROW 54007, 'Khong tim thay phong thi trong khung gio test.', 1;

    SELECT TOP 1 @ExamId = ma_de_kiem_tra
    FROM DeKiemTra
    WHERE ma_mon_hoc = @SubjectId
      AND ma_hoc_ky = @TermId
      AND tieu_de = @ExamTitle
    ORDER BY ma_de_kiem_tra;

    IF @ExamId IS NULL
    BEGIN
        INSERT INTO DeKiemTra
            (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi,
             trang_thai, loai_de_thi, hinh_thuc_thi, trang_thai_duyet, ngay_tao)
        VALUES
            (@SubjectId, @TermId, @ExamTitle, 120,
             N'{"MaKhoaHoc":"COM101-SE18-TEST","ChoPhepLamTruoc":false,"HienThiKetQua":false}',
             'da_len_lich', 'trac_nghiem', 'online_tap_trung', 'da_duyet', @CurrentDate);
        SET @ExamId = SCOPE_IDENTITY();
    END

    DECLARE @Questions TABLE
    (
        question_no INT NOT NULL,
        question_text NVARCHAR(500) NOT NULL,
        answer_a NVARCHAR(255) NOT NULL,
        answer_b NVARCHAR(255) NOT NULL,
        answer_c NVARCHAR(255) NOT NULL,
        answer_d NVARCHAR(255) NOT NULL,
        explanation NVARCHAR(500) NOT NULL
    );

    INSERT INTO @Questions VALUES
        (1, N'Biến dùng để làm gì trong chương trình?', N'Lưu trữ dữ liệu', N'Kết nối Internet', N'Tắt máy tính', N'Tạo cơ sở dữ liệu', N'Biến là vùng nhớ dùng để lưu trữ dữ liệu.'),
        (2, N'Kiểu dữ liệu nào thường dùng để lưu số nguyên?', N'int', N'float', N'double', N'string', N'int dùng để lưu số nguyên.'),
        (3, N'Toán tử nào dùng để so sánh bằng?', N'==', N'=', N'!=', N'&&', N'== là toán tử so sánh bằng.'),
        (4, N'Cấu trúc nào dùng để rẽ nhánh điều kiện?', N'if', N'for', N'while', N'class', N'if dùng để kiểm tra điều kiện và rẽ nhánh.'),
        (5, N'Vòng lặp nào kiểm tra điều kiện trước khi chạy?', N'while', N'do-while', N'switch', N'try', N'while kiểm tra điều kiện trước mỗi vòng lặp.'),
        (6, N'Hàm giúp chương trình đạt lợi ích nào?', N'Tái sử dụng mã nguồn', N'Tăng lỗi chương trình', N'Xóa dữ liệu', N'Tắt trình biên dịch', N'Hàm giúp chia nhỏ và tái sử dụng mã nguồn.'),
        (7, N'Mảng là tập hợp các phần tử thường có đặc điểm gì?', N'Cùng kiểu dữ liệu', N'Không có dữ liệu', N'Chỉ chứa ký tự', N'Luôn có một phần tử', N'Mảng thường lưu nhiều phần tử cùng kiểu dữ liệu.'),
        (8, N'Thuật toán là gì?', N'Các bước giải quyết bài toán', N'Một thiết bị phần cứng', N'Một tài khoản người dùng', N'Một loại màn hình', N'Thuật toán là dãy bước hữu hạn để giải quyết bài toán.'),
        (9, N'Giá trị của 7 chia nguyên cho 2 là bao nhiêu?', N'3', N'3.5', N'4', N'2', N'Chia hai số nguyên cho phần nguyên là 3.'),
        (10, N'Hệ nhị phân sử dụng những chữ số nào?', N'0 và 1', N'1 và 2', N'0 đến 9', N'A và B', N'Hệ nhị phân chỉ sử dụng 0 và 1.');

    DECLARE @QuestionNo INT;
    DECLARE @QuestionText NVARCHAR(500);
    DECLARE @AnswerA NVARCHAR(255);
    DECLARE @AnswerB NVARCHAR(255);
    DECLARE @AnswerC NVARCHAR(255);
    DECLARE @AnswerD NVARCHAR(255);
    DECLARE @Explanation NVARCHAR(500);
    DECLARE @QuestionId INT;

    DECLARE question_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT question_no, question_text, answer_a, answer_b, answer_c, answer_d, explanation
        FROM @Questions
        ORDER BY question_no;

    OPEN question_cursor;
    FETCH NEXT FROM question_cursor INTO @QuestionNo, @QuestionText, @AnswerA, @AnswerB, @AnswerC, @AnswerD, @Explanation;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT TOP 1 @QuestionId = ma_cau_hoi
        FROM CauHoi
        WHERE ma_mon_hoc = @SubjectId
          AND noi_dung = @QuestionText;

        IF @QuestionId IS NULL
        BEGIN
            INSERT INTO CauHoi
                (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, kieu_lua_chon,
                 lua_chon, dap_an_dung, giai_thich_dap_an, do_kho,
                 con_hoat_dong, ngay_tao)
            VALUES
                (@SubjectId, @TeacherId, 'trac_nghiem', @QuestionText, 'chon_mot',
                 N'[{"key":"A","text":"' + @AnswerA + N'"},{"key":"B","text":"' + @AnswerB + N'"},{"key":"C","text":"' + @AnswerC + N'"},{"key":"D","text":"' + @AnswerD + N'"}]',
                 N'["A"]', @Explanation, 'de', 1, @CurrentDate);
            SET @QuestionId = SCOPE_IDENTITY();
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM CauHoiDeKiemTra qd
            WHERE qd.ma_de_kiem_tra = @ExamId
              AND qd.ma_cau_hoi = @QuestionId
        )
        BEGIN
            INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu)
            VALUES (@ExamId, @QuestionId, 1, @QuestionNo);
        END

        SET @QuestionId = NULL;
        FETCH NEXT FROM question_cursor INTO @QuestionNo, @QuestionText, @AnswerA, @AnswerB, @AnswerC, @AnswerD, @Explanation;
    END

    CLOSE question_cursor;
    DEALLOCATE question_cursor;

    SELECT TOP 1 @ScheduleId = ma_lich_thi_tong
    FROM LichThiTong
    WHERE ma_ky_thi = (SELECT TOP 1 ma_ky_thi FROM KyThi WHERE ma_hoc_ky = @TermId AND loai_ky_thi = 'cuoi_ky')
      AND ma_mon_hoc = @SubjectId
      AND ma_de_kiem_tra = @ExamId
      AND trang_thai <> 'da_huy';

    DECLARE @ExamPeriodId INT;
    SELECT TOP 1 @ExamPeriodId = ma_ky_thi
    FROM KyThi
    WHERE ma_hoc_ky = @TermId
      AND loai_ky_thi = 'cuoi_ky';

    IF @ExamPeriodId IS NULL
    BEGIN
        INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, loai_ky_thi, trang_thai, ngay_tao)
        VALUES (N'Kỳ thi cuối kỳ COM101 - SE18 TEST', @TermId, 'cuoi_ky', 'nhap', @CurrentDate);
        SET @ExamPeriodId = SCOPE_IDENTITY();
    END

    IF @ScheduleId IS NULL
    BEGIN
        INSERT INTO LichThiTong
            (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi,
             ngay_thi_du_kien, trang_thai, ngay_tao)
        VALUES
            (@ExamPeriodId, @SubjectId, @ExamId, 'online_tap_trung',
             @ExamDate, 'nhap', @CurrentDate);
        SET @ScheduleId = SCOPE_IDENTITY();
    END

    INSERT INTO CaThi
        (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi,
         thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi,
         trang_thai, ghi_chu, ngay_tao)
    VALUES
        (@ScheduleId, @ExamTitle, @RoomId, @ExamDate, @StartTime, @EndTime,
         @CampusId, 'nhap', @SessionMarker, @CurrentDate);
    SET @SessionId = SCOPE_IDENTITY();

    INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
    SELECT TOP 40 @SessionId, u.ma_nguoi_dung, 'cho_thi', @CurrentDate
    FROM NguoiDung u
    WHERE u.ma_lop = @ClassId
      AND u.ma_don_vi = @CampusId
      AND u.vai_tro_chinh = 'hoc_sinh'
      AND u.trang_thai = 'hoat_dong'
    ORDER BY u.ma_nguoi_dung;

    INSERT INTO PhanCongGiamThi
        (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
    VALUES (@SessionId, @TeacherId, 'giam_thi_chinh', 'du_kien', @CurrentDate);

    COMMIT TRANSACTION;

    SELECT
        @SessionId AS ma_ca_thi,
        @ScheduleId AS ma_lich_thi_tong,
        @ExamId AS ma_de_kiem_tra,
        @ClassId AS ma_lop,
        @CampusId AS ma_don_vi,
        @RoomId AS ma_phong,
        @TeacherId AS ma_giam_thi,
        'nhap' AS trang_thai_ca_thi,
        'cho_thi' AS trang_thai_thi_sinh,
        10 AS so_cau_hoi,
        40 AS so_thi_sinh;

    PRINT N'--- ĐÃ TẠO CA TEST COM101 - SE18 ---';
    PRINT N'--- Giảng viên điểm danh rồi bấm Bắt đầu canh thi để mở ca ---';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    IF CURSOR_STATUS('local', 'question_cursor') > -1
        CLOSE question_cursor;
    IF CURSOR_STATUS('local', 'question_cursor') >= -1
        DEALLOCATE question_cursor;

    PRINT N'!!! LỖI TẠO CA TEST COM101 - SE18 !!!';
    THROW;
END CATCH;
GO
