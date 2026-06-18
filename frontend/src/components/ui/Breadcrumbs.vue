<script setup>
 
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const labelMap = {
  dashboard: 'Dashboard',
  courses: 'Khóa học',
  assignments: 'Bài tập',
  exams: 'Thi / Kiểm tra',
  grades: 'Bảng điểm',
  schedule: 'Thời khóa biểu',
  attendance: 'Điểm danh',
  registrations: 'Đăng ký',
  tuition: 'Học phí',
  profile: 'Hồ sơ',
  notifications: 'Thông báo',
  support: 'Hỗ trợ',
  tickets: 'Ticket',
  requests: 'Đơn từ',
  evaluations: 'Đánh giá',
  classes: 'Lớp học',
  lessons: 'Bài giảng',
  grading: 'Chấm bài',
  'class-progress': 'Tiến độ lớp',
  'class-attendance': 'Điểm danh lớp',
  'class-grades': 'Điểm lớp',
  'student-questions': 'Câu hỏi SV',
  'lesson-comments': 'Nhận xét',
  'exam-results': 'Kết quả thi',
  proctoring: 'Giám thị',
  'attendance-history': 'Lịch sử điểm danh',
  'grading-input': 'Nhập điểm',
  'change-password': 'Đổi mật khẩu',
  rooms: 'Phòng học',
  conflicts: 'Xung đột',
  sections: 'Lớp học phần',
  waitlist: 'Hàng chờ',
  capacity: 'Sức chứa',
  'course-status': 'Trạng thái lớp',
  workflow: 'Quy trình',
  notices: 'Thông báo',
  send: 'Gửi',
  history: 'Lịch sử',
  pending: 'Chờ duyệt',
  published: 'Đã công bố',
  overview: 'Tổng quan',
  'pass-fail': 'Đỗ / Trượt',
  gpa: 'GPA',
  'at-risk': 'Cảnh báo',
  reports: 'Báo cáo',
  ranking: 'Xếp hạng',
  details: 'Chi tiết',
  'ai-feedback': 'AI Feedback',
  strategic: 'Chiến lược',
  semesters: 'Học kỳ',
  campuses: 'Cơ sở',
  // BGH routes
  bgh: 'Ban Giám Hiệu',
  organizations: 'Đơn vị',
  users: 'Người dùng',
  roles: 'Vai trò',
  'academic-programs': 'Chương trình',
  curriculum: 'Khung chương trình',
  'academic-terms': 'Học kỳ',
  'audit-logs': 'Kiểm toán',
  facilities: 'Cơ sở vật chất',
}

const crumbs = computed(() => {
  const parts = route.path.split('/').filter(Boolean)
  const result = []
  let current = ''
  for (const part of parts) {
    current += '/' + part
    const label = labelMap[part] || part.charAt(0).toUpperCase() + part.slice(1)
    result.push({ label, path: current })
  }
  return result
})
</script>

<template>
  <nav aria-label="Breadcrumb" class="flex items-center gap-1.5 text-[11px] font-semibold text-muted">
    <router-link to="/" class="text-link hover:text-heading transition-colors">
      Trang chủ
    </router-link>
    <template v-for="(crumb, i) in crumbs" :key="crumb.path">
      <svg viewBox="0 0 24 24" class="h-3 w-3 flex-shrink-0 fill-current">
        <path d="M9 18l6-6-6-6" />
      </svg>
      <router-link
        v-if="i < crumbs.length - 1"
        :to="crumb.path"
        class="text-link hover:text-heading transition-colors truncate max-w-[120px]"
      >
        {{ crumb.label }}
      </router-link>
      <span v-else class="truncate max-w-[160px] text-heading">
        {{ crumb.label }}
      </span>
    </template>
  </nav>
</template>
