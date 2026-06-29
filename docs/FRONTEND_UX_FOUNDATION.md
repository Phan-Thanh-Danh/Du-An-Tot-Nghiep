# FRONTEND UX FOUNDATION

Tài liệu này xác định nền tảng UI/UX, chiến lược component, thiết kế theo role và hướng dẫn triển khai cho toàn bộ hệ thống EduLMS, đặc biệt chuẩn bị cho 4 module: Đơn từ, Thông báo, Khen thưởng/Kỷ luật, và Thời khóa biểu/Điểm danh.

## PHẦN A — Current-State Audit
### 1. Hiện trạng (What exists & is usable)
- **Stack & Tooling**: Vue 3, Vite, Tailwind CSS v4, Vue Router, Pinia, Lucide Icons, VueUse, Day.js. Các thư viện này đã được cài đặt và hoạt động trơn tru.
- **Routing**: `src/router/index.js` đã chia thành 6 phân hệ chính: `Student`, `Parent`, `Teacher`, `Staff` (Giáo vụ), `BGH` (Hiệu trưởng), và `SuperAdmin`.
- **Auth Guard**: Đã có `auth.js` store hỗ trợ mock login, kiểm tra role và expire token hợp lý. `routeRequiresAuthentication` và role checking đang hoạt động tốt.
- **API Client**: `apiClient.js` cấu trúc tốt, xử lý error và token header gọn gàng.
- **Layouts**: Đã có App Shell layouts chia theo role (ví dụ: `Layout_SinhVien.vue`, `Layout_GiangVien.vue`).

### 2. Vấn đề cần khắc phục (What should be refactored later)
- **Component & View Naming Inconsistency**: Hiện tại thư mục và file đang bị trộn lẫn tiếng Việt và tiếng Anh. 
  - Views: Có cả `views/SinhVien/`, `views/Student/`, `views/GiaoVu/`, `views/GiangVien/`.
  - Components: Có cả `components/SinhVien/`, `components/PhuHuynh/`.
  - *Hướng xử lý:* **Không phá vỡ route hiện tại**, nhưng khi tạo tính năng mới phải dùng chuẩn Tiếng Anh (VD: `components/applications`, `components/schedule`). Trong tương lai sẽ có 1 PR riêng để migrate các thư mục tiếng Việt sang tiếng Anh mà không làm gãy router.
- **Component Base Thiếu Tập Trung**: Đã có `LmsButton.vue`, `LmsCard.vue` nằm rải rác ở root `components/`. Cần gom vào `components/ui/` và đổi tên thành hệ thống `AppButton`, `AppCard`.

## PHẦN B — Role Model

### 1. Student (Sinh viên)
- **Mục tiêu chính**: Nắm bắt tiến độ học tập, lịch học hôm nay, và nhận thông báo khẩn.
- **Cần thấy đầu tiên**: Dashboard (Lịch học hôm nay, thông báo chưa đọc, GPA hiện tại).
- **Hành động thường dùng**: Xem TKB, điểm danh, tạo đơn từ, nộp bài, đọc thông báo.
- **Tone UI**: Rõ ràng, động viên, giảm áp lực, gam màu sáng, text to dễ đọc.

### 2. Teacher (Giảng viên)
- **Mục tiêu chính**: Dạy học, điểm danh, vào điểm, xử lý yêu cầu của lớp.
- **Cần thấy đầu tiên**: Lịch dạy hôm nay, các lớp cần điểm danh, bài cần chấm.
- **Hành động thường dùng**: Điểm danh (nhanh, ít click), nhập điểm.
- **Tone UI**: Nhanh, layout tối ưu cho thao tác lặp lại, màu xanh lá/trung tính.

### 3. Academic Staff / Campus Admin (Giáo vụ)
- **Mục tiêu chính**: Vận hành campus, xử lý đơn từ, sắp xếp TKB, giải quyết xung đột.
- **Cần thấy đầu tiên**: Dashboard vận hành (Đơn chờ duyệt, lớp thiếu GV, xung đột TKB).
- **Hành động thường dùng**: Filter danh sách, duyệt đơn, gửi thông báo, xếp lịch.
- **Tone UI**: Chuyên nghiệp, bảng dữ liệu chứa nhiều thông tin nhưng không rối, nút Bulk Action phải có confirm.

