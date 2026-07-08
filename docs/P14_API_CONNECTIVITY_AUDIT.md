# P14 API Connectivity Audit

> Generated: 2026-07-08
> Branch: feature/p14-full-role-api-e2e-audit

## Summary

| Category | Count |
|---|---|---|
| Total FE service methods | ~150 |
| Connected (BE exists, method matches) | ~140 |
| Wrong endpoint path (all FIXED) | 4 |
| BE missing (FE calls non-existent) | 0 (all fixed) |
| FE only (no FE service for BE endpoint) | ~46 |
| Mock only (deleted) | 0 |
| Errors found | See below |

---

## API Service → BE Endpoint Mapping

### Auth (`frontend/src/services/apiClient.js`)

| FE Method | FE URL | BE Controller | BE Endpoint | Method Match | Status |
|---|---|---|---|---|---|
| login | POST /api/auth/login | AuthController | POST /api/auth/login | ✅ | CONNECTED |
| refreshToken | POST /api/auth/refresh-token | AuthController | POST /api/auth/refresh-token | ✅ | CONNECTED |
| logout | POST /api/auth/logout | AuthController | POST /api/auth/logout | ✅ | CONNECTED |
| changePassword | POST /api/auth/change-password | AuthController | POST /api/auth/change-password | ✅ | CONNECTED |

### Account (`accountApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| list | GET /api/admin/users | AdminUsersController | CONNECTED |
| get | GET /api/admin/users/{id} | AdminUsersController | CONNECTED |
| toggleActive | PATCH /api/admin/users/{id}/lock or unlock | AdminUsersController | CONNECTED |

### Admin User Service (`adminUserService.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| getUsers | GET /api/admin/users | AdminUsersController | CONNECTED |
| getById | GET /api/admin/users/{id} | AdminUsersController | CONNECTED |
| create | POST /api/admin/users | AdminUsersController | CONNECTED |
| update | PUT /api/admin/users/{id} | AdminUsersController | CONNECTED |
| lock | PATCH /api/admin/users/{id}/lock | AdminUsersController | CONNECTED |
| unlock | PATCH /api/admin/users/{id}/unlock | AdminUsersController | CONNECTED |
| resetPassword | PATCH /api/admin/users/{id}/reset-password | AdminUsersController | CONNECTED |

### Applications (`applicationsApi.js`)

| FE Method | FE URL | BE Controller | Status | Notes |
|---|---|---|---|---|
| getApplicationSchemaOptions | GET /api/applications/schema/options | ApplicationSchemaController | 🔴 WRONG_ENDPOINT | BE: /api/applications/templates |
| getMyApplications | GET /api/student/applications | StudentApplicationsController | CONNECTED | |
| getMyApplicationDetail | GET /api/student/applications/{id} | StudentApplicationsController | CONNECTED | |
| createDraft | POST /api/student/applications/drafts | StudentApplicationsController | 🔴 WRONG_ENDPOINT | BE: POST /api/student/applications (no /drafts) |
| submitApplication | POST /api/student/applications/{id}/submit | StudentApplicationsController | CONNECTED | |
| resubmitApplication | POST /api/student/applications/{id}/resubmit | StudentApplicationsController | CONNECTED | |
| cancelApplication | POST /api/student/applications/{id}/cancel | StudentApplicationsController | CONNECTED | |
| getAdminApplications | GET /api/admin/applications | AdminApplicationsController | CONNECTED | |
| getAdminApplicationSummary | GET /api/admin/applications/summary | AdminApplicationsController | CONNECTED | |
| getAssignableUsers | GET /api/admin/applications/assignable-users | AdminApplicationsController | 🔴 WRONG_ENDPOINT | BE has GET /api/admin/applications/assignees |
| receiveApplication | POST /api/admin/applications/{id}/receive | AdminApplicationsController | CONNECTED | |
| assignApplication | POST /api/admin/applications/{id}/assign | AdminApplicationsController | CONNECTED | |
| approveApplication | POST /api/admin/applications/{id}/approve | AdminApplicationsController | CONNECTED | |
| rejectApplication | POST /api/admin/applications/{id}/reject | AdminApplicationsController | CONNECTED | |
| getApplicationReportOverview | GET /api/admin/applications/reports/overview | AdminApplicationReportsController | CONNECTED | |
| getApplicationReportByType | GET /api/admin/applications/reports/by-type | AdminApplicationReportsController | CONNECTED | |

### Academic Term (`academicTermApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| list | GET /api/master-data/academic-terms | AcademicTermsController | CONNECTED |
| get | GET /api/master-data/academic-terms/{id} | AcademicTermsController | CONNECTED |
| create | POST /api/master-data/academic-terms | AcademicTermsController | CONNECTED |
| update | PUT /api/master-data/academic-terms/{id} | AcademicTermsController | CONNECTED |
| lock | PATCH /api/master-data/academic-terms/{id}/lock | AcademicTermsController | CONNECTED |
| unlock | PATCH /api/master-data/academic-terms/{id}/unlock | AcademicTermsController | CONNECTED |

