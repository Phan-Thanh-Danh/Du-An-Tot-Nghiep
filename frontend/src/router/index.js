import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { getHomeRouteByRole } from '@/utils/roleRoutes'
import { getPortalByRole, isValidPortal } from '@/data/authPortals'
import {
  getRequiredRoleFromMatchedRoutes,
  routeRequiresAuthentication,
} from '@/utils/authRedirect'

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
      name: 'portal-landing',
      component: () => import('../views/Auth/PortalLandingView.vue'),
      meta: { public: true, title: 'Cổng truy cập EduLMS' },
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/login/:portal',
      name: 'role-login',
      component: () => import('../views/Auth/RoleLoginView.vue'),
      meta: { public: true, title: 'Đăng nhập EduLMS' },
    },
    {
      path: '/login',
      redirect: (to) => {
        const portalSlug = typeof to.query.portal === 'string' ? to.query.portal : ''
        if (!isValidPortal(portalSlug)) {
          return { name: 'portal-landing' }
        }
        const query = {}
        if (typeof to.query.redirect === 'string') {
          query.redirect = to.query.redirect
        }
        return {
          name: 'role-login',
          params: { portal: portalSlug },
          query,
        }
      },
    },
    {
      path: '/student/exams/:examId/take',
      name: 'student-exam-take',
      component: () => import('../views/Student/ExamTakeView.vue'),
      meta: { requiresAuth: true, role: 'Student', title: 'Làm bài thi', fullscreen: true },
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
      meta: { requiresAuth: true, role: 'Student' },
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
          path: 'rewards',
          name: 'student-rewards',
          component: () => import('../views/Student/RewardsView.vue'),
          meta: { title: 'Khen thưởng' },
        },
        {
          path: 'discipline',
          name: 'student-discipline',
          component: () => import('../views/Student/DisciplineView.vue'),
          meta: { title: 'Kỷ luật' },
        },
        {
          path: 'notifications',
          name: 'student-notifications',
          component: () => import('../views/Student/NotificationsView.vue'),
          meta: { title: 'Thông báo' },
        },
      ],
    },

    // ── Parent Layout (Phụ huynh) ─────────────────────────
    {
      path: '/parent',
      component: () => import('../components/PhuHuynh/Layout_PhuHuynh.vue'),
      meta: { requiresAuth: true, role: 'Parent' },
      children: [
        { path: '', redirect: '/parent/dashboard' },
        {
          path: 'dashboard',
          name: 'parent-dashboard',
          component: () => import('../views/PhuHuynh/DashboardWrapper.vue'),
          meta: { title: 'Dashboard Phụ huynh' },
        },
        {
          path: 'children/list',
          name: 'parent-children-list',
          component: () => import('../views/PhuHuynh/Children/ListView.vue'),
          meta: { title: 'Danh sách học sinh' },
        },
        {
          path: 'children/overview',
          name: 'parent-children-overview',
          component: () => import('../views/PhuHuynh/Children/OverviewView.vue'),
          meta: { title: 'Tổng quan học tập' },
        },
        {
          path: 'learning/grades',
          name: 'parent-grades',
          component: () => import('../views/PhuHuynh/Learning/GradesView.vue'),
          meta: { title: 'Kết quả học tập' },
        },
        {
          path: 'learning/schedule',
          name: 'parent-schedule',
          component: () => import('../views/PhuHuynh/Learning/ScheduleView.vue'),
          meta: { title: 'Thời khóa biểu' },
        },
        {
          path: 'learning/attendance',
          name: 'parent-attendance',
          component: () => import('../views/PhuHuynh/Learning/AttendanceView.vue'),
          meta: { title: 'Điểm danh' },
        },
        {
          path: 'learning/alerts',
          name: 'parent-alerts',
          component: () => import('../views/PhuHuynh/Learning/AlertsView.vue'),
          meta: { title: 'Cảnh báo' },
        },
        {
          path: 'finance/tuition',
          name: 'parent-tuition',
          component: () => import('../views/PhuHuynh/Finance/TuitionView.vue'),
          meta: { title: 'Học phí' },
        },
        {
          path: 'finance/payment',
          name: 'parent-payment',
          component: () => import('../views/PhuHuynh/Finance/PaymentView.vue'),
          meta: { title: 'Thanh toán' },
        },
        {
          path: 'finance/transactions',
          name: 'parent-transactions',
          component: () => import('../views/PhuHuynh/Finance/TransactionsView.vue'),
          meta: { title: 'Lịch sử giao dịch' },
        },
        {
          path: 'finance/invoices',
          name: 'parent-invoices',
          component: () => import('../views/PhuHuynh/Finance/InvoicesView.vue'),
          meta: { title: 'Hóa đơn' },
        },
        {
          path: 'notifications/system',
          name: 'parent-notifications',
          component: () => import('../views/PhuHuynh/Notifications/SystemView.vue'),
          meta: { title: 'Thông báo hệ thống' },
        },
        {
          path: 'notifications/history',
          name: 'parent-notifications-history',
          component: () => import('../views/PhuHuynh/Notifications/HistoryView.vue'),
          meta: { title: 'Lịch sử thông báo' },
        },
        {
          path: 'profile/info',
          name: 'parent-profile-info',
          component: () => import('../views/PhuHuynh/Profile/InfoView.vue'),
          meta: { title: 'Hồ sơ cá nhân' },
        },
        {
          path: 'profile/access-rights',
          name: 'parent-profile-access-rights',
          component: () => import('../views/PhuHuynh/Profile/AccessRightsView.vue'),
          meta: { title: 'Quyền truy cập' },
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
        { path: 'assignments', name: 'teacher-assignments', component: () => import('../views/GiangVien/AssignmentsListView.vue') , meta: { title: 'Phân công giảng viên' } },
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
        { path: 'requests', name: 'teacher-requests', component: () => import('../views/GiangVien/PendingRequestsView.vue') , meta: { title: 'Đơn cần xử lý' } },
        { path: 'requests-history', name: 'teacher-requests-history', component: () => import('../views/GiangVien/RequestsHistoryView.vue') , meta: { title: 'Đơn đã xử lý' } },
        { path: 'profile', name: 'teacher-profile', component: () => import('../views/GiangVien/ProfileView.vue') },

        {
          path: 'notifications', name: 'teacher-notifications', component: () => import('../views/Student/NotificationsView.vue'), meta: { title: 'Thông báo', subtitle: 'Trung tâm thông báo', section: 'Cá nhân' } },
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
        { path: 'schedule', name: 'staff-schedule', component: () => import('../views/GiaoVu/Schedule/ScheduleManagerView.vue') , meta: { title: 'Quản lý thời khóa biểu' } },
        { path: 'assignments', name: 'staff-assignments', component: () => import('../views/GiaoVu/Schedule/TeacherAssignmentView.vue') , meta: { title: 'Phân công giảng viên' } },
        { path: 'buildings', name: 'staff-buildings', component: () => import('../views/GiaoVu/Facilities/BuildingManagementView.vue') , meta: { title: 'Quản lý tòa nhà' } },
        { path: 'floors', name: 'staff-floors', component: () => import('../views/GiaoVu/Facilities/FloorManagementView.vue') , meta: { title: 'Quản lý lầu' } },
        { path: 'shifts', name: 'staff-shifts', component: () => import('../views/GiaoVu/Schedule/ShiftManagementView.vue') , meta: { title: 'Quản lý ca học' } },
        { path: 'rooms', name: 'staff-rooms', component: () => import('../views/GiaoVu/Schedule/RoomManagementView.vue') , meta: { title: 'Quản lý phòng học' } },
        { path: 'conflicts', name: 'staff-conflicts', component: () => import('../views/GiaoVu/Schedule/ConflictCheckView.vue') , meta: { title: 'Kiểm tra xung đột' } },
        { path: 'schedule/pending', name: 'staff-schedule-pending', component: () => import('../views/GiaoVu/Schedule/PendingSchedulesView.vue') , meta: { title: 'Lịch chờ duyệt' } },
        { path: 'schedule/published', name: 'staff-schedule-published', component: () => import('../views/GiaoVu/Schedule/StaffPublishedSchedulesView.vue') , meta: { title: 'Lịch đã công bố' } },
        { path: 'academic-terms', name: 'staff-academic-terms', component: () => import('../views/GiaoVu/AcademicTerms/AcademicTermManagementView.vue'), meta: { title: 'Quản lý học kỳ' } },
        { path: 'subjects', name: 'staff-subjects', component: () => import('../views/GiaoVu/Subjects/SubjectManagementView.vue'), meta: { title: 'Quản lý môn học' } },
        { path: 'courses', name: 'staff-courses', component: () => import('../views/GiaoVu/Courses/CourseManagementView.vue'), meta: { title: 'Danh sách khóa học', subtitle: 'Quản lý và phân phối môn học cho giảng viên và lớp hành chính' } },
        
        // Đăng ký, Dung lượng, Trạng thái khóa học
        { path: 'registrations', name: 'staff-registrations', component: () => import('../views/GiaoVu/Registration/RegistrationPeriodsView.vue'), meta: { title: 'Quản lý đợt đăng ký', subtitle: 'Cấu hình và quản lý các đợt đăng ký môn học' } },
        { path: 'capacity', name: 'staff-capacity', component: () => import('../views/GiaoVu/Registration/CapacityAdjustmentView.vue'), meta: { title: 'Điều chỉnh dung lượng', subtitle: 'Quản lý sức chứa và danh sách chờ các lớp học' } },
        { path: 'course-status', name: 'staff-course-status', component: () => import('../views/GiaoVu/Registration/CourseStatusView.vue'), meta: { title: 'Trạng thái khóa học', subtitle: 'Giám sát trạng thái các lớp học và xử lý hủy lớp' } },
        
        // Đơn từ (Requests)
        { path: 'requests', name: 'staff-requests', component: () => import('../views/GiaoVu/Requests/PendingRequestsView.vue') , meta: { title: 'Đơn cần xử lý' } },
        { path: 'requests-history', name: 'staff-requests-history', component: () => import('../views/GiaoVu/Requests/RequestHistoryView.vue') , meta: { title: 'Đơn đã xử lý' } },
        { path: 'workflow', name: 'staff-workflow', component: () => import('../views/GiaoVu/Requests/WorkflowConfigView.vue') , meta: { title: 'Cấu hình quy trình' } },

        {
          path: 'notices/send',
          name: 'staff-notices-send',
          component: () => import('../views/GiaoVu/Notices/SendNoticeView.vue'),
        },
        {
          path: 'notices/history',
          name: 'staff-notices-history',
          component: () => import('../views/GiaoVu/Notices/NoticeHistoryView.vue'),
        },
        { path: 'classes', name: 'staff-classes', component: () => import('../views/GiaoVu/Classes/ClassManagementView.vue'), meta: { title: 'Lớp hành chính' } },
        { path: 'accounts', name: 'staff-accounts', component: () => import('../views/GiaoVu/Accounts/AccountManagementView.vue'), meta: { title: 'Quản lý tài khoản' } },
        {
          path: 'profile',
          name: 'staff-profile',
          component: () => import('../views/GiaoVu/Profile/StaffProfileView.vue'),
        },
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
        // Đào tạo — Báo cáo học tập
        { path: 'academic/overview', name: 'bgh-academic-overview', component: () => import('../views/BGH/Academic/AcademicOverviewView.vue'), meta: { title: 'Tổng quan kết quả học tập', subtitle: 'Báo cáo phân tích chất lượng học tập trên toàn hệ thống', section: 'Đào tạo & Chương trình' } },
        { path: 'academic/gpa', name: 'bgh-academic-gpa', component: () => import('../views/BGH/Academic/GPAReportsView.vue'), meta: { title: 'Báo cáo GPA hệ thống', subtitle: 'Phân tích điểm trung bình tích lũy theo khoa và lớp', section: 'Đào tạo & Chương trình' } },
        { path: 'academic/at-risk', name: 'bgh-academic-at-risk', component: () => import('../views/BGH/Academic/AtRiskStudentsView.vue'), meta: { title: 'Sinh viên có nguy cơ rớt môn', subtitle: 'Cảnh báo sớm AI dựa trên điểm số và chuyên cần', section: 'Đào tạo & Chương trình' } },
        { path: 'academic/at-risk/:studentId/history', name: 'bgh-academic-at-risk-student-history', component: () => import('../views/BGH/Academic/StudentHistoryView.vue'), meta: { title: 'Lịch sử học tập', subtitle: 'Chi tiết quá trình học tập của sinh viên', section: 'Đào tạo & Chương trình' } },
        { path: 'academic/reports', name: 'bgh-academic-reports', component: () => import('../views/BGH/Academic/AcademicReportsView.vue'), meta: { title: 'Báo cáo học tập chi tiết', subtitle: 'Công cụ phân tích và kết xuất báo cáo đa chiều', section: 'Đào tạo & Chương trình' } },
        { path: 'academic/pass-fail', name: 'bgh-academic-pass-fail', component: () => import('../views/BGH/Academic/PassFailRatesView.vue'), meta: { title: 'Tỷ lệ Pass/Fail môn học', subtitle: 'Theo dõi và phân tích tỷ lệ qua môn, rớt môn', section: 'Đào tạo & Chương trình' } },
        // Phê duyệt & Đánh giá
        { path: 'schedule/pending', name: 'bgh-schedule-pending', component: () => import('../views/BGH/SchedulePendingView.vue'), meta: { title: 'Duyệt Thời khóa biểu', subtitle: 'Phê duyệt thời khóa biểu trước khi công bố', section: 'Phê duyệt & Đánh giá' } },
        { path: 'schedule/conflicts', name: 'bgh-schedule-conflicts', component: () => import('../views/BGH/Schedule/ConflictListView.vue'), meta: { title: 'Xung đột lịch học', subtitle: 'Giám sát các lỗi sắp xếp tài nguyên giảng dạy', section: 'Phê duyệt & Đánh giá' } },
        { path: 'schedule/published', name: 'bgh-schedule-published', component: () => import('../views/BGH/Schedule/PublishedSchedulesView.vue'), meta: { title: 'Thời khóa biểu đã duyệt', subtitle: 'Xem các bộ TKB đã công bố chính thức', section: 'Phê duyệt & Đánh giá' } },
        { path: 'schedule/changes', name: 'bgh-schedule-changes', component: () => import('../views/BGH/Schedule/ScheduleChangesView.vue'), meta: { title: 'Thay đổi & Dạy bù', subtitle: 'Theo dõi biến động lịch học sau công bố', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations', name: 'bgh-evaluations', component: () => import('../views/BGH/EvaluationsView.vue'), meta: { title: 'Đánh giá Giảng viên', subtitle: 'Kết quả khảo sát và đánh giá giảng dạy', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations/ranking', name: 'bgh-evaluations-ranking', component: () => import('../views/BGH/Evaluations/TeacherRankingView.vue'), meta: { title: 'Xếp hạng giảng viên', subtitle: 'Bảng xếp hạng chất lượng dựa trên điểm đánh giá', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations/detail/:teacherId', name: 'bgh-evaluations-detail', component: () => import('../views/BGH/Evaluations/TeacherEvalDetailsView.vue'), meta: { title: 'Chi tiết đánh giá', subtitle: 'Báo cáo phân tích chuyên sâu chất lượng giảng dạy', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations/overview', name: 'bgh-evaluations-overview', component: () => import('../views/BGH/Evaluations/EvalOverviewView.vue'), meta: { title: 'Tổng quan đánh giá', subtitle: 'Báo cáo phân tích chất lượng giảng dạy từ phản hồi sinh viên', section: 'Phê duyệt & Đánh giá' } },
        { path: 'evaluations/ai-analysis', name: 'bgh-evaluations-ai-analysis', component: () => import('../views/BGH/Evaluations/AIFeedbackAnalysisView.vue'), meta: { title: 'Phân tích Feedback AI', subtitle: 'AI phân tích cảm xúc và trích xuất chủ đề từ nhận xét sinh viên', section: 'Phê duyệt & Đánh giá' } },
        // Cơ sở vật chất
        { path: 'facilities', name: 'bgh-facilities', component: () => import('../views/BGH/FacilitiesView.vue'), meta: { title: 'Cơ sở vật chất', subtitle: 'Quản lý Tòa nhà, Tầng và Phòng học', section: 'Cơ sở vật chất' } },
        // Giám sát hệ thống
        { path: 'audit-logs', name: 'bgh-audit-logs', component: () => import('../views/BGH/AuditLogsView.vue'), meta: { title: 'Nhật ký kiểm toán', subtitle: 'Theo dõi lịch sử thay đổi trên hệ thống', section: 'Giám sát hệ thống' } },
        // Cá nhân
        { path: 'profile', name: 'bgh-profile', component: () => import('../views/BGH/ProfileView.vue'), meta: { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin cá nhân và cài đặt tài khoản', section: 'Cá nhân' } },

        {
          path: 'notifications', name: 'bgh-notifications', component: () => import('../views/Student/NotificationsView.vue'), meta: { title: 'Thông báo', subtitle: 'Trung tâm thông báo', section: 'Cá nhân' } },
      ],
    },


    // ── Super Admin Layout ────────────────────────────────
    {
      path: '/super-admin',
      component: () => import('../components/SuperAdmin/Layout_SuperAdmin.vue'),
      meta: { requiresAuth: true, role: ['SuperAdmin', 'Admin'] },
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
          component: () => import('../views/SuperAdmin/ProfileView.vue'),
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
          component: () => import('../views/GiaoVu/Schedule/ScheduleManagerView.vue'),
          meta: { title: 'Thời khóa biểu' },
        },
        {
          path: 'operations/schedules/approval',
          name: 'super-admin-operations-schedules-approval',
          component: () => import('../views/GiaoVu/Schedule/PendingSchedulesView.vue'),
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
          component: () => import('../views/SuperAdmin/Finance/TuitionConfigView.vue'),
          meta: { title: 'Cấu hình học phí' },
        },
        {
          path: 'finance/student-debts',
          name: 'super-admin-finance-student-debts',
          component: () => import('../views/SuperAdmin/Finance/FinanceMonitorView.vue'),
          meta: { title: 'Công nợ sinh viên', financeMode: 'student-debts' },
        },
        {
          path: 'finance/payments',
          name: 'super-admin-finance-payments',
          component: () => import('../views/SuperAdmin/Finance/FinanceMonitorView.vue'),
          meta: { title: 'Theo dõi thanh toán', financeMode: 'payments' },
        },
        {
          path: 'finance/refunds',
          name: 'super-admin-finance-refunds',
          component: () => import('../views/SuperAdmin/Finance/FinanceMonitorView.vue'),
          meta: { title: 'Hoàn phí/Bảo lưu', financeMode: 'refunds' },
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
        },
        {
          path: 'approvals/reports',
          name: 'super-admin-approvals-reports',
          component: () => import('../views/SuperAdmin/ApplicationReportsView.vue'),
          meta: { title: 'Báo cáo đơn từ' },
        },
        {
          path: 'rewards-discipline',
          name: 'super-admin-rewards-discipline',
          component: () => import('../views/SuperAdmin/RewardDisciplineView.vue'),
          meta: { title: 'Khen thưởng & Kỷ luật' },
        },
        {
          path: 'rewards/campaigns',
          name: 'super-admin-rewards-campaigns',
          component: () => import('../views/SuperAdmin/RewardCampaignsView.vue'),
          meta: { title: 'Chiến dịch khen thưởng' },
        },
        {
          path: 'discipline/records',
          name: 'super-admin-discipline-records',
          component: () => import('../views/SuperAdmin/DisciplineRecordsView.vue'),
          meta: { title: 'Hồ sơ kỷ luật' },
        },
        {
          path: 'discipline/appeals',
          name: 'super-admin-discipline-appeals',
          component: () => import('../views/SuperAdmin/DisciplineAppealsView.vue'),
          meta: { title: 'Khiếu nại kỷ luật' },
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
          component: () => import('../views/SuperAdmin/NotificationTemplatesView.vue'),
          meta: { title: 'Template thông báo' },
        },
        {
          path: 'notifications/send',
          name: 'super-admin-notifications-send',
          component: () => import('../views/SuperAdmin/SendNotificationView.vue'),
          meta: { title: 'Gửi thông báo toàn hệ thống' },
        },
        {
          path: 'notifications/history',
          name: 'super-admin-notifications-history',
          component: () => import('../views/SuperAdmin/NotificationHistoryView.vue'),
          meta: { title: 'Lịch sử thông báo' },
        },
        // 9. Quản trị Hệ thống, Audit và Bảo mật
        {
          path: 'audit/logs',
          name: 'super-admin-audit-logs',
          component: () => import('../views/SuperAdmin/AuditLogsView.vue'),
          meta: { title: 'Audit Log' },
        },
        {
          path: 'security/alerts',
          name: 'super-admin-security-alerts',
          component: () => import('../views/SuperAdmin/SecurityAlertsView.vue'),
          meta: { title: 'Security Alert' },
        },
        {
          path: 'system/modules',
          name: 'super-admin-system-modules',
          component: () => import('../views/SuperAdmin/SystemModulesView.vue'),
          meta: { title: 'Bật/Tắt module' },
        },
        {
          path: 'system/ai-automation',
          name: 'super-admin-system-ai-automation',
          component: () => import('../views/SuperAdmin/AiAutomationView.vue'),
          meta: { title: 'Cấu hình AI & Automation' },
        },
      ],
    },

    // ── Content Council Layout ────────────────────────────────────────────
    {
      path: '/content-council',
      component: () => import('../layouts/content-council/ContentCouncilLayout.vue'),
      meta: { requiresAuth: true, role: 'HoiDongQuanLyNoiDung' },
      children: [
        { path: '', redirect: '/content-council/subjects' },
        {
          path: 'subjects',
          name: 'content-council-subjects',
          component: () => import('../pages/content-council/subjects/SubjectListPage.vue'),
          meta: { title: 'Nội dung môn học' },
        },
        {
          path: 'question-bank',
          name: 'content-council-question-bank',
          component: () => import('../pages/content-council/question-bank/QuestionBankPage.vue'),
          meta: { title: 'Ngân hàng câu hỏi' }
        },
        {
          path: 'quizzes',
          name: 'content-council-quizzes',
          component: () => import('../pages/content-council/quizzes/QuizListPage.vue'),
          meta: { title: 'Quiz / Đề kiểm tra' }
        },
        {
          path: 'quizzes/new',
          name: 'content-council-quiz-create',
          component: () => import('../pages/content-council/quizzes/QuizFormPage.vue'),
          meta: { title: 'Tạo Quiz', requiresAuth: true, roles: ['HoiDongQuanLyNoiDung'] }
        },
        {
          path: 'quizzes/:quizId/edit',
          name: 'content-council-quiz-edit',
          component: () => import('../pages/content-council/quizzes/QuizFormPage.vue'),
          meta: { title: 'Chỉnh sửa Quiz', requiresAuth: true, roles: ['HoiDongQuanLyNoiDung'] }
        },
        {
          path: 'quizzes/:quizId/builder',
          name: 'content-council-quiz-builder',
          component: () => import('../pages/content-council/quizzes/QuizBuilderPage.vue'),
          meta: { title: 'Xây dựng đề', requiresAuth: true, roles: ['HoiDongQuanLyNoiDung'] }
        },
        {
          path: 'subjects/:subjectId',
          component: () => import('../layouts/content-council/SubjectDetailLayout.vue'),
          children: [
            {
              path: '',
              redirect: to => ({ name: 'content-council-subject-overview', params: { subjectId: to.params.subjectId } })
            },
            {
              path: 'overview',
              name: 'content-council-subject-overview',
              component: () => import('../pages/content-council/subjects/SubjectOverviewPage.vue'),
              meta: { title: 'Tổng quan môn học' }
            },
            {
              path: 'editor',
              name: 'content-council-subject-editor',
              component: () => import('../pages/content-council/subjects/SubjectEditorPage.vue'),
              meta: { title: 'Trình soạn nội dung' }
            }
          ]
        }
      ]
    },

    // ── Content Council Preview ───────────────────────────────────────────
    {
      path: '/content-council/subjects/:subjectId/preview',
      name: 'content-council-subject-preview',
      component: () => import('../pages/content-council/subjects/SubjectPreviewPage.vue'),
      meta: { title: 'Xem như học sinh', requiresAuth: true, role: 'HoiDongQuanLyNoiDung', previewMode: true }
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
    const homeRoute = getHomeRouteByRole(authStore.role)

    if (homeRoute) {
      return homeRoute
    }

    authStore.logout()
    return '/'
  }

  // Yêu cầu login
  if (!authStore.isAuthenticated && routeRequiresAuthentication(to.matched)) {
    const requiredRole = getRequiredRoleFromMatchedRoutes(to.matched)
    const portal = getPortalByRole(requiredRole)

    if (portal?.enabled) {
      return {
        name: 'role-login',
        params: { portal: portal.slug },
        query: { redirect: to.fullPath },
      }
    }

    return { name: 'portal-landing' }
  }

  // Kiểm tra quyền (Role)
  if (to.meta.role && !authStore.hasRole(to.meta.role)) {
    return { name: 'not-found' }
  }
})

export default router
