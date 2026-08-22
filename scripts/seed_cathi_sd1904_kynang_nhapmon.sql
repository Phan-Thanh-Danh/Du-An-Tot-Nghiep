USE LMS;
GO
SET NOCOUNT ON;

-- ============================================================
-- Script: Thêm 2 ca thi + 2 đề thi (5 câu thực tế mỗi môn)
-- Môn 1: Kỹ năng học tập (GEN101, ma_mon_hoc = 50)
-- Môn 2: Nhập môn lập trình (COM101, ma_mon_hoc = 2)
-- Thí sinh: toàn bộ học sinh lớp SD1904 (ma_lop = 4)
-- Giám thị: teacher.cntt@lms.local (ma_nguoi_dung = 13)
-- Phòng thi: 1 (Phòng A101)
-- Idempotent: chạy lại nhiều lần không tạo trùng.
-- ============================================================

DECLARE @MonKyNang INT = 50;              -- GEN101 - Kỹ năng học tập
DECLARE @MonNhapMon INT = 2;              -- COM101 - Nhập môn lập trình
DECLARE @GiamThiId INT = 13;              -- teacher.cntt@lms.local
DECLARE @PhongThi INT = 1;                -- Phòng A101
DECLARE @LopSd1904 INT = 4;               -- SD1904
DECLARE @MaDonVi INT = 3;
DECLARE @MaHocKy INT = 1;                 -- HK1_2026

-- 1. Tạo kỳ thi (nếu chưa có)
DECLARE @KyThiId INT;
SELECT TOP 1 @KyThiId = ma_ky_thi FROM KyThi WHERE ten_ky_thi = N'Kỳ thi SD1904 - Kỹ năng học tập & Nhập môn lập trình';
IF @KyThiId IS NULL
BEGIN
    INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, trang_thai, loai_ky_thi, ngay_tao)
    VALUES (N'Kỳ thi SD1904 - Kỹ năng học tập & Nhập môn lập trình', @MaHocKy, 'nhap', 'giua_ky', GETDATE());
    SET @KyThiId = SCOPE_IDENTITY();
    PRINT 'Created KyThi: ' + CAST(@KyThiId AS VARCHAR(10));
END
ELSE
    PRINT 'KyThi already exists: ' + CAST(@KyThiId AS VARCHAR(10));

-- 2. Tạo đề thi Kỹ năng học tập (GEN101) - 5 câu hỏi thực tế
DECLARE @De1 INT;
SELECT TOP 1 @De1 = ma_de_kiem_tra FROM DeKiemTra WHERE ma_mon_hoc = @MonKyNang AND tieu_de = N'Đề thi Kỹ năng học tập - SD1904 (thực tế)';
IF @De1 IS NULL
BEGIN
    INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi, ty_le_trac_nghiem, ty_le_tu_luan, trang_thai_duyet, ngay_tao)
    VALUES (@MonKyNang, @MaHocKy, N'Đề thi Kỹ năng học tập - SD1904 (thực tế)', 60, N'{"questions":[]}', 'dang_mo', 'trac_nghiem', 'online_tap_trung', 100, 0, 'da_duyet', GETDATE());
    SET @De1 = SCOPE_IDENTITY();

    INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, kieu_lua_chon, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, giai_thich_dap_an, ngay_tao) VALUES
    (@MonKyNang, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Phương pháp ghi chú Cornell chia trang giấy thành mấy phần chính?',
     N'[{"id":"A","text":"2 phần"},{"id":"B","text":"3 phần"},{"id":"C","text":"4 phần"},{"id":"D","text":"5 phần"}]',
     N'["B"]', 'de', 1, N'Phương pháp Cornell gồm 3 phần: cột ghi chú chính, cột từ khóa và phần tóm tắt cuối trang.', GETDATE()),
    (@MonKyNang, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Kỹ thuật Pomodoro chuẩn khuyên bạn học tập trung liên tục trong bao lâu rồi nghỉ ngắn?',
     N'[{"id":"A","text":"15 phút"},{"id":"B","text":"25 phút"},{"id":"C","text":"50 phút"},{"id":"D","text":"60 phút"}]',
     N'["B"]', 'de', 1, N'Pomodoro chuẩn: 25 phút tập trung + 5 phút nghỉ, lặp lại 4 lần rồi nghỉ dài hơn.', GETDATE()),
    (@MonKyNang, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Để ôn thi hiệu quả, nên ưu tiên học bằng cách nào dưới đây?',
     N'[{"id":"A","text":"Đọc lại tài liệu nhiều lần"},{"id":"B","text":"Làm bài tập, câu hỏi và tự kiểm tra"},{"id":"C","text":"Chép lại slide nguyên văn"},{"id":"D","text":"Học nhồi nhét đêm trước thi"}]',
     N'["B"]', 'de', 1, N'Tự kiểm tra (active recall) giúp ghi nhớ lâu hơn đọc thụ động nhiều lần.', GETDATE()),
    (@MonKyNang, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Khi đọc một chương sách dài trước buổi học, kỹ thuật phù hợp để nắm ý chính nhanh là gì?',
     N'[{"id":"A","text":"Đọc từng từ một từ đầu đến cuối"},{"id":"B","text":"Scanning - quét tìm từ khóa cụ thể"},{"id":"C","text":"Đọc to toàn bộ chương"},{"id":"D","text":"Dịch sang ngôn ngữ khác"}]',
     N'["B"]', 'de', 1, N'Scanning là kỹ thuật quét nhanh để tìm thông tin/ý chính phù hợp khi chuẩn bị bài.', GETDATE()),
    (@MonKyNang, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Để đạt mục tiêu học tập, kế hoạch nên được thiết kế theo nguyên tắc nào dưới đây?',
     N'[{"id":"A","text":"Mục tiêu mơ hồ, không thời hạn"},{"id":"B","text":"Mục tiêu SMART - cụ thể, đo được, khả thi"},{"id":"C","text":"Học tất cả môn cùng lúc"},{"id":"D","text":"Chỉ học khi hứng thú"}]',
     N'["B"]', 'de', 1, N'SMART (Specific, Measurable, Achievable, Relevant, Time-bound) giúp theo dõi và đạt mục tiêu.', GETDATE());
    PRINT 'Created DeKiemTra GEN101: ' + CAST(@De1 AS VARCHAR(10));
