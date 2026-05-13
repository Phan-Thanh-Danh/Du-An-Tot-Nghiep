<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Liên kết', value: '1', unit: 'người', icon: 'Users', tone: 'blue', progress: 50, hint: 'Dữ liệu FE demo' },
  { label: 'Quyền xem', value: '3', unit: 'mục', icon: 'Eye', tone: 'teal', progress: 60, hint: 'Điểm, lịch, học phí' },
  { label: 'Chờ xác nhận', value: '0', unit: 'lời mời', icon: 'MailCheck', tone: 'green', progress: 100, hint: 'Không có pending' },
  { label: 'Cần API', value: 'Parent', icon: 'PlugZap', tone: 'amber', progress: 10, hint: 'Liên kết phụ huynh' },
]

const rows = [
  { title: 'Phụ huynh demo', description: 'Người giám hộ được mô phỏng trong frontend, chưa lấy từ backend.', badge: 'Đã liên kết', tone: 'green', icon: 'UserRoundCheck', meta: ['parent@mock.local', 'Xem điểm', 'Xem lịch'], value: 'Active', valueHint: 'Trạng thái' },
  { title: 'Quyền theo dõi học tập', description: 'Hiển thị nhóm quyền dự kiến để chuẩn bị cho parent portal.', badge: 'Dự kiến', tone: 'blue', icon: 'ShieldCheck', meta: ['Bảng điểm', 'Điểm danh', 'Thông báo'], value: '3 quyền', valueHint: 'Demo' },
  { title: 'Mời phụ huynh mới', description: 'Luồng này cần API invite/verify riêng và kiểm tra bảo mật.', badge: 'Cần bổ sung', tone: 'amber', icon: 'Send', meta: ['OTP/email', 'Xác minh'], value: 'API', valueHint: 'chưa có' },
]

const timeline = [
  { title: 'Bảo mật dữ liệu', description: 'Không mở dữ liệu điểm/lịch cho phụ huynh khi chưa có phân quyền backend.', time: 'Security rule', tone: 'amber' },
  { title: 'Liên kết hiện tại', description: 'Dữ liệu hiển thị chỉ là FE demo.', time: 'Mock FE', tone: 'blue' },
  { title: 'Parent portal', description: 'Có thể mở route riêng khi backend và role Parent hoàn thiện.', time: 'dự kiến', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="Users"
    eyebrow="Cá nhân"
    title="Liên kết phụ huynh"
    subtitle="Trang liên kết phụ huynh đang ở trạng thái cần bổ sung API."
    primary-title="Quyền truy cập phụ huynh"
    primary-description="Màn này ưu tiên bảo mật và minh bạch quyền xem dữ liệu học tập."
    timeline-title="Bảo mật & xác minh"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Hồ sơ cá nhân', to: '/student/profile' }, { label: 'Tạo yêu cầu', to: '/student/requests', primary: true }]"
  />
</template>
