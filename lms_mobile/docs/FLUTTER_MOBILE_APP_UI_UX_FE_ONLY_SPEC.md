# SPEC TRIỂN KHAI APP FLUTTER RIÊNG CHO LMS MOBILE

> Dành cho AI coding agent/Cursor/Claude/Gemini khi thiết kế và code **UI/UX mobile app Flutter riêng** cho repo `Phan-Thanh-Danh/Du-An-Tot-Nghiep`.
>
> Mục tiêu: tạo app Flutter độc lập trong repo, chỉ làm phần **FE mobile**, bám sát file Excel `LMS_Mobile_UI_Spec.xlsx`, tài liệu nghiệp vụ LMS, và giao diện/luồng có sẵn. **Tuyệt đối không sửa Backend.**

---

## 1. Kết luận sau khi xem repo

Repo hiện tại là hệ thống LMS/Academic Management System có:

- `Backend/`: ASP.NET Core + EF Core + SQL Server + JWT.
- `frontend/`: Vue 3 + Vite + Vue Router + Pinia + Tailwind CSS.
- README có ghi frontend hiện có route student shell tại `/student/*`.
- README không thấy thư mục Flutter/mobile app sẵn trong root tree.
- Tài liệu nghiệp vụ và Excel lại yêu cầu rõ mobile app dùng **Flutter 3 + Riverpod + Dio + GoRouter**.

Vì trưởng nhóm yêu cầu **app Flutter riêng**, hướng đúng là:

```txt
Du-An-Tot-Nghiep/
├── Backend/              # KHÔNG ĐỤNG
├── frontend/             # KHÔNG ĐỤNG hoặc chỉ tham khảo UI hiện có
├── docs/                 # chỉ tham khảo
├── lms_mobile/           # TẠO MỚI APP FLUTTER Ở ĐÂY
└── ...
```

Không thiết kế mobile bên trong `frontend/` nữa. Tạo app Flutter độc lập tên `lms_mobile` hoặc `mobile_app`.

---

## 2. Nguyên tắc bắt buộc

### 2.1. Phạm vi được phép làm

AI chỉ được phép tạo/sửa trong:

```txt
lms_mobile/
```

Có thể đọc để tham khảo:

```txt
README.md
docs/
frontend/src/
Tài Liệu/
design-md/
```

### 2.2. Phạm vi cấm đụng

Tuyệt đối không sửa:

```txt
Backend/
Backend/Controllers/
Backend/Services/
Backend/Models/
Backend/DTOs/
Backend/Data/
Backend/Migrations/
Backend/Program.cs
Source Code.sln
frontend/
```

Trường hợp cần API chưa có thì chỉ tạo ghi chú trong:

```txt
lms_mobile/docs/api_needed_for_mobile.md
```

Không tự thêm Controller, không sửa DB, không tạo migration, không chỉnh DTO backend.

### 2.3. Chiến lược dữ liệu

Do Backend hiện còn nhiều API dự kiến chưa hoàn thiện, app Flutter phải dùng chiến lược 2 lớp:

```txt
UI Flutter
  ↓
Repository Interface
  ↓
Mock Repository trước
  ↓
Remote Repository/Dio sau khi Backend có API thật
```

Mọi màn hình phải chạy được bằng mock data, để tổ trưởng xem UI trước.

---

## 3. Hướng đi triển khai từ đầu

### Bước 1: Tạo branch riêng

```bash
git checkout -b feature/flutter-mobile-lms-ui
```

### Bước 2: Tạo Flutter app riêng ở root repo

```bash
flutter create lms_mobile
cd lms_mobile
```

### Bước 3: Thêm package cần dùng

```bash
flutter pub add flutter_riverpod dio go_router flutter_secure_storage google_fonts intl table_calendar fl_chart file_picker cached_network_image shimmer animations
```

Khuyến nghị thêm sau nếu cần:

```bash
flutter pub add connectivity_plus url_launcher image_picker
```

### Bước 4: Chạy kiểm tra app rỗng

```bash
flutter pub get
flutter run
```

