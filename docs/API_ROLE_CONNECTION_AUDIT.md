<<<<<<< HEAD
# API – Role Connection Audit

> Tài liệu đối chiếu giữa frontend API services, backend controllers và phân quyền.
> Cập nhật lần cuối: 2026-07-02

## Chú thích

| Trạng thái | Ý nghĩa |
|---|---|
| ✅ Connected | Frontend gọi đúng endpoint, backend tồn tại, auth match |
| ⚠️ Mismatch | Frontend gọi sai path hoặc method so với backend |
| 🚧 Frontend-Only | Frontend có service nhưng backend chưa có controller |
| 🔧 Backend-Only | Backend có endpoint nhưng frontend chưa có service |
| ⏸️ Disabled | Bị tắt (mock/disabled) có chủ đích, cần env flag để bật |

---

## 1. Auth

| # | Frontend Path | Backend Controller | Method | Authorization | Status |
|---|---|---|---|---|---|
| 1 | `POST /api/auth/login` | AuthController.Login | POST | AllowAnonymous | ✅ |
| 2 | `POST /api/auth/change-password` | AuthController.ChangePassword | POST | Authorize | ✅ |
| 3 | `GET /api/auth/me` | AuthController.GetMe | GET | Authorize | ✅ |
| 4 | `POST /api/auth/refresh-token` | AuthController.RefreshToken | POST | AllowAnonymous | ✅ |
| 5 | `POST /api/auth/logout` | AuthController.Logout | POST | AllowAnonymous | ✅ |
| 6 | `POST /api/auth/forgot-password` | AuthController.ForgotPassword | POST | AllowAnonymous | 🔧 |
| 7 | `POST /api/auth/verify-otp` | AuthController.VerifyOtp | POST | AllowAnonymous | 🔧 |
| 8 | `POST /api/auth/reset-password` | AuthController.ResetPassword | POST | AllowAnonymous | 🔧 |
| 9 | `POST /api/auth/revoke-token` | AuthController.RevokeToken | POST | Admin,SuperAdmin,CampusAdmin | 🔧 |
| 10 | `GET /api/account/me` | AccountController.GetMe | GET | Authorize | 🔧 |
| 11 | `PUT /api/account/profile` | AccountController.UpdateProfile | PUT | Authorize | 🔧 |
| 12 | `PUT /api/account/change-password` | AccountController.ChangePassword | PUT | Authorize | 🔧 |

**Ghi chú:** Auth flow cơ bản (login, refresh, logout, getMe, changePassword) đã kết nối đầy đủ. Các endpoint forgot-password, reset-password, revoke-token, account profile chưa có frontend service.

---

## 2. Super Admin / Admin

| # | Frontend Path | Backend Controller | Method | Authorization | Status |
|---|---|---|---|---|---|
| 1 | `GET /api/admin/users` | AdminUsersController.GetUsers | GET | AdminUserManagement policy | ✅ |
| 2 | `POST /api/admin/users` | AdminUsersController.Create | POST | AdminUserManagement | ⚠️ |
| 3 | `GET /api/admin/users/{id}` | AdminUsersController.GetById | GET | AdminUserManagement | ✅ |
| 4 | `PUT /api/admin/users/{id}` | AdminUsersController.Update | PUT | AdminUserManagement | ⚠️ |
| 5 | `PATCH /api/admin/users/{id}/lock` | AdminUsersController.Lock | PATCH | AdminUserManagement | ✅ |
| 6 | `PATCH /api/admin/users/{id}/unlock` | AdminUsersController.Unlock | PATCH | AdminUserManagement | ✅ |
| 7 | `PATCH /api/admin/users/{id}/reset-password` | AdminUsersController.ResetPassword | PATCH | AdminUserManagement | ✅ |
| 8 | `GET /api/admin/accounts` | AccountManagementExampleController | GET | Admin,SuperAdmin,CampusAdmin | 🔧 |
| 9 | `GET /api/admin/rbac/roles` | RbacController.GetRoles | GET | RbacManagement | ✅ |
| 10 | `POST /api/admin/rbac/roles` | RbacController.CreateRole | POST | RbacManagement | ✅ |
| 11 | `GET /api/admin/rbac/roles/{id}` | RbacController.GetRoleById | GET | RbacManagement | ✅ |
| 12 | `PUT /api/admin/rbac/roles/{id}` | RbacController.UpdateRole | PUT | RbacManagement | ✅ |
| 13 | `DELETE /api/admin/rbac/roles/{id}` | RbacController.DeleteRole | DELETE | RbacManagement | ✅ |
| 14 | `GET /api/admin/rbac/users/{userId}/roles` | RbacController.GetUserRoles | GET | RbacManagement | ✅ |
| 15 | `PUT /api/admin/rbac/users/{userId}/roles` | RbacController.AssignUserRoles | PUT | RbacManagement | ✅ |
| 16 | `GET /api/audit-logs` | AuditLogsController.Get | GET | SuperAdmin,Admin,CampusAdmin | 🔧 |
| 17 | `GET /api/audit-logs/{id}` | AuditLogsController.GetById | GET | SuperAdmin,Admin,CampusAdmin | 🔧 |

**Ghi chú:** `adminUserService` dùng `api/admin/users` — backend route dùng `[Authorize(Policy=AdminUserManagement)]`. Các endpoint audit-logs chưa có frontend service.

