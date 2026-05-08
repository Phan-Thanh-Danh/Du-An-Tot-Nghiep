import { createRouter, createWebHistory } from 'vue-router'

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
      redirect: '/student/dashboard',
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
        // Redirect /student → /student/dashboard
        {
          path: '',
          redirect: '/student/dashboard',
        },

        // ── Dashboard ────────────────────────────────────
        {
          path: 'dashboard',
          name: 'student-dashboard',
          component: () => import('../views/SinhVien/Dashboard.vue'),
          meta: { title: 'Dashboard' },
        },

        // ── Học tập ──────────────────────────────────────
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
          meta: { title: 'Chi tiết khóa học' },//jasdujasjdasd
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
          meta: { title: 'Kết quả bài thi' },
        },
        {
          path: 'grades',
          name: 'student-grades',
          component: () => import('../views/Student/GradesView.vue'),
          meta: { title: 'Bảng điểm' },
        },

        // ── Lịch học ─────────────────────────────────────
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

        // ── Đăng ký ──────────────────────────────────────
        {
          path: 'registrations',
          name: 'student-registrations',
          component: () => import('../views/Student/RegistrationsView.vue'),
          meta: { title: 'Đăng ký môn' },
        },

        // ── Tài chính ────────────────────────────────────
        {
          path: 'tuition',
          name: 'student-tuition',
          component: () => import('../views/Student/TuitionView.vue'),
          meta: { title: 'Học phí & Thanh toán' },
        },

        // ── Hỗ trợ ───────────────────────────────────────
        {
          path: 'support-tickets',
          name: 'student-support-tickets',
          component: () => import('../views/Student/SupportTicketsView.vue'),
          meta: { title: 'Hỗ trợ / Ticket' },
        },
        {
          path: 'requests',
          name: 'student-requests',
          component: () => import('../views/Student/RequestsView.vue'),
          meta: { title: 'Đơn từ' },
        },

        // ── Cá nhân ──────────────────────────────────────
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

    // ── 404 ───────────────────────────────────────────────
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue'),
    },
  ],

  // Scroll behavior: luôn lên đầu trang khi navigate
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0, behavior: 'smooth' }
  },
})

// ── Navigation Guard (stub) ────────────────────────────────
router.beforeEach((to, from, next) => {
  // TODO: kiểm tra auth token từ Pinia store
  // const authStore = useAuthStore()
  // if (to.meta.requiresAuth && !authStore.isLoggedIn) {
  //   return next('/login')
  // }
  next()
})

export default router
