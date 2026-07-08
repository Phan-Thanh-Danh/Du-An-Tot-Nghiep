# P15 - Full API Connection Plan

> Generated: 2026-07-08
> Objective: Connect ALL FE_ONLY / BE_MISSING screens to real backend endpoints. No hiding, no mocking.

## Current State

| Role | Total Routes | CONNECTED | FE_ONLY/BE_MISSING | Priority |
|---|---|---|---|---|
| SuperAdmin | 38 | ~15 | 23 | Medium |
| Admin | shared | ~15 | ~10 | Medium |
| Student | 22 | 22 | 0 | Done |
| Teacher | 25 | 25 | 0 | Done |
| Staff/GiaoVu | 24 | 22 | 2 | High |
| BGH | 28 | ~26 | 2 | High |
| Parent | 15 | 15 | 0 | Done |
| Content Council | 8 | 8 | 0 | Done |
| **Total** | **~152** | **~133** | **~19** | |

## Phases

### Phase 1 - Parent Module (COMPLETED P15A)
Built full Parent backend (15 endpoints) and connected all 15 frontend screens.

### Phase 2 - Student FE_ONLY views (COMPLETED P15B)
Connected: GradesView, EvaluationsView, SupportTicketsView, CourseDetailView, ProfileView, Dashboard, all others.
- StudentDashboardController rewritten with real DB queries (was hardcoded mock)
- studentApi.js mock fallbacks removed, real endpoints mapped
- All student components (AppSidebar/AppTopbar) freed of mock imports
- Student: 22/22 connected, 0 FE_ONLY, 0 BE_MISSING, 0 mock/fallback

### Phase 3 - Teacher FE_ONLY views (COMPLETED P15C)
Connected: Dashboard, Courses, Lessons, Classes, Class Details, Class Workspace, Class Progress, Class Attendance, Gradebook, Assignments, Exams, Create Exam, Exam Results, Grading, Proctoring, Attendance Today, Attendance History, Grading Input, Student Questions, Lesson Comments, Requests, Request History, Profile, Notifications, Change Password.
- Created 5 new backend controllers: TeacherClasses, TeacherCommunications, TeacherRequests, TeacherAttendanceHistory, TeacherExamResults.
- Created TeacherExamController with `POST /api/teacher/exams` (teacher-scoped, validates ownership via MaGiaoVien, creates DeKiemTra).
- Teacher: 25/25 connected, 0 FE_ONLY, 0 BE_MISSING, 0 AUTH_ERROR, 0 mock/fallback.

### Phase 4 - BGH mock data removal (MEDIUM)
Remove mock fallback arrays in UsersView, RolesView, ProgramsView, etc.

### Phase 5 - SuperAdmin FE_ONLY views (MEDIUM)
Connect: LoginHistoryView, ProgramsView, EvaluationsResultsView, report views

### Phase 6 - Verify & Release
- `dotnet build` backend
- `npm run build` frontend
- `npm run test:unit` frontend tests

## Teacher Module API Contract (P15C)

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/teacher/dashboard` | Teacher dashboard overview | Teacher |
| GET | `/api/teacher/classes` | Teacher classes list | Teacher |
| GET | `/api/teacher/classes/{id}` | Teacher class detail | Teacher |
| GET | `/api/teacher/classes/{id}/workspace` | Teacher class workspace | Teacher |
| GET | `/api/teacher/classes/{id}/progress` | Teacher class progress | Teacher |
| GET | `/api/teacher/attendance/today` | Attendance today | Teacher |
| GET | `/api/teacher/attendance/history` | Attendance history | Teacher |
| GET | `/api/teacher/student-questions` | Student questions | Teacher |
| POST | `/api/teacher/student-questions/{id}/reply` | Reply student question | Teacher |
| GET | `/api/teacher/lesson-comments` | Lesson comments | Teacher |
| POST | `/api/teacher/lesson-comments/{id}/reply` | Reply lesson comment | Teacher |
| PATCH | `/api/teacher/lesson-comments/{id}/hide` | Hide lesson comment | Teacher |
| GET | `/api/teacher/requests` | Teacher requests | Teacher |
| POST | `/api/teacher/requests` | Create request | Teacher |
| GET | `/api/teacher/requests/history` | Request history | Teacher |
| GET | `/api/teacher/exam-results` | Exam results | Teacher |
| **POST** | **/api/teacher/exams** | **Create exam (teacher-scoped)** | **Teacher** |
| GET | `/api/teacher/assignments` | Teacher assignments | Teacher |
| POST | `/api/teacher/assignments` | Create assignment | Teacher |
| GET | `/api/teacher/submissions` | Teacher submissions | Teacher |
| PUT | `/api/teacher/submissions/{id}/grade` | Grade submission | Teacher |

## Parent Module API Contract (NEW)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/parent/dashboard` | Parent dashboard overview |
| GET | `/api/parent/children` | List linked children |
| GET | `/api/parent/children/{childId}` | Child overview |
| GET | `/api/parent/children/{childId}/grades` | Child grades |
| GET | `/api/parent/children/{childId}/schedule` | Child schedule |
| GET | `/api/parent/children/{childId}/attendance` | Child attendance |
| GET | `/api/parent/children/{childId}/alerts` | Child alerts |
| GET | `/api/parent/children/{childId}/tuition` | Child tuition |
| GET | `/api/parent/children/{childId}/transactions` | Child transactions |
| GET | `/api/parent/children/{childId}/invoices` | Child invoices |
| POST | `/api/parent/payment` | Make payment |
| GET | `/api/parent/notifications` | Parent notifications |
| GET | `/api/parent/notifications/history` | Notification history |
| GET | `/api/parent/profile` | Parent profile |
| GET | `/api/parent/access-rights` | Access rights |
