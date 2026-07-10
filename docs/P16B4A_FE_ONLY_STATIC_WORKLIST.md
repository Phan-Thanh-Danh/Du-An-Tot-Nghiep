# P16B4A FE_ONLY_STATIC Worklist

> Phase: `P16B.4A`  
> Generated: 2026-07-10  
> Source: `docs/P16_FULL_SCREEN_ACTION_API_MATRIX.md`, `docs/P16B1_HIGH_RISK_WORKLIST.md`  
> Rule: worklist first; no code changes were made before this file existed.

## Summary

| Decision | Count |
| --- | ---: |
| `FALSE_STATIC_SCAN` | 0 |
| `WRAPPER_API_BACKED` | 8 |
| `CONNECT_EXISTING_API` | 9 |
| `UI_ONLY_JUSTIFIED` | 0 |
| `NEEDS_BE_ENDPOINT` | 10 |
| `REMOVE_FROM_100_PERCENT_CLAIM` | 1 |
| **Total** | **28** |

## Worklist

| Role | Route | Component | Current status | Visible business purpose | Is wrapper/shell? | Child components/stores used | Existing API candidate | Decision | Files to inspect | Files to change |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SuperAdmin | `/super-admin/approvals/requests` | `frontend/src/views/SuperAdmin/ApprovalsRequestsView.vue` | `FE_ONLY_STATIC` | Application queue intake/review | Yes | `AdminApplicationsQueue` | `/api/admin/applications`, decision endpoints | `CONNECT_EXISTING_API` | `AdminApplicationsQueue.vue`, `applicationsApi.js` | `AdminApplicationsQueue.vue` |
| SuperAdmin | `/super-admin/approvals/reports` | `frontend/src/views/SuperAdmin/ApplicationReportsView.vue` | `FE_ONLY_STATIC` | Application reports | Yes | `AdminApplicationReports` | `/api/admin/applications/reports/*` | `CONNECT_EXISTING_API` | `AdminApplicationReports.vue`, `applicationsApi.js` | `AdminApplicationReports.vue` |
| SuperAdmin | `/super-admin/rewards-discipline` | `frontend/src/views/SuperAdmin/RewardDisciplineView.vue` | `FE_ONLY_STATIC` | Reward dashboard | Yes | `AwardsView` | `/api/admin/reward-campaigns` | `WRAPPER_API_BACKED` | `AwardsView.vue`, `rewardDisciplineApi.js` | Docs only |
| SuperAdmin | `/super-admin/rewards/campaigns` | `frontend/src/views/SuperAdmin/RewardCampaignsView.vue` | `FE_ONLY_STATIC` | Reward campaign management | Yes | `AwardsView` | `/api/admin/reward-campaigns` | `WRAPPER_API_BACKED` | `AwardsView.vue`, `rewardDisciplineApi.js` | Docs only |
| SuperAdmin | `/super-admin/discipline/records` | `frontend/src/views/SuperAdmin/DisciplineRecordsView.vue` | `FE_ONLY_STATIC` | Discipline records | Yes | `DisciplineView` | `/api/admin/discipline-records` | `WRAPPER_API_BACKED` | `DisciplineView.vue`, `rewardDisciplineApi.js` | Docs only |
| SuperAdmin | `/super-admin/discipline/appeals` | `frontend/src/views/SuperAdmin/DisciplineAppealsView.vue` | `FE_ONLY_STATIC` | Discipline appeals | Yes | `DisciplineView` | `/api/admin/discipline-appeals` | `WRAPPER_API_BACKED` | `DisciplineView.vue`, `rewardDisciplineApi.js` | Docs only |
| SuperAdmin | `/super-admin/reports/learning` | `frontend/src/views/SuperAdmin/LearningReportView.vue` | `FE_ONLY_STATIC` | Learning analytics | No | None | No specific learning analytics endpoint | `NEEDS_BE_ENDPOINT` | `LearningReportView.vue`, `API_CONTRACT.md` | Docs only |
| SuperAdmin | `/super-admin/reports/attendance` | `frontend/src/views/SuperAdmin/AttendanceReportView.vue` | `FE_ONLY_STATIC` | Attendance analytics | No | None | No SuperAdmin attendance analytics endpoint | `NEEDS_BE_ENDPOINT` | `AttendanceReportView.vue`, `API_CONTRACT.md` | Docs only |
| SuperAdmin | `/super-admin/reports/campus-comparison` | `frontend/src/views/SuperAdmin/CampusComparisonView.vue` | `FE_ONLY_STATIC` | Campus comparison analytics | No | None | No campus comparison endpoint | `NEEDS_BE_ENDPOINT` | `CampusComparisonView.vue`, `API_CONTRACT.md` | Docs only |
| SuperAdmin | `/super-admin/reports/export` | `frontend/src/views/SuperAdmin/DataExportView.vue` | `FE_ONLY_STATIC` | Data export | No | None | No data export endpoint | `NEEDS_BE_ENDPOINT` | `DataExportView.vue`, `API_CONTRACT.md` | Docs only |
| SuperAdmin | `/super-admin/notifications/send` | `frontend/src/views/SuperAdmin/SendNotificationView.vue` | `FE_ONLY_STATIC` | Compose/send notification | Yes | `NotificationComposeForm` | `/api/admin/notifications`, preview recipients | `CONNECT_EXISTING_API` | `SendNotificationView.vue`, notification components | `SendNotificationView.vue` |
| BGH | `/bgh/profile` | `frontend/src/views/BGH/ProfileView.vue` | `FE_ONLY_STATIC` | Current user profile/password | No | Auth store | `/api/account/me`, `/api/account/profile`, `/api/account/change-password` | `CONNECT_EXISTING_API` | `ProfileView.vue`, `accountApi.js` | `ProfileView.vue`, service if needed |
| BGH | `/bgh/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | User notification inbox | Yes | `NotificationInbox` | `/api/notifications`, read/read-all | `CONNECT_EXISTING_API` | `NotificationsView.vue`, `NotificationInbox.vue` | `NotificationInbox.vue` |
| Staff | `/staff/dashboard` | `frontend/src/views/GiaoVu/Dashboard.vue` | `FE_ONLY_STATIC` | Staff operating dashboard | No | `useStaffDashboard` | `/api/staff/dashboard` | `WRAPPER_API_BACKED` | `Dashboard.vue`, `useStaffDashboard.js` | Docs only |
| Staff | `/staff/conflicts` | `frontend/src/views/GiaoVu/Schedule/ConflictCheckView.vue` | `FE_ONLY_STATIC` | Schedule conflict check | No | None | `/api/thoi-khoa-bieu/xep-lich-thong-minh/check-xung-dot-batch` | `CONNECT_EXISTING_API` | `ConflictCheckView.vue`, `scheduleApi.js` | `ConflictCheckView.vue` |
| Staff | `/staff/schedule/pending` | `frontend/src/views/GiaoVu/Schedule/PendingSchedulesView.vue` | `FE_ONLY_STATIC` | Schedule drafts pending approval | No | None | `/api/thoi-khoa-bieu/drafts` | `CONNECT_EXISTING_API` | `PendingSchedulesView.vue`, `scheduleApi.js` | `PendingSchedulesView.vue` |
| Staff | `/staff/requests-history` | `frontend/src/views/GiaoVu/Requests/RequestHistoryView.vue` | `FE_ONLY_STATIC` | Processed application history | No | None | `/api/admin/applications` with status filters | `CONNECT_EXISTING_API` | `RequestHistoryView.vue`, `staffApi.js` | `RequestHistoryView.vue` |
| Staff | `/staff/notices/send` | `frontend/src/views/GiaoVu/Notices/SendNoticeView.vue` | `FE_ONLY_STATIC` | Send notice | Yes | `NotificationComposeForm` | `/api/admin/notifications` | `CONNECT_EXISTING_API` | `SendNoticeView.vue`, notification components | `SendNoticeView.vue` |
| Staff | `/staff/notices/history` | `frontend/src/views/GiaoVu/Notices/NoticeHistoryView.vue` | `FE_ONLY_STATIC` | Notice history | No | None | `/api/admin/notifications` | `CONNECT_EXISTING_API` | `NoticeHistoryView.vue`, `notificationsApi.js` | `NoticeHistoryView.vue` |
| Teacher | `/teacher/class-grades` | `frontend/src/views/GiangVien/ClassGradebookView.vue` | `FE_ONLY_STATIC` | Teacher class gradebook summary | No | None | No teacher gradebook endpoint in contract | `NEEDS_BE_ENDPOINT` | `ClassGradebookView.vue`, `teacherApi.js`, `API_CONTRACT.md` | Docs only |
| Teacher | `/teacher/grading-input` | `frontend/src/views/GiangVien/ClassGradesView.vue` | `FE_ONLY_STATIC` | Teacher manual grade entry | No | None | No teacher grade entry endpoint in contract | `NEEDS_BE_ENDPOINT` | `ClassGradesView.vue`, `teacherApi.js`, `API_CONTRACT.md` | Docs only |
| Teacher | `/teacher/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | User notification inbox | Yes | `NotificationInbox` | `/api/notifications`, read/read-all | `WRAPPER_API_BACKED` | `NotificationsView.vue`, `NotificationInbox.vue` | Shared notification fix |
| Student | `/student/exams` | `frontend/src/views/Student/ExamsView.vue` | `FE_ONLY_STATIC` | Student exam list | No | None | `/api/exam/student/list` | `WRAPPER_API_BACKED` | `ExamsView.vue`, `studentExamService.js` | Docs only |
| Student | `/student/exams/2` | `frontend/src/views/Student/ExamResultView.vue` | `FE_ONLY_STATIC` | Exam result detail | No | None | No student exam result/session detail endpoint in contract | `NEEDS_BE_ENDPOINT` | `ExamResultView.vue`, `API_CONTRACT.md` | Docs only |
| Student | `/student/requests` | `frontend/src/views/Student/RequestsView.vue` | `FE_ONLY_STATIC` | Student applications | Yes | `StudentApplicationsHome` | `/api/student/applications` | `CONNECT_EXISTING_API` | `StudentApplicationsHome.vue`, `applicationsApi.js` | `StudentApplicationsHome.vue` |
| Student | `/student/notifications` | `frontend/src/views/Student/NotificationsView.vue` | `FE_ONLY_STATIC` | User notification inbox | Yes | `NotificationInbox` | `/api/notifications`, read/read-all | `WRAPPER_API_BACKED` | `NotificationsView.vue`, `NotificationInbox.vue` | Shared notification fix |
| Parent | `/parent/dashboard` | `frontend/src/views/PhuHuynh/DashboardWrapper.vue` | `FE_ONLY_STATIC` | Parent dashboard wrapper | Yes | `Dashboard.vue` / `MobileDashboard.vue` | `/api/parent/dashboard` | `WRAPPER_API_BACKED` | Parent dashboard views, `parentApi.js` | Docs only |
| ContentCouncil | `/content-council/subjects` | `frontend/src/pages/content-council/subjects/SubjectListPage.vue` | `FE_ONLY_STATIC` | Content subject list | No | `useSubjectFilters`, `subjectStore` | Content council subject APIs via `contentCouncilApi` | `WRAPPER_API_BACKED` | `SubjectListPage.vue`, subject composable/store | Docs only |

## Notes

- `WRAPPER_API_BACKED` rows are false positives from route-level static scan: the route component itself is thin, but child/composable/store already calls real APIs.
- `CONNECT_EXISTING_API` rows are limited to endpoints listed as existing in `docs/API_CONTRACT.md` or controllers already present in `Backend/Controllers`.
- `NEEDS_BE_ENDPOINT` rows are not connected in P16B.4A; they stay outside any full action/API claim.
- P16B.4A does not create backend endpoints and does not claim full action/API coverage.
