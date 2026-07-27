SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- Update Questions for Nhap mon lap trinh (MaDeKiemTra 13)
UPDATE CauHoi 
SET noi_dung = N'Câu 1. Thuật toán (Algorithm) là gì?',
    loai_cau_hoi = 'trac_nghiem',
    kieu_lua_chon = 'chon_mot',
    lua_chon = N'[{"id":"A","text":"Một chương trình đã được biên dịch."},{"id":"B","text":"Một dãy các bước được sắp xếp hợp lý để giải quyết một bài toán."},{"id":"C","text":"Một ngôn ngữ lập trình."},{"id":"D","text":"Một phần mềm dùng để viết mã nguồn."}]',
    dap_an_dung = N'["B"]'
WHERE ma_cau_hoi = 178;

UPDATE CauHoi 
SET noi_dung = N'Câu 2. Trong lập trình, biến (Variable) được dùng để làm gì?',
    loai_cau_hoi = 'trac_nghiem',
    kieu_lua_chon = 'chon_mot',
    lua_chon = N'[{"id":"A","text":"Lưu trữ dữ liệu trong quá trình chương trình thực thi."},{"id":"B","text":"Chỉ hiển thị dữ liệu lên màn hình."},{"id":"C","text":"Xóa dữ liệu khỏi bộ nhớ."},{"id":"D","text":"Dừng chương trình."}]',
    dap_an_dung = N'["A"]'
WHERE ma_cau_hoi = 179;

UPDATE CauHoi 
SET noi_dung = N'Câu 3. Cấu trúc điều khiển nào được sử dụng để thực hiện một khối lệnh nhiều lần?',
    loai_cau_hoi = 'trac_nghiem',
    kieu_lua_chon = 'chon_mot',
    lua_chon = N'[{"id":"A","text":"if...else"},{"id":"B","text":"switch...case"},{"id":"C","text":"Vòng lặp (for, while, do...while)"},{"id":"D","text":"break"}]',
    dap_an_dung = N'["C"]'
WHERE ma_cau_hoi = 180;

UPDATE CauHoi 
SET noi_dung = N'Câu 4. Phát biểu nào sau đây đúng về câu lệnh if...else?',
    loai_cau_hoi = 'trac_nghiem',
    kieu_lua_chon = 'chon_mot',
    lua_chon = N'[{"id":"A","text":"Dùng để khai báo biến."},{"id":"B","text":"Dùng để lựa chọn thực hiện các khối lệnh dựa trên điều kiện."},{"id":"C","text":"Dùng để lặp vô hạn."},{"id":"D","text":"Dùng để nhập dữ liệu từ bàn phím."}]',
    dap_an_dung = N'["B"]'
WHERE ma_cau_hoi = 181;

UPDATE CauHoi 
SET noi_dung = N'Câu 5. Kiểu dữ liệu nào thường được dùng để lưu giá trị Đúng/Sai?',
    loai_cau_hoi = 'trac_nghiem',
    kieu_lua_chon = 'chon_mot',
    lua_chon = N'[{"id":"A","text":"int"},{"id":"B","text":"float"},{"id":"C","text":"char"},{"id":"D","text":"bool (Boolean)"}]',
    dap_an_dung = N'["D"]'
WHERE ma_cau_hoi = 182;

-- Auto-grade existing submitted student sessions
UPDATE PhienThiHocSinh 
SET diem_tu_dong = 4.00,
    diem_cuoi_cung = 4.00,
    so_cau_dung = 2,
    trang_thai_cong_bo = 'da_cham_xong'
WHERE ma_phien_thi = 23;