---

## 3. Applications (Đơn từ)

| # | Frontend Path | Backend Controller | Method | Status |
|---|---|---|---|---|
| 1 | `GET /api/application-schema/options` | — | GET | 🚧 Frontend-Only |
| 2 | `GET /api/application-schema/templates` | — | GET | 🚧 Frontend-Only |
| 3 | `GET /api/application-schema/templates/{id}` | — | GET | 🚧 Frontend-Only |
| 4 | `GET /api/applications/schema/types` | ApplicationSchemaController.GetTypes | GET | 🔧 Backend-Only |
| 5 | `GET /api/applications/schema/statuses` | ApplicationSchemaController.GetStatuses | GET | 🔧 Backend-Only |
| 6 | `GET /api/applications/templates` | ApplicationSchemaController.GetTemplates | GET | 🔧 Backend-Only |
| 7 | `GET /api/applications/templates/{loaiDon}` | ApplicationSchemaController.GetTemplateByType | GET | 🔧 Backend-Only |
| 8 | `GET /api/student/applications` | StudentApplicationsController.Get | GET | ApplicationStudent | ✅ |
| 9 | `POST /api/student/applications` | StudentApplicationsController.Create | POST | ApplicationStudent | ⚠️ |
| 10 | `GET /api/student/applications/{id}` | StudentApplicationsController.GetById | GET | ApplicationStudent | ✅ |
| 11 | `PUT /api/student/applications/{id}` | StudentApplicationsController.Update | PUT | ApplicationStudent | ⚠️ |
| 12 | `POST /api/student/applications/{id}/submit` | StudentApplicationsController.Submit | POST | ApplicationStudent | ✅ |
| 13 | `POST /api/student/applications/{id}/cancel` | StudentApplicationsController.Cancel | POST | ApplicationStudent | ✅ |
| 14 | `POST /api/student/applications/drafts` | — | POST | — | 🚧 Frontend-Only |
| 15 | `PUT /api/student/applications/drafts/{id}` | — | PUT | — | 🚧 Frontend-Only |
| 16 | `POST /api/student/applications/{id}/resubmit` | StudentApplicationsController.Resubmit | POST | ApplicationStudent | 🔧 |
| 17 | `GET /api/admin/applications/types` | — | GET | — | 🚧 Frontend-Only |
| 18 | `POST /api/admin/applications/types` | — | POST | — | 🚧 Frontend-Only |
| 19 | `PUT /api/admin/applications/types/{id}` | — | PUT | — | 🚧 Frontend-Only |
| 20 | `DELETE /api/admin/applications/types/{id}` | — | DELETE | — | 🚧 Frontend-Only |
| 21 | `GET /api/admin/applications` | AdminApplicationsController.GetQueue | GET | ApplicationQueueRead | ✅ |
| 22 | `GET /api/admin/applications/{id}` | AdminApplicationsController.GetDetail | GET | ApplicationQueueRead | ✅ |
| 23 | `POST /api/admin/applications/{id}/approve` | AdminApplicationsController.Approve | POST | ApplicationSensitiveDecision | ✅ |
| 24 | `POST /api/admin/applications/{id}/reject` | AdminApplicationsController.Reject | POST | ApplicationSensitiveDecision | ✅ |
| 25 | `POST /api/admin/applications/{id}/assign` | AdminApplicationsController.Assign | POST | ApplicationAssignmentManage | ✅ |
| 26 | `POST /api/admin/applications/{id}/return` | — | POST | — | 🚧 Frontend-Only |
| 27 | `POST /api/admin/applications/{id}/revoke` | — | POST | — | 🚧 Frontend-Only |
| 28 | `GET /api/admin/applications/{id}/history` | — | GET | — | 🚧 Frontend-Only |
| 29 | `GET /api/admin/applications/statistics` | — | GET | — | 🚧 Frontend-Only |
| 30 | `GET /api/admin/applications/schema` | — | GET | — | 🚧 Frontend-Only |
| 31 | `GET /api/admin/applications/reports/{type}/{year}` | AdminApplicationReportsController.* | GET | ApplicationQueueRead | ⚠️ |
| 32 | `GET /api/admin/applications/reports/{type}/{year}/export` | — | GET | — | 🚧 |
| 33 | `GET /api/admin/applications/queue` | — | GET | — | 🚧 Frontend-Only |
| 34 | `GET /api/admin/applications/queue/stats` | — | GET | — | 🚧 Frontend-Only |
| 35 | `GET /api/admin/applications/queue/summary-by-type` | — | GET | — | 🚧 Frontend-Only |
| 36 | `POST /api/admin/applications/queue/{id}/claim` | — | POST | — | 🚧 Frontend-Only |
| 37 | `POST /api/admin/applications/queue/claim-batch` | — | POST | — | 🚧 Frontend-Only |
| 38 | `POST /api/admin/applications/queue/{id}/unclaim` | — | POST | — | 🚧 Frontend-Only |
| 39 | `GET /api/admin/applications/queue/settings` | — | GET | — | 🚧 Frontend-Only |
| 40 | `PUT /api/admin/applications/queue/settings` | — | PUT | — | 🚧 Frontend-Only |
| 41 | `POST /api/admin/applications/{id}/receive` | AdminApplicationsController.Receive | POST | ApplicationReceive | 🔧 |
| 42 | `POST /api/admin/applications/{id}/request-supplement` | AdminApplicationsController.RequestSupplement | POST | ApplicationReviewOperate | 🔧 |
| 43 | `POST /api/admin/applications/{id}/process` | AdminApplicationsController.Process | POST | ApplicationProcessingOperate | 🔧 |
| 44 | `POST /api/admin/applications/{id}/record-processing-result` | AdminApplicationsController.RecordProcessingResult | POST | ApplicationProcessingOperate | 🔧 |