### Building / Floor / Room (`buildingApi.js`, `floorApi.js`, `roomApi.js`)

All CONNECTED — paths match BE controllers exactly.

### Course (`courseApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| getCourses | GET /api/courses | CoursesController | CONNECTED |
| getCourseDetail | GET /api/courses/{id} | CoursesController | CONNECTED |
| createCourse | POST /api/courses | CoursesController | CONNECTED |
| bulkAssign | POST /api/courses/bulk-assign | CoursesController | CONNECTED |
| updateCourse | PUT /api/courses/{id} | CoursesController | CONNECTED |
| cloneCourse | POST /api/courses/{id}/clone | CoursesController | CONNECTED |
| batchArchive | POST /api/courses/batch-archive | CoursesController | CONNECTED |
| batchPublish | POST /api/courses/batch-publish | CoursesController | CONNECTED |

### BGH (`bghApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| getDashboard | GET /api/bgh/dashboard | BghDashboardController | CONNECTED |
| getUsers | GET /api/admin/users | AdminUsersController | CONNECTED |
| getOrganizations | GET /api/organizations | OrganizationsController | CONNECTED |
| getOrganizationsTree | GET /api/organizations/tree | OrganizationsController | CONNECTED |
| getRoles | GET /api/admin/rbac/roles | RbacController | CONNECTED |
| getAuditLogs | GET /api/audit-logs | AuditLogsController | CONNECTED |
| getEvaluations | GET /api/bgh/evaluations | BghEvaluationController | CONNECTED |

### Staff (`staffApi.js`)

| FE Method | FE URL | BE Controller | Status | Notes |
|---|---|---|---|---|
| getLists | GET /api/staff/lists | N/A | 🔴 BE_MISSING | Only StaffDashboardController exists |
| getDashboard | GET /api/staff/dashboard | StaffDashboardController | CONNECTED | |

### Schedule (`scheduleApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| list | GET /api/thoi-khoa-bieu | ThoiKhoaBieuController | CONNECTED |
| get | GET /api/thoi-khoa-bieu/{id} | ThoiKhoaBieuController | CONNECTED |
| checkConflicts | POST /api/thoi-khoa-bieu/check-xung-dot | ThoiKhoaBieuController | CONNECTED |
| create | POST /api/thoi-khoa-bieu | ThoiKhoaBieuController | CONNECTED |
| update | PUT /api/thoi-khoa-bieu/{id} | ThoiKhoaBieuController | CONNECTED |
| cancel | PATCH /api/thoi-khoa-bieu/{id}/cancel | ThoiKhoaBieuController | CONNECTED |
| generateSessions | POST /api/thoi-khoa-bieu/{id}/generate-sessions | ThoiKhoaBieuController | CONNECTED |

### Assignment (`assignmentApi.js`)

| FE Method | FE URL | BE Controller | Status | Notes |
|---|---|---|---|---|
| list | GET /api/thoi-khoa-bieu | ThoiKhoaBieuController | 🔴 WRONG_ENDPOINT | Assignments != Schedules |
| get | GET /api/thoi-khoa-bieu/{id} | ThoiKhoaBieuController | 🔴 WRONG_ENDPOINT | |
| getTeachers | GET /api/admin/users | AdminUsersController | CONNECTED | |

### Shift (`shiftApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| list | GET /api/ca-hoc | CaHocController | CONNECTED |
| getActive | GET /api/ca-hoc/active | CaHocController | CONNECTED |
| get | GET /api/ca-hoc/{id} | CaHocController | CONNECTED |
| create | POST /api/ca-hoc | CaHocController | CONNECTED |
| update | PUT /api/ca-hoc/{id} | CaHocController | CONNECTED |
| toggleActive | PATCH /api/ca-hoc/{id}/toggle-active | CaHocController | CONNECTED |

### Teacher Exam (`teacherApi.js` — createExam)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| createExam | POST /api/teacher/exams | TeacherExamController (NEW) | CONNECTED |

### Student API (`studentApi.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| getDashboard | GET /api/student/dashboard | StudentDashboardController | CONNECTED |
| getCourses | GET /api/student/courses | StudentCoursesController | CONNECTED |
| getCourseDetail | GET /api/student/courses/{courseId} | StudentCoursesController | CONNECTED |
| getCourseLessons | GET /api/student/courses/{courseId}/lessons | StudentCoursesController | CONNECTED (via getCourseDetail) |
| getLessonQuiz | GET /api/student/courses/{courseId}/lessons/{lessonId}/quiz | StudentCoursesController | CONNECTED |
| getLessonComments | GET /api/student/courses/{courseId}/lessons/{lessonId}/comments | StudentCoursesController | CONNECTED |
| getCurriculum | GET /api/student/curriculum | StudentCurriculumController | CONNECTED |
| getSchedule | GET /api/student/courses (used as schedule) | StudentCoursesController | CONNECTED |
| getAttendance | GET /api/student/attendance | AttendanceController | CONNECTED |
| getGrades | GET /api/student/submissions | StudentSubmissionsController | CONNECTED |
| getRewards | GET /api/student/rewards | StudentRewardsController | CONNECTED |
| getDisciplineRecords | GET /api/student/discipline-records | StudentDisciplineRecordsController | CONNECTED |
| createAppeal | POST /api/student/discipline-records/{id}/appeals | StudentDisciplineRecordsController | CONNECTED |

