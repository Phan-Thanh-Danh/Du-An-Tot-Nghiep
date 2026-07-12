# Route Matrix: Role → Route → Menu → Layout → View → API → Permission → Status

## Legend
- **API_CONNECTED**: View exists, layout correct, real API calls confirmed
- **STATIC_FUNCTIONAL**: View works with static/inline data, no API
- **RUNTIME_VERIFIED**: Verified working in production/browser test
- **UNVERIFIED**: View exists but API status unknown
- **WRONG_CONTEXT**: View reused from different role
- **HIDE_FROM_DEMO**: Route exists but hidden from demo
- **PLACEHOLDER**: Stub or default template
- **MOCK_DATA**: Uses mock data instead of real API
- **INLINE_DATA**: Uses inline data arrays
- **BROKEN**: Route/view has issues

---

## Public (6 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| / | None | None | views/Auth/PortalLandingView.vue | N/A | role: None | **STATIC_FUNCTIONAL** |
| /about | None | None | views/AboutView.vue | N/A | role: None | **PLACEHOLDER** |
| /login/:portal | None | None | views/Auth/RoleLoginView.vue | POST /api/auth/login | role: None | **RUNTIME_VERIFIED** |
| /login | None | None | redirect (function) | N/A | role: None | **STATIC_FUNCTIONAL** |
| /payment/success | None | None | views/Payment/PaymentSuccessView.vue | N/A | role: None | **STATIC_FUNCTIONAL** |
| /payment/cancel | None | None | views/Payment/PaymentCancelView.vue | N/A | role: None | **STATIC_FUNCTIONAL** |

## 404 (1 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /:pathMatch(.*)* | None | None | views/NotFoundView.vue | N/A | role: None | **STATIC_FUNCTIONAL** |

## Student (22 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /student/exams/:examId/take | None | None | views/Student/ExamTakeView.vue | GET /api/exam/taking/session/{maPhienThi} | role: Student | **API_CONNECTED** |
| /student/dashboard | sinhVienMenuGroups.js | Layout_SinhVien | views/SinhVien/Dashboard.vue | GET /api/student/dashboard | role: Student | **API_CONNECTED** |
| /student/courses | sinhVienMenuGroups.js | Layout_SinhVien | views/SinhVien/HocTap/KhoacHoc.vue | GET /api/student/courses | role: Student | **API_CONNECTED** |
| /student/curriculum | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/CurriculumView.vue | GET /api/student/curriculum | role: Student | **API_CONNECTED** |
| /student/courses/:courseId | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/CourseDetailView.vue | GET /api/student/courses/{courseId} | role: Student | **API_CONNECTED** |
| /student/assignments | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/AssignmentsView.vue | GET /api/student/assignments | role: Student | **API_CONNECTED** |
| /student/assignments/:assignmentId | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/AssignmentDetailView.vue | GET /api/student/assignments/{assignmentId} | role: Student | **API_CONNECTED** |
| /student/exams | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/ExamsView.vue | GET /api/exam/student/list | role: Student | **API_CONNECTED** |
| /student/exams/detail/:examId | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/ExamDetailView.vue | GET /api/exam/student/result/{sessionId} | role: Student | **API_CONNECTED** |
| /student/exams/:examResultId | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/ExamResultView.vue | GET /api/exam/student/result/{sessionId} | role: Student | **API_CONNECTED** |
| /student/grades | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/GradesView.vue | GET /api/student/grades | role: Student | **API_CONNECTED** |
| /student/schedule | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/ScheduleView.vue | GET /api/student/schedule | role: Student | **API_CONNECTED** |
| /student/attendance | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/AttendanceView.vue | GET /api/student/attendance | role: Student | **API_CONNECTED** |
| /student/registrations | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/RegistrationsView.vue | GET /api/student/registrations/available | role: Student | **API_CONNECTED** |
| /student/tuition | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/TuitionView.vue | GET /api/student/tuition/invoices | role: Student | **API_CONNECTED** |
| /student/support-tickets | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/SupportTicketsView.vue | GET /api/student/support-tickets | role: Student | **API_CONNECTED** |
| /student/requests | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/RequestsView.vue | GET /api/student/applications | role: Student | **API_CONNECTED** |
| /student/evaluations | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/EvaluationsView.vue | GET /api/student/evaluations | role: Student | **API_CONNECTED** |
| /student/profile | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/ProfileView.vue | GET /api/account/me | role: Student | **API_CONNECTED** |
| /student/rewards | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/RewardsView.vue | GET /api/student/rewards | role: Student | **API_CONNECTED** |
| /student/discipline | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/DisciplineView.vue | GET /api/student/discipline-records | role: Student | **API_CONNECTED** |
| /student/notifications | sinhVienMenuGroups.js | Layout_SinhVien | views/Student/NotificationsView.vue | GET /api/notifications | role: Student | **API_CONNECTED** |