**Ghi chú:** Module application có nhiều lệch nhất. Frontend dùng path `/api/application-schema/*` nhưng backend dùng `/api/applications/schema/*`. Frontend có `drafts` nhưng backend không có. Nhiều admin queue endpoints frontend-Only.

---

## 4. Notifications

| # | Frontend Path | Backend Controller | Method | Status |
|---|---|---|---|---|
| 1 | `GET /api/notifications` | NotificationsController.Get | GET | Authorize | ✅ |
| 2 | `GET /api/notifications/{id}` | NotificationsController.GetById | GET | Authorize | ✅ |
| 3 | `GET /api/notifications/unread-count` | NotificationsController.GetUnreadCount | GET | Authorize | ✅ |
| 4 | `PATCH /api/notifications/{id}/read` | NotificationsController.MarkAsRead | PATCH | Authorize | ✅ |
| 5 | `PATCH /api/notifications/read-all` | NotificationsController.MarkAllAsRead | PATCH | Authorize | ✅ |
| 6 | `DELETE /api/notifications/{id}` | NotificationsController.Hide | DELETE | Authorize | ✅ |
| 7 | `GET /api/admin/notifications` | AdminNotificationsController.Get | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 8 | `POST /api/admin/notifications` | AdminNotificationsController.Create | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 9 | `GET /api/admin/notifications/{id}` | AdminNotificationsController.GetById | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 10 | `PATCH /api/admin/notifications/{id}/cancel` | AdminNotificationsController.Cancel | PATCH | SuperAdmin,Admin,CampusAdmin | ✅ |
| 11 | `POST /api/admin/notifications/preview-recipients` | AdminNotificationsController.PreviewRecipients | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 12 | `GET /api/admin/notifications/{id}/recipients` | AdminNotificationsController.GetRecipients | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 13 | `GET /api/admin/notifications/{id}/statistics` | AdminNotificationsController.GetStatistics | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 14 | `GET /api/admin/notification-templates` | AdminNotificationTemplatesController | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 15 | `POST /api/admin/notification-templates` | AdminNotificationTemplatesController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 16 | `GET /api/admin/notification-templates/{id}` | AdminNotificationTemplatesController | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 17 | `PUT /api/admin/notification-templates/{id}` | AdminNotificationTemplatesController | PUT | SuperAdmin,Admin,CampusAdmin | ✅ |
| 18 | `DELETE /api/admin/notification-templates/{id}` | AdminNotificationTemplatesController | DELETE | SuperAdmin,Admin,CampusAdmin | ✅ |
| 19 | `POST /api/admin/notification-templates/{id}/activate` | AdminNotificationTemplatesController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 20 | `POST /api/admin/notification-templates/{id}/deactivate` | AdminNotificationTemplatesController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 21 | `POST /api/admin/notification-templates/{id}/preview` | AdminNotificationTemplatesController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 22 | `GET /api/admin/specialized-notifications/categories` | AdminSpecializedNotificationsController | GET | SuperAdmin,Admin,CampusAdmin | ✅ |
| 23 | `POST /api/admin/specialized-notifications/preview-recipients` | AdminSpecializedNotificationsController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 24 | `POST /api/admin/specialized-notifications/tuition` | AdminSpecializedNotificationsController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 25 | `POST /api/admin/specialized-notifications/academic` | AdminSpecializedNotificationsController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 26 | `POST /api/admin/specialized-notifications/urgent` | AdminSpecializedNotificationsController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |
| 27 | `POST /api/admin/specialized-notifications/maintenance` | AdminSpecializedNotificationsController | POST | SuperAdmin,Admin,CampusAdmin | ✅ |

**Ghi chú:** Module notifications gần như kết nối hoàn chỉnh nhất. Tất cả endpoint đều khớp.

---

## 5. Staff (Giáo vụ)

| # | Frontend Path | Backend Controller | Status |
|---|---|---|---|
| 1 | `GET /api/staff/dashboard` | — | 🚧 Frontend-Only (mock fallback) |
| 2 | `POST /api/staff/requests/process-all` | — | 🚧 Frontend-Only |
| 3 | `GET /api/staff/notifications` | — | 🚧 Frontend-Only |
| 4 | `PATCH /api/staff/notifications/{id}/read` | — | 🚧 Frontend-Only |
| 5 | `PATCH /api/staff/notifications/read-all` | — | 🚧 Frontend-Only |
| 6 | `DELETE /api/staff/notifications/{id}` | — | 🚧 Frontend-Only |

**Ghi chú:** Staff API hoàn toàn chưa có backend controller. Frontend dùng mock data fallback qua `VITE_ENABLE_MOCK_API` flag. Cần xây dựng backend controller tương ứng.

---

## 6. Student

