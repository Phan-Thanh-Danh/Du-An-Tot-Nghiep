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
| Staff/GiaoVu | 24 | 24 | 0 | Done |
| BGH | 25 | 25 | 0 | Done |
| Parent | 15 | 15 | 0 | Done |
| Content Council | 9 | 9 | 0 | Done |
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

### Phase 4 - BGH mock data removal and Route Verification (COMPLETED P15D & P15D.1)
- Verified all 25 BGH views call real backend endpoints (via `bghApi.js` mapping to `BghAcademicController`, `BghEvaluationController`, `BghDashboardController`, and shared controllers).
- Mock/fallback array count is **0**.
- Fixed **Scope/Auth** issue: `BghDashboardController`, `BghAcademicController`, and `BghEvaluationController` now enforce data isolation using `CurrentUserContext.CampusId` for the `Principal` role. `SuperAdmin` and `Admin` retain global access.
- BGH routes verified: 25/25.
- BGH FE_ONLY: 0.
- BGH BE_MISSING: 0.
- BGH mock/fallback: 0.
- BGH auth/scope issue: 0.

#### Verified BGH Routes
| BGH Route | View | API Method | Backend Endpoint | Status | Evidence |
|---|---|---|---|---|---|
| `/bgh/dashboard` | Dashboard.vue | `getDashboard` | `GET /api/bgh/dashboard` | CONNECTED | Verified in P15D.1 |
| `/bgh/organizations` | OrganizationsView.vue | `getOrganizations` | `GET /api/organizations` | CONNECTED | Verified |
| `/bgh/users` | UsersView.vue | `getUsers` | `GET /api/admin/users` | CONNECTED | Verified |
| `/bgh/roles` | RolesView.vue | `getRoles` | `GET /api/admin/rbac/roles` | CONNECTED | Verified |
| `/bgh/academic-programs` | ProgramsView.vue | `getPrograms` | `GET /api/master-data/training-programs` | CONNECTED | Verified |
| `/bgh/curriculum` | CurriculumView.vue | `* shared` | `GET /api/master-data/subjects` (etc) | CONNECTED | Verified |
| `/bgh/academic-terms` | AcademicTermsView.vue | `getAcademicTerms` | `GET /api/master-data/academic-terms` | CONNECTED | Verified |
| `/bgh/academic/overview` | AcademicOverviewView.vue | `getAcademicOverview` | `GET /api/bgh/academic/overview` | CONNECTED | Scoped/Verified |
| `/bgh/academic/gpa` | GPAReportsView.vue | `getGpaReports` | `GET /api/bgh/academic/gpa` | CONNECTED | Scoped/Verified |
| `/bgh/academic/at-risk` | AtRiskStudentsView.vue | `getAtRiskStudents` | `GET /api/bgh/academic/at-risk` | CONNECTED | Scoped/Verified |
| `/bgh/academic/reports` | AcademicReportsView.vue | `getAcademicReports` | `GET /api/bgh/academic/reports` | CONNECTED | Scoped/Verified |
| `/bgh/academic/pass-fail` | PassFailRatesView.vue | `getPassFailRates` | `GET /api/bgh/academic/pass-fail` | CONNECTED | Scoped/Verified |
| `/bgh/schedule/pending` | SchedulePendingView.vue | `getPendingSchedules` | `GET /api/thoi-khoa-bieu` | CONNECTED | Verified |
| `/bgh/schedule/conflicts` | ConflictListView.vue | `* shared` | `GET /api/thoi-khoa-bieu/drafts` | CONNECTED | Verified |
| `/bgh/schedule/published` | PublishedSchedulesView.vue | `* shared` | `GET /api/thoi-khoa-bieu` | CONNECTED | Verified |
| `/bgh/schedule/changes` | ScheduleChangesView.vue | `getScheduleChanges` | `GET /api/bgh/schedule/changes` | CONNECTED | Scoped/Verified |
| `/bgh/evaluations` | EvaluationsView.vue | `getEvaluations` | `GET /api/bgh/evaluations` | CONNECTED | Scoped/Verified |
| `/bgh/evaluations/ranking` | TeacherRankingView.vue | `getEvaluationRanking` | `GET /api/bgh/evaluations/ranking` | CONNECTED | Scoped/Verified |
| `/bgh/evaluations/detail/:id` | TeacherEvalDetailsView.vue | `getEvaluationDetail` | `GET /api/bgh/evaluations/{id}` | CONNECTED | Scoped/Verified |
| `/bgh/evaluations/overview` | EvalOverviewView.vue | `getEvaluationOverview` | `GET /api/bgh/evaluations/overview` | CONNECTED | Scoped/Verified |
| `/bgh/evaluations/ai-analysis` | AIFeedbackAnalysisView.vue | `getEvaluationAiAnalysis` | `GET /api/bgh/evaluations/ai-analysis` | CONNECTED | Scoped/Verified |
| `/bgh/facilities` | FacilitiesView.vue | `* shared` | `GET /api/master-data/rooms` | CONNECTED | Verified |
| `/bgh/audit-logs` | AuditLogsView.vue | `getAuditLogs` | `GET /api/audit-logs` | CONNECTED | Verified |
| `/bgh/profile` | ProfileView.vue | `* shared` | `GET /api/auth/me` | CONNECTED | Verified |
| `/bgh/notifications` | NotificationsView.vue | `* shared` | `GET /api/notifications` | CONNECTED | Verified |

### Phase 4.1 - Staff/GiaoVu mock data removal and Route Verification (COMPLETED P15D.2)
- Built `ApplicationWorkflowService` and `AdminWorkflowController` for `WorkflowConfigView` and wired to frontend.
- Validated `CourseStatusView` using existing `AdminRegistrationsController`.
- Staff/GiaoVu routes verified: 24/24.
- Staff/GiaoVu FE_ONLY: 0.
- Staff/GiaoVu BE_MISSING: 0.
- Global mock removal for GiaoVu deferred to P15F.

### Phase 4.2 - ContentCouncil mock removal & matrix normalization (COMPLETED P15D.3)
- Normalised actual route count to 9.
- Verified `/content-council/subjects/:id/editor` and `/content-council/subjects/:id/preview` routes are correctly hooked to `CurriculumController`.
- Stripped all `ENABLE_MOCK_API` usages and fallback code inside `content-council` stores (`subjectStore`, `quizStore`, `questionStore`) and `subjectApi.js`.
- Rewrote `subjectStore.ts` to map backend `ChuongDto` to frontend `EditorChapter` struct.
- Normalised API status metrics across `P14_ROLE_SCREEN_API_MATRIX.md`.

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

## P15F.1 - Browser Smoke + Skeleton Standardization

Status: `PASS_WITH_WARNINGS` on 2026-07-08.

- Backend smoke target: `http://localhost:5097`; frontend target: `https://localhost:5173`.
- SQL Server target: `DELL\SQLEXPRESS02`, database `LMS`.
- Verified large seed data exists: `hoc_sinh = 10004`, `giao_vien = 109`.
- Chrome/CDP smoke covered role dashboard entry routes for SuperAdmin, Staff/GiaoVu, Teacher, Student, BGH, Parent, and Content Council.
- Final smoke result: 6 role entries passed; Parent login failed with `401 Unauthorized` for the requested P15 account.
- Console runtime errors after fixes: `0`; network `404`: `0`; network `500`: `0`.
- Added shared skeleton components under `frontend/src/components/common/skeleton`.
- Removed standalone mock data/service files found under `frontend/src/data`, `frontend/src/mocks`, and mock facility services.
- Remaining warning: several service modules still contain opt-in fallback/mock guard code such as `ENABLE_MOCK_API`/`withFallback`; these must be reviewed before claiming a strict zero-mock production state.

Detailed report: `docs/P15F_BROWSER_SMOKE_AND_SKELETON_REPORT.md`.

## P15F.2 - Strict Zero-Mock Finalization + Parent Login Fix

Status: `PASS_WITH_WARNINGS` on 2026-07-08.

- Dropped and recreated local `LMS` on `DELL\SQLEXPRESS02` through EF Core/code-first startup.
- Startup now runs base role/demo seed before optional `SeedProfile=LargeDemo`, so deterministic P12/P15 accounts exist after a clean DB reset.
- Verified large seed data after reset: total users `10131`, `hoc_sinh = 10005`, `giao_vien = 110`, `phu_huynh = 2`, parent links `1`.
- Verified `p15test_parent01@lms.local / Test@123` login now returns `200`.
- Verified P12/P15 Staff, Teacher, Student, BGH, Parent, and ContentCouncil seeded accounts are active.
- Removed remaining `ENABLE_MOCK_API` / `withFallback` service branches and deleted standalone mock files.
- Final hard grep over `frontend/src`, `Backend/Controllers`, and `Backend/Services` for `mock|fake|dummy|DEMO_|ENABLE_MOCK_API|withFallback|@/mocks|studentData.mock|mockFacilitiesData|mockDataService|rewardDisciplineMock` returns `0` hits.
- Final browser smoke covered Parent dashboard, Parent children list, Parent tuition, SuperAdmin dashboard regression, and Student dashboard regression: `5/5` pass, console/runtime/network 401/403/404/500 all `0`.
- Remaining warning: full 165-route browser coverage is still not complete, and `npm run lint` still fails on pre-existing unrelated lint/parser issues even though `npm run build` passes.

## P15F.3 - Release Hardening / Final Warnings Cleanup

Status: `PASS_WITH_WARNINGS` on 2026-07-08.

- Protected config before release/demo review: `Backend/appsettings.json` now uses a generic/default LocalDB connection string and empty secret placeholders; `DELL\SQLEXPRESS02` is kept only in `Backend/appsettings.Development.json`. Application evidence storage defaults to Local temp storage outside Production, so startup does not require committed R2 secrets.
- Removed Parent local business presentation data from targeted screens. `frontend/src/components/PhuHuynh/data/parentData.js` was deleted; Parent dashboard, children list/overview, profile, notification history, and system notifications now load through `parentApi`.
- Parent-specific grep for `parentData|legacy|local|demo|mock|fake|dummy|DEMO_` has `0` production data hits; remaining `local` matches are UI/client state only.
- Expanded browser smoke covered all Parent sidebar routes, all SuperAdmin sidebar routes, and one representative route each for Student, Teacher, Staff, BGH, and ContentCouncil: `65/65` pass.
- P15F.3 smoke totals: console errors `0`, runtime exceptions `0`, network `401/403/404/500 = 0`.
- Strict production mock/fallback grep remains `0` hits.
- Build verification: backend `dotnet build` PASS with 15 warnings and 0 errors; frontend `npm run build` PASS.
- Lint triage completed in `docs/P15F_LINT_BACKLOG.md`; `npm run lint` remains non-build-blocking backlog work.
- Full 165-route browser clickthrough is still pending, so release/demo decision remains `PASS_WITH_WARNINGS` rather than full `PASS`.