## Parent (15 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /parent/dashboard | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/DashboardWrapper.vue | GET /api/parent/dashboard | role: Parent | **API_CONNECTED** |
| /parent/children/list | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Children/ListView.vue | GET /api/parent/children | role: Parent | **API_CONNECTED** |
| /parent/children/overview | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Children/OverviewView.vue | GET /api/parent/children/{childId} | role: Parent | **API_CONNECTED** |
| /parent/learning/grades | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Learning/GradesView.vue | GET /api/parent/children/{childId}/grades | role: Parent | **API_CONNECTED** |
| /parent/learning/schedule | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Learning/ScheduleView.vue | GET /api/parent/children/{childId}/schedule | role: Parent | **API_CONNECTED** |
| /parent/learning/attendance | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Learning/AttendanceView.vue | GET /api/parent/children/{childId}/attendance | role: Parent | **API_CONNECTED** |
| /parent/learning/alerts | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Learning/AlertsView.vue | GET /api/parent/children/{childId}/alerts | role: Parent | **API_CONNECTED** |
| /parent/finance/tuition | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Finance/TuitionView.vue | GET /api/parent/children/{childId}/tuition | role: Parent | **API_CONNECTED** |
| /parent/finance/payment | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Finance/PaymentView.vue | POST /api/parent/payment | role: Parent | **API_CONNECTED** |
| /parent/finance/transactions | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Finance/TransactionsView.vue | GET /api/parent/children/{childId}/transactions | role: Parent | **API_CONNECTED** |
| /parent/finance/invoices | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Finance/InvoicesView.vue | GET /api/parent/children/{childId}/invoices | role: Parent | **API_CONNECTED** |
| /parent/notifications/system | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Notifications/SystemView.vue | GET /api/parent/notifications | role: Parent | **API_CONNECTED** |
| /parent/notifications/history | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Notifications/HistoryView.vue | GET /api/parent/notifications/history | role: Parent | **API_CONNECTED** |
| /parent/profile/info | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Profile/InfoView.vue | GET /api/parent/profile | role: Parent | **API_CONNECTED** |
| /parent/profile/access-rights | phuHuynhMenuGroups.js | Layout_PhuHuynh | views/PhuHuynh/Profile/AccessRightsView.vue | GET /api/parent/access-rights | role: Parent | **API_CONNECTED** |