| # | Frontend Path | Backend Controller | Method | Authorization | Status |
|---|---|---|---|---|---|
| 1 | `GET /api/student/dashboard` | StudentDashboardController.GetDashboard | GET | Student | ✅ |
| 2 | `GET /api/student/courses` | StudentCoursesController.GetCourses | GET | Student | ✅ |
| 3 | `GET /api/student/courses/{courseId}` | StudentCoursesController.GetCourseDetail | GET | Student | ✅ |
| 4 | `GET /api/student/courses/{courseId}/lessons/{lessonId}/quiz` | StudentCoursesController.GetLessonQuiz | GET | Student | ✅ |
| 5 | `GET /api/student/courses/{courseId}/lessons/{lessonId}/comments` | StudentCoursesController.GetLessonComments | GET | Student | ✅ |
| 6 | `GET /api/student/curriculum` | StudentCurriculumController.GetCurriculum | GET | Student | ✅ |
| 7 | `GET /api/student/assignments` | StudentAssignmentsController.GetAssignments | GET | Student | ✅ |
| 8 | `GET /api/student/assignments/{assignmentId}` | StudentAssignmentsController.GetAssignmentDetail | GET | Student | ✅ |
| 9 | `POST /api/student/assignments/{assignmentId}/submit` | StudentAssignmentsController.SubmitAssignment | POST | Student | ✅ |
| 10 | `GET /api/student/discipline-records` | StudentDisciplineRecordsController | GET | Student | ✅ |
| 11 | `GET /api/student/discipline-records/{id}` | StudentDisciplineRecordsController | GET | Student | ✅ |
| 12 | `POST /api/student/discipline-records/{id}/appeals` | StudentDisciplineRecordsController | POST | Student | ✅ |
| 13 | `GET /api/student/discipline-records/appeals/{appealId}` | StudentDisciplineRecordsController | GET | Student | ✅ |
| 14 | `GET /api/student/rewards` | StudentRewardsController.GetMyRewards | GET | Student | 🔧 |
| 15 | `GET /api/student/rewards/{rewardId}` | StudentRewardsController.GetMyRewardById | GET | Student | 🔧 |
| 16 | `GET /api/student/rewards/{rewardId}/certificate/download` | StudentRewardsController.DownloadCertificate | GET | Student | 🔧 |
| 17 | `GET /api/student/tuition/invoices` | StudentTuitionController.GetInvoices | GET | Student | ✅ |
| 18 | `GET /api/student/tuition/transactions` | StudentTuitionController.GetTransactions | GET | Student | ✅ |
| 19 | `POST /api/student/tuition/invoices/{invoiceId}/payments` | StudentTuitionController.CreatePayment | POST | Student | ✅ |
| 20 | `GET /api/student/tuition/payments/{transactionId}` | StudentTuitionController.GetPayment | GET | Student | ✅ |
| 21 | `GET /api/exam/student/list` | ExamController.GetStudentExams | GET | Student | ✅ |

**Ghi chú:** Student module kết nối khá tốt. Reward endpoints chưa có frontend service.

---

## 7. Finance

| # | Frontend Path | Backend Controller | Method | Authorization | Status |
|---|---|---|---|---|---|
| 1 | `GET /api/finance/program-tuition-configs` | ProgramTuitionConfigsController.Get | GET | TuitionConfigReaders | ✅ |
| 2 | `GET /api/finance/program-tuition-configs/options` | ProgramTuitionConfigsController.GetOptions | GET | TuitionConfigReaders | ✅ |
| 3 | `GET /api/finance/program-tuition-configs/{id}` | ProgramTuitionConfigsController.GetById | GET | TuitionConfigReaders | ✅ |
| 4 | `POST /api/finance/program-tuition-configs` | ProgramTuitionConfigsController.Create | POST | TuitionConfigManagers | ✅ |
| 5 | `POST /api/finance/program-tuition-configs/bulk` | ProgramTuitionConfigsController.BulkCreate | POST | TuitionConfigManagers | ✅ |
| 6 | `PUT /api/finance/program-tuition-configs/{id}` | ProgramTuitionConfigsController.Update | PUT | TuitionConfigManagers | ✅ |
| 7 | `PATCH /api/finance/program-tuition-configs/{id}/deactivate` | ProgramTuitionConfigsController.Deactivate | PATCH | TuitionConfigManagers | ✅ |
| 8 | `GET /api/finance/schema/options` | FinanceSchemaController.GetOptions | GET | SchemaReaders | 🔧 |
| 9 | `GET /api/finance/schema/statuses` | FinanceSchemaController.GetStatuses | GET | SchemaReaders | 🔧 |

**Ghi chú:** Finance module kết nối đầy đủ. Schema endpoints chưa có frontend service.

---

## 8. Master Data

