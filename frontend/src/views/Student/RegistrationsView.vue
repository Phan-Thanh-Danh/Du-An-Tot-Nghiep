<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Tín chỉ đã chọn', value: '12', unit: '/18', icon: 'ListChecks', tone: 'blue', progress: 67, hint: 'Dữ liệu FE demo' },
  { label: 'Lớp khả dụng', value: '8', unit: 'lớp', icon: 'School', tone: 'teal', progress: 58, hint: 'Chờ API lớp học phần' },
  { label: 'Môn tiên quyết', value: '1', unit: 'cần kiểm', icon: 'GitBranch', tone: 'amber', progress: 18, hint: 'Cần backend validate' },
  { label: 'Thời gian', value: '3', unit: 'ngày', icon: 'Hourglass', tone: 'violet', progress: 35, hint: 'Còn lại trong đợt demo' },
]

const rows = [
  { title: 'Mạng máy tính cơ bản', description: 'Lớp học phần còn chỗ, lịch thực hành phù hợp.', badge: 'Khả dụng', tone: 'green', icon: 'Network', meta: ['MMT501', '3 tín chỉ', 'Thứ 3 - P.205'], value: '32/45', valueHint: 'Sĩ số' },
  { title: 'Trí tuệ nhân tạo nhập môn', description: 'Môn yêu cầu hoàn thành xác suất thống kê.', badge: 'Cần kiểm', tone: 'amber', icon: 'BrainCircuit', meta: ['AI101', '3 tín chỉ', 'Tiên quyết'], value: 'dự kiến', valueHint: 'API validate' },
  { title: 'Phân tích thiết kế hệ thống', description: 'Lớp lý thuyết, phù hợp lịch hiện tại.', badge: 'Đề xuất', tone: 'blue', icon: 'Workflow', meta: ['PTTK301', '3 tín chỉ', 'Thứ 5'], value: '28/40', valueHint: 'Sĩ số' },
]

const timeline = [
  { title: 'Mở đăng ký', description: 'Đợt đăng ký đang ở trạng thái demo frontend.', time: 'dự kiến', tone: 'blue' },
  { title: 'Kiểm tra điều kiện', description: 'Backend cần kiểm tra tiên quyết, trùng lịch và tín chỉ tối đa.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Xác nhận đăng ký', description: 'Không ghi dữ liệu thật khi chưa có API registrations.', time: 'API rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="ListPlus"
    eyebrow="Đăng ký"
    title="Đăng ký môn"
    subtitle="Module đăng ký môn đang là UI dự kiến, chưa gọi endpoint registration."
    primary-title="Lớp học phần gợi ý"
    primary-description="Bố cục mô phỏng lựa chọn lớp, điều kiện và sĩ số nhưng chưa thực hiện thao tác ghi dữ liệu."
    timeline-title="Quy trình đăng ký"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem thời khóa biểu', to: '/student/schedule' }, { label: 'Gửi yêu cầu học vụ', to: '/student/requests', primary: true }]"
  />
</template>
