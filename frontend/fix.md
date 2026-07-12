# Hướng dẫn & Đặc tả: Đăng nhập nhanh và Đồng bộ Dữ liệu 3 Sinh viên Chuyên ngành (LMS)

Tài liệu này hướng dẫn cách cấu hình đăng nhập nhanh và cơ chế hoạt động của hệ thống đồng bộ dữ liệu mẫu theo chuyên ngành của sinh viên (CNTT, Thiết kế Đồ họa, Marketing) trong dự án LMS.

---

## 1. Thông tin Đăng nhập nhanh (Quick Login)

Để đăng nhập nhanh vào các cổng sinh viên, tại trang đăng nhập sinh viên `/login/student` hiển thị 3 nút demo:

1.  **Sinh viên CNTT (Mặc định)**:
    *   **Tên**: Nguyễn Văn An
    *   **Tài khoản demo**: click nút hoặc nhập email `student`
    *   **Ngành học**: Kỹ thuật Phần mềm (Phát triển phần mềm)
2.  **Sinh viên Thiết kế Đồ họa (TKĐH)**:
    *   **Tên**: Nguyễn Thiết Kế
    *   **Tài khoản demo**: click nút hoặc nhập email `student_gd`
    *   **Ngành học**: Thiết kế đồ họa
3.  **Sinh viên Marketing**:
    *   **Tên**: Trần Thị Marketing
    *   **Tài khoản demo**: click nút hoặc nhập email `student_mkt`
    *   **Ngành học**: Digital Marketing

---

## 2. Logic Xử lý tại Frontend

### 2.1 Cấu hình form Đăng nhập
Form đăng nhập nhanh nằm ở component `AuthLoginForm.vue` ([đường dẫn file](src/components/auth/AuthLoginForm.vue)):
*   Khi truy cập cổng sinh viên (`portal.slug === 'student'`), giao diện tự động render ra 3 nút đăng nhập nhanh thay vì 1 nút chung.
*   Khi click vào nút, email tương ứng (`student`, `student_gd`, `student_mkt`) sẽ được điền vào form và submit tự động.

### 2.2 Xử lý session tại authStore
Pinia Store `useAuthStore` ([đường dẫn file](src/stores/auth.js)):
*   Hàm `login()` sẽ kiểm tra email. Nếu email thuộc 3 tài khoản trên, store sẽ tự cấp thông tin người dùng giả lập tương ứng với role `Student` và lưu session.
*   Store gọi hàm `syncActiveStudentData()` khi đăng nhập hoặc đăng xuất để đồng bộ dữ liệu mock cho các trang con.

### 2.3 Cơ chế đồng bộ dữ liệu động
Dữ liệu mock được quản lý tập trung ở file `studentData.mock.js` (đã xóa — `src/data/studentData.mock.js` không còn tồn tại trong repository):
*   Hàm `syncActiveStudentData()` sẽ đọc thông tin email trong session.
*   Nếu email là `student_gd`, toàn bộ dữ liệu mẫu chung (profile, dashboard, courses, assignments, grades, attendance, schedule, tuition, registrations...) sẽ được ghi đè bằng bộ dữ liệu của ngành **Thiết kế Đồ họa**.
*   Nếu email là `student_mkt`, dữ liệu mẫu chung sẽ được ghi đè bằng bộ dữ liệu của ngành **Marketing**.
*   Các trường hợp còn lại sẽ dùng bộ dữ liệu mặc định của **CNTT**.

---

## 3. Các View hiển thị sử dụng dữ liệu đồng bộ

Toàn bộ các trang chức năng của sinh viên đã được liên kết với dữ liệu reactive từ `studentData.mock.js`. Khi đăng nhập bằng tài khoản khác nhau, dữ liệu trên các trang sau sẽ tự động thay đổi theo đúng chuyên ngành:

*   **Trang chủ Dashboard**: [Dashboard.vue](src/views/SinhVien/Dashboard.vue)
*   **Danh sách Khóa học**: [KhoacHoc.vue](src/views/SinhVien/HocTap/KhoacHoc.vue)
*   **Bài tập môn học**: [AssignmentsView.vue](src/views/Student/AssignmentsView.vue)
*   **Bảng điểm học tập**: [GradesView.vue](src/views/Student/GradesView.vue)
*   **Chuyên cần & Điểm danh**: [AttendanceView.vue](src/views/Student/AttendanceView.vue)
*   **Thời khóa biểu tuần**: [ScheduleView.vue](src/views/Student/ScheduleView.vue)
*   **Hóa đơn học phí**: [TuitionView.vue](src/views/Student/TuitionView.vue)
*   **Đăng ký môn học**: [RegistrationsView.vue](src/views/Student/RegistrationsView.vue)
*   **Khung chương trình học**: [CurriculumView.vue](src/views/Student/CurriculumView.vue)
*   **Chi tiết môn học (bài giảng, video, quiz)**: [CourseDetailView.vue](src/views/Student/CourseDetailView.vue)

---

## 4. Cơ chế Nạp & Điều hướng Môn học Động

Nhằm thay thế việc hiển thị cứng môn "Cấu trúc dữ liệu & Giải thuật" (CTDL101) trước đây, hệ thống đã được nâng cấp sang cơ chế nạp môn học động hoàn chỉnh.

