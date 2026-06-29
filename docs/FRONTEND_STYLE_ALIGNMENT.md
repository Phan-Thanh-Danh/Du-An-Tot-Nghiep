# FRONTEND STYLE ALIGNMENT

Tài liệu này xác định nguyên tắc mở rộng giao diện (UI) cho dự án EduLMS, đặc biệt là khi triển khai 4 module: Đơn từ, Trung tâm thông báo, Khen thưởng/Kỷ luật, và Thời khóa biểu/Điểm danh. 

**Nguyên tắc cốt lõi: Mở rộng dựa trên `liquid-glass.css` hiện tại, KHÔNG thiết kế lại (No Redesign), KHÔNG thêm design system mới.**

---

## 1. Current UI Language (Phân tích style hiện tại)

Dự án đã định hình sẵn một ngôn ngữ thiết kế "Liquid Glass" rất rõ ràng tại `frontend/src/assets/liquid-glass.css`.
- **Màu chính (Primary Tokens)**: Sử dụng các biến `--lg-primary` (Blue/Indigo), `--lg-secondary` (Teal), `--lg-accent` (Violet), `--lg-cyan`.
- **Glass Surface**: Style chủ đạo là thẻ kính (glassmorphism) dùng `--lg-surface-glass` (nền trong suốt, có độ mờ blur 12px) hoặc `--lg-surface-glass-strong`.
- **Background Page**: Trang web dùng gradient nền có orb/blob lơ lửng (`.lg-app-bg`, `.lg-blob`).
- **Border/Radius/Shadow**: Các góc bo tròn từ `md` (12px) đến `xl` (18px) tuỳ thành phần. Shadow dùng `.lg-shadow-sm` hoặc `.lg-shadow-md` nhẹ nhàng.
- **Text/Typography**: Phân cấp rõ ràng qua `--text-heading`, `--text-body`, `--text-muted`. Màu text fallback trong dark mode cũng đã được xử lý (tránh lỗi chữ đen trên nền tối).
- **Thẻ Card & Badge**: Component trạng thái dùng semantic colors: `--color-success-bg`/`text`, `--color-warning-bg`/`text`, `--color-danger-bg`/`text`.
- **Icon**: Sử dụng thư viện `lucide-vue-next` đồng nhất.
- **Layout**: Dùng App Shell (`Layout_SinhVien.vue`) chia sẵn Sidebar (trái) và Topbar (trên).
- **Dark Mode**: Đã được xử lý toàn diện bằng thẻ `:root.dark` trong `liquid-glass.css`.

---

## 2. Phân Tích Component Hiện Có

Hiện tại thư mục `frontend/src/components/ui/` đã có sẵn các component theo chuẩn liquid-glass:
- **`GlassButton.vue`**: Nút bấm chuẩn, hỗ trợ loading state, các variant (primary, secondary, danger, ghost, subtle, success). **BẮT BUỘC dùng component này cho mọi nút bấm**. Không tạo thêm `AppButton`.
- **`GlassBadge.vue`**: Hiển thị trạng thái/nhãn với các variant (primary, success, warning, danger, info, violet). **BẮT BUỘC dùng component này cho mọi trạng thái**. Không tạo thêm `AppStatusBadge`.
- **`GlassPanel.vue`**: Vỏ bọc card cho các section, form, list. Hỗ trợ variant (glass, solid, strong, soft, readable). **BẮT BUỘC dùng component này làm layout card**. Không tạo thêm `AppCard`.
- **`GlassInput.vue`**: Text input chuẩn.
- **`TableShell.vue`**: Khung bao bảng dữ liệu (Table), có xử lý bo góc, background cho table row.
- **`LoadingSkeleton.vue` & `ProgressBar.vue`**: Cho các trạng thái chờ.

**Nguyên tắc:**
- **Sử dụng lại 100%**: Khi xây dựng UI cho module mới, phải import từ `components/ui/`.
- **Không tạo trùng**: Nếu cần 1 nút, dùng `GlassButton`. Nếu cần 1 thẻ chứa thông tin, dùng `GlassPanel`.
- **Chỉ mở rộng khi thiếu**: Nếu cần 1 Select/Dropdown phức tạp, có thể mở rộng từ input hiện có hoặc thêm component mới nhưng phải áp dụng class như `.lg-input`, `.lg-surface-dropdown`.

---

## 3. Quy Tắc Mở Rộng Style

Khi phát triển các chức năng mới, lập trình viên frontend phải tuân thủ:
1. **Tuyệt đối không hardcode hex color**: Mọi màu nền, màu chữ, viền đều phải dùng class chuẩn hoặc biến CSS (ví dụ: `bg-[var(--surface-card)]` hoặc class `.lg-surface-card`).
2. **Tuân thủ Spacing**: Dùng padding chuẩn Tailwind (VD: `p-4`, `p-6`) kết hợp biến spacing của thiết kế (`var(--card-padding-md)`).
3. **Màu sắc mang tính Module (Accent)**: Mỗi module có thể có màu đặc trưng (VD: Khen thưởng dùng Vàng/Amber) nhưng chỉ áp dụng làm màu điểm xuyết (Accent/Badge/Icon), không làm đổi màu nền trang web hay phá vỡ màu xanh dương/kính chủ đạo.
4. **Trạng thái (Loading/Error/Empty)**: 
   - Loading: Phải dùng `LoadingSkeleton.vue` hoặc `GlassButton` có cờ `loading`.
   - Empty/Error: Dùng `EmptyState.vue` hoặc `PopupNotification.vue`. Không dùng alert mặc định của trình duyệt (`alert()`).

