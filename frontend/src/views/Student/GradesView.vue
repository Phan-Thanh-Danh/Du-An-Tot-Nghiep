<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'GPA tích lũy', value: '3.42', unit: '/4.0', icon: 'TrendingUp', tone: 'blue', progress: 86, hint: 'Xếp loại Giỏi' },
  { label: 'Môn đã đạt', value: '24', unit: 'môn', icon: 'CheckCircle2', tone: 'green', progress: 76, hint: 'Không có môn rớt trong demo' },
  { label: 'Tín chỉ', value: '86', unit: '/120', icon: 'BadgeCheck', tone: 'teal', progress: 72, hint: 'Theo chương trình dự kiến' },
  { label: 'Cần rà soát', value: '1', unit: 'điểm', icon: 'AlertTriangle', tone: 'amber', progress: 18, hint: 'Có thể gửi yêu cầu sửa điểm' },
]

const rows = [
  { title: 'Cấu trúc dữ liệu & Giải thuật', description: 'Điểm quá trình 8.0, giữa kỳ 8.5, cuối kỳ dự kiến.', badge: 'Đang học', tone: 'blue', icon: 'BookOpen', meta: ['CTDL101', '3 tín chỉ', 'Kỳ 2'], value: '8.2', valueHint: 'Tạm tính' },
  { title: 'Lập trình Web', description: 'Đã công bố điểm giữa kỳ và bài tập lớn trong FE demo.', badge: 'Tốt', tone: 'green', icon: 'Code2', meta: ['LTW301', '4 tín chỉ', 'Có bài tập lớn'], value: '9.0', valueHint: 'Đã công bố' },
  { title: 'Toán rời rạc', description: 'Chờ điểm giữa kỳ từ giảng viên phụ trách.', badge: 'Chờ điểm', tone: 'amber', icon: 'Sigma', meta: ['TRR201', '3 tín chỉ', 'Thi giữa kỳ'], value: 'cần bổ sung', valueHint: 'API grades' },
  { title: 'Hệ quản trị CSDL', description: 'Môn học đã hoàn thành, chưa có yêu cầu sửa điểm.', badge: 'Hoàn thành', tone: 'teal', icon: 'Database', meta: ['HQTCSDL401', '3 tín chỉ'], value: '8.6', valueHint: 'Đạt' },
]

const timeline = [
  { title: 'Bảng điểm ưu tiên solid', description: 'Dữ liệu điểm không dùng surface quá trong suốt để giữ readability.', time: 'Design rule', tone: 'blue' },
  { title: 'Yêu cầu sửa điểm', description: 'Luồng này là dự kiến, cần API grade-change-request.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Không tự bịa contract', description: 'Frontend chỉ trình bày demo cho đến khi backend có controller.', time: 'API rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="BarChart2"
    eyebrow="Kết quả học tập"
    title="Bảng điểm"
    subtitle="Module điểm số hiện là UI dự kiến; frontend không tự bịa dữ liệu API."
    primary-title="Điểm theo môn học"
    primary-description="Bảng thông tin được tổ chức dạng solid card để dễ đối chiếu điểm, tín chỉ và trạng thái."
    timeline-title="Quy tắc điểm số"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem khóa học', to: '/student/courses' }, { label: 'Gửi yêu cầu', to: '/student/requests', primary: true }]"
  />
</template>
