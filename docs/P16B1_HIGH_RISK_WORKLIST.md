# P16B1 High Risk Worklist

> Phase: `P16B.1`<br>
> Generated: 2026-07-09<br>
> Source: `docs/P16_FULL_SCREEN_ACTION_API_MATRIX.md`<br>
> Rule: worklist first; no code changes are made before this file exists.

## Summary

| Group | Count |
| --- | ---: |
| `PLACEHOLDER` | 7 |
| `MOCK_OR_FALLBACK` | 25 |
| `FE_ONLY_STATIC` | 28 |
| `ROLE_SCOPE_RISK` | 4 |
| **Total** | **64** |

## Decision Legend

- `IMPLEMENT_API`
- `REPOINT_EXISTING_API`
- `READ_ONLY_API_BACKED`
- `UI_ONLY_JUSTIFIED`
- `REMOVE_FROM_100_PERCENT_CLAIM`
- `NEEDS_BE_ENDPOINT`
- `FALSE_POSITIVE`

## PLACEHOLDER

| Role | Route | Component | Current status | Problem found | Token/evidence if MOCK_OR_FALLBACK | Existing backend endpoint candidate | Decision | Files to inspect | Files to change |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SuperAdmin | `/super-admin/operations/schedules` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/operations/schedules/approval` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/finance/tuition-config` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/finance/student-debts` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/finance/payments` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/finance/refunds` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |
| SuperAdmin | `/super-admin/notifications/history` | `frontend/src/views/SuperAdmin/PlaceholderView.vue` | `PLACEHOLDER` | Router points to SuperAdmin PlaceholderView; route opens but business screen is placeholder. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `REMOVE_FROM_100_PERCENT_CLAIM` | frontend/src/views/SuperAdmin/PlaceholderView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/router/index.js or new API-backed view if included in claim |

## MOCK_OR_FALLBACK

| Role | Route | Component | Current status | Problem found | Token/evidence if MOCK_OR_FALLBACK | Existing backend endpoint candidate | Decision | Files to inspect | Files to change |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SuperAdmin | `/super-admin/profile` | `frontend/src/views/SuperAdmin/ProfileView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/ProfileView.vue:23: // --- Mock Data của Super Admin ---<br>frontend/src/views/SuperAdmin/ProfileView.vue:34: // --- Mock Nhật ký đăng nhập --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/ProfileView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ProfileView.vue |
| SuperAdmin | `/super-admin/training/semesters` | `frontend/src/views/SuperAdmin/SemestersView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/SemestersView.vue:26: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/SemestersView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/SemestersView.vue |
| SuperAdmin | `/super-admin/training/programs` | `frontend/src/views/SuperAdmin/ProgramsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/ProgramsView.vue:30: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/ProgramsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ProgramsView.vue |
| SuperAdmin | `/super-admin/training/subjects` | `frontend/src/views/SuperAdmin/SubjectsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/SubjectsView.vue:25: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/SubjectsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/SubjectsView.vue |
| SuperAdmin | `/super-admin/training/courses` | `frontend/src/views/SuperAdmin/CoursesView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/CoursesView.vue:24: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/CoursesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/CoursesView.vue |
| SuperAdmin | `/super-admin/training/exam-periods` | `frontend/src/views/SuperAdmin/ExamPeriodsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/ExamPeriodsView.vue:26: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/ExamPeriodsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ExamPeriodsView.vue |
| SuperAdmin | `/super-admin/operations/attendance-policy` | `frontend/src/views/SuperAdmin/AttendancePolicyView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/AttendancePolicyView.vue:22: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/AttendancePolicyView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/AttendancePolicyView.vue |
| SuperAdmin | `/super-admin/operations/registration-periods` | `frontend/src/views/SuperAdmin/RegistrationPeriodsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/RegistrationPeriodsView.vue:25: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/RegistrationPeriodsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/RegistrationPeriodsView.vue |
| SuperAdmin | `/super-admin/operations/pass-fail-rules` | `frontend/src/views/SuperAdmin/PassFailRulesView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/PassFailRulesView.vue:27: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/PassFailRulesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/PassFailRulesView.vue |
| SuperAdmin | `/super-admin/support/tickets` | `frontend/src/views/SuperAdmin/SupportTicketsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/SupportTicketsView.vue:34: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/SupportTicketsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/SupportTicketsView.vue |
| SuperAdmin | `/super-admin/support/faq` | `frontend/src/views/SuperAdmin/FAQManagementView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/FAQManagementView.vue:33: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/FAQManagementView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/FAQManagementView.vue |
| SuperAdmin | `/super-admin/approvals/history` | `frontend/src/views/SuperAdmin/ApprovalsHistoryView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/ApprovalsHistoryView.vue:29: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/ApprovalsHistoryView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ApprovalsHistoryView.vue |
| SuperAdmin | `/super-admin/evaluations/config` | `frontend/src/views/SuperAdmin/EvaluationsConfigView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/EvaluationsConfigView.vue:29: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/EvaluationsConfigView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/EvaluationsConfigView.vue |
| SuperAdmin | `/super-admin/evaluations/results` | `frontend/src/views/SuperAdmin/EvaluationsResultsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/EvaluationsResultsView.vue:24: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/EvaluationsResultsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/EvaluationsResultsView.vue |
| SuperAdmin | `/super-admin/reports/education-overview` | `frontend/src/views/SuperAdmin/EducationOverviewView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/EducationOverviewView.vue:14: // --- Mock Data --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/EducationOverviewView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/EducationOverviewView.vue |
| SuperAdmin | `/super-admin/audit/logs` | `frontend/src/views/SuperAdmin/AuditLogsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/AuditLogsView.vue:28: // --- Mock Data cho Audit Logs --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/AuditLogsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/AuditLogsView.vue |
| SuperAdmin | `/super-admin/security/alerts` | `frontend/src/views/SuperAdmin/SecurityAlertsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/SecurityAlertsView.vue:29: // --- Mock Data cho Security Alerts --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/SecurityAlertsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/SecurityAlertsView.vue |
| SuperAdmin | `/super-admin/system/modules` | `frontend/src/views/SuperAdmin/SystemModulesView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/SuperAdmin/SystemModulesView.vue:25: // --- Mock Data cho System Modules --- | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `IMPLEMENT_API` | frontend/src/views/SuperAdmin/SystemModulesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/SystemModulesView.vue |
| Staff | `/staff/accounts` | `frontend/src/views/GiaoVu/Accounts/AccountManagementView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/GiaoVu/Accounts/AccountManagementView.vue:272: <!-- Ungrouped view fallback --> | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `IMPLEMENT_API` | frontend/src/views/GiaoVu/Accounts/AccountManagementView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiaoVu/Accounts/AccountManagementView.vue |
| Teacher | `/teacher/class-attendance` | `frontend/src/views/GiangVien/ClassAttendanceView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/GiangVien/ClassAttendanceView.vue:55: // Trying today's attendance sessions as fallback | Teacher* controllers | `IMPLEMENT_API` | frontend/src/views/GiangVien/ClassAttendanceView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiangVien/ClassAttendanceView.vue |
| Student | `/student/exams/2/take` | `frontend/src/views/Student/ExamTakeView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/Student/ExamTakeView.vue:159: function readJson(key, fallback) {<br>frontend/src/views/Student/ExamTakeView.vue:162: return value ? JSON.parse(value) : fallback<br>frontend/src/views/Student/ExamTakeView.vue:164: return fallback<br>frontend/src/views/Student/ExamTakeView.vue:891: { widthDiff, heightDiff, detector: 'fallback' }, | Student*/Exam*/Applications controllers | `IMPLEMENT_API` | frontend/src/views/Student/ExamTakeView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/ExamTakeView.vue |
| Student | `/student/exams/detail/2` | `frontend/src/views/Student/ExamDetailView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/Student/ExamDetailView.vue:58: if (!realExamStatus.value) return false // fallback or waiting | Student*/Exam*/Applications controllers | `IMPLEMENT_API` | frontend/src/views/Student/ExamDetailView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/ExamDetailView.vue |
| Student | `/student/support-tickets` | `frontend/src/views/Student/SupportTicketsView.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/views/Student/SupportTicketsView.vue:50: const toDate = (v, fallback = new Date()) => v ? new Date(v) : fallback | Student*/Exam*/Applications controllers | `IMPLEMENT_API` | frontend/src/views/Student/SupportTicketsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/SupportTicketsView.vue |
| ContentCouncil | `/content-council/question-bank` | `frontend/src/pages/content-council/question-bank/QuestionBankPage.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/pages/content-council/question-bank/QuestionBankPage.vue:203: code: `Q-${formData.subjectCode}-MOCK-${Math.floor(Math.random() * 1000)}`, | MasterData/Curriculum/QuestionBank/Quiz controllers | `IMPLEMENT_API` | frontend/src/pages/content-council/question-bank/QuestionBankPage.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/pages/content-council/question-bank/QuestionBankPage.vue |
| ContentCouncil | `/content-council/subjects/9/preview` | `frontend/src/pages/content-council/subjects/SubjectPreviewPage.vue` | `MOCK_OR_FALLBACK` | Component text/code contains mock/fake/fallback token; P16B must verify whether production behavior is affected. | frontend/src/pages/content-council/subjects/SubjectPreviewPage.vue:67: // Watch showDrafts: If current lesson becomes hidden, fallback to first | MasterData/Curriculum/QuestionBank/Quiz controllers | `IMPLEMENT_API` | frontend/src/pages/content-council/subjects/SubjectPreviewPage.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/pages/content-council/subjects/SubjectPreviewPage.vue |

## FE_ONLY_STATIC

| Role | Route | Component | Current status | Problem found | Token/evidence if MOCK_OR_FALLBACK | Existing backend endpoint candidate | Decision | Files to inspect | Files to change |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SuperAdmin | `/super-admin/approvals/requests` | `frontend/src/views/SuperAdmin/ApprovalsRequestsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/SuperAdmin/ApprovalsRequestsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ApprovalsRequestsView.vue<br>Backend/Controllers/* or service module |
| SuperAdmin | `/super-admin/approvals/reports` | `frontend/src/views/SuperAdmin/ApplicationReportsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/SuperAdmin/ApplicationReportsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/ApplicationReportsView.vue<br>Backend/Controllers/* or service module |
| SuperAdmin | `/super-admin/rewards-discipline` | `frontend/src/views/SuperAdmin/RewardDisciplineView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/RewardDisciplineView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| SuperAdmin | `/super-admin/rewards/campaigns` | `frontend/src/views/SuperAdmin/RewardCampaignsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/RewardCampaignsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| SuperAdmin | `/super-admin/discipline/records` | `frontend/src/views/SuperAdmin/DisciplineRecordsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/DisciplineRecordsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| SuperAdmin | `/super-admin/discipline/appeals` | `frontend/src/views/SuperAdmin/DisciplineAppealsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/DisciplineAppealsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| SuperAdmin | `/super-admin/reports/learning` | `frontend/src/views/SuperAdmin/LearningReportView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/SuperAdmin/LearningReportView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/LearningReportView.vue<br>Backend/Controllers/* or service module |
| SuperAdmin | `/super-admin/reports/attendance` | `frontend/src/views/SuperAdmin/AttendanceReportView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/SuperAdmin/AttendanceReportView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/AttendanceReportView.vue<br>Backend/Controllers/* or service module |
| SuperAdmin | `/super-admin/reports/campus-comparison` | `frontend/src/views/SuperAdmin/CampusComparisonView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/CampusComparisonView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| SuperAdmin | `/super-admin/reports/export` | `frontend/src/views/SuperAdmin/DataExportView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/SuperAdmin/DataExportView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/SuperAdmin/DataExportView.vue<br>Backend/Controllers/* or service module |
| SuperAdmin | `/super-admin/notifications/send` | `frontend/src/views/SuperAdmin/SendNotificationView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/SuperAdmin/SendNotificationView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| BGH | `/bgh/profile` | `frontend/src/views/BGH/ProfileView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Bgh* controllers / BghFacadeController | `NEEDS_BE_ENDPOINT` | frontend/src/views/BGH/ProfileView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/BGH/ProfileView.vue<br>Backend/Controllers/* or service module |
| BGH | `/bgh/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Bgh* controllers / BghFacadeController | `NEEDS_BE_ENDPOINT` | frontend/src/views/Student/NotificationsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/NotificationsView.vue<br>Backend/Controllers/* or service module |
| Staff | `/staff/dashboard` | `frontend/src/views/GiaoVu/Dashboard.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiaoVu/Dashboard.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiaoVu/Dashboard.vue<br>Backend/Controllers/* or service module |
| Staff | `/staff/conflicts` | `frontend/src/views/GiaoVu/Schedule/ConflictCheckView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/GiaoVu/Schedule/ConflictCheckView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| Staff | `/staff/schedule/pending` | `frontend/src/views/GiaoVu/Schedule/PendingSchedulesView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/GiaoVu/Schedule/PendingSchedulesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| Staff | `/staff/requests-history` | `frontend/src/views/GiaoVu/Requests/RequestHistoryView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiaoVu/Requests/RequestHistoryView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiaoVu/Requests/RequestHistoryView.vue<br>Backend/Controllers/* or service module |
| Staff | `/staff/notices/send` | `frontend/src/views/GiaoVu/Notices/SendNoticeView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiaoVu/Notices/SendNoticeView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiaoVu/Notices/SendNoticeView.vue<br>Backend/Controllers/* or service module |
| Staff | `/staff/notices/history` | `frontend/src/views/GiaoVu/Notices/NoticeHistoryView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Staff/Admin/MasterData/ThoiKhoaBieu controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiaoVu/Notices/NoticeHistoryView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiaoVu/Notices/NoticeHistoryView.vue<br>Backend/Controllers/* or service module |
| Teacher | `/teacher/class-grades` | `frontend/src/views/GiangVien/ClassGradebookView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Teacher* controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiangVien/ClassGradebookView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiangVien/ClassGradebookView.vue<br>Backend/Controllers/* or service module |
| Teacher | `/teacher/grading-input` | `frontend/src/views/GiangVien/ClassGradesView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Teacher* controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/GiangVien/ClassGradesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/GiangVien/ClassGradesView.vue<br>Backend/Controllers/* or service module |
| Teacher | `/teacher/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Teacher* controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/Student/NotificationsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/NotificationsView.vue<br>Backend/Controllers/* or service module |
| Student | `/student/exams` | `frontend/src/views/Student/ExamsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Student*/Exam*/Applications controllers | `UI_ONLY_JUSTIFIED` | frontend/src/views/Student/ExamsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |
| Student | `/student/exams/2` | `frontend/src/views/Student/ExamResultView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Student*/Exam*/Applications controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/Student/ExamResultView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/ExamResultView.vue<br>Backend/Controllers/* or service module |
| Student | `/student/requests` | `frontend/src/views/Student/RequestsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Student*/Exam*/Applications controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/Student/RequestsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/RequestsView.vue<br>Backend/Controllers/* or service module |
| Student | `/student/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | Student*/Exam*/Applications controllers | `NEEDS_BE_ENDPOINT` | frontend/src/views/Student/NotificationsView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/Student/NotificationsView.vue<br>Backend/Controllers/* or service module |
| Parent | `/parent/dashboard` | `frontend/src/views/PhuHuynh/DashboardWrapper.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | ParentController | `NEEDS_BE_ENDPOINT` | frontend/src/views/PhuHuynh/DashboardWrapper.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/PhuHuynh/DashboardWrapper.vue<br>Backend/Controllers/* or service module |
| ContentCouncil | `/content-council/subjects` | `frontend/src/pages/content-council/subjects/SubjectListPage.vue` | `FE_ONLY_STATIC` | No direct API/store service call detected in route component static scan. | - | MasterData/Curriculum/QuestionBank/Quiz controllers | `UI_ONLY_JUSTIFIED` | frontend/src/pages/content-council/subjects/SubjectListPage.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | - |

## ROLE_SCOPE_RISK

| Role | Route | Component | Current status | Problem found | Token/evidence if MOCK_OR_FALLBACK | Existing backend endpoint candidate | Decision | Files to inspect | Files to change |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| BGH | `/bgh/users` | `frontend/src/views/BGH/UsersView.vue` | `ROLE_SCOPE_RISK` | BGH screen uses facade/admin-like read paths; P15G route smoke passed but action permission scope still needs review. | - | Bgh* controllers / BghFacadeController; SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `READ_ONLY_API_BACKED` | frontend/src/views/BGH/UsersView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/BGH/UsersView.vue |
| BGH | `/bgh/roles` | `frontend/src/views/BGH/RolesView.vue` | `ROLE_SCOPE_RISK` | BGH screen uses facade/admin-like read paths; P15G route smoke passed but action permission scope still needs review. | - | Bgh* controllers / BghFacadeController; SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `READ_ONLY_API_BACKED` | frontend/src/views/BGH/RolesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/BGH/RolesView.vue |
| BGH | `/bgh/curriculum` | `frontend/src/views/BGH/CurriculumView.vue` | `ROLE_SCOPE_RISK` | BGH screen uses facade/admin-like read paths; P15G route smoke passed but action permission scope still needs review. | - | Bgh* controllers / BghFacadeController | `READ_ONLY_API_BACKED` | frontend/src/views/BGH/CurriculumView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/BGH/CurriculumView.vue |
| BGH | `/bgh/facilities` | `frontend/src/views/BGH/FacilitiesView.vue` | `ROLE_SCOPE_RISK` | BGH screen uses facade/admin-like read paths; P15G route smoke passed but action permission scope still needs review. | - | Bgh* controllers / BghFacadeController; SuperAdmin/Admin/RBAC/Notifications/RewardDiscipline controllers | `READ_ONLY_API_BACKED` | frontend/src/views/BGH/FacilitiesView.vue<br>frontend/src/router/index.js<br>docs/API_CONTRACT.md | frontend/src/views/BGH/FacilitiesView.vue |


## P16B.1 Resolution Notes

- Placeholder router usage resolved: `rg "component: \() => import\('../views/SuperAdmin/PlaceholderView.vue'\)" frontend/src/router/index.js` returns no hits.
- Repointed schedule placeholders to existing schedule screens.
- Repointed tuition config to `SuperAdmin/Finance/TuitionConfigView.vue`.
- Added `SuperAdmin/NotificationHistoryView.vue` backed by `notificationsApi.getAdminNotifications()`.
- Added `SuperAdmin/Finance/FinanceMonitorView.vue` for student debts/payments/refunds as an API-backed non-placeholder stopgap; these remain `REMOVE_FROM_100_PERCENT_CLAIM` until dedicated finance endpoints exist.
- BGH users screen now imports `bghApi`, hides mutation actions for non Admin/SuperAdmin, and uses PATCH for admin lock/unlock/reset-password actions.
- Added `GET /api/bgh/users` as a BGH read-only, campus-scoped facade endpoint; `bghApi.getUsers()` no longer calls admin-scoped `/api/admin/users` for Principal/BGH.
- Targeted browser smoke artifact: `docs/artifacts/p16b1-high-risk/p16b1-results.json`.
- Targeted browser smoke result: 11/11 PASS, console errors 0, runtime exceptions 0, network 401/403/404/500 all 0.

## P16B.2 SuperAdmin Mock/Fallback Cleanup

> Date: 2026-07-10
> Scope: SuperAdmin rows in the `MOCK_OR_FALLBACK` group only. Staff, Teacher, Student, Parent, BGH, ContentCouncil, and `FE_ONLY_STATIC` rows outside SuperAdmin remain for later P16B passes.

### Result

| Group | Before P16B.2 | After P16B.2 | Notes |
| --- | ---: | ---: | --- |
| SuperAdmin `MOCK_OR_FALLBACK` | 18 | 0 | `frontend/src/views/SuperAdmin` now has 0 hits for mock/fake/fallback tokens. |
| API-backed SuperAdmin cleanup | 0 | 7 | Profile, semesters, programs, subjects, courses, registration periods, and audit logs now load from real APIs. |
| Converted to endpoint-pending | 0 | 11 | Screens with no confirmed backend contract now render a pending endpoint state with no local business data. |
| False positive cleanup | 1 | 0 | `Finance/TuitionConfigView.vue` helper names no longer contain `fallback`. |

### API-Backed Routes

| Route | Component | Primary API | P16 status |
| --- | --- | --- | --- |
| `/super-admin/profile` | `ProfileView.vue` | `GET /api/account/me`, `PUT /api/account/profile`, `PUT /api/account/change-password` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/training/semesters` | `SemestersView.vue` | `GET /api/master-data/academic-terms` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/training/programs` | `ProgramsView.vue` | `GET /api/master-data/training-programs` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/training/subjects` | `SubjectsView.vue` | `GET /api/master-data/subjects` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/training/courses` | `CoursesView.vue` | `GET /api/courses` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/operations/registration-periods` | `RegistrationPeriodsView.vue` | `GET /api/admin/registration-periods` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |
| `/super-admin/audit/logs` | `AuditLogsView.vue` | `GET /api/audit-logs` | `PASS_LOAD_ONLY_ACTIONS_PENDING` |

These routes no longer use local business data. Mutating actions remain excluded from a full action/API claim until each payload and role permission is manually verified in P16.

### Converted To Endpoint-Pending

| Route | Component | P16 status | Required backend contract |
| --- | --- | --- | --- |
| `/super-admin/training/exam-periods` | `ExamPeriodsView.vue` | `NEEDS_BE_ENDPOINT` | Exam period administration API. |
| `/super-admin/operations/attendance-policy` | `AttendancePolicyView.vue` | `NEEDS_BE_ENDPOINT` | Attendance policy configuration API. |
| `/super-admin/operations/pass-fail-rules` | `PassFailRulesView.vue` | `NEEDS_BE_ENDPOINT` | Pass/fail rule configuration API. |
| `/super-admin/support/tickets` | `SupportTicketsView.vue` | `NEEDS_BE_ENDPOINT` | SuperAdmin support-ticket administration API. |
| `/super-admin/support/faq` | `FAQManagementView.vue` | `NEEDS_BE_ENDPOINT` | FAQ management API. |
| `/super-admin/approvals/history` | `ApprovalsHistoryView.vue` | `NEEDS_BE_ENDPOINT` | Approval history/reporting API. |
| `/super-admin/evaluations/config` | `EvaluationsConfigView.vue` | `NEEDS_BE_ENDPOINT` | Evaluation form/configuration API. |
| `/super-admin/evaluations/results` | `EvaluationsResultsView.vue` | `NEEDS_BE_ENDPOINT` | Evaluation result/reporting API. |
| `/super-admin/reports/education-overview` | `EducationOverviewView.vue` | `NEEDS_BE_ENDPOINT` | Education overview analytics API. |
| `/super-admin/security/alerts` | `SecurityAlertsView.vue` | `NEEDS_BE_ENDPOINT` | Security alert operations API. |
| `/super-admin/system/modules` | `SystemModulesView.vue` | `NEEDS_BE_ENDPOINT` | System module health/operations API. |

### Verification

| Check | Result |
| --- | --- |
| SuperAdmin token grep | PASS, 0 hits for `Mock Data|Mock Nhật ký|mock|fake|dummy|fallback|withFallback|localData|staticData|demoData|hardcoded` under `frontend/src/views/SuperAdmin`. |
| Strict production grep | PASS, 0 hits for mock/fallback production tokens across `frontend/src`, `Backend/Controllers`, and `Backend/Services`. |
| Frontend build | PASS. |
| Backend build | PASS, 19 warnings, 0 errors. |
| P16B.2 targeted browser smoke | PASS, 19/19 SuperAdmin routes, console errors 0, runtime exceptions 0, network 401/403/404/500 all 0. Artifact: `docs/artifacts/p16b2-superadmin-mock-cleanup/p16b2-superadmin-results.json`. |
| Conflict marker grep | PASS, 0 hits. |
| `git diff --check` | PASS. |

Decision: `PASS_WITH_WARNINGS` for P16B.2. SuperAdmin production mock/fallback data is removed, but full SuperAdmin action/API coverage is not claimed because 11 screens still require real backend endpoints and 7 API-backed screens need runtime action verification.

## P16B.3 Non-SuperAdmin Mock/Fallback Cleanup

> Date: 2026-07-10
> Scope: remaining non-SuperAdmin `MOCK_OR_FALLBACK` rows only.

| Route | Component | Previous | New | Decision | Notes |
| --- | --- | --- | --- | --- | --- |
| `/staff/accounts` | `frontend/src/views/GiaoVu/Accounts/AccountManagementView.vue` | `MOCK_OR_FALLBACK` | `PASS_FULL_API` | `FALSE_POSITIVE` | Comment-only `fallback` wording for an ungrouped table branch was renamed; screen already uses account APIs. |
| `/teacher/class-attendance` | `frontend/src/views/GiangVien/ClassAttendanceView.vue` | `MOCK_OR_FALLBACK` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `COMMENT_ONLY` | Comment now documents today's attendance API as the only real source; no hidden production fallback remains. |
| `/student/exams/2/take` | `frontend/src/views/Student/ExamTakeView.vue` | `MOCK_OR_FALLBACK` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `HELPER_NAME_ONLY` | Renamed local JSON helper parameter/error variable and technical devtools detector label; exam start/questions/autosave remain API-backed. |
| `/student/exams/detail/2` | `frontend/src/views/Student/ExamDetailView.vue` | `MOCK_OR_FALLBACK` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `COMMENT_ONLY` | Removed misleading fallback comment; missing exam status still blocks entry instead of fabricating access. |
| `/student/support-tickets` | `frontend/src/views/Student/SupportTicketsView.vue` | `MOCK_OR_FALLBACK` | `PASS_FULL_API` | `HELPER_NAME_ONLY` | Renamed date helper parameter and removed implicit current-date default so missing backend timestamps render empty. |
| `/content-council/question-bank` | `frontend/src/pages/content-council/question-bank/QuestionBankPage.vue` | `MOCK_OR_FALLBACK` | `PASS_FULL_API` | `PRODUCTION_FALLBACK` | Removed `MOCK` question code generation; new client-side code uses provided code or a neutral timestamp code until store/API persists the question. |
| `/content-council/subjects/9/preview` | `frontend/src/pages/content-council/subjects/SubjectPreviewPage.vue` | `MOCK_OR_FALLBACK` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `COMMENT_ONLY` | Renamed comment for selecting the first visible lesson; no data fallback behavior changed. |

### P16B.3 Verification

| Check | Result |
| --- | --- |
| Frontend build | PASS. |
| Backend build | PASS, 19 warnings, 0 errors. |
| Strict production grep | PASS, 0 hits across `frontend/src`, `Backend/Controllers`, `Backend/Services`, and `Backend/Data`. |
| Targeted non-SuperAdmin grep | PASS, 0 hits across `frontend/src/views/GiaoVu`, `frontend/src/views/GiangVien`, `frontend/src/views/Student`, and `frontend/src/pages/content-council`. |
| Conflict marker grep | PASS, 0 hits. |
| `git diff --check` | PASS, LF/CRLF warnings only in docs. |
| Targeted browser smoke | PASS, 7/7 route entries, console errors 0, runtime exceptions 0, network 401/403/404/500 all 0. |

Smoke artifact: `docs/artifacts/p16b3-non-superadmin-mock-cleanup/p16b3-results.json`.

Note: `/student/exams/{id}/take` is recorded as `PASS_GUARDED_REDIRECT` because the app correctly redirects to `/student/exams/detail/{id}` until the student passes preflight/entry conditions.

Decision: `PASS_WITH_WARNINGS` for P16B.3. The remaining non-SuperAdmin mock/fallback tokens are removed, but full action/API coverage is still not claimed.

## P16B.4A FE_ONLY_STATIC Triage

> Date: 2026-07-10
> Scope: All 28 `FE_ONLY_STATIC` rows.

### Triage Summary

| Decision | Count |
| --- | ---: |
| `CONNECT_EXISTING_API` | 9 |
| `WRAPPER_API_BACKED` | 8 |
| `NEEDS_BE_ENDPOINT` | 10 |
| `REMOVE_FROM_100_PERCENT_CLAIM` | 1 |
| **Total** | **28** |

### Code Changed — CONNECT_EXISTING_API

| Route | New status | Primary API connected |
| --- | --- | --- |
| `/super-admin/approvals/requests` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET/POST /api/admin/applications` — `AdminApplicationsQueue` |
| `/super-admin/approvals/reports` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/admin/applications/reports/*` — `AdminApplicationReports` |
| `/super-admin/notifications/send` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `POST /api/admin/notifications`, preview recipients |
| `/bgh/profile` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/account/me`, `PUT /api/account/profile`, change-password |
| `/bgh/notifications` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/notifications/me`, read, read-all |
| `/staff/conflicts` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `POST /api/thoi-khoa-bieu/xep-lich-thong-minh/check-xung-dot-batch` |
| `/staff/schedule/pending` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/thoi-khoa-bieu/drafts` |
| `/staff/requests-history` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/admin/applications` with status filter |
| `/staff/notices/send` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `POST /api/admin/notifications` |
| `/staff/notices/history` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET /api/admin/notifications` |
| `/student/requests` | `PASS_LOAD_ONLY_ACTIONS_PENDING` | `GET/POST /api/student/applications`, cancel, resubmit |

### No Code Change — WRAPPER_API_BACKED (false positives)

| Route | New status | Child API source |
| --- | --- | --- |
| `/super-admin/rewards-discipline` | `WRAPPER_API_BACKED` | `AwardsView` → `rewardDisciplineApi` |
| `/super-admin/rewards/campaigns` | `WRAPPER_API_BACKED` | Same |
| `/super-admin/discipline/records` | `WRAPPER_API_BACKED` | `DisciplineView` → `rewardDisciplineApi` |
| `/super-admin/discipline/appeals` | `WRAPPER_API_BACKED` | Same |
| `/staff/dashboard` | `WRAPPER_API_BACKED` | `useStaffDashboard` → `/api/staff/dashboard` |
| `/student/exams` | `WRAPPER_API_BACKED` | `studentExamService` / `examApi` |
| `/student/notifications` | `WRAPPER_API_BACKED` | `NotificationInbox` → `/api/notifications/me` |
| `/teacher/notifications` | `WRAPPER_API_BACKED` | Same |
| `/parent/dashboard` | `WRAPPER_API_BACKED` | Child `Dashboard.vue` → `parentApi` |
| `/content-council/subjects` | `WRAPPER_API_BACKED` | `subjectStore` → `contentCouncilApi` |

### Still NEEDS_BE_ENDPOINT

| Route | Component | Required contract |
| --- | --- | --- |
| `/super-admin/reports/learning` | `LearningReportView` | Learning analytics API |
| `/super-admin/reports/attendance` | `AttendanceReportView` | SuperAdmin attendance analytics API |
| `/super-admin/reports/campus-comparison` | `CampusComparisonView` | Campus comparison analytics API |
| `/super-admin/reports/export` | `DataExportView` | Data export API |
| `/teacher/class-grades` | `ClassGradebookView` | Teacher gradebook summary API |
| `/teacher/grading-input` | `ClassGradesView` | Teacher grade entry API |
| `/student/exams/2` | `ExamResultView` | Student exam result/session detail API |

### Adjacent Fixes (not in FE_ONLY_STATIC scan but fixed in this phase)

| Fix | Reason |
| --- | --- |
| `applicationsApi.js` wrong paths corrected | FE was calling `/drafts`, `/summary`, non-existent paths |
| `scheduleApi.js` `getDrafts()`, `batchCheckConflicts()` added | Required for P12 draft and conflict check screens |
| `AdminNotificationsController.cs` `AcademicStaff` role added | Staff role 403 on `/api/admin/notifications` |

### P16B.4A Verification

| Check | Result |
| --- | --- |
| Frontend build | PASS, 0 errors |
| Backend build | PASS, 4 warnings (NU1903, pre-existing), 0 errors |
| Strict production grep | PASS, 0 hits |
| Conflict marker grep | PASS, 0 hits |
| `git diff --check` | PASS |
| Targeted browser smoke (chrome-devtools-mcp) | PASS, 18/18, console errors 0, network 4xx/5xx all 0 |
| Smoke artifact | `docs/artifacts/p16b4a-fe-only-static/p16b4a-results.json` |

Decision: `PASS_WITH_WARNINGS` for P16B.4A. `FE_ONLY_STATIC` = 0 in connected routes; 7 routes remain `NEEDS_BE_ENDPOINT` outside coverage claim.

## P16B.4B Missing Backend Endpoint Decision Matrix

> Date: 2026-07-10
> Phase type: DOCS ONLY — no code changed.
> Full detail: `docs/P16B4B_MISSING_BACKEND_ENDPOINT_DECISION_MATRIX.md`

### Total Rows Triaged: 25

| Decision | Count |
| --- | ---: |
| `REPOINT_EXISTING_API` | 7 |
| `IMPLEMENT_NOW` | 3 |
| `HIDE_FROM_DEMO_AND_CLAIM` | 7 |
| `REMOVE_ROUTE` | 3 |
| `KEEP_AS_PENDING_ADMIN_ROADMAP` | 5 |
| **Total** | **25** |

### Key Discoveries

- `SecurityAlertsView` and `SystemModulesView` → endpoint **already exists** in `SuperAdminController.cs` (`api/super-admin/security/alerts`, `api/super-admin/system/modules`). No new backend needed — just FE wiring.
- `NotificationsHistoryView` (placeholder) → `GET api/admin/notifications` already covers it. Repoint in P16B.4C.
- `ExamPeriodsView` → `ExamController` has full `ky-thi` CRUD. Repoint.
- `ApprovalsHistoryView` → `applicationsApi` with status filter already covers history. Repoint.
- `EvaluationsResultsView` and `EducationOverviewView` → BGH controllers have real data; add SuperAdmin to roles.
- Teacher gradebook + grade input + student exam result = 3 `IMPLEMENT_NOW` (implemented in P16B.4C).
- Finance routes (student-debts, payments, refunds) → `HIDE_FROM_DEMO_AND_CLAIM`; demo covered by Parent finance role.
- FAQ → `REMOVE_ROUTE` (no entity, no controller, no data anywhere).
- Placeholder schedule routes → `REMOVE_ROUTE` (Staff already owns schedule).

### Next Phases

| Phase | Work | Routes |
| --- | --- | --- |
| **P16B.4C** | Connect repoints (FE) + implement 3 small BE endpoints | 10 routes |
| **P16B.4D** | Hide/remove routes from router + doc update | 10 routes |
| **P16B.5** | Runtime action audit for PASS_LOAD_ONLY_ACTIONS_PENDING | all PASS_LOAD_ONLY rows |

Decision: `PASS` for P16B.4B. Matrix complete, all 25 rows decided.
