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
        INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, trang_thai, loai_de_thi, hinh_thuc_thi, ngay_tao)
        VALUES (@MonId, 1, N'Đề thi ' + @MonTen, 60, 'dang_mo', 'giua_ky', 'online_tu_do', GETDATE());
        SET @DeKiemTraId = SCOPE_IDENTITY();
    END

    -- Xóa câu hỏi cũ của đề thi này để seed lại
    DELETE FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = @DeKiemTraId;

    DECLARE @i INT = 1;
    DECLARE @CauHoiId INT;
    WHILE @i <= 5
    BEGIN
        INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, kieu_lua_chon, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, ngay_tao)
        VALUES (
            @MonId, 
            15, 
            'trac_nghiem', 
            'chon_mot', 
            N'Câu ' + CAST(@i AS NVARCHAR) + N': Kiến thức liên quan đến ' + @MonTen + N' (Sample)?', 
            N'[{"id":"A","text":"Lựa chọn A"},{"id":"B","text":"Lựa chọn B"},{"id":"C","text":"Lựa chọn C"},{"id":"D","text":"Lựa chọn D"}]', 
            N'["A"]',
            'de',
            1, 
            GETDATE()
        );
        SET @CauHoiId = SCOPE_IDENTITY();

        INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu)
        VALUES (@DeKiemTraId, @CauHoiId, 2.0, @i);

        SET @i = @i + 1;
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
    WHERE vai_tro_chinh = 'hoc_sinh' AND (email LIKE '%sd1904%' OR email = 'student01@edulms.local');

    FETCH NEXT FROM cur INTO @MonId, @MonTen;
END

CLOSE cur;
DEALLOCATE cur;
