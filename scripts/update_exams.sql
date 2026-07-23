BEGIN TRAN;

-- 1. Huy 4 ca thi cu
UPDATE CaThi SET trang_thai = 'da_huy' WHERE ma_ca_thi IN (1, 2, 3, 4);
UPDATE LichThiTong SET trang_thai = 'da_huy' WHERE ma_lich_thi_tong IN (1, 2, 3, 4);

-- 2. Chuan bi cau hoi (cau_hinh_de_thi)
DECLARE @cauHinh NVARCHAR(MAX) = N'{"questions":[{"id":1,"content":"Câu hỏi trắc nghiệm 1?","type":"mcq","options":["A","B","C","D"],"answer":"A"},{"id":2,"content":"Câu hỏi trắc nghiệm 2?","type":"mcq","options":["A","B","C","D"],"answer":"B"}]}';

-- 3. Them De kiem tra moi cho GEN101 va GEN102 (ma_mon_hoc = 50, 51)
INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi, trang_thai, hinh_thuc_thi, ty_le_trac_nghiem, ngay_tao, ngay_cap_nhat)
VALUES 
(50, 3, N'Quiz Kỹ năng học tập', 45, @cauHinh, 'dang_mo', 'trac_nghiem', 100, GETDATE(), GETDATE()),
(51, 3, N'Quiz Tin học cơ bản', 45, @cauHinh, 'dang_mo', 'trac_nghiem', 100, GETDATE(), GETDATE());

DECLARE @De1 INT = SCOPE_IDENTITY() - 1;
DECLARE @De2 INT = SCOPE_IDENTITY();

-- 4. Them Lich thi tong
INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
VALUES 
(1, 50, @De1, 'online_tu_do', GETDATE(), 'da_gui_ve_co_so', GETDATE()),
(1, 51, @De2, 'online_tu_do', GETDATE(), 'da_gui_ve_co_so', GETDATE());

DECLARE @Lich1 INT = SCOPE_IDENTITY() - 1;
DECLARE @Lich2 INT = SCOPE_IDENTITY();

-- 5. Them Ca Thi
INSERT INTO CaThi (ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ngay_tao)
VALUES 
(@Lich1, N'Ca thi GEN101 - Kỹ năng học tập', 1, CAST(GETDATE() AS DATE), GETDATE(), DATEADD(HOUR, 2, GETDATE()), 3, 'dang_mo', GETDATE()),
(@Lich2, N'Ca thi GEN102 - Tin học cơ bản', 1, CAST(GETDATE() AS DATE), GETDATE(), DATEADD(HOUR, 2, GETDATE()), 3, 'dang_mo', GETDATE());

DECLARE @Ca1 INT = SCOPE_IDENTITY() - 1;
DECLARE @Ca2 INT = SCOPE_IDENTITY();

-- 6. Them Thi sinh (ma_nguoi_dung = 24)
INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
VALUES 
(@Ca1, 24, 'cho_thi', GETDATE()),
(@Ca2, 24, 'cho_thi', GETDATE());

-- 7. Them Giam thi (ma_nguoi_dung = 15)
INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai, ngay_tao)
VALUES 
(@Ca1, 15, 'giam_thi_chinh', 'du_kien', GETDATE()),
(@Ca2, 15, 'giam_thi_chinh', 'du_kien', GETDATE());

COMMIT TRAN;
