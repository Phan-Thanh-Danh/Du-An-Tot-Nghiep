# P17 Smart Feature Demo Guide

Tài liệu này hướng dẫn các bước demo chi tiết (Demo Script) cho 2 tính năng thông minh cốt lõi trong hệ thống (Smart Course Allocation & Smart Timetable). Mọi thao tác trong hướng dẫn này được chạy với **dữ liệu thật** từ cơ sở dữ liệu và **không sử dụng mock hay dummy**.

## Yêu cầu trước khi Demo
1. **Kiểm tra môi trường:**
   - Đảm bảo Backend và Frontend đều đang chạy.
   - Database đã được seed với `P17_DEMO_*` prefix an toàn (xem `P17_SMART_FEATURE_DEMO_SEED_PLAN.md`).
2. **Tài khoản đăng nhập:**
   - Dùng tài khoản Giáo vụ (Role: `AcademicStaff`) hoặc Quản trị (Role: `Admin`) có quyền thao tác lịch học và phân công giảng viên.
   - Ví dụ: `p17demo_giaovu@lms.local` / `Test@123`

---

## 1. Demo Smart Course Allocation (Xếp lớp & Phân công tự động)
**Mục đích:** Thể hiện khả năng hệ thống tự động gán hàng loạt giảng viên vào các lớp học (dựa trên chuyên môn, thời gian khả dụng).

**Các bước thực hiện:**
1. Đăng nhập với tài khoản Giáo vụ.
2. Từ Sidebar, chọn **Phân công GV** (TeacherAssignmentView / `GET /api/courses` & `GET /api/teachers`).
3. Trên màn hình danh sách Lớp học/Môn học cần phân công, nhấn vào nút **Phân công hàng loạt** (Bulk Assign) hoặc **Gợi ý tự động**.
4. Chọn danh sách các môn/lớp.
5. Nhấn **Thực hiện phân công**. 
6. Quan sát trạng thái loading UI hiển thị đúng (không giật lag) và hệ thống gọi API `POST /api/courses/bulk-assign` lên Backend.
7. Đọc kết quả phân công: Các giảng viên phù hợp được tự động map vào các lớp. Báo cáo tỷ lệ thành công.

---

## 2. Demo Smart Timetable (Xếp lịch thông minh)
**Mục đích:** Thể hiện khả năng hệ thống sử dụng thuật toán tối ưu (Genetic Algorithm) để sinh lịch học tự động cho một Học kỳ, giảm thiểu xung đột.

**Các bước thực hiện:**
1. Vẫn dùng tài khoản Giáo vụ, điều hướng tới **Quản lý lịch học** (ScheduleManagerView).
2. Nhấn vào nút **Xếp lịch tự động** (Generate Smart Timetable) ở góc phải. Nút này sẽ có biểu tượng Wand (đũa phép) và hiển thị trạng thái đang xử lý.
3. Hệ thống sẽ gọi API `POST /api/thoi-khoa-bieu/generate` với các tham số tối ưu (`tongTheHe`, `kichThuocQuanThe`, `tyLeCheo`, `doTuoiThoToiDa`) và `clientDraftId` do frontend sinh (`crypto.randomUUID()`).
4. Trong lúc chạy, frontend poll `GET /api/thoi-khoa-bieu/drafts/{clientDraftId}/progress` mỗi ~500ms và hiển thị modal **tiến trình GA**: số thế hệ hiện tại / tổng thế hệ, fitness tốt nhất (`bestFitness`), số buổi xếp được / không xếp được.
5. Sau khi có thông báo sinh thành công, điều hướng tới **Lịch chờ duyệt** (PendingSchedulesView).
6. Bạn sẽ thấy một "Bản nháp" mới xuất hiện. API `GET /api/thoi-khoa-bieu/drafts` sẽ lấy về danh sách này; bản nháp hiển thị thêm **score breakdown** (điểm fitness theo từng tiêu chí: giáo viên, phòng, lớp, trải ngày...).
7. (Tùy chọn) Chọn "Kiểm tra xung đột" để gọi API `POST /api/thoi-khoa-bieu/check-xung-dot-batch`, chứng minh bản nháp an toàn. Có thể chạy thêm script `docs/sql/xac-thuc-xep-lich.sql` (thay `REPLACE_WITH_DRAFT_ID`) để xác thực toàn vẹn: 0 xung đột GV/lớp/phòng, đủ buổi/tuần theo `QuyDoiTinChi`, không vượt sức chứa phòng.
8. Trong màn "Lịch chờ duyệt", chọn bản nháp đó và nhấn nút **Phê duyệt & Xuất bản**. Nút sẽ có trạng thái loading. 
9. API `POST /api/thoi-khoa-bieu/publish` được gọi, hệ thống chuyển lịch từ trạng thái nháp sang các bảng thật (`ThoiKhoaBieu`, `BuoiHoc`).
10. Quay lại màn hình **Quản lý lịch học**, các lịch học mới sẽ xuất hiện với trạng thái "Đã xuất bản".

## Ghi chú Demo GA (đã xác thực trên HK3_2026)
- Chỉ học kỳ tương lai gần nhất mới được xếp; các học kỳ đã kết thúc bị chặn.
- Nếu học kỳ đang có lịch `da_xuat_ban` (do seeder tự publish), cần set `TrangThai = 'nhap'` trước khi generate, nếu không API trả 409.
- Job generation chỉ nhận trạng thái `draft` / `da_xuat_ban` (CHECK constraint DB) — không dùng giá trị khác.
- Kết quả smoke chuẩn: 12/12 khóa xếp được, 0 xung đột GV/lớp/phòng, số buổi/tuần khớp `QuyDoiTinChi`, 0 vượt sức chứa, phân bố trải đều các ngày trong tuần.

---

## Ghi chú về Safe Seed
- Trong quá trình demo, nếu cần xóa dữ liệu demo để làm lại, chỉ xóa những bản ghi có prefix `P17_DEMO_` hoặc `DEMO_` trong Database.
- Tuyệt đối không xóa hay hardcode dữ liệu của P15.
