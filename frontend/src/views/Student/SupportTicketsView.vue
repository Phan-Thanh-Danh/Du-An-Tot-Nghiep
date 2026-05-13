<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Ticket mở', value: '1', unit: 'phiếu', icon: 'MessageCircleWarning', tone: 'amber', progress: 20, hint: 'Đang chờ phản hồi' },
  { label: 'Đã xử lý', value: '6', unit: 'phiếu', icon: 'CheckCircle2', tone: 'green', progress: 86, hint: 'Trong học kỳ' },
  { label: 'Thời gian TB', value: '8', unit: 'giờ', icon: 'Timer', tone: 'blue', progress: 55, hint: 'Dữ liệu demo' },
  { label: 'FAQ', value: '12', unit: 'mục', icon: 'CircleHelp', tone: 'teal', progress: 70, hint: 'Cần API FAQ' },
]

const rows = [
  { title: 'Lỗi truy cập tài liệu môn CTDL', description: 'Sinh viên không mở được file PDF trong bài học tuần 10.', badge: 'Đang xử lý', tone: 'amber', icon: 'FileWarning', meta: ['Kỹ thuật', 'Ưu tiên vừa', '2 giờ trước'], value: '#TK-1024', valueHint: 'Mã ticket' },
  { title: 'Xác nhận học phí kỳ 2', description: 'Yêu cầu kiểm tra trạng thái thanh toán đã nộp.', badge: 'Hoàn tất', tone: 'green', icon: 'ReceiptText', meta: ['Tài chính', 'Đã đóng'], value: '#TK-1018', valueHint: 'Mã ticket' },
  { title: 'Tư vấn đăng ký môn tự chọn', description: 'Cần hỗ trợ chọn lớp học phần phù hợp thời khóa biểu.', badge: 'Mẫu', tone: 'blue', icon: 'MessagesSquare', meta: ['Học vụ', 'Dự kiến'], value: 'cần bổ sung', valueHint: 'API support' },
]

const timeline = [
  { title: 'Tạo ticket', description: 'Form thật cần validate, loading và error state trước khi gửi.', time: 'UI rule', tone: 'blue' },
  { title: 'Trao đổi', description: 'Tin nhắn hỗ trợ cần endpoint support messages.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Đóng ticket', description: 'Không tự thay đổi trạng thái khi chưa có backend workflow.', time: 'API rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="MessageCircleHelp"
    eyebrow="Hỗ trợ"
    title="Hỗ trợ & Ticket"
    subtitle="Module hỗ trợ đang ở trạng thái dự kiến, chưa gọi API support/notification."
    primary-title="Ticket gần đây"
    primary-description="Trải nghiệm hỗ trợ được trình bày rõ trạng thái, nhóm vấn đề và mã theo dõi."
    timeline-title="Quy trình hỗ trợ"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem đơn từ', to: '/student/requests' }, { label: 'Tạo ticket mới', to: '/student/support-tickets', primary: true }]"
  />
</template>
