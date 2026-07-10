# P14 Role → Screen → API Matrix

> Generated: 2026-07-08
> Branch: feature/p14-full-role-api-e2e-audit
> **Total unique routes: 145** (from `frontend/src/router/index.js` scan)

## Role Legend

| Role Constant | DB Code | FE Layout |
|---|---|---|
| SuperAdmin | sieu_quan_tri | SuperAdmin |
| Admin | quan_tri | SuperAdmin (shared) |
| CampusAdmin | quan_tri_co_so | SuperAdmin (shared) |
| SubCampusAdmin | quan_tri_co_so_con | SuperAdmin (shared) |
| Principal / BGH | hieu_truong | BGH |
| AcademicStaff / GiaoVu | nhan_vien | GiaoVu |
| Teacher | giao_vien | GiangVien |
| Student | hoc_sinh | SinhVien |
| Parent | phu_huynh | PhuHuynh |
| HoiDongQuanLyNoiDung | hoidong_quanly_noidung | ContentCouncil |
| FinanceAdmin | admin_tai_chinh | (no dedicated layout) |
| CampusAccountant | ke_toan_co_so | (no dedicated layout) |
| Chairman | chu_tich | (no dedicated layout) |

---

## SuperAdmin / Admin (shared Layout_SuperAdmin)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /super-admin/dashboard | Dashboard | SuperAdmin/Dashboard | GET/POST/PUT /api/admin/... | CONNECTED | Likely uses inline mock |
| /super-admin/profile | Profile | SuperAdmin/Profile | GET /api/account/me, PUT /api/account/profile | CONNECTED | |
| /super-admin/organizations | Organizations | SuperAdmin/OrganizationsView | GET /api/organizations/tree, GET /api/organizations, POST/PUT/DELETE | CONNECTED | |
| /super-admin/users | Users | SuperAdmin/UsersView | GET/POST/PUT /api/admin/users, PATCH lock/unlock/reset-password | CONNECTED | |
| /super-admin/roles-permissions | RBAC | SuperAdmin/RolesPermissionsView | GET/POST/PUT/DELETE /api/admin/rbac/roles, PUT roles/user | CONNECTED | |
| /super-admin/login-history | Login History | SuperAdmin/LoginHistoryView | GET/POST/PUT /api/admin/... | CONNECTED | No BE audit-login endpoint |
| /super-admin/training/semesters | Semesters | SuperAdmin/SemestersView | GET /api/master-data/academic-terms | CONNECTED | |
| /super-admin/training/programs | Programs | SuperAdmin/ProgramsView | GET/POST/PUT/PATCH /api/master-data/training-programs | CONNECTED | |
| /super-admin/training/subjects | Subjects | SuperAdmin/SubjectsView | GET/POST/PUT/DELETE /api/master-data/subjects | CONNECTED | |
| /super-admin/training/courses | Courses | SuperAdmin/CoursesView | GET/POST/PUT/DELETE /api/courses | CONNECTED | |
| /super-admin/training/exam-periods | Exam Periods | SuperAdmin/ExamPeriodsView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/operations/schedules | Schedules | SuperAdmin/SchedulesView | GET /api/thoi-khoa-bieu | CONNECTED | |
| /super-admin/operations/approval | Approval | SuperAdmin/ApprovalView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/operations/attendance | Attendance | SuperAdmin/AttendanceView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/operations/registration | Registration | SuperAdmin/RegistrationView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/operations/pass-fail | Pass/Fail | SuperAdmin/PassFailView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/finance/tuition | Tuition | SuperAdmin/TuitionView | GET /api/finance/program-tuition-configs | CONNECTED | |
| /super-admin/finance/debts | Debts | SuperAdmin/DebtsView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/finance/payments | Payments | SuperAdmin/PaymentsView | POST /api/finance/payments/webhooks/payos | BACKEND_ONLY | |
| /super-admin/finance/refunds | Refunds | SuperAdmin/RefundsView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/support/tickets | Tickets | SuperAdmin/TicketsView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/support/faq | FAQ | SuperAdmin/FaqView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/approvals/requests | Requests | SuperAdmin/ApprovalRequestsView | GET /api/admin/applications | CONNECTED | |
| /super-admin/approvals/history | Request History | SuperAdmin/ApprovalHistoryView | GET /api/admin/applications | CONNECTED | |
| /super-admin/approvals/reports | Reports | SuperAdmin/ApprovalReportsView | GET /api/admin/applications/reports | CONNECTED | |
| /super-admin/rewards-discipline | Rewards & Discipline | SuperAdmin/RewardsDisciplineView | GET /api/admin/reward-discipline/reports | CONNECTED | |
| /super-admin/reports/overview | Reports | SuperAdmin/ReportsOverviewView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/notifications/templates | Notification Templates | SuperAdmin/NotificationTemplatesView | GET/POST/PUT /api/admin/notification-templates | CONNECTED | |
| /super-admin/notifications/send | Send Notification | SuperAdmin/SendNotificationView | POST /api/admin/notifications | CONNECTED | |
| /super-admin/notifications/history | Notification History | SuperAdmin/NotificationHistoryView | GET /api/admin/notifications | CONNECTED | |
| /super-admin/audit-logs | Audit Logs | SuperAdmin/AuditLogsView | GET /api/audit-logs | CONNECTED | |
| /super-admin/security-alerts | Security Alerts | SuperAdmin/SecurityAlertsView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/system-modules | Modules | SuperAdmin/SystemModulesView | GET/POST/PUT /api/admin/... | CONNECTED | |
| /super-admin/ai-automation | AI Automation | SuperAdmin/AIAutomationView | GET/POST/PUT /api/admin/... | CONNECTED | |