## Teacher (27 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /teacher/dashboard | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/Dashboard.vue | GET /api/teacher/dashboard | role: Teacher | **API_CONNECTED** |
| /teacher/schedule | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/TeachingScheduleView.vue | GET /api/teacher/schedule | role: Teacher | **API_CONNECTED** |
| /teacher/courses | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/CoursesView.vue | GET /api/courses | role: Teacher | **API_CONNECTED** |
| /teacher/lessons | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/LessonsView.vue | API unknown | role: Teacher | **UNVERIFIED** |
| /teacher/classes | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassListView.vue | GET /api/teacher/classes | role: Teacher | **API_CONNECTED** |
| /teacher/classes/:id/details | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassDetailView.vue | GET /api/teacher/classes/{classId} | role: Teacher | **API_CONNECTED** |
| /teacher/classes/:id/workspace | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassWorkspaceView.vue | GET /api/teacher/classes/{classId}/workspace | role: Teacher | **API_CONNECTED** |
| /teacher/class-progress | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassProgressView.vue | GET /api/teacher/classes/{classId}/progress | role: Teacher | **API_CONNECTED** |
| /teacher/class-attendance | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassAttendanceView.vue | API unknown | role: Teacher | **UNVERIFIED** |
| /teacher/class-grades | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassGradebookView.vue | GET /api/teacher/classes/{classId}/grades | role: Teacher | **API_CONNECTED** |
| /teacher/assignments | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/AssignmentsListView.vue | GET /api/teacher/assignments | role: Teacher | **API_CONNECTED** |
| /teacher/exams | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ExamsView.vue | GET /api/exam/ca-thi | role: Teacher | **API_CONNECTED** |
| /teacher/exams/create | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/CreateExamView.vue | POST /api/teacher/exams | role: Teacher | **API_CONNECTED** |
| /teacher/grading | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/GradingView.vue | GET /api/teacher/submissions | role: Teacher | **API_CONNECTED** |
| /teacher/exam-results | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ExamResultsView.vue | GET /api/teacher/exam-results | role: Teacher | **API_CONNECTED** |
| /teacher/proctoring | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ProctoringView.vue | SignalR /hubs/exam-monitoring | role: Teacher | **API_CONNECTED** |
| /teacher/attendance | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/AttendanceTodayView.vue | GET /api/teacher/attendance/today | role: Teacher | **API_CONNECTED** |
| /teacher/attendance-history | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/AttendanceHistoryView.vue | GET /api/teacher/attendance/history | role: Teacher | **API_CONNECTED** |
| /teacher/grading-input | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ClassGradesView.vue | GET /api/teacher/classes/{classId}/grades | role: Teacher | **API_CONNECTED** |
| /teacher/student-questions | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/StudentQuestionsView.vue | GET /api/teacher/student-questions | role: Teacher | **API_CONNECTED** |
| /teacher/lesson-comments | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/LessonCommentsView.vue | GET /api/teacher/lesson-comments | role: Teacher | **API_CONNECTED** |
| /teacher/requests | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/PendingRequestsView.vue | GET /api/teacher/requests | role: Teacher | **API_CONNECTED** |
| /teacher/requests-history | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/RequestsHistoryView.vue | GET /api/teacher/requests/history | role: Teacher | **API_CONNECTED** |
| /teacher/profile | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ProfileView.vue | N/A | role: Teacher | **STATIC_FUNCTIONAL** |
| /teacher/teaching-preferences | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/TeachingPreferencesView.vue | GET /api/teacher/teaching-preferences/context | role: Teacher | **API_CONNECTED** |
| /teacher/notifications | giangVienMenuGroups.js | Layout_GiangVien | views/Student/NotificationsView.vue | GET /api/notifications | role: Teacher | **WRONG_CONTEXT** |
| /teacher/change-password | giangVienMenuGroups.js | Layout_GiangVien | views/GiangVien/ChangePasswordView.vue | N/A | role: Teacher | **STATIC_FUNCTIONAL** |

## Staff/Giao Vu (25 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /staff/dashboard | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Dashboard.vue | GET /api/staff/dashboard | role: AcademicStaff | **API_CONNECTED** |
| /staff/schedule | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/ScheduleManagerView.vue | GET /api/thoi-khoa-bieu | role: AcademicStaff | **API_CONNECTED** |
| /staff/teaching-preferences | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/TeachingPreferenceSummaryView.vue | GET /api/staff/teaching-preferences/summary | role: AcademicStaff | **API_CONNECTED** |
| /staff/assignments | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/TeacherAssignmentView.vue | GET /api/thoi-khoa-bieu | role: AcademicStaff | **API_CONNECTED** |
| /staff/buildings | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Facilities/BuildingManagementView.vue | GET /api/master-data/buildings | role: AcademicStaff | **API_CONNECTED** |
| /staff/floors | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Facilities/FloorManagementView.vue | GET /api/master-data/floors | role: AcademicStaff | **API_CONNECTED** |
| /staff/shifts | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/ShiftManagementView.vue | GET /api/ca-hoc | role: AcademicStaff | **API_CONNECTED** |
| /staff/rooms | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/RoomManagementView.vue | GET /api/master-data/rooms | role: AcademicStaff | **API_CONNECTED** |
| /staff/conflicts | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/ConflictCheckView.vue | POST /api/thoi-khoa-bieu/check-xung-dot | role: AcademicStaff | **API_CONNECTED** |
| /staff/schedule/pending | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/PendingSchedulesView.vue | GET /api/thoi-khoa-bieu/drafts | role: AcademicStaff | **API_CONNECTED** |
| /staff/schedule/published | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Schedule/StaffPublishedSchedulesView.vue | GET /api/thoi-khoa-bieu | role: AcademicStaff | **API_CONNECTED** |
| /staff/academic-terms | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/AcademicTerms/AcademicTermManagementView.vue | GET /api/master-data/academic-terms | role: AcademicStaff | **API_CONNECTED** |
| /staff/subjects | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Subjects/SubjectManagementView.vue | GET /api/master-data/subjects | role: AcademicStaff | **API_CONNECTED** |
| /staff/courses | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Courses/CourseManagementView.vue | GET /api/courses | role: AcademicStaff | **API_CONNECTED** |
| /staff/registrations | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Registration/RegistrationPeriodsView.vue | GET /api/admin/registration-periods | role: AcademicStaff | **API_CONNECTED** |
| /staff/capacity | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Registration/CapacityAdjustmentView.vue | GET /api/admin/course-sections/capacity | role: AcademicStaff | **API_CONNECTED** |
| /staff/course-status | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Registration/CourseStatusView.vue | POST /api/admin/course-sections/{id}/cancel | role: AcademicStaff | **API_CONNECTED** |
| /staff/requests | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Requests/PendingRequestsView.vue | GET /api/admin/applications | role: AcademicStaff | **API_CONNECTED** |
| /staff/requests-history | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Requests/RequestHistoryView.vue | GET /api/admin/applications | role: AcademicStaff | **API_CONNECTED** |
| /staff/workflow | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Requests/WorkflowConfigView.vue | GET /api/admin/applications/workflow | role: AcademicStaff | **API_CONNECTED** |
| /staff/notices/send | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Notices/SendNoticeView.vue | POST /api/admin/notifications | role: AcademicStaff | **API_CONNECTED** |
| /staff/notices/history | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Notices/NoticeHistoryView.vue | GET /api/admin/notifications | role: AcademicStaff | **API_CONNECTED** |
| /staff/classes | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Classes/ClassManagementView.vue | GET /api/admin/classes | role: AcademicStaff | **API_CONNECTED** |
| /staff/accounts | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Accounts/AccountManagementView.vue | GET /api/admin/users | role: AcademicStaff | **API_CONNECTED** |
| /staff/profile | giaoVuMenuGroups.js | Layout_GiaoVu | views/GiaoVu/Profile/StaffProfileView.vue | N/A | role: AcademicStaff | **STATIC_FUNCTIONAL** |

