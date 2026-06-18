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
        if (authStore.hasRole('SuperAdmin')) return '/super-admin/dashboard'
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
    {
      path: '/student/exams/:examId/take',
      name: 'student-exam-take',
      component: () => import('../views/Student/ExamTakeView.vue'),
      meta: { requiresAuth: true, role: 'student', title: 'Làm bài thi', fullscreen: true },
    },
    {
      path: '/payment/success',
      name: 'payment-success',
      component: () => import('../views/Payment/PaymentSuccessView.vue'),
      meta: { title: 'Thanh toán đang xác nhận' },
    },
    {
      path: '/payment/cancel',
      name: 'payment-cancel',
      component: () => import('../views/Payment/PaymentCancelView.vue'),
      meta: { title: 'Đã hủy thanh toán' },
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
          path: 'curriculum',
          name: 'student-curriculum',
          component: () => import('../views/Student/CurriculumView.vue'),
          meta: { title: 'Khung chương trình' },
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
          path: 'exams/detail/:examId',
          name: 'student-exam-detail',
          component: () => import('../views/Student/ExamDetailView.vue'),
          meta: { title: 'Chi tiết bài thi' },
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
          path: 'notifications',
          name: 'student-notifications',
          component: () => import('../views/Student/NotificationsView.vue'),
          meta: { title: 'Thông báo' },
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
        { path: 'classes/:id/details', name: 'teacher-class-details', component: () => import('../views/GiangVien/ClassDetailView.vue') },
        { path: 'classes/:id/workspace', name: 'teacher-class-workspace', component: () => import('../views/GiangVien/ClassWorkspaceView.vue') },
        { path: 'class-progress', name: 'teacher-class-progress', component: () => import('../views/GiangVien/ClassProgressView.vue') },
        { path: 'class-attendance', name: 'teacher-class-attendance', component: () => import('../views/GiangVien/ClassAttendanceView.vue') },
        { path: 'class-grades', name: 'teacher-class-grades', component: () => import('../views/GiangVien/ClassGradebookView.vue') },
        { path: 'assignments', name: 'teacher-assignments', component: () => import('../views/GiangVien/AssignmentsListView.vue') },
        { path: 'exams', name: 'teacher-exams', component: () => import('../views/GiangVien/ExamsView.vue') },
        { path: 'exams/create', name: 'teacher-exams-create', component: () => import('../views/GiangVien/CreateExamView.vue') },
        { path: 'grading', name: 'teacher-grading', component: () => import('../views/GiangVien/GradingView.vue') },
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
        { path: 'notifications', name: 'teacher-notifications', component: () => import('../views/SuperAdmin/PlaceholderView.vue'), meta: { title: 'Thông báo', subtitle: 'Trung tâm thông báo', section: 'Cá nhân' } },
        { path: 'change-password', name: 'teacher-change-password', component: () => import('../views/GiangVien/ChangePasswordView.vue') },
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
        { path: 'notifications', name: 'staff-notifications', component: () => import('../views/GiaoVu/StaffNotificationsView.vue'), meta: { title: 'Thông báo' } },
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
          meta: { title: 'Dashboard chiến lược', subtitle: 'Tổng quan hệ thống đào tạo, chất lượng và thống kê', section: 'Dashboard' },
        },
        // Cơ cấu tổ chức
        { path: 'organizations', name: 'bgh-organizations', component: () => import('../views/BGH/OrganizationsView.vue'), meta: { title: 'Quản lý Đơn vị', subtitle: 'Cơ cấu tổ chức các khoa, phòng ban', section: 'Cơ cấu tổ chức' } },
        { path: 'users', name: 'bgh-users', component: () => import('../views/BGH/UsersView.vue'), meta: { title: 'Quản lý Người dùng', subtitle: 'Danh sách tài khoản sinh viên, giảng viên và nhân sự', section: 'Cơ cấu tổ chức' } },
        { path: 'roles', name: 'bgh-roles', component: () => import('../views/BGH/RolesView.vue'), meta: { title: 'Vai trò & Phân quyền', subtitle: 'Cấu hình quyền hạn truy cập hệ thống', section: 'Cơ cấu tổ chức' } },
        // Đào tạo & Chương trình
        { path: 'academic-programs', name: 'bgh-academic-programs', component: () => import('../views/BGH/ProgramsView.vue'), meta: { title: 'Ngành & Chuyên ngành', subtitle: 'Quản lý các chuyên ngành đào tạo', section: 'Đào tạo & Chương trình' } },
        { path: 'curriculum', name: 'bgh-curriculum', component: () => import('../views/BGH/CurriculumView.vue'), meta: { title: 'Khung chương trình', subtitle: 'Phê duyệt và theo dõi khung chương trình', section: 'Đào tạo & Chương trình' } },
        { path: 'academic-terms', name: 'bgh-academic-terms', component: () => import('../views/BGH/AcademicTermsView.vue'), meta: { title: 'Học kỳ & Khóa', subtitle: 'Quản lý các học kỳ và khóa học', section: 'Đào tạo & Chương trình' } },
        // Phê duyệt & Đánh giá
        { path: 'schedule/pending', name: 'bgh-schedule-pending', component: () => import('../views/BGH/SchedulePendingView.vue'), meta: { title: 'Duyệt Thời khóa biểu', subtitle: 'Phê duyệt thời khóa biểu trước khi công bố', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations', name: 'bgh-evaluations', component: () => import('../views/BGH/EvaluationsView.vue'), meta: { title: 'Đánh giá Giảng viên', subtitle: 'Kết quả khảo sát và đánh giá giảng dạy', section: 'Phê duyệt & Đánh giá' } },
        // Cơ sở vật chất
        { path: 'facilities', name: 'bgh-facilities', component: () => import('../views/BGH/FacilitiesView.vue'), meta: { title: 'Cơ sở vật chất', subtitle: 'Quản lý Tòa nhà, Tầng và Phòng học', section: 'Cơ sở vật chất' } },
        // Giám sát hệ thống
        { path: 'audit-logs', name: 'bgh-audit-logs', component: () => import('../views/BGH/AuditLogsView.vue'), meta: { title: 'Nhật ký kiểm toán', subtitle: 'Theo dõi lịch sử thay đổi trên hệ thống', section: 'Giám sát hệ thống' } },
        // Cá nhân
        { path: 'profile', name: 'bgh-profile', component: () => import('../views/BGH/ProfileView.vue'), meta: { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin cá nhân và cài đặt tài khoản', section: 'Cá nhân' } },
        { path: 'notifications', name: 'bgh-notifications', component: () => import('../views/BGH/PlaceholderView.vue'), meta: { title: 'Thông báo', subtitle: 'Trung tâm thông báo', section: 'Cá nhân' } },
      ],
    },

    // ── Super Admin Layout ────────────────────────────────
    {
      path: '/super-admin',
      component: () => import('../components/SuperAdmin/Layout_SuperAdmin.vue'),
      meta: { requiresAuth: true, role: 'SuperAdmin' },
      children: [
        { path: '', redirect: '/super-admin/dashboard' },
        {
          path: 'dashboard',
          name: 'super-admin-dashboard',
          component: () => import('../views/SuperAdmin/Dashboard.vue'),
          meta: { title: 'Dashboard Quản trị' },
        },
        {
          path: 'profile',
          name: 'super-admin-profile',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Hồ sơ cá nhân Admin' },
        },
        // 2. Quản lý Cơ sở (Organization Hierarchy)
        {
          path: 'organizations',
          name: 'super-admin-organizations',
          component: () => import('../views/SuperAdmin/OrganizationsView.vue'),
          meta: { title: 'Quản lý cây tổ chức' },
        },

        // 3. Tài khoản và Phân quyền (RBAC)
        {
          path: 'users',
          name: 'super-admin-users',
          component: () => import('../views/SuperAdmin/UsersView.vue'),
          meta: { title: 'Danh sách người dùng' },
        },

        {
          path: 'roles-permissions',
          name: 'super-admin-roles-permissions',
          component: () => import('../views/SuperAdmin/RolesPermissionsView.vue'),
          meta: { title: 'Vai trò & Quyền hạn' },
        },
        {
          path: 'login-history',
          name: 'super-admin-login-history',
          component: () => import('../views/SuperAdmin/LoginHistoryView.vue'),
          meta: { title: 'Lịch sử đăng nhập' },
        },
        // 4. Quản lý Đào tạo và Học vụ
        {
          path: 'training/semesters',
          name: 'super-admin-training-semesters',
          component: () => import('../views/SuperAdmin/SemestersView.vue'),
          meta: { title: 'Cấu hình học kỳ' },
        },
        {
          path: 'training/programs',
          name: 'super-admin-training-programs',
          component: () => import('../views/SuperAdmin/ProgramsView.vue'),
          meta: { title: 'Cấu trúc chương trình' },
        },
        {
          path: 'training/subjects',
          name: 'super-admin-training-subjects',
          component: () => import('../views/SuperAdmin/SubjectsView.vue'),
          meta: { title: 'Quản lý môn học' },
        },
        {
          path: 'training/courses',
          name: 'super-admin-training-courses',
          component: () => import('../views/SuperAdmin/CoursesView.vue'),
          meta: { title: 'Quản lý khóa học' },
        },
        {
          path: 'training/exam-periods',
          name: 'super-admin-training-exam-periods',
          component: () => import('../views/SuperAdmin/ExamPeriodsView.vue'),
          meta: { title: 'Mở/Đóng giai đoạn thi' },
        },
        {
          path: 'operations/schedules',
          name: 'super-admin-operations-schedules',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Thời khóa biểu' },
        },
        {
          path: 'operations/schedules/approval',
          name: 'super-admin-operations-schedules-approval',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Duyệt/Publish TKB' },
        },
        {
          path: 'operations/attendance-policy',
          name: 'super-admin-operations-attendance-policy',
          component: () => import('../views/SuperAdmin/AttendancePolicyView.vue'),
          meta: { title: 'Quỹ vắng & Chuyên cần' },
        },
        {
          path: 'operations/registration-periods',
          name: 'super-admin-operations-registration-periods',
          component: () => import('../views/SuperAdmin/RegistrationPeriodsView.vue'),
          meta: { title: 'Mở/Đóng đăng ký môn' },
        },
        {
          path: 'operations/pass-fail-rules',
          name: 'super-admin-operations-pass-fail-rules',
          component: () => import('../views/SuperAdmin/PassFailRulesView.vue'),
          meta: { title: 'Điều kiện Pass/Fail' },
        },
        // 5. Tài chính và Học phí
        {
          path: 'finance/tuition-config',
          name: 'super-admin-finance-tuition-config',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Cấu hình học phí' },
        },
        {
          path: 'finance/student-debts',
          name: 'super-admin-finance-student-debts',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Công nợ sinh viên' },
        },
        {
          path: 'finance/payments',
          name: 'super-admin-finance-payments',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Theo dõi thanh toán' },
        },
        {
          path: 'finance/refunds',
          name: 'super-admin-finance-refunds',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Hoàn phí/Bảo lưu' },
        },
        // 6. Hỗ trợ, Đơn từ và Đánh giá
        {
          path: 'support/tickets',
          name: 'super-admin-support-tickets',
          component: () => import('../views/SuperAdmin/SupportTicketsView.vue'),
          meta: { title: 'Ticket hỗ trợ' },
        },
        {
          path: 'support/faq',
          name: 'super-admin-support-faq',
          component: () => import('../views/SuperAdmin/FAQManagementView.vue'),
          meta: { title: 'Quản lý FAQ' },
        },
        {
          path: 'approvals/requests',
          name: 'super-admin-approvals-requests',
          component: () => import('../views/SuperAdmin/ApprovalsRequestsView.vue'),
          meta: { title: 'Đơn cần duyệt' },
        },
        {
          path: 'approvals/history',
          name: 'super-admin-approvals-history',
          component: () => import('../views/SuperAdmin/ApprovalsHistoryView.vue'),
          meta: { title: 'Lịch sử duyệt đơn' },
        },
        {
          path: 'evaluations/config',
          name: 'super-admin-evaluations-config',
          component: () => import('../views/SuperAdmin/EvaluationsConfigView.vue'),
          meta: { title: 'Cấu hình đánh giá GV' },
        },
        {
          path: 'evaluations/results',
          name: 'super-admin-evaluations-results',
          component: () => import('../views/SuperAdmin/EvaluationsResultsView.vue'),
          meta: { title: 'Kết quả đánh giá GV' },
        },
        {
          path: 'awards',
          name: 'super-admin-awards',
          component: () => import('../views/SuperAdmin/AwardsView.vue'),
          meta: { title: 'Khen thưởng' },
        },
        {
          path: 'discipline',
          name: 'super-admin-discipline',
          component: () => import('../views/SuperAdmin/DisciplineView.vue'),
          meta: { title: 'Kỷ luật' },
        },
        // 7. Báo cáo và Phân tích (Analytics)
        {
          path: 'reports/education-overview',
          name: 'super-admin-reports-education-overview',
          component: () => import('../views/SuperAdmin/EducationOverviewView.vue'),
          meta: { title: 'Tổng quan đào tạo' },
        },
        {
          path: 'reports/learning',
          name: 'super-admin-reports-learning',
          component: () => import('../views/SuperAdmin/LearningReportView.vue'),
          meta: { title: 'Báo cáo học tập' },
        },
        {
          path: 'reports/attendance',
          name: 'super-admin-reports-attendance',
          component: () => import('../views/SuperAdmin/AttendanceReportView.vue'),
          meta: { title: 'Báo cáo chuyên cần' },
        },
        {
          path: 'reports/campus-comparison',
          name: 'super-admin-reports-campus-comparison',
          component: () => import('../views/SuperAdmin/CampusComparisonView.vue'),
          meta: { title: 'So sánh cơ sở' },
        },
        {
          path: 'reports/export',
          name: 'super-admin-reports-export',
          component: () => import('../views/SuperAdmin/DataExportView.vue'),
          meta: { title: 'Export dữ liệu' },
        },
        // 8. Trung tâm Thông báo (Notification Hub)
        {
          path: 'notifications/templates',
          name: 'super-admin-notifications-templates',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Template thông báo' },
        },
        {
          path: 'notifications/send',
          name: 'super-admin-notifications-send',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Gửi thông báo toàn hệ thống' },
        },
        {
          path: 'notifications/history',
          name: 'super-admin-notifications-history',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Lịch sử thông báo' },
        },
        // 9. Quản trị Hệ thống, Audit và Bảo mật
        {
          path: 'audit/logs',
          name: 'super-admin-audit-logs',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Audit Log' },
        },
        {
          path: 'security/alerts',
          name: 'super-admin-security-alerts',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Security Alert' },
        },
        {
          path: 'system/modules',
          name: 'super-admin-system-modules',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Bật/Tắt module' },
        },
        {
          path: 'system/ai-automation',
          name: 'super-admin-system-ai-automation',
          component: () => import('../views/SuperAdmin/PlaceholderView.vue'),
          meta: { title: 'Cấu hình AI & Automation' },
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

  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0, behavior: 'smooth' }
  },
})

// ── Navigation Guard ────────────────────────────────────────
router.beforeEach((to) => {
  const authStore = useAuthStore()
  authStore.ensureFreshSession()

  // Chuyển hướng nếu truy cập trang public khi đã login
  if (to.meta.public && authStore.isAuthenticated) {
    if (authStore.hasRole('SuperAdmin')) return '/super-admin/dashboard'
    if (authStore.hasRole('Teacher')) return '/teacher/dashboard'
    if (authStore.hasRole('AcademicStaff')) return '/staff/dashboard'
    return '/student/dashboard'
  }

  // Yêu cầu login
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return {
      path: '/login',
      query: { redirect: to.fullPath },
    }
  }

  // Kiểm tra quyền (Role)
  if (to.meta.role && !authStore.hasRole(to.meta.role)) {
    return { name: 'not-found' }
  }
})

export default router