| # | Frontend Path | Backend Controller | Method | Status |
|---|---|---|---|---|
| 1 | `GET /api/organizations` | OrganizationsController.GetAll | GET | Authorize | ✅ |
| 2 | `POST /api/organizations` | OrganizationsController.Create | POST | SuperAdmin | ✅ |
| 3 | `GET /api/organizations/tree` | OrganizationsController.GetTree | GET | Authorize | ✅ |
| 4 | `GET /api/organizations/{id}` | OrganizationsController.GetById | GET | Authorize | ✅ |
| 5 | `PUT /api/organizations/{id}` | OrganizationsController.Update | PUT | SuperAdmin | ✅ |
| 6 | `DELETE /api/organizations/{id}` | OrganizationsController.Delete | DELETE | SuperAdmin | ✅ |
| 7 | `GET /api/organizations/{id}/subtree` | OrganizationsController.GetSubtree | GET | Authorize | ✅ |
| 8 | `DELETE /api/organizations/{id}/hard-delete` | OrganizationsController.HardDelete | DELETE | SuperAdmin | ✅ |
| 9 | `GET /api/master-data/buildings` | BuildingsController.GetBuildings | GET | SuperAdmin,Admin,... | ✅ |
| 10 | `GET /api/master-data/buildings/{id}` | BuildingsController.GetById | GET | ReaderRoles | ✅ |
| 11 | `POST /api/master-data/buildings` | BuildingsController.Create | POST | SuperAdmin,Admin,... | ✅ |
| 12 | `PUT /api/master-data/buildings/{id}` | BuildingsController.Update | PUT | ManagerRoles | ✅ |
| 13 | `DELETE /api/master-data/buildings/{id}` | BuildingsController.Delete | DELETE | ManagerRoles | ✅ |
| 14 | `GET /api/master-data/floors` | FloorsController.GetFloors | GET | SuperAdmin,Admin,... | ✅ |
| 15 | `GET /api/master-data/buildings/{buildingId}/floors` | FloorsController.GetByBuilding | GET | ReaderRoles | ✅ |
| 16 | `GET /api/master-data/floors/{id}` | FloorsController.GetById | GET | ReaderRoles | ✅ |
| 17 | `POST /api/master-data/floors` | FloorsController.Create | POST | SuperAdmin,Admin,... | ✅ |
| 18 | `PUT /api/master-data/floors/{id}` | FloorsController.Update | PUT | ManagerRoles | ✅ |
| 19 | `DELETE /api/master-data/floors/{id}` | FloorsController.Delete | DELETE | ManagerRoles | ✅ |
| 20 | `GET /api/master-data/rooms` | RoomsController.GetRooms | GET | SuperAdmin,Admin,... | ✅ |
| 21 | `GET /api/master-data/rooms/{id}` | RoomsController.GetById | GET | ReaderRoles | ✅ |
| 22 | `GET /api/master-data/floors/{floorId}/rooms` | RoomsController.GetByFloor | GET | ReaderRoles | ✅ |
| 23 | `POST /api/master-data/rooms` | RoomsController.Create | POST | ManagerRoles | ✅ |
| 24 | `PUT /api/master-data/rooms/{id}` | RoomsController.Update | PUT | ManagerRoles | ✅ |
| 25 | `DELETE /api/master-data/rooms/{id}` | RoomsController.Delete | DELETE | ManagerRoles | ✅ |

**Ghi chú:** Master data (organizations, buildings, floors, rooms) kết nối đầy đủ.

---

## 9. Endpoints Backend-Only (chưa có frontend service)

| # | Path | Controller | Gợi ý module frontend |
|---|---|---|---|
| 1 | `GET /api/auth/forgot-password` | AuthController | auth |
| 2 | `GET /api/auth/verify-otp` | AuthController | auth |
| 3 | `GET /api/auth/reset-password` | AuthController | auth |
| 4 | `GET /api/auth/revoke-token` | AuthController | auth |
| 5 | `GET /api/account/me` | AccountController | account |
| 6 | `PUT /api/account/profile` | AccountController | account |
| 7 | `PUT /api/account/change-password` | AccountController | account |
| 8 | `GET /api/audit-logs` | AuditLogsController | admin |
| 9 | `GET /api/audit-logs/{id}` | AuditLogsController | admin |
| 10 | `GET /api/admin/accounts` | AccountManagementExampleController | admin |
| 11 | `GET /api/admin/certificate-templates` | AdminCertificateTemplatesController | admin |
| 12 | `POST /api/admin/certificate-templates` | AdminCertificateTemplatesController | admin |
| 13 | `GET /api/admin/certificate-templates/{id}` | AdminCertificateTemplatesController | admin |
| 14 | `PUT /api/admin/certificate-templates/{id}` | AdminCertificateTemplatesController | admin |
| 15 | `DELETE /api/admin/certificate-templates/{id}` | AdminCertificateTemplatesController | admin |
| 16 | `POST /api/admin/certificate-templates/{id}/preview` | AdminCertificateTemplatesController | admin |
| 17 | `GET /api/master-data/academic-terms` | AcademicTermsController | master-data |
| 18 | `GET /api/master-data/subjects` | SubjectsController | master-data |
| 19 | `GET /api/master-data/majors` | NganhDaoTaoController | master-data |
| 20 | `GET /api/master-data/specializations` | ChuyenNganhController | master-data |
| 21 | `GET /api/master-data/campus-specializations` | ChuyenNganhTheoCoSoController | master-data |
| 22 | `GET /api/master-data/cohorts` | CohortsController | master-data |
| 23 | `GET /api/master-data/training-programs` | TrainingProgramsController | master-data |
| 24 | `GET /api/master-data/course-syllabuses` | CourseSyllabusesController | master-data |
| 25 | `GET /api/ca-hoc` | CaHocController | schedule |
| 26 | `GET /api/buoi-hoc` | BuoiHocController | schedule |
| 27 | `GET /api/thoi-khoa-bieu` | ThoiKhoaBieuController | schedule |
| 28 | `GET /api/teacher/attendance/today` | AttendanceController | teacher |
| 29 | `GET /api/student/attendance` | AttendanceController | student |
| 30 | `POST /api/admin/attendance-automation/run-once` | AttendanceAutomationController | admin |
| 31 | `GET /api/admin/attendance/unlock-requests` | AttendanceUnlockController | admin |
| 32 | `GET /api/curriculum/subjects/{subjectId}/chapters` | CurriculumController | curriculum |
| 33 | `GET /api/exam/ky-thi` | ExamController | exam |
| 34 | `GET /api/exam/lich-thi-tong` | ExamController | exam |
| 35 | `GET /api/exam/ca-thi` | ExamController | exam |
| 36 | `GET /api/exam/de-kiem-tra` | QuizManagementController | exam (HoiDongQuanLyNoiDung) |
| 37 | `GET /api/question-bank/questions` | QuestionBankController | exam (HoiDongQuanLyNoiDung) |
| 38 | `GET /api/quiz-attempts/{quizId}/availability` | QuizAttemptsController | student (quiz) |
| 39 | `GET /api/learning-progress/contents/{contentId}` | LearningProgressController | student |
| 40 | `GET /api/finance/schema/options` | FinanceSchemaController | finance |
| 41 | `GET /api/finance/schema/statuses` | FinanceSchemaController | finance |
| 42 | `GET /api/reward-discipline/schema/options` | RewardDisciplineController | reward-discipline |
| 43 | `GET /api/admin/discipline-records` | AdminDisciplineRecordsController | admin (discipline) |
| 44 | `GET /api/admin/discipline-appeals` | AdminDisciplineAppealsController | admin (discipline) |
| 45 | `GET /api/admin/rewards` | AdminRewardsController | admin (rewards) |
| 46 | `GET /api/admin/reward-campaigns` | AdminRewardCampaignsController | admin (rewards) |