### Bước 5: Tạo kiến trúc thư mục

```txt
lms_mobile/lib/
├── main.dart
├── app/
│   ├── lms_mobile_app.dart
│   ├── router/app_router.dart
│   └── bootstrap.dart
├── core/
│   ├── constants/app_constants.dart
│   ├── theme/app_colors.dart
│   ├── theme/app_theme.dart
│   ├── theme/app_text_styles.dart
│   ├── network/api_client.dart
│   ├── network/api_endpoints.dart
│   ├── storage/secure_storage.dart
│   ├── utils/date_time_formatters.dart
│   └── widgets/
│       ├── app_bottom_nav.dart
│       ├── app_top_bar.dart
│       ├── app_stat_card.dart
│       ├── app_section_header.dart
│       ├── app_empty_state.dart
│       ├── app_error_state.dart
│       ├── app_loading_skeleton.dart
│       ├── app_status_badge.dart
│       └── app_primary_button.dart
├── features/
│   ├── auth/
│   │   ├── data/
│   │   ├── presentation/login_screen.dart
│   │   └── presentation/role_select_screen.dart
│   ├── student/
│   │   ├── data/student_mock_repository.dart
│   │   ├── data/student_remote_repository.dart
│   │   ├── data/student_repository.dart
│   │   ├── models/
│   │   └── presentation/
│   │       ├── student_shell.dart
│   │       ├── dashboard/student_dashboard_screen.dart
│   │       ├── courses/student_courses_screen.dart
│   │       ├── courses/student_course_detail_screen.dart
│   │       ├── assignments/student_assignments_screen.dart
│   │       ├── exams/student_exam_schedule_screen.dart
│   │       ├── grades/student_grades_screen.dart
│   │       ├── schedule/student_schedule_screen.dart
│   │       ├── attendance/student_attendance_screen.dart
│   │       ├── tuition/student_tuition_screen.dart
│   │       ├── notifications/student_notifications_screen.dart
│   │       └── profile/student_profile_screen.dart
│   └── parent/
│       ├── data/parent_mock_repository.dart
│       ├── data/parent_remote_repository.dart
│       ├── data/parent_repository.dart
│       ├── models/
│       └── presentation/
│           ├── parent_shell.dart
│           ├── dashboard/parent_dashboard_screen.dart
│           ├── children/parent_children_screen.dart
│           ├── grades/parent_grades_screen.dart
│           ├── attendance/parent_attendance_screen.dart
│           ├── schedule/parent_schedule_screen.dart
│           ├── exams/parent_exam_schedule_screen.dart
│           ├── finance/parent_tuition_screen.dart
│           ├── notifications/parent_notifications_screen.dart
│           └── profile/parent_profile_screen.dart
└── docs/
    ├── api_needed_for_mobile.md
    └── ui_ux_notes.md
```

---

## 4. Công nghệ bắt buộc cho app Flutter

```yaml
Flutter: 3.x
State management: flutter_riverpod
Routing: go_router
HTTP client: dio
Secure token: flutter_secure_storage
Calendar: table_calendar
Chart: fl_chart
Font: Google Fonts Inter
Design style: Material 3, mobile-first, clean education app
```

Không dùng GetX cho dự án này để tránh lệch yêu cầu Excel/tài liệu.

---

## 5. Design System Mobile

Bám theo Excel `03_Design_System`.

### 5.1. Màu sắc

```dart
class AppColors {
  static const primary = Color(0xFF2563EB);
  static const success = Color(0xFF22C55E);
  static const error = Color(0xFFEF4444);
  static const warning = Color(0xFFF59E0B);
  static const background = Color(0xFFF8FAFC);
  static const surface = Color(0xFFFFFFFF);
  static const textPrimary = Color(0xFF0F172A);
  static const textSecondary = Color(0xFF64748B);
  static const border = Color(0xFFE2E8F0);
}
```

### 5.2. Typography

Font chính: **Inter**.

```txt
Display: 28px SemiBold
Title: 20px SemiBold
Subtitle: 16px SemiBold
Body: 14px Regular/Medium
Caption: 12px Regular
```