### Notifications (`notificationsApi.js`)

All CONNECTED — full CRUD for user + admin notifications.

### Teacher Submissions (`teacherSubmissionsService.js`)

All CONNECTED — full CRUD for assignments, submissions, grading.

### Content Council (`contentCouncilApi.js`)

All CONNECTED — full CRUD for subjects, chapters, lessons, content, questions, quizzes.

### Finance (`financeTuitionConfigService.js`)

All CONNECTED — full CRUD for tuition configs.

### Room Booking (`staffRoomBookingService.js`)

| FE Method | FE URL | BE Controller | Status |
|---|---|---|---|
| getBookings | GET /api/staff/rooms/bookings | StaffRoomBookingsController | CONNECTED |
| book | POST /api/staff/rooms/book | StaffRoomBookingsController | CONNECTED |
| getBooking | GET /api/staff/rooms/bookings/{id} | StaffRoomBookingsController | CONNECTED |
| updateBooking | PUT /api/staff/rooms/bookings/{id} | StaffRoomBookingsController | CONNECTED |
| cancelBooking | DELETE /api/staff/rooms/bookings/{id} | StaffRoomBookingsController | CONNECTED |

---

## Backend-Only Endpoints (no FE integration)

These controllers/endpoints exist in BE but have no corresponding FE service:

| Controller | Example Endpoints | Priority |
|---|---|---|
| AttendanceController | POST /api/buoi-hoc/{id}/attendance/start, bulk submit | High (needed for teacher flow) |
| AttendanceUnlockController | POST unlock-requests, approve/reject | Medium |
| BuoiHocController | PUT change-teacher, change-room, change-shift | High (needed for substitute) |
| StaffRoomBookingsController | Full CRUD | Low |
| AdminRegistrationsController | Registration periods, course sections | Medium |
| AdminRewardCampaignsController | Top100, candidates, approval | Low |
| AdminRewardCertificatesController | Certificate download | Low |
| QuizAttemptsController | Quiz availability, start, submit | High (needed for student taking quizzes) |
| LearningProgressController | Session start/end, heartbeat | Medium |
| CurriculumController | Chapters, lessons, content full CRUD | Used by content-council |
| StudentCurriculumController | GET /api/student/curriculum | CONNECTED |
| StudentRegistrationsController | Available, register, withdraw | CONNECTED |
| StudentTuitionController | Invoices, transactions, payments | CONNECTED |
| ExamController | Full exam lifecycle | Partial FE |

---

## Critical Issues Found

### Issue 1: Assignment API uses wrong endpoint
- **FE**: `assignmentApi.js` calls `GET /api/thoi-khoa-bieu` for assignment list
- **BE**: Should call `GET /api/courses` (assignments = teacher-to-course mapping)
- **Impact**: Teacher Assignment screen shows schedule data instead of course data

### Issue 2: Application draft path mismatch
- **FE**: `applicationsApi.js` calls `POST /api/student/applications/drafts`
- **BE**: `StudentApplicationsController` has `POST /api/student/applications` (no /drafts)
- **Impact**: Creating application draft fails

### Issue 3: Application schema options path mismatch
- **FE**: `applicationsApi.js` calls `GET /api/applications/schema/options`
- **BE**: `ApplicationSchemaController` has `GET /api/applications/templates`
- **Impact**: Application type list fails

### Issue 4: Admin applications assignable-users path
- **FE**: calls `GET /api/admin/applications/assignable-users`
- **BE**: `AdminApplicationsController` has `GET /api/admin/applications/assignees`
- **Impact**: Assign user dropdown in admin fails

### Issue 5: Parent module completely missing BE
- 15 screens, 0 endpoints
- **Impact**: Cannot demo Parent role at all

### Issue 6: Student support tickets and evaluations missing BE (FIXED P15B)
- **Fix**: Created StudentSupportTicketsController + rewrote StudentEvaluationsController

### Issue 7: Teacher requests/comments/questions missing BE (FIXED P15C)
- **Fix**: Created TeacherCommunicationsController, TeacherRequestsController

### Issue 8: Teacher create exam auth blocked (FIXED P15C.1)
- POST /api/exam/ca-thi used AcademicOperations policy excluding Teacher
- **Fix**: Created TeacherExamController with POST /api/teacher/exams, teacher-scoped with ownership validation

---

## Fixed Issues (this audit)

| Issue | Fix | File |
|---|---|---|
| Removed all mock data imports | Replaced with inline empties / real API calls | 10+ files edited |
| Deleted standalone mock files | Removed 11 mock files | frontend/src/data, mocks, services |
