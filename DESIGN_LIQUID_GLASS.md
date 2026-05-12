# DESIGN_LIQUID_GLASS.md

> **Mục đích**: File này là “source of truth” cho AI agent khi thiết kế giao diện theo phong cách **Liquid Glass UI** cho dự án LMS. Agent phải đọc file này trước khi sửa UI, tạo component, viết Tailwind class, hoặc thiết kế page mới.

---

## 0. Agent Usage Rules

Khi làm UI cho dự án này, agent phải tuân thủ:

1. Đọc file này trước khi code UI.
2. Không tự tạo style lệch khỏi token trong file này.
3. Không dùng glass quá đậm làm giảm readability.
4. Không dùng quá nhiều blur trên toàn màn hình.
5. Không copy giao diện từ sản phẩm khác.
6. Không thêm UI library mới nếu không cần.
7. Ưu tiên Tailwind CSS và CSS thuần.
8. Component phải có trạng thái: default, hover, focus, active, disabled, loading, error nếu phù hợp.
9. Giao diện phải responsive.
10. Giao diện phải đạt accessibility cơ bản: contrast, focus state, keyboard navigation, aria-label.

---

## 1. Design Identity

### 1.1 Tên phong cách

**LMS Liquid Glass Academic**

### 1.2 Mô tả ngắn

Phong cách giao diện hiện đại kết hợp:

- Glassmorphism
- Frosted glass
- Liquid surfaces
- Soft depth
- Gradient light
- Subtle motion
- Academic trust
- Premium SaaS dashboard

Mục tiêu là tạo UI **sạch, sang, mượt, có chiều sâu**, phù hợp hệ thống LMS/Academic Management System.

### 1.3 Cảm xúc cần truyền tải

Giao diện phải tạo cảm giác:

- Chuyên nghiệp
- Tin cậy
- Học thuật
- Cao cấp
- Bình tĩnh
- Tập trung
- Hiện đại
- Mượt mà
- Có chiều sâu nhưng không rối

### 1.4 Không khí thị giác

Liquid Glass UI trong dự án này **không phải** kiểu “trong suốt lòe loẹt”. Nó là hệ thống giao diện có các lớp:

```txt
Background gradient
→ Soft glowing blobs
→ Frosted glass surfaces
→ Solid readable cards
→ Elevated interactive controls
→ Clear text/content layer
```

---

## 2. Core Principles

### 2.1 Layering

Liquid Glass dựa trên nhiều lớp thị giác.

Các lớp chuẩn:

```txt
Layer 0: Base background
Layer 1: Ambient gradient / mesh glow
Layer 2: Decorative blur blobs
Layer 3: Glass container
Layer 4: Solid or semi-solid content card
Layer 5: Text, icons, buttons, inputs
Layer 6: Floating overlays / modals
```

Không đặt text quan trọng trực tiếp lên nền quá phức tạp. Text phải nằm trên surface đủ ổn định.

### 2.2 Transparency With Control

Glass không có nghĩa là “trong suốt càng nhiều càng tốt”.

Quy tắc:

```txt
Primary content surface opacity: 82% - 96%
Secondary glass surface opacity: 55% - 78%
Decorative glass opacity: 20% - 45%
```

Nội dung chính như form login, bảng điểm, dashboard card phải ưu tiên đọc rõ.

### 2.3 Blur As Depth, Not Decoration

Blur dùng để tạo chiều sâu, không dùng để khoe hiệu ứng.

Mức blur:

```txt
Subtle: 8px - 12px
Standard: 16px - 24px
Strong: 32px - 48px
Extreme decorative only: 64px - 96px
```

Không dùng `backdrop-blur-3xl` cho mọi card. Card nội dung nên dùng blur vừa phải.

### 2.4 Soft Contrast

Phong cách này dùng contrast mềm nhưng không được thiếu contrast.

Nên:

- Text chính rất rõ.
- Text phụ dịu hơn.
- Border nhẹ.
- Shadow mềm.
- Glow kiểm soát.

Không nên:

- Text xám nhạt trên nền glass sáng.
- Button mờ không rõ CTA.
- Border quá yếu khiến card mất hình khối.

### 2.5 Motion Feels Like Liquid

Motion nên:

- Chậm vừa phải
- Smooth
- Ease-out / spring nhẹ
- Có cảm giác trôi, nổi, mềm

Không nên:

- Bounce quá mạnh
- Nhấp nháy
- Parallax quá nặng
- Animation liên tục gây mất tập trung

---

## 3. Design Tokens

> Các token này là chuẩn mặc định cho dự án LMS. Agent chỉ được thay đổi nếu task yêu cầu rõ.

### 3.1 Color Tokens

