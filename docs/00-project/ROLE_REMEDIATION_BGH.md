# KẾ HOẠCH KHẮC PHỤC VÀ HOÀN THIỆN VAI TRÒ BAN GIÁM HIỆU (BGH)

> **Tài liệu tham chiếu gốc:** [LMS_3_ROLE_REMEDIATION_PLAN.md](file:///D:/A/Du-An-Tot-Nghiep/docs/00-project/LMS_3_ROLE_REMEDIATION_PLAN.md)  
> **Phạm vi vai trò:** Ban Giám Hiệu (`Principal`)  
> **Nguyên tắc:** Tập trung quản lý nhân sự giảng viên theo cơ sở, hồ sơ năng lực chuyên môn, tải giảng dạy, nhật ký ca dạy thực tế và cây phân quyền phạm vi; loại bỏ toàn bộ dữ liệu giả lập.

---

## 1. Hiện Trạng và Các Vấn Đề Cần Khắc Phục Ở Vai Trò BGH

| STT | Khu vực / Tệp hiện tại | Hiện trạng trong code | Vấn đề cần khắc phục |
|---|---|---|---|
| 1 | `frontend/src/views/BGH/UsersView.vue` | Đang là màn hình Quản lý người dùng chung; chỉ cho phép `SuperAdmin/Admin` sửa (`canEdit`). Nút "Nhập Excel" dùng `setTimeout(1000)` rồi báo thành công giả. | **P0:** Xóa bỏ fake success tại nút nhập Excel (disable hoặc hiển thị thông báo chưa hỗ trợ). Không để BGH dùng màn quản lý người dùng chung này để quản lý GV. |
| 2 | `frontend/src/views/BGH/RolesView.vue` | Hiển thị bảng vai trò dạng phẳng, chỉ đọc; route `/bgh/roles` chưa được đưa vào menu BGH trực quan. | **P1.2:** Chuyển đổi hoặc bổ sung giao diện cây phạm vi quản lý `Đơn vị -> Vai trò -> Người dùng` phản ánh đúng dữ liệu phân quyền thực tế. |
| 3 | `Backend/Controllers/BghFacadeController.cs`, `BghAcademicController.cs` | Đã có API lấy danh sách người dùng theo cơ sở (`GET /api/bgh/users`), thống kê học vụ, cảnh báo sinh viên. | **P1.1:** Chưa có API chuyên biệt về **Hồ sơ nhân sự giảng viên** tổng hợp: học vị, bằng cấp, môn được dạy, mức độ phù hợp, tải giảng dạy, nguyện vọng và nhật ký ca dạy. |
| 4 | Audit Log (`Backend/Controllers/AuditLogsController.cs`) | Đã có hệ thống ghi log kiểm toán chung trong backend. | **P1.1:** Bắt buộc mọi thao tác BGH tạo/sửa/khóa giảng viên, phân công chuyên môn phải ghi Audit Log kèm lý do và chi tiết giá trị thay đổi. |

---

## 2. Danh Sách Nhiệm Vụ Chi Tiết Cho Vai Trò BGH

### Giai đoạn P0: Dọn Sai Lệch và Khóa Claim Sai

- [ ] **Nhiệm vụ P0.1 - Xử lý nút Nhập Excel giả trong `UsersView.vue`:**
  - *Tệp tác động:* `frontend/src/views/BGH/UsersView.vue`
  - *Yêu cầu:* Xóa bỏ toàn bộ đoạn code `setTimeout(..., 1000)` và thông báo `Đã nhập thành công danh sách người dùng từ file...`.
  - *Giải pháp:* Thay thế bằng trạng thái vô hiệu hóa (disabled button) kèm tooltip thông báo rõ: `"Chức năng nhập file Excel đang được cập nhật ở phiên bản sau"`, hoặc thông báo modal rõ ràng. Tuyệt đối không để xảy ra tình trạng backend không nhận file mà giao diện báo thành công.
- [ ] **Nhiệm vụ P0.2 - Chuẩn hóa phạm vi dữ liệu Demo của BGH:**
  - Đảm bảo tài khoản BGH demo (thuộc một `MaDonVi` cụ thể, ví dụ cơ sở 1) khi đăng nhập chỉ thấy giảng viên, lớp học và dữ liệu thuộc cơ sở của mình.

---

### Giai đoạn P1: Xây Dựng Chức Năng Cốt Lõi

#### P1.1. Xây Dựng Phân Hệ Quản Lý Nhân Sự Giảng Viên (`Teacher Personnel Management`)

Tách riêng một module chuyên dụng cho BGH: **"Quản lý Nhân sự Giảng viên"** thay vì sử dụng chung màn hình người dùng.

##### 1. Backend (API & Nghiệp vụ):
- **Tạo mới Entity / Model & DTOs:**
  - `Backend/DTOs/TeacherPersonnel/TeacherPersonnelListDto.cs`: Thông tin cơ bản, mã GV, học vị, chuyên ngành chính, số môn được phép dạy, số lớp đang dạy kỳ này, trạng thái.
  - `Backend/DTOs/TeacherPersonnel/TeacherPersonnelDetailDto.cs`: Hồ sơ chi tiết (học vị, chứng chỉ minh chứng, chuyên ngành chính/phụ, danh sách môn được dạy kèm mức độ phù hợp, số năm kinh nghiệm, số lần đã dạy).
  - `Backend/DTOs/TeacherPersonnel/TeacherWorkloadSummaryDto.cs`: Tải giảng dạy trong học kỳ (tổng số lớp `KhoaHoc`, số ca/tuần, số giờ chuẩn quy đổi).
  - `Backend/DTOs/TeacherPersonnel/TeacherSessionLogDto.cs`: Nhật ký ca dạy thật (lấy từ `KhoaHoc` + `ThoiKhoaBieu` + `BuoiHoc`, phân biệt GV chính/GV dạy thay, trạng thái buổi học, thời điểm mở/gửi điểm danh, điểm danh đúng hạn hay trễ).
  - `Backend/DTOs/TeacherPersonnel/TeacherEvaluationSummaryDto.cs`: Kết quả đánh giá từ sinh viên (điểm trung bình, cỡ mẫu số lượng đánh giá, học kỳ).
  - `Backend/DTOs/TeacherPersonnel/UpdateTeacherPersonnelRequestDto.cs`: Cập nhật thông tin giảng viên.
- **Tạo mới Controller & Service:**
  - `Backend/Controllers/BghTeacherPersonnelController.cs`:
    - `GET /api/bgh/teacher-personnel`: Danh sách GV có phân trang, lọc theo đơn vị/khoa, chuyên ngành, môn có thể dạy, trạng thái, học kỳ. (Bắt buộc scope theo `MaDonVi` của BGH).
    - `GET /api/bgh/teacher-personnel/{id}`: Chi tiết hồ sơ nhân sự, năng lực chuyên môn và chứng chỉ.
    - `GET /api/bgh/teacher-personnel/{id}/workload`: Thống kê tải giảng dạy theo học kỳ.
    - `GET /api/bgh/teacher-personnel/{id}/session-logs`: Nhật ký các ca dạy thật trong học kỳ.
    - `GET /api/bgh/teacher-personnel/{id}/evaluations`: Thống kê đánh giá của sinh viên (kèm cỡ mẫu).
    - `POST /api/bgh/teacher-personnel`: Tạo mới tài khoản giảng viên trong cơ sở (chặn gán quyền `SuperAdmin/Admin/Principal`).
    - `PUT /api/bgh/teacher-personnel/{id}`: Cập nhật thông tin chuyên môn, trạng thái giảng viên.
    - `POST /api/bgh/teacher-personnel/{id}/toggle-lock`: Khóa / Mở khóa giảng viên trong cơ sở.
  - `Backend/Services/TeacherPersonnel/TeacherPersonnelService.cs`: Thực hiện business logic, kiểm tra scope cơ sở, tính toán thống kê ca dạy và ghi `AuditLog`.
- **Ràng buộc bảo mật & Audit:**
  - BGH chỉ có quyền sửa/khóa GV thuộc cơ sở của mình.
  - Mọi thao tác ghi phải tạo một bản ghi `AuditLog` với `NguoiThucHien = CurrentUserId`, `HanhDong`, `GiaTriCu`, `GiaTriMoi`, `LyDo`.

##### 2. Frontend (Giao diện & Tích hợp):
- **Tạo Service API mới:**
  - `frontend/src/services/bghPersonnelApi.js`: Kết nối toàn bộ các endpoint nhân sự nêu trên.
- **Tạo các Views & Components:**
  - `frontend/src/views/BGH/HumanResources/TeacherPersonnelListView.vue`: Bảng danh sách GV với bộ lọc nâng cao (khoa, môn dạy, trạng thái, học kỳ), thống kê nhanh số lượng GV cơ sở.
  - `frontend/src/views/BGH/HumanResources/TeacherPersonnelDetailView.vue`: Chi tiết hồ sơ đa tab:
    - *Tab 1 - Hồ sơ & Chuyên môn:* Thông tin học vị, chứng chỉ, chuyên ngành, môn được phép dạy (`GiaoVienMonHoc`), mức độ phù hợp, số năm kinh nghiệm.
    - *Tab 2 - Tải giảng dạy & Lịch:* Số lớp phụ trách, số ca/tuần, biểu đồ phân bổ thời gian.
    - *Tab 3 - Nhật ký ca dạy:* Danh sách ca dạy thực tế, trạng thái buổi học, tình trạng gửi điểm danh đúng hạn/trễ.
    - *Tab 4 - Đánh giá sinh viên:* Điểm đánh giá trung bình kèm số lượt đánh giá thực tế và phân bổ điểm theo kỳ.
    - *Tab 5 - Lịch sử thay đổi (Audit):* Lịch sử cập nhật hồ sơ của giảng viên.
  - `frontend/src/views/BGH/HumanResources/TeacherPersonnelModal.vue`: Modal thêm mới / chỉnh sửa giảng viên.
- **Cập nhật Menu & Router:**
  - Thêm menu item **"Nhân sự Giảng viên"** vào `frontend/src/components/BGH/data/menuData.js`.
  - Đăng ký route `/bgh/human-resources` và `/bgh/human-resources/:id` trong `frontend/src/router/index.js`.

---

#### P1.2. Cây Phạm Vi Quản Lý và Phân Quyền (`Scope & Hierarchy Tree`)

- **Backend:**
  - Sử dụng API cấu trúc tổ chức và vai trò hiện có hoặc bổ sung endpoint cây phân cấp:
    - `GET /api/bgh/organization-tree/hierarchy`: Trả về cây thực tế `Đơn vị/Cơ sở -> Vai trò -> Người dùng (Vai trò chính & Vai trò phụ)`.
- **Frontend:**
  - Nâng cấp `frontend/src/views/BGH/RolesView.vue` (hoặc tạo `frontend/src/views/BGH/OrganizationHierarchyView.vue`):
    - Hiển thị cấu trúc cây quản trị thực tế của cơ sở.
    - Khi bấm vào từng node, hiển thị danh sách người dùng thuộc node đó.
    - BGH chỉ có thể click quản lý trên các node Giảng viên thuộc quyền.
    - **Lưu ý:** Tuyệt đối không vẽ checkbox phân quyền từng action giả nếu backend chưa có mô hình permission chi tiết.

---

## 3. Danh Mục Tệp Liên Quan Trực Tiếp Tới Vai Trò BGH

### 3.1. Các Tệp Tạo Mới (NEW)
1. `Backend/DTOs/TeacherPersonnel/TeacherPersonnelListDto.cs`
2. `Backend/DTOs/TeacherPersonnel/TeacherPersonnelDetailDto.cs`
3. `Backend/DTOs/TeacherPersonnel/TeacherWorkloadSummaryDto.cs`
4. `Backend/DTOs/TeacherPersonnel/TeacherSessionLogDto.cs`
5. `Backend/DTOs/TeacherPersonnel/TeacherEvaluationSummaryDto.cs`
6. `Backend/DTOs/TeacherPersonnel/UpdateTeacherPersonnelRequestDto.cs`
7. `Backend/Services/TeacherPersonnel/ITeacherPersonnelService.cs`
8. `Backend/Services/TeacherPersonnel/TeacherPersonnelService.cs`
9. `Backend/Controllers/BghTeacherPersonnelController.cs`
10. `frontend/src/services/bghPersonnelApi.js`
11. `frontend/src/views/BGH/HumanResources/TeacherPersonnelListView.vue`
12. `frontend/src/views/BGH/HumanResources/TeacherPersonnelDetailView.vue`
13. `frontend/src/views/BGH/HumanResources/TeacherPersonnelModal.vue`

### 3.2. Các Tệp Chỉnh Sửa (MODIFY)
1. `frontend/src/views/BGH/UsersView.vue` (Xóa fake excel, điều chỉnh phân quyền xem)
2. `frontend/src/views/BGH/RolesView.vue` (Cải tiến hiển thị cây phân quyền phạm vi)
3. `frontend/src/components/BGH/data/menuData.js` (Thêm mục Nhân sự Giảng viên)
4. `frontend/src/router/index.js` (Đăng ký routes Nhân sự BGH)
5. `Backend/Program.cs` (Đăng ký DI cho `ITeacherPersonnelService`)

### 3.3. Các Tệp Giữ Nguyên (DO NOT MODIFY)
- Các view quản trị học thuật đã ổn định: `AcademicTermsView.vue`, `CurriculumView.vue`, `FacilitiesView.vue`, `OrganizationsView.vue`.
- Layout BGH và hệ thống token CSS.

---

## 4. Kế Hoạch Kiểm Thử và Nghiệm Thu (Evidence & Verification Plan)

Để nghiệm thu vai trò BGH, bắt buộc phải có đầy đủ bộ bằng chứng sau:

1. **Bằng chứng P0:** Ảnh chụp màn hình màn hình `UsersView.vue` không còn nút giả lập `setTimeout` import Excel.
2. **Bằng chứng P1.1 (Danh sách & Lọc):** Ảnh chụp màn hình danh sách GV tại cơ sở của BGH kèm request/response JSON lọc theo môn dạy và chuyên ngành.
3. **Bằng chứng P1.1 (Hồ sơ & Nhật ký ca dạy):** Ảnh chụp màn hình chi tiết một giảng viên thể hiện đầy đủ:
   - Danh sách môn được phép dạy từ `GiaoVienMonHoc`.
   - Nhật ký ca dạy thật từ `KhoaHoc`/`BuoiHoc` thể hiện đúng trạng thái điểm danh và ca dạy thay/chính.
   - Thống kê đánh giá sinh viên kèm cỡ mẫu.
4. **Bằng chứng P1.1 (Audit Log):** Thao tác chỉnh sửa thông tin giảng viên -> Query SQL bảng `NhatKyKiemToan` / `AuditLog` chứng minh bản ghi audit được tạo với đúng `GiaTriCu` và `GiaTriMoi`.
5. **Bằng chứng P1.2 (Cây phân quyền):** Ảnh chụp màn hình cây cơ sở -> vai trò -> người dùng phản ánh đúng dữ liệu DB thật.

> [!NOTE]
> **Lưu ý về môi trường:** Mọi truy vấn SQL kiểm tra đối chiếu dữ liệu BGH (Audit log, HoSoChuyenMon, BuoiHoc) đều thực hiện trực tiếp trên container Docker `sqlserver` (Port 1433, Database `LMS`).
