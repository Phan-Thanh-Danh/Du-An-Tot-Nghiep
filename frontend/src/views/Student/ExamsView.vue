<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Sắp thi', value: '2', unit: 'ca', icon: 'CalendarClock', tone: 'blue', progress: 38, hint: 'Trong 7 ngày tới' },
  { label: 'Đã hoàn thành', value: '5', unit: 'bài', icon: 'CheckCircle2', tone: 'green', progress: 72, hint: 'Kỳ hiện tại' },
  { label: 'Điểm mới', value: '1', unit: 'kết quả', icon: 'Award', tone: 'violet', progress: 20, hint: 'Lập trình Web' },
  { label: 'Cảnh báo', value: '0', unit: 'lỗi', icon: 'ShieldCheck', tone: 'teal', progress: 100, hint: 'Không có vi phạm demo' },
]

const rows = [
  { title: 'Thi giữa kỳ Toán rời rạc', description: 'Ca thi trực tiếp, sinh viên cần có mặt trước 15 phút.', badge: 'Sắp thi', tone: 'blue', icon: 'FileCheck', meta: ['09/05/2025', '07:30', 'P.201'], value: '60 phút', valueHint: 'Thời lượng', to: '/student/exams/demo' },
  { title: 'Quiz Cấu trúc dữ liệu', description: 'Bài kiểm tra ngắn theo chương cây và đồ thị.', badge: 'Online', tone: 'teal', icon: 'Timer', meta: ['12 câu hỏi', '1 lần làm', 'Dự kiến'], value: '15 phút', valueHint: 'Giới hạn' },
  { title: 'Kết quả Lập trình Web', description: 'Điểm kiểm tra giữa kỳ đã được công bố trong dữ liệu FE demo.', badge: 'Đã có điểm', tone: 'green', icon: 'Award', meta: ['LTW301', 'Giữa kỳ'], value: '9.0', valueHint: '/10', to: '/student/exams/ltw301' },
]

const timeline = [
  { title: 'Thi Toán rời rạc', description: 'Ca thi gần nhất được đặt nổi bật để dễ chuẩn bị.', time: '09/05/2025', tone: 'blue' },
  { title: 'Phiên làm bài', description: 'Luồng start/submit exam cần API session trước khi triển khai thật.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Kết quả', description: 'Bảng kết quả dùng solid surface, không đặt dữ liệu điểm trên nền glass mờ.', time: 'UI rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="FileCheck"
    eyebrow="Đánh giá"
    title="Thi / Kiểm tra"
    subtitle="Màn thi đang ở trạng thái dự kiến để giữ navigation và layout đồng nhất theo Liquid Glass."
    primary-title="Lịch thi và kết quả"
    primary-description="Thông tin thi được tổ chức theo mức ưu tiên thời gian và trạng thái công bố."
    timeline-title="Chuẩn bị"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem bảng điểm', to: '/student/grades' }, { label: 'Hỗ trợ thi', to: '/student/support-tickets', primary: true }]"
  />
</template>
