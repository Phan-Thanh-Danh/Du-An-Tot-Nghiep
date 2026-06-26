# EduLMS Portal Motion Storyboard & Interaction Specification

Tài liệu này định nghĩa hệ thống motion (chuyển động) cho Landing Page EduLMS (Phase D3), làm cơ sở để lập trình viên triển khai trong Phase D4. Motion được thiết kế bám sát art direction **"Calm Intelligent Campus"**.

---

## 1. Motion Principles
Hệ thống chuyển động phải tạo cảm giác:
- **Bình tĩnh, thông minh, có chiều sâu, mượt, phản hồi rõ, hiện đại, giáo dục**.
- **KHÔNG**: gaming, cyberpunk, bounce, elastic, playful trẻ em, landing SaaS đại trà, glitch, neon, card bay quá đà.

Mọi chuyển động chỉ phục vụ một trong 3 mục đích:
1. Dẫn dắt ánh nhìn.
2. Giải thích mối quan hệ (ví dụ: dữ liệu chạy về lõi).
3. Phản hồi tương tác của người dùng.

---

## 2. Motion Tokens & Easing
### Duration Tokens
- `--motion-instant: 120ms`
- `--motion-fast: 180ms`
- `--motion-base: 240ms`
- `--motion-medium: 320ms`
- `--motion-slow: 480ms`
- `--motion-entrance: 560ms`

### Easing Functions
- **Entrance:** `cubic-bezier(0.16, 1, 0.3, 1)` (Mượt, hãm tốc tốt khi kết thúc).
- **Interaction:** `cubic-bezier(0.2, 0.8, 0.2, 1)` (Phản hồi nhanh, snap nhẹ).
- **Exit:** `cubic-bezier(0.4, 0, 1, 1)` (Rút lui mượt mà).
- **Linear:** Chỉ dùng cho Shader time hoặc đường connection chạy chậm. Tuyệt đối không dùng Bounce/Elastic.

---

## 3. Entrance Timeline
Tổng thời gian timeline không vượt quá **900ms**. HTML default phải hiển thị tĩnh, animation kích hoạt bằng class khi mount (tránh chớp tắt nếu hỏng JS).

| Element | Start | Duration | From | To |
| --- | --- | --- | --- | --- |
| Header | 0ms | 320ms | opacity 0, y -8px | opacity 1, y 0 |
| Eyebrow | 80ms | 420ms | opacity 0, y 10px | opacity 1, y 0 |
| Headline | 140ms | 560ms | opacity 0, y 14px | opacity 1, y 0 |
| Subtitle | 220ms | 480ms | opacity 0, y 10px | opacity 1, y 0 |
| Trust chips | 300ms | 420ms | opacity 0, y 8px | opacity 1, y 0 |
| Constellation | 360ms | 520ms | opacity 0, scale 0.97| opacity 1, scale 1 |
| Student card | 220ms | 520ms | opacity 0, x 14px | opacity 1, x 0 |
| Teacher card | 300ms | 480ms | opacity 0, y 12px | opacity 1, y 0 |
| Parent card | 360ms | 480ms | opacity 0, y 12px | opacity 1, y 0 |
| Staff card | 430ms | 460ms | opacity 0, y 10px | opacity 1, y 0 |
| Footer | 520ms | 360ms | opacity 0 | opacity 1 |

---

## 4. Specific Motion Specifications

### 4.1. Header & Headline
- **Header:** Fade + trượt xuống từ `-8px`. Logo không xoay, không shimmer loop. Nút "Hỗ trợ" hiện cùng lúc.
- **Headline:** Dùng block reveal (overflow: hidden). Dòng đầu trượt lên từ 12-14px. Dòng gradient trượt lên trễ hơn khoảng 60-80ms. Không gõ phím (typing).

### 4.2. Trust Chips
- **Entrance:** Stagger 40-60ms từng chip. Opacity 0→1, translateY 8px→0. Icon scale `0.96→1` một lần duy nhất.
- **Hover:** Lift tối đa 1px, thay đổi background/border theo cấu hình. Không scale.

### 4.3. Constellation System
- **Entrance:** Cả cụm fade + scale từ 0.97. Render lần lượt từ lõi (Core) ra ngoài, connector lan ra các Node.
- **Ambient Motion (Chuyển động liên tục 1):** 
  - **Core:** Glow opacity `0.65→0.85`, scale `1→1.015`. Thời gian 5-6s (alternate).
  - **Nodes:** Drift ngẫu nhiên độc lập tối đa 1-2px, chu kỳ 6-9s lệch pha. Không xoay.
  - **Connector:** Opacity pulse theo sóng từ lõi ra node (4-6s). Chậm rãi, không giật cục như điện.
  - **Labels:** Hoàn toàn đứng yên để đảm bảo độ đọc.

### 4.4. Portal Card Interactions
**Student Card (Mức ưu tiên 1):**
- **Hover:** translateY `-3px`, scale `1.006`. Tăng nhẹ blue tint của border. Icon scale `1.035`. Arrow di chuyển sang phải `4px`.
- **Focus:** Không lift, thêm focus-visible ring.
- **Active:** translateY `-1px`, scale `0.998` (100-120ms), không làm chệch layout.
- **Line-art/Feature chips:** Không scale/dịch chuyển độc lập, chỉ sáng lên chút ít.

**Teacher / Parent Card:**
- **Hover:** translateY `-2px`, scale `1.004`. Arrow di chuyển sang phải `3px`. Icon scale `1.03`.

### 4.5. Pointer Spotlight
- Radial highlight mờ đi theo trỏ chuột (kích thước 320-420px, opacity 0.35-0.45).
- Chỉ áp dụng cho Student, Teacher, Parent khi đang hover.
- *Triển khai bằng CSS variables + throttled global pointer event*, không tạo rAF loop riêng.

### 4.6. Staff Accordion
- **Open:** Chevron xoay 180deg (240ms). Panel dùng CSS Grid rows `0fr→1fr`, opacity `0→1` và nội dung bên trong trượt từ `-4px→0` (260-300ms). Link con stagger 35-45ms hiện lên.
- **Close:** Link con lặn nhanh hơn, panel thu lại `0fr` (220-260ms), chevron xoay về 0. Cấm dùng trick max-height 999px.

### 4.7. Route Transition
- Không dùng hiệu ứng loading che màn hình hay slide khổng lồ.
- **Landing Exit:** Opacity `1→0`, translateY `-4px` (Thời lượng cực ngắn 140-180ms).
- **Login Enter:** Opacity `0→1`, translateY `6px→0` (220-280ms). Chuyển cảnh nhạy bén.

### 4.8. Shader Prototype (Phase D4)
- Concept **Knowledge Flow Field**: Màn sương dữ liệu, Domain warp siêu chậm chạy ngầm. Blue bên Hero, Cyan bên Portal. Cycle kéo dài 20-28s. Pointer tác động rất nhẹ. Opacity tổng `0.10-0.16`.
- Không sử dụng particle/neon. Hoàn toàn đóng vai trò "khí quyển" (Atmosphere). Nếu tắt JS, trang Web tĩnh vẫn phải duy trì 100% thẩm mỹ.

---

## 5. Cấu hình Thích ứng

### 5.1. Reduced Motion (`prefers-reduced-motion`)
- Vô hiệu hóa Shader, Constellation ambient, Pointer spotlight.
- Entrance không translate, chỉ xuất hiện tĩnh hoặc mờ cực nhạt trong 100ms.
- Card hover: Giữ nguyên transform, chỉ đổi màu và shadow.
- Mở accordion tức thời. Route transition chỉ dùng opacity dưới 100ms.

### 5.2. Mobile Devices (<768px)
- Vô hiệu hóa Pointer spotlight, card lift, scale card.
- Rút ngắn Entrance timeline còn 300-500ms.
- Tắt Shader trên thiết bị yếu. Accordion nhanh (tối đa 240ms).
- Tap feedback bằng background tint. Tránh animation làm giật khung hình khi Chrome co giãn thanh điều hướng.

### 5.3. Performance Budget
- 1 Global Animation Timeline bằng Vue composable hoặc Web Animations API cơ bản.
- 1 Global Pointer Listener duy nhất (throttled). Tối đa 1 Shader Canvas tổng thể.
- **Cấm:** Animate `blur()`, `backdrop-filter`, `width`, `padding`, `margin`, `box-shadow` liên tục.
- Pause toàn bộ ambient khi tab bị ẩn (Page Visibility API).

---

## 6. Audit System & Component
- **Cấu trúc DOM hiện tại:** Các cổng đăng nhập nằm ở mảng `.hero-right`. Accordion Staff nằm chung layout, quản lý state `staffOpen` bằng Vue ref. Các Node constellation là SVG/Div thuần nằm đè `position: absolute`. Mũi tên và icon là `lucide-vue-next`.
- **Vấn đề "Nút Tròn Đen Chữ V":** Thuộc về công cụ ngoại vi `vite-plugin-vue-devtools` trong file `vite.config.js`. Không thuộc mã nguồn UI. Không xuất hiện trên Producton. Không cần xoá code, chỉ tắt khi muốn chụp screenshot sạch.

---

## 7. Lộ trình Triển khai (D4)
Không làm tất cả cùng lúc. Trình tự bắt buộc:
1. **D4.1:** Mount-state entrance (Sử dụng Vue `<Transition>` và CSS keyframes cơ bản).
2. **D4.2:** Tích hợp Portal card interactions (Hover/Focus).
3. **D4.3:** Hoàn thiện Staff accordion CSS Grid transition.
4. **D4.4:** Bổ sung Constellation ambient motion (CSS keyframes).
5. **D4.5:** Route transition sang màn hình Đăng nhập cụ thể.
6. **D4.6:** Viết Shader prototype riêng lẻ bên ngoài.
7. **D4.7:** Tích hợp Shader vào Landing.
8. **D4.8:** Khớp logic `prefers-reduced-motion`.
9. **D4.9:** Tối ưu hóa Performance QA (Frame-rate, Memory leak).
