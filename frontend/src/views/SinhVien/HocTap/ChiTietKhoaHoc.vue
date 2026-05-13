<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

defineOptions({
  name: 'StudentCourseDetailFallbackPage',
})

const metrics = [
  { label: 'Tiến độ', value: '72', unit: '%', icon: 'TrendingUp', tone: 'blue', progress: 72, hint: 'FE demo' },
  { label: 'Buổi học', value: '11/15', icon: 'BookOpenCheck', tone: 'teal', progress: 73, hint: 'Đã hoàn thành' },
  { label: 'Tài liệu', value: '8', icon: 'FolderOpen', tone: 'violet', progress: 55, hint: 'Bài giảng' },
]

const rows = [
  { title: 'Tổng quan môn học', description: 'Hero course detail glass, tiến độ và CTA học tiếp được chuẩn hóa qua route chính.', badge: 'Detail', tone: 'blue', icon: 'PanelTop', meta: ['Course detail', 'Progress'], value: 'UI', valueHint: 'fallback', to: '/student/courses/CTDL101' },
  { title: 'Danh sách bài học', description: 'Lesson item dùng surface solid-soft để đọc tốt trên nền glass.', badge: 'Lessons', tone: 'teal', icon: 'ListChecks', meta: ['Chapter', 'Lesson'], value: '12', valueHint: 'mục' },
]

const timeline = [
  { title: 'Mở route chính', description: 'Router hiện dùng Student/CourseDetailView.vue cho /student/courses/:courseId.', time: 'Hiện tại', tone: 'blue' },
  { title: 'Không tạo endpoint mới', description: 'Placeholder này chỉ đảm bảo style nếu file được gọi.', time: 'FE only', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="BookOpen"
    eyebrow="Học tập"
    title="Chi tiết khóa học"
    subtitle="Màn dự phòng cho chi tiết khóa học trong thư mục SinhVien."
    primary-title="Nội dung khóa học"
    primary-description="Đảm bảo các lesson/progress block giữ cùng ngôn ngữ Liquid Glass."
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Mở khóa học mẫu', to: '/student/courses/CTDL101', primary: true }]"
  />
</template>
