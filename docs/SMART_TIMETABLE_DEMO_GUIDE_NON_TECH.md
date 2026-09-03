# HƯỚNG DẪN THỰC HÀNH DEMO HỆ THỐNG XẾP THỜI KHÓA BIỂU THÔNG MINH
*(Tài liệu dành cho Sinh viên báo cáo đồ án và Cán bộ Giáo vụ — 100% Phi kỹ thuật)*

---

## C1. TỔNG QUAN XẾP LỊCH THÔNG MINH

Hệ thống Xếp thời khóa biểu thông minh là công cụ hỗ trợ phòng Đào tạo và Cán bộ Giáo vụ tự động hóa việc lập kế hoạch giảng dạy, loại bỏ hoàn toàn các xung đột trùng lịch và tối ưu hóa tài nguyên giảng đường. Quy trình hoạt động tổng quát gồm 13 bước đơn giản:

1. **Giáo vụ đăng nhập**: Cán bộ Giáo vụ đăng nhập vào Cổng thông tin Giáo vụ.
2. **Tự động nhận diện cơ sở**: Hệ thống tự động xác định cơ sở làm việc của Giáo vụ (Ví dụ: Cơ sở Hồ Chí Minh) và thiết lập phạm vi dữ liệu cách ly tuyệt đối, không lẫn với cơ sở khác.
3. **Tự động chọn học kỳ phù hợp**: Hệ thống tự nhận diện học kỳ sắp diễn ra gần nhất được phép lập lịch (Ví dụ: `HK1_2027`).
4. **Kiểm tra dữ liệu tiền kỳ (Readiness Check)**: Trước khi xếp lịch, hệ thống tự động rà soát 11 điều kiện tiên quyết:
   - Danh sách các khóa học mở trong kỳ;
   - Các đợt học (Block);
   - Bảng quy đổi tín chỉ ra số ca học/tuần;
   - Danh sách giảng viên được phân công;
   - Lịch báo bận / nguyện vọng rảnh của giảng viên;
   - Định mức giảng dạy (trần tải tối đa 6 ca/tuần/giảng viên);
   - Danh sách phòng học đang hoạt động;
   - Sức chứa của từng phòng so với sĩ số lớp học;
   - Khung ca học đang áp dụng;
   - Tổng số chỗ trống (slot) phòng học có đủ cho tổng nhu cầu;
   - Trạng thái khóa lịch của học kỳ.
5. **Giáo vụ khởi chạy**: Khi toàn bộ điều kiện báo "Sẵn sàng", Giáo vụ bấm nút **"Xếp lịch ngay"**.
6. **Bộ máy tối ưu lịch tự động tìm phương án**: Bộ máy xếp lịch tự động chạy thuật toán tìm kiếm phương án tối ưu, đảm bảo triệt để các ràng buộc:
   - Một lớp học không học 2 môn cùng một lúc;
   - Một giảng viên không dạy 2 nơi cùng một ca;
   - Một phòng học không xếp 2 lớp cùng một giờ;
   - Phòng học phải đủ chỗ cho sĩ số sinh viên;
   - Giảng viên chỉ dạy vào các khung giờ không báo bận;
   - Không giảng viên nào bị quá tải vượt trần 6 ca/tuần.
7. **Tạo bản nháp thời khóa biểu**: Kết quả sắp xếp thành công được lưu dưới dạng Bản nháp (Draft), không ảnh hưởng đến dữ liệu đang chạy thực tế.
8. **Xem và rà soát đa chiều**: Giáo vụ có thể xem bản nháp trực quan theo 3 góc nhìn: **Theo lớp học**, **Theo giảng viên**, hoặc **Theo phòng học**.
9. **Duyệt và Xuất bản (Publish)**: Sau khi kiểm tra ưng ý, Giáo vụ bấm **"Xuất bản"** để áp dụng lịch chính thức.
10. **Tạo lịch học và buổi học tự động**: Hệ thống chuyển đổi toàn bộ bản nháp thành Thời khóa biểu chính thức và sinh tự động toàn bộ các Buổi học cụ thể theo từng ngày trong kỳ.
11. **Đồng bộ thời gian thực cho Giảng viên & Sinh viên**: Giảng viên vào mục "Lịch giảng dạy" và Sinh viên vào mục "Thời khóa biểu" sẽ nhìn thấy ngay lịch học chính thức đồng nhất 100%.
12. **Cơ chế cho phép làm lại trong 30 phút**: Nếu phát hiện cần điều chỉnh, Giáo vụ vẫn có thể xếp lại lịch trong vòng 30 phút kể từ lúc xuất bản, với điều kiện chưa có buổi học nào được điểm danh.
13. **Khóa lịch bất biến**: Ngay khi có bất kỳ buổi học nào đã được Giảng viên điểm danh, hoặc sau khi hết hạn 30 phút, lịch sẽ tự động khóa cứng vĩnh viễn để bảo toàn tính toàn vẹn học vụ.