### 5.3. Spacing

```txt
4 / 8 / 12 / 16 / 20 / 24 / 32
```

### 5.4. Radius

```txt
Card: 16px
Button: 12px
BottomSheet: 24px
Input: 12px
Avatar: 999px
```

### 5.5. Touch target

Tất cả button, tab, icon button phải tối thiểu:

```txt
44x44px
```

### 5.6. Shadow

Dùng shadow nhẹ, không dùng glassmorphism nặng.

```dart
BoxShadow(
  color: Colors.black.withOpacity(0.06),
  blurRadius: 16,
  offset: Offset(0, 6),
)
```

---

## 6. Navigation Mobile

### 6.1. Vai trò Student

Bottom navigation tối đa 5 tab:

```txt
1. Trang chủ
2. Học tập
3. Lịch
4. Kết quả
5. Cá nhân
```

Các màn con đi qua card/action/list item.

Routes đề xuất:

```txt
/login
/role-select
/student/dashboard
/student/courses
/student/courses/:courseId
/student/assignments
/student/exams
/student/grades
/student/schedule
/student/attendance
/student/tuition
/student/notifications
/student/profile
```

### 6.2. Vai trò Parent

Bottom navigation:

```txt
1. Trang chủ
2. Con của tôi
3. Học tập
4. Tài chính
5. Cá nhân
```

Routes đề xuất:

```txt
/parent/dashboard
/parent/children
/parent/grades
/parent/attendance
/parent/schedule
/parent/exams
/parent/tuition
/parent/notifications
/parent/profile
```

---

## 7. Mapping màn hình theo Excel của tổ trưởng

## 7.1. Student Mobile UI

| Màn hình | Mục tiêu | Layout | Component chính | API dự kiến | Mock trước |
|---|---|---|---|---|---|
| Dashboard | Nhìn nhanh tình hình cá nhân | AppBar + Greeting + KPI + Quick actions + Lịch hôm nay + Bài tập sắp hạn | `AppStatCard`, `TodayScheduleCard`, `UpcomingAssignmentCard`, `NotificationPreview` | `GET /me/dashboard` | Có |
| Thời khóa biểu | Xem lịch học | Calendar + list theo ngày/tuần | `TableCalendar`, `ScheduleCard`, filter tuần/ngày | `GET /schedule` | Có |
| Lịch thi | Xem lịch thi | List card + countdown | `ExamCard`, `CountdownBadge` | `GET /exams` | Có |
| Bảng điểm | Xem điểm/GPA | Semester tabs + subject cards + chart | `GradeCard`, `GpaChart`, `SemesterTab` | `GET /grades` | Có |
| Học phí | Theo dõi công nợ/thanh toán | Summary + invoice history + QR placeholder | `TuitionSummaryCard`, `InvoiceCard`, `PaymentButton` | `GET /tuition` | Có |
| Điểm danh | Theo dõi chuyên cần | Stats + attendance list | `AttendanceStatsCard`, `AttendanceRecordTile` | `GET /attendance` | Có |
| Khóa học | Học nội dung | Grid/list course cards | `CourseCard`, `ProgressBar` | `GET /courses` | Có |
| Chi tiết khóa học | Xem bài học | Header + lesson list + video/PDF placeholder + comments | `LessonTile`, `LearningProgress`, `CommentThread` | `GET /courses/:id` | Có |
| Bài tập | Theo dõi/nộp bài | Tabs: chưa nộp/đã nộp/quá hạn | `AssignmentCard`, `StatusBadge`, `UploadPanel` | `GET /assignments` | Có |
| Thông báo | Xem thông báo | Timeline | `NotificationTile`, `UnreadBadge` | `GET /notifications` | Có |
| Hồ sơ | Quản lý cá nhân | Profile header + form + settings | `ProfileHeader`, `InfoRow`, `ChangePasswordTile` | `GET/PUT /profile` | Có |

### Student Dashboard UX

Mục tiêu: mở app lên là biết ngay:

- Hôm nay học gì?
- Có bài nào sắp đến hạn?
- GPA hiện tại ra sao?
- Có cảnh báo học phí/điểm danh không?

Thứ tự ưu tiên UI:

```txt
Greeting
→ KPI 3-4 card
→ Lịch hôm nay
→ Bài tập sắp deadline
→ Thông báo mới
→ Quick actions
```

### Student Course Detail UX

Đây là màn quan trọng nhất của học sinh.

Bố cục mobile:

```txt
Course Header
Progress tổng
Lesson List dạng accordion
Lesson Player placeholder: video/pdf/text
Comment thread dưới lesson
```

Không cố nhét layout trái/phải kiểu desktop. Trên mobile chuyển thành stack dọc.

---

## 7.2. Parent Mobile UI

| Màn hình | Mục tiêu | Layout | Component chính | API dự kiến | Mock trước |
|---|---|---|---|---|---|
| Dashboard | Theo dõi nhanh tình hình con | Chọn học sinh + KPI + cảnh báo | `ChildSwitcher`, `ParentKpiCard`, `AlertCard` | `GET /parent/dashboard` | Có |
| Con của tôi | Danh sách con/em liên kết | List child cards | `ChildCard`, `PermissionBadge` | `GET /parent/profile` | Có |
| Kết quả học tập | Xem điểm/GPA của con | Subject cards + GPA summary | `GradeCard`, `GpaSummaryCard` | `GET /parent/grades` | Có |
| Điểm danh | Theo dõi chuyên cần | Stats + list | `AttendanceStatsCard`, `AttendanceRecordTile` | `GET /parent/attendance` | Có |
| Thời khóa biểu | Xem lịch học của con | Calendar + list | `ScheduleCard`, `ChildSwitcher` | `GET /parent/schedule` | Có |
| Lịch thi | Xem ca thi/phòng thi | List card | `ExamCard` | `GET /parent/exams` | Có |
| Học phí | Xem/Thanh toán học phí | Summary + invoice + QR placeholder | `TuitionSummaryCard`, `InvoiceCard` | `GET /parent/tuition` | Có |
| Thông báo | Cảnh báo hệ thống | Timeline | `NotificationTile` | `GET /parent/notifications` | Có |
| Hồ sơ | Thông tin phụ huynh | Form + quyền truy cập | `ProfileHeader`, `LinkedStudentList` | `GET /parent/profile` | Có |

### Parent Dashboard UX

Phụ huynh không cần nhiều nghiệp vụ học thuật phức tạp. Màn chính cần rõ:

```txt
Chọn con
→ Điểm học tập
→ Chuyên cần
→ Lịch học hôm nay
→ Công nợ học phí
→ Cảnh báo mới
```

Nếu phụ huynh có nhiều con, luôn giữ `ChildSwitcher` ở đầu các màn học tập/tài chính.

---

## 8. Component system cần code

Tạo trong:

```txt
lib/core/widgets/
```

Danh sách component bắt buộc:

```txt
AppTopBar
AppBottomNav
AppStatCard
AppSectionHeader
AppStatusBadge
AppPrimaryButton
AppSecondaryButton
AppEmptyState
AppErrorState
AppLoadingSkeleton
AppSearchField
AppFilterChip
AppInfoRow
ProgressBar
```

Component theo nghiệp vụ:

```txt
CourseCard
LessonTile
AssignmentCard
ExamCard
ScheduleCard
AttendanceStatsCard
AttendanceRecordTile
GradeCard
GpaChart
TuitionSummaryCard
InvoiceCard
NotificationTile
ChildSwitcher
ChildCard
ProfileHeader
```

---

## 9. Mock data bắt buộc có

Tạo mock đủ để demo đẹp, không để màn hình trống.

### Student mock

```txt
- 4 khóa học
- 5 lịch học tuần này
- 3 bài tập: sắp hạn, đã nộp, quá hạn
- 3 lịch thi
- 6 môn có điểm
- 10 bản ghi điểm danh
- 4 hóa đơn học phí
- 8 thông báo
```

### Parent mock

