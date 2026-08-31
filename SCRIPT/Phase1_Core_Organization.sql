USE LMS;
GO

SET NOCOUNT ON;
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 1: HỆ THỐNG TỔ CHỨC & HỌC THUẬT ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. HỆ THỐNG TỔ CHỨC (DonVi)
    -- Cấu trúc: Root -> 5 cơ sở (co_so)
    -- Không tạo Khoa (co_so_con) để đơn giản hóa
    -- ==========================================
    DECLARE @RootId INT;
    IF NOT EXISTS (SELECT 1 FROM DonVi WHERE cap_don_vi = 'root')
    BEGIN
        INSERT INTO DonVi (ten_don_vi, cap_don_vi, con_hoat_dong, ngay_tao)
        VALUES (N'Trường Đại học AET', 'root', 1, @CurrentDate);
        SET @RootId = SCOPE_IDENTITY();
    END
    ELSE
        SELECT @RootId = ma_don_vi FROM DonVi WHERE cap_don_vi = 'root';

    -- Tạo 5 cơ sở theo tên thực tế
    DECLARE @Campuses TABLE (ten NVARCHAR(255));
    INSERT INTO @Campuses VALUES
        (N'Trường AET Cơ sở TP.HCM'),
        (N'Trường AET Cơ sở Hà Nội'),
        (N'Trường AET Cơ sở Đà Nẵng'),
        (N'Trường AET Cơ sở Đồng Nai'),
        (N'Trường AET Cơ sở Cần Thơ');

    DECLARE @CampusTen NVARCHAR(255);
    DECLARE curCampus CURSOR LOCAL FOR SELECT ten FROM @Campuses;
    OPEN curCampus;
    FETCH NEXT FROM curCampus INTO @CampusTen;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM DonVi WHERE ten_don_vi = @CampusTen AND cap_don_vi = 'co_so')
            INSERT INTO DonVi (ten_don_vi, cap_don_vi, ma_don_vi_cha, con_hoat_dong, ngay_tao)
            VALUES (@CampusTen, 'co_so', @RootId, 1, @CurrentDate);
        FETCH NEXT FROM curCampus INTO @CampusTen;
    END
    CLOSE curCampus;
    DEALLOCATE curCampus;

    PRINT N'  [OK] DonVi: 1 root + 5 co so';

    -- ==========================================
    -- 2. CƠ SỞ VẬT CHẤT (Tòa nhà, Tầng, Phòng)
    -- Mỗi cơ sở: 2 tòa nhà, 3 tầng/tòa, 8 phòng/tầng = 48 phòng/cơ sở
    -- ==========================================
    DECLARE @CampusId INT;
    DECLARE @CampusTen2 NVARCHAR(255);
    DECLARE curCS CURSOR LOCAL FOR SELECT ma_don_vi, ten_don_vi FROM DonVi WHERE cap_don_vi = 'co_so';
    OPEN curCS;
    FETCH NEXT FROM curCS INTO @CampusId, @CampusTen2;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @b INT = 1;
        WHILE @b <= 2
        BEGIN
            DECLARE @BuildingCode NVARCHAR(50) = 'B' + CHAR(64 + @b) + '_' + CAST(@CampusId AS NVARCHAR);
            DECLARE @BuildingName NVARCHAR(100) = N'Tòa nhà ' + CHAR(64 + @b);
            DECLARE @BuildingId INT;

            IF NOT EXISTS (SELECT 1 FROM ToaNha WHERE ma_code_toa_nha = @BuildingCode)
            BEGIN
                INSERT INTO ToaNha (ma_don_vi, ten_toa_nha, ma_code_toa_nha, so_tang, con_hoat_dong)
                VALUES (@CampusId, @BuildingName, @BuildingCode, 3, 1);
                SET @BuildingId = SCOPE_IDENTITY();
            END
            ELSE
                SELECT @BuildingId = ma_toa_nha FROM ToaNha WHERE ma_code_toa_nha = @BuildingCode;

            DECLARE @f INT = 1;
            WHILE @f <= 3
            BEGIN
                DECLARE @FloorId INT;
                IF NOT EXISTS (SELECT 1 FROM Tang WHERE ma_toa_nha = @BuildingId AND thu_tu_tang = @f)
                BEGIN
                    INSERT INTO Tang (ma_toa_nha, ten_tang, thu_tu_tang, con_hoat_dong)
                    VALUES (@BuildingId, N'Tầng ' + CAST(@f AS NVARCHAR), @f, 1);
                    SET @FloorId = SCOPE_IDENTITY();
                END
                ELSE
                    SELECT @FloorId = ma_tang FROM Tang WHERE ma_toa_nha = @BuildingId AND thu_tu_tang = @f;

                DECLARE @r INT = 1;
                WHILE @r <= 8
                BEGIN
                    DECLARE @RoomCode NVARCHAR(50) = @BuildingCode + '_' + CAST(@f AS NVARCHAR) + '0' + CAST(@r AS NVARCHAR);
                    DECLARE @RoomName NVARCHAR(100) = CHAR(64 + @b) + CAST(@f AS NVARCHAR) + '0' + CAST(@r AS NVARCHAR);
                    IF NOT EXISTS (SELECT 1 FROM PhongHoc WHERE ma_code_phong = @RoomCode)
                        INSERT INTO PhongHoc (ma_tang, ma_don_vi, ten_phong, ma_code_phong, loai_phong, suc_chua, trang_thai_phong)
                        VALUES (@FloorId, @CampusId, N'Phòng ' + @RoomName, @RoomCode, 'ly_thuyet', 40, 'hoat_dong');
                    SET @r += 1;
                END
                SET @f += 1;
            END
            SET @b += 1;
        END

        -- Học kỳ & Block cho cơ sở
        DECLARE @HocKyCode NVARCHAR(50) = 'HK_FA26_CS' + CAST(@CampusId AS NVARCHAR);
        DECLARE @HocKyId INT;
        IF NOT EXISTS (SELECT 1 FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1)
        BEGIN
            INSERT INTO HocKy (ma_code_hoc_ky, ten_hoc_ky, ma_don_vi, nam_hoc, thu_tu_trong_nam, ngay_bat_dau, ngay_ket_thuc, so_tin_chi_toi_da)
            VALUES (@HocKyCode, N'Học kỳ Mùa Thu 2026', @CampusId, '2026', 1, '2026-09-01', '2026-12-31', 24);
            SET @HocKyId = SCOPE_IDENTITY();
        END
        ELSE
            SELECT @HocKyId = ma_hoc_ky FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1;

        IF NOT EXISTS (SELECT 1 FROM Block WHERE ma_hoc_ky = @HocKyId AND thu_tu_block = 1)
            INSERT INTO Block (ma_hoc_ky, ten_block, thu_tu_block, ngay_bat_dau, ngay_ket_thuc)
            VALUES (@HocKyId, N'Block 1', 1, '2026-09-01', '2026-10-31');

        IF NOT EXISTS (SELECT 1 FROM Block WHERE ma_hoc_ky = @HocKyId AND thu_tu_block = 2)
            INSERT INTO Block (ma_hoc_ky, ten_block, thu_tu_block, ngay_bat_dau, ngay_ket_thuc)
            VALUES (@HocKyId, N'Block 2', 2, '2026-11-01', '2026-12-31');

        FETCH NEXT FROM curCS INTO @CampusId, @CampusTen2;
    END
    CLOSE curCS;
    DEALLOCATE curCS;

    PRINT N'  [OK] ToaNha, Tang, PhongHoc, HocKy, Block da tao xong';

    -- ==========================================
    -- 3. HỌC THUẬT: Ngành, Chuyên ngành, Môn học, CTĐT
    -- ==========================================

    -- 3.1 Ngành đào tạo
    IF NOT EXISTS (SELECT 1 FROM NganhDaoTao WHERE ma_code_nganh = 'CNTT')
        INSERT INTO NganhDaoTao (ma_code_nganh, ten_nganh) VALUES ('CNTT', N'Công nghệ thông tin');
    IF NOT EXISTS (SELECT 1 FROM NganhDaoTao WHERE ma_code_nganh = 'TKDH')
        INSERT INTO NganhDaoTao (ma_code_nganh, ten_nganh) VALUES ('TKDH', N'Thiết kế Đồ họa');
    IF NOT EXISTS (SELECT 1 FROM NganhDaoTao WHERE ma_code_nganh = 'MKT')
        INSERT INTO NganhDaoTao (ma_code_nganh, ten_nganh) VALUES ('MKT', N'Digital Marketing');

    DECLARE @NganhCNTT INT, @NganhTKDH INT, @NganhMKT INT;
    SELECT @NganhCNTT = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'CNTT';
    SELECT @NganhTKDH = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'TKDH';
    SELECT @NganhMKT  = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'MKT';

    -- 3.2 Chuyên ngành
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Kỹ thuật phần mềm')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh) VALUES (@NganhCNTT, N'Kỹ thuật phần mềm');
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'An toàn thông tin')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh) VALUES (@NganhCNTT, N'An toàn thông tin');
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Trí tuệ nhân tạo')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh) VALUES (@NganhCNTT, N'Trí tuệ nhân tạo');
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Thiết kế đồ họa')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh) VALUES (@NganhTKDH, N'Thiết kế đồ họa');
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Digital Marketing')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh) VALUES (@NganhMKT, N'Digital Marketing');

    -- 3.3 Chuyên ngành theo cơ sở (tất cả co_so x tất cả chuyên ngành)
    INSERT INTO ChuyenNganhTheoCoSo (ma_don_vi, ma_chuyen_nganh, con_hoat_dong, trang_thai)
    SELECT d.ma_don_vi, c.ma_chuyen_nganh, 1, 'active'
    FROM DonVi d CROSS JOIN ChuyenNganh c
    WHERE d.cap_don_vi = 'co_so'
      AND NOT EXISTS (
          SELECT 1 FROM ChuyenNganhTheoCoSo cs
          WHERE cs.ma_don_vi = d.ma_don_vi AND cs.ma_chuyen_nganh = c.ma_chuyen_nganh
      );

    -- 3.4 Môn học
    IF NOT EXISTS (SELECT 1 FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101')
        INSERT INTO DanhMucMonHoc (ma_code_mon_hoc, ten_mon_hoc, so_tin_chi, con_hoat_dong)
        VALUES
            ('COM101', N'Nhập môn lập trình', 3, 1),
            ('DBI202', N'Hệ quản trị CSDL', 3, 1),
            ('WEB104', N'Thiết kế trang web', 3, 1),
            ('UIX101', N'Thiết kế UI/UX', 3, 1),
            ('MKT101', N'Marketing căn bản', 3, 1);

    -- 3.5 Khóa tuyển sinh & Quy đổi tín chỉ
    IF NOT EXISTS (SELECT 1 FROM KhoaTuyenSinh WHERE ma_code_khoa = 'K19')
        INSERT INTO KhoaTuyenSinh (ma_code_khoa, ten_khoa, nam_bat_dau, nam_ket_thuc_du_kien, con_hoat_dong)
        VALUES ('K19', N'Khóa 19', 2023, 2026, 1);
    IF NOT EXISTS (SELECT 1 FROM KhoaTuyenSinh WHERE ma_code_khoa = 'K20')
        INSERT INTO KhoaTuyenSinh (ma_code_khoa, ten_khoa, nam_bat_dau, nam_ket_thuc_du_kien, con_hoat_dong)
        VALUES ('K20', N'Khóa 20', 2024, 2027, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 3)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (3, 1, 2, 1);

    -- 3.6 Chương trình đào tạo mẫu (CNTT - Kỹ thuật phần mềm - K20)
    DECLARE @ChuyenNganhSE INT, @K20 INT, @ChuongTrinhId INT;
    SELECT @ChuyenNganhSE = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Kỹ thuật phần mềm';
    SELECT @K20 = ma_khoa_tuyen_sinh FROM KhoaTuyenSinh WHERE ma_code_khoa = 'K20';

    IF @ChuyenNganhSE IS NOT NULL AND @K20 IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_SE_K20')
        BEGIN
            INSERT INTO ChuongTrinhDaoTao (ma_code_chuong_trinh, ten_chuong_trinh, ma_chuyen_nganh, ma_khoa_tuyen_sinh, so_hoc_ky, thoi_gian_dao_tao_thang, tong_tin_chi_yeu_cau, version, trang_thai)
            VALUES ('CTDT_SE_K20', N'Chương trình Kỹ thuật phần mềm K20', @ChuyenNganhSE, @K20, 7, 28, 120, '1.0', 'active');
            SET @ChuongTrinhId = SCOPE_IDENTITY();
        END
        ELSE
            SELECT @ChuongTrinhId = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_SE_K20';

        DECLARE @MonCOM101 INT, @MonDBI202 INT;
        SELECT @MonCOM101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101';
        SELECT @MonDBI202 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'DBI202';

        IF @MonCOM101 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @ChuongTrinhId AND ma_mon_hoc = @MonCOM101)
            INSERT INTO MonHocTrongChuongTrinh (ma_chuong_trinh, ma_mon_hoc, hoc_ky_du_kien, loai_mon_hoc, so_tin_chi, bat_buoc, con_hoat_dong)
            VALUES (@ChuongTrinhId, @MonCOM101, 1, 'bat_buoc', 3, 1, 1);

        IF @MonDBI202 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @ChuongTrinhId AND ma_mon_hoc = @MonDBI202)
            INSERT INTO MonHocTrongChuongTrinh (ma_chuong_trinh, ma_mon_hoc, hoc_ky_du_kien, loai_mon_hoc, so_tin_chi, bat_buoc, con_hoat_dong)
            VALUES (@ChuongTrinhId, @MonDBI202, 2, 'bat_buoc', 3, 1, 1);

        IF @MonCOM101 IS NOT NULL AND @MonDBI202 IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM MonHocTienQuyet WHERE ma_mon_hoc = @MonDBI202 AND ma_mon_tien_quyet = @MonCOM101)
            INSERT INTO MonHocTienQuyet (ma_mon_hoc, ma_mon_tien_quyet) VALUES (@MonDBI202, @MonCOM101);
    END

    PRINT N'  [OK] Nganh, ChuyenNganh, MonHoc, KhoaTuyenSinh, ChuongTrinhDaoTao da tao xong';

    -- ==========================================
    -- 4. GẮN THIẾT BỊ CHO CÁC PHÒNG HỌC
    -- ==========================================
    INSERT INTO ThietBiPhong (ma_phong, ten_thiet_bi, so_luong, tinh_trang)
    SELECT p.ma_phong, N'Máy chiếu', 1, N'Bình thường'
    FROM PhongHoc p
    WHERE NOT EXISTS (SELECT 1 FROM ThietBiPhong t WHERE t.ma_phong = p.ma_phong AND t.ten_thiet_bi = N'Máy chiếu');

    PRINT N'  [OK] ThietBiPhong da tao xong';

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH PHASE 1 THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI TRONG PHASE 1 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
