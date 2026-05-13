<script setup>
import { useRoute } from 'vue-router'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const route = useRoute()
const examResultId = String(route.params.examResultId || 'exam')

const metrics = [
  { label: 'Điểm', value: '9.0', unit: '/10', icon: 'Award', tone: 'green', progress: 90, hint: 'Dữ liệu FE demo' },
  { label: 'Đúng', value: '18', unit: '/20', icon: 'CheckCircle2', tone: 'teal', progress: 90, hint: 'Câu trả lời đúng' },
  { label: 'Thời gian', value: '42', unit: 'phút', icon: 'Timer', tone: 'blue', progress: 70, hint: 'Trong giới hạn' },
  { label: 'Vi phạm', value: '0', unit: 'lỗi', icon: 'ShieldCheck', tone: 'green', progress: 100, hint: 'Không có trong demo' },
]

const rows = [
  { title: 'Tổng quan phiên thi', description: 'Kết quả mô phỏng cho phiên kiểm tra, chưa đọc từ backend.', badge: 'FE demo', tone: 'blue', icon: 'FileCheck2', meta: [examResultId, 'Đã nộp', 'Không vi phạm'], value: '9.0', valueHint: 'Điểm' },
  { title: 'Phân tích câu trả lời', description: 'Khu vực này sẽ dùng table/detail solid khi có câu trả lời JSON.', badge: 'Dự kiến', tone: 'teal', icon: 'ListChecks', meta: ['18 đúng', '2 sai', '0 bỏ trống'], value: '90%', valueHint: 'Độ chính xác' },
  { title: 'Nhật ký phiên thi', description: 'Cần API exam-session để hiển thị log thật.', badge: 'Cần bổ sung', tone: 'amber', icon: 'ScrollText', meta: ['StartedAt', 'SubmittedAt', 'ViolationLog'], value: 'API', valueHint: 'session' },
]

const timeline = [
  { title: 'Bắt đầu phiên thi', description: 'Dữ liệu phiên làm bài là mô phỏng frontend.', time: '08:00', tone: 'blue' },
  { title: 'Nộp bài', description: 'Submit thật cần endpoint exam-sessions.', time: '08:42', tone: 'green' },
  { title: 'Công bố kết quả', description: 'Kết quả chỉ là UI demo khi chưa có controller exams.', time: 'dự kiến', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="FileCheck2"
    eyebrow="Đánh giá"
    :title="`Kết quả kiểm tra ${examResultId}`"
    subtitle="Trang kết quả thi là UI dự kiến, chưa gọi API exam-session."
    primary-title="Tổng quan kết quả"
    primary-description="Bố cục kết quả dùng solid surface để điểm, câu trả lời và log luôn rõ ràng."
    timeline-title="Phiên thi"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Lịch thi', to: '/student/exams' }, { label: 'Bảng điểm', to: '/student/grades', primary: true }]"
  />
</template>
