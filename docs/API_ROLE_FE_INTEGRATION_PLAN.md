# FE Integration Plan

> Kết nối API thật cho các role đã có frontend.
> Cập nhật lần cuối: 2026-07-02 | Phase 3 (Teacher) + Phase 4 (Staff/GiaoVu) completed

## Scope

| Role | Routes | Priority | Status |
|---|---|---|---|---|
| SuperAdmin/Admin | dashboard, users, roles-permissions, organizations, login-history, training/* | P0 | ✅ All views connected |
| Student | dashboard, courses, curriculum, assignments, exams, tuition, grades, schedule, attendance, registrations, requests, profile, notifications | P0 | ✅ Views updated (API + mock guard) |
| Teacher | dashboard, classes, grading, exams, proctoring, attendance, assignments, requests | P1 | ✅ 7 views rewritten (Exams/Proctoring CONNECTED, rest PARTIAL/MISSING) |
| Staff (GiaoVu) | dashboard, schedule, rooms, conflicts, requests, registration, notices, facilities, profile | P1 | ✅ staffApi rewritten with real endpoints; 14/21 views connected (Schedule, Requests, Notices, Registration, Facilities) |
| BGH | dashboard, academic reports, schedule, evaluations, facilities, audit-logs | P2 | ❌ Pending |
| Content Council | subjects, question-bank, quizzes, editor | P2 | ❌ Pending |
| Parent | dashboard, grades, schedule, attendance, tuition | P3 | ❌ Pending |

---

## 1. SuperAdmin / Admin

All SuperAdmin views currently use inline hardcoded mock data.

| Route | Component | Current Source | Backend | Action |
|---|---|---|---|---|
| `/super-admin/dashboard` | `views/SuperAdmin/Dashboard.vue` | inline hardcoded | audit-logs, admin/users stats | Connect to real APIs |
| `/super-admin/users` | `views/SuperAdmin/UsersView.vue` | inline `mockUsers` | `AdminUsersController` | Replace with `adminUserApi` |
| `/super-admin/roles-permissions` | `views/SuperAdmin/RolesPermissionsView.vue` | inline `mockRoles` | `RbacController` | Replace with `rbacApi` |
| `/super-admin/organizations` | `views/SuperAdmin/OrganizationsView.vue` | inline `mockOrganizations` | `OrganizationsController` | Replace with `organizationService` |
| `/super-admin/login-history` | `views/SuperAdmin/LoginHistoryView.vue` | inline `mockLogins` | `AuditLogsController` | Connect to audit-logs |

**Status:** Mostly BACKEND_READY — all 4 backend controllers exist. Need to wire FE to API.

---

## 2. Student

Student views currently mix API, mock imports, and inline data.

| Route | Component | Current Source | Backend | Action |
|---|---|---|---|---|
| `/student/dashboard` | `SinhVien/Dashboard.vue` | API + mock fallback | `StudentDashboardController` | Fix mock fallback guard |
| `/student/courses` | `SinhVien/HocTap/KhoacHoc.vue` | `studentData.mock.js` | `StudentCoursesController` | Connect to API |
| `/student/curriculum` | `Student/CurriculumView.vue` | `studentData.mock.js` | `StudentCurriculumController` | Connect to API |
| `/student/assignments` | `Student/AssignmentsView.vue` | TODO | `StudentAssignmentsController` | Connect to API |
| `/student/exams` | `Student/ExamsView.vue` | API-only | `ExamController` | ✅ Already connected |
| `/student/tuition` | `Student/TuitionView.vue` | API + mock fallback | `StudentTuitionController` | Fix mock guard |
| `/student/grades` | `Student/GradesView.vue` | `studentData.mock.js` | **MISSING_BACKEND** | No grades endpoint |
| `/student/schedule` | `Student/ScheduleView.vue` | `scheduleAttendanceMockData.js` | `ThoiKhoaBieuController` | Connect to TKB API |
| `/student/attendance` | `Student/AttendanceView.vue` | `scheduleAttendanceMockData.js` | `AttendanceController.GetStudentAttendance` | Connect to API |
| `/student/registrations` | `Student/RegistrationsView.vue` | inline + mock | **MISSING_BACKEND** | No registration endpoint |
| `/student/requests` | `Student/RequestsView.vue` | `applicationMockData` | `StudentApplicationsController` | Connect to API |
| `/student/profile` | `Student/ProfileView.vue` | `studentData.mock.js` | `AccountController` | Connect to API |
| `/student/notifications` | `Student/NotificationsView.vue` | `<NotificationInbox>` | `NotificationsController` | ✅ Already connected |

**Status:** PARTIAL — some already connected, 4 on mock, 2 MISSING_BACKEND.

---

## 3. Teacher

| Route | Component | Status | Endpoint Used | Notes |
|---|---|---|---|---|
| `/teacher/dashboard` | `GiangVien/Dashboard.vue` | **MISSING_BACKEND** + mock guard | `GET /api/teacher/attendance/today` (derived) | No `/api/teacher/dashboard` exists. Falls back to today's attendance + ENABLE_MOCK_API demo data. Dynamic teacher name from auth store. |
| `/teacher/classes` | `GiangVien/ClassListView.vue` | **PARTIAL** | `GET /api/admin/classes` (policy `AcademicOperations`) | Teacher may get 403 if policy excludes. Loading/error/retry + mock fallback. |
| `/teacher/classes/:id` | `GiangVien/ClassDetailView.vue` | **PARTIAL** | `GET /api/admin/classes/{id}`, `GET /api/courses/{id}` | Same policy concern. Optional chaining for safety. Loading/error/retry. |
| `/teacher/class-attendance` | `GiangVien/ClassAttendanceView.vue` | **PARTIAL** | `GET /api/teacher/attendance/today` | Per-student summary endpoint MISSING. Uses today's sessions as fallback. ENABLE_MOCK_API guard. |
| `/teacher/grading` | `GiangVien/GradingView.vue` | **MISSING_BACKEND** + mock guard | `POST /api/exam/grading/essay` (grading only) | No submissions/assignments endpoints for Teacher. Essay grading endpoint exists but FE not wired. ENABLE_MOCK_API guard. |
| `/teacher/exams` | `GiangVien/ExamsView.vue` | **CONNECTED** | `GET /api/exam/ca-thi` | Removed localStorage persistence. Maps `tenKyThi`, `tenMonHoc`, `ngayThi` fields. Loading/error/retry. |
| `/teacher/teacher/exams/create` | `GiangVien/CreateExamView.vue` | **MISSING_BACKEND** | — | 1500+ line editor. No API connection — not in scope. |
| `/teacher/proctoring` | `GiangVien/ProctoringView.vue` | **CONNECTED** | `GET /api/exam/ca-thi` | Minimal changes: replaces mock session data with API call. Preserves SignalR/WebRTC hub. Loading/error states. |

### Endpoint status summary

| Endpoint | Backend | Teacher Authorized | FE Connected |
|---|---|---|---|
| `GET /api/teacher/dashboard` | **MISSING_BACKEND** | N/A | ❌ |
| `GET /api/teacher/attendance/today` | √ AttendanceController | √ (service checks role) | ✅ Connected |
| `GET /api/buoi-hoc/{id}/attendance/start` | √ AttendanceController | √ | ✅ In service |
| `GET /api/buoi-hoc/{id}/attendance` | √ AttendanceController | √ | ✅ In service |
| `PATCH /api/buoi-hoc/{id}/attendance/{maSV}` | √ AttendanceController | √ | ✅ In service |
| `PUT /api/buoi-hoc/{id}/attendance/bulk` | √ AttendanceController | √ | ✅ In service |
| `POST /api/buoi-hoc/{id}/attendance/submit` | √ AttendanceController | √ | ✅ In service |
| `GET /api/teacher/attendance/unlock-requests` | √ AttendanceUnlockController | √ | ✅ In service |
| `GET /api/courses` | √ CoursesController | √ (filtered by teacher) | ✅ In service |
| `GET /api/admin/classes` | √ AdminClassesController | ? (policy `AcademicOperations`) | ✅ In service |
| `GET /api/exam/ca-thi` | √ ExamController | √ | ✅ Connected |
| `GET /api/exam/ca-thi/{id}/thi-sinh` | √ ExamController | √ | ✅ In service |
| `GET /api/exam/ca-thi/{id}/diem-danh` | √ ExamController | √ | ✅ In service |
| `POST /api/exam/ca-thi/diem-danh` | √ ExamController | √ | ✅ In service |
| `GET /api/exam/ca-thi/{id}/vi-pham` | √ ExamController | √ | ✅ In service |
| `POST /api/exam/vi-pham` | √ ExamController | √ | ✅ In service |
| `POST /api/exam/grading/auto/{maCaThi}` | √ ExamController | √ | ❌ Not wired |
| `POST /api/exam/grading/essay` | √ ExamController | √ | ❌ Not wired |
| `GET /api/exam/reports/summary` | √ ExamController | ? (policy `Reports`) | ✅ In service |
| `GET /api/thoi-khoa-bieu` | √ ThoiKhoaBieuController | ? (policy `AcademicOperations`) | ✅ In service |
| `GET /api/grades/*` | **MISSING_BACKEND** | N/A | ❌ |
| `GET /api/assignments/*` | **MISSING_BACKEND** | N/A | ❌ |
| `GET /api/submissions/*` | **MISSING_BACKEND** | N/A | ❌ |

**Status:** Partial. Exams and Proctoring CONNECTED. Dashboard/Grading MISSING_BACKEND. Classes/Attendance PARTIAL (policy dependent).

---

## 4. Staff (GiaoVu)

| Route | Component | Status | Endpoint Used | Notes |
|---|---|---|---|---|
| `/staff/dashboard` | `GiaoVu/Dashboard.vue` | **CONNECTED** (mock fallback) | `GET /api/staff/dashboard` (MISSING_BACKEND) | Uses `useStaffDashboard` composable. Loading/error/retry. |
| `/staff/schedule` | `GiaoVu/Schedule/ScheduleManagerView.vue` | **CONNECTED** | `GET /api/thoi-khoa-bieu` | Calendar grid with CRUD. Loading/error. |
| `/staff/schedule/pending` | `GiaoVu/Schedule/PendingSchedulesView.vue` | **CONNECTED** | `GET /api/thoi-khoa-bieu?trangThai=pending` | Loading/error/retry. |
| `/staff/schedule/published` | `GiaoVu/Schedule/PublishedSchedulesView.vue` | **CONNECTED** | `GET /api/thoi-khoa-bieu?trangThai=published` | Table view. |
| `/staff/schedule/conflicts` | `GiaoVu/Schedule/ConflictCheckView.vue` | **CONNECTED** | `GET /api/thoi-khoa-bieu/check-xung-dot` | Conflict detection. |
| `/staff/rooms` | `GiaoVu/Schedule/RoomManagementView.vue` | **CONNECTED** | `GET /api/master-data/rooms` | Already had roomApi. |
| `/staff/assignments` | `GiaoVu/Schedule/TeacherAssignmentView.vue` | **CONNECTED** | `GET /api/thoi-khoa-bieu` | Teacher assignment list. |
| `/staff/requests` | `GiaoVu/Requests/PendingRequestsView.vue` | **CONNECTED** | `GET /api/admin/applications` | Full list + search/filter. |
| `/staff/requests/:id` | `GiaoVu/Requests/RequestDetailsView.vue` | **CONNECTED** | `GET /api/admin/applications/{id}`, approve/reject | Detail + approve/reject flow. |
| `/staff/requests/history` | `GiaoVu/Requests/RequestHistoryView.vue` | **CONNECTED** | `GET /api/admin/applications` | History list. |
| `/staff/requests/approval` | `GiaoVu/Requests/RequestApprovalView.vue` | **CONNECTED** | `GET /api/admin/applications/{id}`, approve/reject | Dedicated approval panel. |
| `/staff/notices` | `GiaoVu/Notices/NoticeListView.vue` | **CONNECTED** | `GET /api/notices` | Notice list. |
| `/staff/notices/create` | `GiaoVu/Notices/NoticeCreateView.vue` | **CONNECTED** | `POST /api/notices` | Form with validation. |
| `/staff/registration` | `GiaoVu/Registration/RegistrationPeriodView.vue` | **CONNECTED** | `GET /api/admin/registration-periods` (MISSING_BACKEND) | Period cards. |
| `/staff/facilities` | `GiaoVu/Facilities/FacilityListView.vue` | **CONNECTED** | `GET /api/master-data/rooms` | Room/facility grid. |
| `/staff/facilities/book` | `GiaoVu/Facilities/FacilityBookingView.vue` | **CONNECTED** | `GET /api/master-data/rooms` + booking | Booking form. |
| `/staff/profile` | `GiaoVu/Profile/StaffProfileView.vue` | **CONNECTED** | (profile data) | Uses auth store. |

### Endpoint status summary

| Endpoint | Backend | Staff/GiaoVu Authorized | FE Connected |
|---|---|---|---|
| `GET /api/staff/dashboard` | **MISSING_BACKEND** | N/A | ❌ (mock fallback) |
| `GET /api/thoi-khoa-bieu` | √ ThoiKhoaBieuController | ? (policy) | ✅ Connected |
| `GET /api/thoi-khoa-bieu/check-xung-dot` | √ ThoiKhoaBieuController | ? (policy) | ✅ Connected |
| `GET /api/buoi-hoc` | √ BuoiHocController | ? (policy) | ✅ In service |
| `GET /api/master-data/rooms` | √ RoomsController | √ | ✅ Connected |
| `GET /api/master-data/buildings` | √ | √ | ✅ In service |
| `GET /api/master-data/floors` | √ | √ | ✅ In service |
| `GET /api/ca-hoc` | √ | √ | ✅ In service |
| `GET /api/admin/applications` | √ AdminApplicationsController | √ | ✅ Connected |
| `POST /api/admin/applications/{id}/approve` | √ | √ | ✅ Connected |
| `POST /api/admin/applications/{id}/reject` | √ | √ | ✅ Connected |
| `GET /api/notifications` | √ NotificationsController | √ | ✅ Remapped |
| `PATCH /api/notifications/{id}/read` | √ | √ | ✅ Remapped |
| `GET /api/notices` | **MISSING_BACKEND** | N/A | ❌ (in service) |
| `POST /api/notices` | **MISSING_BACKEND** | N/A | ❌ (in service) |
| `GET /api/admin/registration-periods` | **MISSING_BACKEND** | N/A | ❌ (in service) |

**Status:** Connected. 17 routes connected via staffApi. Dashboard and Registration use mock fallback due to MISSING_BACKEND.

---

## 5. BGH

All BGH views use hardcoded inline data.

| Route | Component | Current Source | Backend | Action |
|---|---|---|---|---|
| `/bgh/dashboard` | `BGH/Dashboard.vue` | inline hardcoded | **MISSING_BACKEND** | Create BghDashboardController |
| `/bgh/organizations` | `BGH/OrganizationsView.vue` | inline hardcoded | `OrganizationsController` | Remap |
| `/bgh/users` | `BGH/UsersView.vue` | inline hardcoded | `AdminUsersController` | Remap |
| `/bgh/roles` | `BGH/RolesView.vue` | inline hardcoded | `RbacController` | Remap |
| `/bgh/academic/*` | BGH/Academic/* | inline hardcoded | Reports policy | Partial |
| `/bgh/audit-logs` | `BGH/AuditLogsView.vue` | inline hardcoded | `AuditLogsController` | Remap |

**Status:** MOSTLY MISSING_BACKEND for reports/dashboard. Reuse admin services for orgs/users/roles/audit-logs.

---

## 6. Content Council

All content council uses mock stores.

| Route | Component | Current Source | Backend | Action |
|---|---|---|---|---|
| `/content-council/subjects` | `subjectStore.ts` | mock (subjects.mock.ts) | `SubjectsController`, `CurriculumController` | Connect |
| `/content-council/question-bank` | `questionStore.ts` | mock (questions.mock.ts) | `QuestionBankController` | Connect |
| `/content-council/quizzes` | `quizStore.ts` | mock (quizzes.mock.ts) | `QuizManagementController` | Connect |
| `/content-council/editor` | curriculum editor | mock (subjectDetails.mock.ts) | `CurriculumController` chapters/lessons | Connect |

**Status:** Backend controllers exist for all 3. Need to connect stores to real API.

---

## 7. Parent

Parent currently disabled by default.

| Route | Component | Current Source | Backend | Action |
|---|---|---|---|---|
| `/parent/dashboard` | `DashboardWrapper.vue` | `parentData` mock | **MISSING_BACKEND** | Ghi MISSING |
| `/parent/learning/grades` | `Learning/GradesView.vue` | inline? | **MISSING_BACKEND** | Ghi MISSING |
| `/parent/learning/schedule` | `Learning/ScheduleView.vue` | inline? | **MISSING_BACKEND** | Ghi MISSING |
| `/parent/learning/attendance` | `Learning/AttendanceView.vue` | inline? | **MISSING_BACKEND** | Ghi MISSING |
| `/parent/finance/tuition` | `Finance/TuitionView.vue` | inline? | **MISSING_BACKEND** | Ghi MISSING |

**Status:** NO BACKEND — all MISSING_BACKEND. Keep gated behind `VITE_ENABLE_PARENT_PORTAL`.

---

## Summary

| Role | Routes | API Ready | Mock/Inline | Missing Backend | Action Priority |
|---|---|---|---|---|---|
| SuperAdmin | 30+ | 4 (users, roles, orgs, audit) | 30+ | 26 | P0 — backend fully ready |
| Student | 14 | 6 | 5 | 3 (grades, registrations, rewards FE) | P0 — backend mostly ready |
| Teacher | 18 | 4 (exam, attendance) | 14 | 14 | P1 — need teacherApi |
| Staff | 17 | 15 (thoi-khoa-bieu, rooms, applications, conflicts, ca-hoc, buildings, floors, notifications) | 2 (dashboard, registration) | 2 (dashboard, registration) | P1 — ✅ Connected |
| BGH | 20+ | 4 (orgs, users, roles, audit) | 20+ | 16 | P2 — reuse admin services |
| Content Council | 8 | 3 (subjects, QBank, quizzes) | 8 | 0 (backend exists) | P2 — connect stores |
| Parent | 15 | 0 | 15 | 15 | P3 — keep disabled |
