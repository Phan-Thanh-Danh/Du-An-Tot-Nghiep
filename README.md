# LMS Academic Management System

Dự án tốt nghiệp xây dựng hệ thống LMS/Academic Management System cho môi trường đào tạo, tập trung vào quản lý người dùng, tổ chức/cơ sở, khóa học, bài học, bài tập, điểm danh, điểm số, thông báo, hỗ trợ và các tính năng AI dự kiến.

## Mục Tiêu Dự Án

- Xây dựng nền tảng học tập trực tuyến có backend API rõ ràng và frontend dễ mở rộng.
- Chuẩn hóa dữ liệu học vụ trên SQL Server bằng EF Core.
- Hỗ trợ xác thực JWT, phân quyền theo vai trò và phạm vi cơ sở.
- Làm nền tảng demo đồ án tốt nghiệp, ưu tiên luồng học sinh trước rồi mở rộng sang giáo viên/admin.
- Tài liệu hóa kiến trúc để AI agent và thành viên mới không tự bịa API hoặc phá cấu trúc hiện tại.

## Tech Stack

Backend:
- ASP.NET Core `net10.0`
- EF Core + SQL Server
- JWT Bearer Authentication
- Controller-Service-DTO pattern
- Middleware tùy biến cho exception, JWT context, first login và campus scope

Frontend:
- Vue 3 + Vite
- Vue Router
- Pinia
- Tailwind CSS
- Vitest, ESLint, Oxlint, Prettier
- `lucide-vue-next` cho icon

## Vai Trò Người Dùng

Vai trò được chuẩn hóa trong `Backend/Constants/AuthConstants.cs`:

- `SuperAdmin`: quản trị toàn hệ thống.
- `Admin`: quản trị học vụ.
- `CampusAdmin`: quản trị theo cơ sở.
- `SubCampusAdmin`: quản trị cơ sở con.
- `AcademicStaff`: nhân viên học vụ.
- `Principal`: hiệu trưởng/quản lý báo cáo.
- `Teacher`: giảng viên.
- `Student`: học sinh/sinh viên.
- `Parent`: phụ huynh.

## Module Chính

Đã có API:
- Authentication: đăng nhập, đổi mật khẩu.
- Organizations: quản lý đơn vị/cơ sở, cây tổ chức, xóa mềm/xóa cứng.
- Admin accounts example: endpoint mẫu kiểm tra role.
- Frontend auth cơ bản: login form, auth store, route guard, role guard, logout.

Dự kiến/cần bổ sung API:
- Users/account management đầy đủ.
- Courses, chapters, lessons, progress.
- Assignments, submissions, grading.
- Exams/quizzes, questions, student exam sessions.
- Attendance, schedule, classrooms.
- Grades, grade change requests.
- Notifications, support tickets.
- Reports, audit logs.
- AI features: tóm tắt bài học, cảnh báo rủi ro, đạo văn, phân tích cảm xúc.

## Cấu Trúc Thư Mục

```text
.
├── Backend/
│   ├── Constants/            # Role, status, claim constants
│   ├── Controllers/          # ASP.NET Core API controllers
│   ├── Data/                 # ApplicationDbContext, seed data
│   ├── DTOs/                 # Request/response DTOs theo module
│   ├── Exceptions/           # ApiException
│   ├── Helpers/              # JWT, password helpers
│   ├── Middlewares/          # Exception/JWT/first-login/campus scope
│   ├── Migrations/           # EF Core migrations
│   ├── Models/               # Entity models ánh xạ SQL Server
│   ├── Services/             # Business services theo module
│   ├── Backend.csproj
│   └── Program.cs
├── frontend/
│   ├── src/
│   │   ├── assets/
│   │   ├── components/
│   │   ├── router/
│   │   ├── stores/
│   │   └── views/
│   ├── package.json
│   └── vite.config.js
├── docs/                     # Tài liệu contract/architecture/demo
├── .cursor/rules/            # Rule cho Cursor agent
├── AGENTS.md                 # Context bắt buộc cho AI agent
├── CLAUDE.md                 # Rule cho Claude/Codex style agent
└── Source Code.sln
```

## Cách Chạy Backend

Yêu cầu:
- .NET SDK tương thích `net10.0`.
- SQL Server.
- Connection string `DefaultConnection` trong `Backend/appsettings.Development.json` hoặc user secrets.

Lệnh chạy:

```powershell
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

Ghi chú:
- `Program.cs` hiện tự chạy `Database.MigrateAsync()` khi app khởi động.
- Không commit secret, token, mật khẩu hoặc connection string thật.
- OpenAPI chỉ được map trong môi trường Development.

## Cách Chạy Frontend

Yêu cầu:
- Node.js `^20.19.0 || >=22.12.0`
- npm

Lệnh chạy:

```powershell
cd frontend
npm install
npm run dev
```

Frontend hiện có route student shell tại `/student/*`, auth store Pinia và route guard cơ bản. API base lấy từ `VITE_API_BASE_URL`; nếu không cấu hình, request sẽ dùng same-origin `/api`.

## Build/Test

Backend:

```powershell
cd Backend
dotnet restore
dotnet build
```

Frontend:

```powershell
cd frontend
npm run build
npm run test:unit
npm run lint
```

Chỉ chạy format khi cần và tránh format toàn bộ repo ngoài phạm vi task.

## Roadmap Phát Triển

1. Bổ sung change-password UI cho trạng thái đăng nhập lần đầu và refresh-token/logout API nếu cần.
2. Xây dựng API users/account management thật thay cho controller mẫu.
3. Bổ sung API cho student dashboard: khóa học, bài học, bài tập, điểm danh, điểm số.
4. Chuẩn hóa API client theo từng module và contract request/response.
5. Thêm test backend cho Auth/Organizations và test frontend cho route guard/store.
6. Hoàn thiện teacher/admin flows.
7. Triển khai báo cáo, audit log UI và AI features theo dữ liệu hiện có.
8. Chuẩn hóa môi trường deploy, biến môi trường và seed data demo.

## Ghi Chú Cho Đồ Án Tốt Nghiệp

- Dự án có schema database rộng, nhưng API hiện thực mới ở giai đoạn đầu. Khi thuyết minh cần phân biệt rõ phần đã triển khai và phần dự kiến.
- Tài liệu trong `docs/` là nguồn context chính cho AI agent và thành viên phát triển.
- Khi bổ sung tính năng, cập nhật `docs/API_CONTRACT.md` ngay sau khi thêm controller.
- Không copy nguyên source từ LMS open-source khác; chỉ tham khảo ý tưởng ở mức kiến trúc/nghiệp vụ.
- Ưu tiên demo luồng chạy ổn định hơn là mở quá nhiều màn hình chưa nối dữ liệu.