---

## 10. Policies Map

Backend authorization policies và role composition:

| Policy | Roles |
|---|---|
| `AdminOnly` | Admin, SuperAdmin |
| `AdminUserManagement` | Admin, SuperAdmin, CampusAdmin |
| `RbacManagement` | Admin, SuperAdmin, CampusAdmin |
| `AcademicOperations` | Admin, SuperAdmin, AcademicStaff, CampusAdmin, Chairman, HoiDongQuanLyNoiDung |
| `Reports` | Admin, SuperAdmin, Principal, CampusAdmin |
| `ApplicationStudent` | Student |
| `ApplicationQueueRead` | SuperAdmin, Admin, CampusAdmin, SubCampusAdmin, AcademicStaff, Principal |
| `ApplicationReceive` | SuperAdmin, Admin, CampusAdmin, SubCampusAdmin, AcademicStaff |
| `ApplicationAssignmentManage` | SuperAdmin, Admin, CampusAdmin, SubCampusAdmin |
| `ApplicationReviewOperate` | SuperAdmin, Admin, CampusAdmin, SubCampusAdmin, AcademicStaff |
| `ApplicationSensitiveDecision` | SuperAdmin, Admin, CampusAdmin, Principal |
| `ApplicationProcessingOperate` | SuperAdmin, Admin, CampusAdmin, SubCampusAdmin, AcademicStaff |
| `ApplicationSystemAdmin` | SuperAdmin, Admin |

---

## 11. Mock / Fallback Inventory

| File | Endpoint | Mock Flag | Status |
|---|---|---|---|
| `staffApi.js` | `/api/staff/*` | `VITE_ENABLE_MOCK_API` | ✅ Fixed |
| `tuitionService.js` | `/api/student/tuition/*` | `VITE_ENABLE_MOCK_API` | ✅ Fixed |
| `buildingApi.js` | `/api/master-data/buildings/*` | `withFallback` helper | ⏸️ Cần review |
| `floorApi.js` | `/api/master-data/floors/*` | `withFallback` helper | ⏸️ Cần review |
| `roomApi.js` | `/api/master-data/rooms/*` | `withFallback` helper | ⏸️ Cần review |
| `mockDataService.js` | Pure mock (no API) | — | ⏸️ Phụ thuộc view cũ |
| `mockFacilitiesData.js` | Static mock data | — | ⏸️ Phụ thuộc view cũ |

---

## Tổng kết

| Module | Total Endpoints | ✅ Connected | ⚠️ Mismatch | 🚧 Frontend-Only | 🔧 Backend-Only |
|---|---|---|---|---|---|
| Auth | 12 | 5 | 0 | 0 | 7 |
| Admin Users / RBAC | 15 | 15 | 0 | 0 | 0 |
| Applications | 44 | 7 | 3 | 24 | 10 |
| Notifications | 27 | 27 | 0 | 0 | 0 |
| Staff | 6 | 0 | 0 | 6 | 0 |
| Student | 21 | 14 | 0 | 0 | 7 |
| Finance | 9 | 7 | 0 | 0 | 2 |
| Master Data | 25 | 25 | 0 | 0 | 0 |
| **Tổng** | **159** | **100** | **3** | **30** | **26** |

> **Kết luận:** 100/159 endpoint (63%) đã kết nối đúng. Module Applications cần refactor lớn nhất do path lệch. Staff API cần xây backend controller. Các mock fallback đã được gắn cờ env flag.
=======
# API Role Connection Audit

