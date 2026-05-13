<script setup>
import { useRoute } from 'vue-router'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const route = useRoute()
const courseId = String(route.params.courseId || 'course')

const metrics = [
  { label: 'Tiến độ', value: '72', unit: '%', icon: 'Gauge', tone: 'blue', progress: 72, hint: 'Theo dữ liệu FE demo' },
  { label: 'Bài học', value: '9', unit: '/12', icon: 'BookOpenCheck', tone: 'green', progress: 75, hint: 'Đã hoàn thành' },
  { label: 'Bài tập', value: '2', unit: 'mục', icon: 'ClipboardList', tone: 'orange', progress: 40, hint: '1 bài gần hạn' },
  { label: 'Tài liệu', value: '18', unit: 'file', icon: 'Files', tone: 'violet', progress: 60, hint: 'PDF, video, quiz' },
]

const rows = [
  { title: 'Chương 1: Ôn tập nền tảng', description: 'Video, slide và quiz ngắn dùng solid surface để nội dung dài dễ đọc.', badge: 'Hoàn thành', tone: 'green', icon: 'CheckCircle2', meta: ['3 bài học', '1 quiz'], value: '100%', valueHint: 'Tiến độ' },
  { title: 'Chương 2: Cấu trúc dữ liệu tuyến tính', description: 'Danh sách, stack, queue và bài thực hành mô phỏng.', badge: 'Đang học', tone: 'blue', icon: 'ListTree', meta: ['4 bài học', '2 tài liệu'], value: '72%', valueHint: 'Tiến độ' },
  { title: 'Chương 3: Cây và đồ thị', description: 'Nội dung dự kiến sẽ nối lesson/progress API khi có controller.', badge: 'Dự kiến', tone: 'amber', icon: 'GitBranch', meta: ['5 bài học', 'Bài tập tuần 10'], value: 'cần bổ sung', valueHint: 'API lessons' },
]

const timeline = [
  { title: 'Tiếp tục chương đang học', description: 'Ưu tiên đưa sinh viên quay lại bài học gần nhất.', time: courseId, tone: 'blue' },
  { title: 'Lesson viewer', description: 'Nội dung bài học dài dùng surface solid, không dùng glass quá nhiều.', time: 'UI rule', tone: 'teal' },
  { title: 'Progress API', description: 'Cần API lesson/progress trước khi lưu tiến độ thật.', time: 'cần bổ sung', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="BookOpenCheck"
    eyebrow="Khóa học"
    :title="`Chi tiết khóa học ${courseId}`"
    subtitle="Màn chi tiết khóa học đang được giữ ở trạng thái UI dự kiến, chưa gọi API course/lesson."
    primary-title="Cấu trúc chương học"
    primary-description="Bố cục course detail tập trung vào tiến độ, chương học và hành động tiếp tục học."
    timeline-title="Lộ trình học"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Tất cả khóa học', to: '/student/courses' }, { label: 'Bài tập khóa học', to: '/student/assignments', primary: true }]"
  />
</template>
