# Báo Cáo Tiến Độ Nhiệm Vụ Dashboard & Module Ban Giám Hiệu (BGH)

Tài liệu này tổng hợp toàn bộ danh sách các hạng mục task đã hoàn thành và chưa hoàn thành trong module BGH theo các phân tích mới nhất.

---

## 1. TỔNG QUAN TRẠNG THÁI (SUMMARY)

- **Tổng số hạng mục phân tích**: 20 mục (từ 1.1 đến 6.1)
- **Đã hoàn thành (`[x]`)**: 16 task (Sửa lỗi tiếng Việt UTF-8 CSDL, Seed nạp dữ liệu test, Redesign Giao diện UI/UX, Logic Backend TKB/Báo cáo/Khung chương trình)
- **Tạm hoãn (`[ ]`)**: 4 mục (Phân tích AI, Nhật ký kiểm toán, Cá nhân, Lịch dạy bù test seed)

---

## 2. DANH SÁCH CHI TIẾT CÁC TASK

### 1. Cơ Cấu Tổ Chức
- [x] **1.1. Quản lý đơn vị**: Đã bổ sung scope query multi-campus theo đơn vị trong `BghFacadeController` & `BghAcademicController`.
- [x] **1.2. Mục quản lý người dùng (`frontend/src/views/BGH/UsersView.vue`)**:
  - [x] Đã bỏ tự động reload khi chọn combo box (chỉ reload khi bấm nút "Lọc dữ liệu").
  - [x] Đã sửa ô tìm kiếm tên: gõ chữ không bị khựng/dừng giữa chừng, chỉ lọc khi bấm phím `Enter` hoặc bấm nút "Lọc dữ liệu".
- [x] **1.3. Mục vai trò phân quyền (`frontend/src/views/BGH/RolesView.vue`)**:
  - [x] Đã chuẩn hóa giao diện và hiển thị mã code vai trò (`maCodeVaiTro`) dạng `font-mono` chuẩn mã code.

---

## 2. Đào Tạo & Chương Trình
- [x] **2.1. Ngành & Chuyên ngành (`frontend/src/views/BGH/ProgramsView.vue`)**:
  - [x] Sửa lỗi state dropdown: Bấm chi tiết 1 mục thì chỉ mở duy nhất dòng được bấm (tránh hiện tượng tất cả các mục cùng mở xuống).
  - [x] Bổ sung và định dạng đẹp mắt các trường chi tiết (Mã chương trình, Phiên bản, Ngày hiệu lực, Ngày hết hiệu lực, Người gửi duyệt, Người duyệt, Ngày tạo, Mô tả).
- [x] **2.2. Khung chương trình (`frontend/src/views/BGH/CurriculumView.vue`)**:
  - [x] Đã sửa khớp route API backend `/api/master-data/training-program-terms/by-program/{id}` và `/api/master-data/training-program-subjects/by-program/{id}`.
  - [x] Đã phân quyền `Principal` vào policy `AcademicOperations` và controllers master data.
  - [x] Bổ sung UI trạng thái rỗng đẹp mắt khi chương trình chưa có dữ liệu môn học.
- [x] **2.3. Học kỳ & Khóa**: Đã hoạt động ổn định.
- [x] **2.4. Tổng quan kết quả học tập**: Phân bố điểm số (A/B/C/D/F) và tỷ lệ Pass/Fail truy vấn chính xác dữ liệu CSDL thực tế.
- [x] **2.5. Báo cáo GPA**: Tính toán phân bố GPA và xu hướng các học kỳ lấy dữ liệu thật từ bảng `DiemSo`.
- [x] **2.6. SV nguy cơ rớt môn**: Lọc và phân trang học sinh nguy cơ rớt môn (`GpaMonHoc < 4`) từ CSDL.
- [x] **2.7. Báo cáo chi tiết**: Đã bổ sung truy vấn dữ liệu thực tế `MonthlyStats` & `DepartmentStats`, tích hợp xuất file Excel (`exportBghToExcel`) và in PDF (`printBghPage`).
- [x] **2.8. Tỷ lệ Pass/Fail (`frontend/src/views/BGH/Academic/PassFailRatesView.vue`)**:
  - [x] Redesign biểu đồ tỷ lệ Pass/Fail với dải màu và hiệu ứng mượt mà (Senior Motion Design).
  - [x] Sửa nút "Lọc nâng cao": chuyển vị trí bảng lọc nâng cao lên ngay dưới Filter Bar giúp thao tác click mượt mà.

---

