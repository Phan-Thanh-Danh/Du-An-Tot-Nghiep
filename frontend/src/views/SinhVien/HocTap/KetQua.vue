<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentResultFallbackPage',
})

const metrics = [
  { label: 'GPA', value: '3.42', unit: '/4.0', icon: 'Award', tone: 'green', progress: 86, hint: 'FE demo' },
  { label: 'Tín chỉ đạt', value: '48', icon: 'CheckCircle2', tone: 'blue', progress: 68, hint: 'Tích lũy' },
  { label: 'Cảnh báo', value: '0', icon: 'ShieldCheck', tone: 'teal', progress: 100, hint: 'Học vụ' },
]

const rows = [
  { title: 'Kết quả học tập', description: 'Trang chính đang dùng /student/grades với table shell rõ và badge đạt/cảnh báo.', badge: 'Grades', tone: 'green', icon: 'Table2', meta: ['Bảng điểm', 'Học kỳ'], value: 'Xem', valueHint: '/student/grades', to: '/student/grades' },
  { title: 'Điểm mới công bố', description: 'Các điểm mới được hiển thị ở dashboard và bảng điểm theo cùng style.', badge: 'Mới', tone: 'blue', icon: 'Sparkles', meta: ['Recent grades'], value: '3', valueHint: 'môn' },
]

const timeline = [
  { title: 'Kiểm tra học kỳ', description: 'Bảng điểm giữ surface đọc rõ, không blur từng row.', time: 'UI rule', tone: 'green' },
  { title: 'Chờ API điểm', description: 'Không gọi endpoint khi contract chưa có.', time: 'Dự kiến', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="Award"
    eyebrow="Kết quả"
    title="Kết quả học tập"
    subtitle="Màn dự phòng đồng bộ style cho kết quả học tập sinh viên."
    primary-title="Tổng quan điểm"
    primary-description="Các bảng dữ liệu giữ contrast cao theo chuẩn Liquid Glass."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở bảng điểm', to: '/student/grades', primary: true }]"
  />
</template>
