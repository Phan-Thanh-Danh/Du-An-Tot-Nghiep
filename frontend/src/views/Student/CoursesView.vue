<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Đang học', value: '3', unit: 'môn', icon: 'BookOpen', tone: 'blue', progress: 60, hint: 'Route chính dùng KhoacHoc.vue' },
  { label: 'Hoàn thành', value: '1', unit: 'môn', icon: 'CheckCircle2', tone: 'green', progress: 20, hint: 'Dữ liệu dự phòng' },
  { label: 'Sắp tới', value: '1', unit: 'môn', icon: 'Clock3', tone: 'violet', progress: 20, hint: 'Học kỳ kế tiếp' },
  { label: 'API', value: 'dự kiến', icon: 'PlugZap', tone: 'amber', progress: 10, hint: 'Courses controller chưa có' },
]

const rows = [
  { title: 'Route khóa học chính', description: 'Sinh viên đang được điều hướng tới SinhVien/HocTap/KhoacHoc.vue.', badge: 'Đang dùng', tone: 'blue', icon: 'Route', meta: ['/student/courses', 'Liquid Glass'], value: 'Active', valueHint: 'Router' },
  { title: 'Bộ lọc học kỳ', description: 'Dự kiến bổ sung theo API khóa học thật.', badge: 'Dự kiến', tone: 'amber', icon: 'Filter', meta: ['Học kỳ', 'Trạng thái', 'Giảng viên'], value: 'API', valueHint: 'cần bổ sung' },
  { title: 'Tiến độ học', description: 'Không lưu tiến độ thật khi chưa có lessons/progress API.', badge: 'FE demo', tone: 'teal', icon: 'Gauge', meta: ['progress', 'lesson count'], value: 'demo', valueHint: 'frontend' },
]

const timeline = [
  { title: 'Danh sách khóa học', description: 'Màn chính đã được tinh chỉnh riêng với search/filter/grid/list.', time: 'Đã có UI', tone: 'blue' },
  { title: 'Chi tiết khóa học', description: 'Route dynamic dùng module page để trình bày chương học dự kiến.', time: 'Đã đồng bộ', tone: 'teal' },
  { title: 'API courses', description: 'Chỉ nối khi backend có controller thật.', time: 'cần bổ sung', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="BookOpen"
    eyebrow="Khóa học"
    title="Khóa học"
    subtitle="Placeholder dự phòng cho route khóa học, giữ style thống nhất nếu được gọi trực tiếp."
    primary-title="Thông tin route dự phòng"
    primary-description="Trang này giữ cùng hệ Liquid Glass nếu route dự phòng được import trực tiếp."
    timeline-title="Trạng thái triển khai"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở danh sách chính', to: '/student/courses', primary: true }]"
  />
</template>
