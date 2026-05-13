<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentExamPage',
})

const metrics = [
  { label: 'Route chính', value: 'Exams', icon: 'FileCheck', tone: 'blue', progress: 100, hint: 'Student/ExamsView.vue' },
  { label: 'UI', value: 'đồng bộ', icon: 'Sparkles', tone: 'teal', progress: 100, hint: 'Liquid Glass' },
  { label: 'API', value: 'dự kiến', icon: 'PlugZap', tone: 'amber', progress: 10, hint: 'Exam controller chưa có' },
]

const rows = [
  { title: 'Lịch thi', description: 'Trang dự phòng cho khu vực học tập, route chính đang dùng Student/ExamsView.vue.', badge: 'Dự phòng', tone: 'blue', icon: 'CalendarClock', meta: ['Thi', 'Kiểm tra'], value: 'FE', valueHint: 'fallback' },
  { title: 'Kết quả', description: 'Kết quả thi sẽ chỉ nối khi API exam có controller thật.', badge: 'Dự kiến', tone: 'teal', icon: 'Award', meta: ['Exam result', 'Session'], value: 'API', valueHint: 'cần bổ sung' },
]

const timeline = [
  { title: 'Đồng bộ route', description: 'Giữ cùng component pattern để không lệch style nếu được gọi.', time: 'UI', tone: 'blue' },
  { title: 'Nối API sau', description: 'Không gọi endpoint dự kiến.', time: 'API rule', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="FileCheck"
    eyebrow="Học tập"
    title="Thi / Kiểm tra"
    subtitle="Placeholder dự phòng cho module thi trong thư mục SinhVien."
    primary-title="Route dự phòng"
    primary-description="Giữ nhất quán Liquid Glass cho mọi màn student liên quan đến thi."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở lịch thi', to: '/student/exams', primary: true }]"
  />
</template>
