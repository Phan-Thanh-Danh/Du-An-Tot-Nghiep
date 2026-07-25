USE LMS;
GO
SET QUOTED_IDENTIFIER ON;

DECLARE @KyThiId INT;
SELECT TOP 1 @KyThiId = ma_ky_thi FROM KyThi;
IF @KyThiId IS NULL
BEGIN
    INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, ngay_tao) VALUES (N'Kỳ thi chính thức', 1, GETDATE());
    SET @KyThiId = SCOPE_IDENTITY();
END

DECLARE @MaPhong INT = 1;
DECLARE @MaGiamThi INT = 15;

-- Danh sach mon hoc:
-- 2: Nhập môn lập trình
-- 51: Tin học cơ bản
-- 50: Kỹ năng học tập

-- Xoa du lieu cu
DELETE FROM PhienThiHocSinh;
DELETE FROM DiemDanhThi;
DELETE FROM ThiSinhCaThi;
DELETE FROM PhanCongGiamThi;
DELETE FROM CaThi;
DELETE FROM LichThiTong;

DECLARE @MonHocs TABLE (id INT, ten NVARCHAR(255));
INSERT INTO @MonHocs VALUES (2, N'Nhập môn lập trình'), (51, N'Tin học cơ bản'), (50, N'Kỹ năng học tập');

DECLARE @MonId INT, @MonTen NVARCHAR(255);
DECLARE cur CURSOR FOR SELECT id, ten FROM @MonHocs;
OPEN cur;
FETCH NEXT FROM cur INTO @MonId, @MonTen;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Tao de kiem tra
    DECLARE @DeKiemTraId INT;
    SELECT TOP 1 @DeKiemTraId = ma_de_kiem_tra FROM DeKiemTra WHERE ma_mon_hoc = @MonId;
    IF @DeKiemTraId IS NULL
    BEGIN
        INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi, ngay_tao)
        VALUES (@MonId, 1, N'Đề thi ' + @MonTen, 60, 
        '{"questions":[' +
        '{"id":1,"content":"Câu 1: Kiến thức cơ bản của ' + @MonTen + ' là gì?","type":"mcq","options":["A. Lựa chọn 1","B. Lựa chọn 2","C. Lựa chọn 3","D. Lựa chọn 4"],"answer":"A"},' +
        '{"id":2,"content":"Câu 2: Phát biểu nào đúng về ' + @MonTen + '?","type":"mcq","options":["A. Đúng 1","B. Đúng 2","C. Đúng 3","D. Đúng 4"],"answer":"B"},' +
        '{"id":3,"content":"Câu 3: Đặc điểm nổi bật của ' + @MonTen + '?","type":"mcq","options":["A. Đặc điểm A","B. Đặc điểm B","C. Đặc điểm C","D. Đặc điểm D"],"answer":"C"},' +
        '{"id":4,"content":"Câu 4: Ứng dụng của ' + @MonTen + '?","type":"mcq","options":["A. Ứng dụng X","B. Ứng dụng Y","C. Ứng dụng Z","D. Ứng dụng W"],"answer":"D"},' +
        '{"id":5,"content":"Câu 5: Nhận định sai về ' + @MonTen + '?","type":"mcq","options":["A. Sai 1","B. Sai 2","C. Sai 3","D. Sai 4"],"answer":"A"}' +
        ']}',
        'dang_mo', 'giua_ky', 'online_tu_do', GETDATE());
        SET @DeKiemTraId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE DeKiemTra SET cau_hinh_de_thi = 
        '{"questions":[' +
        '{"id":1,"content":"Câu 1: Kiến thức cơ bản của ' + @MonTen + ' là gì?","type":"mcq","options":["A. Lựa chọn 1","B. Lựa chọn 2","C. Lựa chọn 3","D. Lựa chọn 4"],"answer":"A"},' +
        '{"id":2,"content":"Câu 2: Phát biểu nào đúng về ' + @MonTen + '?","type":"mcq","options":["A. Đúng 1","B. Đúng 2","C. Đúng 3","D. Đúng 4"],"answer":"B"},' +
        '{"id":3,"content":"Câu 3: Đặc điểm nổi bật của ' + @MonTen + '?","type":"mcq","options":["A. Đặc điểm A","B. Đặc điểm B","C. Đặc điểm C","D. Đặc điểm D"],"answer":"C"},' +
        '{"id":4,"content":"Câu 4: Ứng dụng của ' + @MonTen + '?","type":"mcq","options":["A. Ứng dụng X","B. Ứng dụng Y","C. Ứng dụng Z","D. Ứng dụng W"],"answer":"D"},' +
        '{"id":5,"content":"Câu 5: Nhận định sai về ' + @MonTen + '?","type":"mcq","options":["A. Sai 1","B. Sai 2","C. Sai 3","D. Sai 4"],"answer":"A"}' +
        ']}' WHERE ma_de_kiem_tra = @DeKiemTraId;
    END

    -- Lich Thi Tong
    DECLARE @LichThiId INT;
    INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
    VALUES (@KyThiId, @MonId, @DeKiemTraId, 'online_tap_trung', GETDATE(), 'da_gui_ve_co_so', GETDATE());
    SET @LichThiId = SCOPE_IDENTITY();

    -- Ca Thi
    DECLARE @CaThiId INT;
    INSERT INTO CaThi (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ngay_tao)
    VALUES (@LichThiId, N'Thi ' + @MonTen, @MaPhong, CAST(GETDATE() AS DATE), GETDATE(), DATEADD(HOUR, 2, GETDATE()), 1, 'da_san_sang', GETDATE());
    SET @CaThiId = SCOPE_IDENTITY();

    -- Phan cong giam thi
    INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
    VALUES (@CaThiId, @MaGiamThi, 'giam_thi_chinh', 'da_xac_nhan', GETDATE());

    -- Thi sinh
    INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
    SELECT @CaThiId, ma_nguoi_dung, 'cho_thi', GETDATE()
    FROM NguoiDung 
    WHERE vai_tro_chinh = 'hoc_sinh' AND email LIKE '%sd1904%';

    FETCH NEXT FROM cur INTO @MonId, @MonTen;
END

CLOSE cur;
DEALLOCATE cur;