## 3. Phê Duyệt & Đánh Giá
- [x] **3.1. Duyệt thời khóa biểu**: Đã bổ sung 2 API backend `POST /api/bgh/schedules/{id}/approve` & `POST /api/bgh/schedules/{id}/reject` lưu trực tiếp trạng thái `da_xuat_ban` / `da_huy` vào CSDL.
- [x] **3.2. Xung đột lịch học**: Đã hiển thị và lọc chính xác các TKB có xung đột từ CSDL.
- [x] **3.3. TKB đã duyệt (`frontend/src/views/BGH/Schedule/PublishedSchedulesView.vue`)**:
  - [x] Làm tròn hiển thị tổng số giờ giảng dạy tối đa 1 chữ số thập phân (chuyển `2.9166666666666665h` $\rightarrow$ `2.9h`).
- [ ] **3.4. Thay đổi và dạy bù**: Chờ nạp dữ liệu test.
- [x] **3.5. Đánh giá giảng viên (`frontend/src/views/BGH/Evaluations/TeacherEvalDetailsView.vue` / `Backend/Data/Data.cs`)**:
  - [x] **Sửa lỗi UTF-8 CSDL**: Khắc phục các chuỗi bị lỗi mã hóa tiếng Việt trong CSDL / Seeder (`Giáº£ng viĂªn...` $\rightarrow$ `Giảng viên truyền đạt kiến thức rõ ràng`).
  - [x] **Redesign Giao Diện 8 Mục**:
    1. Khoảng thở & Layout (Whitespace): tăng padding & gap giữa các card.
    2. Header & Breadcrumbs tinh gọn.
    3. Thẻ Giảng viên với Avatar bo tròn kèm Badge chứng nhận "Top 3 Giảng Viên Yêu Thích Nhất Kỳ".
    4. Lưới Tiêu chí 2x2 với Rating Sao hổ phách (Amber `#f59e0b`).
    5. Biểu đồ Biến động điểm theo học kỳ với curved line SVG & dark glass tooltip.
    6. Thẻ Ý kiến AI dạng quote Serif kèm Badge từ khóa bán trong suốt (`#NhiệtTình`, `#ChuyênMônCao`, `#TậnTâm`, `#ThựcTế`).
    7. Bo góc 12px, Typography & Bóng mờ chuẩn hiện đại.
- [x] **3.6. Tổng quan đánh giá (`frontend/src/views/BGH/Evaluations/EvalOverviewView.vue` & `TeacherRankingView.vue`)**:
  - [x] Seed dữ liệu xu hướng đánh giá để biểu đồ line chart hiển thị đầy đủ.
  - [x] Sửa thẻ cảnh báo giảng viên điểm thấp: Nhấp vào tự động chuyển hướng và lọc đúng các giảng viên điểm thấp (`< 3.8`).
- [ ] **3.7. Phân tích feedback AI**: Tạm hoãn (Chưa phát triển AI).

---

## 4. Cơ Sở Vật Chất
- [x] **4.1. Tòa nhà & phòng học (`Backend/Data/Data.cs`)**:
  - [x] Đã nạp (Seed) bổ sung 20+ bản ghi Tòa nhà & Phòng học đa dạng (Tòa A, B, C, P, D, E, F) vào CSDL SQL Server để phục vụ kiểm thử.

---

## 5. Giám Sát Hệ Thống
- [ ] **5.1. Nhật ký kiểm toán**: Tạm hoãn (Chờ làm rõ nghiệp vụ).

---

## 6. Cá Nhân
- [ ] **6.1. Cá nhân**: Tạm hoãn (Chưa cần đụng tới).

---
*Ngày cập nhật*: 07/08/2026�n đại.
- [x] **3.6. Tổng quan đánh giá (`frontend/src/views/BGH/Evaluations/EvalOverviewView.vue` & `TeacherRankingView.vue`)**:
  - [x] Seed dữ liệu xu hướng đánh giá để biểu đồ line chart hiển thị đầy đủ.
  - [x] Sửa thẻ cảnh báo giảng viên điểm thấp: Nhấp vào tự động chuyển hướng và lọc đúng các giảng viên điểm thấp (`< 3.8`).
- [ ] **3.7. Phân tích feedback AI**: Tạm hoãn (Chưa phát triển AI).

---

### 4. Cơ Sở Vật Chất
- [x] **4.1. Tòa nhà & phòng học (`Backend/Data/Data.cs`)**:
  - [x] Đã nạp (Seed) bổ sung 20+ bản ghi Tòa nhà & Phòng học đa dạng (Tòa A, B, C, P, D, E, F) vào CSDL SQL Server để phục vụ kiểm thử.

---

### 5. Giám Sát Hệ Thống
- [ ] **5.1. Nhật ký kiểm toán**: Tạm hoãn (Chờ làm rõ nghiệp vụ).

---

### 6. Cá Nhân
- [ ] **6.1. Cá nhân**: Tạm hoãn (Chưa cần đụng tới).

---
*Ngày cập nhật*: 07/08/2026