## BGH (26 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /bgh/dashboard | bghMenuGroups.js | Layout_BGH | views/BGH/Dashboard.vue | GET /api/bgh/dashboard | role: Principal | **API_CONNECTED** |
| /bgh/organizations | bghMenuGroups.js | Layout_BGH | views/BGH/OrganizationsView.vue | GET /api/organizations | role: Principal | **API_CONNECTED** |
| /bgh/users | bghMenuGroups.js | Layout_BGH | views/BGH/UsersView.vue | GET /api/bgh/users | role: Principal | **API_CONNECTED** |
| /bgh/roles | bghMenuGroups.js | Layout_BGH | views/BGH/RolesView.vue | GET /api/bgh/rbac/roles | role: Principal | **API_CONNECTED** |
| /bgh/academic-programs | bghMenuGroups.js | Layout_BGH | views/BGH/ProgramsView.vue | GET /api/bgh/master-data/training-programs | role: Principal | **API_CONNECTED** |
| /bgh/curriculum | bghMenuGroups.js | Layout_BGH | views/BGH/CurriculumView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/academic-terms | bghMenuGroups.js | Layout_BGH | views/BGH/AcademicTermsView.vue | GET /api/bgh/master-data/academic-terms | role: Principal | **API_CONNECTED** |
| /bgh/academic/overview | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/AcademicOverviewView.vue | GET /api/bgh/academic/overview | role: Principal | **API_CONNECTED** |
| /bgh/academic/gpa | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/GPAReportsView.vue | GET /api/bgh/academic/gpa | role: Principal | **API_CONNECTED** |
| /bgh/academic/at-risk | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/AtRiskStudentsView.vue | GET /api/bgh/academic/at-risk | role: Principal | **API_CONNECTED** |
| /bgh/academic/at-risk/:studentId/history | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/StudentHistoryView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/academic/reports | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/AcademicReportsView.vue | GET /api/bgh/academic/reports | role: Principal | **API_CONNECTED** |
| /bgh/academic/pass-fail | bghMenuGroups.js | Layout_BGH | views/BGH/Academic/PassFailRatesView.vue | GET /api/bgh/academic/pass-fail | role: Principal | **API_CONNECTED** |
| /bgh/schedule/pending | bghMenuGroups.js | Layout_BGH | views/BGH/SchedulePendingView.vue | GET /api/bgh/schedules | role: Principal | **API_CONNECTED** |
| /bgh/schedule/conflicts | bghMenuGroups.js | Layout_BGH | views/BGH/Schedule/ConflictListView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/schedule/published | bghMenuGroups.js | Layout_BGH | views/BGH/Schedule/PublishedSchedulesView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/schedule/changes | bghMenuGroups.js | Layout_BGH | views/BGH/Schedule/ScheduleChangesView.vue | GET /api/bgh/schedule/changes | role: Principal | **API_CONNECTED** |
| /bgh/evaluations | bghMenuGroups.js | Layout_BGH | views/BGH/EvaluationsView.vue | GET /api/bgh/evaluations | role: Principal | **API_CONNECTED** |
| /bgh/evaluations/ranking | bghMenuGroups.js | Layout_BGH | views/BGH/Evaluations/TeacherRankingView.vue | GET /api/bgh/evaluations/ranking | role: Principal | **API_CONNECTED** |
| /bgh/evaluations/detail/:teacherId | bghMenuGroups.js | Layout_BGH | views/BGH/Evaluations/TeacherEvalDetailsView.vue | GET /api/bgh/evaluations/{teacherId} | role: Principal | **API_CONNECTED** |
| /bgh/evaluations/overview | bghMenuGroups.js | Layout_BGH | views/BGH/Evaluations/EvalOverviewView.vue | GET /api/bgh/evaluations/overview | role: Principal | **API_CONNECTED** |
| /bgh/evaluations/ai-analysis | bghMenuGroups.js | Layout_BGH | views/BGH/Evaluations/AIFeedbackAnalysisView.vue | GET /api/bgh/evaluations/ai-analysis | role: Principal | **API_CONNECTED** |
| /bgh/facilities | bghMenuGroups.js | Layout_BGH | views/BGH/FacilitiesView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/audit-logs | bghMenuGroups.js | Layout_BGH | views/BGH/AuditLogsView.vue | GET /api/bgh/audit-logs | role: Principal | **API_CONNECTED** |
| /bgh/profile | bghMenuGroups.js | Layout_BGH | views/BGH/ProfileView.vue | API unknown | role: Principal | **UNVERIFIED** |
| /bgh/notifications | bghMenuGroups.js | Layout_BGH | views/Student/NotificationsView.vue | GET /api/notifications | role: Principal | **WRONG_CONTEXT** |

