---
version: alpha
name: LMS Academic Modern
description: Modern, calm, premium academic design system for a Vietnamese LMS graduation project.
colors:
  primary: "#1E3A8A"
  on-primary: "#FFFFFF"
  primary-container: "#DBEAFE"
  secondary: "#0F766E"
  on-secondary: "#FFFFFF"
  tertiary: "#7C3AED"
  on-tertiary: "#FFFFFF"
  background: "#F8FAFC"
  surface: "#FFFFFF"
  surface-soft: "#F1F5F9"
  text: "#0F172A"
  text-muted: "#64748B"
  border: "#E2E8F0"
  danger: "#DC2626"
  success: "#16A34A"
  warning: "#D97706"
typography:
  h1:
    fontFamily: Inter
    fontSize: 40px
    fontWeight: 800
    lineHeight: 1.1
    letterSpacing: -0.03em
  h2:
    fontFamily: Inter
    fontSize: 28px
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: -0.02em
  body:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: 600
    lineHeight: 1.4
  caption:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: 400
    lineHeight: 1.4
rounded:
  sm: 8px
  md: 14px
  lg: 20px
  xl: 28px
  full: 9999px
spacing:
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  2xl: 48px
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.on-primary}"
    rounded: "{rounded.md}"
    padding: 14px 18px
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.md}"
    padding: 12px 14px
  card:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.xl}"
    padding: 32px
---

# LMS Academic Modern Design System

## Overview

Thiết kế dành cho hệ thống LMS hiện đại, chuyên nghiệp và đáng tin cậy. Giao diện phải tạo cảm giác học thuật, sạch sẽ, mượt mà, không rối mắt. Ưu tiên readability, accessibility, responsive và trải nghiệm đăng nhập nhanh.

## Colors

Sử dụng nền sáng với gradient nhẹ từ slate/blue/indigo. Màu chính là navy blue tạo cảm giác tin cậy. Cyan/teal/violet chỉ dùng làm accent. Không dùng quá nhiều màu sặc sỡ.

## Typography

Dùng Inter hoặc system sans-serif. Heading đậm, tracking âm nhẹ. Body dễ đọc. Label rõ ràng. Không scale font bằng viewport width; hierarchy phải ổn định trên mobile và desktop.

## Layout

Login page dùng bố cục 2 cột trên desktop:

- Bên trái: branding, mô tả LMS, feature highlights, decorative gradient.
- Bên phải: login card.

Trên mobile chuyển thành 1 cột, login card ở giữa. Các màn nghiệp vụ nên dùng layout dense, rõ ràng, phù hợp thao tác lặp lại.

## Elevation & Depth

Dùng shadow mềm, blur nhẹ, border tinh tế. Cho phép glassmorphism nhẹ nhưng không làm giảm độ đọc. Elevation dùng để phân cấp nội dung, không dùng để trang trí quá mức.

## Shapes

Bo góc lớn, mềm, hiện đại. Button và input bo góc vừa phải. Card bo góc lớn. Không lạm dụng bo tròn cực lớn cho mọi thành phần.

## Components

Login card phải có:

- Logo/brand LMS.
- Title.
- Subtitle.
- Email/username input.
- Password input.
- Show/hide password.
- Remember me.
- Forgot password.
- Login button.
- Loading state.
- Error alert.
- Demo account hint nếu phù hợp.
- Link quay về trang chủ nếu có.

Input cần có label thật, focus ring rõ và thông báo lỗi dễ hiểu. Button chính dùng navy, hover sang sắc thái sáng hơn hoặc indigo/cyan có kiểm soát.

## Do's and Don'ts

Do:

- UI sạch, hiện đại, responsive.
- Có transition/animation mượt.
- Có focus state rõ ràng.
- Có loading/error state.
- Có accessibility label.
- Code tách logic rõ ràng.

Don't:

- Không dùng màu quá chói.
- Không hardcode token thật.
- Không để mock login trong production logic.
- Không phá router hiện tại.
- Không rewrite toàn bộ app.
- Không thêm dependency mới nếu không cần.