```txt
- 2 học sinh được liên kết
- Mỗi học sinh có GPA, attendance, học phí, lịch học, lịch thi riêng
- 5 cảnh báo/thông báo
```

---

## 10. API Client không đụng Backend

Tạo `ApiClient` chuẩn nhưng chưa bắt buộc gọi API thật.

```dart
class ApiClient {
  final Dio dio;

  ApiClient(this.dio);
}
```

Base URL lấy từ dart define:

```bash
flutter run --dart-define=API_BASE_URL=http://localhost:5000/api
```

Trong code:

```dart
const apiBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'http://10.0.2.2:5000/api',
);
```

Lưu ý Android emulator dùng `10.0.2.2`, không dùng `localhost`.

---

## 11. Trạng thái UI bắt buộc

Mỗi màn phải có đủ:

```txt
Loading
Loaded
Empty
Error
Pull-to-refresh nếu là danh sách
```

Ví dụ:

```dart
AsyncValue.when(
  loading: () => AppLoadingSkeleton(),
  error: (error, stack) => AppErrorState(message: error.toString()),
  data: (data) => ...,
)
```

---

## 12. Chuẩn UI/UX hiện đại cần áp dụng

### 12.1. Style tổng thể

```txt
Clean Education App
Material 3
Card-based dashboard
Large rounded cards
Soft background
High readability
Light + Dark mode
Không dùng màu quá chói
Không dùng gradient quá nhiều
```

### 12.2. Trải nghiệm thao tác

```txt
- Bottom nav rõ ràng
- Mỗi màn chỉ 1 mục tiêu chính
- CTA nổi bật nhưng không quá nhiều
- Dữ liệu quan trọng đặt trên cùng
- Badge trạng thái dễ hiểu
- Pull to refresh cho dashboard/list
- Skeleton loading thay vì spinner đơn điệu
- Empty state có hướng dẫn tiếp theo
```

### 12.3. Accessibility

```txt
- Touch target >= 44px
- Text chính tối thiểu 14px
- Contrast tốt trên nền sáng/tối
- Không dùng màu làm tín hiệu duy nhất; badge phải có chữ
- Form có label rõ ràng
```

---

## 13. Các màn nên ưu tiên làm trước

### Phase 1: Nền móng

```txt
1. Tạo Flutter project
2. Setup theme
3. Setup router
4. Setup mock repositories
5. Setup shell Student/Parent
6. Setup bottom navigation
```

### Phase 2: Student MVP

```txt
1. Student Dashboard
2. Student Schedule
3. Student Courses
4. Student Assignments
5. Student Grades
6. Student Attendance
7. Student Tuition
8. Student Notifications
9. Student Profile
```

### Phase 3: Parent MVP

```txt
1. Parent Dashboard
2. Parent Children
3. Parent Grades
4. Parent Attendance
5. Parent Schedule
6. Parent Exams
7. Parent Tuition
8. Parent Notifications
9. Parent Profile
```

### Phase 4: Kết nối API sau

```txt
1. Chuyển mock repository sang remote repository từng màn
2. Gắn login thật
3. Lưu token bằng flutter_secure_storage
4. Dio interceptor attach Bearer token
5. Refresh token nếu Backend có
```

---

## 14. Prompt giao trực tiếp cho AI coding agent

Copy toàn bộ prompt này vào Cursor/Claude/Codex sau khi đặt file `.md` này trong repo:

