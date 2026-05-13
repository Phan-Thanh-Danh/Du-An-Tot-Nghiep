<script setup>
import { useRoute } from 'vue-router'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const route = useRoute()
const assignmentId = String(route.params.assignmentId || 'assignment')

const metrics = [
  { label: 'Hạn nộp', value: '23:59', icon: 'ClockAlert', tone: 'orange', progress: 82, hint: 'Hôm nay trong dữ liệu demo' },
  { label: 'Số lần nộp', value: '0', unit: '/2', icon: 'UploadCloud', tone: 'blue', progress: 0, hint: 'Chưa có submission thật' },
  { label: 'Rubric', value: '4', unit: 'mục', icon: 'ListChecks', tone: 'teal', progress: 60, hint: 'Cấu trúc chấm điểm dự kiến' },
  { label: 'Định dạng', value: 'ZIP', icon: 'FileArchive', tone: 'violet', progress: 50, hint: 'Mô phỏng yêu cầu file' },
]

const rows = [
  { title: 'Yêu cầu bài tập', description: 'Cài đặt, giải thích và kiểm thử cấu trúc dữ liệu theo đề bài.', badge: 'Bắt buộc', tone: 'blue', icon: 'ClipboardList', meta: ['Mô tả rõ', 'Có báo cáo', 'Có source code'], value: '40%', valueHint: 'Rubric' },
  { title: 'Dropzone nộp bài', description: 'Khu vực upload thật cần validate file, loading state và error message.', badge: 'Dự kiến', tone: 'amber', icon: 'UploadCloud', meta: ['.zip', '.pdf', 'max size cần bổ sung'], value: 'API', valueHint: 'submissions' },
  { title: 'Lịch sử nộp', description: 'Chỉ hiển thị dữ liệu thật khi backend trả về danh sách submission.', badge: 'Chưa có', tone: 'slate', icon: 'History', meta: ['0 lần nộp', assignmentId], value: '0', valueHint: 'bản nộp' },
]

const timeline = [
  { title: 'Đọc yêu cầu', description: 'Sinh viên cần nắm rubric trước khi nộp bài.', time: 'Bước 1', tone: 'blue' },
  { title: 'Nộp bài', description: 'Chưa mở thao tác thật vì API submissions chưa có.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Nhận xét & điểm', description: 'Điểm và feedback chỉ hiển thị khi backend có dữ liệu.', time: 'dự kiến', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="UploadCloud"
    eyebrow="Bài tập"
    :title="`Chi tiết bài tập ${assignmentId}`"
    subtitle="Trang chi tiết bài tập đang chờ API assignment/submission, hiện chỉ chuẩn hóa khung FE."
    primary-title="Yêu cầu và nộp bài"
    primary-description="Form nộp bài tương lai sẽ dùng surface solid, có trạng thái rõ ràng và không hardcode upload thật."
    timeline-title="Luồng nộp bài"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Danh sách bài tập', to: '/student/assignments' }, { label: 'Hỗ trợ nộp bài', to: '/student/support-tickets', primary: true }]"
  />
</template>