### 4. Principal (Ban Giám Hiệu) & SuperAdmin
- **Mục tiêu chính**: Giám sát tổng quan, quản trị hệ thống, cấp quyền, cấu hình.
- **Cần thấy đầu tiên**: Biểu đồ thống kê, cảnh báo rủi ro, thông số toàn campus.
- **Hành động thường dùng**: Xem report, switch campus (nếu là SuperAdmin).
- **Tone UI**: Dashboard quyền lực, dùng không gian rộng (wide layout), chart rõ ràng.

### 5. Parent (Phụ huynh)
- **Mục tiêu chính**: Xem điểm, lịch học, học phí, điểm danh của con.
- **Hành động thường dùng**: Read-only dữ liệu, nhận thông báo.
- **Tone UI**: Tối giản, bảo mật, rõ ràng.

## PHẦN C — Design Principles
1. **Academic but modern**: Hiện đại nhưng không lòe loẹt. Dùng glassmorphism cực nhẹ hoặc solid color hiện đại.
2. **Actionable Dashboards**: Mọi widget trên dashboard phải click được hoặc giải quyết được 1 câu hỏi tức thì.
3. **No Overwhelming Tables**: Bảng dữ liệu có filter, sticky header. Không hiện 50 cột nếu user chỉ cần 5.
4. **State Transparency**: Mọi module phải có trạng thái rõ ràng: Loading (Skeleton), Error (Alert), Empty (Illustration/Text + Call to Action), Permission Denied.
5. **Safe Destructive Actions**: Xóa/Hủy/Từ chối phải luôn có Confirm Dialog.

## PHẦN D — Design Tokens (Tailwind v4 CSS Variables)
Tailwind v4 hỗ trợ `@theme` trong CSS.

**Spacing & Radius:**
- Base spacing: 4px (p-1 = 4px, p-4 = 16px).
- Card padding: `p-5` (20px) hoặc `p-6` (24px).
- Radius: `rounded-lg` (8px) cho input, `rounded-xl` (12px) hoặc `rounded-2xl` (16px) cho Card/Modal.

**Typography:**
- Font: Inter hoặc Roboto (System font stack fallback).
- Sizes: Page Title (`text-2xl font-bold`), Section Title (`text-lg font-semibold`), Body (`text-sm` hoặc `text-base`).

**Colors:**
- Primary: Blue/Indigo (LMS Core).
- Success: Emerald (`bg-emerald-50 text-emerald-700`).
- Warning: Amber (`bg-amber-50 text-amber-700`).
- Danger: Rose/Red (`bg-rose-50 text-rose-700`).
- Background: `bg-slate-50` cho layout tổng, `bg-white` cho thẻ Card.
- Border: `border-slate-200`.

## PHẦN E — Component System
Gom toàn bộ UI core vào `src/components/ui/`:
- `AppButton.vue`: Solid, outline, ghost, link variants. Hỗ trợ loading icon.
- `AppCard.vue`: Card trắng, bo góc, border nhẹ, có slot header/body/footer.
- `AppStatusBadge.vue`: Semantic colors (success, warning, danger, neutral).
- `AppInput.vue` / `AppTextarea.vue` / `AppSelect.vue`.
- `AppTable.vue`: Bảng chuẩn hóa.
- `AppModal.vue`: Dialog chuẩn, focus trap.
- `AppEmptyState.vue`: Trạng thái rỗng.

Component nghiệp vụ đặt tại:
- `src/components/applications/`
- `src/components/notifications/`
- `src/components/rewards-discipline/`
- `src/components/schedule/`
- `src/components/attendance/`

## PHẦN F — Layout Strategy
Mỗi Role sẽ bọc trong một Wrapper chuyên biệt:
- **Main Width**: `max-w-7xl` cho Student (dễ đọc), `max-w-[1600px]` hoặc `w-full` cho Admin/Staff (để chứa bảng).
- **Sidebar**: Toggle được trên mobile, cố định trên Desktop.
- **Header**: Breadcrumbs, Notification bell, User dropdown.

## PHẦN G — Module UX Direction

### 1. Đơn từ (Applications)
- **Student**: Wizard tạo đơn step-by-step. Timeline trạng thái dọc (Draft -> Pending -> Approved -> Completed).
- **Staff/Admin**: Kanban hoặc Queue List để phân công. Cửa sổ detail có split screen (Bên trái: thông tin/minh chứng, Bên phải: Action/Chat log).

