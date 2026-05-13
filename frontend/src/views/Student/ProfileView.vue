<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Hồ sơ', value: '80', unit: '%', icon: 'UserCircle', tone: 'blue', progress: 80, hint: 'Thiếu thông tin liên hệ phụ' },
  { label: 'Bảo mật', value: 'Tốt', icon: 'ShieldCheck', tone: 'green', progress: 88, hint: 'Token mock FE khi demo' },
  { label: 'Cơ sở', value: 'CS1', icon: 'Building2', tone: 'teal', progress: 100, hint: 'Theo user mock/store' },
  { label: 'Thông báo', value: '3', unit: 'kênh', icon: 'Bell', tone: 'violet', progress: 50, hint: 'Dự kiến preferences' },
]

const rows = [
  { title: 'Sinh Viên Demo', description: 'Tài khoản mock FE phục vụ kiểm thử giao diện student.', badge: 'Mock FE', tone: 'blue', icon: 'UserCircle', meta: ['student@mock.local', 'Role Student', 'Campus 1'], value: 'Active', valueHint: 'Trạng thái' },
  { title: 'Đổi mật khẩu', description: 'Backend đã có API change-password, frontend cần màn riêng để nối thật.', badge: 'Có API', tone: 'green', icon: 'KeyRound', meta: ['POST /api/auth/change-password', 'JWT'], value: 'có thể nối', valueHint: 'Auth flow' },
  { title: 'Tùy chọn thông báo', description: 'Các cấu hình nhận thông báo hiện là dự kiến.', badge: 'Cần bổ sung', tone: 'amber', icon: 'BellRing', meta: ['Email', 'Web', 'Mobile'], value: 'dự kiến', valueHint: 'API' },
]

const timeline = [
  { title: 'Không hardcode role', description: 'Component lấy role/displayName từ auth store khi có.', time: 'FE rule', tone: 'blue' },
  { title: 'Đổi mật khẩu', description: 'Có thể nối API hiện có trong backend ở task riêng.', time: 'đã có API', tone: 'green' },
  { title: 'Thông tin cá nhân', description: 'Cần API user/me trước khi chỉnh sửa hồ sơ thật.', time: 'cần bổ sung', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="UserCircle"
    eyebrow="Cá nhân"
    title="Hồ sơ cá nhân"
    subtitle="Trang hồ sơ hiện giữ phần trình bày FE đồng nhất, chưa lưu dữ liệu mới."
    primary-title="Thông tin tài khoản"
    primary-description="Bố cục hồ sơ tách rõ thông tin định danh, bảo mật và cấu hình cá nhân."
    timeline-title="Lưu ý hồ sơ"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Liên kết phụ huynh', to: '/student/parent-links' }, { label: 'Đơn từ cá nhân', to: '/student/requests', primary: true }]"
  />
</template>