END
ELSE
    PRINT 'DeKiemTra GEN101 already exists: ' + CAST(@De1 AS VARCHAR(10));

-- Liên kết câu hỏi mới với đề (chỉ khi chưa có liên kết)
IF NOT EXISTS (SELECT 1 FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = @De1)
BEGIN
    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu)
    SELECT @De1, ma_cau_hoi, 2.0, ROW_NUMBER() OVER (ORDER BY ma_cau_hoi)
    FROM CauHoi
    WHERE ma_mon_hoc = @MonKyNang
      AND (noi_dung LIKE N'%ghi chú Cornell%' OR noi_dung LIKE N'%Pomodoro%'
        OR noi_dung LIKE N'%ôn thi hiệu quả%' OR noi_dung LIKE N'%chương sách dài%'
        OR noi_dung LIKE N'%SMART%');
    PRINT 'Linked questions for GEN101: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
END

-- 3. Tạo đề thi Nhập môn lập trình (COM101) - 5 câu hỏi thực tế
DECLARE @De2 INT;
SELECT TOP 1 @De2 = ma_de_kiem_tra FROM DeKiemTra WHERE ma_mon_hoc = @MonNhapMon AND tieu_de = N'Đề thi Nhập môn lập trình - SD1904 (thực tế)';
IF @De2 IS NULL
BEGIN
    INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi, ty_le_trac_nghiem, ty_le_tu_luan, trang_thai_duyet, ngay_tao)
    VALUES (@MonNhapMon, @MaHocKy, N'Đề thi Nhập môn lập trình - SD1904 (thực tế)', 60, N'{"questions":[]}', 'dang_mo', 'trac_nghiem', 'online_tap_trung', 100, 0, 'da_duyet', GETDATE());
    SET @De2 = SCOPE_IDENTITY();

    INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, kieu_lua_chon, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, giai_thich_dap_an, ngay_tao) VALUES
    (@MonNhapMon, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Trong ngôn ngữ lập trình, biến (variable) dùng để làm gì?',
     N'[{"id":"A","text":"In kết quả ra màn hình"},{"id":"B","text":"Lưu trữ dữ liệu để dùng lại trong chương trình"},{"id":"C","text":"Xóa dữ liệu khỏi máy tính"},{"id":"D","text":"Kết nối Internet"}]',
     N'["B"]', 'de', 1, N'Biến là vùng nhớ đặt tên để lưu trữ dữ liệu, có thể đọc và thay đổi trong chương trình.', GETDATE()),
    (@MonNhapMon, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Vòng lặp for trong lập trình thường được dùng khi nào?',
     N'[{"id":"A","text":"Khi biết trước số lần lặp"},{"id":"B","text":"Khi chương trình bị lỗi"},{"id":"C","text":"Khi chỉ chạy đúng 1 lần"},{"id":"D","text":"Khi cần so sánh 2 số"}]',
     N'["A"]', 'de', 1, N'Vòng lặp for phù hợp khi số lần lặp được xác định trước nhờ biến đếm.', GETDATE()),
    (@MonNhapMon, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Câu lệnh điều kiện if...else dùng để làm gì?',
     N'[{"id":"A","text":"Lặp lại một đoạn code nhiều lần"},{"id":"B","text":"Chọn nhánh thực thi dựa trên điều kiện"},{"id":"C","text":"Khai báo biến mới"},{"id":"D","text":"Chuyển đổi kiểu dữ liệu"}]',
     N'["B"]', 'de', 1, N'if...else kiểm tra điều kiện và thực thi nhánh tương ứng (đúng/sai).', GETDATE()),
    (@MonNhapMon, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Hàm (function) trong lập trình có lợi ích chính nào?',
     N'[{"id":"A","text":"Làm chương trình chạy nhanh hơn chắc chắn"},{"id":"B","text":"Tái sử dụng code, chia nhỏ bài toán dễ quản lý"},{"id":"C","text":"Xóa biến không cần thiết"},{"id":"D","text":"Tự động tạo dữ liệu"}]',
     N'["B"]', 'de', 1, N'Hàm đóng gói một khối lệnh để gọi lại nhiều lần, giúp code rõ ràng và dễ bảo trì.', GETDATE()),
    (@MonNhapMon, @GiamThiId, 'trac_nghiem', 'chon_mot', N'Khi chương trình báo lỗi "division by zero" nghĩa là gì?',
     N'[{"id":"A","text":"Chia một số cho 0 - phép toán không hợp lệ"},{"id":"B","text":"Chương trình chạy quá nhanh"},{"id":"C","text":"Thiếu bộ nhớ RAM"},{"id":"D","text":"Sai tên biến in hoa thường"}]',
     N'["A"]', 'de', 1, N'Chia cho 0 là lỗi toán học trong hầu hết ngôn ngữ lập trình, cần kiểm tra mẫu số trước khi chia.', GETDATE());
    PRINT 'Created DeKiemTra COM101: ' + CAST(@De2 AS VARCHAR(10));
