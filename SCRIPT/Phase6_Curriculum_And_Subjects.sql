USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'======================================================================';
PRINT N'--- BẮT ĐẦU PHASE 6: BỔ SUNG TOÀN DIỆN MÔN HỌC & KHUNG CHƯƠNG TRÌNH ĐÀO TẠO ---';
PRINT N'======================================================================';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==================================================================
    -- 1. LẤY ID CÁC NGÀNH VÀ CHUYÊN NGÀNH
    -- ==================================================================
    DECLARE @NganhCNTT INT, @NganhTKDH INT, @NganhMKT INT;
    SELECT @NganhCNTT = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'CNTT';
    SELECT @NganhTKDH = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'TKDH';
    SELECT @NganhMKT  = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'MKT';

    DECLARE @ChuyenNganhSE INT, @ChuyenNganhGD INT, @ChuyenNganhDM INT, @ChuyenNganhIA INT, @ChuyenNganhAI INT;
    SELECT @ChuyenNganhSE = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Kỹ thuật phần mềm';
    SELECT @ChuyenNganhGD = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Thiết kế đồ họa';
    SELECT @ChuyenNganhDM = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Digital Marketing';
    SELECT @ChuyenNganhIA = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'An toàn thông tin';
    SELECT @ChuyenNganhAI = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Trí tuệ nhân tạo';

    -- ==================================================================
    -- 1.1 BỔ SUNG BẢNG QUY ĐỔI TÍN CHỈ (QuyDoiTinChi)
    -- ==================================================================
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 2)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (2, 1, 2, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 3)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (3, 1, 3, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 4)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (4, 2, 2, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 5)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (5, 2, 3, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 9)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (9, 2, 4, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 12)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (12, 2, 4, 1);

    -- ==================================================================
    -- 2. NẠP TOÀN BỘ DANH MỤC MÔN HỌC (DanhMucMonHoc)
    -- ==================================================================
    PRINT N'- 1. Đang nạp danh mục môn học chuẩn hóa...';

    DECLARE @Subjects TABLE (
        code NVARCHAR(50), 
        ten NVARCHAR(255), 
        tc INT, 
        nganh INT, 
        chuyenNganh INT
    );

    INSERT INTO @Subjects VALUES
        -- Môn đại cương & Kỹ năng chung
        ('ENG101', N'Tiếng Anh căn bản 1', 3, NULL, NULL),
        ('ENG102', N'Tiếng Anh chuyên ngành 1', 3, NULL, NULL),
        ('ENG103', N'Tiếng Anh chuyên ngành 2', 3, NULL, NULL),
        ('SSG101', N'Kỹ năng học tập & Làm việc nhóm', 3, NULL, NULL),
        ('ENT101', N'Khởi nghiệp đổi mới sáng tạo', 3, NULL, NULL),
        ('ETH301', N'Đạo đức nghề nghiệp & Pháp luật', 3, NULL, NULL),

        -- Khối Kỹ thuật phần mềm (SE) & CNTT
        ('COM101', N'Nhập môn lập trình', 3, @NganhCNTT, @ChuyenNganhSE),
        ('MAT101', N'Toán rời rạc & Đại số tuyến tính', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRF192', N'Kỹ thuật lập trình C/C++', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CEA201', N'Kiến trúc máy tính & Hệ điều hành', 3, @NganhCNTT, @ChuyenNganhSE),
        ('DBI202', N'Hệ quản trị CSDL & SQL Server', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WEB104', N'Thiết kế trang web (HTML5/CSS3/JS)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRO192', N'Lập trình hướng đối tượng với Java', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CSD201', N'Cấu trúc dữ liệu & Giải thuật', 3, @NganhCNTT, @ChuyenNganhSE),
        ('NWC203', N'Mạng máy tính căn bản', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WED201', N'Lập trình Frontend nâng cao (Vue.js/React)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRN211', N'Lập trình ứng dụng với C# .NET', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWP391', N'Dự án phần mềm thực chiến 1', 3, @NganhCNTT, @ChuyenNganhSE),
        ('MAS291', N'Xác suất thống kê cho CNTT', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWE201', N'Nhập môn Kỹ thuật phần mềm', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRN231', N'Xây dựng RESTful API với ASP.NET Core', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWT301', N'Kiểm thử phần mềm & Đảm bảo chất lượng (QA/QC)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRM392', N'Lập trình ứng dụng di động (Mobile App)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWR302', N'Phân tích thiết kế hệ thống & Yêu cầu phần mềm', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PMG201', N'Quản lý dự án CNTT (Agile / Scrum)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('IOT102', N'Nhập môn Internet vạn vật (IoT)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WDU301', N'Điện toán đám mây & DevOps (Docker, CI/CD)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('AIL302', N'Trí tuệ nhân tạo & Machine Learning căn bản', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWP490', N'Dự án phần mềm thực chiến 2', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SEC301', N'An toàn và Bảo mật thông tin ứng dụng', 3, @NganhCNTT, @ChuyenNganhSE),
        ('DBS301', N'Cơ sở dữ liệu NoSQL & Big Data', 3, @NganhCNTT, @ChuyenNganhSE),
        ('OJT401', N'Thực tập tốt nghiệp doanh nghiệp CNTT (OJT)', 9, @NganhCNTT, @ChuyenNganhSE),
        ('SEM401', N'Hội thảo chuyên đề công nghệ mới', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CAP499', N'Đồ án tốt nghiệp Kỹ thuật phần mềm', 12, @NganhCNTT, @ChuyenNganhSE),
        ('SRE401', N'Vận hành hệ thống phần mềm & SRE', 3, @NganhCNTT, @ChuyenNganhSE),

        -- Khối Thiết kế Đồ họa (GD)
        ('UIX101', N'Thiết kế UI/UX căn bản', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ART101', N'Mỹ thuật căn bản & Hình họa', 3, @NganhTKDH, @ChuyenNganhGD),
        ('COL101', N'Lý thuyết màu sắc & Bố cục thị giác', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PSH101', N'Xử lý ảnh kỹ thuật số (Photoshop)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ILL101', N'Thiết kế đồ họa vector (Illustrator)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('TYP101', N'Nghệ thuật chữ trong thiết kế (Typography)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('IND101', N'Dàn trang & Xuất bản điện tử (InDesign)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('FDM101', N'Nhiếp ảnh & Xử lý ánh sáng studio', 3, @NganhTKDH, @ChuyenNganhGD),
        ('BRD201', N'Thiết kế hệ thống nhận diện thương hiệu (Branding)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PKG201', N'Thiết kế bao bì & Nhãn hiệu sản phẩm', 3, @NganhTKDH, @ChuyenNganhGD),
        ('AFX201', N'Đồ họa chuyển động 2D (After Effects)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('UIX201', N'Thiết kế trải nghiệm người dùng nâng cao (Figma)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PRD201', N'Kỹ thuật in ấn và vật liệu mỹ thuật', 3, @NganhTKDH, @ChuyenNganhGD),
        ('MAX201', N'Đồ họa không gian 3D căn bản (Blender)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('VFX201', N'Kỹ xảo hình ảnh & Dựng video (Premiere Pro)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GDP391', N'Đồ án thiết kế đồ họa thực chiến 1', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ILL202', N'Minh họa kỹ thuật số (Digital Painting)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ADR201', N'Nghệ thuật chỉ đạo hình ảnh (Art Direction)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('MAX301', N'Diễn hoạt & Tạo hình nhân vật 3D nâng cao', 3, @NganhTKDH, @ChuyenNganhGD),
        ('UIX301', N'Thiết kế Design System cho ứng dụng đa nền tảng', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GDP491', N'Đồ án thiết kế đồ họa thực chiến 2', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GMD201', N'Thiết kế mỹ thuật game (Game Art UI/Asset)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('CPY201', N'Sáng tạo nội dung thị giác (Visual Copywriting)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('OJT402', N'Thực tập tốt nghiệp doanh nghiệp thiết kế (OJT)', 9, @NganhTKDH, @ChuyenNganhGD),
        ('POR401', N'Xây dựng Portfolio chuyên nghiệp & Personal Branding', 3, @NganhTKDH, @ChuyenNganhGD),
        ('CAP498', N'Đồ án tốt nghiệp Thiết kế đồ họa', 12, @NganhTKDH, @ChuyenNganhGD),
        ('EXH401', N'Tổ chức triển lãm & Trình bày đồ án', 3, @NganhTKDH, @ChuyenNganhGD),

        -- Khối Digital Marketing (DM)
        ('MKT101', N'Marketing căn bản', 3, @NganhMKT, @ChuyenNganhDM),
        ('ECO101', N'Kinh tế vi mô & Hành vi người tiêu dùng', 3, @NganhMKT, @ChuyenNganhDM),
        ('ICT101', N'Công nghệ thông tin ứng dụng trong kinh doanh', 3, @NganhMKT, @ChuyenNganhDM),
        ('STA101', N'Thống kê kinh doanh căn bản', 3, @NganhMKT, @ChuyenNganhDM),
        ('MKT201', N'Nhập môn Digital Marketing & E-Commerce', 3, @NganhMKT, @ChuyenNganhDM),
        ('CPY101', N'Sáng tạo nội dung tiếp thị (Content Marketing)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SEO101', N'Tối ưu hóa công cụ tìm kiếm (SEO căn bản)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SEM201', N'Quảng cáo tìm kiếm Google Ads (SEM)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SMM201', N'Tiếp thị trên mạng xã hội (Social Media Marketing)', 3, @NganhMKT, @ChuyenNganhDM),
        ('EMA201', N'Tiếp thị tự động qua Email (Email Marketing Automation)', 3, @NganhMKT, @ChuyenNganhDM),
        ('VID201', N'Sản xuất video ngắn truyền thông (Short-form Video)', 3, @NganhMKT, @ChuyenNganhDM),
        ('MRK202', N'Nghiên cứu thị trường & Phân tích đối thủ', 3, @NganhMKT, @ChuyenNganhDM),
        ('MKA301', N'Phân tích dữ liệu tiếp thị (Google Analytics 4 / PowerBI)', 3, @NganhMKT, @ChuyenNganhDM),
        ('PPC301', N'Quảng cáo đa kênh trả phí (Meta Ads / TikTok Ads)', 3, @NganhMKT, @ChuyenNganhDM),
        ('DMP391', N'Dự án Digital Marketing thực chiến 1', 3, @NganhMKT, @ChuyenNganhDM),
        ('ECOM301', N'Quản trị vận hành sàn TMĐT (Shopee, TikTok Shop)', 3, @NganhMKT, @ChuyenNganhDM),
        ('CRM201', N'Quản trị quan hệ khách hàng (CRM & Chăm sóc số)', 3, @NganhMKT, @ChuyenNganhDM),
        ('PRM201', N'Truyền thông & Quan hệ công chúng trực tuyến (Online PR)', 3, @NganhMKT, @ChuyenNganhDM),
        ('GRH301', N'Tăng trưởng kinh doanh số (Growth Hacking & Phễu chuyển đổi)', 3, @NganhMKT, @ChuyenNganhDM),
        ('AIK301', N'Ứng dụng AI trong sáng tạo & Tối ưu hóa Marketing', 3, @NganhMKT, @ChuyenNganhDM),
        ('DMP491', N'Dự án Digital Marketing thực chiến 2', 3, @NganhMKT, @ChuyenNganhDM),
        ('INF201', N'Quản lý chiến dịch Influencer & KOL/KOC Marketing', 3, @NganhMKT, @ChuyenNganhDM),
        ('LAW201', N'Pháp luật trong thương mại điện tử & Bản quyền số', 3, @NganhMKT, @ChuyenNganhDM),
        ('OJT403', N'Thực tập tốt nghiệp doanh nghiệp Marketing (OJT)', 9, @NganhMKT, @ChuyenNganhDM),
        ('STR401', N'Hoạch định chiến lược Digital Marketing tổng thể', 3, @NganhMKT, @ChuyenNganhDM),
        ('CAP497', N'Đồ án tốt nghiệp Chiến lược Digital Marketing', 12, @NganhMKT, @ChuyenNganhDM),
        ('BDM401', N'Quản trị ngân sách & Đo lường hiệu quả ROI tiếp thị', 3, @NganhMKT, @ChuyenNganhDM);

    -- Nạp vào DanhMucMonHoc nếu chưa tồn tại
    INSERT INTO DanhMucMonHoc (ma_code_mon_hoc, ten_mon_hoc, so_tin_chi, con_hoat_dong, ma_nganh, ma_chuyen_nganh)
    SELECT s.code, s.ten, s.tc, 1, s.nganh, s.chuyenNganh
    FROM @Subjects s
    WHERE NOT EXISTS (SELECT 1 FROM DanhMucMonHoc m WHERE m.ma_code_mon_hoc = s.code);

    -- Cập nhật lại số tín chỉ & ngành nếu đã có sẵn
    UPDATE m
    SET m.ten_mon_hoc = s.ten,
        m.so_tin_chi = s.tc,
        m.ma_nganh = ISNULL(s.nganh, m.ma_nganh),
        m.ma_chuyen_nganh = ISNULL(s.chuyenNganh, m.ma_chuyen_nganh),
        m.con_hoat_dong = 1
    FROM DanhMucMonHoc m
    JOIN @Subjects s ON m.ma_code_mon_hoc = s.code;

    PRINT N'  [OK] Đã hoàn tất danh mục ' + CAST((SELECT COUNT(*) FROM DanhMucMonHoc) AS NVARCHAR) + N' môn học!';

    -- ==================================================================
    -- 2.1 LIÊN KẾT MÔN HỌC - CHUYÊN NGÀNH (MonHocChuyenNganh)
    -- ==================================================================
    PRINT N'- 1.1 Đang liên kết môn học vào MonHocChuyenNganh...';

    -- Môn chuyên ngành SE
    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhSE
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhSE OR m.ma_nganh = @NganhCNTT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhSE);

    -- Môn chuyên ngành GD
    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhGD
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhGD OR m.ma_nganh = @NganhTKDH OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301', 'WEB104'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhGD);

    -- Môn chuyên ngành DM
    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhDM
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhDM OR m.ma_nganh = @NganhMKT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301', 'WEB104', 'PSH101'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhDM);

    PRINT N'  [OK] Đã liên kết MonHocChuyenNganh!';

    -- ==================================================================
    -- 3. GẮN MÔN HỌC VÀO TỪNG CHƯƠNG TRÌNH ĐÀO TẠO (120 TC / 7 HỌC KỲ)
    -- ==================================================================
    PRINT N'- 2. Đang cấu trúc 7 học kỳ và 120 tín chỉ cho các CTĐT...';

    DECLARE @CtdtSE INT, @CtdtGD INT, @CtdtDM INT;
    SELECT @CtdtSE = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_SE_K20';
    SELECT @CtdtGD = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_GD_K20';
    SELECT @CtdtDM = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_DM_K20';

    -- Bảng tạm chứa cấu hình môn theo từng CTĐT và từng kỳ
    DECLARE @ProgramCurriculum TABLE (
        ctdtId INT,
        monCode NVARCHAR(50),
        hocKy INT,
        loai NVARCHAR(30),
        thuTu INT
    );

    -- 3.1 CẤU HÌNH CTĐT KỸ THUẬT PHẦN MỀM (SE) - 120 TÍN CHỈ (7 KỲ)
    INSERT INTO @ProgramCurriculum VALUES
        -- Kỳ 1 (18 TC)
        (@CtdtSE, 'COM101', 1, 'bat_buoc', 1),
        (@CtdtSE, 'ENG101', 1, 'bat_buoc', 2),
        (@CtdtSE, 'MAT101', 1, 'bat_buoc', 3),
        (@CtdtSE, 'PRF192', 1, 'bat_buoc', 4),
        (@CtdtSE, 'SSG101', 1, 'bat_buoc', 5),
        (@CtdtSE, 'CEA201', 1, 'bat_buoc', 6),
        -- Kỳ 2 (18 TC)
        (@CtdtSE, 'DBI202', 2, 'bat_buoc', 1),
        (@CtdtSE, 'WEB104', 2, 'bat_buoc', 2),
        (@CtdtSE, 'ENG102', 2, 'bat_buoc', 3),
        (@CtdtSE, 'PRO192', 2, 'bat_buoc', 4),
        (@CtdtSE, 'CSD201', 2, 'bat_buoc', 5),
        (@CtdtSE, 'NWC203', 2, 'bat_buoc', 6),
        -- Kỳ 3 (18 TC)
        (@CtdtSE, 'WED201', 3, 'bat_buoc', 1),
        (@CtdtSE, 'PRN211', 3, 'bat_buoc', 2),
        (@CtdtSE, 'SWP391', 3, 'bat_buoc', 3),
        (@CtdtSE, 'MAS291', 3, 'bat_buoc', 4),
        (@CtdtSE, 'SWE201', 3, 'bat_buoc', 5),
        (@CtdtSE, 'ENG103', 3, 'bat_buoc', 6),
        -- Kỳ 4 (18 TC)
        (@CtdtSE, 'PRN231', 4, 'bat_buoc', 1),
        (@CtdtSE, 'SWT301', 4, 'bat_buoc', 2),
        (@CtdtSE, 'PRM392', 4, 'bat_buoc', 3),
        (@CtdtSE, 'SWR302', 4, 'bat_buoc', 4),
        (@CtdtSE, 'PMG201', 4, 'bat_buoc', 5),
        (@CtdtSE, 'IOT102', 4, 'tu_chon',  6),
        -- Kỳ 5 (18 TC)
        (@CtdtSE, 'WDU301', 5, 'bat_buoc', 1),
        (@CtdtSE, 'AIL302', 5, 'bat_buoc', 2),
        (@CtdtSE, 'SWP490', 5, 'bat_buoc', 3),
        (@CtdtSE, 'SEC301', 5, 'bat_buoc', 4),
        (@CtdtSE, 'DBS301', 5, 'tu_chon',  5),
        (@CtdtSE, 'ENT101', 5, 'bat_buoc', 6),
        -- Kỳ 6 (15 TC)
        (@CtdtSE, 'OJT401', 6, 'bat_buoc', 1),
        (@CtdtSE, 'SEM401', 6, 'bat_buoc', 2),
        (@CtdtSE, 'ETH301', 6, 'bat_buoc', 3),
        -- Kỳ 7 (15 TC)
        (@CtdtSE, 'CAP499', 7, 'bat_buoc', 1),
        (@CtdtSE, 'SRE401', 7, 'bat_buoc', 2);

    -- 3.2 CẤU HÌNH CTĐT THIẾT KẾ ĐỒ HỌA (GD) - 120 TÍN CHỈ (7 KỲ)
    INSERT INTO @ProgramCurriculum VALUES
        -- Kỳ 1 (18 TC)
        (@CtdtGD, 'UIX101', 1, 'bat_buoc', 1),
        (@CtdtGD, 'ART101', 1, 'bat_buoc', 2),
        (@CtdtGD, 'COL101', 1, 'bat_buoc', 3),
        (@CtdtGD, 'PSH101', 1, 'bat_buoc', 4),
        (@CtdtGD, 'ENG101', 1, 'bat_buoc', 5),
        (@CtdtGD, 'SSG101', 1, 'bat_buoc', 6),
        -- Kỳ 2 (18 TC)
        (@CtdtGD, 'ILL101', 2, 'bat_buoc', 1),
        (@CtdtGD, 'TYP101', 2, 'bat_buoc', 2),
        (@CtdtGD, 'IND101', 2, 'bat_buoc', 3),
        (@CtdtGD, 'FDM101', 2, 'bat_buoc', 4),
        (@CtdtGD, 'ENG102', 2, 'bat_buoc', 5),
        (@CtdtGD, 'WEB104', 2, 'bat_buoc', 6),
        -- Kỳ 3 (18 TC)
        (@CtdtGD, 'BRD201', 3, 'bat_buoc', 1),
        (@CtdtGD, 'PKG201', 3, 'bat_buoc', 2),
        (@CtdtGD, 'AFX201', 3, 'bat_buoc', 3),
        (@CtdtGD, 'UIX201', 3, 'bat_buoc', 4),
        (@CtdtGD, 'PRD201', 3, 'bat_buoc', 5),
        (@CtdtGD, 'ENG103', 3, 'bat_buoc', 6),
        -- Kỳ 4 (18 TC)
        (@CtdtGD, 'MAX201', 4, 'bat_buoc', 1),
        (@CtdtGD, 'VFX201', 4, 'bat_buoc', 2),
        (@CtdtGD, 'GDP391', 4, 'bat_buoc', 3),
        (@CtdtGD, 'MKT101', 4, 'bat_buoc', 4),
        (@CtdtGD, 'ILL202', 4, 'tu_chon',  5),
        (@CtdtGD, 'ADR201', 4, 'bat_buoc', 6),
        -- Kỳ 5 (18 TC)
        (@CtdtGD, 'MAX301', 5, 'bat_buoc', 1),
        (@CtdtGD, 'UIX301', 5, 'bat_buoc', 2),
        (@CtdtGD, 'GDP491', 5, 'bat_buoc', 3),
        (@CtdtGD, 'GMD201', 5, 'tu_chon',  4),
        (@CtdtGD, 'CPY201', 5, 'bat_buoc', 5),
        (@CtdtGD, 'ENT101', 5, 'bat_buoc', 6),
        -- Kỳ 6 (15 TC)
        (@CtdtGD, 'OJT402', 6, 'bat_buoc', 1),
        (@CtdtGD, 'POR401', 6, 'bat_buoc', 2),
        (@CtdtGD, 'ETH301', 6, 'bat_buoc', 3),
        -- Kỳ 7 (15 TC)
        (@CtdtGD, 'CAP498', 7, 'bat_buoc', 1),
        (@CtdtGD, 'EXH401', 7, 'bat_buoc', 2);

    -- 3.3 CẤU HÌNH CTĐT DIGITAL MARKETING (DM) - 120 TÍN CHỈ (7 KỲ)
    INSERT INTO @ProgramCurriculum VALUES
        -- Kỳ 1 (18 TC)
        (@CtdtDM, 'MKT101', 1, 'bat_buoc', 1),
        (@CtdtDM, 'ECO101', 1, 'bat_buoc', 2),
        (@CtdtDM, 'ICT101', 1, 'bat_buoc', 3),
        (@CtdtDM, 'STA101', 1, 'bat_buoc', 4),
        (@CtdtDM, 'ENG101', 1, 'bat_buoc', 5),
        (@CtdtDM, 'SSG101', 1, 'bat_buoc', 6),
        -- Kỳ 2 (18 TC)
        (@CtdtDM, 'MKT201', 2, 'bat_buoc', 1),
        (@CtdtDM, 'CPY101', 2, 'bat_buoc', 2),
        (@CtdtDM, 'SEO101', 2, 'bat_buoc', 3),
        (@CtdtDM, 'PSH101', 2, 'bat_buoc', 4),
        (@CtdtDM, 'ENG102', 2, 'bat_buoc', 5),
        (@CtdtDM, 'WEB104', 2, 'bat_buoc', 6),
        -- Kỳ 3 (18 TC)
        (@CtdtDM, 'SEM201', 3, 'bat_buoc', 1),
        (@CtdtDM, 'SMM201', 3, 'bat_buoc', 2),
        (@CtdtDM, 'EMA201', 3, 'bat_buoc', 3),
        (@CtdtDM, 'VID201', 3, 'bat_buoc', 4),
        (@CtdtDM, 'MRK202', 3, 'bat_buoc', 5),
        (@CtdtDM, 'ENG103', 3, 'bat_buoc', 6),
        -- Kỳ 4 (18 TC)
        (@CtdtDM, 'MKA301', 4, 'bat_buoc', 1),
        (@CtdtDM, 'PPC301', 4, 'bat_buoc', 2),
        (@CtdtDM, 'DMP391', 4, 'bat_buoc', 3),
        (@CtdtDM, 'ECOM301', 4, 'bat_buoc', 4),
        (@CtdtDM, 'CRM201', 4, 'bat_buoc', 5),
        (@CtdtDM, 'PRM201', 4, 'tu_chon',  6),
        -- Kỳ 5 (18 TC)
        (@CtdtDM, 'GRH301', 5, 'bat_buoc', 1),
        (@CtdtDM, 'AIK301', 5, 'bat_buoc', 2),
        (@CtdtDM, 'DMP491', 5, 'bat_buoc', 3),
        (@CtdtDM, 'INF201', 5, 'bat_buoc', 4),
        (@CtdtDM, 'LAW201', 5, 'tu_chon',  5),
        (@CtdtDM, 'ENT101', 5, 'bat_buoc', 6),
        -- Kỳ 6 (15 TC)
        (@CtdtDM, 'OJT403', 6, 'bat_buoc', 1),
        (@CtdtDM, 'STR401', 6, 'bat_buoc', 2),
        (@CtdtDM, 'ETH301', 6, 'bat_buoc', 3),
        -- Kỳ 7 (15 TC)
        (@CtdtDM, 'CAP497', 7, 'bat_buoc', 1),
        (@CtdtDM, 'BDM401', 7, 'bat_buoc', 2);

    -- Nạp vào MonHocTrongChuongTrinh
    INSERT INTO MonHocTrongChuongTrinh (
        ma_chuong_trinh, 
        ma_mon_hoc, 
        hoc_ky_du_kien, 
        so_tin_chi, 
        loai_mon_hoc, 
        bat_buoc, 
        thu_tu, 
        con_hoat_dong, 
        ngay_tao
    )
    SELECT 
        pc.ctdtId, 
        m.ma_mon_hoc, 
        pc.hocKy, 
        m.so_tin_chi, 
        pc.loai, 
        CASE WHEN pc.loai = 'bat_buoc' THEN 1 ELSE 0 END, 
        pc.thuTu, 
        1, 
        @CurrentDate
    FROM @ProgramCurriculum pc
    JOIN DanhMucMonHoc m ON m.ma_code_mon_hoc = pc.monCode
    WHERE pc.ctdtId IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM MonHocTrongChuongTrinh tc 
          WHERE tc.ma_chuong_trinh = pc.ctdtId AND tc.ma_mon_hoc = m.ma_mon_hoc
      );

    -- Cập nhật lại số tín chỉ và học kỳ nếu đã có
    UPDATE tc
    SET tc.hoc_ky_du_kien = pc.hocKy,
        tc.so_tin_chi = m.so_tin_chi,
        tc.loai_mon_hoc = pc.loai,
        tc.bat_buoc = CASE WHEN pc.loai = 'bat_buoc' THEN 1 ELSE 0 END,
        tc.thu_tu = pc.thuTu,
        tc.con_hoat_dong = 1
    FROM MonHocTrongChuongTrinh tc
    JOIN DanhMucMonHoc m ON tc.ma_mon_hoc = m.ma_mon_hoc
    JOIN @ProgramCurriculum pc ON pc.ctdtId = tc.ma_chuong_trinh AND pc.monCode = m.ma_code_mon_hoc;

    PRINT N'  [OK] Đã hoàn thành gắn môn học vào MonHocTrongChuongTrinh!';

    -- ==================================================================
    -- 4. BỔ SUNG MÔN HỌC TIÊN QUYẾT (MonHocTienQuyet)
    -- ==================================================================
    PRINT N'- 3. Đang thiết lập quan hệ môn học tiên quyết...';

    DECLARE @Prerequisites TABLE (mon NVARCHAR(50), tienQuyet NVARCHAR(50));
    INSERT INTO @Prerequisites VALUES
        -- CNTT & SE
        ('PRO192', 'PRF192'),  -- Học Lập trình C trước khi học OOP Java
        ('CSD201', 'PRO192'),  -- Học OOP Java trước khi học Cấu trúc dữ liệu & Giải thuật
        ('DBI202', 'COM101'),  -- Học Nhập môn lập trình trước khi học CSDL
        ('WED201', 'WEB104'),  -- Học Web HTML/CSS trước khi học Frontend nâng cao Vue/React
        ('PRN211', 'PRO192'),  -- Học OOP trước khi học C# .NET
        ('PRN231', 'PRN211'),  -- Học C# .NET trước khi học Web API
        ('PRN231', 'DBI202'),  -- Học CSDL trước khi học Web API
        ('SWP391', 'WED201'),  -- Học Frontend trước khi làm Dự án 1
        ('SWP391', 'DBI202'),  -- Học CSDL trước khi làm Dự án 1
        ('SWP490', 'SWP391'),  -- Hoàn thành Dự án 1 trước khi làm Dự án 2
        ('PRM392', 'PRO192'),  -- Học OOP trước khi học Mobile App
        ('WDU301', 'NWC203'),  -- Học Mạng máy tính trước khi học Cloud DevOps
        ('CAP499', 'OJT401'),  -- Thực tập doanh nghiệp trước khi làm Đồ án tốt nghiệp

        -- Thiết kế Đồ họa (GD)
        ('ILL101', 'ART101'),  -- Mỹ thuật căn bản trước khi học Illustrator
        ('PSH101', 'COL101'),  -- Màu sắc & bố cục trước khi học Photoshop
        ('BRD201', 'ILL101'),  -- Học Illustrator trước khi Thiết kế Thương hiệu
        ('UIX201', 'UIX101'),  -- Học UI/UX căn bản trước khi học Figma nâng cao
        ('AFX201', 'PSH101'),  -- Học Photoshop trước khi làm Animation After Effects
        ('MAX301', 'MAX201'),  -- Học 3D căn bản trước khi làm 3D nhân vật nâng cao
        ('GDP491', 'GDP391'),  -- Hoàn thành Đồ án 1 trước khi làm Đồ án 2
        ('CAP498', 'OJT402'),  -- Thực tập trước khi làm Đồ án tốt nghiệp

        -- Digital Marketing (DM)
        ('MKT201', 'MKT101'),  -- Học Marketing căn bản trước khi học Digital Marketing
        ('SEO101', 'MKT201'),  -- Học Digital Marketing trước khi làm SEO
        ('SEM201', 'SEO101'),  -- Học SEO trước khi chạy Google Ads SEM
        ('PPC301', 'SEM201'),  -- Học Google Ads trước khi chạy Đa kênh trả phí
        ('MKA301', 'STA101'),  -- Thống kê kinh doanh trước khi Phân tích dữ liệu Google Analytics
        ('DMP491', 'DMP391'),  -- Dự án Marketing 1 trước khi làm Dự án 2
        ('CAP497', 'OJT403');  -- Thực tập Marketing trước khi làm Đồ án tốt nghiệp

    INSERT INTO MonHocTienQuyet (ma_mon_hoc, ma_mon_tien_quyet)
    SELECT m1.ma_mon_hoc, m2.ma_mon_hoc
    FROM @Prerequisites p
    JOIN DanhMucMonHoc m1 ON m1.ma_code_mon_hoc = p.mon
    JOIN DanhMucMonHoc m2 ON m2.ma_code_mon_hoc = p.tienQuyet
    WHERE NOT EXISTS (
        SELECT 1 FROM MonHocTienQuyet tq 
        WHERE tq.ma_mon_hoc = m1.ma_mon_hoc AND tq.ma_mon_tien_quyet = m2.ma_mon_hoc
    );

    PRINT N'  [OK] Đã thiết lập ràng buộc môn học tiên quyết!';

    -- ==================================================================
    -- 5. PHÂN CÔNG GIẢNG VIÊN ĐƯỢC PHÉP DẠY CÁC MÔN MỚI (GiaoVienMonHoc)
    -- ==================================================================
    PRINT N'- 4. Đang cấp quyền giảng dạy cho giảng viên theo chuyên ngành...';

    -- Giảng viên SE dạy tất cả các môn CNTT & SE
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhSE
      AND (m.ma_chuyen_nganh = @ChuyenNganhSE OR m.ma_nganh = @NganhCNTT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm 
          WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    -- Giảng viên GD dạy tất cả các môn Đồ họa & Thiết kế
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhGD
      AND (m.ma_chuyen_nganh = @ChuyenNganhGD OR m.ma_nganh = @NganhTKDH OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm 
          WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    -- Giảng viên DM dạy tất cả các môn Marketing & Kinh doanh số
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhDM
      AND (m.ma_chuyen_nganh = @ChuyenNganhDM OR m.ma_nganh = @NganhMKT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm 
          WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    PRINT N'  [OK] Đã hoàn tất phân quyền giảng dạy GiaoVienMonHoc!';

    COMMIT TRANSACTION;

    PRINT N'======================================================================';
    PRINT N'--- HOÀN THÀNH PHASE 6 THÀNH CÔNG! ---';
    PRINT N'- Tổng số môn học: ' + CAST((SELECT COUNT(*) FROM DanhMucMonHoc) AS NVARCHAR);
    PRINT N'- Môn trong CTĐT SE: ' + CAST((SELECT COUNT(*) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtSE) AS NVARCHAR) + N' môn (' + CAST((SELECT SUM(so_tin_chi) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtSE) AS NVARCHAR) + N' TC)';
    PRINT N'- Môn trong CTĐT GD: ' + CAST((SELECT COUNT(*) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtGD) AS NVARCHAR) + N' môn (' + CAST((SELECT SUM(so_tin_chi) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtGD) AS NVARCHAR) + N' TC)';
    PRINT N'- Môn trong CTĐT DM: ' + CAST((SELECT COUNT(*) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtDM) AS NVARCHAR) + N' môn (' + CAST((SELECT SUM(so_tin_chi) FROM MonHocTrongChuongTrinh WHERE ma_chuong_trinh = @CtdtDM) AS NVARCHAR) + N' TC)';
    PRINT N'======================================================================';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI TRONG PHASE 6 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