## SuperAdmin (42 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /super-admin/dashboard | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/Dashboard.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/profile | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ProfileView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/organizations | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/OrganizationsView.vue | GET /api/organizations | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/users | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/UsersView.vue | GET /api/admin/users | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/roles-permissions | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/RolesPermissionsView.vue | GET /api/admin/rbac/roles | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/login-history | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/LoginHistoryView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/training/semesters | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SemestersView.vue | GET /api/master-data/academic-terms | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/training/programs | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ProgramsView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/training/subjects | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SubjectsView.vue | GET /api/master-data/subjects | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/training/courses | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/CoursesView.vue | GET /api/courses | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/training/exam-periods | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ExamPeriodsView.vue | GET /api/exam/ky-thi | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/operations/attendance-policy | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/AttendancePolicyView.vue | N/A | role: SuperAdmin|Admin | **STATIC_FUNCTIONAL** |
| /super-admin/operations/registration-periods | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/RegistrationPeriodsView.vue | GET /api/admin/registration-periods | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/operations/pass-fail-rules | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/PassFailRulesView.vue | N/A | role: SuperAdmin|Admin | **STATIC_FUNCTIONAL** |
| /super-admin/finance/tuition-config | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/Finance/TuitionConfigView.vue | GET /api/finance/program-tuition-configs | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/finance/student-debts | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/Finance/FinanceMonitorView.vue | API unknown | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/finance/payments | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/Finance/FinanceMonitorView.vue | API unknown | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/finance/refunds | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/Finance/FinanceMonitorView.vue | API unknown | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/support/tickets | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SupportTicketsView.vue | N/A | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/approvals/requests | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ApprovalsRequestsView.vue | GET /api/admin/applications | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/approvals/history | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ApprovalsHistoryView.vue | API unknown | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/approvals/reports | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/ApplicationReportsView.vue | GET /api/admin/applications/reports/overview | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/rewards-discipline | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/RewardDisciplineView.vue | GET /api/admin/reward-discipline/reports/overview | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/rewards/campaigns | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/RewardCampaignsView.vue | GET /api/admin/reward-campaigns | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/discipline/records | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/DisciplineRecordsView.vue | GET /api/admin/discipline-records | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/discipline/appeals | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/DisciplineAppealsView.vue | GET /api/admin/discipline-appeals | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/evaluations/config | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/EvaluationsConfigView.vue | N/A | role: SuperAdmin|Admin | **STATIC_FUNCTIONAL** |
| /super-admin/evaluations/results | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/EvaluationsResultsView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/awards | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/AwardsView.vue | API unknown | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/discipline | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/DisciplineView.vue | API unknown | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/reports/education-overview | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/EducationOverviewView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/reports/learning | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/LearningReportView.vue | N/A | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/reports/attendance | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/AttendanceReportView.vue | N/A | role: SuperAdmin|Admin | **HIDE_FROM_DEMO** |
| /super-admin/reports/campus-comparison | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/CampusComparisonView.vue | N/A | role: SuperAdmin|Admin | **STATIC_FUNCTIONAL** |
| /super-admin/reports/export | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/DataExportView.vue | N/A | role: SuperAdmin|Admin | **STATIC_FUNCTIONAL** |
| /super-admin/notifications/templates | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/NotificationTemplatesView.vue | GET /api/admin/notification-templates | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/notifications/send | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SendNotificationView.vue | POST /api/admin/notifications | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/notifications/history | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/NotificationHistoryView.vue | GET /api/admin/notifications | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/audit/logs | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/AuditLogsView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |
| /super-admin/security/alerts | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SecurityAlertsView.vue | GET /api/super-admin/security/alerts | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/system/modules | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/SystemModulesView.vue | GET /api/super-admin/system/modules | role: SuperAdmin|Admin | **API_CONNECTED** |
| /super-admin/system/ai-automation | superAdminMenuGroups.js | Layout_SuperAdmin | views/SuperAdmin/AiAutomationView.vue | API unknown | role: SuperAdmin|Admin | **UNVERIFIED** |

