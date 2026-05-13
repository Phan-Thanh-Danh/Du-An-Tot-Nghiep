<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Phiếu mở', value: '2', unit: 'môn', icon: 'Star', tone: 'blue', progress: 40, hint: 'Trong kỳ hiện tại' },
  { label: 'Đã gửi', value: '4', unit: 'phiếu', icon: 'SendHorizontal', tone: 'green', progress: 67, hint: 'Dữ liệu demo' },
  { label: 'Ẩn danh', value: 'Có', icon: 'ShieldCheck', tone: 'teal', progress: 100, hint: 'Dự kiến theo nghiệp vụ' },
  { label: 'AI', value: 'dự kiến', icon: 'Sparkles', tone: 'violet', progress: 15, hint: 'Chưa có controller AI' },
]

const rows = [
  { title: 'Đánh giá môn Lập trình Web', description: 'Phiếu đánh giá chất lượng giảng dạy và học liệu.', badge: 'Đang mở', tone: 'blue', icon: 'Star', meta: ['LTW301', '5 câu hỏi', 'Ẩn danh'], value: '5 phút', valueHint: 'Dự kiến' },
  { title: 'Đánh giá môn CSDL', description: 'Phiếu đã hoàn thành trong dữ liệu demo.', badge: 'Đã gửi', tone: 'green', icon: 'CheckCircle2', meta: ['HQTCSDL401', 'Hoàn thành'], value: 'Đã lưu', valueHint: 'FE demo' },
  { title: 'Phân tích cảm xúc AI', description: 'Trường dữ liệu có trong database nhưng chưa có API AI.', badge: 'Dự kiến', tone: 'violet', icon: 'Sparkles', meta: ['AiCamXuc', 'AiChuDe'], value: 'cần bổ sung', valueHint: 'Controller' },
]

const timeline = [
  { title: 'Mở phiếu', description: 'Sinh viên đánh giá sau khi môn học có đủ điều kiện.', time: 'Bước 1', tone: 'blue' },
  { title: 'Gửi phản hồi', description: 'Form thật cần validate và role alert cho lỗi.', time: 'Bước 2', tone: 'teal' },
  { title: 'Phân tích AI', description: 'Chỉ hiển thị khi backend có service/controller AI.', time: 'dự kiến', tone: 'violet' },
]
</script>

<template>
  <StudentModulePage
    icon="Star"
    eyebrow="Cá nhân"
    title="Đánh giá giảng viên"
    subtitle="Trang đánh giá giảng viên chỉ dựng UI dự kiến theo style Liquid Glass."
    primary-title="Phiếu đánh giá"
    primary-description="Bề mặt form và danh sách ưu tiên rõ ràng, tránh trang trí làm loãng nội dung phản hồi."
    timeline-title="Quy trình đánh giá"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Hồ sơ cá nhân', to: '/student/profile' }, { label: 'Hỗ trợ', to: '/student/support-tickets', primary: true }]"
  />
</template>
