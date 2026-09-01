USE LMS;
GO

SET NOCOUNT ON;
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU ĐỒNG BỘ KHÓA HỌC KỲ 1 THEO ĐÚNG KHUNG CHƯƠNG TRÌNH ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- =========================================================================
    -- BƯỚC 1: CẤP QUYỀN GIẢNG DẠY (GiaoVienMonHoc) CHO GIẢNG VIÊN THEO CÁC MÔN KỲ 1
    -- =========================================================================
    PRINT N'- Đang cấp quyền giảng dạy (GiaoVienMonHoc) cho các môn Kỳ 1...';
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, mct.ma_mon_hoc, 5, 10, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    JOIN ChuongTrinhDaoTao ct ON ct.ma_chuyen_nganh = gvcn.ma_chuyen_nganh
    JOIN MonHocTrongChuongTrinh mct ON mct.ma_chuong_trinh = ct.ma_chuong_trinh AND mct.hoc_ky_du_kien = 1
    WHERE NOT EXISTS (
        SELECT 1 FROM GiaoVienMonHoc gm 
        WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = mct.ma_mon_hoc
    );

    -- Cấp thêm cho các môn đại cương (ENG101, SSG101...) cho tất cả giảng viên nếu còn thiếu
    DECLARE @ENG101 INT, @SSG101 INT;
    SELECT @ENG101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'ENG101';
    SELECT @SSG101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'SSG101';

    IF @ENG101 IS NOT NULL
    BEGIN
        INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
        SELECT u.ma_nguoi_dung, @ENG101, 5, 10, 3, 0, 1, @CurrentDate
        FROM NguoiDung u
        WHERE u.vai_tro_chinh = 'giao_vien'
          AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = u.ma_nguoi_dung AND gm.ma_mon_hoc = @ENG101);
    END

    IF @SSG101 IS NOT NULL
    BEGIN
        INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
        SELECT u.ma_nguoi_dung, @SSG101, 5, 10, 3, 0, 1, @CurrentDate
        FROM NguoiDung u
        WHERE u.vai_tro_chinh = 'giao_vien'
          AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = u.ma_nguoi_dung AND gm.ma_mon_hoc = @SSG101);
    END

    -- =========================================================================
    -- BƯỚC 2: CHUYỂN MÔN KỲ 2 (DBI202) VỀ ĐÚNG HỌC KỲ 2 (NẾU ĐANG GẮN Ở HỌC KỲ 1)
    -- =========================================================================
    PRINT N'- Đang chuyển môn DBI202 về đúng Học kỳ 2...';
    DECLARE @DBI202 INT;
    SELECT @DBI202 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'DBI202';

    IF @DBI202 IS NOT NULL
    BEGIN
        -- Cập nhật ma_hoc_ky của DBI202 sang Học kỳ 2 (thu_tu_trong_nam = 2) cùng cơ sở
        UPDATE k
        SET k.ma_hoc_ky = hk2.ma_hoc_ky
        FROM KhoaHoc k
        JOIN HocKy hk1 ON k.ma_hoc_ky = hk1.ma_hoc_ky AND hk1.thu_tu_trong_nam = 1
        JOIN HocKy hk2 ON hk1.ma_don_vi = hk2.ma_don_vi AND hk2.nam_hoc = hk1.nam_hoc AND hk2.thu_tu_trong_nam = 2
        WHERE k.ma_mon_hoc = @DBI202;
    END

    -- =========================================================================
    -- BƯỚC 3: TỰ ĐỘNG MỞ ĐẦY ĐỦ CÁC MÔN HỌC KỲ 1 THEO ĐÚNG KHUNG CHƯƠNG TRÌNH CHO CÁC LỚP
    -- =========================================================================
    PRINT N'- Đang mở toàn bộ khóa học Kỳ 1 theo đúng Khung chương trình đào tạo của từng lớp...';

    DECLARE @LopCursor CURSOR;
    DECLARE @MaLop INT, @TenLop NVARCHAR(100), @MaDonVi INT, @MaChuongTrinh INT;
    DECLARE @MaHocKy1 INT;

    SET @LopCursor = CURSOR FOR
    SELECT l.ma_lop, l.ten_lop, l.ma_don_vi, l.ma_chuong_trinh
    FROM LopHanhChinh l
    WHERE l.ma_chuong_trinh IS NOT NULL;

    OPEN @LopCursor;
    FETCH NEXT FROM @LopCursor INTO @MaLop, @TenLop, @MaDonVi, @MaChuongTrinh;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT TOP 1 @MaHocKy1 = ma_hoc_ky 
        FROM HocKy 
        WHERE ma_don_vi = @MaDonVi AND nam_hoc = '2026' AND thu_tu_trong_nam = 1
        ORDER BY ma_hoc_ky;

        IF @MaHocKy1 IS NOT NULL
        BEGIN
            INSERT INTO KhoaHoc (ma_don_vi, ma_giao_vien, ma_hoc_ky, ma_lop, ma_mon_hoc, tieu_de, trang_thai, SoBlockHoc, ngay_tao)
            SELECT 
                @MaDonVi,
                ISNULL(
                    (SELECT TOP 1 gv.ma_nguoi_dung 
                     FROM NguoiDung gv 
                     JOIN GiaoVienMonHoc gm ON gm.ma_giao_vien = gv.ma_nguoi_dung
                     WHERE gm.ma_mon_hoc = mct.ma_mon_hoc AND gv.ma_don_vi = @MaDonVi
                     ORDER BY gv.ma_nguoi_dung),
                    (SELECT TOP 1 ma_giao_vien FROM GiaoVienMonHoc WHERE ma_mon_hoc = mct.ma_mon_hoc)
                ),
                @MaHocKy1,
                @MaLop,
                mct.ma_mon_hoc,
                mh.ten_mon_hoc + N' - ' + @TenLop,
                'da_xuat_ban',
                1,
                @CurrentDate
            FROM MonHocTrongChuongTrinh mct
            JOIN DanhMucMonHoc mh ON mh.ma_mon_hoc = mct.ma_mon_hoc
            WHERE mct.ma_chuong_trinh = @MaChuongTrinh 
              AND mct.hoc_ky_du_kien = 1
              AND mct.con_hoat_dong = 1
              AND NOT EXISTS (
                  SELECT 1 FROM KhoaHoc k 
                  WHERE k.ma_lop = @MaLop 
                    AND k.ma_mon_hoc = mct.ma_mon_hoc 
                    AND k.ma_hoc_ky = @MaHocKy1
              );
        END

        FETCH NEXT FROM @LopCursor INTO @MaLop, @TenLop, @MaDonVi, @MaChuongTrinh;
    END

    CLOSE @LopCursor;
    DEALLOCATE @LopCursor;

    COMMIT TRANSACTION;
    PRINT N'=== HOÀN TẤT ĐỒNG BỘ 100% KHÓA HỌC KỲ 1 VỚI KHUNG CHƯƠNG TRÌNH ===';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT N'LỖI: ' + @ErrMsg;
END CATCH;
GO
