USE LMS;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @SubjectId INT;
DECLARE @ExamId INT;

PRINT N'--- CẬP NHẬT 10 CÂU HỎI COM101 - SE18 ---';

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT @SubjectId = ma_mon_hoc
    FROM DanhMucMonHoc
    WHERE ma_code_mon_hoc = 'COM101';

    SELECT TOP 1 @ExamId = ma_de_kiem_tra
    FROM DeKiemTra
    WHERE ma_mon_hoc = @SubjectId
      AND tieu_de = N'COM101 - Lớp SE18 - Bài thi cuối kỳ TEST'
    ORDER BY ma_de_kiem_tra DESC;

    IF @SubjectId IS NULL
        THROW 55001, 'Khong tim thay mon COM101.', 1;
    IF @ExamId IS NULL
        THROW 55002, 'Khong tim thay de thi COM101 - SE18 TEST.', 1;

    DECLARE @Questions TABLE
    (
        question_no INT NOT NULL PRIMARY KEY,
        question_text NVARCHAR(500) NOT NULL,
        answer_a NVARCHAR(255) NOT NULL,
        answer_b NVARCHAR(255) NOT NULL,
        answer_c NVARCHAR(255) NOT NULL,
        answer_d NVARCHAR(255) NOT NULL,
        correct_answer CHAR(1) NOT NULL,
        explanation NVARCHAR(500) NOT NULL
    );

    INSERT INTO @Questions VALUES
        (1, N'Biến dùng để làm gì trong chương trình?', N'Lưu trữ dữ liệu', N'Kết nối Internet', N'Tắt máy tính', N'Tạo cơ sở dữ liệu', 'A', N'Biến là vùng nhớ dùng để lưu trữ dữ liệu.'),
        (2, N'Kiểu dữ liệu nào thường dùng để lưu số nguyên?', N'int', N'float', N'double', N'string', 'A', N'int dùng để lưu số nguyên.'),
        (3, N'Toán tử nào dùng để so sánh bằng?', N'==', N'=', N'!=', N'&&', 'A', N'== là toán tử so sánh bằng.'),
        (4, N'Cấu trúc nào dùng để rẽ nhánh điều kiện?', N'if', N'for', N'while', N'class', 'A', N'if dùng để kiểm tra điều kiện và rẽ nhánh.'),
        (5, N'Vòng lặp nào kiểm tra điều kiện trước khi chạy?', N'while', N'do-while', N'switch', N'try', 'A', N'while kiểm tra điều kiện trước mỗi vòng lặp.'),
        (6, N'Hàm giúp chương trình đạt lợi ích nào?', N'Tái sử dụng mã nguồn', N'Tăng lỗi chương trình', N'Xóa dữ liệu', N'Tắt trình biên dịch', 'A', N'Hàm giúp chia nhỏ và tái sử dụng mã nguồn.'),
        (7, N'Mảng là tập hợp các phần tử thường có đặc điểm gì?', N'Cùng kiểu dữ liệu', N'Không có dữ liệu', N'Chỉ chứa ký tự', N'Luôn có một phần tử', 'A', N'Mảng thường lưu nhiều phần tử cùng kiểu dữ liệu.'),
        (8, N'Thuật toán là gì?', N'Các bước giải quyết bài toán', N'Một thiết bị phần cứng', N'Một tài khoản người dùng', N'Một loại màn hình', 'A', N'Thuật toán là dãy bước hữu hạn để giải quyết bài toán.'),
        (9, N'Giá trị của 7 chia nguyên cho 2 là bao nhiêu?', N'3', N'3.5', N'4', N'2', 'A', N'Chia hai số nguyên cho phần nguyên là 3.'),
        (10, N'Hệ nhị phân sử dụng những chữ số nào?', N'0 và 1', N'1 và 2', N'0 đến 9', N'A và B', 'A', N'Hệ nhị phân chỉ sử dụng 0 và 1.');

    UPDATE q
    SET q.loai_cau_hoi = 'trac_nghiem',
        q.kieu_lua_chon = 'chon_mot',
        q.lua_chon = N'[{"id":"A","text":"' + REPLACE(x.answer_a, '"', '""')
            + N'"},{"id":"B","text":"' + REPLACE(x.answer_b, '"', '""')
            + N'"},{"id":"C","text":"' + REPLACE(x.answer_c, '"', '""')
            + N'"},{"id":"D","text":"' + REPLACE(x.answer_d, '"', '""') + N'"}]',
        q.dap_an_dung = N'["' + x.correct_answer + N'"]',
        q.giai_thich_dap_an = x.explanation,
        q.con_hoat_dong = 1,
        q.ngay_cap_nhat = @CurrentDate
    FROM CauHoi q
    INNER JOIN @Questions x ON x.question_text = q.noi_dung
    WHERE q.ma_mon_hoc = @SubjectId;

    INSERT INTO CauHoi
        (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, kieu_lua_chon,
         lua_chon, dap_an_dung, giai_thich_dap_an, do_kho, con_hoat_dong, ngay_tao)
    SELECT
        @SubjectId,
        NULL,
        'trac_nghiem',
        x.question_text,
        'chon_mot',
        N'[{"id":"A","text":"' + REPLACE(x.answer_a, '"', '""')
            + N'"},{"id":"B","text":"' + REPLACE(x.answer_b, '"', '""')
            + N'"},{"id":"C","text":"' + REPLACE(x.answer_c, '"', '""')
            + N'"},{"id":"D","text":"' + REPLACE(x.answer_d, '"', '""') + N'"}]',
        N'["' + x.correct_answer + N'"]',
        x.explanation,
        'de',
        1,
        @CurrentDate
    FROM @Questions x
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM CauHoi q
        WHERE q.ma_mon_hoc = @SubjectId
          AND q.noi_dung = x.question_text
    );

    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu)
    SELECT @ExamId, q.ma_cau_hoi, 1, x.question_no
    FROM @Questions x
    INNER JOIN CauHoi q ON q.ma_mon_hoc = @SubjectId AND q.noi_dung = x.question_text
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM CauHoiDeKiemTra qd
        WHERE qd.ma_de_kiem_tra = @ExamId
          AND qd.ma_cau_hoi = q.ma_cau_hoi
    );

    UPDATE qd
    SET qd.diem_so = 1,
        qd.thu_tu = x.question_no
    FROM CauHoiDeKiemTra qd
    INNER JOIN CauHoi q ON q.ma_cau_hoi = qd.ma_cau_hoi
    INNER JOIN @Questions x ON x.question_text = q.noi_dung
    WHERE qd.ma_de_kiem_tra = @ExamId;

    UPDATE d
    SET d.trang_thai = CASE WHEN d.trang_thai = 'nhap' THEN 'da_len_lich' ELSE d.trang_thai END,
        d.trang_thai_duyet = CASE WHEN d.trang_thai_duyet IS NULL THEN 'da_duyet' ELSE d.trang_thai_duyet END,
        d.ngay_cap_nhat = @CurrentDate
    FROM DeKiemTra d
    WHERE d.ma_de_kiem_tra = @ExamId;

    COMMIT TRANSACTION;

    SELECT
        @ExamId AS ma_de_kiem_tra,
        COUNT(qd.ma_cau_hoi) AS so_cau_hoi,
        SUM(qd.diem_so) AS tong_diem,
        N'Đáp án đúng lưu dạng JSON array, lựa chọn dùng id/text' AS ghi_chu
    FROM CauHoiDeKiemTra qd
    WHERE qd.ma_de_kiem_tra = @ExamId;

    SELECT
        qd.thu_tu,
        q.noi_dung,
        q.lua_chon,
        q.dap_an_dung,
        q.giai_thich_dap_an
    FROM CauHoiDeKiemTra qd
    INNER JOIN CauHoi q ON q.ma_cau_hoi = qd.ma_cau_hoi
    WHERE qd.ma_de_kiem_tra = @ExamId
    ORDER BY qd.thu_tu;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI CẬP NHẬT CÂU HỎI COM101 - SE18 !!!';
    THROW;
END CATCH;
GO