```css
:root {
  /* Brand */
  --lg-primary: #1e3a8a;
  --lg-primary-strong: #172554;
  --lg-primary-soft: #dbeafe;

  --lg-secondary: #0f766e;
  --lg-secondary-soft: #ccfbf1;

  --lg-tertiary: #7c3aed;
  --lg-tertiary-soft: #ede9fe;

  /* Academic accent */
  --lg-cyan: #0891b2;
  --lg-blue: #2563eb;
  --lg-indigo: #4f46e5;
  --lg-violet: #7c3aed;

  /* Background */
  --lg-bg: #f8fafc;
  --lg-bg-soft: #f1f5f9;
  --lg-bg-deep: #e0f2fe;

  /* Surface */
  --lg-surface: rgba(255, 255, 255, 0.86);
  --lg-surface-strong: rgba(255, 255, 255, 0.94);
  --lg-surface-soft: rgba(255, 255, 255, 0.68);
  --lg-surface-dark: rgba(15, 23, 42, 0.72);

  /* Text */
  --lg-text: #0f172a;
  --lg-text-muted: #64748b;
  --lg-text-soft: #94a3b8;
  --lg-text-inverse: #ffffff;

  /* Border */
  --lg-border: rgba(148, 163, 184, 0.28);
  --lg-border-strong: rgba(100, 116, 139, 0.36);
  --lg-border-light: rgba(255, 255, 255, 0.56);

  /* Status */
  --lg-success: #16a34a;
  --lg-success-soft: #dcfce7;
  --lg-warning: #d97706;
  --lg-warning-soft: #fef3c7;
  --lg-danger: #dc2626;
  --lg-danger-soft: #fee2e2;
  --lg-info: #0284c7;
  --lg-info-soft: #e0f2fe;
}
```

### 3.2 Tailwind Color Mapping

Nếu dùng Tailwind trực tiếp, ưu tiên:

```txt
Primary: blue-900 / blue-800 / blue-700
Accent: cyan-600 / teal-700 / indigo-600 / violet-600
Background: slate-50 / blue-50 / indigo-50
Text: slate-950 / slate-800 / slate-600 / slate-400
Border: slate-200/60, white/60, white/40
Status: green-600, amber-600, red-600, sky-600
```

### 3.3 Typography Tokens

```css
:root {
  --lg-font-sans: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;

  --lg-h1-size: 40px;
  --lg-h1-line: 1.08;
  --lg-h1-weight: 800;
  --lg-h1-tracking: -0.035em;

  --lg-h2-size: 30px;
  --lg-h2-line: 1.16;
  --lg-h2-weight: 750;
  --lg-h2-tracking: -0.03em;

  --lg-h3-size: 22px;
  --lg-h3-line: 1.24;
  --lg-h3-weight: 700;
  --lg-h3-tracking: -0.02em;

  --lg-body-size: 16px;
  --lg-body-line: 1.6;
  --lg-body-weight: 400;

  --lg-label-size: 14px;
  --lg-label-line: 1.4;
  --lg-label-weight: 600;

  --lg-caption-size: 13px;
  --lg-caption-line: 1.4;
  --lg-caption-weight: 400;
}
```

### 3.4 Spacing Tokens

```css
:root {
  --lg-space-1: 4px;
  --lg-space-2: 8px;
  --lg-space-3: 12px;
  --lg-space-4: 16px;
  --lg-space-5: 20px;
  --lg-space-6: 24px;
  --lg-space-8: 32px;
  --lg-space-10: 40px;
  --lg-space-12: 48px;
  --lg-space-16: 64px;
  --lg-space-20: 80px;
}
```

### 3.5 Radius Tokens

```css
:root {
  --lg-radius-sm: 10px;
  --lg-radius-md: 14px;
  --lg-radius-lg: 20px;
  --lg-radius-xl: 28px;
  --lg-radius-2xl: 36px;
  --lg-radius-full: 9999px;
}
```

Tailwind mapping:

```txt
Input/Button: rounded-xl
Card: rounded-2xl / rounded-3xl
Hero/Login shell: rounded-[2rem]
Badge: rounded-full
Modal: rounded-[2rem]
```

### 3.6 Blur Tokens

```css
:root {
  --lg-blur-subtle: 8px;
  --lg-blur-sm: 12px;
  --lg-blur-md: 20px;
  --lg-blur-lg: 32px;
  --lg-blur-xl: 48px;
  --lg-blur-decorative: 80px;
}
```

Tailwind mapping:

```txt
Small glass: backdrop-blur-md
Standard glass: backdrop-blur-xl
Strong overlay: backdrop-blur-2xl
Decorative blob: blur-3xl
```

### 3.7 Shadow Tokens