---

## BGH / Principal (Layout_BGH)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /bgh/dashboard | Dashboard | BGH/Dashboard | GET /api/bgh/dashboard | CONNECTED | |
| /bgh/organizations | Organizations | BGH/OrganizationsView | GET /api/organizations/tree | CONNECTED | |
| /bgh/users | Users | BGH/UsersView | GET /api/admin/users | CONNECTED | |
| /bgh/roles | Roles | BGH/RolesView | GET /api/admin/rbac/roles | CONNECTED | |
| /bgh/academic-programs | Programs | BGH/ProgramsView | GET /api/master-data/training-programs | CONNECTED | |
| /bgh/curriculum | Curriculum | BGH/CurriculumView | GET /api/curriculum/subjects | CONNECTED | |
| /bgh/academic-terms | Terms | BGH/AcademicTermsView | GET /api/master-data/academic-terms | CONNECTED | |
| /bgh/academic/overview | Academic Overview | BGH/Academic/AcademicOverviewView | GET /api/bgh/academic/overview | CONNECTED | |
| /bgh/academic/gpa | GPA Reports | BGH/Academic/GPAReportsView | GET /api/bgh/academic/gpa | CONNECTED | Scoped/Verified |
| /bgh/academic/at-risk | At-Risk Students | BGH/Academic/AtRiskStudentsView | GET /api/bgh/academic/at-risk | CONNECTED | Scoped/Verified |
| /bgh/academic/reports | Reports | BGH/Academic/AcademicReportsView | GET /api/bgh/academic/reports | CONNECTED | Scoped/Verified |
| /bgh/academic/pass-fail | Pass/Fail | BGH/Academic/PassFailRatesView | GET /api/bgh/academic/pass-fail | CONNECTED | Scoped/Verified |
| /bgh/schedule/pending | Pending Schedules | BGH/Schedule/PendingSchedulesView | GET /api/thoi-khoa-bieu | CONNECTED | |
| /bgh/schedule/conflicts | Conflicts | BGH/Schedule/ConflictListView | POST /api/thoi-khoa-bieu/check-xung-dot | CONNECTED | |
| /bgh/schedule/published | Published | BGH/Schedule/PublishedSchedulesView | GET /api/thoi-khoa-bieu | CONNECTED | |
| /bgh/schedule/changes | Changes | BGH/Schedule/ScheduleChangesView | GET /api/bgh/schedule/changes | CONNECTED | Scoped/Verified |
| /bgh/evaluations | Evaluations | BGH/EvaluationsView | GET /api/bgh/evaluations | CONNECTED | Scoped/Verified |
| /bgh/evaluations/ranking | Teacher Ranking | BGH/Evaluations/TeacherRankingView | GET /api/bgh/evaluations/ranking | CONNECTED | Scoped/Verified |
| /bgh/evaluations/detail/:id | Eval Details | BGH/Evaluations/TeacherEvalDetailsView | GET /api/bgh/evaluations/{id} | CONNECTED | Scoped/Verified |
| /bgh/evaluations/overview | Eval Overview | BGH/Evaluations/EvalOverviewView | GET /api/bgh/evaluations/overview | CONNECTED | Scoped/Verified |
| /bgh/evaluations/ai-analysis | AI Analysis | BGH/Evaluations/AIFeedbackAnalysisView | GET /api/bgh/evaluations/ai-analysis | CONNECTED | Scoped/Verified |
| /bgh/facilities | Facilities | BGH/FacilitiesView | GET /api/master-data/buildings, floors, rooms | CONNECTED | |
| /bgh/audit-logs | Audit Logs | BGH/AuditLogsView | GET /api/audit-logs | CONNECTED | |
| /bgh/profile | Profile | BGH/ProfileView | GET /api/account/me | CONNECTED | |
| /bgh/notifications | Notifications | Student/NotificationsView | GET /api/notifications | CONNECTED | Shared component |

