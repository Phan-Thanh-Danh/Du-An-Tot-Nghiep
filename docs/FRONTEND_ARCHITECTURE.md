# Frontend Architecture

## Stack Frontend

- Vue 3
- Vite
- Vue Router
- Pinia
- Tailwind CSS
- Vitest
- ESLint/Oxlint/Prettier
- lucide-vue-next

## Cấu Trúc Thư Mục

```text
frontend/
├── src/
│   ├── assets/
│   ├── components/
│   │   ├── SinhVien/
│   │   └── icons/
│   ├── router/
│   ├── stores/
│   └── views/
│       ├── Student/
│       └── SinhVien/
├── package.json
└── vite.config.js
```

## Router

Router nằm tại `frontend/src/router/index.js`.

Đã có:
- `/login`
- `/about`
- `/student/dashboard`
- `/student/courses`
- `/student/courses/:courseId`
- `/student/assignments`
- `/student/assignments/:assignmentId`
- `/student/exams`
- `/student/exams/:examResultId`
- `/student/grades`
- `/student/schedule`
- `/student/attendance`
- `/student/registrations`
- `/student/tuition`
- `/student/support-tickets`
- `/student/requests`
- `/student/evaluations`
- `/student/profile`
- `/student/parent-links`

Auth guard đã kiểm tra route private và redirect về `/login?redirect=...` khi chưa đăng nhập.

## Pinia Stores

Pinia đã được đăng ký trong `main.js`.

Hiện có:
- `stores/counter.js`: store mẫu từ scaffold.
- `stores/auth.js`: lưu access token, user, hạn token, trạng thái đổi mật khẩu, login/logout và role check.

Dự kiến/cần bổ sung:
- `studentStore` hoặc store theo module nếu cần cache dashboard/courses.
- Không lưu secret hoặc password trong store.

## API Client

Đã có API client cơ bản tại `src/services/apiClient.js`.

Hiện có:
- Base URL lấy từ `VITE_API_BASE_URL`; nếu không cấu hình, request sẽ dùng same-origin `/api`.
- Tự gắn `Authorization: Bearer <token>` khi có token trong localStorage.
- Chuẩn hóa lỗi qua `ApiError`.
- `authApi.login` và `authApi.changePassword`.

Dự kiến/cần bổ sung:
- Tách API theo module khi courses/assignments/grades có controller thật.
- Xử lý `401/403` tập trung hơn nếu backend mở rộng refresh-token/logout.
- Không gọi endpoint `dự kiến` nếu backend chưa có controller hoặc mock rõ ràng.

## Layout

Student layout hiện tại:
- `components/SinhVien/Layout_SinhVien.vue`
- Sidebar/topbar/breadcrumb trong `components/SinhVien`
- Menu data trong `components/SinhVien/data/menuData.js`

Khi thêm teacher/admin layout, nên tách layout theo role thay vì nhồi tất cả vào student shell.

## Views

Hiện có hai nhóm tên thư mục:

- `views/Student`: nhiều view student bằng tiếng Anh.
- `views/SinhVien`: dashboard và một số màn học tập tiếng Việt.

Khi chỉnh view hiện có, giữ nguyên vị trí file để tránh phá router. Khi thêm mới, chọn theo module đang mở rộng và cập nhật router rõ ràng.

## Components

Quy ước:
- Component dùng lại đặt trong `components`.
- Component theo role/student đặt trong `components/SinhVien`.
- Icon ưu tiên `lucide-vue-next` nếu có sẵn.
- Không tạo UI marketing/landing page cho màn nghiệp vụ.
- Màn hình nghiệp vụ cần tối ưu cho thao tác lặp lại, dễ đọc, không quá trang trí.

## Auth Guard

Hiện `beforeEach` đã dùng `authStore`.

- Nếu route có `meta.requiresAuth` và chưa login, redirect `/login`.
- Nếu route có `meta.role`, kiểm tra role user.
- Nếu token hết hạn theo `expiresAt`, store tự clear session.
- Tránh hardcode user role ở component; lấy từ auth store.

## Role Guard

Hiện có:

- Student chỉ vào `/student/*`.
- Role frontend được so sánh không phân biệt hoa thường, nên `student` trong router khớp với `Student` từ backend.
- Role frontend chỉ để cải thiện UX; backend vẫn là nguồn kiểm tra quyền cuối cùng.

Dự kiến:
- Teacher/admin có layout/route riêng khi được triển khai.

## Loading/Error/Empty States

Khi gọi API:

- Loading: skeleton/spinner hoặc trạng thái đang tải rõ ràng.
- Error: hiển thị lỗi thân thiện, có retry nếu phù hợp.
- Empty: thông báo không có dữ liệu và hành động tiếp theo nếu có.
- Không để component crash khi API trả null/empty array.

## Testing Recommendation

Ưu tiên:

1. Test auth store login/logout/token state.
2. Test router guard cho public/private/role route.
3. Test component student dashboard với loading/error/empty.
4. Test API client xử lý `401`, `403`, validation error.
5. Smoke test build sau khi đổi route/view.

Lệnh verify:

```powershell
cd frontend
npm run build
npm run test:unit
npm run lint
```
