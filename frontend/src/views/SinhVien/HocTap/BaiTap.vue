<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentAssignmentFallbackPage',
})

const metrics = [
  { label: 'Bài cần nộp', value: '3', unit: 'bài', icon: 'ClipboardList', tone: 'amber', progress: 60, hint: 'FE demo' },
  { label: 'Đã hoàn thành', value: '12', unit: 'bài', icon: 'CheckCircle2', tone: 'green', progress: 82, hint: 'Theo học kỳ' },
  { label: 'API', value: 'dự kiến', icon: 'PlugZap', tone: 'slate', progress: 10, hint: 'Không gọi backend' },
]

const rows = [
  { title: 'Bài tập CTDL - Tuần 10', description: 'Placeholder dự phòng cho route bài tập trong thư mục SinhVien.', badge: 'Sắp hết hạn', tone: 'amber', icon: 'FileClock', meta: ['23:59 hôm nay', 'CTDL101'], value: 'Chưa nộp', valueHint: 'FE state', to: '/student/assignments' },
  { title: 'Bài tập lớn Lập trình Web', description: 'Khu nộp bài thật đang dùng Student/AssignmentDetailView.vue khi có route id.', badge: 'Đang làm', tone: 'blue', icon: 'FileText', meta: ['LTW301', 'Nhóm'], value: '70%', valueHint: 'tiến độ' },
]

const timeline = [
  { title: 'Ưu tiên bài hết hạn', description: 'Card cảnh báo dùng badge màu amber để người học nhìn nhanh.', time: 'Hôm nay', tone: 'amber' },
  { title: 'Chờ contract bài tập', description: 'Chỉ nối API khi backend có controller và tài liệu contract.', time: 'Dự kiến', tone: 'slate' },
]
</script>

<template>
  <StudentModulePage
    icon="ClipboardList"
    eyebrow="Học tập"
    title="Bài tập"
    subtitle="Màn dự phòng đồng bộ Liquid Glass cho danh sách bài tập sinh viên."
    primary-title="Danh sách bài tập"
    primary-description="Các item dùng trạng thái FE để không phụ thuộc backend chưa có."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở bài tập', to: '/student/assignments', primary: true }]"
  />
</template>
