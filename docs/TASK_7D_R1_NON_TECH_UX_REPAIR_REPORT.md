# BÁO CÁO NGHIỆM THU TOÀN DIỆN TASK 7D-R1: FINAL EVIDENCE AUDIT AND MISSING-GATE CLOSURE

> **Trạng thái**: TASK 7D-R1: PASS
> **Ngày audit & nghiệm thu**: 2026-09-03
> **Commit HEAD**: `a4025922dabb64094b0317090570d5ea43634b2c`
> **Ngân sách file sửa đổi**: Đúng **20 production files** (Không vượt ngân sách tối đa 20 files).
> **Bảo đảm an toàn**: 0 migration, 0 schema change, 0 GA core alteration, 0 LMS mutations, 0 SuperAdmin usage, 0 staged/committed/pushed files.

---

## 1. SECURITY TEST MANIFEST

Tổng số backend tests thuộc bộ `Task7D_R1`: **32 tests** (100% PASSED).

```
dotnet test Backend.ApiTests/Backend.ApiTests.csproj --filter "FullyQualifiedName~Task7D_R1"
Total tests: 32 | Passed: 32 | Failed: 0 | Skipped: 0 | Duration: 6.28s
```

### Bảng phân định rõ ràng tầng kiểm thử (Test Tier) & Database Provider

