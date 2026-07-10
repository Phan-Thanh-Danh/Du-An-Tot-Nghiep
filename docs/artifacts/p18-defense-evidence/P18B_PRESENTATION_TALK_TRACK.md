# P18B Presentation Talk Track

## Opening
Kính thưa hội đồng, dự án của em là hệ thống quản lý học vụ (LMS) phục vụ hoạt động đào tạo với nhiều vai trò người dùng và khối lượng dữ liệu lớn. Điểm nhấn của dự án không nằm ở giao diện mà là luồng dữ liệu sống kết nối trực tiếp xuống SQL Server, với việc phân quyền chặt chẽ thông qua JWT, đảm bảo không có tính năng nào là UI tĩnh (fake data). 

## Login
Sau đây em xin demo đăng nhập. Em sử dụng tài khoản Giao Vụ. Như thầy cô có thể thấy trên Network tab, token JWT được sinh ra thật từ backend, và giao diện Dashboard cũng được load từ các API có thực, được bọc trong các middleware phân quyền của ASP.NET Core.

## Smart Timetable
Tính năng phức tạp nhất của dự án là Xếp lịch thông minh. Để tránh việc ghi đè trực tiếp gây lỗi dữ liệu toàn trường, hệ thống sinh ra một Bản Nháp. Khi em bấm "Tạo bản nháp mới", backend sẽ load dữ liệu. 
Tiếp theo, em bấm "Kiểm tra xung đột" để xác nhận xem giáo viên hay phòng học có bị trùng giờ không.
Và cuối cùng, khi em bấm "Publish", backend sẽ lưu dữ liệu thực tế xuống DB thông qua một Database Transaction nguyên tử. Nếu có bất kỳ lỗi gì, toàn bộ lịch sẽ được rollback. Nếu em cố tình Publish lần nữa, hệ thống sẽ chặn ngay lập tức.

## Evidence
Để minh chứng rõ hơn, đây là file `p17-api-smoke-results.json` ghi lại log HTTP 200 OK thực tế từ hệ thống khi gọi các endpoint này. Điều đó khẳng định frontend giao tiếp đúng với DB thực, không dùng mock hay giả lập logic.

## Limitations
Em cũng xin minh bạch một số giới hạn: do là môi trường demo, một số hành động xóa/sửa sâu (destructive mutations) sẽ ảnh hưởng cấu trúc dữ liệu diện rộng nên em không chạy trực tiếp để tránh phá vỡ dataset mẫu. Các tính năng AI phức tạp hay payment production cũng chưa hoàn thiện ở mức triển khai thực tế.

## Closing
Tóm lại, dự án đã xây dựng thành công một bộ khung kiến trúc chuẩn, đảm bảo an toàn dữ liệu, phân quyền bảo mật cao và thao tác học vụ phức tạp được thực thi an toàn thông qua cơ chế Draft - Transaction. Em xin cảm ơn hội đồng đã lắng nghe và rất mong nhận được góp ý!
