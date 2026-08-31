USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BAT DAU PHASE 5: NOI DUNG MON HOC (LMS) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. TAO HOC LIEU DUNG CHUNG
    -- ==========================================
    PRINT N'>> Tao CourseSyllabus, Chuong, BaiHoc, BaiHocNoiDung...';

    DECLARE @Subjects TABLE (MaMon INT, Code NVARCHAR(50), Ten NVARCHAR(255));
    INSERT INTO @Subjects
    SELECT ma_mon_hoc, ma_code_mon_hoc, ten_mon_hoc FROM DanhMucMonHoc
    WHERE ma_code_mon_hoc IN ('COM101', 'DBI202', 'WEB104', 'UIX101', 'MKT101');

    -- a. CourseSyllabus (KHONG phai DeCuongMonHoc)
    -- Model: ma_mon_hoc, ma_chuyen_nganh, ten_syllabus, version, trang_thai, bat_buoc, con_hoat_dong, ngay_tao
    -- KHONG CO cot: mo_ta
    INSERT INTO CourseSyllabus (ma_mon_hoc, ma_chuyen_nganh, ten_syllabus, version, trang_thai, bat_buoc, con_hoat_dong, ngay_tao)
    SELECT s.MaMon, c.ma_chuyen_nganh, N'De cuong ' + s.Ten, 'v1.0', 'active', 1, 1, @CurrentDate
    FROM @Subjects s
    JOIN ChuyenNganh c ON (
        (s.Code IN ('COM101', 'DBI202', 'WEB104') AND c.ten_chuyen_nganh = N'Ky thuat phan mem') OR
        (s.Code = 'UIX101' AND c.ten_chuyen_nganh = N'Thiet ke do hoa') OR
        (s.Code = 'MKT101' AND c.ten_chuyen_nganh = N'Digital Marketing')
    )
    WHERE NOT EXISTS (
        SELECT 1 FROM CourseSyllabus d WHERE d.ma_mon_hoc = s.MaMon AND d.ma_chuyen_nganh = c.ma_chuyen_nganh
    );

    -- b. Chuong - 5 chuong moi mon
    -- Model: ma_mon_hoc, tieu_de, thu_tu, da_an, ngay_tao
    WITH Numbers AS (SELECT TOP 5 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO Chuong (ma_mon_hoc, tieu_de, thu_tu, da_an, ngay_tao)
    SELECT s.MaMon, N'Chuong ' + CAST(n.N AS NVARCHAR) + N': Kien thuc cot loi', n.N, 0, @CurrentDate
    FROM @Subjects s CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM Chuong c WHERE c.ma_mon_hoc = s.MaMon AND c.thu_tu = n.N);

    -- c. BaiHoc - 3 bai moi chuong
    -- Model: ma_chuong, tieu_de, loai_bai_hoc, thu_tu, da_an, trang_thai, ngay_tao
    WITH Numbers AS (SELECT TOP 3 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO BaiHoc (ma_chuong, tieu_de, loai_bai_hoc, thu_tu, da_an, trang_thai, ngay_tao)
    SELECT c.ma_chuong, N'Bai hoc ' + CAST(n.N AS NVARCHAR), n.N, 0, 'da_xuat_ban', @CurrentDate
    FROM Chuong c CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM BaiHoc b WHERE b.ma_chuong = c.ma_chuong AND b.thu_tu = n.N);

    -- d. BaiHocNoiDung - 1 Video + 1 TaiLieu moi bai hoc
    -- Model: ma_bai_hoc, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai, ngay_tao
    -- KHONG CO cot: tieu_de
    INSERT INTO BaiHocNoiDung (ma_bai_hoc, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai, ngay_tao)
    SELECT ma_bai_hoc, 'video', 1, 1800, 'https://cdn.lms.local/video.mp4', 'da_xuat_ban', @CurrentDate
    FROM BaiHoc
    WHERE NOT EXISTS (
        SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = BaiHoc.ma_bai_hoc AND nd.loai_noi_dung = 'video'
    );

    INSERT INTO BaiHocNoiDung (ma_bai_hoc, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai, ngay_tao)
    SELECT ma_bai_hoc, 'tai_lieu', 2, 0, 'https://cdn.lms.local/doc.pdf', 'da_xuat_ban', @CurrentDate
    FROM BaiHoc
    WHERE NOT EXISTS (
        SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = BaiHoc.ma_bai_hoc AND nd.loai_noi_dung = 'tai_lieu'
    );

    -- ==========================================
    -- 2. TIEN DO HOC TAP SINH VIEN (1 co so, gioi han data)
    -- ==========================================
    PRINT N'>> Tao Tien do hoc tap Sinh vien...';

    DECLARE @Campus1 INT;
    SELECT TOP 1 @Campus1 = ma_don_vi FROM DonVi WHERE cap_don_vi = 'co_so' ORDER BY ma_don_vi;

    DECLARE @CourseCOM101 INT;
    SELECT TOP 1 @CourseCOM101 = ma_khoa_hoc FROM KhoaHoc
    WHERE ma_don_vi = @Campus1 AND ma_mon_hoc = (SELECT ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101');

    IF @CourseCOM101 IS NOT NULL
    BEGIN
        -- a. TienDoNoiDungHocTap
        -- Model: ma_hoc_sinh, ma_noi_dung, loai_noi_dung, trang_thai, phan_tram_tien_do,
        --        so_giay_da_xac_nhan, phan_tram_cuon_lon_nhat, lan_tuong_tac_cuoi, ngay_tao
        INSERT INTO TienDoNoiDungHocTap (ma_hoc_sinh, ma_noi_dung, loai_noi_dung, phan_tram_tien_do, phan_tram_cuon_lon_nhat, so_giay_da_xac_nhan, trang_thai, lan_tuong_tac_cuoi, ngay_tao)
        SELECT
            u.ma_nguoi_dung,
            nd.ma_noi_dung,
            nd.loai_noi_dung,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 50.0
                 ELSE 0.0 END,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0 ELSE 50.0 END,
            1800,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 'hoan_thanh'
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 'dang_hoc'
                 ELSE 'chua_bat_dau' END,
            @CurrentDate,
            @CurrentDate
        FROM NguoiDung u
        JOIN KhoaHoc kh ON u.ma_lop = kh.ma_lop
        JOIN Chuong c ON kh.ma_mon_hoc = c.ma_mon_hoc
        JOIN BaiHoc bh ON c.ma_chuong = bh.ma_chuong
        JOIN BaiHocNoiDung nd ON bh.ma_bai_hoc = nd.ma_bai_hoc
        WHERE kh.ma_khoa_hoc = @CourseCOM101 AND u.vai_tro_chinh = 'hoc_sinh'
        AND NOT EXISTS (
            SELECT 1 FROM TienDoNoiDungHocTap t WHERE t.ma_hoc_sinh = u.ma_nguoi_dung AND t.ma_noi_dung = nd.ma_noi_dung
        );

        -- b. TienDoBaiHoc
        -- Model: ma_hoc_sinh, ma_bai_hoc, phan_tram_tien_do, hoan_thanh_luc
        -- KHONG CO cot: lan_gui_nhip_tim_cuoi (nullable, ko can), ghi_chu (nullable)
        INSERT INTO TienDoBaiHoc (ma_hoc_sinh, ma_bai_hoc, phan_tram_tien_do, hoan_thanh_luc)
        SELECT
            u.ma_nguoi_dung,
            bh.ma_bai_hoc,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 50.0
                 ELSE 0.0 END,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN @CurrentDate ELSE NULL END
        FROM NguoiDung u
        JOIN KhoaHoc kh ON u.ma_lop = kh.ma_lop
        JOIN Chuong c ON kh.ma_mon_hoc = c.ma_mon_hoc
        JOIN BaiHoc bh ON c.ma_chuong = bh.ma_chuong
        WHERE kh.ma_khoa_hoc = @CourseCOM101 AND u.vai_tro_chinh = 'hoc_sinh'
        AND NOT EXISTS (
            SELECT 1 FROM TienDoBaiHoc t WHERE t.ma_hoc_sinh = u.ma_nguoi_dung AND t.ma_bai_hoc = bh.ma_bai_hoc
        );
    END

    -- ==========================================
    -- 3. LOG PHIEN HOC & BINH LUAN (~100 dong)
    -- ==========================================
    PRINT N'>> Tao Log Phien hoc & Binh luan (~100 dong)...';

    -- PhienHocNoiDung:
    -- Model: ma_hoc_sinh, ma_noi_dung, session_token(Guid), bat_dau_luc, ket_thuc_luc,
    --        dia_chi_ip_hash, user_agent_hash, nhip_tim_cuoi_luc, vi_tri_video_cuoi_giay,
    --        so_giay_hoat_dong_da_xac_nhan, so_thu_tu_nhip_tim_cuoi, trang_thai, ngay_tao
    INSERT INTO PhienHocNoiDung (ma_hoc_sinh, ma_noi_dung, session_token, bat_dau_luc, ket_thuc_luc,
                                  dia_chi_ip_hash, user_agent_hash, nhip_tim_cuoi_luc,
                                  vi_tri_video_cuoi_giay, so_giay_hoat_dong_da_xac_nhan,
                                  so_thu_tu_nhip_tim_cuoi, trang_thai, ngay_tao)
    SELECT TOP 100
        ma_hoc_sinh,
        ma_noi_dung,
        NEWID(),
        DATEADD(MINUTE, -30, @CurrentDate),
        @CurrentDate,
        '192.168.1.x',
        'Chrome/120',
        @CurrentDate,
        900,
        1800,
        1,
        'da_ket_thuc',
        @CurrentDate
    FROM TienDoNoiDungHocTap WHERE phan_tram_tien_do > 0;

    -- BinhLuan: ma_bai_hoc, ma_nguoi_dung, noi_dung, giay_trong_video, da_ghim, ngay_tao
    INSERT INTO BinhLuan (ma_bai_hoc, ma_nguoi_dung, noi_dung, giay_trong_video, da_ghim, ngay_tao)
    SELECT TOP 100
        ma_bai_hoc,
        ma_hoc_sinh,
        N'Thay oi cho em hoi doan video nay giai thich them duoc khong a?',
        300,
        0,
        @CurrentDate
    FROM TienDoBaiHoc WHERE phan_tram_tien_do > 0 ORDER BY NEWID();

    COMMIT TRANSACTION;
    PRINT N'--- HOAN THANH TOAN BO PHASE 5 THANH CONG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CO LOI XAY RA TRONG PHASE 5 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