## Mục tiêu

Tài liệu này theo dõi trạng thái kết nối API theo từng role, route, component và backend endpoint. Mục tiêu là tránh tình trạng mỗi role gọi API một kiểu, frontend dùng mock âm thầm, hoặc frontend gọi sai path so với backend.

## Quy ước trạng thái

| Status | Ý nghĩa |
|---|---|
| CONNECTED | Frontend gọi API thật, backend endpoint có, response shape đang khớp ở mức sử dụng hiện tại. |
| PARTIAL | Có gọi API nhưng còn hardcode, mock fallback, thiếu scope theo user, hoặc chưa đủ nghiệp vụ. |
| MOCK | Frontend dùng mock/local state/static data. |
| MISSING_BACKEND | Frontend cần/gọi API nhưng backend chưa có controller/action tương ứng. |
| MISSING_FRONTEND | Backend có API nhưng frontend chưa gọi. |
| PATH_MISMATCH | Frontend gọi sai path so với backend contract. |
| BLOCKED | Cần quyết định nghiệp vụ/schema trước khi nối. |

## Tổng hợp nhanh

| Module | Status | Ghi chú |
|---|---|---|
| Auth | PARTIAL | Login/refresh/logout backend có. Frontend đã bổ sung lưu refresh token và retry 401 trong `apiClient`. Cần test thực tế 401 refresh. |
| Applications | PARTIAL | Đã sửa path schema frontend từ `/api/application-schema/*` sang `/api/applications/schema/*`. Còn cần audit sâu 24 frontend-only path nếu có trong các view. |
| Staff | MISSING_BACKEND | `staffApi` đang gọi `/api/staff/*`; backend route `api/staff` chưa thấy. Mock fallback đã được khóa sau env `VITE_ENABLE_MOCK_API`. |
| Notifications | CONNECTED | Theo audit trước: module match tốt nhất. Cần giữ nguyên contract khi nối role khác. |
| Finance | PARTIAL | Một số endpoint có backend nhưng chưa expose đủ ở frontend. |
| SuperAdmin Users/RBAC | MISSING_FRONTEND | Backend có `/api/admin/users` và `/api/admin/rbac/*`; `UsersView.vue` vẫn dùng mock/local state. |
| Student | PARTIAL | Có service và controller, nhưng dashboard/assignments còn hardcode hoặc mock; submit assignment từng có hardcode student id. |
| Teacher | MOCK/PARTIAL | Exam/proctoring có API; dashboard/classes/grading còn static hoặc thiếu service riêng. |
| Parent | MOCK/BLOCKED | Portal disabled theo `VITE_ENABLE_PARENT_PORTAL`; cần backend scope phụ huynh-con trước khi bật. |
| BGH | MOCK/PARTIAL | Có route/report UI nhưng dashboard đang static; cần API aggregator report. |
| Content Council | MOCK | Store dùng `initializeSubjectMockData`; cần nối subject/question-bank/quiz API thật. |

## Bảng audit route trọng điểm

