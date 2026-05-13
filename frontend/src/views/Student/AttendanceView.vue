<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Tỷ lệ tham dự', value: '92', unit: '%', icon: 'UserCheck', tone: 'green', progress: 92, hint: 'Đạt yêu cầu học vụ' },
  { label: 'Có mặt', value: '34', unit: 'buổi', icon: 'CheckCircle2', tone: 'teal', progress: 85, hint: 'Tính trong kỳ hiện tại' },
  { label: 'Vắng', value: '3', unit: 'buổi', icon: 'CircleOff', tone: 'orange', progress: 18, hint: '1 buổi có phép' },
  { label: 'Đi muộn', value: '2', unit: 'lần', icon: 'ClockAlert', tone: 'amber', progress: 12, hint: 'Theo dữ liệu demo' },
]

const rows = [
  { title: 'Cấu trúc dữ liệu & Giải thuật', description: 'Điểm danh tốt, chưa vượt ngưỡng cảnh báo.', badge: '92%', tone: 'green', icon: 'UserCheck', meta: ['11/12 buổi', '1 vắng có phép'], value: 'An toàn', valueHint: 'Trạng thái' },
  { title: 'Toán rời rạc', description: 'Cần chú ý một buổi vắng chưa có phép.', badge: '86%', tone: 'amber', icon: 'AlertTriangle', meta: ['6/7 buổi', '1 vắng'], value: 'Theo dõi', valueHint: 'Cảnh báo nhẹ' },
  { title: 'Lập trình Web', description: 'Đã tham dự đầy đủ các buổi thực hành gần đây.', badge: '100%', tone: 'teal', icon: 'MonitorCheck', meta: ['10/10 buổi', '0 vắng'], value: 'Tốt', valueHint: 'Ổn định' },
]

const timeline = [
  { title: 'Điểm danh gần nhất', description: 'Toán rời rạc, P.105, trạng thái sắp tới.', time: '13:30 hôm nay', tone: 'blue' },
  { title: 'Yêu cầu mở khóa', description: 'Chỉ gửi thật khi backend có attendance-unlock-request API.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Không hard delete', description: 'Dữ liệu học vụ ưu tiên trạng thái/xóa mềm theo rule dự án.', time: 'Database rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="UserCheck"
    eyebrow="Lịch học"
    title="Điểm danh"
    subtitle="Module điểm danh là UI dự kiến, chưa gọi endpoint attendance."
    primary-title="Tình trạng theo môn"
    primary-description="Các hàng điểm danh dùng bề mặt solid để trạng thái và tỷ lệ luôn dễ đọc."
    timeline-title="Cần chú ý"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem lịch học', to: '/student/schedule' }, { label: 'Tạo đơn giải trình', to: '/student/requests', primary: true }]"
  />
</template>