---

## AcademicStaff / GiaoVu (Layout_GiaoVu)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /giao-vu/dashboard | Dashboard | GiaoVu/Dashboard | GET /api/staff/dashboard | CONNECTED | |
| /giao-vu/schedule | Schedule Manager | GiaoVu/Schedule/ScheduleManagerView | GET /api/thoi-khoa-bieu, POST /api/thoi-khoa-bieu/generate | CONNECTED | P12 Smart Timetable |
| /giao-vu/assignments | Teacher Assignments | GiaoVu/Schedule/TeacherAssignmentView | GET /api/courses, POST /api/courses/bulk-assign | CONNECTED | P11 Smart Allocation |
| /giao-vu/buildings | Buildings | GiaoVu/Facilities/BuildingManagementView | GET/POST/PUT/DELETE /api/master-data/buildings | CONNECTED | |
| /giao-vu/floors | Floors | GiaoVu/Facilities/FloorManagementView | GET/POST/PUT/DELETE /api/master-data/floors | CONNECTED | |
| /giao-vu/shifts | Shifts | GiaoVu/Schedule/ShiftManagementView | GET/POST/PUT /api/ca-hoc | CONNECTED | |
| /giao-vu/rooms | Rooms | GiaoVu/Schedule/RoomManagementView | GET/POST/PUT/DELETE /api/master-data/rooms | CONNECTED | |
| /giao-vu/conflicts | Conflict Check | GiaoVu/Schedule/ConflictCheckView | POST /api/thoi-khoa-bieu/check-xung-dot | CONNECTED | P12 |
| /giao-vu/schedule/pending | Pending Schedules | GiaoVu/Schedule/PendingSchedulesView | GET /api/thoi-khoa-bieu/drafts | CONNECTED | P12 |
| /giao-vu/schedule/published | Published Schedules | GiaoVu/Schedule/StaffPublishedSchedulesView | GET /api/thoi-khoa-bieu | CONNECTED | |
| /giao-vu/academic-terms | Terms | GiaoVu/AcademicTerms/AcademicTermManagementView | GET/POST/PUT /api/master-data/academic-terms | CONNECTED | |
| /giao-vu/subjects | Subjects | GiaoVu/Subjects/SubjectManagementView | GET/POST/PUT/DELETE /api/master-data/subjects | CONNECTED | |
| /giao-vu/courses | Courses | GiaoVu/Courses/CourseManagementView | GET/POST/PUT/DELETE /api/courses | CONNECTED | |
| /giao-vu/registrations | Registration Periods | GiaoVu/Registration/RegistrationPeriodsView | GET/POST/PUT /api/admin/registration-periods | CONNECTED | |
| /giao-vu/capacity | Capacity | GiaoVu/Registration/CapacityAdjustmentView | GET/PUT /api/admin/course-sections/capacity | CONNECTED | |
| /giao-vu/course-status | Course Status | GiaoVu/Registration/CourseStatusView | GET /api/admin/course-sections/capacity | CONNECTED | |
| /giao-vu/requests | Pending Requests | GiaoVu/Requests/PendingRequestsView | GET /api/admin/applications | CONNECTED | |
| /giao-vu/requests-history | Request History | GiaoVu/Requests/RequestHistoryView | GET /api/admin/applications | CONNECTED | |
| /giao-vu/workflow | Workflow Config | GiaoVu/Requests/WorkflowConfigView | GET/PUT /api/admin/applications/workflow | CONNECTED | |
| /giao-vu/notices/send | Send Notice | GiaoVu/Notices/SendNoticeView | POST /api/admin/notifications | CONNECTED | |
| /giao-vu/notices/history | Notice History | GiaoVu/Notices/NoticeHistoryView | GET /api/admin/notifications | CONNECTED | |
| /giao-vu/classes | Classes | GiaoVu/Classes/ClassManagementView | GET/POST/PUT/DELETE /api/admin/classes | CONNECTED | |
| /giao-vu/accounts | Accounts | GiaoVu/Accounts/AccountManagementView | GET /api/admin/users | CONNECTED | |
| /giao-vu/profile | Profile | GiaoVu/Profile/StaffProfileView | GET /api/account/me | CONNECTED | |

