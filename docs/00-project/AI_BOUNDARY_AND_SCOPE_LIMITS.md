# AI WORKSPACE BOUNDARY & SCOPE LIMITS (QUY TẮC GIỚI HẠN PHẠM VI LÀM VIỆC CỦA AI)

> **Mục đích:** Thiết lập ranh giới cứng (Hard Boundaries) và danh mục các tệp được phép / nghiêm cấm AI can thiệp trong đợt triển khai kế hoạch khắc phục 3 role **Ban Giám Hiệu (BGH)**, **Giảng Viên (Teacher)**, và **Sinh Viên (Student)** theo [LMS_3_ROLE_REMEDIATION_PLAN.md](file:///D:/A/Du-An-Tot-Nghiep/docs/00-project/LMS_3_ROLE_REMEDIATION_PLAN.md).  
> **Nguyên tắc:** Bất kỳ agent hay phiên AI nào khi làm việc trong repository này **bắt buộc tuân thủ 100%** các quy tắc dưới đây, không có ngoại lệ.

---

## 1. Ma trận Ranh giới Tổng quan (Boundary Matrix)

| Khu vực / Phân hệ | Trạng thái quyền AI | Ghi chú & Ràng buộc |
|---|---|---|
| **Role BGH (`Principal`)** | ✅ **ĐƯỢC PHÉP** | Quản lý nhân sự GV theo cơ sở, năng lực, tải giảng dạy, nhật ký ca dạy, cây phạm vi. |
| **Role Giảng Viên (`Teacher`)** | ✅ **ĐƯỢC PHÉP** | Xem lịch dạy, lớp, điểm danh thật, chấm bài nộp file thật, coi thi không suy diễn. |
| **Role Sinh Viên (`Student`)** | ✅ **ĐƯỢC PHÉP** | Xem lịch học công bố, nộp file bài tập thật, thi trực tuyến an toàn concurrency, nhận kết quả, đánh giá GV. |
| **Backend & Database chung** | ⚠️ **CÓ ĐIỀU KIỆN** | Chỉ sửa Controller, Service, DTO, Model và Migration **phục vụ trực tiếp** cho 3 role trên. |
| **Role Giáo Vụ (`AcademicStaff`)** | ❌ **NGHIÊM CẤM** | Không sửa UI/API riêng của Giáo vụ trong đợt này. |
| **Role Super Admin / Admin** | ❌ **NGHIÊM CẤM** | Không can thiệp màn hình quản trị toàn cục, phân quyền hệ thống ngoài scope BGH. |
| **Role Phụ Huynh (`Parent`)** | ❌ **NGHIÊM CẤM** | Nằm ngoài phạm vi đợt remediation này. |
| **Role Hội Đồng Nội Dung (`ContentCouncil`)**| ❌ **NGHIÊM CẤM** | Nằm ngoài phạm vi đợt remediation này. |
| **Thuật toán & Xếp lịch tự động** | ❌ **NGHIÊM CẤM** | Không sửa/viết lại engine xếp lịch, thuật toán phân bổ phòng/ca học. Thành viên khác phụ trách. |
| **Hạ tầng Auth & Security gốc** | ❌ **NGHIÊM CẤM** | Không sửa JWT token format, Auth Middleware cốt lõi, base `auth.js` store. |
| **Hệ thống CSS Design Tokens** | ❌ **NGHIÊM CẤM** | Không sửa biến màu trong `liquid-glass.css`, giữ nguyên semantic token pattern. |

---

## 2. Vùng Cấm Tuyệt Đối (Forbidden Zones)

AI **tuyệt đối không được phép chỉnh sửa, xóa hoặc ghi đè** các tệp và thư mục sau:

### 2.1. Frontend - Các Role ngoài phạm vi
- `frontend/src/views/Admin/**`
- `frontend/src/views/SuperAdmin/**`
- `frontend/src/views/GiaoVu/**`
- `frontend/src/views/Parent/**`
- `frontend/src/views/ContentCouncil/**`
- `frontend/src/components/Admin/**`
- `frontend/src/components/SuperAdmin/**`
- `frontend/src/components/GiaoVu/**`
- `frontend/src/components/Parent/**`
- `frontend/src/components/ContentCouncil/**`

### 2.2. Frontend - Shared Layouts & Core Foundation
- `frontend/src/components/SinhVien/Layout_SinhVien.vue` (trừ khi cần cập nhật route link đã thống nhất)
- `frontend/src/assets/liquid-glass.css` (Cấm sửa token gốc)
- `frontend/src/stores/auth.js` (Cấm sửa cơ chế lưu token/session)
- `frontend/src/services/apiClient.js` (Cấm đổi interceptor cơ sở)

### 2.3. Backend - Scheduling Engine & Subsystems ngoài phạm vi
- `Backend/Services/AcademicScheduling/**` (Engine xếp lịch tự động - DO NOT TOUCH)
- `Backend/Controllers/AcademicSchedulingContextController.cs`
- `Backend/Controllers/FinancePaymentWebhooksController.cs`
- `Backend/Controllers/FinanceSchemaController.cs`
- `Backend/Controllers/ProgramTuitionConfigsController.cs`
- `Backend/Controllers/ParentController.cs`

### 2.4. Database & Infrastructure
- Cấm xóa các Migration đã có trong `Backend/Migrations/**`.
- Cấm đổi tên cột/bảng trong `Backend/Models/**` gây breaking database mà không tạo migration có chủ đích.
- Cấm sửa `Program.cs` trừ khi đăng ký DI cho Service/Repository mới của 3 role.

---

## 3. Vùng Được Phép Làm Việc (Allowed Workspaces)

AI được phép đọc, tạo mới, hoặc chỉnh sửa các tệp thuộc các khu vực sau:

### 3.1. Phân hệ Ban Giám Hiệu (BGH)
- **Frontend Views & Components:**
  - `frontend/src/views/BGH/UsersView.vue` (Loại bỏ fake Excel import, tinh chỉnh quyền sửa)
  - `frontend/src/views/BGH/RolesView.vue` (Hiển thị cây quản lý thật)
  - `frontend/src/views/BGH/HumanResources/**` (Tạo mới module quản lý nhân sự GV)
  - `frontend/src/views/BGH/Dashboard.vue`, `AcademicTermsView.vue`, `CurriculumView.vue`, `FacilitiesView.vue`, `OrganizationsView.vue`, `ProgramsView.vue`
  - `frontend/src/components/BGH/**`
- **Frontend Services:**
  - `frontend/src/services/bghApi.js`
  - `frontend/src/services/bghPersonnelApi.js` (Tạo mới nếu cần)
  - `frontend/src/services/bghEvaluationApi.js`
- **Backend Controllers & Services:**
  - `Backend/Controllers/BghAcademicController.cs`
  - `Backend/Controllers/BghDashboardController.cs`
  - `Backend/Controllers/BghEvaluationController.cs`
  - `Backend/Controllers/BghFacadeController.cs`
  - `Backend/Controllers/BghTeacherPersonnelController.cs` (Tạo mới)
  - `Backend/Services/TeacherPersonnel/**` (Tạo mới)
  - `Backend/DTOs/TeacherPersonnel/**` (Tạo mới)

### 3.2. Phân hệ Giảng Viên (Teacher)
- **Frontend Views & Components:**
  - `frontend/src/views/GiangVien/TeachingScheduleView.vue` (Lịch dạy)
  - `frontend/src/views/GiangVien/ClassListView.vue`, `ClassDetailView.vue`, `ClassAttendanceView.vue`, `AttendanceHistoryView.vue`
  - `frontend/src/views/GiangVien/AssignmentListView.vue`, `AssignmentSubmissionsView.vue` (Tải file thật, chấm điểm)
  - `frontend/src/views/GiangVien/ProctoringDashboardView.vue`, `ProctoringAttendanceView.vue`, `ProctoringReportView.vue`, `ProctoringSessionsView.vue` (Coi thi không suy diễn)
  - `frontend/src/views/GiangVien/ExamResultsView.vue`, `ProfileView.vue`, `TeachingPreferencesView.vue`
  - `frontend/src/components/GiangVien/**`
- **Frontend Services:**
  - `frontend/src/services/teacherApi.js` (Làm sạch mock dữ liệu coi thi)
  - `frontend/src/services/examProctoringHub.js`
- **Backend Controllers & Services:**
  - `Backend/Controllers/TeacherScheduleController.cs`
  - `Backend/Controllers/TeacherClassesController.cs`
  - `Backend/Controllers/TeacherAttendanceHistoryController.cs`
  - `Backend/Controllers/TeacherSubmissionsController.cs`
  - `Backend/Controllers/TeacherExamController.cs`, `TeacherExamResultsController.cs`
  - `Backend/Controllers/TeacherTeachingPreferencesController.cs`
  - `Backend/Services/Teacher/**`

### 3.3. Phân hệ Sinh Viên (Student)
- **Frontend Views & Components:**
  - `frontend/src/views/Student/AssignmentDetailView.vue`, `AssignmentsView.vue` (Nộp file thật)
  - `frontend/src/views/Student/ScheduleView.vue` (Xem lịch học công bố)
  - `frontend/src/views/Student/ExamTakeView.vue`, `ExamsView.vue`, `ExamDetailView.vue`, `ExamResultView.vue` (Thi trực tuyến)
  - `frontend/src/views/Student/GradesView.vue`, `AttendanceView.vue`, `EvaluationsView.vue`, `CourseDetailView.vue`, `CoursesView.vue`, `CurriculumView.vue`
  - `frontend/src/views/SinhVien/**`
  - `frontend/src/components/Student/**`
- **Frontend Services:**
  - `frontend/src/services/studentApi.js`
  - `frontend/src/services/assignmentApi.js`
  - `frontend/src/services/examApi.js`
  - `frontend/src/services/evaluationsApi.js`
- **Backend Controllers & Services:**
  - `Backend/Controllers/StudentAssignmentsController.cs`
  - `Backend/Controllers/StudentScheduleController.cs`
  - `Backend/Controllers/ExamController.cs`, `QuizAttemptsController.cs`
  - `Backend/Controllers/StudentEvaluationsController.cs`
  - `Backend/Controllers/StudentGradesController.cs`
  - `Backend/Services/Exam/**`, `Backend/Services/QuizAttempts/**`
  - `Backend/Models/PhienThiHocSinh.cs` (Thêm Concurrency Version)

### 3.4. Router & Menu Configuration (Dành riêng cho 3 role)
- `frontend/src/router/index.js` (Chỉ thêm/sửa route thuộc `/bgh/`, `/giang-vien/`, `/sinh-vien/` hoặc `/student/`)
- `frontend/src/components/BGH/data/menuData.js`
- `frontend/src/components/GiangVien/data/menuData.js`

---

## 4. Quy Tắc Ứng Xử Của AI Khi Viết Code (AI Operating Rules)

1. **QUY TẮC SỐ 1 - KHÔNG FAKE SUCCESS, KHÔNG MOCK GIẢ LẬP:**
   - Tuyệt đối cấm dùng `setTimeout` để tạo cảm giác "đã thành công" khi backend chưa thực hiện ghi dữ liệu.
   - Tuyệt đối cấm tự bịa response `logs: []`, `status: "present"`, `streamStatus: "streaming"` khi chưa có kết nối thật từ SignalR/WebRTC/API.
   - Nếu tính năng chưa có backend: Giao diện phải hiển thị rõ trạng thái `Chưa hỗ trợ` hoặc disable nút kèm tooltip giải thích.
2. **QUY TẮC SỐ 2 - CHỈ SỬ DỤNG DỮ LIỆU TỪ SQL SERVER THẬT:**
   - Không tạo các file mock JSON trong `frontend/src/mocks` để bypass API.
   - Mọi truy vấn phải đi qua API Controller -> EF Core -> SQL Server.
3. **QUY TẮC SỐ 3 - BẢO ĐẢM TÍNH TOÀN VẸN VÀ ĐỒNG THỜI (CONCURRENCY SAFETY):**
   - Các bảng có tần suất ghi cao hoặc tranh chấp đồng thời (như `PhienThiHocSinh`, `BaiNop`) phải có cơ chế chống ghi đè (Optimistic Concurrency / Timestamp / Versioning).
   - Thao tác nộp bài phải có tính `idempotent` (gửi nhiều lần cùng 1 nội dung không gây duplicate kết quả).
4. **QUY TẮC SỐ 4 - KIỂM SOÁT PHẠM VI (CAMPUS / TENANT SCOPING):**
   - Mọi query của BGH phải lọc theo cơ sở (`MaDonVi` của user hiện tại).
   - BGH chỉ có quyền thao tác trên giảng viên/sinh viên thuộc cơ sở của mình.
   - Cấm BGH gán hoặc tạo tài khoản có quyền `SuperAdmin`, `Admin`, `Principal`.
5. **QUY TẮC SỐ 5 - GHI NHẬT KÝ KIỂM TOÁN (AUDIT LOGGING):**
   - Mọi thao tác ghi dữ liệu nhân sự, thay đổi quyền, phân công chuyên môn của BGH phải được ghi nhận vào `NhatKyKiemToan` (`AuditLog`) với đầy đủ: `NguoiThucHien`, `ThoiDiem`, `HanhDong`, `GiaTriCu`, `GiaTriMoi`, `LyDo`.
6. **QUY TẮC SỐ 6 - CHUẨN THIẾT KẾ SEMANTIC TOKEN:**
   - Tuyệt đối không hardcode `bg-white`, `text-slate-900`, `border-slate-200`.
   - Sử dụng đúng token của hệ thống: `surface-card`, `surface-input`, `text-heading`, `text-body`, `border-default`, `lg-glass`.

---

## 5. Quy Định Về Môi Trường Database (SQL Server trên Docker)

> [!IMPORTANT]
> **Toàn bộ cơ sở dữ liệu (Database) của dự án chạy trong Docker Container (`sqlserver`).**  
> Khi AI hoặc người dùng thực hiện truy vấn dữ liệu, kiểm tra lỗi DB, đối chiếu audit/bản ghi, hoặc chạy lệnh liên quan đến database:
> - **Container Database:** Dịch vụ `sqlserver` trong `docker-compose.yml` (Image: `mcr.microsoft.com/mssql/server:2022-latest`).
> - **Cổng kết nối:** `1433:1433` (Host: `127.0.0.1,1433` hoặc `localhost,1433`).
> - **Tài khoản SA:** `sa` / `Test@123_PassWord!`, Database: `LMS`.
> - **Cách thức truy vấn/kiểm tra:**
>   - Qua chuỗi kết nối Docker: `Server=127.0.0.1,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;`
>   - Hoặc qua lệnh Docker trực tiếp: `docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Test@123_PassWord!" -C -d LMS -Q "<CÂU_LỆNH_SQL>"`
> - **Khi có lỗi DB hoặc yêu cầu tái tạo dữ liệu:** Kiểm tra trạng thái container bằng `docker compose ps` / `docker compose logs sqlserver` và chạy seed tương ứng theo cấu hình `SeedProfile=LargeDemo`.

---

## 6. Danh Sách Kiểm Tra Khi Hoàn Thành Nhiệm Vụ (AI Verification Checklist)

Trước khi bàn giao bất kỳ task nào, AI phải tự kiểm tra:

- [ ] Không có file nào ngoài danh mục Cho Phép (Allowed Workspaces) bị thay đổi trong git status.
- [ ] Backend build thành công (`dotnet build Backend`).
- [ ] Frontend build và lint thành công (`npm run build`, `npm run lint` trong `frontend`).
- [ ] Không có token, password hoặc connection string nhạy cảm bị commit.
- [ ] Mọi endpoint mới đều có kiểm thử tích hợp (API Integration Test) hoặc bài test tương ứng.
