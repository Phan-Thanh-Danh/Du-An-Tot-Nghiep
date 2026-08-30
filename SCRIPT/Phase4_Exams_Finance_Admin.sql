USE LMS;
GO

SET NOCOUNT ON;

DECLARE @N INT = 5; 
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 4: KỲ THI, TÀI CHÍNH, HÀNH CHÍNH & KHEN THƯỞNG ---';

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @i INT = 1;
    WHILE @i <= @N
    BEGIN
        DECLARE @CampusCode NVARCHAR(50) = 'CAMPUS_AET_' + CAST(@i AS NVARCHAR);
        DECLARE @CampusId INT;
        SELECT @CampusId = ma_don_vi FROM DonVi WHERE ma_code = @CampusCode;

        DECLARE @TermId INT;
        SELECT @TermId = ma_hoc_ky FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1;

        IF @CampusId IS NOT NULL AND @TermId IS NOT NULL
        BEGIN
            PRINT N'>> Xử lý dữ liệu cho ' + @CampusCode;

            -- ==========================================
            -- 1. TÀI CHÍNH: PHÁT SINH HỌC PHÍ (HOADON & GIAODICH)
            -- ==========================================
            WITH Students AS (
                SELECT ma_nguoi_dung FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId
            )
            INSERT INTO HoaDon (ma_hoa_don_code, ma_don_vi, ma_hoc_ky, ma_hoc_sinh, loai_hoa_don, so_tien, trang_thai, ngay_tao)
            SELECT 
                'INV_' + @CampusCode + '_' + CAST(@TermId AS NVARCHAR) + '_' + CAST(ma_nguoi_dung AS NVARCHAR),
                @CampusId,
                @TermId,
                ma_nguoi_dung,
                'hoc_phi', -- Fixed CK_HoaDon_loai_hoa_don
                15000000, 
                CASE WHEN (ma_nguoi_dung % 10) = 0 THEN 'chua_thanh_toan' ELSE 'da_thanh_toan' END,
                @CurrentDate
            FROM Students;

            INSERT INTO GiaoDich (ma_hoa_don, ma_tham_chieu_noi_bo, loai_giao_dich, so_tien, trang_thai, nha_cung_cap_thanh_toan, ngay_tao)
            SELECT 
                ma_hoa_don,
                'TXN_' + ma_hoa_don_code,
                'thanh_toan_hoc_phi', -- Fixed CK_GiaoDich_loai_giao_dich
                so_tien,
                'thanh_cong', -- Fixed CK_GiaoDich_trang_thai
                'payos', -- Fixed CK_GiaoDich_provider
                @CurrentDate
            FROM HoaDon 
            WHERE ma_don_vi = @CampusId AND trang_thai = 'da_thanh_toan';

            -- ==========================================
            -- 2. KỲ THI (KYTHI)
            -- ==========================================
            INSERT INTO KyThi (ten_ky_thi, ma_hoc_ky, ma_don_vi, ngay_bat_dau, ngay_ket_thuc, loai_ky_thi, trang_thai, ngay_tao)
            VALUES (N'Kỳ thi cuối kỳ Mùa Thu 2026', @TermId, @CampusId, DATEADD(MONTH, 3, @CurrentDate), DATEADD(MONTH, 3, DATEADD(DAY, 14, @CurrentDate)), 'cuoi_ky', 'nhap', @CurrentDate);
            DECLARE @ExamId INT = SCOPE_IDENTITY();

            DECLARE @COM101 INT;
            SELECT @COM101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101';
            
            INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, thoi_luong_phut, hinh_thuc_thi, trang_thai)
            VALUES (@ExamId, @COM101, 90, 'online_tap_trung', 'nhap');
            DECLARE @ScheduleId INT = SCOPE_IDENTITY();

            INSERT INTO CaThi (ma_lich_thi_tong, ma_phong, thoi_gian_bat_dau, thoi_gian_ket_thuc, so_luong_giam_thi, trang_thai)
            VALUES (@ScheduleId, (SELECT TOP 1 ma_phong FROM PhongHoc p JOIN ToaNha t ON p.ma_toa_nha = t.ma_toa_nha WHERE t.ma_don_vi = @CampusId ORDER BY NEWID()), 
            DATEADD(MONTH, 3, @CurrentDate), DATEADD(HOUR, 2, DATEADD(MONTH, 3, @CurrentDate)), 1, 'da_san_sang');
            DECLARE @SessionId INT = SCOPE_IDENTITY();

            INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, so_bao_danh, trang_thai_du_thi)
            SELECT TOP 35 @SessionId, ma_nguoi_dung, 'SBD' + CAST(ma_nguoi_dung AS NVARCHAR), 'duoc_thi'
            FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId;

            INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giao_vien, vai_tro_giam_thi)
            SELECT TOP 1 @SessionId, ma_nguoi_dung, 'giam_thi_chinh'
            FROM NguoiDung WHERE vai_tro_chinh = 'giao_vien' AND ma_don_vi = @CampusId ORDER BY NEWID();

            -- ==========================================
            -- 3. ĐƠN TỪ (DONTU) & HỖ TRỢ
            -- ==========================================
            DECLARE @MauDonNghiHoc INT;
            SELECT TOP 1 @MauDonNghiHoc = ma_mau_don FROM MauDonTu WHERE loai_don = 'nghi_phep';

            IF @MauDonNghiHoc IS NOT NULL
            BEGIN
                INSERT INTO DonTu (ma_mau_don, ma_hoc_sinh, ma_don_vi, loai_don, tieu_de, du_lieu_bieu_mau, trang_thai, trang_thai_xu_ly_nghiep_vu, ngay_nop, ngay_tao)
                SELECT TOP 100 
                    @MauDonNghiHoc, ma_nguoi_dung, @CampusId, 'nghi_phep', N'Đơn xin nghỉ phép', '{}', 'da_nop', 'chua_xu_ly', @CurrentDate, @CurrentDate
                FROM NguoiDung WHERE vai_tro_chinh = 'hoc_sinh' AND ma_don_vi = @CampusId ORDER BY NEWID();
            END

            -- ==========================================
            -- 4. KHEN THƯỞNG (DOTKHENTHUONG)
            -- ==========================================
            INSERT INTO DotKhenThuong (ten_dot, ma_hoc_ky, ma_don_vi, loai_dot, so_luong_toi_da, trang_thai, ngay_tao)
            VALUES (N'Tuyên dương Top GPA Khóa Thu 2026', @TermId, @CampusId, 'TOP_100_HOC_KY', 100, 'da_cong_bo', @CurrentDate);
            DECLARE @RewardId INT = SCOPE_IDENTITY();

            INSERT INTO UngVienKhenThuong (ma_dot_khen_thuong, ma_hoc_sinh, diem_xet, xep_hang, trang_thai)
            SELECT TOP 100 @RewardId, ds.ma_hoc_sinh, ds.gpa_mon_hoc, ROW_NUMBER() OVER(ORDER BY ds.gpa_mon_hoc DESC), 'da_duyet'
            FROM DiemSo ds
            WHERE ds.ma_don_vi = @CampusId AND ds.ma_mon_hoc = @COM101 AND ds.trang_thai = 'dat'
            ORDER BY ds.gpa_mon_hoc DESC;
            
            INSERT INTO KhenThuong (ma_ung_vien, ma_hoc_sinh, ma_don_vi, ma_hoc_ky, loai_khen_thuong, ma_dot_khen_thuong, danh_hieu, xep_hang, url_chung_tu, trang_thai)
            SELECT ma_ung_vien, ma_hoc_sinh, @CampusId, @TermId, 'TOP_100_HOC_KY', ma_dot_khen_thuong, N'Sinh viên Giỏi', xep_hang, 'N/A', 'da_cap'
            FROM UngVienKhenThuong WHERE ma_dot_khen_thuong = @RewardId;
        END

        SET @i += 1;
    END

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH TOÀN BỘ PHASE 4 THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA TRONG PHASE 4 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