*(Lưu ý: Hệ thống vận hành dựa trên bộ máy tối ưu lịch toán học kết hợp thuật toán di truyền thông minh, đặt con người làm trung tâm kiểm duyệt trước khi ban hành chính thức).*

---

## C2. CHECKLIST TRƯỚC BUỔI DEMO

Trước khi bắt đầu buổi trình bày, người thực hiện cần kiểm tra các mục sau để đảm bảo hệ thống ở trạng thái hoàn hảo nhất:

| STT | Hạng mục kiểm tra | Cách kiểm tra trên màn hình | Kết quả mong đợi | Làm gì nếu không đúng? |
|:---:|---|---|---|---|
| 1 | **Dịch vụ CSDL & Hệ thống** | Mở trình duyệt truy cập `https://localhost:5173` | Trang đăng nhập hiển thị bình thường | Kiểm tra Docker Desktop và đảm bảo container `du-an-tot-nghiep-sqlserver-1` và backend đang chạy. |
| 2 | **Tài khoản Giáo vụ** | Đăng nhập bằng tài khoản Giáo vụ cơ sở 14 | Vào thẳng Dashboard Giáo vụ, góc phải trên hiện "Cơ sở: FPT Polytechnic Hồ Chí Minh" | Đăng xuất, kiểm tra lại việc chọn đúng cổng dành cho Giáo vụ/Nhân viên. |
| 3 | **Học kỳ mục tiêu** | Mở menu *Lập lịch & Thời khóa biểu* -> *Quản lý thời khóa biểu* | Học kỳ hiển thị: `HK1_2027` (Mã: 15) | Kiểm tra bộ chọn học kỳ trên đầu màn hình; chọn đúng `HK1_2027`. |
| 4 | **Số lượng khóa học** | Đọc thẻ thống kê trên màn hình xếp lịch | Thẻ khóa học ghi rõ: **30 khóa học** | Liên hệ kỹ thuật hoặc khôi phục bản lưu `LMS_READY_FOR_DEMO.bak` (xem phụ lục). |
| 5 | **Trạng thái kiểm tra** | Xem bảng "Kiểm tra điều kiện xếp lịch" | Toàn bộ 11 mục hiển thị nhãn xanh **"Sẵn sàng"**, không có mục nào báo lỗi | Bấm "Kiểm tra lại điều kiện"; nếu vẫn lỗi, xem chi tiết mục thiếu để bổ sung. |
| 6 | **Nút "Xếp lịch ngay"** | Quan sát nút hành động chính ở góc trên | Nút có màu xanh nổi bật, chữ rõ ràng, **bấm được (enabled)** | Nếu nút bị mờ, xem bảng checklist bên dưới xem có mục nào chưa sẵn sàng không. |
| 7 | **Bản nháp hiện tại** | Nhìn mục "Bản nháp đang có" hoặc danh sách nháp | Hiển thị: "Chưa có bản nháp nào" | Nếu có bản nháp cũ, bấm nút "Xóa nháp" để đưa về trạng thái sạch ban đầu. |
| 8 | **Lịch đã xuất bản** | Mở menu *Lịch đã công bố* | Chưa có lịch chính thức nào của `HK1_2027` | Khôi phục bản lưu `LMS_READY_FOR_DEMO.bak` trước giờ demo. |
| 9 | **Tab trình duyệt** | Kiểm tra các tab đang mở trên máy | Chỉ mở duy nhất 1 tab chức năng Xếp lịch | Đóng các tab trùng lặp để tránh xung đột thao tác tiến trình. |
| 10 | **Tài khoản đối chiếu** | Chuẩn bị sẵn email tài khoản Giảng viên và Sinh viên mẫu | Sẵn sàng mở cửa sổ ẩn danh để đối chiếu sau khi xuất bản | Tham khảo mục C3 và C7 bên dưới. |

---

## C3. KỊCH BẢN DEMO THEO TỪNG CÚ CLICK (12 BƯỚC CHI TIẾT)

### Bước 1 — Mở trang đăng nhập
- **URL để copy**: `https://localhost:5173/login/staff`
- **Tài khoản**: Nhập email Giáo vụ: `p12test_staff01@lms.local`
- **Mật khẩu**: Nhập mật khẩu tài khoản thử nghiệm nội bộ của hệ thống.
- **Thao tác**: Bấm nút **"Đăng nhập"**.
- **Kết quả mong đợi**: Hệ thống chuyển hướng vào màn hình Dashboard Giáo vụ (`/staff/dashboard`). Góc trên cùng bên phải hiển thị tên "P12 Test Giáo Vụ" và đơn vị "FPT Polytechnic Hồ Chí Minh".

---

### Bước 2 — Mở chức năng Xếp lịch thông minh
- **Menu cần bấm**: Trên thanh điều hướng bên trái (Sidebar), tìm nhóm **"Lập lịch & Thời khóa biểu"**, bấm vào mục con **"Quản lý thời khóa biểu"**.
- **Route thực tế**: `/staff/schedule` (URL: `https://localhost:5173/staff/schedule`).
- **Tiêu đề màn hình**: "Quản lý thời khóa biểu thông minh" kèm phụ đề "Cơ sở: FPT Polytechnic Hồ Chí Minh".
- **Kết quả mong đợi**: Màn hình tải hoàn tất trong 1–2 giây, tự động tải bối cảnh học vụ của cơ sở.

---

### Bước 3 — Đọc và giải thích trạng thái Sẵn sàng
- **Vị trí học kỳ**: Ở thẻ thông tin chính, hiển thị rõ học kỳ đích là **HK1_2027** (Từ ngày 01/01/2027 đến 30/04/2027).
- **Vị trí số lượng khóa học**: Hiển thị con số **30 khóa học** cần lập lịch.
- **Bảng kiểm tra điều kiện**:
  - Người trình bày chỉ vào 11 tiêu chí kiểm tra (Khóa học, Block, Quy đổi tín chỉ, Năng lực giảng viên, Lịch rảnh, Định mức giờ dạy, Phòng học hoạt động, Sức chứa phòng, Ca học hoạt động, Tổng số slot phòng, Trạng thái khóa).
  - Tất cả 11 tiêu chí đều mang nhãn màu xanh lá **"Sẵn sàng"**.
  - Giải thích với hội đồng: *Hệ thống đã tự động bảo đảm 100% tính khả thi trước khi cho phép lập lịch, ngăn chặn hoàn toàn tình trạng xếp lịch lỗi do thiếu phòng hay quá tải giảng viên.*

---

### Bước 4 — Bấm "Xếp lịch ngay"
- **Thao tác**: Bấm chính xác **1 lần** vào nút màu xanh **"Xếp lịch ngay"** ở góc phải màn hình.
- **Lưu ý quan trọng**:
  - Không bấm đúp (double-click).
  - Khi đã bấm, nút sẽ tự động chuyển sang trạng thái đang xử lý và hiển thị vòng xoay tiến trình.
  - Không tải lại trang (F5) trong lúc thuật toán đang chạy.

---

### Bước 5 — Quan sát tiến trình xếp lịch
- **Trực quan trên màn hình**: Hộp thoại tiến trình xuất hiện hiển thị thanh tiến độ chạy từ 0% đến 100%.
- **Các thông điệp trực quan**:
  - "Đang khởi tạo quần thể phương án lịch..."
  - "Đang tối ưu hóa các ca học và đánh giá độ phù hợp..."
  - "Đã tìm thấy phương án tối ưu, đang tạo bản nháp..."
- **Thời gian xử lý**: Thông thường kéo dài khoảng **3 đến 6 giây**.
- **Kết thúc tiến trình**: Hệ thống tự động chuyển tiếp Giáo vụ sang trang **"Bản nháp thời khóa biểu"** (`/staff/schedule/pending`).

---

### Bước 6 — Xem tổng quan Bản nháp vừa tạo
- **Thẻ tóm tắt kết quả**:
  - **Khóa học đã xếp**: `30/30 khóa` (Đạt 100%).
  - **Chưa xếp được**: `0 khóa`.
  - **Xung đột cứng**: `0 xung đột` (Không trùng lớp, không trùng giảng viên, không trùng phòng).
  - **Tổng số buổi học trong tuần**: `90 buổi / tuần` (Mỗi khóa học 3 tín chỉ = 3 buổi/tuần, 30 khóa × 3 = 90 buổi).
- **Giải thích cơ chế hiển thị mượt mà (Load More)**:
  - Dòng trạng thái ghi rõ: **"Đang hiển thị 50/90 buổi"**.
  - Nhìn xuống cuối danh sách, có nút **"Xem thêm 40 buổi"**.
  - Bấm vào nút **"Xem thêm 40 buổi"**, danh sách mở rộng ngay lập tức hiển thị đủ **"90/90 buổi"** mà không làm chậm trình duyệt.

---

### Bước 7 — Xem lịch Theo lớp học (Class View)
- **Thao tác**: Trên thanh công cụ lọc của bản nháp, bấm vào nút/tab **"Theo lớp học"**.
- **Tìm khóa học mẫu**: Tìm lớp **`SD1901 - CNTT Phát triển phần mềm K2026`** (Khóa học: `Smart Demo 2027 - SD1901`, Môn: `HTML/CSS/JS Cơ bản`).
- **Thông tin chi tiết hiển thị**:
  - Khóa học được xếp 3 ca trong tuần (Ví dụ: Thứ 2, Thứ 4, Thứ 6).
  - Phòng học được phân bổ (Ví dụ: Phòng P.201 - Tòa nhà T).
  - Giảng viên được phân công: **Giảng Viên Lập trình Web (Hồ Chí Minh)**.
- **Tổng số ca**: Gom nhóm bảo toàn nguyên vẹn toàn bộ 90 buổi học, không thất thoát bất kỳ buổi nào.

---

### Bước 8 — Xem lịch Theo giảng viên (Teacher View)
- **Thao tác**: Bấm vào nút/tab **"Theo giảng viên"**.
- **Tìm giảng viên mẫu**: Tìm mục giảng viên **"Giảng Viên Lập trình Web (Hồ Chí Minh)"** (Mã: 10633).
- **Quan sát kết quả**:
  - Hiển thị danh sách các ca dạy trong tuần của giảng viên.
  - Tổng số ca dạy của giảng viên không vượt quá 6 ca/tuần (đảm bảo đúng chuẩn quy định chống quá tải).
  - Các ca dạy phân bổ hợp lý, không bị trùng lặp ca cùng ngày cùng giờ.

---

### Bước 9 — Xem lịch Theo phòng học (Room View)
- **Thao tác**: Bấm vào nút/tab **"Theo phòng học"**.
- **Tìm phòng học**: Chọn phòng học mà khóa mẫu SD1901 được phân (Ví dụ: Phòng P.201).
- **Đối chiếu**:
  - Tại mỗi ca học trong tuần, phòng chỉ phục vụ duy nhất 1 lớp học phần, không có 2 lớp học chung một phòng tại cùng một thời điểm.
  - Sức chứa phòng đáp ứng đầy đủ sĩ số 29 sinh viên của lớp SD1901.

---

### Bước 10 — Mở xem "Chi tiết kỹ thuật" (Tùy chọn cho Hội đồng)
- **Thao tác**: Nếu Thầy/Cô trong Hội đồng đặt câu hỏi về thuật toán, bấm vào nút **"Chi tiết kỹ thuật"** (Technical Details).
- **Thông số hiển thị**:
  - **Điểm chất lượng lịch (Fitness Score)**: Thể hiện mức độ tối ưu về độ rảnh và sở thích của Giảng viên.
  - **Số thế hệ thuật toán đã chạy**: Ví dụ 100 thế hệ.
  - **Số xung đột cứng**: 0.
  - **Cảnh báo mềm**: 0 hoặc các khuyến nghị khoảng cách ca dạy.
- **Thao tác tiếp theo**: Bấm đóng hộp thoại sau khi trình bày xong.

---

### Bước 11 — Mở hộp thoại Xác nhận Xuất bản
- **Thao tác**: Bấm nút màu xanh **"Xuất bản lịch chính thức"** (Publish Schedule).
- **Hộp thoại xác nhận hiện ra**:
  - Thông báo: *"Bạn có chắc chắn muốn xuất bản thời khóa biểu này thành lịch học chính thức?"*
  - Cảnh báo rõ ràng: Hệ thống cho phép xuất bản lại trong vòng **30 phút** nếu chưa có hoạt động điểm danh. Nếu đã có điểm danh, lịch sẽ bị khóa cứng vĩnh viễn.

---

### Bước 12 — Xuất bản chính thức (Publish)
- **Thao tác**: Bấm nút **"Xác nhận xuất bản"** trong hộp thoại.
- **Phản hồi hệ thống**:
  - Thông báo xuất hiện: *"Xuất bản thời khóa biểu thành công! Đã tự động tạo các buổi học chi tiết trong kỳ."*
  - Hệ thống tự động chuyển sang trang **"Lịch đã công bố"** (`/staff/schedule/published`).
  - Toàn bộ 30 khóa học và các buổi học trong kỳ đã chính thức có hiệu lực trên toàn trường.

---

## C4. ĐỐI CHIẾU SAU PUBLISH — PHÍA GIÁO VỤ

1. **Mở màn hình lịch đã xuất bản**:
   - URL: `https://localhost:5173/staff/schedule/published`
   - Hoặc bấm Sidebar -> *Lập lịch & Thời khóa biểu* -> **"Lịch đã công bố"**.
2. **Chọn học kỳ**: Đảm bảo chọn học kỳ **`HK1_2027`**.
3. **Tìm khóa học mẫu**: Tìm khóa học **`Smart Demo 2027 - SD1901`** (Lớp `SD1901 - CNTT Phát triển phần mềm K2026`).
4. **Ghi nhận thông tin thực tế**:
   - Môn học: `HTML/CSS/JS Cơ bản`
   - Lớp: `SD1901 - CNTT Phát triển phần mềm K2026`
   - Thứ trong tuần và Ca học: Ghi nhận chính xác (Ví dụ: Thứ 2, Thứ 4, Thứ 6 - Ca 1: 07:30 - 09:00)
   - Phòng học: Ghi nhận mã phòng (Ví dụ: `P.201`)
   - Giảng viên: `Giảng Viên Lập trình Web (Hồ Chí Minh)`
5. Giữ nguyên thông tin này để đối chiếu ở màn hình của Giảng viên và Sinh viên.

---

## C5. ĐỐI CHIẾU SAU PUBLISH — PHÍA GIẢNG VIÊN

1. **Đăng xuất tài khoản Giáo vụ**: Bấm biểu tượng người dùng ở góc trên cùng bên phải -> Bấm **"Đăng xuất"**.
2. **Khuyên dùng**: Mở một **cửa sổ trình duyệt ẩn danh mới** (Incognito Window) để đăng nhập tài khoản Giảng viên, tránh lưu đệm phiên làm việc.
3. **Mở trang đăng nhập**: Truy cập `https://localhost:5173/login/teacher`.
4. **Đăng nhập tài khoản Giảng viên mẫu**:
   - **Email**: `teacher.v11.14.502@edulms.local`
   - **Tên giảng viên**: Giảng Viên Lập trình Web (Hồ Chí Minh)
   - **Mật khẩu**: Nhập mật khẩu tài khoản thử nghiệm nội bộ.
   - Bấm **"Đăng nhập"**.
5. **Mở Lịch giảng dạy**:
   - Trên Sidebar, bấm vào mục **"Lịch giảng dạy"** (`/teacher/schedule`).
   - URL copy nhanh: `https://localhost:5173/teacher/schedule`.
6. **Xem và đối chiếu**:
   - Chọn tuần học bắt đầu từ ngày 01/01/2027 (tuần đầu tiên của `HK1_2027`).
   - Tìm buổi dạy của môn **`HTML/CSS/JS Cơ bản`** (Lớp `SD1901`).
   - **Xác nhận**: Thứ trong tuần, Ca học, Giờ học, và Phòng học hiển thị trên lịch của Thầy/Cô **trùng khớp 100%** với thông tin Giáo vụ vừa xem tại mục C4.

*(Danh sách tài khoản Giảng viên dự phòng cùng cơ sở nếu cần kiểm tra thêm: `largedemo.smart.gv01@lms.local`, `largedemo.smart.gv02@lms.local`, `teacher.csharp.b@lms.local`).*

---

## C6. ĐỐI CHIẾU SAU PUBLISH — PHÍA SINH VIÊN

1. **Đăng xuất tài khoản Giảng viên**.
2. **Mở trang đăng nhập**: Truy cập `https://localhost:5173/login/student`.
3. **Đăng nhập tài khoản Sinh viên mẫu**:
   - **Email**: `student.cntt01@lms.local`
   - **Tên sinh viên**: Nguyễn Văn Sinh Viên CNTT
   - **Lớp hành chính**: `SD1901 - CNTT Phát triển phần mềm K2026`
   - **Mật khẩu**: Nhập mật khẩu tài khoản thử nghiệm nội bộ.
   - Bấm **"Đăng nhập"**.
4. **Mở Thời khóa biểu Sinh viên**:
   - Trên Sidebar, bấm vào mục **"Thời khóa biểu"** (`/student/schedule`).
   - URL copy nhanh: `https://localhost:5173/student/schedule`.
5. **Xem và đối chiếu**:
   - Bấm nút chuyển tuần trên giao diện lịch để di chuyển đến tuần đầu tiên của tháng 01/2027.
   - Nhìn vào ô lịch tương ứng: Môn học **`HTML/CSS/JS Cơ bản`** xuất hiện đúng ca, đúng thứ, đúng phòng học và hiển thị đúng tên giảng viên `Giảng Viên Lập trình Web (Hồ Chí Minh)`.
6. **Cơ chế tải dữ liệu của Sinh viên**:
   - Hệ thống tự động xác định sinh viên thuộc Lớp hành chính `SD1901`, truy xuất các Buổi học chính thức thuộc các khóa học đã công bố (`da_xuat_ban`) trong học kỳ mà lớp tham gia.
   - Sinh viên không cần đăng ký thủ công lại lịch mà lịch được phân phối trực tiếp, chính xác và minh bạch.

---

## C7. BẢNG ĐỐI CHIẾU DỮ LIỆU ĐỒNG BỘ BA ROLE

Khi trình diễn trước Hội đồng, người demo có thể điền trực tiếp các thông số thực tế vào bảng sau để chứng minh tính đồng bộ tuyệt đối:

| Tiêu chí dữ liệu | Màn hình Giáo vụ | Màn hình Giảng viên | Màn hình Sinh viên | Đánh giá |
|---|:---:|:---:|:---:|:---:|
| **Tên khóa học / Môn học** | Smart Demo 2027 - SD1901 (`HTML/CSS/JS Cơ bản`) | `HTML/CSS/JS Cơ bản` | `HTML/CSS/JS Cơ bản` | **Trùng khớp 100%** |
| **Lớp sinh viên** | SD1901 | SD1901 | SD1901 | **Trùng khớp 100%** |
| **Thứ học trong tuần** | *(Ví dụ: Thứ 2, 4, 6)* | *(Ví dụ: Thứ 2, 4, 6)* | *(Ví dụ: Thứ 2, 4, 6)* | **Trùng khớp 100%** |
| **Ca học & Giờ học** | *(Ví dụ: Ca 1: 07:30 - 09:00)* | *(Ví dụ: Ca 1: 07:30 - 09:00)* | *(Ví dụ: Ca 1: 07:30 - 09:00)* | **Trùng khớp 100%** |
| **Phòng học phân bổ** | *(Ví dụ: Phòng P.201)* | *(Ví dụ: Phòng P.201)* | *(Ví dụ: Phòng P.201)* | **Trùng khớp 100%** |
| **Giảng viên giảng dạy** | Giảng Viên Lập trình Web | Giảng Viên Lập trình Web | Giảng Viên Lập trình Web | **Trùng khớp 100%** |
| **Phạm vi cơ sở** | Hồ Chí Minh | Hồ Chí Minh | Hồ Chí Minh | **Cách ly độc lập** |

**Tiêu chuẩn nghiệm thu đạt yêu cầu**:
- Ba màn hình hiển thị cùng môn, cùng thứ, cùng ca, cùng giờ, cùng phòng và cùng giảng viên.
- Không phát sinh bất kỳ lỗi xung đột lịch hay trùng chéo phòng học.
- Dữ liệu hoàn toàn độc lập theo đúng cơ sở Hồ Chí Minh (Campus 14), không xuất hiện phòng học hay lớp của cơ sở khác.

---

## C8. BẢNG ROUTE & ĐƯỜNG DẪN CÓ THỂ COPY NHANH

Dưới đây là bảng tổng hợp toàn bộ các đường dẫn thực tế trên môi trường chạy tại máy (`https://localhost:5173`), đã được xác minh hoạt động không gặp lỗi:

| Chức năng | Vai trò (Role) | Thao tác trên Menu | Đường dẫn tương đối | URL đầy đủ (Click hoặc Copy) |
|---|---|---|---|---|
| **Cổng đăng nhập chung** | Công khai | Truy cập trang chủ | `/` | `https://localhost:5173/` |
| **Đăng nhập Giáo vụ** | Công khai | Chọn cổng Cán bộ / Giáo vụ | `/login/staff` | `https://localhost:5173/login/staff` |
| **Đăng nhập Giảng viên** | Công khai | Chọn cổng Giảng viên | `/login/teacher` | `https://localhost:5173/login/teacher` |
| **Đăng nhập Sinh viên** | Công khai | Chọn cổng Sinh viên | `/login/student` | `https://localhost:5173/login/student` |
| **Dashboard Giáo vụ** | `AcademicStaff` | Sidebar -> Tổng quan | `/staff/dashboard` | `https://localhost:5173/staff/dashboard` |
| **Xếp lịch thông minh** | `AcademicStaff` | Sidebar -> Lập lịch & TKB -> Quản lý TKB | `/staff/schedule` | `https://localhost:5173/staff/schedule` |
| **Bản nháp thời khóa biểu** | `AcademicStaff` | Sidebar -> Lập lịch & TKB -> Bản nháp TKB | `/staff/schedule/pending` | `https://localhost:5173/staff/schedule/pending` |
| **Lịch đã công bố (Giáo vụ)** | `AcademicStaff` | Sidebar -> Lập lịch & TKB -> Lịch đã công bố | `/staff/schedule/published` | `https://localhost:5173/staff/schedule/published` |
| **Dashboard Giảng viên** | `Teacher` | Sidebar -> Dashboard | `/teacher/dashboard` | `https://localhost:5173/teacher/dashboard` |
| **Lịch giảng dạy (Giảng viên)** | `Teacher` | Sidebar -> Lịch giảng dạy | `/teacher/schedule` | `https://localhost:5173/teacher/schedule` |
| **Dashboard Sinh viên** | `Student` | Sidebar -> Dashboard | `/student/dashboard` | `https://localhost:5173/student/dashboard` |
| **Thời khóa biểu Sinh viên** | `Student` | Sidebar -> Thời khóa biểu | `/student/schedule` | `https://localhost:5173/student/schedule` |

---

## C9. KỊCH BẢN THUYẾT TRÌNH BẢO VỆ ĐỒ ÁN (5 — 7 PHÚT)

*Dưới đây là gợi ý lời thoại chuẩn mực, tự tin và chuyên nghiệp khi đứng trước Hội đồng chấm tốt nghiệp:*

> **Kính thưa Thầy Cô trong Hội đồng chấm đồ án tốt nghiệp,**
>
> Trong công tác quản lý đào tạo tại các trường đại học và cao đẳng, bài toán xếp thời khóa biểu luôn là một thách thức lớn. Giáo vụ thường mất từ nhiều ngày đến vài tuần để sắp xếp hàng trăm lớp học mà vẫn dễ xảy ra tình trạng trùng phòng, trùng ca dạy của giảng viên, hoặc sinh viên bị bố trí lịch học không hợp lý.
>
> Nhóm chúng em đã nghiên cứu và xây dựng thành công phân hệ **Xếp thời khóa biểu thông minh** tích hợp trực tiếp trong Hệ thống Quản lý Đào tạo EduLMS. Ngay sau đây, em xin phép được trình diễn trực tiếp phân hệ này:
>
> **1. Kiểm soát điều kiện khả thi tự động (Readiness):**
> *(Thao tác mở màn hình `/staff/schedule`)*
> Khi Giáo vụ truy cập, hệ thống tự động xác định cơ sở Hồ Chí Minh và chọn học kỳ `HK1_2027` với 30 khóa học cần xếp. Ngay tại đây, hệ thống tự động rà soát toàn bộ 11 điều kiện học vụ: từ quy đổi tín chỉ, năng lực chuyên môn của giảng viên, lịch báo bận, trần tải giảng dạy tối đa 6 ca/tuần, cho đến kiểm tra kích thước phòng học có đủ sức chứa sĩ số lớp hay không. Chỉ khi toàn bộ 11 điều kiện này chuyển màu xanh "Sẵn sàng", nút hành động mới cho phép bấm.
>
> **2. Xếp lịch tối ưu chỉ với 1 cú click:**
> *(Thao tác bấm nút "Xếp lịch ngay")*
> Khi em bấm nút "Xếp lịch ngay", bộ máy tối ưu lịch của hệ thống áp dụng thuật toán di truyền thông minh để tìm kiếm phương án tối ưu toàn cục. Toàn bộ 30 khóa học với 90 ca học trong tuần được tính toán và xếp lịch hoàn tất chỉ trong vòng khoảng 4 giây.
>
> **3. Bản nháp minh bạch và trực quan:**
> *(Thao tác chỉ vào màn hình `/staff/schedule/pending`)*
> Kết quả tạo ra bản nháp đạt 30/30 khóa học, 0 xung đột cứng. Giáo vụ có thể kiểm tra trực quan theo 3 góc nhìn: Theo lớp học, Theo giảng viên, và Theo từng phòng học. Đặc biệt, giao diện hỗ trợ tải lũy tiến mượt mà, bảo toàn trọn vẹn 100% dữ liệu mà không gây giật lag trình duyệt.
>
> **4. Cơ chế kiểm duyệt và bảo vệ bất biến (Publish & Lock):**
> *(Thao tác bấm "Xuất bản" và xác nhận)*
> Hệ thống áp dụng triết lý "Con người là trung tâm kiểm duyệt" (Human-in-the-loop). Khi Giáo vụ bấm "Xuất bản", hệ thống mới chính thức tạo các buổi học chi tiết trong kỳ. Nếu cần điều chỉnh, Giáo vụ vẫn có quyền xếp lại trong 30 phút. Nhưng một khi giảng viên đã bắt đầu điểm danh buổi học đầu tiên, toàn bộ lịch sẽ khóa vĩnh viễn để bảo vệ tính bất biến của điểm danh và dữ liệu học tập.
>
> **5. Đồng bộ thời gian thực đa vai trò:**
> *(Thao tác mở nhanh màn hình Giảng viên và Sinh viên)*
> Ngay sau khi xuất bản, Giảng viên và Sinh viên đăng nhập vào hệ thống đều thấy lịch học của mình hiển thị đồng bộ, chuẩn xác đến từng phòng, từng ca và từng giảng viên.
>
> Phân hệ này đã giải quyết triệt để bài toán xếp lịch thủ công, tiết kiệm hàng chục giờ lao động cho cán bộ đào tạo và đảm bảo tính chuẩn xác tuyệt đối cho nhà trường. Em xin trân trọng cảm ơn Thầy Cô và kính mời Thầy Cô đặt câu hỏi!