## Content Council (9 routes)

| Route | Menu | Layout | View | API | Permission | Status |
|-------|------|--------|------|-----|------------|--------|
| /content-council/subjects | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/subjects/SubjectListPage.vue | GET /api/master-data/subjects | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/question-bank | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/question-bank/QuestionBankPage.vue | GET /api/question-bank/questions | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/quizzes | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/quizzes/QuizListPage.vue | GET /api/exam/de-kiem-tra/search | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/quizzes/new | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/quizzes/QuizFormPage.vue | POST /api/exam/de-kiem-tra | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/quizzes/:quizId/edit | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/quizzes/QuizFormPage.vue | PUT /api/exam/de-kiem-tra/{id} | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/quizzes/:quizId/builder | contentCouncilMenuGroups.js | ContentCouncilLayout | pages/content-council/quizzes/QuizBuilderPage.vue | GET /api/exam/de-kiem-tra/{id}/cau-hoi | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/subjects/:subjectId/overview | contentCouncilMenuGroups.js | SubjectDetailLayout | pages/content-council/subjects/SubjectOverviewPage.vue | GET /api/master-data/subjects/{id} | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/subjects/:subjectId/editor | contentCouncilMenuGroups.js | SubjectDetailLayout | pages/content-council/subjects/SubjectEditorPage.vue | GET /api/curriculum/subjects/{id}/chapters | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |
| /content-council/subjects/:subjectId/preview | contentCouncilMenuGroups.js | None (fullscreen) | pages/content-council/subjects/SubjectPreviewPage.vue | GET /api/curriculum/lessons/{id}/content | role: HoiDongQuanLyNoiDung | **API_CONNECTED** |

## Summary Counts

| Metric | Count |
|--------|-------|
| Total routes (excluding redirects) | 173 |
| API_CONNECTED routes | 133 |
| STATIC_FUNCTIONAL routes | 13 |
| RUNTIME_VERIFIED routes | 1 |
| PLACEHOLDER routes | 1 |
| UNVERIFIED routes | 16 |
| WRONG_CONTEXT routes | 2 |
| HIDE_FROM_DEMO routes | 7 |
| MOCK_DATA routes | 0 |
| INLINE_DATA routes | 0 |
| BROKEN routes | 0 |
| Public routes | 6 (/, /about, /login/:portal, /login, /payment/success, /payment/cancel) + 1 404 |
| Routes reusing Student view across roles | 2 (teacher/notifications, bgh/notifications) |
| Routes without meta.title (risk) | 26 (see note below) |

---