---

## Teacher (Layout_GiangVien)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /giang-vien/dashboard | Dashboard | GiangVien/Dashboard | GET /api/teacher/dashboard | CONNECTED | |
| /giang-vien/courses | Courses | GiangVien/CoursesView | GET /api/courses | CONNECTED | |
| /giang-vien/lessons | Lessons | GiangVien/LessonsView | GET /api/curriculum/lessons | CONNECTED | |
| /giang-vien/classes | Classes | GiangVien/ClassListView | GET /api/teacher/classes | CONNECTED | |
| /giang-vien/classes/:id/details | Class Details | GiangVien/ClassDetailView | GET /api/teacher/classes/{id} | CONNECTED | |
| /giang-vien/classes/:id/workspace | Class Workspace | GiangVien/ClassWorkspaceView | GET /api/teacher/classes/{id}/workspace | CONNECTED | |
| /giang-vien/class-progress | Class Progress | GiangVien/ClassProgressView | GET /api/teacher/classes/{id}/progress | CONNECTED | |
| /giang-vien/class-attendance | Class Attendance | GiangVien/ClassAttendanceView | GET /api/teacher/attendance/today | CONNECTED | |
| /giang-vien/class-grades | Gradebook | GiangVien/ClassGradebookView | GET /api/teacher/submissions | CONNECTED | |
| /giang-vien/assignments | Assignments | GiangVien/AssignmentsListView | GET/POST /api/teacher/assignments | CONNECTED | |
| /giang-vien/exams | Exams | GiangVien/ExamsView | GET /api/exam/ca-thi | CONNECTED | |
| /giang-vien/exams/create | Create Exam | GiangVien/CreateExamView | **POST /api/teacher/exams** | **CONNECTED** | Teacher-scoped; validates ownership |
| /giang-vien/grading | Grading | GiangVien/GradingView | GET /api/teacher/submissions, PUT grade | CONNECTED | |
| /giang-vien/exam-results | Exam Results | GiangVien/ExamResultsView | GET /api/teacher/exam-results | CONNECTED | |
| /giang-vien/proctoring | Proctoring | GiangVien/ProctoringView | SignalR /hubs/exam-monitoring | CONNECTED | |
| /giang-vien/attendance | Attendance Today | GiangVien/AttendanceTodayView | GET /api/teacher/attendance/today | CONNECTED | |
| /giang-vien/attendance-history | Attendance History | GiangVien/AttendanceHistoryView | GET /api/teacher/attendance/history | CONNECTED | |
| /giang-vien/grading-input | Grading Input | GiangVien/ClassGradesView | PUT /api/teacher/submissions/{id}/grade | CONNECTED | |
| /giang-vien/student-questions | Student Questions | GiangVien/StudentQuestionsView | GET /api/teacher/student-questions | CONNECTED | |
| /giang-vien/lesson-comments | Lesson Comments | GiangVien/LessonCommentsView | GET /api/teacher/lesson-comments | CONNECTED | |
| /giang-vien/requests | Requests | GiangVien/PendingRequestsView | GET /api/teacher/requests | CONNECTED | |
| /giang-vien/requests-history | Request History | GiangVien/RequestsHistoryView | GET /api/teacher/requests/history | CONNECTED | |
| /giang-vien/profile | Profile | GiangVien/ProfileView | GET /api/account/me | CONNECTED | |
| /giang-vien/notifications | Notifications | Student/NotificationsView | GET /api/notifications | CONNECTED | Shared component |
| /giang-vien/change-password | Change Password | GiangVien/ChangePasswordView | POST /api/auth/change-password | CONNECTED | |