```txt
Bạn là Senior Flutter Mobile Engineer + UI/UX Designer. Hãy tạo app Flutter riêng cho LMS trong repo này, nhưng CHỈ được làm trong thư mục `lms_mobile/`. Tuyệt đối không sửa Backend, không sửa migration, không sửa controller, không sửa service, không sửa database, không sửa frontend Vue hiện có.

Yêu cầu:
1. Nếu chưa có `lms_mobile/`, hãy tạo app Flutter riêng bằng cấu trúc chuẩn.
2. Dùng Flutter 3, Riverpod, Dio, GoRouter, Material 3, Google Fonts Inter.
3. Thiết kế app mobile LMS cho 2 vai trò: Student và Parent theo file Excel `LMS_Mobile_UI_Spec.xlsx`.
4. Dùng mock data trước để tất cả màn hình chạy được, đẹp, có dữ liệu demo. Không yêu cầu Backend phải có đủ API.
5. Tạo repository interface để sau này đổi mock sang API thật.
6. Tạo design system gồm màu #2563EB, #22C55E, #EF4444, #F59E0B, background #F8FAFC; card radius 16, button radius 12, bottom sheet radius 24, spacing 8/16/24, font Inter.
7. Tạo bottom navigation cho Student: Trang chủ, Học tập, Lịch, Kết quả, Cá nhân.
8. Tạo bottom navigation cho Parent: Trang chủ, Con của tôi, Học tập, Tài chính, Cá nhân.
9. Code các màn Student: Dashboard, Courses, Course Detail, Assignments, Exam Schedule, Grades, Schedule, Attendance, Tuition, Notifications, Profile.
10. Code các màn Parent: Dashboard, Children, Grades, Attendance, Schedule, Exam Schedule, Tuition, Notifications, Profile.
11. Mỗi màn phải có Loading, Empty, Error, Loaded state.
12. UI phải mobile-first, sạch, hiện đại, giống app giáo dục chuyên nghiệp, ưu tiên card, badge, chart, calendar, timeline.
13. Không gọi API thật nếu chưa chắc endpoint tồn tại. Nếu endpoint thiếu, ghi chú vào `lms_mobile/docs/api_needed_for_mobile.md`.
14. Sau khi code xong, chạy `flutter analyze` và sửa lỗi trong phạm vi `lms_mobile/`.
15. Không format hay sửa file ngoài `lms_mobile/`.

Kết quả mong đợi:
- App Flutter chạy được.
- Có navigation Student/Parent.
- Có mock data đầy đủ.
- UI đẹp, thống nhất, dễ demo với trưởng nhóm.
- Backend không bị thay đổi.
```

---

## 15. Checklist kiểm tra sau khi AI code

Sau khi AI làm xong, kiểm tra:

```bash
cd lms_mobile
flutter pub get
flutter analyze
flutter test
flutter run
```

Kiểm tra Git diff:

```bash
git status
git diff --stat
```

Diff hợp lệ chỉ nên nằm trong:

```txt
lms_mobile/
```

Nếu thấy các file này bị sửa thì phải revert ngay:

```txt
Backend/**
frontend/**
Source Code.sln
```

Lệnh revert ví dụ:

```bash
git checkout -- Backend frontend "Source Code.sln"
```

---

## 16. Định nghĩa hoàn thành

Dự án Flutter mobile được xem là đạt bản demo khi:

```txt
[ ] App chạy được trên Android emulator hoặc Chrome/mobile size.
[ ] Có màn chọn vai trò hoặc login mock.
[ ] Vào được Student shell.
[ ] Vào được Parent shell.
[ ] Dashboard Student có KPI, lịch hôm nay, bài tập, thông báo.
[ ] Dashboard Parent có chọn học sinh, điểm, chuyên cần, công nợ, cảnh báo.
[ ] Courses/Assignments/Grades/Schedule/Attendance/Tuition có dữ liệu mock.
[ ] Có theme thống nhất, card đẹp, font Inter.
[ ] Có empty/loading/error state.
[ ] Không sửa Backend.
[ ] Có `api_needed_for_mobile.md` ghi endpoint cần Backend bổ sung sau.
```

---

## 17. Ghi chú cho báo cáo với trưởng nhóm

Có thể trình bày hướng làm như sau:

```txt
Em sẽ không sửa Backend hiện tại. Em tạo app Flutter riêng trong thư mục `lms_mobile/`, dùng Riverpod + Dio + GoRouter theo đúng Excel. Giai đoạn đầu em dựng UI/UX và mock data để demo đầy đủ Student/Parent. Khi Backend hoàn thiện API, em chỉ đổi repository từ mock sang remote bằng Dio, không ảnh hưởng cấu trúc app và không động vào BE.
```

