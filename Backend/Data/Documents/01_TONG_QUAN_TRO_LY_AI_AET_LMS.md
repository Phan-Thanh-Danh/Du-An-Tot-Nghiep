---
document_id: AET-RAG-001
title: Tổng quan trợ lý AI trong AET LMS
source: Đặc tả tích hợp AET LMS Local AI
effective_date: 2026-09-02
updated_at: 2026-09-02
allowed_roles: [Student, Teacher, Admin]
department: ALL
status: active
document_type: product-guide
---

# Tổng quan trợ lý AI trong AET LMS

Trợ lý AI là chức năng bổ sung trong AET LMS, hỗ trợ người dùng tìm tài liệu, giải thích thông tin học tập và hướng dẫn sử dụng hệ thống. AI không thay thế các chức năng chính thức như xem điểm, học tập, thi, điểm danh hoặc quản lý lớp.

Hệ thống sử dụng AI local thông qua Ollama. Qwen 2.5 3B được ưu tiên cho câu hỏi ngắn, FAQ và nội dung RAG. Qwen 3.5 9B được dùng khi cần phân tích phức tạp. Qwen3 Embedding 0.6B tạo vector để tìm các đoạn tài liệu liên quan.

Đối với dữ liệu cá nhân như điểm, lịch học, chuyên cần và bài tập, Backend lấy dữ liệu theo người đang đăng nhập. Kết quả đơn giản có thể được trả trực tiếp theo mẫu để tăng tốc độ và độ chính xác. Model chỉ được dùng khi cần giải thích hoặc đề xuất dựa trên dữ liệu đã giới hạn.

Nếu không tìm thấy tài liệu hoặc dữ liệu đủ tin cậy, trợ lý phải nói rõ chưa có đủ thông tin, không tự tạo quy định hoặc số liệu.

