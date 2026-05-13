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
        { path: 'courses', name: 'teacher-courses', component: { render: () => null } },
        { path: 'lessons', name: 'teacher-lessons', component: { render: () => null } },
        { path: 'classes', name: 'teacher-classes', component: { render: () => null } },
        { path: 'class-progress', name: 'teacher-class-progress', component: { render: () => null } },
        { path: 'class-attendance', name: 'teacher-class-attendance', component: { render: () => null } },
        { path: 'class-grades', name: 'teacher-class-grades', component: { render: () => null } },
        { path: 'assignments', name: 'teacher-assignments', component: { render: () => null } },
        { path: 'grading', name: 'teacher-grading', component: { render: () => null } },
        { path: 'questions', name: 'teacher-questions', component: { render: () => null } },
        { path: 'exams', name: 'teacher-exams', component: { render: () => null } },
        { path: 'exam-results', name: 'teacher-exam-results', component: { render: () => null } },
        { path: 'proctoring', name: 'teacher-proctoring', component: { render: () => null } },
        { path: 'attendance', name: 'teacher-attendance-today', component: { render: () => null } },
        { path: 'grading-input', name: 'teacher-grading-input', component: { render: () => null } },
        { path: 'student-questions', name: 'teacher-student-questions', component: { render: () => null } },
        { path: 'lesson-comments', name: 'teacher-lesson-comments', component: { render: () => null } },
        { path: 'requests', name: 'teacher-requests', component: { render: () => null } },
        { path: 'requests-history', name: 'teacher-requests-history', component: { render: () => null } },
        { path: 'profile', name: 'teacher-profile', component: { render: () => null } },
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
          component: { render: () => null },
          meta: { title: 'Tổng quan giáo vụ' },
        },
        { path: 'schedule', name: 'staff-schedule', component: { render: () => null } },
        { path: 'assignments', name: 'staff-assignments', component: { render: () => null } },
        { path: 'rooms', name: 'staff-rooms', component: { render: () => null } },
        { path: 'conflicts', name: 'staff-conflicts', component: { render: () => null } },
        { path: 'registrations', name: 'staff-registrations', component: { render: () => null } },
        { path: 'sections', name: 'staff-sections', component: { render: () => null } },
        { path: 'registration-list', name: 'staff-registration-list', component: { render: () => null } },
        { path: 'requests', name: 'staff-requests', component: { render: () => null } },
        { path: 'requests-history', name: 'staff-requests-history', component: { render: () => null } },
        { path: 'workflow', name: 'staff-workflow', component: { render: () => null } },
        { path: 'notices/send', name: 'staff-notices-send', component: { render: () => null } },
        { path: 'notices/history', name: 'staff-notices-history', component: { render: () => null } },
        { path: 'profile', name: 'staff-profile', component: { render: () => null } },
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