```css
:root {
  --lg-shadow-sm: 0 8px 24px rgba(15, 23, 42, 0.08);
  --lg-shadow-md: 0 18px 48px rgba(15, 23, 42, 0.12);
  --lg-shadow-lg: 0 28px 80px rgba(15, 23, 42, 0.16);
  --lg-shadow-glow-blue: 0 0 60px rgba(37, 99, 235, 0.18);
  --lg-shadow-glow-cyan: 0 0 60px rgba(8, 145, 178, 0.16);
  --lg-shadow-inner: inset 0 1px 0 rgba(255, 255, 255, 0.65);
}
```

Quy tắc:

- Card thường: shadow-sm hoặc shadow-md.
- Login card / modal: shadow-lg.
- Button chính: shadow nhẹ + hover shadow-md.
- Không dùng shadow quá đen.

### 3.8 Border Tokens

```css
:root {
  --lg-border-width: 1px;
  --lg-border-glass: 1px solid rgba(255, 255, 255, 0.58);
  --lg-border-muted: 1px solid rgba(148, 163, 184, 0.24);
  --lg-border-focus: 2px solid rgba(37, 99, 235, 0.55);
}
```

---

## 4. Background System

### 4.1 Standard Page Background

Nền chuẩn cho page LMS:

```html
<div class="min-h-screen bg-[radial-gradient(circle_at_top_left,rgba(14,165,233,0.18),transparent_32%),radial-gradient(circle_at_top_right,rgba(124,58,237,0.16),transparent_30%),linear-gradient(180deg,#f8fafc_0%,#eef2ff_100%)]">
</div>
```

### 4.2 Login Background

Login nên có nền giàu chiều sâu hơn:

```txt
Base: slate-50 → blue-50 → indigo-50
Blob 1: cyan glow top-left
Blob 2: violet glow top-right
Blob 3: blue glow bottom
Noise: optional, opacity rất thấp
Glass panel: right or center
```

### 4.3 Decorative Blobs

Quy tắc blob:

```txt
Position: absolute
Pointer events: none
Opacity: 20% - 45%
Blur: 48px - 96px
Animation: very slow drift, 12s - 24s
```

Tailwind example:

```html
<div class="pointer-events-none absolute -left-24 top-20 h-72 w-72 rounded-full bg-cyan-300/30 blur-3xl"></div>
<div class="pointer-events-none absolute right-0 top-10 h-96 w-96 rounded-full bg-violet-300/25 blur-3xl"></div>
<div class="pointer-events-none absolute bottom-0 left-1/3 h-80 w-80 rounded-full bg-blue-300/20 blur-3xl"></div>
```

### 4.4 Noise Texture

Noise giúp glass tự nhiên hơn, nhưng phải cực nhẹ.

```css
.lg-noise::before {
  content: "";
  position: absolute;
  inset: 0;
  pointer-events: none;
  opacity: 0.035;
  background-image:
    radial-gradient(circle at 1px 1px, rgba(15, 23, 42, 0.28) 1px, transparent 0);
  background-size: 18px 18px;
}
```

Không dùng noise đậm trên form hoặc table.

---

## 5. Surface System

### 5.1 Glass Surface

Glass surface chuẩn:

```css
.lg-glass {
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.88),
    rgba(255, 255, 255, 0.64)
  );
  border: 1px solid rgba(255, 255, 255, 0.58);
  box-shadow:
    0 24px 72px rgba(15, 23, 42, 0.14),
    inset 0 1px 0 rgba(255, 255, 255, 0.65);
  backdrop-filter: blur(24px) saturate(145%);
  -webkit-backdrop-filter: blur(24px) saturate(145%);
}
```

### 5.2 Solid Readable Surface

Dùng cho nội dung nhiều chữ, bảng, form dài:

```css
.lg-solid-surface {
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(226, 232, 240, 0.86);
  box-shadow: 0 18px 48px rgba(15, 23, 42, 0.08);
}
```

### 5.3 Dark Glass Surface

Chỉ dùng cho hero/side panel, không dùng đại trà.

```css
.lg-dark-glass {
  background: linear-gradient(
    135deg,
    rgba(15, 23, 42, 0.82),
    rgba(30, 41, 59, 0.68)
  );
  border: 1px solid rgba(255, 255, 255, 0.12);
  color: white;
  backdrop-filter: blur(28px) saturate(140%);
}
```

### 5.4 Surface Usage

```txt
Login card: glass or solid-glass hybrid
Dashboard card: solid readable surface
Sidebar: glass surface with stronger opacity
Navbar: glass surface, sticky, blur-md/xl
Modal: strong solid/glass surface
Table container: solid surface, not transparent
Dropdown: glass surface, high opacity
Toast: glass surface, high opacity
```

---

