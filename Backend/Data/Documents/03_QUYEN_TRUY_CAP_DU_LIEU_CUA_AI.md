---
document_id: AET-RAG-003
title: Quyền truy cập dữ liệu của trợ lý AI
source: Đặc tả tích hợp AET LMS Local AI
effective_date: 2026-09-02
updated_at: 2026-09-02
allowed_roles: [Student, Teacher, Admin]
department: ALL
status: active
document_type: security-policy
---

# Quyền truy cập dữ liệu của trợ lý AI

Model AI không được nhận connection string, password, JWT, refresh token hoặc quyền chạy câu SQL tùy ý. Việc truy cập SQL Server do Backend AET LMS kiểm soát.

Backend xác định người dùng từ JWT, kiểm tra role và gọi một chức năng đã cho phép. Các chức năng dự kiến gồm xem điểm của chính người dùng, lịch học cá nhân, chuyên cần, bài tập chưa hoàn thành và điều kiện dự thi.

Sinh viên chỉ được xem dữ liệu của chính mình. Giáo viên chỉ được xem dữ liệu trong phạm vi lớp hoặc môn được phân công. Quản trị viên truy cập theo quyền hiện hữu của AET LMS. Model không được phép bỏ qua các quy tắc phân quyền này.

Trong giai đoạn đầu, trợ lý chỉ đọc dữ liệu. Nếu sau này có hành động ghi dữ liệu, người dùng phải xem trước và xác nhận, Backend phải kiểm tra quyền lại và hệ thống phải ghi audit log.

Không đưa bản xuất toàn bộ database vào RAG. Dữ liệu thay đổi thường xuyên phải được truy vấn tại thời điểm người dùng hỏi.

