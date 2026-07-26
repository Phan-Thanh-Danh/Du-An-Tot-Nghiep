SET NOCOUNT ON;
BEGIN TRANSACTION;

INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
VALUES (50, 1, 'trac_nghiem', N'Kỹ năng quan trọng nhất trong học tập đại học là gì?', N'[{"id":"A","text":"Học thuộc lòng"},{"id":"B","text":"Quản lý thời gian và tự học"},{"id":"C","text":"Chơi game"},{"id":"D","text":"Ngủ đủ giấc"}]', '["B"]', 'de', 1, 'chon_mot', GETUTCDATE(), GETUTCDATE());
DECLARE @Q1 INT = SCOPE_IDENTITY();

INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
VALUES (50, 1, 'trac_nghiem', N'Mục tiêu SMART là viết tắt của các từ nào?', N'[{"id":"A","text":"Specific, Measurable, Achievable, Relevant, Time-bound"},{"id":"B","text":"Super, Master, Awesome, Real, Time"},{"id":"C","text":"Small, Medium, Average, Regular, Tall"},{"id":"D","text":"Không có ý nghĩa gì"}]', '["A"]', 'trung_binh', 1, 'chon_mot', GETUTCDATE(), GETUTCDATE());
DECLARE @Q2 INT = SCOPE_IDENTITY();

INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, lua_chon, dap_an_dung, do_kho, con_hoat_dong, kieu_lua_chon, ngay_tao, ngay_cap_nhat)
VALUES (50, 1, 'trac_nghiem', N'Phương pháp Pomodoro dùng để làm gì?', N'[{"id":"A","text":"Nấu ăn"},{"id":"B","text":"Trồng cà chua"},{"id":"C","text":"Quản lý thời gian, tăng sự tập trung"},{"id":"D","text":"Tập thể dục"}]', '["C"]', 'trung_binh', 1, 'chon_mot', GETUTCDATE(), GETUTCDATE());
DECLARE @Q3 INT = SCOPE_IDENTITY();

INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES (10, @Q1, 3.33, 1);
INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES (10, @Q2, 3.33, 2);
INSERT INTO CauHoiDeKiemTra (ma_de_kiem_tra, ma_cau_hoi, diem_so, thu_tu) VALUES (10, @Q3, 3.34, 3);

COMMIT;
SELECT 'Thanh cong' as KET_QUA;