### 4.1 Điều hướng Động từ Curriculum
Tại trang Khung chương trình học (`CurriculumView.vue`), các đường dẫn cứng đã được chuyển đổi sang dạng liên kết động sử dụng thuộc tính `:to` và biến `item.subjectCode` của từng môn học:
*   **Học trước (Suggestion)**: `:to="'/student/courses/' + item.subjectCode"`
*   **Card View**: `:to="'/student/courses/' + item.subjectCode"`
*   **Table View**: `:to="'/student/courses/' + item.subjectCode"`

Điều này giúp URL thay đổi tương ứng theo mã môn được chọn, ví dụ: `/student/courses/WEB201`, `/student/courses/LTW301`, v.v.

### 4.2 Tra cứu và Fallback dữ liệu tại Chi tiết Môn học
Khi trang Chi tiết môn học (`CourseDetailView.vue`) được mở, component sẽ thực hiện:

1.  **Đọc Route Parameter**:
    Đọc tham số `:courseId` từ route để lấy mã môn học được yêu cầu (ví dụ: `LTW301`).
2.  **Tra cứu và Fallback trong computed `currentSubject`**:
    *   **Bước 1**: Tìm kiếm mã môn trong chương trình học hiện tại (`mockStudentCurriculum.semesters`).
    *   **Bước 2**: Nếu không có, tìm kiếm trong lịch sử học trước phiên bản cũ (`mockCurriculumVersionData.earlyLearningHistory`).
    *   **Bước 3 (Fallback quan trọng)**: Nếu mã môn học không thuộc chương trình của sinh viên hiện tại (ví dụ: người dùng truy cập trực tiếp vào môn **`LTW301`**), hệ thống sẽ đối chiếu bảng map tên môn học (`LTW301` -> "Lập trình Web") và tự động sinh thông tin môn học mock động thay vì bị crash hoặc fallback về môn mặc định CTDL101.
3.  **Tạo các computed properties động**:
    *   `mockCourse`: Cung cấp tiêu đề, mã môn, số tín chỉ, tên giảng viên chuyên ngành và màu gradient bìa tương ứng với mã môn học.
    *   `mockStats` & `miniStats`: Tính toán động phần trăm tiến độ, số bài học đã xong, số tài liệu và bài tập tương ứng với tiến độ thực tế của môn đó.
    *   `mockQuizQuestions`: Tự động nạp câu hỏi quiz riêng biệt (môn Vue `WEB201` có câu hỏi Vue, môn Web API `API201` có câu hỏi Web API, v.v.).
    *   `mockLessons` & `learningLessons`:
        *   Nạp bộ bài học chi tiết được chuẩn bị sẵn cho môn Vue (`WEB201`), API (`API201`), Đồ họa (`gdLessons`), Marketing (`mktLessons`).
        *   Tự động sinh ra lộ trình bài học hợp lý đúng tên môn học cho các môn học khác bằng hàm `generateDefaultLessonsForSubject()`.
    *   `mockAISummary`: Tự động sinh nội dung tóm tắt bài học của AI chứa tiêu đề động của bài học đang chọn thay vì bị cứng nội dung.

### 4.3 Quản lý Trạng thái & Reset qua Watcher
*   Sử dụng `watch(courseId, ...)` với tuỳ chọn `{ immediate: true }` được đặt ở **cuối** block `script setup`.
*   Mỗi khi người dùng chuyển giữa các môn học khác nhau, watcher này sẽ tự động:
    *   Đặt lại câu trả lời quiz (`quizAnswers.value = {}`).
    *   Đặt lại trạng thái nộp bài quiz (`quizSubmitted.value = false`).
    *   Tìm bài học hiện tại đang học (active) hoặc bài học đầu tiên của môn học mới để kích hoạt và mở rộng chương chứa bài học đó.

### 4.4 Các lỗi đặc biệt đã sửa đổi (Troubleshooting)

1.  **Lỗi Hoisting `ReferenceError: Cannot access 'quizSubmitted' before initialization`**:
    *   *Nguyên nhân*: Do watcher chạy ở chế độ `immediate: true` ngay khi khởi tạo component, nhưng biến `quizSubmitted` lại được khai báo ở phía dưới file, dẫn đến việc watcher cố gắng gán `quizSubmitted.value = false` trước khi nó được khởi tạo.
    *   *Khắc phục*: Di chuyển khai báo `const quizSubmitted = ref(false)` lên đầu file (ngay sau các khai báo ref cơ bản đầu tiên), đồng thời đặt watcher ở cuối block script setup để đảm bảo tất cả các phụ thuộc đều được định nghĩa trước khi thực thi.
2.  **Lỗi Unwrap Computed Ref ở `miniStats`**:
    *   *Nguyên nhân*: Thuộc tính `mockStats` được định nghĩa lại dưới dạng computed ref, nhưng trong computed `miniStats` lại truy cập dạng `mockStats[0]` thay vì `mockStats.value[0]`.
    *   *Khắc phục*: Thêm `.value` khi tham chiếu đến `mockStats` trong phần script.