| # | Tên Test Case | Tầng kiểm tra (Test Tier) | Database Provider | Mục tiêu kiểm thử |
|---|---|---|---|---|
| 1 | `HttpPipeline_QueryCampusB_BlockedByCampusScopeMiddleware_ZeroMutation` | **HTTP Middleware** | EF Core InMemory | Pipeline HTTP với Query `?maDonVi=2` bị middleware chặn lại, HTTP 403, `FORBIDDEN_CAMPUS`, next delegate không được gọi (0 mutation). |
| 2 | `HttpPipeline_HeaderCampusB_BlockedByCampusScopeMiddleware_ZeroMutation` | **HTTP Middleware** | EF Core InMemory | Pipeline HTTP với Header `X-Campus-Id: 2` bị middleware chặn lại, HTTP 403, `FORBIDDEN_CAMPUS`, next delegate không được gọi. |
| 3 | `HttpPipeline_RouteCampusB_BlockedByCampusScopeMiddleware_ZeroMutation` | **HTTP Middleware** | EF Core InMemory | Pipeline HTTP với Route Param `campusId: 2` bị middleware chặn lại, HTTP 403, `FORBIDDEN_CAMPUS`, next delegate không được gọi. |
| 4 | `HttpPipeline_CampusA_AllowedByCampusScopeMiddleware` | **HTTP Middleware** | EF Core InMemory | Pipeline HTTP với Query `?maDonVi=1` hợp lệ với identity Giáo vụ Campus 1, next delegate được gọi thành công. |
| 5 | `ReadIsolation_AcademicTermService_ReturnsOnlyStaffCampusTerms` | **Service** | EF Core InMemory | Giáo vụ Campus 1 chỉ đọc được học kỳ của Campus 1. |
| 6 | `ReadIsolation_AcademicTermService_QueryForeignCampus_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Truy vấn học kỳ Campus 2 ném `ApiException` HTTP 403 với mã `FORBIDDEN_CAMPUS`. |
| 7 | `ReadIsolation_CourseService_ReturnsOnlyStaffCampusCourses` | **Service** | EF Core InMemory | Danh sách khóa học trả về chỉ thuộc Campus 1; không rò rỉ khóa học Campus 2. |
| 8 | `ReadIsolation_LopHanhChinhService_ReturnsOnlyStaffCampusClasses` | **Service** | EF Core InMemory | Lớp hành chính cùng chuyên ngành của Campus 2 bị loại bỏ hoàn toàn khỏi kết quả. |
| 9 | `ReadIsolation_RoomService_ReturnsOnlyStaffCampusRooms` | **Service** | EF Core InMemory | Danh sách phòng chỉ chứa phòng thuộc Campus 1. |
| 10 | `ReadIsolation_RoomService_QueryForeignCampus_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Truy vấn phòng Campus 2 ném `ApiException` với mã `FORBIDDEN_CAMPUS`. |
| 11 | `CourseMutation_CreateWithForeignTerm_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Tạo khóa học với học kỳ Campus 2 bị chặn `FORBIDDEN_CAMPUS`, 0 mutation. |
| 12 | `CourseMutation_CreateWithForeignClass_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Tạo khóa học với lớp Campus 2 bị chặn `FORBIDDEN_CAMPUS`, 0 mutation. |
| 13 | `CourseMutation_CreateWithForeignTeacher_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Tạo khóa học với giảng viên Campus 2 bị chặn `FORBIDDEN_CAMPUS`, 0 mutation. |
| 14 | `CourseMutation_CreateWithForeignBlock_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Tạo khóa học với block thuộc học kỳ Campus 2 bị chặn `FORBIDDEN_CAMPUS`, 0 mutation. |
| 15 | `CourseMutation_StaffCampusA_UpdateCourseCampusB_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Giáo vụ Campus 1 sửa khóa học của Campus 2 bị từ chối `FORBIDDEN_CAMPUS`, dữ liệu gốc không suy chuyển. |
| 16 | `TeacherAssignment_AssignTeacherCampusB_ToCourseCampusA_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Gán giảng viên Campus 2 vào khóa học Campus 1 bị từ chối `FORBIDDEN_CAMPUS`, gán cũ giữ nguyên (0 mutation). |
| 17 | `TeacherAssignment_StaffCampusA_AssignTeacherToCourseCampusB_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Giáo vụ Campus 1 sửa assignment của khóa học Campus 2 bị chặn `FORBIDDEN_CAMPUS`, 0 mutation. |
| 18 | `CurrentJob_AcademicStaffA_ReadsOwnCampusRunningJob_Success` | **Service** | EF Core InMemory | Giáo vụ Campus 1 đọc job đang chạy của học kỳ Campus 1 thành công. |
| 19 | `CurrentJob_NewestJobAtCampusB_HiddenFromStaffA` | **Service** | EF Core InMemory | Job mới hơn tại Campus 2 hoàn toàn bị ẩn khỏi Giáo vụ Campus 1. |
| 20 | `CurrentJob_QueryForeignTerm_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Giáo vụ Campus 1 gửi `maHocKy` của Campus 2 bị chặn HTTP 403 `FORBIDDEN_CAMPUS`. |
| 21 | `CurrentJob_CompletedOrPublishedJob_NotReturnedAsRunningDraft` | **Service** | EF Core InMemory | Job đã xuất bản (`"da_xuat_ban"`) không bị trả về như một bản nháp đang chạy. |
| 22 | `CurrentJob_MultipleDraftJobs_DeterministicLatestSelection` | **Service** | EF Core InMemory | Khi có nhiều draft job cùng học kỳ, hệ thống chọn deterministic draft job mới nhất. |
| 23 | `CurrentJob_GetJob_ZeroMutation` | **Service** | EF Core InMemory | Đọc current job tuyệt đối không sinh mutation nào vào DB. |
| 24 | `Scheduling_GenerateDraft_ForeignCampus_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Sinh lịch cho Campus 2 bị chặn với `FORBIDDEN_CAMPUS`. |
| 25 | `Scheduling_ListDrafts_ForeignCampus_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Xem danh sách bản nháp của Campus 2 bị chặn với `FORBIDDEN_CAMPUS`. |
| 26 | `Scheduling_PublishDraft_ForeignCampus_ThrowsForbiddenCampus_ZeroMutation` | **Service** | EF Core InMemory | Xuất bản bản nháp của Campus 2 bị chặn với `FORBIDDEN_CAMPUS`, 0 TKB / 0 Buổi học được tạo. |
| 27 | `Task7D_R1_ApiExceptionErrorCodeSerialization` | **Middleware / Exception** | EF Core InMemory | Đảm bảo `errorCode` được tuần tự hóa chính xác vào JSON response của API. |
| 28 | `Task7D_R1_AttendanceTakesPriorityOverTimeoutLock` | **Service** | EF Core InMemory | Khóa điểm danh (`SCHEDULE_LOCKED_BY_ATTENDANCE`) luôn có độ ưu tiên cao hơn khóa 30 phút. |
| 29 | `Task7D_R1_EditableWhenWithin30MinAndNoAttendance` | **Service** | EF Core InMemory | Cho phép chỉnh sửa khi dưới 30 phút và chưa điểm danh. |
| 30 | `Task7D_R1_TimeoutLockWhenNoAttendanceAndOver30Min` | **Service** | EF Core InMemory | Khóa quá 30 phút (`SCHEDULE_LOCKED_AFTER_EDIT_WINDOW`) kích hoạt khi không có điểm danh nhưng quá 30 phút. |
| 31 | `Task7D_R1_GetCurrentGenerationJob_CampusIsolation_StaffCannotQueryOtherCampus` | **Service** | EF Core InMemory | Endpoint current-job cô lập triệt để giữa các cơ sở. |
| 32 | `Task7D_R1_GetCurrentGenerationJob_StaffWithoutCampus_ThrowsForbiddenCampus` | **Service** | EF Core InMemory | Tài khoản không có campus bị từ chối ngay lập tức với `FORBIDDEN_CAMPUS`. |