END
ELSE
    PRINT 'DeKiemTra COM101 already exists: ' + CAST(@De2 AS VARCHAR(10));

IF NOT EXISTS (SELECT 1 FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = @De2)
BEGIN
    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu)
    SELECT @De2, ma_cau_hoi, 2.0, ROW_NUMBER() OVER (ORDER BY ma_cau_hoi)
    FROM CauHoi
    WHERE ma_mon_hoc = @MonNhapMon
      AND (noi_dung LIKE N'%biến (variable)%' OR noi_dung LIKE N'%Vòng lặp for%'
        OR noi_dung LIKE N'%if...else%' OR noi_dung LIKE N'%Hàm (function)%'
        OR noi_dung LIKE N'%division by zero%');
    PRINT 'Linked questions for COM101: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
END

-- 4. Tạo LichThiTong cho 2 môn
DECLARE @Lich1 INT, @Lich2 INT;
IF NOT EXISTS (SELECT 1 FROM LichThiTong WHERE ma_ky_thi = @KyThiId AND ma_mon_hoc = @MonKyNang)
BEGIN
    INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
    VALUES (@KyThiId, @MonKyNang, @De1, 'online_tap_trung', DATEADD(DAY, 1, GETDATE()), 'da_gui_ve_co_so', GETDATE());
    SET @Lich1 = SCOPE_IDENTITY();
    PRINT 'Created LichThiTong GEN101: ' + CAST(@Lich1 AS VARCHAR(10));
END
ELSE
    SELECT @Lich1 = ma_lich_thi_tong FROM LichThiTong WHERE ma_ky_thi = @KyThiId AND ma_mon_hoc = @MonKyNang;

IF NOT EXISTS (SELECT 1 FROM LichThiTong WHERE ma_ky_thi = @KyThiId AND ma_mon_hoc = @MonNhapMon)
BEGIN
    INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
    VALUES (@KyThiId, @MonNhapMon, @De2, 'online_tap_trung', DATEADD(DAY, 2, GETDATE()), 'da_gui_ve_co_so', GETDATE());
    SET @Lich2 = SCOPE_IDENTITY();
    PRINT 'Created LichThiTong COM101: ' + CAST(@Lich2 AS VARCHAR(10));
END
ELSE
    SELECT @Lich2 = ma_lich_thi_tong FROM LichThiTong WHERE ma_ky_thi = @KyThiId AND ma_mon_hoc = @MonNhapMon;

-- 5. Tạo 2 ca thi + phân công giám thị + thí sinh SD1904
DECLARE @CaThi1 INT, @CaThi2 INT;

