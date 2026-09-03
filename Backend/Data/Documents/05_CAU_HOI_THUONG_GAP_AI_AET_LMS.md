---
document_id: AET-RAG-005
title: Câu hỏi thường gặp về AI trong AET LMS
source: Đặc tả tích hợp AET LMS Local AI
effective_date: 2026-09-02
updated_at: 2026-09-02
allowed_roles: [Student, Teacher, Admin]
department: ALL
status: active
document_type: faq
---

# Câu hỏi thường gặp về AI trong AET LMS

## AI có tự đọc toàn bộ database không?

Không. Backend chỉ cung cấp phần dữ liệu cần thiết sau khi xác thực và kiểm tra quyền.

## AI có thể xem điểm của sinh viên khác không?

Sinh viên không được xem điểm của người khác. Giáo viên và quản trị viên chỉ xem theo quyền được cấp trong AET LMS.

## Vì sao câu hỏi đơn giản không dùng model 9B?

Thông tin như điểm, lịch học hoặc số buổi vắng nên được Backend truy vấn và trả theo mẫu. Cách này nhanh và chính xác hơn việc yêu cầu model suy luận lại số liệu.

## Khi nào hệ thống dùng model 3B?

Model 3B phù hợp cho FAQ, hướng dẫn và câu hỏi dựa trên các đoạn tài liệu RAG.

## Khi nào hệ thống dùng model 9B?

Model 9B dùng cho yêu cầu phân tích phức tạp, chẳng hạn đề xuất kế hoạch học tập dựa trên nhiều loại dữ liệu đã được Backend cung cấp.

## AI có thể tự xuất bản thời khóa biểu không?

Không. AI chỉ hỗ trợ giải thích và tối ưu ưu tiên. Người có quyền phải xem bản Draft và chủ động Publish.

## Nếu không tìm thấy câu trả lời thì sao?

Trợ lý phải nói rõ chưa tìm thấy đủ thông tin và không được tự bịa quy định hoặc dữ liệu.

