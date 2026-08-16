# KẾ HOẠCH KHẮC PHỤC VÀ HOÀN THIỆN VAI TRÒ GIẢNG VIÊN (TEACHER)

> **Tài liệu tham chiếu gốc:** [LMS_3_ROLE_REMEDIATION_PLAN.md](file:///D:/A/Du-An-Tot-Nghiep/docs/00-project/LMS_3_ROLE_REMEDIATION_PLAN.md)  
> **Phạm vi vai trò:** Giảng Viên (`Teacher`)  
> **Nguyên tắc:** Làm sạch dữ liệu coi thi (loại bỏ mọi suy diễn mock); đảm bảo luồng lịch dạy - quản lý lớp - điểm danh thật - tải file bài tập thật để chấm - coi thi qua SignalR thật.

---

## 1. Hiện Trạng và Các Vấn Đề Cần Khắc Phục Ở Vai Trò Giảng Viên

| STT | Khu vực / Tệp hiện tại | Hiện trạng trong code | Vấn đề cần khắc phục |
|---|---|---|---|
| 1 | `frontend/src/services/teacherApi.js` (hàm `getExamStudents`) | Đang tự gán mặc định `attendanceStatus: 'present'`, tự gán `preflightStatus: 'pass'`, tự suy diễn `streamStatus: 'streaming'` và trả về mảng `logs: []` rỗng giả. | **P0:** Xóa bỏ toàn bộ dữ liệu suy diễn; trạng thái điểm danh và video stream của thí sinh phải đến từ API điểm danh ca thi và SignalR Hub thật. |
| 2 | `frontend/src/views/GiangVien/ProctoringDashboardView.vue`, `ProctoringAttendanceView.vue` | Giao diện giám sát thi hiển thị danh sách thí sinh và log sự kiện. | **P0 / P1.4:** Kết nối trực tiếp với `examProctoringHub.js` và API vi phạm thật (`NhatKyViPhamThi`); không hiển thị log giả lập nếu chưa có sự kiện. |
| 3 | `frontend/src/views/GiangVien/AssignmentSubmissionsView.vue`, `TeacherSubmissionsController.cs` | Giảng viên xem danh sách bài nộp của sinh viên và chấm điểm. | **P1.3:** Đảm bảo nút tải bài nộp tải đúng file vật lý từ storage thật; chứng minh file tải về có mã băm SHA-256 trùng khớp 100% với file sinh viên nộp. |
| 4 | `frontend/src/views/GiangVien/TeachingScheduleView.vue`, `TeacherScheduleController.cs` | Hiển thị thời khóa biểu giảng dạy theo tuần/tháng/học kỳ. | **P1.1:** Xác minh tính nhất quán của dữ liệu: lịch dạy của GV phải trùng khớp 100% với lịch học của Sinh viên và nhật ký ca dạy ở BGH. |
| 5 | `frontend/src/views/GiangVien/ClassAttendanceView.vue`, `AttendanceHistoryView.vue` | Điểm danh lớp học và lịch sử điểm danh. | **P1.2:** Ghi nhận chính xác thời điểm mở/gửi/khóa điểm danh theo chính sách `AttendancePolicy`; phân biệt ca dạy chính và ca dạy thay. |

---

## 2. Danh Sách Nhiệm Vụ Chi Tiết Cho Vai Trò Giảng Viên

### Giai đoạn P0: Dọn Sai Lệch và Khóa Claim Suy Diễn

- [ ] **Nhiệm vụ P0.1 - Làm sạch hàm `getExamStudents` trong `teacherApi.js`:**
  - *Tệp tác động:* `frontend/src/services/teacherApi.js`
  - *Yêu cầu:*
    - Xóa bỏ dòng `attendanceStatus: 'present'` (không được mặc định thí sinh có mặt khi chưa điểm danh).
    - Xóa bỏ việc tự gán `streamStatus: 'streaming'` khi chưa có kết nối WebRTC/SignalR xác nhận.
    - Xóa bỏ việc trả về `logs: []` giả lập mà phải lấy từ danh sách sự kiện/vi phạm thật (`/api/exam/ca-thi/{examId}/vi-pham`).
    - Trạng thái thí sinh phải phản ánh chính xác trường `TrangThaiDuThi` từ DB (`chua_thi`, `dang_thi`, `da_nop`, `dinh_chi`, `vang_thi`).
- [ ] **Nhiệm vụ P0.2 - Làm sạch giao diện Coi thi (`ProctoringDashboardView.vue`):**
  - Đảm bảo thẻ thông tin thí sinh chỉ hiển thị icon camera đang phát khi SignalR Hub nhận được luồng stream thật của thí sinh đó.
  - Khi không có log vi phạm nào, hiển thị trạng thái trống (Empty State) trung thực: *"Chưa có sự kiện hoặc vi phạm nào được ghi nhận"* thay vì hiển thị dữ liệu giả.

---

### Giai đoạn P1: Hoàn Thiện Luồng Cốt Lõi

#### P1.1. Lịch Dạy Cá Nhân Nhất Quán (`Teaching Schedule`)
- **Tệp tác động:** `frontend/src/views/GiangVien/TeachingScheduleView.vue`, `Backend/Controllers/TeacherScheduleController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Giảng viên xem lịch dạy đã công bố theo ngày, tuần, tháng, học kỳ.
  - Mỗi ca dạy hiển thị rõ: Tên môn, Lớp học phần (`KhoaHoc`), Ca học (`CaHoc`), Phòng học (`PhongHoc`), Hình thức (Trực tiếp/Online), Trạng thái buổi học (Chưa diễn ra, Đang diễn ra, Đã hoàn thành, Đã hủy, Dời lịch).
  - Phân biệt rõ ca dạy chính thức và ca được phân công **dạy thay**.
  - Không sửa đổi hoặc can thiệp vào thuật toán xếp lịch tự động.

#### P1.2. Quản Lý Lớp Học & Điểm Danh Thực Tế (`Class & Attendance Management`)
- **Tệp tác động:** `frontend/src/views/GiangVien/ClassAttendanceView.vue`, `frontend/src/views/GiangVien/AttendanceHistoryView.vue`, `Backend/Controllers/TeacherClassesController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Mở ca điểm danh khi buổi học bắt đầu.
  - Điểm danh từng sinh viên với các trạng thái: Có mặt (`CoMat`), Đi trễ (`DiTre`), Vắng có phép (`VangCoPhep`), Vắng không phép (`VangKhongPhep`).
  - Gửi điểm danh và khóa điểm danh: Tuân thủ hạn giờ gửi điểm danh từ `AttendancePolicy` của hệ thống.
  - Lịch sử điểm danh: Xem lại toàn bộ các buổi đã điểm danh kèm thời gian gửi thực tế để BGH kiểm tra đúng hạn hay trễ hạn.

#### P1.3. Chấm Bài Tập & Tải File Nộp Thật (`Assignment Grading & File Downloads`)
- **Tệp tác động:** `frontend/src/views/GiangVien/AssignmentSubmissionsView.vue`, `Backend/Controllers/TeacherSubmissionsController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Giảng viên mở danh sách sinh viên nộp bài tập của một lớp học phần.
  - Nút **"Tải bài nộp"**: Gọi API lấy URL tải file thật từ Storage (R2/Local) và kích hoạt tải về trình duyệt.
  - Khả năng chấm điểm: Nhập điểm số (0 - 10), nhập nhận xét chi tiết, công bố hoặc lưu nháp điểm.
  - **Quy trình nghiệm thu:** Tải file `.pdf` bài nộp của sinh viên -> Đo mã băm SHA-256 của file tải về so với file gốc sinh viên tải lên -> Kết quả băm phải khớp 100%.

#### P1.4. Coi Thi, Ghi Nhận Vi Phạm & Lập Biên Bản Thi (`Proctoring & Incident Logging`)
- **Tệp tác động:** `frontend/src/views/GiangVien/ProctoringDashboardView.vue`, `frontend/src/views/GiangVien/ProctoringReportView.vue`, `frontend/src/services/examProctoringHub.js`, `Backend/Controllers/ExamController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Giảng viên vào ca thi được phân công coi thi (`CaThi`).
  - Điểm danh thí sinh vào phòng thi (`POST /api/exam/ca-thi/diem-danh`).
  - Giám sát thí sinh trong thời gian làm bài qua SignalR Presence: phát hiện thí sinh rời màn hình, ngắt kết nối hoặc có hành vi bất thường.
  - Ghi nhận vi phạm thật (`POST /api/exam/vi-pham`): Chọn thí sinh, chọn loại vi phạm (Nhìn tài liệu, Rời màn hình, Nhờ người thi hộ), mức độ (Nhắc nhở, Cảnh cáo, Đình chỉ), lưu vào bảng `NhatKyViPhamThi`.
  - Kết thúc ca thi & Lập biên bản ca thi (`POST /api/exam/bien-ban`): Tổng kết số lượng thí sinh dự thi, vắng thi, số biên bản vi phạm, lưu vào bảng `BienBanThi`.

#### P1.5. Xem Hồ Sơ Chuyên Môn & Gửi Nguyện Vọng Giảng Dạy (`Teacher Profile & Preferences`)
- **Tệp tác động:** `frontend/src/views/GiangVien/ProfileView.vue`, `frontend/src/views/GiangVien/TeachingPreferencesView.vue`, `Backend/Controllers/TeacherTeachingPreferencesController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Giảng viên xem hồ sơ năng lực của bản thân (học vị, các môn được phép giảng dạy, chứng chỉ).
  - Đăng ký nguyện vọng giảng dạy cho học kỳ tới (ca dạy ưu tiên, ngày dạy ưu tiên, số giờ mong muốn).

---

## 3. Danh Mục Tệp Liên Quan Trực Tiếp Tới Vai Trò Giảng Viên

### 3.1. Các Tệp Chỉnh Sửa Chính (MODIFY)
1. `frontend/src/services/teacherApi.js` (Làm sạch hoàn toàn các đoạn gán giả lập ở `getExamStudents` và hàm liên quan)
2. `frontend/src/services/examProctoringHub.js` (Đảm bảo bắt sự kiện stream/presence thật từ SignalR)
3. `frontend/src/views/GiangVien/ProctoringDashboardView.vue` (Điều chỉnh UI coi thi sạch dữ liệu giả)
4. `frontend/src/views/GiangVien/ProctoringAttendanceView.vue` (Điểm danh ca thi thật)
5. `frontend/src/views/GiangVien/ProctoringReportView.vue` (Báo cáo ca thi và biên bản thật)
6. `frontend/src/views/GiangVien/AssignmentSubmissionsView.vue` (Tải file bài nộp thật và chấm điểm)
7. `frontend/src/views/GiangVien/ClassAttendanceView.vue` (Điểm danh lớp học theo policy)
8. `frontend/src/views/GiangVien/TeachingScheduleView.vue` (Xem lịch dạy công bố)
9. `Backend/Controllers/TeacherSubmissionsController.cs` (Xử lý tải file nộp và cập nhật điểm)
10. `Backend/Controllers/TeacherClassesController.cs` (Xử lý lớp và điểm danh)

### 3.2. Các Tệp Giữ Nguyên (DO NOT MODIFY)
- Các thuật toán sinh đề, chấm điểm trắc nghiệm tự động cốt lõi nếu không có lỗi.
- Layout Giảng Viên và hệ thống CSS Tokens.

---

## 4. Kế Hoạch Kiểm Thử và Nghiệm Thu (Evidence & Verification Plan)

Để nghiệm thu vai trò Giảng Viên, bắt buộc phải có đầy đủ bộ bằng chứng sau:

1. **Bằng chứng P0 (Coi thi không suy diễn):**
   - Ảnh chụp Network tab khi gọi `GET /api/exam/ca-thi/{id}/thi-sinh` và response JSON thực tế.
   - Ảnh chụp màn hình `ProctoringDashboardView.vue` thể hiện đúng trạng thái thực của thí sinh (không bị auto-present hay auto-streaming).
2. **Bằng chứng P1.1 (Lịch dạy):**
   - Ảnh chụp màn hình lịch dạy của Giảng viên khớp với lịch học của Sinh viên cùng lớp học phần.
3. **Bằng chứng P1.2 (Điểm danh lớp):**
   - Thực hiện điểm danh một buổi học -> Ảnh chụp màn hình xác nhận đã gửi điểm danh -> Query SQL kiểm tra bản ghi điểm danh và thời gian gửi.
4. **Bằng chứng P1.3 (Tải file bài nộp & Đối chiếu Hash):**
   - Giảng viên bấm nút tải bài nộp của sinh viên.
   - Thực hiện tính toán mã băm `certutil -hashfile <file_tai_ve> SHA256` trên máy và so sánh với mã băm file ban đầu sinh viên nộp. Hai mã băm phải trùng khớp hoàn toàn.
5. **Bằng chứng P1.4 (Ghi nhận vi phạm & Lập biên bản ca thi):**
   - Giảng viên lập một vi phạm cho thí sinh -> Kiểm tra API `POST /api/exam/vi-pham` thành công -> Query SQL bảng `NhatKyViPhamThi`.
   - Giảng viên tạo biên bản ca thi -> Query SQL bảng `BienBanThi`.

> [!NOTE]
> **Lưu ý về môi trường:** Mọi truy vấn SQL kiểm tra đối chiếu dữ liệu Giảng viên (Điểm danh, Biên bản thi, Vi phạm thi) đều thực hiện trực tiếp trên container Docker `sqlserver` (Port 1433, Database `LMS`).
