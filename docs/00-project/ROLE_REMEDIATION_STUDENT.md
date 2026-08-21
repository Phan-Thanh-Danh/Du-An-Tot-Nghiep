# KẾ HOẠCH KHẮC PHỤC VÀ HOÀN THIỆN VAI TRÒ SINH VIÊN (STUDENT)

> **Tài liệu tham chiếu gốc:** [LMS_3_ROLE_REMEDIATION_PLAN.md](file:///D:/A/Du-An-Tot-Nghiep/docs/00-project/LMS_3_ROLE_REMEDIATION_PLAN.md)  
> **Phạm vi vai trò:** Sinh Viên (`Student`)  
> **Nguyên tắc:** Đảm bảo luồng nộp bài tập file thật với mã băm đối chiếu; xem lịch học công bố nhất quán; thi trực tuyến an toàn chịu tải đồng thời (Concurrency-safe); xem điểm và đánh giá giảng viên thật.

---

## 1. Hiện Trạng và Các Vấn Đề Cần Khắc Phục Ở Vai Trò Sinh Viên

| STT | Khu vực / Tệp hiện tại | Hiện trạng trong code | Vấn đề cần khắc phục |
|---|---|---|---|
| 1 | `frontend/src/views/Student/AssignmentDetailView.vue`, `StudentAssignmentsController.cs` | Đã có API upload file lên R2/Local Storage và lưu bản ghi `BaiNop`. | **P1.1:** Hoàn thiện luồng nộp file thật: ràng buộc dung lượng tối thiểu/tối đa, định dạng cho phép, ghi đè lần nộp, lưu trữ bền vững; refresh trang vẫn giữ trạng thái đã nộp; chuẩn bị bài test đối chiếu mã băm SHA-256 với Giảng viên. |
| 2 | `frontend/src/views/Student/ScheduleView.vue`, `StudentScheduleController.cs` | Sinh viên xem lịch học theo ngày/tuần/tháng/học kỳ. | **P1.2:** Đảm bảo dữ liệu thời khóa biểu lấy từ các lớp học phần sinh viên đã đăng ký; đồng bộ 100% về thời gian, phòng học, giảng viên với màn hình Giảng viên và BGH. |
| 3 | `Backend/Models/PhienThiHocSinh.cs`, `QuizAttemptService.cs`, `frontend/src/views/Student/ExamTakeView.vue` | Đã có quy trình bắt đầu thi, tự động lưu câu trả lời (autosave), nộp bài (submit) và chấm điểm tự động. | **P1.3 & P2.1:** **Rủi ro tranh chấp đồng thời (Concurrency):** `PhienThiHocSinh` chưa có cơ chế kiểm soát đồng thời lạc quan (Optimistic Concurrency/RowVersion). Autosave có nguy cơ bị gói tin mạng cũ ghi đè câu trả lời mới. Nộp bài nhiều lần do giật lag có thể gây lỗi DB. |
| 4 | `frontend/src/views/Student/GradesView.vue`, `ExamResultView.vue` | Xem điểm số bài thi và điểm tổng kết học phần. | **P1.4:** Hiển thị điểm thành phần, điểm thi cuối kỳ và kết quả Đạt/Rớt dựa trên cấu hình ngưỡng đạt (`PassFailRule`) của môn học. |
| 5 | `frontend/src/views/Student/EvaluationsView.vue`, `StudentEvaluationsController.cs` | Đánh giá giảng viên cuối học kỳ. | **P1.5:** Lấy đúng danh sách câu hỏi khảo sát từ hệ thống (`MauDanhGia`), ghi nhận đánh giá của sinh viên vào cơ sở dữ liệu để tổng hợp cho BGH xem. |

---

## 2. Danh Sách Nhiệm Vụ Chi Tiết Cho Vai Trò Sinh Viên

### Giai đoạn P1: Hoàn Thiện Luồng Nghiệp Vụ Cốt Lõi

#### P1.1. Nộp Bài Tập File Thật & Xác Thực Toàn Vẹn Dữ Liệu (`Real Assignment Submission & Hash Verification`)
- **Tệp tác động:** `frontend/src/views/Student/AssignmentDetailView.vue`, `Backend/Controllers/StudentAssignmentsController.cs`.
- **Yêu cầu nghiệp vụ & Kỹ thuật:**
  - Sinh viên chọn tệp (ví dụ: `.pdf`, `.zip`, `.docx`) để nộp cho bài tập.
  - Client & Server validate:
    - Định dạng tệp cho phép theo cấu hình bài tập.
    - Dung lượng tối thiểu (mặc định ≥ 10 KB để tránh nộp file rỗng) và tối đa (mặc định ≤ 50 MB).
  - Backend lưu trữ tệp vào Storage (R2 hoặc Local Storage bền vững), tạo/cập nhật bản ghi `BaiNop` với đường dẫn `UrlTapTin`, thời gian nộp `ThoiDiemNop`, và cờ nộp trễ `NopTre`.
  - Quản lý số lần nộp: Cho phép nộp lại (ghi đè hoặc tăng số lần nộp) nếu bài tập còn hạn và chưa vượt quá số lần nộp tối đa.
  - Khi refresh trình duyệt: Giao diện hiển thị ngay trạng thái *"Đã nộp bài"*, tên file đã nộp, thời gian nộp và nút tải xuống để kiểm tra lại bài của chính mình.
  - **Kịch bản nghiệm thu đối chiếu SHA-256:**
    1. Sinh viên chuẩn bị file `BaoCao_Assignment1.pdf` có mã SHA-256 xác định trước.
    2. Sinh viên tải file lên qua giao diện.
    3. Giảng viên mở bài nộp của sinh viên này và tải file về máy.
    4. Đo mã băm SHA-256 của file tải về. Hai mã băm phải trùng khớp tuyệt đối.

---

#### P1.2. Xem Lịch Học Đã Công Bố Nhất Quán (`Student Schedule`)
- **Tệp tác động:** `frontend/src/views/Student/ScheduleView.vue`, `Backend/Controllers/StudentScheduleController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Hiển thị đầy đủ các buổi học thuộc các lớp học phần sinh viên đang theo học.
  - Thông tin mỗi buổi: Môn học, Giảng viên phụ trách, Ca học (giờ bắt đầu - giờ kết thúc), Phòng học, Tình trạng buổi học (Bình thường, Đã hủy, Đổi phòng, Học bù).
  - Trạng thái điểm danh cá nhân của sinh viên trong từng buổi học (Có mặt, Đi trễ, Vắng).

---

#### P1.3. Thi Trực Tuyến & An Toàn Tranh Chấp Đồng Thời (`Exam Taking & Concurrency Safety`)
- **Tệp tác động:**
  - `Backend/Models/PhienThiHocSinh.cs`
  - `Backend/Services/QuizAttempts/QuizAttemptService.cs`
  - `Backend/Controllers/ExamController.cs`, `QuizAttemptsController.cs`
  - `frontend/src/views/Student/ExamTakeView.vue`
  - `frontend/src/services/examApi.js`
- **Yêu cầu kỹ thuật chống lỗi đồng thời (P2.1 Concurrency Safeguards):**
  - **1. Optimistic Concurrency Control:**
    - Bổ sung trường `Timestamp` / `RowVersion` hoặc `NgayCapNhat` trên Entity `PhienThiHocSinh`.
    - Khi cập nhật câu trả lời, câu lệnh UPDATE phải kiểm tra điều kiện phiên thi còn hợp lệ và chưa bị kết thúc.
  - **2. Versioned Autosave (Tự động lưu có phiên bản):**
    - Mỗi request autosave từ client gửi kèm số thứ tự phiên bản (`versionNumber`) hoặc timestamp client.
    - Server chỉ chấp nhận ghi đè câu trả lời nếu phiên bản của request lớn hơn phiên bản hiện tại trong DB; bỏ qua các gói tin cũ đến trễ do độ trễ mạng.
  - **3. Idempotent Submit (Nộp bài chống trùng lặp):**
    - Khi sinh viên bấm "Nộp bài", client vô hiệu hóa nút nộp để tránh double-click.
    - Server xử lý nộp bài trong Transaction: nếu phiên thi đã ở trạng thái `da_nop`, trả về kết quả đã chấm trước đó thay vì thực hiện chấm lại lần 2 hoặc báo lỗi 500.
  - **4. Start đồng thời an toàn:**
    - Khi nhiều sinh viên cùng bấm "Bắt đầu làm bài" tại thời điểm mở đề, server kiểm tra xem sinh viên đã có phiên thi đang mở hay chưa. Nếu đã có, trả về phiên thi hiện tại kèm snapshot đề thi (`DeThiSnapshotJson`), không tạo trùng bản ghi gây lỗi Unique Constraint.
  - **5. Chấm điểm trắc nghiệm tin cậy:**
    - Chấm điểm ngay khi nộp cho các câu trắc nghiệm dựa trên snapshot đáp án; ghi nhận điểm số rõ ràng, xử lý ngoại lệ an toàn có retry log nếu xảy ra lỗi I/O.

---

#### P1.4. Tra Cứu Kết Quả Thi và Bảng Điểm (`Grades & Transcripts`)
- **Tệp tác động:** `frontend/src/views/Student/GradesView.vue`, `frontend/src/views/Student/ExamResultView.vue`, `Backend/Controllers/StudentGradesController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Xem kết quả bài thi: Điểm số, số câu đúng/sai (nếu đề thi cho phép công bố đáp án).
  - Xem bảng điểm tổng kết môn học: Điểm chuyên cần, Điểm bài tập, Điểm giữa kỳ, Điểm thi cuối kỳ, Điểm GPA môn học.
  - Kết quả Đạt / Rớt thể hiện rõ ràng theo đúng cấu hình ngưỡng điểm đạt (`NguongDat`) của môn học.

---

#### P1.5. Đánh Giá Giảng Viên (`Teacher Evaluation Survey`)
- **Tệp tác động:** `frontend/src/views/Student/EvaluationsView.vue`, `Backend/Controllers/StudentEvaluationsController.cs`.
- **Yêu cầu nghiệp vụ:**
  - Lấy danh sách các lớp học phần và giảng viên cần đánh giá trong học kỳ.
  - Hiển thị bộ câu hỏi khảo sát từ mẫu đánh giá chuẩn (`MauDanhGia`).
  - Gửi đánh giá ẩn danh (lưu điểm đánh giá và nhận xét vào `DanhGiaGiaoVien`, không để lộ danh tính sinh viên khi BGH hoặc Giảng viên xem kết quả tổng hợp).

---

## 3. Danh Mục Tệp Liên Quan Trực Tiếp Tới Vai Trò Sinh Viên

### 3.1. Các Tệp Chỉnh Sửa Chính (MODIFY)
1. `Backend/Models/PhienThiHocSinh.cs` (Bổ sung kiểm soát Concurrency / RowVersion nếu cần)
2. `Backend/Services/QuizAttempts/QuizAttemptService.cs` (Hoàn thiện logic start an toàn, versioned autosave, idempotent submit)
3. `Backend/Controllers/StudentAssignmentsController.cs` (Kiểm tra validate dung lượng, loại file, lưu storage thật)
4. `frontend/src/views/Student/AssignmentDetailView.vue` (UI nộp file thật, hiển thị chi tiết bài nộp, hỗ trợ tải lại file đã nộp)
5. `frontend/src/views/Student/ExamTakeView.vue` (Client autosave có timestamp, chặn double-submit, xử lý mất mạng/reconnect)
6. `frontend/src/views/Student/ScheduleView.vue` (Xem lịch học công bố đồng bộ)
7. `frontend/src/views/Student/GradesView.vue` (Bảng điểm và kết quả đạt/rớt)
8. `frontend/src/views/Student/EvaluationsView.vue` (Khảo sát đánh giá giảng viên)

### 3.2. Các Tệp Giữ Nguyên (DO NOT MODIFY)
- Giao diện đăng ký môn học (`RegistrationsView.vue`), học phí (`TuitionView.vue`) nếu không có lỗi phát sinh.
- Layout Sinh viên và hệ thống Design Tokens.

---

## 4. Kế Hoạch Kiểm Thử và Nghiệm Thu (Evidence & Verification Plan)

Để nghiệm thu vai trò Sinh Viên, bắt buộc phải có đầy đủ bộ bằng chứng sau:

1. **Bằng chứng P1.1 (Nộp file bài tập & Khớp mã SHA-256):**
   - Sinh viên chọn file PDF mẫu (ví dụ: `Demo_Submission.pdf`).
   - Network tab ghi nhận request `POST /api/student/assignments/{id}/submit` trả về 200 OK.
   - F5 tải lại trang: Giao diện vẫn hiển thị trạng thái đã nộp và đường dẫn tải file.
   - Giảng viên tải file về -> Chạy lệnh băm SHA-256 -> Ảnh chụp màn hình hai mã băm SHA-256 khớp từng ký tự.
2. **Bằng chứng P1.2 (Lịch học đồng bộ):**
   - Ảnh chụp màn hình lịch học của sinh viên hiển thị đúng ca học, phòng học, môn học và giảng viên phụ trách.
3. **Bằng chứng P1.3 (Thi trực tuyến an toàn đồng thời):**
   - Sinh viên làm bài thi -> Tự động lưu đáp án sau mỗi câu chọn -> Bấm nộp bài thành công.
   - Thực hiện test nộp bài lặp lại (idempotent submit) và test autosave trễ -> Xác nhận hệ thống không bị ghi đè dữ liệu sai lệch.
4. **Bằng chứng P1.4 (Xem kết quả & Điểm):**
   - Ảnh chụp màn hình trang xem kết quả thi và bảng điểm chi tiết.
5. **Bằng chứng P1.5 (Đánh giá giảng viên):**
   - Sinh viên gửi khảo sát đánh giá giảng viên -> Query SQL xác nhận bản ghi được lưu vào bảng `DanhGiaGiaoVien`.

> [!NOTE]
> **Lưu ý về môi trường:** Mọi truy vấn SQL kiểm tra đối chiếu dữ liệu Sinh viên (Phiên thi, Bài nộp, Điểm số, Đánh giá GV) đều thực hiện trực tiếp trên container Docker `sqlserver` (Port 1433, Database `LMS`).
