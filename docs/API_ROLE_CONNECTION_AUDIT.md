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
