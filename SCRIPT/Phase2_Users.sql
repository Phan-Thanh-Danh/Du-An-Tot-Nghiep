USE LMS;
GO

SET NOCOUNT ON;

DECLARE @N INT = 5; -- Số cơ sở
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @DefaultPasswordHash NVARCHAR(MAX) = 'PBKDF2.100000.E08Uerno/mWBsiCCQpVZuQ==.H6STe78bQZXHynCV5JXHIBve2jSLcXJt1/INXAUWF/4='; -- Hash cho '123456'

PRINT N'--- BẮT ĐẦU PHASE 2: TẠO HÀNG VẠN TÀI KHOẢN (MẬT KHẨU: 123456) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 0. SEED BẢNG VAI TRÒ (VaiTro) - chạy 1 lần
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE ma_code_vai_tro = 'sieu_quan_tri')
    BEGIN
        INSERT INTO VaiTro (ma_vai_tro, ma_code_vai_tro, ten_vai_tro) VALUES
            (1,  'sieu_quan_tri',           N'Siêu quản trị'),
            (2,  'quan_tri',                N'Quản trị'),
            (3,  'quan_tri_co_so',          N'Quản trị cơ sở'),
            (4,  'quan_tri_co_so_con',      N'Quản trị cơ sở con'),
            (5,  'hieu_truong',             N'Hiệu trưởng'),
            (6,  'chu_tich',               N'Chủ tịch hội đồng'),
            (7,  'giao_vien',              N'Giảng viên'),
            (8,  'nhan_vien',              N'Nhân viên giáo vụ'),
            (9,  'hoc_sinh',               N'Sinh viên'),
            (10, 'phu_huynh',              N'Phụ huynh'),
            (11, 'hoidong_quanly_noidung', N'Hội đồng quản lý nội dung'),
            (12, 'admin_tai_chinh',        N'Admin tài chính'),
            (13, 'ke_toan_co_so',          N'Kế toán cơ sở'),
            (14, 'ke_toan_truong_co_so',   N'Kế toán trưởng cơ sở');
    END

    DECLARE @i INT = 1;
    WHILE @i <= @N
    BEGIN
        DECLARE @CampusCode NVARCHAR(50) = 'CAMPUS_AET_' + CAST(@i AS NVARCHAR);
        DECLARE @CampusId INT;
        SELECT @CampusId = ma_don_vi FROM DonVi WHERE ten_don_vi = N'Trường AET Cơ sở ' + CAST(@i AS NVARCHAR) AND cap_don_vi = 'co_so';

        IF @CampusId IS NOT NULL
        BEGIN
            PRINT N'Đang tạo dữ liệu cho: ' + @CampusCode;

            -- Tạo Bảng số ảo (Numbers Table) 1 đến 3000 để Batch Insert siêu tốc
            -- master.dbo.spt_values chứa khoảng 2500 dòng, CROSS JOIN tạo ra 6 triệu dòng.
            
            -- ==========================================
            -- 1. TẠO 1000 GIẢNG VIÊN (Teacher)
            -- ==========================================
            PRINT N'  - Tạo 1000 Giảng viên...';
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            SELECT TOP 1000
                'gv' + CAST(N AS NVARCHAR) + '.c' + CAST(@i AS NVARCHAR) + '@aet.local',
                N'Giảng viên ' + CAST(N AS NVARCHAR) + N' - Cơ sở ' + CAST(@i AS NVARCHAR),
                @CampusId,
                @DefaultPasswordHash,
                'giao_vien',
                'hoat_dong',
                0,
                @CurrentDate
            FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums;

            -- ==========================================
            -- 2. TẠO LỚP HÀNH CHÍNH (85 Lớp)
            -- ==========================================
            PRINT N'  - Tạo 85 Lớp Hành chính...';
            -- Lấy 85 giảng viên ngẫu nhiên vừa tạo để làm GVCN
            DECLARE @GVCN TABLE (id INT, row_num INT);
            INSERT INTO @GVCN (id, row_num)
            SELECT TOP 85 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung) 
            FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'giao_vien' ORDER BY NEWID();

            DECLARE @ChuongTrinhId INT;
            SELECT TOP 1 @ChuongTrinhId = ma_chuong_trinh FROM ChuongTrinhDaoTao;

            INSERT INTO LopHanhChinh (ma_code_lop, ten_lop, ma_don_vi, ma_chuong_trinh, ma_giao_vien_chu_nhiem, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
            SELECT 
                'SE19' + RIGHT('00' + CAST(N AS NVARCHAR), 2) + '_C' + CAST(@i AS NVARCHAR),
                N'Lớp SE K19 - ' + CAST(N AS NVARCHAR),
                @CampusId,
                @ChuongTrinhId,
                g.id,
                2024,
                35,
                1
            FROM (SELECT TOP 85 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values) AS Nums
            JOIN @GVCN g ON Nums.N = g.row_num;

            DELETE FROM @GVCN;

            -- ==========================================
            -- 3. TẠO 3000 SINH VIÊN (Student) & Phân bổ vào Lớp
            -- ==========================================
            PRINT N'  - Tạo 3000 Sinh viên...';
            -- Lấy danh sách Lớp hành chính của cơ sở này
            DECLARE @Classes TABLE (id INT, row_num INT);
            INSERT INTO @Classes (id, row_num)
            SELECT ma_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) FROM LopHanhChinh WHERE ma_don_vi = @CampusId;
            
            -- Chia 3000 sinh viên vào 85 lớp (khoảng 35 sv/lớp) bằng công thức (N % 85) + 1
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, ma_lop, mat_khau_hash, vai_tro_chinh, nam_nhap_hoc, trang_thai, dang_nhap_lan_dau, ngay_tao)
            SELECT TOP 3000
                'sv' + CAST(N AS NVARCHAR) + '.c' + CAST(@i AS NVARCHAR) + '@aet.local',
                N'Sinh viên ' + CAST(N AS NVARCHAR) + N' - Cơ sở ' + CAST(@i AS NVARCHAR),
                @CampusId,
                c.id, -- Phân bổ lớp
                @DefaultPasswordHash,
                'hoc_sinh',
                2024,
                'hoat_dong',
                0,
                @CurrentDate
            FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums
            JOIN @Classes c ON c.row_num = ((Nums.N - 1) % 85) + 1;

            DELETE FROM @Classes;

            -- ==========================================
            -- 4. TẠO CÁC VAI TRÒ KHÁC (AcademicStaff, CampusAdmin)
            -- ==========================================
            PRINT N'  - Tạo Giáo vụ & Admin...';
            -- 10 Giáo vụ
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            SELECT TOP 10
                'giaovu' + CAST(N AS NVARCHAR) + '.c' + CAST(@i AS NVARCHAR) + '@aet.local',
                N'Giáo vụ ' + CAST(N AS NVARCHAR),
                @CampusId,
                @DefaultPasswordHash,
                'nhan_vien',
                'hoat_dong',
                0,
                @CurrentDate
            FROM (SELECT TOP 10 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values) AS Nums;

            -- 1 Admin Cơ sở
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            VALUES (
                'admin.c' + CAST(@i AS NVARCHAR) + '@aet.local',
                N'Admin Cơ sở ' + CAST(@i AS NVARCHAR),
                @CampusId,
                @DefaultPasswordHash,
                'quan_tri_co_so',
                'hoat_dong',
                0,
                @CurrentDate
            );

            -- ==========================================
            -- 5. TẠO PHỤ HUYNH (2000 Phụ huynh) & Liên kết
            -- ==========================================
            PRINT N'  - Tạo 2000 Phụ huynh & Liên kết Sinh viên...';
            
            -- Lấy 2000 sinh viên đầu tiên của cơ sở này
            DECLARE @Students TABLE (id INT, row_num INT);
            INSERT INTO @Students (id, row_num)
            SELECT TOP 2000 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung) 
            FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'hoc_sinh';

            -- Insert 2000 Phụ huynh (Lưu lại ID vừa sinh bằng OUTPUT để liên kết)
            -- Vì SQL Server Insert Output hơi phức tạp với bảng tạm, ta insert theo Batch rồi join theo Email
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            SELECT TOP 2000
                'phuhuynh' + CAST(N AS NVARCHAR) + '.c' + CAST(@i AS NVARCHAR) + '@aet.local',
                N'Phụ huynh ' + CAST(N AS NVARCHAR) + N' - Cơ sở ' + CAST(@i AS NVARCHAR),
                @CampusId,
                @DefaultPasswordHash,
                'phu_huynh',
                'hoat_dong',
                0,
                @CurrentDate
            FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums;

            -- Lấy lại 2000 Phụ huynh vừa Insert
            DECLARE @Parents TABLE (id INT, row_num INT);
            INSERT INTO @Parents (id, row_num)
            SELECT TOP 2000 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung) 
            FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'phu_huynh' ORDER BY ma_nguoi_dung DESC;

            -- Tạo Liên kết Phụ huynh - Sinh viên
            INSERT INTO LienKetPhuHuynh (ma_phu_huynh, ma_hoc_sinh, quyen_xem, trang_thai)
            SELECT p.id, s.id, '["Diem", "DiemDanh", "HocPhi"]', 'hoat_dong'
            FROM @Parents p
            JOIN @Students s ON p.row_num = s.row_num;

            DELETE FROM @Students;
            DELETE FROM @Parents;

            -- ==========================================
            -- 6. GÁN VAI TRÒ VÀO BẢNG PhanQuyenNguoiDung
            -- ==========================================
            PRINT N'  - Gán vai trò vào PhanQuyenNguoiDung...';
            -- Gán theo vai_tro_chinh hiện có trong NguoiDung, map sang ma_vai_tro trong VaiTro
            INSERT INTO PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro)
            SELECT nd.ma_nguoi_dung, vt.ma_vai_tro
            FROM NguoiDung nd
            JOIN VaiTro vt ON vt.ma_code_vai_tro = nd.vai_tro_chinh
            WHERE nd.ma_don_vi = @CampusId
              AND NOT EXISTS (
                  SELECT 1 FROM PhanQuyenNguoiDung pq
                  WHERE pq.ma_nguoi_dung = nd.ma_nguoi_dung AND pq.ma_vai_tro = vt.ma_vai_tro
              );

        END

        SET @i += 1;
    END

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH TOÀN BỘ PHASE 2 THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA TRONG PHASE 2 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO

