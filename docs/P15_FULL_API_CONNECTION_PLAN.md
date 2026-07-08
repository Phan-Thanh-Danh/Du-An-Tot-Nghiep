# P15 - Full API Connection Plan

> Generated: 2026-07-08
> Objective: Connect ALL FE_ONLY / BE_MISSING screens to real backend endpoints. No hiding, no mocking.

## Current State

| Role | Total Routes | CONNECTED | FE_ONLY/BE_MISSING | Priority |
|---|---|---|---|---|
| SuperAdmin | 38 | ~15 | 23 | Medium |
| Admin | shared | ~15 | ~10 | Medium |
| Student | 22 | 22 | 0 | Done |
| Teacher | 23 | 17 | 6 | High |
| Staff/GiaoVu | 24 | 22 | 2 | High |
| BGH | 28 | ~26 | 2 | High |
| Parent | 15 | 15 | 0 | Done |
| Content Council | 8 | 8 | 0 | Done |
| **Total** | **~145** | **~123** | **~22** | |

## Phases

### Phase 1 - Parent Module (COMPLETED P15A)
Built full Parent backend (15 endpoints) and connected all 15 frontend screens.

### Phase 2 - Student FE_ONLY views (COMPLETED P15B)
Connected: GradesView, EvaluationsView, SupportTicketsView, CourseDetailView, ProfileView, Dashboard, all others.
- StudentDashboardController rewritten with real DB queries (was hardcoded mock)
- studentApi.js mock fallbacks removed, real endpoints mapped
- All student components (AppSidebar/AppTopbar) freed of mock imports
- Student: 22/22 connected, 0 FE_ONLY, 0 BE_MISSING, 0 mock/fallback

### Phase 3 - Teacher FE_ONLY views (HIGH)
Connect: CoursesView, ProfileView, ClassGradebookView, CreateExamView, ExamResultsView

### Phase 4 - BGH mock data removal (MEDIUM)
Remove mock fallback arrays in UsersView, RolesView, ProgramsView, etc.

### Phase 5 - SuperAdmin FE_ONLY views (MEDIUM)
Connect: LoginHistoryView, ProgramsView, EvaluationsResultsView, report views

### Phase 6 - Verify & Release
- `dotnet build` backend
- `npm run build` frontend
- `npm run test:unit` frontend tests

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
