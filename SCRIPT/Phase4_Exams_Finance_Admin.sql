USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 4: KỲ THI, TÀI CHÍNH, HÀNH CHÍNH & KHEN THƯỞNG ---';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CampusId INT;
    DECLARE @CampusIdx INT = 0;
    DECLARE curCS CURSOR LOCAL FOR SELECT ma_don_vi FROM DonVi WHERE cap_don_vi = 'co_so' ORDER BY ma_don_vi;
    OPEN curCS;
    FETCH NEXT FROM curCS INTO @CampusId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @CampusIdx += 1;
        DECLARE @CampusCode NVARCHAR(50) = 'CAMPUS_' + CAST(@CampusIdx AS NVARCHAR);

        DECLARE @TermId INT;
        SELECT @TermId = ma_hoc_ky FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1;

        IF @CampusId IS NOT NULL AND @TermId IS NOT NULL
        BEGIN
            PRINT N'>> Xu ly du lieu cho ' + @CampusCode;

            -- 1. TAI CHINH
            WITH Students AS (
                SELECT ma_nguoi_dung FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId
            )
            -- HoaDon: han_thanh_toan NOT NULL (DateOnly/date), giam_tru NOT NULL (default 0), da_thanh_toan NOT NULL (default 0)
            INSERT INTO HoaDon (ma_hoa_don_code, ma_don_vi, ma_hoc_ky, ma_hoc_sinh, loai_hoa_don, so_tien, giam_tru, da_thanh_toan, trang_thai, han_thanh_toan, ngay_tao)
            SELECT
                'INV_' + @CampusCode + '_' + CAST(@TermId AS NVARCHAR) + '_' + CAST(ma_nguoi_dung AS NVARCHAR),
                @CampusId, @TermId, ma_nguoi_dung, 'hoc_phi',
                15000000,   -- so_tien
                0,          -- giam_tru
                CASE WHEN (ma_nguoi_dung % 10) != 0 THEN 15000000 ELSE 0 END, -- da_thanh_toan
                CASE WHEN (ma_nguoi_dung % 10) = 0 THEN 'chua_thanh_toan' ELSE 'da_thanh_toan' END,
                CAST(DATEADD(MONTH, 2, @CurrentDate) AS DATE), -- han_thanh_toan: 2 tháng kể từ bây giờ
                @CurrentDate
            FROM Students
            WHERE NOT EXISTS (
                SELECT 1 FROM HoaDon h
                WHERE h.ma_hoa_don_code = 'INV_' + @CampusCode + '_' + CAST(@TermId AS NVARCHAR) + '_' + CAST(ma_nguoi_dung AS NVARCHAR)
            );

            INSERT INTO GiaoDich (ma_hoa_don, ma_tham_chieu_noi_bo, loai_giao_dich, so_tien, trang_thai, nha_cung_cap_thanh_toan, ngay_tao)
            SELECT ma_hoa_don, 'TXN_' + ma_hoa_don_code, 'thanh_toan_hoc_phi', so_tien, 'thanh_cong', 'payos', @CurrentDate
            FROM HoaDon
            WHERE ma_don_vi = @CampusId AND trang_thai = 'da_thanh_toan'
            AND NOT EXISTS (SELECT 1 FROM GiaoDich g WHERE g.ma_tham_chieu_noi_bo = 'TXN_' + HoaDon.ma_hoa_don_code);

            -- 2. KY THI
            -- KyThi model: ten_ky_thi, ma_hoc_ky, loai_ky_thi, trang_thai (KHONG CO ma_don_vi, ngay_bat_dau, ngay_ket_thuc)
            DECLARE @ExamId INT;
            SELECT TOP 1 @ExamId = ma_ky_thi FROM KyThi WHERE ma_hoc_ky = @TermId AND loai_ky_thi = 'cuoi_ky';
            IF @ExamId IS NULL
            BEGIN
                INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, loai_ky_thi, trang_thai, ngay_tao)
                VALUES (N'Ky thi cuoi ky Thu 2026 - ' + @CampusCode, @TermId, 'cuoi_ky', 'nhap', @CurrentDate);
                SET @ExamId = SCOPE_IDENTITY();
            END

            DECLARE @COM101 INT;
            SELECT @COM101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101';

            -- LichThiTong model: ma_ky_thi, ma_mon_hoc, hinh_thuc_thi, ngay_thi_du_kien, trang_thai (KHONG CO thoi_luong_phut)
            DECLARE @ScheduleId INT;
            SELECT @ScheduleId = ma_lich_thi_tong FROM LichThiTong WHERE ma_ky_thi = @ExamId AND ma_mon_hoc = @COM101;
            IF @ScheduleId IS NULL
            BEGIN
                INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
                VALUES (@ExamId, @COM101, 'online_tap_trung', DATEADD(MONTH, 3, @CurrentDate), 'nhap', @CurrentDate);
                SET @ScheduleId = SCOPE_IDENTITY();
            END

            -- CaThi model: ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai
            -- (KHONG CO so_luong_giam_thi)
            DECLARE @SessionId INT;
            SELECT @SessionId = ma_ca_thi FROM CaThi WHERE ma_lich_thi_tong = @ScheduleId AND ma_don_vi = @CampusId;
            IF @SessionId IS NULL
            BEGIN
                INSERT INTO CaThi (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ngay_tao)
                VALUES (
                    @ScheduleId,
                    N'Ca thi COM101 - ' + @CampusCode,
                    (SELECT TOP 1 ma_phong FROM PhongHoc WHERE ma_don_vi = @CampusId AND trang_thai_phong = 'hoat_dong' ORDER BY NEWID()),
                    CAST(DATEADD(MONTH, 3, @CurrentDate) AS DATE),
                    DATEADD(MONTH, 3, @CurrentDate),
                    DATEADD(HOUR, 2, DATEADD(MONTH, 3, @CurrentDate)),
                    @CampusId, 'da_san_sang', @CurrentDate
                );
                SET @SessionId = SCOPE_IDENTITY();
            END

            -- ThiSinhCaThi model: ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi (KHONG CO so_bao_danh)
            INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
            SELECT TOP 35 @SessionId, ma_nguoi_dung, 'duoc_thi', @CurrentDate
            FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId
            AND NOT EXISTS (SELECT 1 FROM ThiSinhCaThi t WHERE t.ma_ca_thi = @SessionId AND t.ma_hoc_sinh = NguoiDung.ma_nguoi_dung)
            ORDER BY NEWID();

            -- PhanCongGiamThi: cot la ma_giam_thi (KHONG PHAI ma_giao_vien)
            INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
            SELECT TOP 1 @SessionId, ma_nguoi_dung, 'giam_thi_chinh', 'du_kien', @CurrentDate
            FROM NguoiDung WHERE vai_tro_chinh = 'giao_vien' AND ma_don_vi = @CampusId
            AND NOT EXISTS (SELECT 1 FROM PhanCongGiamThi p WHERE p.ma_ca_thi = @SessionId AND p.ma_giam_thi = NguoiDung.ma_nguoi_dung)
            ORDER BY NEWID();

            -- 3. DON TU
            DECLARE @MauDonNghiHoc INT;
            SELECT TOP 1 @MauDonNghiHoc = ma_mau_don FROM MauDonTu WHERE loai_don = 'nghi_phep';
            IF @MauDonNghiHoc IS NOT NULL
            BEGIN
                INSERT INTO DonTu (ma_mau_don, ma_hoc_sinh, ma_don_vi, loai_don, tieu_de, du_lieu_bieu_mau, trang_thai, trang_thai_xu_ly_nghiep_vu, ngay_nop, ngay_tao)
                SELECT TOP 100
                    @MauDonNghiHoc, ma_nguoi_dung, @CampusId, 'nghi_phep',
                    N'Don xin nghi phep', '{}', 'da_nop', 'chua_xu_ly', @CurrentDate, @CurrentDate
                FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId
                AND NOT EXISTS (SELECT 1 FROM DonTu d WHERE d.ma_hoc_sinh = NguoiDung.ma_nguoi_dung AND d.loai_don = 'nghi_phep')
                ORDER BY NEWID();
            END

            -- 4. KHEN THUONG
            -- DotKhenThuong PK la ma_dot_khen_thuong (KHONG PHAI ma_dot), can co nguoi_tao
            DECLARE @RewardId INT;
            DECLARE @SuperAdminId INT;
            SELECT TOP 1 @SuperAdminId = ma_nguoi_dung FROM NguoiDung WHERE vai_tro_chinh = 'sieu_quan_tri';

            SELECT @RewardId = ma_dot_khen_thuong
            FROM DotKhenThuong WHERE ma_hoc_ky = @TermId AND ma_don_vi = @CampusId AND loai_dot = 'TOP_100_HOC_KY';

            IF @RewardId IS NULL AND @SuperAdminId IS NOT NULL
            BEGIN
                INSERT INTO DotKhenThuong (ten_dot, ma_hoc_ky, ma_don_vi, loai_dot, so_luong_toi_da, trang_thai, nguoi_tao, ngay_tao)
                VALUES (N'Tuyen duong Top GPA Thu 2026', @TermId, @CampusId, 'TOP_100_HOC_KY', 100, 'da_cong_bo', @SuperAdminId, @CurrentDate);
                SET @RewardId = SCOPE_IDENTITY();
            END

            IF @RewardId IS NOT NULL
            BEGIN
                -- UngVienKhenThuong: Lấy Top 100 GPA toàn cơ sở (cho sinh viên của mọi ngành)
                WITH TopStudents AS (
                    SELECT 
                        ds.ma_hoc_sinh,
                        AVG(ds.gpa_mon_hoc) AS gpa_tb,
                        ROW_NUMBER() OVER(ORDER BY AVG(ds.gpa_mon_hoc) DESC) AS xep_hang
                    FROM DiemSo ds
                    WHERE ds.ma_don_vi = @CampusId AND ds.trang_thai = 'dat'
                    GROUP BY ds.ma_hoc_sinh
                )
                INSERT INTO UngVienKhenThuong (ma_dot_khen_thuong, ma_hoc_sinh, ma_hoc_ky, diem_xet, xep_hang, trang_thai, ngay_tao)
                SELECT TOP 100 
                    @RewardId, ts.ma_hoc_sinh, @TermId, CAST(ts.gpa_tb AS DECIMAL(4,2)),
                    CAST(ts.xep_hang AS INT),
                    'da_duyet', @CurrentDate
                FROM TopStudents ts
                WHERE NOT EXISTS (
                    SELECT 1 FROM UngVienKhenThuong u
                    WHERE u.ma_dot_khen_thuong = @RewardId AND u.ma_hoc_sinh = ts.ma_hoc_sinh
                )
                ORDER BY ts.xep_hang;

                -- KhenThuong: ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_dot_khen_thuong, loai_khen_thuong,
                --             trang_thai, url_chung_tu, cap_luc, da_huy, danh_hieu_snapshot
                -- (KHONG CO: ma_ung_vien, danh_hieu)
                INSERT INTO KhenThuong (ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_dot_khen_thuong, loai_khen_thuong, trang_thai, url_chung_tu, cap_luc, da_huy, gpa_dat_duoc, xep_hang, danh_hieu_snapshot)
                SELECT
                    @CampusId, uv.ma_hoc_sinh, @TermId, @RewardId,
                    'TOP_100_HOC_KY', 'da_cap', 'N/A', @CurrentDate, 0,
                    uv.diem_xet, uv.xep_hang, N'Sinh vien Gioi'
                FROM UngVienKhenThuong uv
                WHERE uv.ma_dot_khen_thuong = @RewardId
                AND NOT EXISTS (
                    SELECT 1 FROM KhenThuong k
                    WHERE k.ma_dot_khen_thuong = @RewardId AND k.ma_hoc_sinh = uv.ma_hoc_sinh
                );
            END
        END

        FETCH NEXT FROM curCS INTO @CampusId;
    END
    CLOSE curCS;
    DEALLOCATE curCS;

    COMMIT TRANSACTION;
    PRINT N'--- HOAN THANH TOAN BO PHASE 4 THANH CONG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CO LOI XAY RA TRONG PHASE 4 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