### 2. Trung tâm thông báo (Notifications)
- **Student**: Inbox drawer hoặc trang riêng. Indicator màu đỏ cho thẻ Urgent. Nút "Mark all as read".
- **Admin**: Flow tạo thông báo 3 bước: Chọn Template -> Viết nội dung (Editor.js) -> Chọn người nhận & Preview -> Gửi.

### 3. Khen thưởng / Kỷ luật (Rewards & Discipline)
- **Student Reward**: Giao diện vinh danh, rực rỡ nhẹ (Gold/Amber/Emerald). Nút download chứng nhận (PDF) to, nổi bật.
- **Student Discipline**: Màn hình Private, bảo mật, gam màu trung tính/đỏ nhạt, không mang tính "sỉ nhục". Có CTA khiếu nại (Appeal) rõ ràng.
- **Admin**: Dashboard sinh list PDF hàng loạt. Filter mạnh để tìm kiếm sinh viên.

### 4. Thời khóa biểu & Điểm danh (Schedule & Attendance)
- **Student**: Calendar view theo tuần, dùng màu khác biệt cho lớp Bù/Dời ca.
- **Teacher**: Điểm danh dạng Grid list. Tích xanh (Có mặt) mặc định, chỉ click khi Vắng/Đi muộn. Có lock indicator (Đếm ngược 10/15 phút).
- **Admin**: Drag & drop (sau này) hoặc form chỉnh sửa ca, cảnh báo xung đột (Conflict) bằng banner đỏ rực trước khi Publish.

## PHẦN H — API Client Strategy
Chuyển từ việc gọi API hỗn loạn sang các module-based services trong `src/services/`:
- `applicationsApi.js`
- `notificationsApi.js`
- `rewardsDisciplineApi.js`
- `scheduleApi.js`
- `attendanceApi.js`
Quy tắc:
1. Return thẳng `data` object từ request.
2. Xử lý try/catch ở component hoặc store.
3. Handle 401/403 tự động ở interceptor (nếu có) hoặc báo lỗi qua Toast.

## PHẦN I — Library Adoption Recommendation
1. **Shadcn-vue** (Cân nhắc): Tích hợp có chọn lọc, copy paste các components khó (như Dialog, Tabs, Select, DatePicker) thay vì tự code từ đầu, sau đó tuỳ biến theo Tailwind v4 của dự án.
2. **@tanstack/vue-query** (Khuyến nghị cao): Cực kỳ cần thiết cho TKB, list đơn từ, list thông báo để quản lý caching, loading, error state và pagination chuẩn chỉ.
3. **Vee-validate & Zod** (Khuyến nghị): Dùng form validation cho Đơn từ và TKB form để tránh rác state.
4. KHÔNG cài component lib nguyên khối như Element Plus hay Vuetify để tránh phình to và khó custom Tailwind.

## PHẦN J — Accessibility (a11y) Checklist
- [x] Text color contrast luôn > 4.5:1.
- [x] Nút bấm (Button) phải có focus ring (vd: `focus:ring-2 focus:ring-blue-500 focus:outline-none`).
- [x] Icon-only buttons (như nút X đóng modal) phải có `aria-label="Đóng"`.
- [x] Trạng thái lỗi Form không chỉ dùng màu đỏ, mà phải kèm text báo lỗi rõ ràng.

## PHẦN K — Responsive Rules
- Màn hình `< 768px` (Mobile): Menu chuyển thành Drawer. Bảng (Table) tự cuộn ngang dọc hoặc biến thành Stacked Cards.
- Màn hình `>= 1024px` (Desktop): Sidebar luôn mở (expanded).

## PHẦN L — Rủi Ro & Non-goals
- **Rủi ro**: Quá nhiều component tiếng Việt hỗn loạn. 
- **Non-goals**: Không rewrite các component `views/SinhVien/` ngay lập tức nếu không liên quan tới 4 module cốt lõi để tránh breaking changes. Sẽ rewrite dần khi chạm tới tính năng.

---
*Tài liệu này là cam kết UX cho các PR tiếp theo (FE-APPLICATIONS-UI, FE-NOTIFICATION-UI, v.v).*