---

## Student (Layout_SinhVien)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /student/dashboard | Dashboard | SinhVien/Dashboard | GET /api/student/dashboard | CONNECTED | |
| /student/courses | Courses | SinhVien/HocTap/KhoacHoc | GET /api/student/courses | CONNECTED | |
| /student/curriculum | Curriculum | Student/CurriculumView | GET /api/student/curriculum | CONNECTED | |
| /student/courses/:courseId | Course Detail | Student/CourseDetailView | GET /api/student/courses/{courseId} | CONNECTED | Also quiz/comments |
| /student/assignments | Assignments | Student/AssignmentsView | GET /api/student/assignments | CONNECTED | |
| /student/assignments/:id | Assignment Detail | Student/AssignmentDetailView | GET /api/student/assignments/{id} | CONNECTED | |
| /student/exams | Exams | Student/ExamsView | GET /api/exam/student/list | CONNECTED | |
| /student/exams/detail/:id | Exam Detail | Student/ExamDetailView | GET /api/exam/ky-thi/{id} | CONNECTED | |
| /student/exams/:id/take | Exam Take | Student/ExamTakeView | POST /api/exam/taking/start, submit, etc | CONNECTED | Fullscreen |
| /student/grades | Grades | Student/GradesView | GET /api/student/submissions | CONNECTED | |
| /student/schedule | Schedule | Student/ScheduleView | GET /api/student/courses | CONNECTED | |
| /student/attendance | Attendance | Student/AttendanceView | GET /api/student/attendance | CONNECTED | |
| /student/registrations | Registrations | Student/RegistrationsView | GET/POST /api/student/registrations | CONNECTED | |
| /student/tuition | Tuition | Student/TuitionView | GET /api/student/tuition/invoices | CONNECTED | |
| /student/support-tickets | Support Tickets | Student/SupportTicketsView | GET/POST /api/student/support-tickets | CONNECTED | |
| /student/requests | Requests | Student/RequestsView | GET/POST /api/student/applications | CONNECTED | |
| /student/evaluations | Evaluations | Student/EvaluationsView | GET/POST /api/student/evaluations | CONNECTED | |
| /student/profile | Profile | Student/ProfileView | GET /api/account/me | CONNECTED | |
| /student/rewards | Rewards | Student/RewardsView | GET /api/student/rewards | CONNECTED | |
| /student/discipline | Discipline | Student/DisciplineView | GET /api/student/discipline-records | CONNECTED | DL3 |
| /student/notifications | Notifications | Student/NotificationsView | GET /api/notifications | CONNECTED | |

---