-- Ca thi 1: Kỹ năng học tập
SELECT TOP 1 @CaThi1 = ma_ca_thi FROM CaThi WHERE ma_lich_thi_tong = @Lich1;
IF @CaThi1 IS NULL
BEGIN
    INSERT INTO CaThi (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ngay_tao)
    VALUES (@Lich1, N'Ca thi Kỹ năng học tập - SD1904', @PhongThi, CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), DATEADD(DAY, 1, GETDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 1, GETDATE())), @MaDonVi, 'da_san_sang', GETDATE());
    SET @CaThi1 = SCOPE_IDENTITY();
    PRINT 'Created CaThi GEN101: ' + CAST(@CaThi1 AS VARCHAR(10));
END

-- Ca thi 2: Nhập môn lập trình
SELECT TOP 1 @CaThi2 = ma_ca_thi FROM CaThi WHERE ma_lich_thi_tong = @Lich2;
IF @CaThi2 IS NULL
BEGIN
    INSERT INTO CaThi (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ngay_tao)
    VALUES (@Lich2, N'Ca thi Nhập môn lập trình - SD1904', @PhongThi, CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), DATEADD(DAY, 2, GETDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 2, GETDATE())), @MaDonVi, 'da_san_sang', GETDATE());
    SET @CaThi2 = SCOPE_IDENTITY();
    PRINT 'Created CaThi COM101: ' + CAST(@CaThi2 AS VARCHAR(10));
END

-- Phân công giám thị teacher.cntt@lms.local cho cả 2 ca (idempotent)
IF NOT EXISTS (SELECT 1 FROM PhanCongGiamThi WHERE ma_ca_thi = @CaThi1 AND ma_giam_thi = @GiamThiId)
    INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
    VALUES (@CaThi1, @GiamThiId, 'giam_thi_chinh', 'da_xac_nhan', GETDATE());

IF NOT EXISTS (SELECT 1 FROM PhanCongGiamThi WHERE ma_ca_thi = @CaThi2 AND ma_giam_thi = @GiamThiId)
    INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
    VALUES (@CaThi2, @GiamThiId, 'giam_thi_chinh', 'da_xac_nhan', GETDATE());

-- Thí sinh: toàn bộ học sinh lớp SD1904 (idempotent theo từng ca)
INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
SELECT @CaThi1, u.ma_nguoi_dung, 'cho_thi', GETDATE()
FROM NguoiDung u
WHERE u.ma_lop = @LopSd1904 AND u.vai_tro_chinh = 'hoc_sinh'
  AND NOT EXISTS (SELECT 1 FROM ThiSinhCaThi t WHERE t.ma_ca_thi = @CaThi1 AND t.ma_hoc_sinh = u.ma_nguoi_dung);
PRINT 'ThiSinh added to CaThi1: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
SELECT @CaThi2, u.ma_nguoi_dung, 'cho_thi', GETDATE()
FROM NguoiDung u
WHERE u.ma_lop = @LopSd1904 AND u.vai_tro_chinh = 'hoc_sinh'
  AND NOT EXISTS (SELECT 1 FROM ThiSinhCaThi t WHERE t.ma_ca_thi = @CaThi2 AND t.ma_hoc_sinh = u.ma_nguoi_dung);
PRINT 'ThiSinh added to CaThi2: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- ============================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================
SELECT 'CaThi1' AS label, ma_ca_thi, ten_ca_thi, trang_thai FROM CaThi WHERE ma_ca_thi = @CaThi1
UNION ALL SELECT 'CaThi2', ma_ca_thi, ten_ca_thi, trang_thai FROM CaThi WHERE ma_ca_thi = @CaThi2;

SELECT 'GEN101' AS mon, COUNT(*) AS so_cau_hoi_trong_de
FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = @De1
UNION ALL SELECT 'COM101', COUNT(*) FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = @De2;

SELECT t.ma_ca_thi, t.ten_ca_thi, COUNT(ts.ma_thi_sinh_ca_thi) AS so_thi_sinh
FROM CaThi t
LEFT JOIN ThiSinhCaThi ts ON ts.ma_ca_thi = t.ma_ca_thi
WHERE t.ma_ca_thi IN (@CaThi1, @CaThi2)
GROUP BY t.ma_ca_thi, t.ten_ca_thi;

SELECT p.ma_ca_thi, p.ma_giam_thi, u.email AS giam_thi, p.vai_tro_giam_thi
FROM PhanCongGiamThi p
JOIN NguoiDung u ON u.ma_nguoi_dung = p.ma_giam_thi
WHERE p.ma_ca_thi IN (@CaThi1, @CaThi2);
GO