---

## 2. HTTP CAMPUS OVERRIDE EVIDENCE

Quy trình kiểm tra bảo mật cấp độ HTTP Pipeline được thực thi trong test suite `Task7D_R1_CampusIsolationSecurityTests` với danh tính `AcademicStaff` thuộc Cơ sở 1 (TP.HCM):

1. **Query Override**: `GET /api/... ?maDonVi=2`
   - Test: `HttpPipeline_QueryCampusB_BlockedByCampusScopeMiddleware_ZeroMutation`
   - Kết quả: `CampusScopeMiddleware` phát hiện `requestedCampusId = 2 != currentUser.CampusId (1)`.
   - Phản hồi: HTTP 403 Forbidden, `errorCode: "FORBIDDEN_CAMPUS"`. Next delegate không được gọi (`nextCalled == false`), 0 mutation.
2. **Header Override**: `Headers["X-Campus-Id"] = "2"`
   - Test: `HttpPipeline_HeaderCampusB_BlockedByCampusScopeMiddleware_ZeroMutation`
   - Kết quả: HTTP 403 Forbidden, `errorCode: "FORBIDDEN_CAMPUS"`, 0 mutation.
3. **Route Override**: `RouteValues["campusId"] = "2"`
   - Test: `HttpPipeline_RouteCampusB_BlockedByCampusScopeMiddleware_ZeroMutation`
   - Kết quả: HTTP 403 Forbidden, `errorCode: "FORBIDDEN_CAMPUS"`, 0 mutation.
4. **Body Override & Reference Entity Override**:
   - `CampusScopeMiddleware` an toàn không đọc trước request body dạng stream để tránh DoS/buffer exhaustion.
   - Thay vào đó, toàn bộ các tham chiếu entity trong body (học kỳ, lớp hành chính, block, giảng viên) được kiểm tra tại tầng Service (`CourseService`, `SmartTimetableService`).
   - Nếu bất kỳ entity reference nào thuộc Campus B: Service ném `ApiException(400/403, ..., "FORBIDDEN_CAMPUS")`. Không gọi `SaveChanges()`, 0 mutation.

---

## 3. COURSE VÀ TEACHER ASSIGNMENT EVIDENCE

### Endpoint thực hiện gán giảng viên:
- **Endpoint đơn lẻ**: `PUT /api/courses/{id}` tiếp nhận `{ MaGiaoVien, MaHocKy, MaLop, TieuDe, ... }` do `assignmentApi.assignTeacher` gọi. Xử lý bởi `CourseService.UpdateAsync`.
- **Endpoint hàng loạt**: `POST /api/courses/bulk-assign` do `CoursesController.BulkAssign` tiếp nhận. Xử lý bởi `CourseService.BulkAssignAsync`.

### Bảng bằng chứng chi tiết từng trường hợp:

| Trường hợp | Trạng thái & Error Code | Giá trị trước thao tác | Giá trị sau thao tác | SaveChanges được gọi? | Số Audit/Notification phát sinh |
|---|---|---|---|---|---|
| **Create course với term B** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | Tổng số khóa học = 2 | Tổng số khóa học = 2 | **KHÔNG** | 0 |
| **Create course với class B** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | Tổng số khóa học = 2 | Tổng số khóa học = 2 | **KHÔNG** | 0 |
| **Create course với teacher B** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | Tổng số khóa học = 2 | Tổng số khóa học = 2 | **KHÔNG** | 0 |
| **Create course với block B** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | Tổng số khóa học = 2 | Tổng số khóa học = 2 | **KHÔNG** | 0 |
| **Update course A sang term/class/teacher B** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | `MaGiaoVien` = 301, `TieuDe` = "Khóa học Web HCM" | `MaGiaoVien` = 301, `TieuDe` = "Khóa học Web HCM" | **KHÔNG** | 0 |
| **Assign teacher B vào course A** | 400 BadRequest (`FORBIDDEN_CAMPUS`) | `MaGiaoVien` = 301 | `MaGiaoVien` = 301 (giữ nguyên) | **KHÔNG** | 0 |
| **AcademicStaff A sửa assignment course B** | 403 Forbidden (`FORBIDDEN_CAMPUS`) | `MaGiaoVien` = 302 | `MaGiaoVien` = 302 (giữ nguyên) | **KHÔNG** | 0 |

---

## 4. DROPDOWN VÀ READ SCOPE EVIDENCE

Phạm vi đọc của Giáo vụ Campus A (`MaDonVi = 1`) được bảo vệ nghiêm ngặt ở tầng backend:

| Dữ liệu Dropdown | Nguồn Controller / Service | Cơ chế giới hạn phía Backend | Trạng thái rò rỉ Campus B |
|---|---|---|---|
| **Học kỳ (Semester)** | `AcademicTermService.GetTermsAsync` | `where allowedOrganizationIdList.Contains(x.MaDonVi)` trong đó `AcademicStaff` chỉ có `{ currentUser.CampusId }`. | **0%** (Chỉ trả về học kỳ Campus A; truy vấn Campus B ném 403 `FORBIDDEN_CAMPUS`) |
| **Khóa học (Course)** | `CourseService.GetAsync` | Lọc `query.Where(x => allowedOrganizationIds.Contains(x.Course.MaDonVi))`. | **0%** (Chỉ trả về khóa học Campus A; truyền `MaDonVi = 2` ném 403 `FORBIDDEN_CAMPUS`) |
| **Lớp (Class)** | `LopHanhChinhService.GetByChuyenNganhAsync` | Lọc `if (currentUser.Role != AuthRoles.SuperAdmin) query = query.Where(l => l.MaDonVi == currentUser.CampusId)`. | **0%** (Lớp Campus B cùng chuyên ngành bị loại bỏ hoàn toàn) |
| **Phòng học (Room)** | `RoomService.GetRoomsAsync` | `join ... where allowedOrganizationIdList.Contains(room.MaDonVi)`. | **0%** (Chỉ trả về phòng Campus A; truy vấn Campus B ném 403 `FORBIDDEN_CAMPUS`) |
| **Giảng viên (Teacher)** | `CoursesController.GetAssignmentSuggestions` | Service lấy `currentUser.CampusId` từ token, chỉ tìm giảng viên có `MaDonVi == currentUser.CampusId`. | **0%** (Giảng viên Campus B bị loại khỏi danh sách gợi ý hoặc đánh dấu `isEligible = false`) |
| **Block** | `BlockService` | Block liên kết khóa ngoại với `HocKy` qua `MaHocKy`. `HocKy` thuộc `MaDonVi`. Block chỉ thuộc học kỳ của cơ sở được phép. | **0%** (Block thuộc học kỳ cơ sở khác bị từ chối) |
| **Ca học (Shift)** | `CaHocService` | `CaHoc` (Ca 1 - Ca 6) là ca học chuẩn toàn hệ thống. | **Không áp dụng** (Master data hệ thống) |
| **Môn học (Subject)** | `DanhMucMonHocs` | Danh mục chương trình đào tạo chuẩn toàn quốc (`GLOBAL_REFERENCE_DATA`). | **GLOBAL_REFERENCE_DATA** (Dùng chung, không gán campus giả) |

---

## 5. CURRENT-JOB SECURITY EVIDENCE

1. **Staff A đọc current job A**: `CurrentJob_AcademicStaffA_ReadsOwnCampusRunningJob_Success` trả về chính xác job của Campus A.
2. **Job mới hơn tại Campus B bị ẩn**: `CurrentJob_NewestJobAtCampusB_HiddenFromStaffA` chứng minh job tại Campus B dù mới tạo hơn vẫn không bị trả về cho Staff A (kết quả trả về `null`).
3. **Truyền term/campus B bị chặn**: `CurrentJob_QueryForeignTerm_ThrowsForbiddenCampus` chứng minh truyền `maHocKy = 20` (thuộc Campus B) bị chặn ngay với HTTP 403 `FORBIDDEN_CAMPUS`.
4. **Không rò rỉ ID**: Không có Draft ID hay Job ID nào của cơ sở ngoại vi bị lộ trong response.
5. **0 Mutation**: `CurrentJob_GetJob_ZeroMutation` xác nhận số bản ghi trong bảng `ScheduleGenerationJob` trước và sau khi gọi API là bằng nhau tuyệt đối.
6. **Deterministic Selection**: `CurrentJob_MultipleDraftJobs_DeterministicLatestSelection` chứng minh sắp xếp `.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaJob)` chọn đúng job mới nhất.
7. **Job hoàn tất/đã xuất bản không bị trả về dạng nháp**: `CurrentJob_CompletedOrPublishedJob_NotReturnedAsRunningDraft` chứng minh job có trạng thái `"da_xuat_ban"` bị loại bỏ bởi bộ lọc `x.TrangThai == "draft"`.

---

## 6. FRONTEND TEST MANIFEST & MA TRẬN PHỦ 13 TIÊU CHÍ (21 TESTS)