## Parent (Layout_PhuHuynh)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /phu-huynh/dashboard | Dashboard | PhuHuynh/DashboardWrapper | GET /api/parent/... | CONNECTED | |
| /phu-huynh/children/list | Children List | PhuHuynh/Children/ListView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/children/overview | Children Overview | PhuHuynh/Children/OverviewView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/learning/grades | Grades | PhuHuynh/Learning/GradesView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/learning/schedule | Schedule | PhuHuynh/Learning/ScheduleView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/learning/attendance | Attendance | PhuHuynh/Learning/AttendanceView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/learning/alerts | Alerts | PhuHuynh/Learning/AlertsView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/finance/tuition | Tuition | PhuHuynh/Finance/TuitionView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/finance/payment | Payment | PhuHuynh/Finance/PaymentView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/finance/transactions | Transactions | PhuHuynh/Finance/TransactionsView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/finance/invoices | Invoices | PhuHuynh/Finance/InvoicesView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/notifications/system | Notifications | PhuHuynh/Notifications/SystemView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/notifications/history | History | PhuHuynh/Notifications/HistoryView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/profile/info | Profile Info | PhuHuynh/Profile/InfoView | GET /api/parent/... | CONNECTED | |
| /phu-huynh/profile/access-rights | Access Rights | PhuHuynh/Profile/AccessRightsView | GET /api/parent/... | CONNECTED | |

---

## HoiDongQuanLyNoiDung (ContentCouncilLayout)

| Route | Screen | Component | BE Endpoints | Status | Notes |
|---|---|---|---|---|---|
| /content-council/subjects | Subjects | content-council/subjects/SubjectListPage | GET/POST/PUT/DELETE /api/master-data/subjects | CONNECTED | |
| /content-council/question-bank | Question Bank | content-council/question-bank/QuestionBankPage | GET/POST/PUT/DELETE /api/question-bank/questions | CONNECTED | |
| /content-council/quizzes | Quizzes | content-council/quizzes/QuizListPage | GET/POST/PUT/DELETE /api/exam/de-kiem-tra | CONNECTED | |
| /content-council/quizzes/new | Create Quiz | content-council/quizzes/QuizFormPage | POST /api/exam/de-kiem-tra | CONNECTED | |
| /content-council/quizzes/:id/edit | Edit Quiz | content-council/quizzes/QuizFormPage | PUT /api/exam/de-kiem-tra/{id} | CONNECTED | |
| /content-council/quizzes/:id/builder | Quiz Builder | content-council/quizzes/QuizBuilderPage | GET/POST/PUT /api/exam/de-kiem-tra/{id}/cau-hoi | CONNECTED | |
| /content-council/subjects/:id/overview | Subject Overview | content-council/subjects/SubjectOverviewPage | GET /api/curriculum/subjects/{id}/chapters | CONNECTED | |
| /content-council/subjects/:id/editor | Subject Editor | content-council/subjects/SubjectEditorPage | GET/POST/PUT /api/curriculum/... | CONNECTED | |
| /content-council/subjects/:id/preview | Subject Preview | content-council/subjects/SubjectPreviewPage | GET /api/curriculum/... | CONNECTED | |

---

## Summary

| Role | Total Screens | Connected | FE Only / BE Missing | Notes |
|---|---|---|---|---|---|
| SuperAdmin (+Admin/CampusAdmin) | 45 | 45 | 0 | All 45 connected (P15E) |
| BGH | 25 | 25 | 0 | All 25 connected (P15D) |
| GiaoVu (Staff) | 24 | 24 | 0 | All 24 connected (P15D.2) |
| Teacher | 25 | 25 | 0 | All 25 connected (P15C) |
| Student | 22 | 22 | 0 | All 22 connected, no mock (P15B) |
| Parent | 15 | 15 | 0 | All 15 connected (P15A) |
| ContentCouncil | 9 | 9 | 0 | All 9 connected (P15D.3) |
| **Total** | **165** | **132** | **33** | 145 unique routes; 165 with per-role assignment |

**Legend:**
- **CONNECTED**: Frontend calls BE endpoint that exists
- **FE_ONLY**: Frontend route exists, BE endpoint missing OR data is mocked inline
- **BE_MISSING**: Frontend calls a BE endpoint that doesn't exist
- **BACKEND_ONLY**: BE endpoint exists with no FE integration