---

## 4. Module-specific Style Mapping (Hướng Dẫn Triển Khai 4 Module)

### 4.1. Đơn từ (Applications)
- **Form Đơn**: Nằm gọn trong một `GlassPanel` (variant="readable" hoặc "solid").
- **Timeline/Trạng thái**: Trạng thái duyệt của đơn hiển thị bằng `GlassBadge` (vd: Pending -> warning, Approved -> success, Rejected -> danger).
- **Upload Minh chứng**: Hiển thị dưới dạng một box kéo thả hoặc nút `GlassButton` "Upload", có icon `UploadCloud`. Cảm giác thao tác nhẹ nhàng, dễ chịu.
- **Admin View**: Danh sách đơn dùng `TableShell` kết hợp các filter ở phía trên (trong `GlassPanel` nhỏ).

### 4.2. Trung tâm thông báo (Notifications)
- **Inbox/List**: Dùng list xếp dọc, mỗi item là một `GlassPanel` dạng hover-able (interactive). 
- **Urgency (Độ khẩn)**: Thông báo khẩn dùng `GlassBadge` màu đỏ (danger) kèm icon `AlertCircle` ở góc. 
- **Admin Compose**: Trình soạn thảo (Editor.js) nằm trong một `GlassPanel` lớn. Preview box nằm bên cạnh mô phỏng giao diện điện thoại/card.

### 4.3. Khen thưởng / Kỷ luật (Rewards & Discipline)
- **Khen thưởng (Student View)**: Nổi bật bằng `GlassPanel` có hiệu ứng `glow="true"`. Tích hợp icon `Award`, `Trophy`. Badge dùng màu Gold/Amber. Nút tải chứng nhận dùng `GlassButton` (primary hoặc success).
- **Kỷ luật (Student View)**: Giao diện nghiêm túc. Dùng `GlassPanel` (variant="strong" hoặc "solid"). Trạng thái dùng `GlassBadge` (danger hoặc secondary). Action "Khiếu nại" cần được làm rõ bằng `GlassButton` (secondary).
- **Admin View**: Danh sách hàng loạt dùng `TableShell`. Có tính năng tick chọn (bulk action).

### 4.4. Thời khóa biểu & Điểm danh (Schedule & Attendance)
- **Schedule**: Không sử dụng thư viện lịch quá nặng. Tự custom Grid hoặc List view. Mỗi ca học là một thẻ (dùng `div` nhỏ có border và màu nền nhạt từ biến `--accent-primary-soft`). Buổi bị hủy đánh dấu bằng `GlassBadge` (danger).
- **Attendance (Giảng viên)**: Giao diện Grid list điểm danh. Phải cực kỳ nhanh cho thao tác bấm. Tích xanh (Có mặt) làm mặc định. Chỉ thao tác chuyển khi Vắng/Đi muộn. Sử dụng `GlassButton` size `sm`.
- **Timer/Lock**: Đếm ngược (10/15 phút điểm danh) hiển thị trên một `GlassPanel` sticky nhỏ ở góc. Khi quá giờ chuyển thẻ thành trạng thái `locked` (disabled opacity).

---

## 5. Library Policy (Chính sách Thư viện)

- **Không cài đặt thêm thư viện UI lớn** (như Vuetify, Element Plus, Ant Design) vì sẽ phá vỡ `liquid-glass.css` hiện có.
- **Không dùng shadcn-vue** làm nền tảng chính. Nếu thật sự cần một component phức tạp (như DatePicker cao cấp hay Dialog), chỉ copy mã nguồn và điều chỉnh CSS classes cho khớp với liquid-glass, KHÔNG import nguyên bộ theme mới.
- Khuyến nghị sử dụng các thư viện sẵn có trong `package.json`: 
  - `lucide-vue-next` cho mọi icon.
  - `dayjs` cho xử lý ngày giờ.
  - `xlsx` cho xuất/nhập file.
- **Bổ sung khi thật sự cần**:
  - Có thể thêm `@tanstack/vue-query` khi làm list/report nhiều (quản lý server state).
  - Có thể thêm `vee-validate` + `zod` nếu form yêu cầu validate động cực kỳ phức tạp.

---

**KẾT LUẬN**: 
Mọi PR frontend từ nay về sau (FE-APPLICATIONS, FE-NOTIFICATIONS, FE-REWARD, v.v.) đều **PHẢI** tuân thủ tuyệt đối cấu trúc component tại `frontend/src/components/ui/` và các class trong `liquid-glass.css`. Nghiêm cấm hành vi thiết kế lại (redesign) làm mất tính đồng bộ của dự án.