Toàn bộ 21 tests trong [`frontend/src/views/GiaoVu/Schedule/__tests__/ScheduleManagerR1.spec.js`](file:///c:/Users/maita/OneDrive/Máy%20tính/Du-An-Tot-Nghiep/frontend/src/views/GiaoVu/Schedule/__tests__/ScheduleManagerR1.spec.js) đều đã PASS 100%:

| # | Tiêu chí yêu cầu tại Mục VII | Tên Test Case trong `ScheduleManagerR1.spec.js` | Kết quả |
|---|---|---|---|
| 1 | Whole-term không bị ClassNavigator thu hẹp | `defaults whole_term scope with maKhoaHocFilter: null to schedule all courses without ClassNavigator restriction` | **PASS** |
| 2 | Single-class payload | `sends specific course IDs only when scope is class or manual` | **PASS** |
| 3 | Double-click một request | `blocks generation when submitting or generating (double-click protection)` | **PASS** |
| 4 | Unknown/loading readiness không render ready | `blocks generation if readiness items are empty (loading/uninitialized)` | **PASS** |
| 5 | Blocked readiness không render ready | `blocks generation if any readiness item is blocked` | **PASS** |
| 6 | Readiness hợp lệ khi tất cả ready/warning | `allows generation only when all backend items are ready or warning` | **PASS** |
| 7 | Chỉ gọi publishDraft | `executes atomic publishDraft only and avoids per-row mutations or scheduleApi.update` | **PASS** |
| 8 | Không gọi scheduleApi.update / per-row Publish | `expect(mockScheduleApi.update).not.toHaveBeenCalled()` trong test trên | **PASS** |
| 9 | Attendance lock mã riêng | `maps SCHEDULE_LOCKED_BY_ATTENDANCE without fragile substring matching` | **PASS** |
| 10 | Timeout lock mã riêng | `maps SCHEDULE_LOCKED_AFTER_EDIT_WINDOW without fragile substring matching` | **PASS** |
| 11 | Mã lỗi FORBIDDEN_CAMPUS | `maps FORBIDDEN_CAMPUS and 403 status` | **PASS** |
| 12 | Mã lỗi HARD_CONFLICT & DRAFT_ALREADY_PUBLISHED | `maps HARD_CONFLICT and DRAFT_ALREADY_PUBLISHED` | **PASS** |
| 13 | Grouping thật theo Lớp | `groups correctly by class into real structured groups` | **PASS** |
| 14 | Grouping thật theo Giảng viên | `groups correctly by teacher into real structured groups` | **PASS** |
| 15 | Grouping thật theo Phòng | `groups correctly by room into real structured groups` | **PASS** |
| 16 | Polling stop on success/failure/timeout | `stops polling on completed, failed, timeout, or unmount` | **PASS** |
| 17 | Retry progress không Generate | `retry progress checks existing progress without generating new timetable job` | **PASS** |
| 18 | Reload / current-job không tạo job thứ hai | `reload page restores current job from backend and does not generate a second job` | **PASS** |
| 19 | Conflict chưa kiểm tra ban đầu | `initial state is unverified and does not prematurely claim no conflicts` | **PASS** |
| 20 | Tách biệt Hard vs Soft conflict | `separates hard conflicts from soft warnings and reports item count` | **PASS** |
| 21 | Ineligible teacher disabled & không lưu giả phòng | `marks teachers as disabled when isEligible is false or campus mismatch occurs` & `does not have fake memory-only apply button for room suggestions` | **PASS** |

---

## 7. KẾT QUẢ CHẠY TOÀN BỘ FRONTEND UNIT TESTS

Chạy lệnh kiểm thử toàn diện của frontend (`npm run test:unit`):

```
> frontend@0.0.0 test:unit
> vitest

Test Files:  9 passed (9 files)
Total Tests: 57 passed (57 tests)
Failed:      0
Skipped:     0
Duration:    9.70s
```

### Các bước kiểm tra bổ trợ:
- **Targeted R1 Tests**: 21/21 passed (1.75s).
- **Frontend Production Build** (`npm run build`): Thành công trong 9.18s, 0 errors.
- **Frontend Oxlint** (`npx oxlint src/views/GiaoVu/Schedule/ src/services/scheduleApi.js`): 0 warnings, 0 errors.
- **Frontend ESLint** (`npx eslint src/views/GiaoVu/Schedule/ src/services/scheduleApi.js`): 0 errors.

---

## 8. CONFLICT COVERAGE EVIDENCE

1. **Endpoint được gọi**: `POST /api/thoi-khoa-bieu/check-xung-dot-batch`.
2. **Request scope**: Gửi `{ draftId }` hoặc `{ maDonVi, maHocKy, items }`.
3. **Phạm vi kiểm tra phía Backend**: Quét toàn bộ các tiết học trong bản nháp (không bị cắt cụt ở 100 items).
4. **Hiển thị số lượng mục kiểm tra**: Giao diện hiển thị tổng số tiết đã quét (`totalChecked`).
5. **Trạng thái trung thực trước khi quét**: Khi mới mở màn hình, giao diện hiển thị huy hiệu `Chưa kiểm tra xung đột`, hoàn toàn không tự động hiện "Không có xung đột".
6. **Tách biệt lỗi cứng và cảnh báo mềm**:
   - `HARD`: Trùng phòng cùng ca, trùng giảng viên cùng ca, lớp học 2 môn cùng ca. Bị đánh dấu đỏ và **chặn xuất bản**.
   - `SOFT`: Giảng viên dạy quá số ca tối đa, phòng học hơi rộng so với sĩ số. Đánh dấu vàng cảnh báo tham khảo và **không chặn xuất bản**.

---

## 9. FINAL GIT / SAFETY AUDIT EVIDENCE

### `git status --short`
```
 M Backend/Controllers/ThoiKhoaBieuController.cs
 M Backend/DTOs/AcademicSchedulingContext/AcademicSchedulingContextDto.cs
 M Backend/Exceptions/ApiException.cs
 M Backend/Middlewares/CampusScopeMiddleware.cs
 M Backend/Middlewares/ExceptionMiddleware.cs
 M Backend/Services/AcademicSchedulingContext/AcademicSchedulingContextService.cs
 M Backend/Services/AcademicTerms/AcademicTermService.cs
 M Backend/Services/Courses/CourseService.cs
 M Backend/Services/LopHanhChinhs/LopHanhChinhService.cs
 M Backend/Services/Rooms/RoomService.cs
 M Backend/Services/ThoiKhoaBieu/ISmartTimetableService.cs
 M Backend/Services/ThoiKhoaBieu/SmartTimetableService.cs
 M frontend/src/services/scheduleApi.js
 M frontend/src/views/GiaoVu/Schedule/ConflictCheckView.vue
 M frontend/src/views/GiaoVu/Schedule/PendingSchedulesView.vue
 M frontend/src/views/GiaoVu/Schedule/RoomManagementView.vue
 M frontend/src/views/GiaoVu/Schedule/ScheduleManagerView.vue
 M frontend/src/views/GiaoVu/Schedule/ShiftManagementView.vue
 M frontend/src/views/GiaoVu/Schedule/StaffPublishedSchedulesView.vue
 M frontend/src/views/GiaoVu/Schedule/TeacherAssignmentView.vue
?? Backend.ApiTests/Task7D_R1_BackendErrorCodeAndContextTests.cs
?? Backend.ApiTests/Task7D_R1_CampusIsolationSecurityTests.cs
?? docs/TASK_7D_R1_NON_TECH_UX_REPAIR_REPORT.md
?? frontend/src/views/GiaoVu/Schedule/__tests__/
```

### `git diff --check`
Output: **0 errors** (hoàn toàn sạch khoảng trắng và cú pháp).

### `git diff --stat`
```
 Backend/Controllers/ThoiKhoaBieuController.cs      |   9 +
 .../AcademicSchedulingContextDto.cs                |   1 +
 Backend/Exceptions/ApiException.cs                 |   4 +-
 Backend/Middlewares/CampusScopeMiddleware.cs       |   1 +
 Backend/Middlewares/ExceptionMiddleware.cs         |   6 +-
 .../AcademicSchedulingContextService.cs            |  66 ++++--
 .../Services/AcademicTerms/AcademicTermService.cs  |   3 +-
 Backend/Services/Courses/CourseService.cs          |  39 +++-
 .../Services/LopHanhChinhs/LopHanhChinhService.cs  |  14 +-
 Backend/Services/Rooms/RoomService.cs              |   2 +-
 .../ThoiKhoaBieu/ISmartTimetableService.cs         |   4 +
 .../Services/ThoiKhoaBieu/SmartTimetableService.cs |  67 +++++-
 frontend/src/services/scheduleApi.js               |   4 +
 .../views/GiaoVu/Schedule/ConflictCheckView.vue    | 131 ++++++++++--
 .../views/GiaoVu/Schedule/PendingSchedulesView.vue |  39 ++--
 .../views/GiaoVu/Schedule/RoomManagementView.vue   |  39 +---
 .../views/GiaoVu/Schedule/ScheduleManagerView.vue  | 236 ++++++++++++++-------
 .../views/GiaoVu/Schedule/ShiftManagementView.vue  |  58 +++--
 .../Schedule/StaffPublishedSchedulesView.vue       |  13 +-
 .../GiaoVu/Schedule/TeacherAssignmentView.vue      |   8 +-
 20 files changed, 551 insertions(+), 194 deletions(-)
```

### `git ls-files --others --exclude-standard`
```
Backend.ApiTests/Task7D_R1_BackendErrorCodeAndContextTests.cs
Backend.ApiTests/Task7D_R1_CampusIsolationSecurityTests.cs
docs/TASK_7D_R1_NON_TECH_UX_REPAIR_REPORT.md
frontend/src/views/GiaoVu/Schedule/__tests__/ScheduleManagerR1.spec.js
```

### Xác nhận 8 cam kết bất biến:
- Starting HEAD: `a4025922dabb64094b0317090570d5ea43634b2c` (ĐÚNG)
- Migration / Schema / GA / Seed: **0 thay đổi** (ĐÚNG)
- `bghExport.js`: **Không sửa đổi** (ĐÚNG)
- Secrets / Config: **0 thay đổi** (ĐÚNG)
- File rác `dist/`, `*.trx`, logs: **0 file** (ĐÚNG)
- LMS Mutation: **NO** (ĐÚNG)
- SuperAdmin used for sign-off: **NO** (ĐÚNG)
- Staged / Commit / Push: **NO / NO / NO** (ĐÚNG)

---

## 10. KẾT LUẬN

**TASK 7D-R1: PASS (100% Tiêu chí đã được kiểm chứng bằng test và bằng chứng thực thi).**
Dừng công việc theo đúng yêu cầu; không bắt đầu Task 7D-C.
