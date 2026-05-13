<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Đang xử lý', value: '2', unit: 'đơn', icon: 'Hourglass', tone: 'amber', progress: 34, hint: 'Chờ phòng đào tạo' },
  { label: 'Đã duyệt', value: '8', unit: 'đơn', icon: 'CheckCircle2', tone: 'green', progress: 78, hint: 'Trong năm học' },
  { label: 'Mẫu đơn', value: '5', unit: 'loại', icon: 'Files', tone: 'blue', progress: 60, hint: 'Dữ liệu FE demo' },
  { label: 'Cần bổ sung', value: 'API', icon: 'PlugZap', tone: 'violet', progress: 15, hint: 'Workflow approval' },
]

const rows = [
  { title: 'Đơn xin xác nhận sinh viên', description: 'Mẫu đơn phục vụ xác nhận đang học tại trường.', badge: 'Đang xử lý', tone: 'amber', icon: 'FileText', meta: ['Học vụ', 'Gửi 1 ngày trước'], value: '#DT-2025-12', valueHint: 'Mã đơn' },
  { title: 'Đơn xin nghỉ có phép', description: 'Xin phép vắng buổi Toán rời rạc vì lý do cá nhân.', badge: 'Đã duyệt', tone: 'green', icon: 'FileCheck2', meta: ['Điểm danh', 'Đã duyệt'], value: '#DT-2025-08', valueHint: 'Mã đơn' },
  { title: 'Yêu cầu sửa điểm', description: 'Luồng cần đối chiếu audit và xác nhận của giảng viên.', badge: 'Dự kiến', tone: 'blue', icon: 'PencilLine', meta: ['Điểm số', 'Cần API'], value: 'cần bổ sung', valueHint: 'Workflow' },
]

const timeline = [
  { title: 'Tiếp nhận', description: 'Sinh viên gửi đơn, hệ thống ghi nhận trạng thái ban đầu.', time: 'Bước 1', tone: 'blue' },
  { title: 'Duyệt học vụ', description: 'Cần API workflow và log duyệt trước khi triển khai thật.', time: 'Bước 2', tone: 'amber' },
  { title: 'Thông báo kết quả', description: 'Kết quả duyệt sẽ nối notification khi backend sẵn sàng.', time: 'Bước 3', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="FileText"
    eyebrow="Hỗ trợ"
    title="Đơn từ"
    subtitle="Module đơn từ đang chờ API, hiện chỉ chuẩn hóa khung FE và navigation."
    primary-title="Đơn từ gần đây"
    primary-description="Các đơn được trình bày theo trạng thái xử lý, loại nghiệp vụ và mã theo dõi."
    timeline-title="Workflow dự kiến"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Ticket hỗ trợ', to: '/student/support-tickets' }, { label: 'Tạo đơn mới', to: '/student/requests', primary: true }]"
  />
</template>