---

## C10. HƯỚNG DẪN XỬ LÝ TÌNH HUỐNG KHÔNG KỸ THUẬT

Trong quá trình demo, nếu gặp các hiện tượng bất thường, người thực hiện chỉ cần bình tĩnh làm theo bảng hướng dẫn sau:

| Hiện tượng | Điều TUYỆT ĐỐI KHÔNG làm | Thao tác ĐÚNG NÊN LÀM |
|---|---|---|
| **Nút "Xếp lịch ngay" bị mờ (không bấm được)** | Không cố bấm liên tục vào nút mờ. | Kéo xuống xem bảng "Kiểm tra điều kiện". Tìm mục có nhãn màu cam/đỏ, đọc dòng hướng dẫn để biết đang thiếu phòng, thiếu giảng viên hay thiếu block học. |
| **Tiến trình xếp lịch chạy lâu hơn 10 giây** | Không bấm F5 tải lại trang; không bấm nút Tạo lại trên tab khác. | Chờ hết 15 giây. Nếu thanh tiến độ đứng yên, hệ thống sẽ hiện nút *"Kiểm tra lại tiến trình"*. Bấm nút đó để hệ thống tự động cập nhật kết quả. |
| **Báo lỗi 403 (Không có quyền truy cập)** | Không sửa đường dẫn URL trên thanh địa chỉ. | Đăng xuất tài khoản hiện tại, kiểm tra xem có đang dùng nhầm tài khoản Sinh viên/Giảng viên để vào trang Giáo vụ hay không. Đăng nhập lại bằng tài khoản Giáo vụ. |
| **Danh sách buổi học trong nháp chỉ thấy 50 buổi** | Không kết luận hệ thống bị mất dữ liệu. | Kéo chuột xuống cuối danh sách, nhìn thấy nút *"Xem thêm 40 buổi"*. Bấm vào nút đó để hiển thị đủ toàn bộ 90 buổi. |
| **Màn hình hiện cảnh báo "Xung đột cứng"** | Không cố bấm nút "Xuất bản". | Đọc chi tiết cảnh báo xem lớp nào, giảng viên nào hoặc phòng nào đang bị xếp trùng. Bấm xóa bản nháp và kiểm tra lại điều kiện dữ liệu trước khi xếp lại. |
| **Sau khi xuất bản, không cho phép xếp đè lại** | Không tìm cách sửa cơ sở dữ liệu. | Kiểm tra xem đã vượt quá thời hạn 30 phút chưa, hoặc đã có giáo viên nào vào điểm danh thử chưa. Khi đã có điểm danh, quy chế học vụ cấm thay đổi lịch. |

---

## C11. QUẢN LÝ DỮ LIỆU SAU KHI DEMO

Sau khi buổi demo kết thúc thành công và Giáo vụ đã bấm **Xuất bản**:
- Dữ liệu học kỳ `HK1_2027` lúc này đã trở thành Lịch chính thức (đã có Thời khóa biểu và Buổi học).
- Nếu muốn đưa hệ thống quay trở lại trạng thái ban đầu để chuẩn bị cho buổi demo tiếp theo, cán bộ kỹ thuật chỉ cần thực hiện việc khôi phục lại bản sao lưu đã tạo sẵn trước đó.

---

### PHỤ LỤC KỸ THUẬT: HƯỚNG DẪN KHÔI PHỤC LẠI BẢN SAO LƯU (DÀNH CHO KỸ THUẬT VIÊN)
*(Phần này dành riêng cho quản trị viên hệ thống, người dùng thông thường không cần thực hiện)*

Khi cần hoàn trả CSDL về trạng thái sạch nguyên bản trước khi xếp lịch:
1. Mở PowerShell trên máy chủ.
2. Chạy lệnh khôi phục bản backup `LMS_READY_FOR_DEMO.bak` đã tạo an toàn trong container SQL Server:

```powershell
docker exec du-an-tot-nghiep-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P [REDACTED_SA_PASSWORD] -C -Q "
ALTER DATABASE LMS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE LMS FROM DISK = '/var/opt/mssql/data/LMS_READY_FOR_DEMO.bak' WITH REPLACE;
ALTER DATABASE LMS SET MULTI_USER;
"
```

3. Khởi động lại backend để hoàn tất:

```powershell
docker restart du-an-tot-nghiep-backend-1
```
