INSERT INTO BaiTap (ma_mon_hoc, tieu_de, mo_ta, han_nop, so_lan_nop_toi_da, dinh_dang_cho_phep, huong_dan_cham_diem, trang_thai) 
VALUES 
(50, N'Lab 1: Lập kế hoạch học tập', N'Sinh viên lập thời gian biểu chi tiết cho môn học này trong tuần đầu tiên.', DATEADD(day, 7, GETDATE()), 3, N'["pdf","docx"]', N'Đánh giá tính khả thi và chi tiết của thời gian biểu.', N'da_xuat_ban'), 
(50, N'Lab 2: Kỹ năng quản lý thời gian', N'Viết một báo cáo ngắn (500 từ) về cách bạn quản lý thời gian trong tuần qua.', DATEADD(day, 14, GETDATE()), 3, N'["pdf","docx"]', N'Nội dung chân thực, có dẫn chứng cụ thể.', N'da_xuat_ban'), 
(50, N'Lab 3: Đọc hiểu và tóm tắt', N'Tóm tắt một bài báo khoa học liên quan đến chuyên ngành của bạn.', DATEADD(day, 21, GETDATE()), 3, N'["pdf","docx"]', N'Tóm tắt đầy đủ ý chính, không sao chép nguyên văn.', N'da_xuat_ban'), 
(50, N'Lab 4: Chuẩn bị cho kỳ thi', N'Tạo một bản mindmap tổng hợp kiến thức đã học.', DATEADD(day, 28, GETDATE()), 3, N'["pdf","png","jpg"]', N'Mindmap rõ ràng, dễ hiểu, logic.', N'da_xuat_ban');