## 6. Layout Patterns

### 6.1 Login Layout

Desktop:

```txt
Full-screen container
└── 2-column layout
    ├── Left visual/branding panel
    │   ├── Logo
    │   ├── Headline
    │   ├── LMS value proposition
    │   ├── Feature pills
    │   └── Academic stats
    └── Right login card
        ├── Title
        ├── Subtitle
        ├── Form
        ├── Remember/Forgot
        ├── Submit button
        └── Help/demo hint
```

Mobile:

```txt
Single column
└── Centered login card
    ├── Compact brand
    ├── Form
    └── Support links
```

### 6.2 Dashboard Layout

```txt
App Shell
├── Glass sidebar
├── Top navigation
└── Content area
    ├── Page heading
    ├── Summary cards
    ├── Main content grid
    └── Secondary widgets
```

### 6.3 Content Density

LMS có nhiều dữ liệu nên không dùng glass quá dày.

```txt
Marketing/login pages: more glass, more gradient
Dashboard: balanced glass + solid cards
Tables/forms: mostly solid readable surfaces
Modals/dropdowns: glass with high opacity
```

---

## 7. Component Patterns

## 7.1 Button

### Primary Button

```html
<button class="inline-flex items-center justify-center rounded-xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-900/20 transition-all duration-200 hover:-translate-y-0.5 hover:bg-blue-800 hover:shadow-xl hover:shadow-blue-900/25 focus:outline-none focus:ring-4 focus:ring-blue-500/25 disabled:translate-y-0 disabled:cursor-not-allowed disabled:opacity-60">
  Đăng nhập
</button>
```

Rules:

```txt
Primary CTA phải rõ, không quá trong suốt.
Hover có lift nhẹ.
Focus ring rõ.
Disabled giảm opacity, không hover.
Loading có spinner hoặc text “Đang xử lý...”.
```

### Secondary Button

```html
<button class="inline-flex items-center justify-center rounded-xl border border-white/60 bg-white/60 px-5 py-3 text-sm font-semibold text-slate-800 shadow-sm backdrop-blur-xl transition-all duration-200 hover:bg-white/80 hover:shadow-md focus:outline-none focus:ring-4 focus:ring-blue-500/20">
  Xem demo
</button>
```

### Ghost Button

```html
<button class="rounded-xl px-4 py-2 text-sm font-semibold text-slate-600 transition hover:bg-white/50 hover:text-slate-950 focus:outline-none focus:ring-4 focus:ring-blue-500/20">
  Quên mật khẩu?
</button>
```

---

## 7.2 Input

Input chuẩn:

```html
<div class="space-y-2">
  <label for="email" class="text-sm font-semibold text-slate-700">
    Email hoặc mã sinh viên
  </label>
  <div class="relative">
    <input
      id="email"
      type="text"
      autocomplete="username"
      class="w-full rounded-xl border border-slate-200/80 bg-white/80 px-4 py-3 text-sm text-slate-950 shadow-sm outline-none backdrop-blur transition-all duration-200 placeholder:text-slate-400 focus:border-blue-500/60 focus:bg-white focus:ring-4 focus:ring-blue-500/15"
      placeholder="Nhập email hoặc mã sinh viên"
    />
  </div>
</div>
```

Rules:

```txt
Input background phải đủ sáng.
Placeholder không quá đậm.
Focus ring mềm nhưng rõ.
Error state phải hiển thị text + border màu đỏ.
Không chỉ dùng màu để báo lỗi; cần message.
```

Error input:

```html
<input class="border-red-300 bg-red-50/60 focus:border-red-500 focus:ring-red-500/15" />
<p class="text-sm font-medium text-red-600">Vui lòng nhập mật khẩu.</p>
```

---

## 7.3 Card

### Standard LMS Card

```html
<section class="rounded-3xl border border-white/60 bg-white/82 p-6 shadow-lg shadow-slate-900/8 backdrop-blur-xl">
  ...
</section>
```

### Data Card

```html
<section class="rounded-2xl border border-slate-200/80 bg-white/95 p-5 shadow-sm">
  ...
</section>
```

### Interactive Card

```html
<article class="group rounded-3xl border border-white/60 bg-white/80 p-6 shadow-md backdrop-blur-xl transition-all duration-300 hover:-translate-y-1 hover:bg-white/90 hover:shadow-xl">
  ...
</article>
```

Rules:

```txt
Dashboard stats card có thể glass.
Table card nên solid.
Hover lift chỉ dùng cho card clickable.
Không animate mọi card liên tục.
```

---

## 7.4 Navbar

```html
<header class="sticky top-0 z-40 border-b border-white/50 bg-white/72 backdrop-blur-2xl">
  <div class="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
    ...
  </div>
</header>
```