| Priority | Role | Route | Component | Service | Endpoint | Backend Controller | Status | Ghi chú |
|---|---|---|---|---|---|---|---|---|
| P1 | Student | `/student/dashboard` | `views/SinhVien/Dashboard.vue` | `studentApi.getDashboard` | `/api/student/dashboard` | `StudentDashboardController` | PARTIAL | Backend đang trả dữ liệu dashboard cứng; cần lấy theo JWT user. |
| P1 | Student | `/student/courses` | `views/SinhVien/*Courses*` | `studentApi.getCourses` | `/api/student/courses` | `StudentCoursesController` | PARTIAL | Cần kiểm response shape với UI. |
| P1 | Student | `/student/assignments` | `views/SinhVien/*Assignments*` | `studentApi.getAssignments` | `/api/student/assignments` | `StudentAssignmentsController` | PARTIAL | `GetAssignments` còn mock; submit cần scope theo user. |
| P1 | Student | `/student/exams` | `views/Student/*Exam*` | `examApi` | `/api/exam/student/list`, `/api/exam/taking/*` | `ExamController` | PARTIAL | Đang ưu tiên hoàn thiện exam/proctoring. |
| P2 | Student | `/student/tuition` | `views/SinhVien/*Tuition*` | `tuitionService` | `/api/student/tuition/*` | `StudentTuitionController` | PARTIAL | Cần kiểm các endpoint backend-only. |
| P1 | Teacher | `/teacher/dashboard` | `views/GiangVien/Dashboard.vue` | none | missing | missing | MOCK | Static data trong component. |
| P1 | Teacher | `/teacher/classes` | `views/GiangVien/ClassListView.vue` | missing | missing | missing | MOCK/PARTIAL | Cần `teacherApi`. |
| P1 | Teacher | `/teacher/grading` | `views/GiangVien/GradingView.vue` | missing | missing | missing | MOCK/PARTIAL | Cần submission/grading API. |
| P1 | Teacher | `/teacher/proctoring` | `views/GiangVien/ProctoringView.vue` | `examApi`, `examProctoringHub` | `/api/exam/ca-thi/*`, `/hubs/exam-monitoring` | `ExamController`, `ExamMonitoringHub` | PARTIAL | Đang hoàn thiện WebRTC/signaling. |
| P1 | Staff | `/staff/dashboard` | `views/GiaoVu/Dashboard.vue` | `staffApi.getDashboard` | `/api/staff/dashboard` | missing | MISSING_BACKEND | Frontend không còn fallback mock trừ khi bật `VITE_ENABLE_MOCK_API`. |
| P1 | Staff | `/staff/requests` | `views/GiaoVu/Requests/*` | `staffApi`/applications | `/api/staff/requests/*` hoặc `/api/admin/applications/*` | partial | MISSING_BACKEND/PARTIAL | Nên đổi sang applications queue API nếu phù hợp. |
| P2 | Staff | `/staff/schedule` | `views/GiaoVu/Schedule/*` | missing/mixed | master-data/schedule APIs | `ThoiKhoaBieuController`, related | PARTIAL | Cần map endpoint thật theo màn. |
| P2 | BGH | `/bgh/dashboard` | `views/BGH/Dashboard.vue` | none | missing | missing | MOCK | Cần `/api/bgh/dashboard` hoặc report aggregator. |
| P2 | BGH | `/bgh/academic/overview` | `views/BGH/Academic/*` | missing | missing/report APIs | partial | MOCK/PARTIAL | Dùng policy Reports. |
| P1 | SuperAdmin | `/super-admin/users` | `views/SuperAdmin/UsersView.vue` | missing | `/api/admin/users` | `AdminUsersController` | MISSING_FRONTEND | Backend có CRUD user, frontend vẫn mock. |
| P1 | SuperAdmin | `/super-admin/roles-permissions` | `views/SuperAdmin/RolesPermissionsView.vue` | missing | `/api/admin/rbac/roles` | `RbacController` | MISSING_FRONTEND | Backend RBAC có sẵn. |
| P2 | SuperAdmin | `/super-admin/organizations` | `views/SuperAdmin/OrganizationsView.vue` | mixed | `/api/admin/organizations` hoặc related | `OrganizationsController` | PARTIAL | Cần kiểm path thực tế. |
| P3 | Parent | `/parent/dashboard` | `views/PhuHuynh/Dashboard.vue` | none | missing | missing | MOCK/BLOCKED | Chưa bật portal mặc định. Cần quan hệ phụ huynh-con. |
| P3 | Parent | `/parent/finance/tuition` | `views/PhuHuynh/Finance/*` | none | missing | missing | MOCK/BLOCKED | Không được trả dữ liệu con nếu chưa kiểm scope. |
| P2 | Content Council | `/content-council/subjects` | `pages/content-council/subjects/SubjectListPage.vue` | store mock | possible `/api/master-data/subjects` | `SubjectsController` | MOCK | Store đang init mock, cần contentCouncilApi. |
| P2 | Content Council | `/content-council/question-bank` | `QuestionBankPage.vue` | mock/missing | question-bank APIs | `QuestionBankController` | PARTIAL/MISSING_FRONTEND | Cần map chi tiết. |
| P2 | Content Council | `/content-council/quizzes` | `QuizListPage.vue` | mock/missing | quiz-management APIs | `QuizManagementController` | PARTIAL/MISSING_FRONTEND | Cần map chi tiết. |

## Endpoint mismatch đã xử lý trong branch này

| Trước | Sau | Lý do |
|---|---|---|
| `/api/application-schema/options` | `/api/applications/schema/options` | Theo backend contract Applications Schema. |
| `/api/application-schema/templates` | `/api/applications/schema/templates` | Đồng bộ path với backend. |
| `/api/application-schema/templates/{id}` | `/api/applications/schema/templates/{id}` | Đồng bộ path với backend. |

## Mock inventory cần xử lý tiếp

| File | Loại mock | Gợi ý xử lý |
|---|---|---|
| `frontend/src/views/SuperAdmin/UsersView.vue` | `mockUsers`, CRUD local, `alert()` | P1.1 nối `/api/admin/users`. |
| `frontend/src/views/GiangVien/Dashboard.vue` | Static dashboard | P1.3 tạo `teacherApi.getDashboard`. |
| `frontend/src/views/BGH/Dashboard.vue` | Static report/KPI | P1.5 tạo BGH report API aggregator. |
| `frontend/src/views/PhuHuynh/Dashboard.vue` | `childrenData`, localStorage | P1.6 tạo parent APIs và scope phụ huynh-con. |
| `frontend/src/stores/content-council/subjectStore.ts` | `initializeSubjectMockData()` | P1.7 nối content council API. |
| `frontend/src/services/staffApi.js` | mock fallback | Đã khóa sau `VITE_ENABLE_MOCK_API`; backend `/api/staff/*` vẫn cần tạo hoặc remap. |

## Thứ tự đề xuất tiếp theo

1. P0.7: chạy build/lint/test trên branch này, sửa lỗi phát sinh nếu có.
2. P1.1: nối SuperAdmin Users/RBAC vì backend đã có controller rõ.
3. P1.2: sửa Student dashboard/assignments để không hardcode/mock.
4. P1.3: Teacher dashboard/classes/grading + hoàn thiện proctoring.
5. P1.4: quyết định Staff dùng `/api/staff/*` aggregator hay remap sang schedule/application APIs hiện có.
6. P1.5: BGH report aggregator.
7. P1.6: Parent portal sau khi có scope phụ huynh-con.
8. P1.7: Content Council API thật.
>>>>>>> pr45-local
