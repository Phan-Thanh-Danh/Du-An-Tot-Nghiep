import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// ─────────────────────────────────────────────────────────
// Router config cho hệ thống LMS
// Convention: kebab-case paths, :id / :courseId / :studentId
// ─────────────────────────────────────────────────────────

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // ── Public ────────────────────────────────────────────
    {
      path: '/',
      name: 'home',
      redirect: () => {
        const authStore = useAuthStore()
        if (authStore.hasRole('Principal')) return '/bgh/dashboard'
        if (authStore.hasRole('Teacher')) return '/teacher/dashboard'
        if (authStore.hasRole('AcademicStaff')) return '/staff/dashboard'
        return '/student/dashboard'
      },
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { public: true },
    },

    // ── Student Layout (App Shell) ─────────────────────────
    {
      path: '/student',
      component: () => import('../components/SinhVien/Layout_SinhVien.vue'),
      meta: { requiresAuth: true, role: 'student' },
      children: [
        { path: '', redirect: '/student/dashboard' },
        {
          path: 'dashboard',
          name: 'student-dashboard',
          component: () => import('../views/SinhVien/Dashboard.vue'),
          meta: { title: 'Dashboard' },
        },
        {
          path: 'courses',
          name: 'student-courses',
          component: () => import('../views/SinhVien/HocTap/KhoacHoc.vue'),
          meta: { title: 'Khóa học' },
        },
        {
          path: 'courses/:courseId',
          name: 'student-course-detail',
          component: () => import('../views/Student/CourseDetailView.vue'),
          meta: { title: 'Chi tiết khóa học' },
        },
        {
          path: 'assignments',
          name: 'student-assignments',
          component: () => import('../views/Student/AssignmentsView.vue'),
          meta: { title: 'Bài tập' },
        },
        {
          path: 'assignments/:assignmentId',
          name: 'student-assignment-detail',
          component: () => import('../views/Student/AssignmentDetailView.vue'),
          meta: { title: 'Chi tiết bài tập' },
        },
        {
          path: 'exams',
          name: 'student-exams',
          component: () => import('../views/Student/ExamsView.vue'),
          meta: { title: 'Thi / Kiểm tra' },
        },
        {
          path: 'exams/:examResultId',
          name: 'student-exam-result',
          component: () => import('../views/Student/ExamResultView.vue'),
          meta: { title: 'Kết quả kiểm tra' },
        },
        {
          path: 'grades',
          name: 'student-grades',
          component: () => import('../views/Student/GradesView.vue'),
          meta: { title: 'Bảng điểm' },
        },
        {
          path: 'schedule',
          name: 'student-schedule',
          component: () => import('../views/Student/ScheduleView.vue'),
          meta: { title: 'Thời khóa biểu' },
        },
        {
          path: 'attendance',
          name: 'student-attendance',
          component: () => import('../views/Student/AttendanceView.vue'),
          meta: { title: 'Điểm danh' },
        },
        {
          path: 'registrations',
          name: 'student-registrations',
          component: () => import('../views/Student/RegistrationsView.vue'),
          meta: { title: 'Đăng ký môn' },
        },
        {
          path: 'tuition',
          name: 'student-tuition',
          component: () => import('../views/Student/TuitionView.vue'),
          meta: { title: 'Học phí & Thanh toán' },
        },
        {
          path: 'support-tickets',
          name: 'student-support-tickets',
          component: () => import('../views/Student/SupportTicketsView.vue'),
          meta: { title: 'Hỗ trợ & Ticket' },
        },
        {
          path: 'requests',
          name: 'student-requests',
          component: () => import('../views/Student/RequestsView.vue'),
          meta: { title: 'Đơn từ' },
        },
        {
          path: 'evaluations',
          name: 'student-evaluations',
          component: () => import('../views/Student/EvaluationsView.vue'),
          meta: { title: 'Đánh giá giảng viên' },
        },
        {
          path: 'profile',
          name: 'student-profile',
          component: () => import('../views/Student/ProfileView.vue'),
          meta: { title: 'Hồ sơ cá nhân' },
        },
        {
          path: 'parent-links',
          name: 'student-parent-links',
          component: () => import('../views/Student/ParentLinksView.vue'),
          meta: { title: 'Liên kết phụ huynh' },
        },
      ],
    },

    // ── Teacher Layout (App Shell) ─────────────────────────
    {
      path: '/teacher',
      component: () => import('../components/GiangVien/Layout_GiangVien.vue'),
      meta: { requiresAuth: true, role: 'Teacher' },
      children: [
        { path: '', redirect: '/teacher/dashboard' },
        {
          path: 'dashboard',
          name: 'teacher-dashboard',
          component: () => import('../views/GiangVien/Dashboard.vue'),
          meta: { title: 'Tổng quan giảng dạy' },
        },
        // Chi tiết chức năng Giảng viên (Placeholder components)
        { path: 'courses', name: 'teacher-courses', component: () => import('../views/GiangVien/CoursesView.vue') },
        { path: 'lessons', name: 'teacher-lessons', component: () => import('../views/GiangVien/LessonsView.vue') },
        { path: 'classes', name: 'teacher-classes', component: () => import('../views/GiangVien/ClassListView.vue') },
        { path: 'class-progress', name: 'teacher-class-progress', component: () => import('../views/GiangVien/ClassProgressView.vue') },
        { path: 'class-attendance', name: 'teacher-class-attendance', component: () => import('../views/GiangVien/ClassAttendanceView.vue') },
        { path: 'class-grades', name: 'teacher-class-grades', component: () => import('../views/GiangVien/ClassGradebookView.vue') },
        { path: 'assignments', name: 'teacher-assignments', component: () => import('../views/GiangVien/AssignmentsListView.vue') },
        { path: 'grading', name: 'teacher-grading', component: () => import('../views/GiangVien/GradingView.vue') },
        { path: 'questions', name: 'teacher-questions', component: () => import('../views/GiangVien/QuestionBankView.vue') },
        { path: 'exams', name: 'teacher-exams', component: () => import('../views/GiangVien/ExamsView.vue') },
        { path: 'exam-results', name: 'teacher-exam-results', component: () => import('../views/GiangVien/ExamResultsView.vue') },
        { path: 'proctoring', name: 'teacher-proctoring', component: () => import('../views/GiangVien/ProctoringView.vue') },
        { path: 'attendance', name: 'teacher-attendance-today', component: () => import('../views/GiangVien/AttendanceTodayView.vue') },
        { path: 'attendance-history', name: 'teacher-attendance-history', component: () => import('../views/GiangVien/AttendanceHistoryView.vue') },
        { path: 'grading-input', name: 'teacher-grading-input', component: () => import('../views/GiangVien/ClassGradesView.vue') },
        { path: 'student-questions', name: 'teacher-student-questions', component: () => import('../views/GiangVien/StudentQuestionsView.vue') },
        { path: 'lesson-comments', name: 'teacher-lesson-comments', component: () => import('../views/GiangVien/LessonCommentsView.vue') },
        { path: 'requests', name: 'teacher-requests', component: () => import('../views/GiangVien/PendingRequestsView.vue') },
        { path: 'requests-history', name: 'teacher-requests-history', component: () => import('../views/GiangVien/RequestsHistoryView.vue') },
        { path: 'profile', name: 'teacher-profile', component: () => import('../views/GiangVien/ProfileView.vue') },
      ],
    },

    // ── Staff Layout (Giáo vụ) ─────────────────────────
    {
      path: '/staff',
      component: () => import('../components/GiaoVu/Layout_GiaoVu.vue'),
      meta: { requiresAuth: true, role: 'AcademicStaff' },
      children: [
        { path: '', redirect: '/staff/dashboard' },
        {
          path: 'dashboard',
          name: 'staff-dashboard',
          component: () => import('../views/GiaoVu/Dashboard.vue'),
          meta: { title: 'Tổng quan giáo vụ' },
        },
        { path: 'schedule', name: 'staff-schedule', component: () => import('../views/GiaoVu/Schedule/ScheduleManagerView.vue') },
        { path: 'assignments', name: 'staff-assignments', component: () => import('../views/GiaoVu/Schedule/TeacherAssignmentView.vue') },
        { path: 'rooms', name: 'staff-rooms', component: () => import('../views/GiaoVu/Schedule/RoomManagementView.vue') },
        { path: 'conflicts', name: 'staff-conflicts', component: () => import('../views/GiaoVu/Schedule/ConflictCheckView.vue') },
        { path: 'schedule/pending', name: 'staff-schedule-pending', component: () => import('../views/GiaoVu/Schedule/PendingSchedulesView.vue') },
        { path: 'schedule/published', name: 'staff-schedule-published', component: () => import('../views/GiaoVu/Schedule/PublishedSchedulesView.vue') },
        { path: 'registrations', name: 'staff-registrations', component: () => import('../views/GiaoVu/Registration/RegistrationPeriodsView.vue') },
        { path: 'sections', name: 'staff-sections', component: () => import('../views/GiaoVu/Registration/SectionClassesView.vue') },
        { path: 'registration-list', name: 'staff-registration-list', component: () => import('../views/GiaoVu/Registration/RegListView.vue') },
        { path: 'waitlist', name: 'staff-waitlist', component: () => import('../views/GiaoVu/Registration/WaitView.vue') },
        { path: 'capacity', name: 'staff-capacity', component: () => import('../views/GiaoVu/Registration/CapacityAdjustmentView.vue') },
        { path: 'course-status', name: 'staff-course-status', component: () => import('../views/GiaoVu/Registration/CourseStatusView.vue') },
        { path: 'requests', name: 'staff-requests', component: () => import('../views/GiaoVu/Requests/PendingRequestsView.vue') },
        { path: 'requests/:id', name: 'staff-request-details', component: () => import('../views/GiaoVu/Requests/RequestDetailsView.vue') },
        { path: 'requests-history', name: 'staff-requests-history', component: () => import('../views/GiaoVu/Requests/RequestHistoryView.vue') },
        { path: 'workflow', name: 'staff-workflow', component: () => import('../views/GiaoVu/Requests/WorkflowConfigView.vue') },
        { path: 'notices/send', name: 'staff-notices-send', component: () => import('../views/GiaoVu/Notices/SendNoticeView.vue') },
        { path: 'notices/history', name: 'staff-notices-history', component: () => import('../views/GiaoVu/Notices/NoticeHistoryView.vue') },
        { path: 'profile', name: 'staff-profile', component: () => import('../views/GiaoVu/Profile/StaffProfileView.vue') },
      ],
    },

    // ── BGH Layout (Hiệu trưởng/Ban Giám Hiệu) ────────────────
    {
      path: '/bgh',
      component: () => import('../components/BGH/Layout_BGH.vue'),
      meta: { requiresAuth: true, role: 'Principal' },
      children: [
        { path: '', redirect: '/bgh/dashboard' },
        {
          path: 'dashboard',
          name: 'bgh-dashboard',
          component: () => import('../views/BGH/Dashboard.vue'),
          meta: { title: 'Dashboard Chiến Lược' },
        },
        // TKB
        { path: 'schedule/pending', name: 'bgh-schedule-pending', component: () => import('../views/BGH/Schedule/PendingSchedulesView.vue') },
        { path: 'schedule/approved', name: 'bgh-schedule-approved', component: () => import('../views/BGH/Schedule/PublishedSchedulesView.vue') },
        { path: 'schedule/conflicts', name: 'bgh-schedule-conflicts', component: () => import('../views/BGH/Schedule/ConflictListView.vue') },
        { path: 'schedule/changes', name: 'bgh-schedule-changes', component: () => import('../views/BGH/Schedule/ScheduleChangesView.vue') },
        // Academic
        { path: 'academic/overview', name: 'bgh-academic-overview', component: () => import('../views/BGH/Academic/AcademicOverviewView.vue') },
        { path: 'academic/pass-fail', name: 'bgh-academic-pass-fail', component: () => import('../views/BGH/Academic/PassFailRatesView.vue') },
        { path: 'academic/gpa', name: 'bgh-academic-gpa', component: () => import('../views/BGH/Academic/GPAReportsView.vue') },
        { path: 'academic/at-risk', name: 'bgh-academic-at-risk', component: () => import('../views/BGH/Academic/AtRiskStudentsView.vue') },
        { path: 'academic/reports', name: 'bgh-academic-reports', component: () => import('../views/BGH/Academic/AcademicReportsView.vue') },
        // Evaluations
        { path: 'evaluations/overview', name: 'bgh-evaluations-overview', component: () => import('../views/BGH/Evaluations/EvalOverviewView.vue') },
        { path: 'evaluations/ranking', name: 'bgh-evaluations-ranking', component: () => import('../views/BGH/Evaluations/TeacherRankingView.vue') },
        { path: 'evaluations/details', name: 'bgh-evaluations-details', component: () => import('../views/BGH/Evaluations/TeacherEvalDetailsView.vue') },
        { path: 'evaluations/ai-feedback', name: 'bgh-evaluations-ai-feedback', component: () => import('../views/BGH/Evaluations/AIFeedbackAnalysisView.vue') },
        // Strategic
        { path: 'strategic/dashboard', name: 'bgh-strategic-dashboard', component: () => import('../views/BGH/PlaceholderView.vue') },
        { path: 'strategic/semesters', name: 'bgh-strategic-semesters', component: () => import('../views/BGH/PlaceholderView.vue') },
        { path: 'strategic/campuses', name: 'bgh-strategic-campuses', component: () => import('../views/BGH/PlaceholderView.vue') },
        // Profile
        { path: 'profile', name: 'bgh-profile', component: () => import('../views/BGH/PlaceholderView.vue') },
      ],
    },

    // ── 404 ───────────────────────────────────────────────
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue'),
    },
  ],

  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0, behavior: 'smooth' }
  },
})

// ── Navigation Guard ────────────────────────────────────────
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  authStore.ensureFreshSession()

  // Chuyển hướng nếu truy cập trang public khi đã login
  if (to.meta.public && authStore.isAuthenticated) {
    if (authStore.hasRole('Teacher')) return next('/teacher/dashboard')
    if (authStore.hasRole('AcademicStaff')) return next('/staff/dashboard')
    return next('/student/dashboard')
  }

  // Yêu cầu login
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({
      path: '/login',
      query: { redirect: to.fullPath },
    })
  }

  // Kiểm tra quyền (Role)
  if (to.meta.role && !authStore.hasRole(to.meta.role)) {
    return next({ name: 'not-found' })
  }

  next()
})

export default router
