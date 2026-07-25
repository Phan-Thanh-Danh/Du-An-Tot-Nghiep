USE LMS;
GO
SET QUOTED_IDENTIFIER ON;

INSERT INTO DeKiemTra (ma_mon_hoc, ma_hoc_ky, tieu_de, thoi_gian_phut, cau_hinh_de_thi, trang_thai, loai_de_thi, hinh_thuc_thi, ngay_tao) 
VALUES (2, 1, N'Đề thi Nhập môn lập trình', 60, '{"questions":[{"id":1,"content":"Câu 1: Kiến thức cơ bản của Nhập môn lập trình là gì?","type":"mcq","options":["A. Lựa chọn 1","B. Lựa chọn 2","C. Lựa chọn 3","D. Lựa chọn 4"],"answer":"A"},{"id":2,"content":"Câu 2: Phát biểu nào đúng về Nhập môn lập trình?","type":"mcq","options":["A. Đúng 1","B. Đúng 2","C. Đúng 3","D. Đúng 4"],"answer":"B"},{"id":3,"content":"Câu 3: Đặc điểm nổi bật của Nhập môn lập trình?","type":"mcq","options":["A. Đặc điểm A","B. Đặc điểm B","C. Đặc điểm C","D. Đặc điểm D"],"answer":"C"},{"id":4,"content":"Câu 4: Ứng dụng của Nhập môn lập trình?","type":"mcq","options":["A. Ứng dụng X","B. Ứng dụng Y","C. Ứng dụng Z","D. Ứng dụng W"],"answer":"D"},{"id":5,"content":"Câu 5: Nhận định sai về Nhập môn lập trình?","type":"mcq","options":["A. Sai 1","B. Sai 2","C. Sai 3","D. Sai 4"],"answer":"A"}]}', 'dang_mo', 'trac_nghiem', 'online_tu_do', GETDATE()); 

UPDATE LichThiTong SET ma_de_kiem_tra = SCOPE_IDENTITY() WHERE ma_lich_thi_tong = 32;
