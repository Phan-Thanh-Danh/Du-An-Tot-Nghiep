<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentGradebookFallbackPage',
})

const metrics = [
  { label: 'Điểm TB', value: '8.4', unit: '/10', icon: 'Gauge', tone: 'green', progress: 84, hint: 'FE demo' },
  { label: 'Môn đã có điểm', value: '5', icon: 'BookCheck', tone: 'blue', progress: 75, hint: 'Học kỳ 2' },
  { label: 'Chờ công bố', value: '1', icon: 'Clock3', tone: 'amber', progress: 35, hint: 'Kết quả thi' },
]

const rows = [
  { title: 'Bảng điểm học kỳ', description: 'Route chính /student/grades dùng table shell glass nhẹ và header semi-solid.', badge: 'Table', tone: 'green', icon: 'Table2', meta: ['Readable', 'Semi-solid'], value: 'Mở', valueHint: '/student/grades', to: '/student/grades' },
  { title: 'Badge kết quả', description: 'Đạt, cảnh báo, chưa công bố dùng chip màu có contrast đủ mạnh.', badge: 'Status', tone: 'blue', icon: 'BadgeCheck', meta: ['Pass', 'Warning'], value: 'Đồng bộ', valueHint: 'UI' },
]

const timeline = [
  { title: 'Ưu tiên khả năng đọc', description: 'Bảng dữ liệu không dùng blur trên từng row để tránh mỏi mắt.', time: 'Design rule', tone: 'green' },
  { title: 'Chờ API bảng điểm', description: 'Không tự bịa endpoint điểm.', time: 'Dự kiến', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="Table2"
    eyebrow="Kết quả"
    title="Bảng điểm"
    subtitle="Màn dự phòng cho bảng điểm trong thư mục SinhVien."
    primary-title="Bảng điểm sinh viên"
    primary-description="Giữ table shell rõ ràng, contrast cao và đồng bộ Liquid Glass."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở bảng điểm', to: '/student/grades', primary: true }]"
  />
</template>
