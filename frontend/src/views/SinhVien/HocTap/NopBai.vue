<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentSubmissionFallbackPage',
})

const metrics = [
  { label: 'Bài đang mở', value: '2', icon: 'UploadCloud', tone: 'blue', progress: 50, hint: 'FE demo' },
  { label: 'Đã nộp', value: '12', icon: 'CheckCircle2', tone: 'green', progress: 85, hint: 'Trong kỳ' },
  { label: 'Quá hạn', value: '0', icon: 'AlertTriangle', tone: 'teal', progress: 100, hint: 'Không cảnh báo' },
]

const rows = [
  { title: 'Khu vực nộp bài', description: 'Upload area ở route detail dùng dashed glass style khi có bài cụ thể.', badge: 'Submission', tone: 'blue', icon: 'UploadCloud', meta: ['File', 'Rubric'], value: 'Sẵn sàng', valueHint: 'UI' },
  { title: 'Rubric và hướng dẫn', description: 'Nội dung hướng dẫn dùng solid-soft để đọc rõ hơn nền trong suốt.', badge: 'Guide', tone: 'teal', icon: 'ClipboardCheck', meta: ['Rubric', 'Deadline'], value: 'Đọc', valueHint: 'trước khi nộp' },
]

const timeline = [
  { title: 'Không tự tạo backend upload', description: 'Chỉ dựng UI, chờ API upload/submission thật.', time: 'FE only', tone: 'amber' },
  { title: 'Điều hướng detail', description: 'Route chính cho bài cụ thể là /student/assignments/:assignmentId.', time: 'Router', tone: 'blue' },
]
</script>

<template>
  <StudentModulePage
    icon="UploadCloud"
    eyebrow="Học tập"
    title="Nộp bài"
    subtitle="Màn dự phòng cho luồng nộp bài, giữ UI Liquid Glass rõ và dễ đọc."
    primary-title="Submission area"
    primary-description="Không gọi API upload; chỉ chuẩn hóa layout và trạng thái FE."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở bài tập', to: '/student/assignments', primary: true }]"
  />
</template>
