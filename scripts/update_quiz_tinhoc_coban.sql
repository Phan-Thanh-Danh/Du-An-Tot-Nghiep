USE LMS;
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Ensure MonHoc 51 (GEN102) is named "Tin học cơ bản"
    UPDATE DanhMucMonHoc
    SET ten_mon_hoc = N'Tin học cơ bản',
        ma_code_mon_hoc = 'GEN102'
    WHERE ma_mon_hoc = 51;

    -- Also update MonHoc 2 to "Tin học cơ bản" just in case any legacy code references ma_mon_hoc = 2
    UPDATE DanhMucMonHoc
    SET ten_mon_hoc = N'Tin học cơ bản'
    WHERE ma_mon_hoc = 2;

    -- 2. Update DeKiemTra titles for ID 11 and ID 13
    UPDATE DeKiemTra
    SET tieu_de = N'Đề thi trắc nghiệm - Môn: Tin học cơ bản',
        ma_mon_hoc = 51,
        thoi_gian_phut = 15,
        trang_thai = N'da_cong_bo'
    WHERE ma_de_kiem_tra IN (11, 13);

    -- 3. Update CaThi 85 (and related active ca thi) to "Thi Tin học cơ bản"
    UPDATE CaThi
    SET ten_ca_thi = N'Thi Tin học cơ bản'
    WHERE ma_ca_thi = 85;

    UPDATE l
    SET l.ma_mon_hoc = 51,
        l.ma_de_kiem_tra = 11
    FROM LichThiTong l
    INNER JOIN CaThi c ON c.ma_lich_thi_tong = l.ma_lich_thi_tong
    WHERE c.ma_ca_thi = 85;

    -- 4. Delete old question links for DeKiemTra 11 and 13
    DELETE FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra IN (11, 13);

    -- 5. Insert 5 questions into CauHoi table
    DECLARE @Q1 INT, @Q2 INT, @Q3 INT, @Q4 INT, @Q5 INT;

    -- Question 1
    INSERT INTO CauHoi (ma_mon_hoc, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
    VALUES (
        51,
        'trac_nghiem',
        N'Thiết bị nào sau đây được xem là thiết bị nhập dữ liệu (Input Device)?',
        N'[{"id":"A","text":"Màn hình (Monitor)"},{"id":"B","text":"Máy in (Printer)"},{"id":"C","text":"Bàn phím (Keyboard)"},{"id":"D","text":"Loa (Speaker)"}]',
        N'["C"]',
        'trung_binh',
        1,
        'chon_mot',
        GETDATE(),
        GETDATE()
    );
    SET @Q1 = SCOPE_IDENTITY();

    -- Question 2
    INSERT INTO CauHoi (ma_mon_hoc, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
    VALUES (
        51,
        'trac_nghiem',
        N'Phần mềm nào sau đây thuộc bộ ứng dụng Microsoft Office?',
        N'[{"id":"A","text":"Adobe Photoshop"},{"id":"B","text":"Microsoft Word"},{"id":"C","text":"Google Chrome"},{"id":"D","text":"VLC Media Player"}]',
        N'["B"]',
        'trung_binh',
        1,
        'chon_mot',
        GETDATE(),
        GETDATE()
    );
    SET @Q2 = SCOPE_IDENTITY();

    -- Question 3
    INSERT INTO CauHoi (ma_mon_hoc, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
    VALUES (
        51,
        'trac_nghiem',
        N'Đuôi mở rộng mặc định của tệp văn bản Microsoft Word (từ phiên bản 2007 trở lên) là:',
        N'[{"id":"A","text":".xls"},{"id":"B","text":".pptx"},{"id":"C","text":".docx"},{"id":"D","text":".pdf"}]',
        N'["C"]',
        'trung_binh',
        1,
        'chon_mot',
        GETDATE(),
        GETDATE()
    );
    SET @Q3 = SCOPE_IDENTITY();

    -- Question 4
    INSERT INTO CauHoi (ma_mon_hoc, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
    VALUES (
        51,
        'trac_nghiem',
        N'Tổ hợp phím Ctrl + C trong Windows có chức năng gì?',
        N'[{"id":"A","text":"Cắt dữ liệu."},{"id":"B","text":"Dán dữ liệu."},{"id":"C","text":"Sao chép dữ liệu."},{"id":"D","text":"Hoàn tác thao tác."}]',
        N'["C"]',
        'trung_binh',
        1,
        'chon_mot',
        GETDATE(),
        GETDATE()
    );
    SET @Q4 = SCOPE_IDENTITY();

    -- Question 5
    INSERT INTO CauHoi (ma_mon_hoc, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
    VALUES (
        51,
        'trac_nghiem',
        N'Thiết bị nào sau đây có chức năng lưu trữ dữ liệu lâu dài?',
        N'[{"id":"A","text":"RAM"},{"id":"B","text":"CPU"},{"id":"C","text":"Ổ cứng (HDD/SSD)"},{"id":"D","text":"Card mạng"}]',
        N'["C"]',
        'trung_binh',
        1,
        'chon_mot',
        GETDATE(),
        GETDATE()
    );
    SET @Q5 = SCOPE_IDENTITY();

    -- 6. Link questions to DeKiemTra 11 and DeKiemTra 13 (2.0 points per question = 10 points total)
    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES
    (11, @Q1, 2.00, 1),
    (11, @Q2, 2.00, 2),
    (11, @Q3, 2.00, 3),
    (11, @Q4, 2.00, 4),
    (11, @Q5, 2.00, 5),
    (13, @Q1, 2.00, 1),
    (13, @Q2, 2.00, 2),
    (13, @Q3, 2.00, 3),
    (13, @Q4, 2.00, 4),
    (13, @Q5, 2.00, 5);

    COMMIT TRANSACTION;
    PRINT N'Cập nhật thành công 5 câu hỏi Đề thi trắc nghiệm - Môn: Tin học cơ bản!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO
