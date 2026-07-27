USE LMS;

DECLARE @deId INT, @lichId INT, @maxQ INT;

-- 1. DeKiemTra V2 - dung hinh_thuc_thi hop le
INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, loai_de_thi, hinh_thuc_thi, trang_thai, ma_nguoi_soan, ngay_tao)
SELECT 50, ma_hoc_ky, 'Quiz Ky nang hoc tap V2', 30, 'trac_nghiem', 'online_tap_trung', 'dang_mo', ma_nguoi_soan, GETUTCDATE()
FROM DeKiemTra WHERE ma_de_kiem_tra = 10;
SET @deId = SCOPE_IDENTITY();
PRINT 'DeKiemTra ID: ' + CAST(@deId AS NVARCHAR(20));

-- 2. 5 cau hoi - dap_an_dung la JSON array
INSERT INTO CauHoi (noi_dung, loai_cau_hoi, do_kho, dap_an_dung, lua_chon, ma_mon_hoc, ngay_tao, nguoi_tao)
VALUES
('Muc tieu cua viec lap ke hoach hoc tap la gi?','trac_nghiem','de','["B"]','[{"key":"A","text":"Hoc theo cam hung"},{"key":"B","text":"Quan ly thoi gian hieu qua"},{"key":"C","text":"Giam so mon hoc"},{"key":"D","text":"Chi hoan thanh bai tap"}]',50,GETUTCDATE(),15),
('Khi lam viec nhom, yeu to nao quan trong nhat?','trac_nghiem','de','["C"]','[{"key":"A","text":"Moi nguoi lam rieng"},{"key":"B","text":"Chi truong nhom quyet"},{"key":"C","text":"Giao tiep va phan cong ro rang"},{"key":"D","text":"Lam theo so thich"}]',50,GETUTCDATE(),15),
('Phuong phap nao giup ghi nho kien thuc hieu qua?','trac_nghiem','trung_binh','["C"]','[{"key":"A","text":"Chi doc 1 lan"},{"key":"B","text":"Hoc thuoc long"},{"key":"C","text":"On tap dinh ky ket hop ghi chu"},{"key":"D","text":"Chi hoc truoc thi"}]',50,GETUTCDATE(),15),
('Khi gap kho khan trong hoc tap, sinh vien nen?','trac_nghiem','de','["C"]','[{"key":"A","text":"Bo qua"},{"key":"B","text":"Cho ky thi"},{"key":"C","text":"Chu dong tim tai lieu hoi thay"},{"key":"D","text":"Sao chep bai"}]',50,GETUTCDATE(),15),
('Ky nang quan ly thoi gian hieu qua giup nguoi hoc:','trac_nghiem','de','["A"]','[{"key":"A","text":"Hoan thanh dung han giam ap luc"},{"key":"B","text":"Choi game nhieu hon"},{"key":"C","text":"Khong can ke hoach"},{"key":"D","text":"Chi tap trung 1 mon"}]',50,GETUTCDATE(),15);

SET @maxQ = SCOPE_IDENTITY();
PRINT 'CauHoi IDs: ' + CAST(@maxQ-4 AS NVARCHAR) + ' to ' + CAST(@maxQ AS NVARCHAR);

-- 3. Link cau hoi den de
INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES
(@deId,@maxQ-4,2,1),(@deId,@maxQ-3,2,2),(@deId,@maxQ-2,2,3),(@deId,@maxQ-1,2,4),(@deId,@maxQ,2,5);
PRINT 'Questions linked: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- 4. LichThiTong - dung trang_thai hop le
INSERT INTO LichThiTong (ma_ky_thi, ma_mon_hoc, ma_de_kiem_tra, hinh_thuc_thi, ngay_thi_du_kien, trang_thai, ngay_tao)
SELECT ma_ky_thi, 50, @deId, 'online_tap_trung', DATEADD(day,1,ngay_thi_du_kien), 'nhap', GETUTCDATE()
FROM LichThiTong WHERE ma_lich_thi_tong = 88;
SET @lichId = SCOPE_IDENTITY();
PRINT 'LichThiTong ID: ' + CAST(@lichId AS NVARCHAR);

-- 5. CaThi 88
SET IDENTITY_INSERT CaThi ON;
INSERT INTO CaThi (ma_ca_thi, ma_lich_thi_tong, ten_ca_thi, ma_phong, ngay_thi, thoi_gian_bat_dau, thoi_gian_ket_thuc, ma_don_vi, trang_thai, ghi_chu, ngay_tao)
SELECT 88, @lichId, 'Thi Ky nang hoc tap V2', ma_phong, DATEADD(day,1,ngay_thi), DATEADD(day,1,thoi_gian_bat_dau), DATEADD(day,1,thoi_gian_ket_thuc), ma_don_vi, 'da_san_sang', 'Ca thi V2', GETUTCDATE()
FROM CaThi WHERE ma_ca_thi = 87;
SET IDENTITY_INSERT CaThi OFF;
PRINT 'CaThi 88 created';

-- 6. ThiSinhCaThi
INSERT INTO ThiSinhCaThi (ma_ca_thi, ma_hoc_sinh, trang_thai_du_thi, ngay_tao)
SELECT 88, ma_hoc_sinh, 'duoc_thi', GETUTCDATE() FROM ThiSinhCaThi WHERE ma_ca_thi = 87;
PRINT 'ThiSinh copied: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- 7. PhanCongGiamThi
INSERT INTO PhanCongGiamThi (ma_ca_thi, ma_giam_thi, vai_tro_giam_thi, trang_thai)
SELECT 88, ma_giam_thi, vai_tro_giam_thi, trang_thai FROM PhanCongGiamThi WHERE ma_ca_thi = 87;
PRINT 'GiamThi copied: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Verify
SELECT c.ma_ca_thi, c.ten_ca_thi, c.trang_thai, l.ma_de_kiem_tra, d.tieu_de,
       (SELECT COUNT(*) FROM ThiSinhCaThi WHERE ma_ca_thi=c.ma_ca_thi) so_ts
FROM CaThi c JOIN LichThiTong l ON c.ma_lich_thi_tong=l.ma_lich_thi_tong
JOIN DeKiemTra d ON l.ma_de_kiem_tra=d.ma_de_kiem_tra
WHERE c.ma_ca_thi IN (87,88);