Rules:

```txt
Navbar glass được phép.
Text/icon phải contrast rõ.
Nếu scroll trên content phức tạp, tăng opacity navbar.
```

---

## 7.5 Sidebar

```html
<aside class="h-screen w-72 border-r border-white/50 bg-white/72 p-4 shadow-xl shadow-slate-900/5 backdrop-blur-2xl">
  ...
</aside>
```

Active nav item:

```html
<a class="flex items-center gap-3 rounded-2xl bg-blue-900 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-900/20">
  Dashboard
</a>
```

Inactive nav item:

```html
<a class="flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium text-slate-600 transition hover:bg-white/70 hover:text-slate-950">
  Khóa học
</a>
```

---

## 7.6 Modal

```html
<div class="fixed inset-0 z-50 grid place-items-center bg-slate-950/35 p-4 backdrop-blur-sm">
  <section class="w-full max-w-lg rounded-[2rem] border border-white/60 bg-white/92 p-6 shadow-2xl backdrop-blur-2xl">
    ...
  </section>
</div>
```

Rules:

```txt
Backdrop overlay tối vừa đủ.
Modal surface phải dễ đọc.
Focus trap nếu implement được.
Esc/close button rõ.
```

---

## 7.7 Alert

Success:

```html
<div class="rounded-2xl border border-green-200 bg-green-50/85 p-4 text-sm text-green-800 shadow-sm backdrop-blur">
  Đăng nhập thành công.
</div>
```

Error:

```html
<div role="alert" class="rounded-2xl border border-red-200 bg-red-50/90 p-4 text-sm font-medium text-red-700 shadow-sm">
  Email hoặc mật khẩu không đúng.
</div>
```

Info:

```html
<div class="rounded-2xl border border-sky-200 bg-sky-50/85 p-4 text-sm text-sky-800">
  Hệ thống đang bảo trì lúc 22:00.
</div>
```

---

## 7.8 Badge / Pill

```html
<span class="inline-flex items-center rounded-full border border-blue-200 bg-blue-50/80 px-3 py-1 text-xs font-semibold text-blue-700">
  LMS
</span>
```

Glass pill:

```html
<span class="inline-flex items-center rounded-full border border-white/50 bg-white/50 px-3 py-1 text-xs font-semibold text-slate-700 backdrop-blur-xl">
  Học kỳ hiện tại
</span>
```

---

## 7.9 Table

Không dùng table trong suốt quá mạnh.

```html
<div class="overflow-hidden rounded-3xl border border-slate-200/80 bg-white/95 shadow-sm">
  <table class="w-full text-sm">
    <thead class="bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-500">
      ...
    </thead>
    <tbody class="divide-y divide-slate-100 text-slate-700">
      ...
    </tbody>
  </table>
</div>
```

Rules:

```txt
Table cần readability.
Không đặt table trên glass mờ.
Header bảng nên solid.
Row hover nhẹ.
```

---

## 7.10 Form Card

```html
<form class="rounded-[2rem] border border-white/60 bg-white/88 p-6 shadow-xl shadow-slate-900/10 backdrop-blur-2xl">
  ...
</form>
```

Rules:

```txt
Form phải có label rõ.
Input spacing đều.
Error hiện sát field.
Submit button nổi bật.
```

---

## 8. Login Page Specification

### 8.1 Visual Structure

Login page phải có:

```txt
- Full-screen background
- Soft academic gradient
- Floating blurred blobs
- Optional noise overlay
- Brand block
- Login card
- Smooth entrance animation
```

### 8.2 Required UI

Login card:

```txt
Logo/brand
Title: Đăng nhập LMS
Subtitle: Truy cập hệ thống học tập và quản lý đào tạo
Email/username field
Password field
Show/hide password button
Remember me checkbox
Forgot password link
Submit button
Loading state
Error state
Support/demo hint
```

### 8.3 Login Card Style

```txt
Width desktop: 420px - 520px
Padding desktop: 32px - 40px
Padding mobile: 24px
Radius: 28px - 36px
Blur: 20px - 32px
Surface opacity: 86% - 94%
Border: white/50 - white/70
Shadow: soft large
```

### 8.4 Login Motion

```css
@keyframes lg-enter {
  from {
    opacity: 0;
    transform: translateY(18px) scale(0.985);
    filter: blur(6px);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
    filter: blur(0);
  }
}

.lg-enter {
  animation: lg-enter 520ms cubic-bezier(0.16, 1, 0.3, 1) both;
}
```

### 8.5 Login Background Motion

```css
@keyframes lg-float {
  0%, 100% {
    transform: translate3d(0, 0, 0) scale(1);
  }
  50% {
    transform: translate3d(18px, -16px, 0) scale(1.04);
  }
}

.lg-float {
  animation: lg-float 18s ease-in-out infinite;
}
```

Respect reduced motion:

```css
@media (prefers-reduced-motion: reduce) {
  .lg-enter,
  .lg-float {
    animation: none;
  }

  * {
    scroll-behavior: auto !important;
  }
}
```

---

## 9. Motion System

### 9.1 Motion Principles

Motion phải:

```txt
Subtle
Smooth
Purposeful
Readable
Non-distracting
```

### 9.2 Duration Tokens

```css
:root {
  --lg-duration-fast: 150ms;
  --lg-duration-normal: 220ms;
  --lg-duration-slow: 360ms;
  --lg-duration-enter: 520ms;
  --lg-duration-decorative: 18000ms;
}
```

### 9.3 Easing Tokens

```css
:root {
  --lg-ease-out: cubic-bezier(0.16, 1, 0.3, 1);
  --lg-ease-in-out: cubic-bezier(0.65, 0, 0.35, 1);
  --lg-ease-soft: cubic-bezier(0.22, 1, 0.36, 1);
}
```

### 9.4 Component Motion

```txt
Button hover: 150ms - 220ms
Input focus: 150ms - 220ms
Card hover: 220ms - 300ms
Modal enter: 300ms - 420ms
Page enter: 420ms - 560ms
Background blob: 12s - 24s
```

### 9.5 Transform Rules

```txt
Button hover translateY: -1px to -2px
Card hover translateY: -2px to -4px
Modal enter scale: 0.98 → 1
Page enter translateY: 12px - 20px
```

Không dùng transform quá lớn cho UI học tập.

---

## 10. Accessibility Rules

### 10.1 Contrast

Text chính phải rõ.

```txt
Primary text: slate-950 / slate-900
Secondary text: slate-600
Muted text: slate-500 only on solid/light background
Never use slate-400 for important text
```

### 10.2 Focus State

Mọi control interactive phải có focus ring.

```html
focus:outline-none focus:ring-4 focus:ring-blue-500/20
```

### 10.3 Keyboard

Các component cần hỗ trợ:

```txt
Tab focus
Enter/Space activation
Esc close modal/dropdown
Visible focus state
No keyboard trap except modal with intentional focus trap
```

### 10.4 Reduced Motion

Mọi animation decorative phải tắt khi user bật reduced motion.

```css
@media (prefers-reduced-motion: reduce) {
  .animate-liquid,
  .lg-float,
  .lg-enter {
    animation: none !important;
    transition-duration: 0ms !important;
  }
}
```

### 10.5 Forms

Form phải có:

```txt
label liên kết input bằng for/id
aria-label cho icon button
role="alert" cho error
autocomplete phù hợp
disabled state rõ
```

### 10.6 Glass Accessibility

Không đặt text quan trọng trên:

```txt
blob gradient mạnh
image background phức tạp
glass surface opacity dưới 70%
blur quá nhiều nhưng nền vẫn nhiễu
```

Nếu nền phức tạp, tăng opacity surface hoặc thêm overlay trắng.

---

## 11. Performance Rules

### 11.1 Backdrop Filter

`backdrop-filter` tốn tài nguyên. Dùng có kiểm soát.

Quy tắc:

```txt
Không áp dụng backdrop-blur cho hàng trăm item.
Không dùng blur mạnh trong list dài.
Không animate blur liên tục.
Không đặt backdrop-filter trong table row.
```

Dùng blur cho:

```txt
Navbar
Sidebar
Login card
Modal
Dropdown
A few dashboard cards
```

Không dùng blur cho:

```txt
Every table row
Every list item
Large scrolling feed
Dense data grid
```

### 11.2 Animation Performance

Nên animate:

```txt
opacity
transform
```

Tránh animate:

```txt
width
height
top
left
blur
box-shadow quá phức tạp
```

### 11.3 Mobile Performance

Trên mobile:

```txt
Giảm số blob
Giảm blur
Giảm shadow
Tắt animation decorative nếu lag
```

CSS example:

```css
@media (max-width: 640px) {
  .lg-heavy-blur {
    backdrop-filter: blur(14px);
    -webkit-backdrop-filter: blur(14px);
  }

  .lg-mobile-hide-decor {
    display: none;
  }
}
```

---

## 12. Tailwind Utility Recipes

### 12.1 Glass Card

```txt
rounded-3xl border border-white/60 bg-white/80 shadow-xl shadow-slate-900/10 backdrop-blur-2xl
```

### 12.2 Solid Card

```txt
rounded-3xl border border-slate-200/80 bg-white/95 shadow-sm
```

### 12.3 Primary Button

```txt
rounded-xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-900/20 transition-all duration-200 hover:-translate-y-0.5 hover:bg-blue-800 hover:shadow-xl focus:outline-none focus:ring-4 focus:ring-blue-500/25 disabled:cursor-not-allowed disabled:opacity-60
```

### 12.4 Input

```txt
w-full rounded-xl border border-slate-200/80 bg-white/80 px-4 py-3 text-sm text-slate-950 shadow-sm outline-none backdrop-blur transition-all duration-200 placeholder:text-slate-400 focus:border-blue-500/60 focus:bg-white focus:ring-4 focus:ring-blue-500/15
```

### 12.5 Page Background

```txt
min-h-screen bg-[radial-gradient(circle_at_top_left,rgba(14,165,233,0.18),transparent_32%),radial-gradient(circle_at_top_right,rgba(124,58,237,0.16),transparent_30%),linear-gradient(180deg,#f8fafc_0%,#eef2ff_100%)]
```

### 12.6 Badge

```txt
inline-flex items-center rounded-full border border-blue-200 bg-blue-50/80 px-3 py-1 text-xs font-semibold text-blue-700
```

### 12.7 Error Alert

```txt
rounded-2xl border border-red-200 bg-red-50/90 p-4 text-sm font-medium text-red-700 shadow-sm
```

---

## 13. Vue Component Conventions

### 13.1 Component Naming

```txt
LoginView.vue
LiquidGlassCard.vue
LmsButton.vue
LmsInput.vue
LmsAlert.vue
LmsBadge.vue
StudentSummaryCard.vue
CourseProgressCard.vue
```

### 13.2 Component Structure

```vue
<script setup>
/**
 * Imports
 * Props
 * Emits
 * State
 * Computed
 * Methods
 */
</script>

<template>
  <!-- semantic HTML first, style second -->
</template>

<style scoped>
/* only component-specific CSS, prefer Tailwind utilities */
</style>
```

### 13.3 UI Logic

Không đặt API logic dài trong component.

Nên:

```txt
Component → Pinia store → API client → backend
```

Login:

```txt
LoginView.vue
→ useAuthStore().login()
→ auth.api.js
→ /api/auth/login
```

### 13.4 State Requirements

Mỗi form/page nên có:

```txt
loading
error
success if needed
empty if data page
disabled state
```

---

## 14. LMS-Specific UI Patterns

### 14.1 Student Dashboard

Dùng Liquid Glass nhẹ:

```txt
Hero welcome card: glass
Stats cards: glass/solid hybrid
Upcoming assignment card: solid
Schedule card: solid
Progress card: glass with chart
Notifications: solid readable
```

### 14.2 Course Card

Course card có thể dùng glass:

```txt
Thumbnail/gradient header
Course title
Teacher
Progress bar
Status badge
CTA
```

Progress bar:

```html
<div class="h-2 rounded-full bg-slate-200">
  <div class="h-2 rounded-full bg-gradient-to-r from-blue-700 to-cyan-500" style="width: 62%"></div>
</div>
```

### 14.3 Lesson Viewer

Không dùng glass cho nội dung bài học dài.

```txt
Lesson shell: subtle background
Content panel: white/solid
Sidebar lesson list: glass or solid
Video/PDF area: solid dark/light container
```

### 14.4 Assignment Submit

Form upload cần rõ ràng:

```txt
Dropzone: dashed border + glass light
Rubric: solid card
Submit button: primary solid
Error/success: clear alert
```

### 14.5 Gradebook

Table cần solid, không glass quá nhiều.

```txt
Container: white/95
Header: slate-50
Rows: white
Hover: slate-50
Status badges: colored soft
```

---

## 15. Do / Don't

### 15.1 Do

- Dùng glass cho container lớn, navbar, card quan trọng.
- Dùng solid surface cho nội dung dài.
- Giữ text contrast cao.
- Dùng gradient nhẹ.
- Dùng shadow mềm.
- Dùng blur có kiểm soát.
- Dùng animation chậm, mượt.
- Có focus states.
- Có responsive states.
- Có loading/error/empty states.
- Dùng token nhất quán.

### 15.2 Don't

- Không làm toàn bộ UI trong suốt.
- Không dùng blur quá mạnh ở mọi nơi.
- Không đặt text nhỏ lên gradient.
- Không dùng shadow đen nặng.
- Không dùng quá nhiều màu neon.
- Không animate liên tục các phần quan trọng.
- Không làm form khó đọc.
- Không bỏ qua accessibility.
- Không dùng `backdrop-filter` trên list/table lớn.
- Không copy style Apple/Microsoft y nguyên; chỉ học nguyên lý.

---

## 16. Implementation CSS Snippet

Có thể thêm vào `frontend/src/assets/liquid-glass.css` hoặc `frontend/src/assets/main.css`.

```css
:root {
  --lg-primary: #1e3a8a;
  --lg-primary-soft: #dbeafe;
  --lg-secondary: #0f766e;
  --lg-tertiary: #7c3aed;
  --lg-bg: #f8fafc;
  --lg-text: #0f172a;
  --lg-text-muted: #64748b;
  --lg-border: rgba(148, 163, 184, 0.28);
}

.lg-page-bg {
  background:
    radial-gradient(circle at top left, rgba(14, 165, 233, 0.18), transparent 32%),
    radial-gradient(circle at top right, rgba(124, 58, 237, 0.16), transparent 30%),
    linear-gradient(180deg, #f8fafc 0%, #eef2ff 100%);
}

.lg-glass {
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.88),
    rgba(255, 255, 255, 0.64)
  );
  border: 1px solid rgba(255, 255, 255, 0.58);
  box-shadow:
    0 24px 72px rgba(15, 23, 42, 0.14),
    inset 0 1px 0 rgba(255, 255, 255, 0.65);
  backdrop-filter: blur(24px) saturate(145%);
  -webkit-backdrop-filter: blur(24px) saturate(145%);
}

.lg-solid {
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(226, 232, 240, 0.86);
  box-shadow: 0 18px 48px rgba(15, 23, 42, 0.08);
}

.lg-enter {
  animation: lg-enter 520ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

.lg-float {
  animation: lg-float 18s ease-in-out infinite;
}

@keyframes lg-enter {
  from {
    opacity: 0;
    transform: translateY(18px) scale(0.985);
    filter: blur(6px);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
    filter: blur(0);
  }
}

@keyframes lg-float {
  0%, 100% {
    transform: translate3d(0, 0, 0) scale(1);
  }
  50% {
    transform: translate3d(18px, -16px, 0) scale(1.04);
  }
}

@media (prefers-reduced-motion: reduce) {
  .lg-enter,
  .lg-float {
    animation: none !important;
  }

  * {
    scroll-behavior: auto !important;
  }
}
```

---

## 17. Agent Prompt Block

Khi yêu cầu agent làm UI, có thể dùng đoạn sau:

```md
Trước khi thiết kế UI, hãy đọc `DESIGN_LIQUID_GLASS.md`.

Áp dụng phong cách LMS Liquid Glass Academic:
- Nền gradient sáng, có glow nhẹ
- Glass surface có blur vừa phải
- Text rõ, contrast tốt
- Component hiện đại, bo góc mềm
- Motion mượt, không gây rối
- Dùng Tailwind CSS
- Không thêm dependency mới
- Không dùng glass quá mức cho table/form dài
- Có loading/error/empty states
- Có accessibility cơ bản
- Respect prefers-reduced-motion

Khi code, tuân thủ token màu, radius, shadow, blur, spacing trong file này.
Nếu cần tạo component mới, dùng naming rõ ràng và không phá cấu trúc hiện tại.
```

---

## 18. Login Page Agent Prompt

```md
Hãy thiết kế lại `frontend/src/views/LoginView.vue` theo `DESIGN_LIQUID_GLASS.md`.

Yêu cầu:
- Full-screen liquid glass background
- Glass login card
- Brand block LMS
- Email/username input
- Password input
- Show/hide password
- Remember me
- Forgot password
- Submit button
- Loading state
- Error state
- Validate rỗng
- Enter để submit
- Redirect `/student/dashboard` sau login thành công
- Dùng auth store/API nếu có
- Không hardcode token
- Không thêm dependency
- Responsive mobile/tablet/desktop
- Có reduced motion support
```

---

## 19. Quality Checklist

Trước khi hoàn thành UI, agent phải kiểm tra:

```txt
[ ] Đã đọc DESIGN_LIQUID_GLASS.md
[ ] Dùng đúng màu primary/accent
[ ] Text đọc rõ
[ ] Glass không quá trong suốt
[ ] Có focus state
[ ] Có hover state
[ ] Có disabled/loading/error state
[ ] Responsive mobile
[ ] Không hardcode secret/token
[ ] Không thêm dependency không cần thiết
[ ] Không phá router/store hiện có
[ ] Không dùng backdrop blur trên list/table lớn
[ ] Có prefers-reduced-motion nếu có animation
[ ] Build/lint không lỗi do thay đổi UI
```

---

## 20. Final Rule

Liquid Glass UI trong dự án này phải là:

```txt
Premium but readable.
Modern but academic.
Animated but calm.
Glass but accessible.
Beautiful but maintainable.
```

Nếu phải chọn giữa “đẹp hơn” và “dễ đọc hơn”, hãy chọn **dễ đọc hơn**.
