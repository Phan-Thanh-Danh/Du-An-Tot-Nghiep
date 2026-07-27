USE LMS;
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY
    -------------------------------------------------------
    -- 1. RESTORE MON HOC 2: "Nhập môn lập trình"
    -------------------------------------------------------
    UPDATE DanhMucMonHoc
    SET ten_mon_hoc = N'Nhập môn lập trình',
        ma_code_mon_hoc = 'PRF192'
    WHERE ma_mon_hoc = 2;

    -- RESTORE DE KIEM TRA 13: "Đề thi Nhập môn lập trình"
    UPDATE DeKiemTra
    SET tieu_de = N'Đề thi Nhập môn lập trình',
        ma_mon_hoc = 2,
        thoi_gian_phut = 15,
        trang_thai = N'da_cong_bo'
    WHERE ma_de_kiem_tra = 13;

    -- RESTORE CA THI 86 to "Thi Nhập môn lập trình"
    UPDATE CaThi
    SET ten_ca_thi = N'Thi Nhập môn lập trình'
    WHERE ma_ca_thi = 86;

    UPDATE l
    SET l.ma_mon_hoc = 2,
        l.ma_de_kiem_tra = 13
    FROM LichThiTong l
    INNER JOIN CaThi c ON c.ma_lich_thi_tong = l.ma_lich_thi_tong
    WHERE c.ma_ca_thi = 86;

    -- Link original programming questions (178..182) back to DeKiemTra 13
    DELETE FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = 13;
    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES
    (13, 178, 2.00, 1),
    (13, 179, 2.00, 2),
    (13, 180, 2.00, 3),
    (13, 181, 2.00, 4),
    (13, 182, 2.00, 5);


    -------------------------------------------------------
    -- 2. ENSURE MON HOC 51: "Tin học cơ bản" ONLY
    -------------------------------------------------------
    UPDATE DanhMucMonHoc
    SET ten_mon_hoc = N'Tin học cơ bản',
        ma_code_mon_hoc = 'GEN102'
    WHERE ma_mon_hoc = 51;

    -- UPDATE DE KIEM TRA 11: "Đề thi trắc nghiệm - Môn: Tin học cơ bản"
    UPDATE DeKiemTra
    SET tieu_de = N'Đề thi trắc nghiệm - Môn: Tin học cơ bản',
        ma_mon_hoc = 51,
        thoi_gian_phut = 15,
        trang_thai = N'da_cong_bo'
    WHERE ma_de_kiem_tra = 11;

    -- UPDATE CA THI 85 to "Thi Tin học cơ bản"
    UPDATE CaThi
    SET ten_ca_thi = N'Thi Tin học cơ bản'
    WHERE ma_ca_thi = 85;

    UPDATE l
    SET l.ma_mon_hoc = 51,
        l.ma_de_kiem_tra = 11
    FROM LichThiTong l
    INNER JOIN CaThi c ON c.ma_lich_thi_tong = l.ma_lich_thi_tong
    WHERE c.ma_ca_thi = 85;

    -- Ensure DeKiemTra 11 links to the 5 Tin học cơ bản questions (193..197)
    DELETE FROM CauHoiDeKiemTra WHERE ma_de_kiem_tra = 11;
    INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES
    (11, 193, 2.00, 1),
    (11, 194, 2.00, 2),
    (11, 195, 2.00, 3),
    (11, 196, 2.00, 4),
    (11, 197, 2.00, 5);

    COMMIT TRANSACTION;
    PRINT N'Đã phục hồi Môn Nhập môn lập trình (Ca 86) và Đã chuẩn hóa Môn Tin học cơ bản (Ca 85)!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO
