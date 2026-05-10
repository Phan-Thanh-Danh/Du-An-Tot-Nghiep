# CLAUDE.md

Hướng dẫn hành vi cho Claude/Codex/Cursor khi làm việc trong dự án LMS Academic Management System.

## Behavior Rules

- Luôn đọc `README.md`, `AGENTS.md` và các file trong `docs/` liên quan trước khi sửa code.
- Tài liệu và comment nghiệp vụ ưu tiên tiếng Việt; code giữ phong cách hiện tại của repo.
- Không tự khẳng định endpoint, bảng, service hoặc store đã tồn tại nếu chưa kiểm tra file thật.
- Nếu thiếu thông tin, ghi rõ `dự kiến` hoặc `cần bổ sung`.
- Không hardcode secret, token, password, connection string thật hoặc dữ liệu nhạy cảm.

## Think Before Coding

- Xác định module bị ảnh hưởng: backend, frontend, database, tài liệu hoặc demo.
- Kiểm tra controller/service/DTO/model/router/store hiện có trước khi viết mới.
- Nếu có nhiều cách hiểu, nêu giả định ngắn gọn trước khi triển khai.
- Với API, đối chiếu `docs/API_CONTRACT.md` và controller thật.

## Make Surgical Changes

- Chỉ sửa file liên quan trực tiếp đến yêu cầu.
- Không rewrite business logic, không đổi stack, không thêm dependency nếu không được yêu cầu.
- Không format toàn bộ repo.
- Không đổi tên entity/model/route/component khi chưa có lý do rõ ràng.
- Không xóa code hoặc migrations hiện có nếu người dùng chưa yêu cầu.

## Follow Existing Architecture

- Backend theo hướng Controller -> Service -> DbContext.
- DTO đặt trong `Backend/DTOs/<Module>`.
- Service đặt trong `Backend/Services/<Module>`.
- Entity ánh xạ trong `Backend/Models` và `ApplicationDbContext`.
- Frontend dùng Vue 3, Vue Router, Pinia, Tailwind và component/layout hiện có.
- Route student hiện nằm dưới `/student` với layout `Layout_SinhVien.vue`.

## API Rules

- Endpoint đã có chỉ được ghi là đã có khi thấy controller/action thật.
- Endpoint chưa thấy controller phải ghi là `dự kiến`.
- Response lỗi backend nên đi qua `ExceptionMiddleware`/`ApiException`.
- Endpoint cần auth phải dùng `[Authorize]`; role cụ thể dùng constants trong `AuthRoles`.
- Không bỏ qua campus scope/role scope khi làm dữ liệu học vụ.

## Verification Commands

Backend:

```powershell
cd Backend
dotnet restore
dotnet build
```

Frontend:

```powershell
cd frontend
npm install
npm run build
npm run test:unit
npm run lint
```

Tài liệu:

```powershell
Get-ChildItem README.md,AGENTS.md,CLAUDE.md,docs,.cursor/rules -Recurse
```

## Do Not

- Không rewrite file không liên quan.
- Không copy nguyên source LMS khác.
- Không bịa endpoint hoặc API response.
- Không thêm thư viện mới cho việc có thể giải quyết bằng stack hiện tại.
- Không commit secret hoặc dữ liệu cá nhân.
- Không xóa migration/schema/model vì thấy chưa dùng ở frontend.
