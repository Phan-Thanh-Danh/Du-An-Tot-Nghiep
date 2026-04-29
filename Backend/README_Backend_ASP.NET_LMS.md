# README Backend ASP.NET – LMS

Tài liệu này mô tả bối cảnh tổng quan hệ thống LMS và quy tắc code cho backend ASP.NET Core, nhằm giúp team code nhất quán, dễ bảo trì và giảm bug khi phát triển.

---

## 1. Bối cảnh tổng quan hệ thống

Hệ thống LMS là nền tảng quản lý học tập nội bộ, phục vụ nhiều nhóm người dùng:

- Admin
- Học sinh
- Giảng viên
- Giáo vụ
- Hiệu trưởng / BGH
- Phụ huynh

Backend chịu trách nhiệm cung cấp REST API cho Web Vue.js và Mobile Flutter, xử lý nghiệp vụ học tập, điểm danh, điểm số, tài chính, đăng ký môn học, đơn từ, thông báo realtime và tích hợp AI.

Tech stack chính:

- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT Authentication
- ASP.NET Identity
- SignalR
- Hangfire
- Azure Blob / MinIO
- SendGrid / SMTP
- VNPay / MoMo
- Claude API / Python ML Service

---

## 2. Kiến trúc backend hiện tại

Cấu trúc thư mục đang dùng:

```text
Backend/
│
├── Controllers/
├── Data/
├── DTOs/
├── Helpers/
├── Middlewares/
├── Models/
├── Properties/
├── Repository/
│   ├── Repositories/
│   └── Services/
├── Services/
│   └── NewClass.cs
│
├── appsettings.Development.json
├── appsettings.json
├── Backend.csproj
├── Backend.http
├── Backend.sln
└── Program.cs
```

---

## 3. Quy tắc trách nhiệm từng thư mục

### Controllers/

Chỉ nhận request, gọi service và trả response.

Controller không được chứa business logic.

Đúng:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateUserRequest request)
{
    var result = await _userService.CreateAsync(request);
    return Ok(result);
}
```

Sai:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateUserRequest request)
{
    var user = new User();
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return Ok(user);
}
```

---

### Services/

Chứa business logic chính của hệ thống.

Ví dụ:

- kiểm tra quyền
- kiểm tra campus scope
- validate nghiệp vụ
- gọi repository
- gọi email, payment, AI, storage
- xử lý transaction nghiệp vụ

Quy tắc:

- Service không trả Entity trực tiếp ra Controller.
- Service nên trả DTO hoặc Result object.
- Service không viết query SQL quá phức tạp trực tiếp nếu đã có Repository.

---

### Repository/Repositories/

Chứa logic truy cập database.

Ví dụ:

- query bằng EF Core
- query phức tạp bằng Dapper
- filter theo campus
- phân trang
- include dữ liệu liên quan

Repository không xử lý nghiệp vụ.

Sai:

```csharp
public async Task EnrollStudentAsync(...)
{
    if (student.TotalCredits > maxCredits)
        throw new Exception("Over credit limit");
}
```

Đúng:

```csharp
public async Task<Student?> GetByIdAsync(Guid id)
{
    return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
}
```

---

### Repository/Services/

Nếu đã có thư mục `Services/` ở root, nên tránh để service nghiệp vụ nằm lẫn trong `Repository/Services`.

Khuyến nghị:

- `Repository/Services/` chỉ dùng tạm thời nếu project đang cũ.
- Service nghiệp vụ mới nên đưa vào `Services/`.
- Về lâu dài nên đổi thành:
  - `Repository/Interfaces/`
  - `Repository/Implementations/`
  - `Services/Interfaces/`
  - `Services/Implementations/`

---

### Models/

Chứa Entity map với database.

Quy tắc:

- Một model tương ứng một bảng chính.
- Không dùng Model làm request body hoặc response body.
- Không để validation message cho UI trong Model.
- Nên có các field audit chung nếu phù hợp:

```csharp
public DateTime CreatedAt { get; set; }
public DateTime? UpdatedAt { get; set; }
public Guid? CreatedBy { get; set; }
public Guid? UpdatedBy { get; set; }
public Guid CampusId { get; set; }
public bool IsActive { get; set; } = true;
```

---

### DTOs/

Chứa object dùng để nhận request và trả response.

Nên chia rõ:

```text
DTOs/
├── Requests/
└── Responses/
```

Ví dụ:

```csharp
public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
}
```

```csharp
public class CourseResponse
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

Không return trực tiếp Entity ra API.

---

### Data/

Chứa:

- `AppDbContext`
- cấu hình EF Core
- seed data
- migration

Khuyến nghị thêm:

```text
Data/
├── AppDbContext.cs
├── Configurations/
├── Seeders/
└── Migrations/
```

---

### Middlewares/

Chứa middleware dùng toàn hệ thống.

Nên có:

- ExceptionHandlingMiddleware
- CampusScopeMiddleware
- RequestLoggingMiddleware
- JwtValidationMiddleware nếu cần custom

Tất cả lỗi backend nên được chuẩn hóa tại middleware, không throw lung tung ra response.

---

### Helpers/

Chỉ chứa hàm hỗ trợ nhỏ, không chứa business logic.

Ví dụ hợp lý:

- DateTimeHelper
- PasswordHelper
- FileHelper
- SlugHelper
- PaginationHelper

Không nên đưa logic như tính GPA, xử lý học phí, xét điều kiện đăng ký môn vào Helpers. Các logic đó phải nằm trong Service.

---

## 4. Luồng code chuẩn

Tất cả API nên đi theo flow sau:

```text
Client
  ↓
Controller
  ↓
Service
  ↓
Repository
  ↓
DbContext / SQL Server
```

Không được đi tắt:

```text
Controller → DbContext
Controller → Repository
Controller → External API
```

Ngoại lệ rất nhỏ: health check hoặc endpoint test nội bộ.

---

## 5. Quy tắc đặt tên

### Controller

```text
UsersController
CoursesController
AttendanceController
GradesController
```

### Service

```text
IUserService
UserService
ICourseService
CourseService
```

### Repository

```text
IUserRepository
UserRepository
ICourseRepository
CourseRepository
```

### DTO

```text
CreateUserRequest
UpdateUserRequest
UserResponse
PagedUserResponse
```

### Method

Tên method phải nói rõ hành động:

```csharp
GetByIdAsync()
CreateAsync()
UpdateAsync()
DeleteAsync()
LockUserAsync()
GetStudentsByClassAsync()
```

Không đặt tên mơ hồ:

```csharp
Handle()
Process()
DoSomething()
Run()
```

---

## 6. Quy tắc API Response

Tất cả response nên thống nhất format:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }
}
```

Ví dụ thành công:

```json
{
  "success": true,
  "message": "Created successfully",
  "data": {
    "id": "..."
  },
  "errors": null
}
```

Ví dụ lỗi:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": {
    "email": ["Email is required"]
  }
}
```

---

## 7. Quy tắc xử lý lỗi

Không dùng `throw new Exception()` chung chung.

Nên tạo custom exception:

```text
Exceptions/
├── NotFoundException.cs
├── BadRequestException.cs
├── ForbiddenException.cs
├── UnauthorizedException.cs
├── ConflictException.cs
└── BusinessException.cs
```

Ví dụ:

```csharp
if (user == null)
    throw new NotFoundException("User not found");
```

Middleware sẽ map:

| Exception | HTTP Status |
|---|---|
| BadRequestException | 400 |
| UnauthorizedException | 401 |
| ForbiddenException | 403 |
| NotFoundException | 404 |
| ConflictException | 409 |
| BusinessException | 422 |
| Exception | 500 |

---

## 8. Quy tắc validation

Validate ở 2 lớp:

### DTO validation

Dùng Data Annotation hoặc FluentValidation.

```csharp
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
```

### Business validation

Đặt trong Service.

Ví dụ:

- email đã tồn tại
- lớp không tồn tại
- học sinh vượt tín chỉ tối đa
- điểm đã bị khóa
- user không thuộc campus hiện tại

---

## 9. Quy tắc async/await

Tất cả thao tác I/O phải dùng async:

- database
- email
- upload file
- payment gateway
- AI API
- SignalR
- Hangfire enqueue

Đúng:

```csharp
public async Task<UserResponse> GetByIdAsync(Guid id)
{
    var user = await _userRepository.GetByIdAsync(id);
    ...
}
```

Sai:

```csharp
var user = _context.Users.FirstOrDefault(x => x.Id == id);
```

---

## 10. Quy tắc Entity Framework Core

### Luôn dùng AsNoTracking khi chỉ đọc

```csharp
var users = await _context.Users
    .AsNoTracking()
    .ToListAsync();
```

### Không query thiếu campus scope

Sai:

```csharp
var grades = await _context.Grades.ToListAsync();
```

Đúng:

```csharp
var grades = await _context.Grades
    .Where(x => campusIds.Contains(x.CampusId))
    .ToListAsync();
```

### Không Include quá sâu nếu không cần

Sai:

```csharp
.Include(x => x.Class)
.Include(x => x.Teacher)
.Include(x => x.Students)
.Include(x => x.Grades)
.Include(x => x.Attendance)
```

Nên projection sang DTO:

```csharp
.Select(x => new CourseResponse
{
    CourseId = x.CourseId,
    Title = x.Title
})
```

---

## 11. Quy tắc Multi-Campus

Hệ thống hỗ trợ nhiều cơ sở theo mô hình cha - con.

Bắt buộc các bảng nghiệp vụ cốt lõi có `CampusId`:

- Users
- Classes
- Courses
- Attendance
- Grades
- Invoices
- Transactions
- Schedules
- Applications
- Notifications

Mọi query nghiệp vụ phải filter theo campus scope.

```sql
WHERE campus_id IN (
    SELECT id FROM fn_GetCampusSubtree(@campusId)
)
```

Không được viết API trả dữ liệu toàn hệ thống nếu user không phải Super Admin.

Quy tắc quyền:

| Vai trò | Phạm vi dữ liệu |
|---|---|
| Super Admin | Toàn hệ thống |
| Campus Admin | Campus của mình và campus con |
| Sub-Campus Admin | Chỉ campus được gán |
| Teacher | Lớp / môn mình phụ trách |
| Student | Dữ liệu của chính mình |
| Parent | Dữ liệu con được cấp quyền |

---

## 12. Quy tắc bảo mật

### JWT

JWT claim tối thiểu phải có:

```text
user_id
email
role
campus_id
exp
```

Không tin dữ liệu `user_id`, `role`, `campus_id` từ request body. Luôn lấy từ JWT claim.

### Password

- Không lưu plain text password.
- Dùng BCrypt.
- Salt rounds tối thiểu 12.

### Refresh Token

- Lưu refresh token dạng hash.
- Có bảng revoked token.
- Logout phải revoke refresh token.

### Authorization

Không chỉ check ở frontend. Backend bắt buộc check role và ownership.

Ví dụ:

- Học sinh chỉ xem điểm của chính mình.
- Giảng viên chỉ xem lớp mình dạy.
- Campus Admin không xem dữ liệu campus anh em.
- Phụ huynh chỉ xem thông tin được học sinh cấp quyền.

---

## 13. Quy tắc không xóa dữ liệu quan trọng

Không physical delete các dữ liệu sau:

- Grades
- Attendance
- Invoices
- Transactions
- Applications
- AuditLogs
- Evaluations
- Awards
- Discipline records

Thay vào đó dùng:

```csharp
entity.IsActive = false;
entity.DeletedAt = DateTime.UtcNow;
entity.DeletedBy = currentUserId;
```

Hoặc chuyển trạng thái:

```text
locked
cancelled
revoked
archived
inactive
```

---

## 14. Quy tắc Audit Log

Mọi thay đổi dữ liệu nhạy cảm phải ghi audit log.

Áp dụng cho:

- điểm số
- tài chính
- phân quyền
- đăng ký môn
- đơn từ
- điểm danh
- thay đổi lịch học
- khóa/mở khóa tài khoản

Format đề xuất:

```csharp
public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedBy { get; set; }
    public Guid CampusId { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

AuditLog không bao giờ được xóa.

---

## 15. Quy tắc Transaction

Các nghiệp vụ nhiều bước phải dùng transaction.

Bắt buộc dùng transaction cho:

- đăng ký môn học
- đổi lớp
- thanh toán học phí
- hoàn phí
- cập nhật điểm sau duyệt
- publish thời khóa biểu
- duyệt đơn có auto execute

Ví dụ:

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();

try
{
    // step 1
    // step 2
    // step 3

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 16. Quy tắc xử lý điểm danh

Không tự động gán học sinh là vắng nếu giảng viên chưa điểm danh.

Nếu hết buổi mà chưa điểm danh:

```text
session_status = UNCONFIRMED
```

Không được tự set toàn bộ học sinh thành `absent`.

Luồng đúng:

1. GV điểm danh.
2. Hệ thống lưu Attendance.
3. Sau thời gian cấu hình, session bị khóa.
4. Nếu GV quên, gửi yêu cầu mở khóa bổ sung.
5. Admin duyệt thì mở trong thời gian giới hạn.
6. Mọi thay đổi sau mở khóa phải ghi audit log.

---

## 17. Quy tắc xử lý điểm số

Điểm số là dữ liệu nhạy cảm.

Quy tắc:

- Điểm phải nằm trong khoảng 0–10.
- Sau khi khóa điểm, không update trực tiếp.
- Muốn sửa điểm phải qua request duyệt.
- Mọi thay đổi điểm phải ghi GradeChangeLog và AuditLog.
- GPA nên tính bằng Stored Procedure hoặc service chuyên trách.
- Không tính GPA ở Controller.

Không được làm:

```csharp
grade.FinalScore = request.FinalScore;
await _context.SaveChangesAsync();
```

Phải kiểm tra:

- người sửa là ai
- điểm đã khóa chưa
- có approval chưa
- có lý do sửa không
- có audit log không

---

## 18. Quy tắc xử lý tài chính

Tài chính không được sửa trực tiếp hoặc xóa hóa đơn.

Quy tắc:

- Không xóa Invoice.
- Không xóa Transaction.
- Mọi điều chỉnh phải tạo bút toán đối ứng.
- Callback payment phải verify HMAC.
- `transaction_id` phải unique.
- Thanh toán đang xử lý dùng trạng thái `PROCESSING`.
- Nếu timeout thì Hangfire rollback về `UNPAID` hoặc đối soát lại gateway.
- Không cập nhật `PAID` nếu chưa verify chữ ký.

---

## 19. Quy tắc Background Jobs

Các tác vụ lâu hoặc không cần trả kết quả ngay phải đưa vào Hangfire.

Ví dụ:

- gửi email
- tạo PDF
- chấm AI
- phát hiện đạo văn
- nhắc học phí
- nhắc điểm danh
- tổng hợp báo cáo
- retry payment reconciliation
- gửi notification hàng loạt

Không để request chờ quá lâu vì sẽ gây timeout.

Sai:

```csharp
await _emailService.SendBulkEmailAsync(1000Users);
return Ok();
```

Đúng:

```csharp
_backgroundJobClient.Enqueue(() => _notificationJob.SendBulkAsync(notificationId));
return Accepted();
```

---

## 20. Quy tắc SignalR

SignalR dùng cho realtime event:

- thông báo điểm danh
- notification
- ticket chat
- trạng thái thi
- cập nhật lịch phòng
- unlock bài học

Không dùng SignalR thay cho REST API chính.

REST API vẫn là source of truth. SignalR chỉ push thay đổi tới client.

---

## 21. Quy tắc tích hợp bên ngoài

Các service ngoài phải tách riêng, không gọi trực tiếp trong Controller.

Nên có folder:

```text
Integrations/
├── AI/
├── Email/
├── Payment/
├── Storage/
└── Pdf/
```

Ví dụ interface:

```csharp
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}
```

```csharp
public interface IPaymentGateway
{
    Task<PaymentUrlResponse> CreatePaymentUrlAsync(CreatePaymentRequest request);
    Task<bool> VerifyCallbackAsync(PaymentCallbackRequest request);
}
```

---

## 22. Quy tắc appsettings

Không hard-code config trong code.

Sai:

```csharp
var secret = "abc123";
```

Đúng:

```csharp
var secret = _configuration["Jwt:SecretKey"];
```

Không commit secret thật lên Git:

- JWT Secret
- DB Password
- SendGrid API Key
- VNPay Secret
- MoMo Secret
- Claude API Key
- Azure Blob Connection String

Dùng:

- appsettings.Development.json cho local
- environment variables cho production
- user-secrets cho dev cá nhân

---

## 23. Quy tắc logging

Log đủ để debug nhưng không log dữ liệu nhạy cảm.

Không log:

- password
- refresh token
- access token
- payment secret
- full card/bank info
- private key
- file chứng từ nhạy cảm

Nên log:

- request id
- user id
- campus id
- action
- entity id
- exception message
- stack trace ở môi trường dev

---

## 24. Quy tắc mapping DTO

Khuyến nghị dùng AutoMapper hoặc mapping thủ công nhất quán.

Không trả entity có navigation property lớn.

Sai:

```csharp
return Ok(course);
```

Đúng:

```csharp
return Ok(new CourseResponse
{
    CourseId = course.CourseId,
    Title = course.Title,
    Status = course.Status
});
```

---

## 25. Quy tắc phân trang

Các API list bắt buộc có phân trang.

Ví dụ:

```text
GET /api/users?pageIndex=1&pageSize=20
GET /api/courses?pageIndex=1&pageSize=20
GET /api/grades?pageIndex=1&pageSize=50
```

Không trả toàn bộ bảng.

Response đề xuất:

```csharp
public class PagedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<T> Items { get; set; } = [];
}
```

---

## 26. Quy tắc status

Không dùng string rải rác trong code.

Sai:

```csharp
if (user.Status == "active")
```

Đúng:

```csharp
if (user.Status == UserStatus.Active)
```

Nên tạo constants hoặc enum:

```csharp
public static class UserStatus
{
    public const string Active = "active";
    public const string Locked = "locked";
    public const string FirstLogin = "first_login";
}
```

---

## 27. Quy tắc module quan trọng

### M1 - Auth

- Dùng ASP.NET Identity.
- JWT có role và campus_id.
- Sai mật khẩu nhiều lần phải lock tạm.
- First login bắt buộc đổi mật khẩu.

### M2 - Course

- Upload file qua service riêng.
- Không cho xóa bài học đã có progress.
- Progress chỉ tăng, không giảm.

### M3 - Assignment

- Validate MIME type.
- Không block request khi chạy plagiarism.
- AI grade chỉ là gợi ý, GV phải xác nhận.

### M5 - Attendance

- Không auto absent khi GV chưa điểm danh.
- Sau lock chỉ Admin được sửa.
- Mọi sửa sau lock phải audit.

### M6 - Grades

- Không sửa điểm sau khi khóa nếu không có approval.
- GPA không tính ở Controller.
- GradeChangeLog bắt buộc.

### M10 - Finance

- Callback phải verify HMAC.
- Không xóa invoice.
- Không set paid nếu gateway chưa xác nhận.

### M12 - Enrollment

- Kiểm tra prerequisite.
- Kiểm tra max credit.
- Check slot trong transaction.
- Waitlist FIFO.

### M13 - Applications

- Không sửa đơn sau submitted.
- Reject phải có lý do.
- Approve có auto execute phải audit.

---

## 28. Checklist trước khi tạo API mới

Trước khi code một API, kiểm tra:

- [ ] API này thuộc module nào?
- [ ] Role nào được gọi?
- [ ] Có cần campus filter không?
- [ ] Có cần check ownership không?
- [ ] Request DTO đã validate chưa?
- [ ] Controller có mỏng không?
- [ ] Business logic đã nằm trong Service chưa?
- [ ] Query DB đã nằm trong Repository chưa?
- [ ] Có cần transaction không?
- [ ] Có cần audit log không?
- [ ] Có cần soft delete không?
- [ ] Có cần SignalR notification không?
- [ ] Có cần Hangfire job không?
- [ ] Response có đúng ApiResponse không?
- [ ] Có phân trang nếu là list không?
- [ ] Có test case cho lỗi chính không?

---

## 29. Checklist code review

Reviewer cần kiểm tra:

- [ ] Không query DB trực tiếp trong Controller.
- [ ] Không trả Entity trực tiếp ra API.
- [ ] Không thiếu campus scope.
- [ ] Không hard-code role/status/string.
- [ ] Không throw Exception chung chung.
- [ ] Không xóa vật lý dữ liệu nhạy cảm.
- [ ] Không update điểm/tài chính thiếu audit.
- [ ] Không gọi API ngoài trực tiếp trong Controller.
- [ ] Không dùng sync method cho DB/API.
- [ ] Không thiếu transaction ở nghiệp vụ nhiều bước.
- [ ] Không log token/password/secret.
- [ ] Không thiếu pagination.
- [ ] Không bỏ qua authorization ở backend.

---

## 30. Quy tắc Git commit

Format đề xuất:

```text
feat(auth): add login with JWT
fix(attendance): prevent auto absent when session unconfirmed
refactor(course): move upload logic to storage service
chore(db): add campus id to grades table
```

Loại commit:

| Type | Ý nghĩa |
|---|---|
| feat | thêm tính năng |
| fix | sửa bug |
| refactor | refactor code |
| chore | cấu hình, build, package |
| docs | tài liệu |
| test | test |
| perf | tối ưu hiệu năng |

---

## 31. Gợi ý cải tiến cấu trúc sau này

Khi project lớn hơn, nên refactor dần sang cấu trúc rõ hơn:

```text
Backend/
├── Controllers/
├── Application/
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/
├── Domain/
│   ├── Entities/
│   ├── Constants/
│   └── Enums/
├── Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Integrations/
│   └── BackgroundJobs/
├── Middlewares/
├── Helpers/
└── Program.cs
```

Không cần refactor ngay từ đầu nếu team chưa quen. Trước mắt chỉ cần giữ đúng nguyên tắc:

```text
Controller mỏng
Service xử lý nghiệp vụ
Repository xử lý database
DTO không lẫn Entity
Campus scope bắt buộc
Audit log cho dữ liệu nhạy cảm
Không xóa vật lý dữ liệu quan trọng
```

---

## 32. Nguyên tắc quan trọng nhất

Nếu không chắc code nên đặt ở đâu, áp dụng quy tắc sau:

| Câu hỏi | Đặt ở đâu |
|---|---|
| Nhận request / trả response? | Controller |
| Xử lý nghiệp vụ? | Service |
| Query database? | Repository |
| Map bảng database? | Models |
| Request/Response API? | DTOs |
| Bắt lỗi toàn cục? | Middlewares |
| Hàm tiện ích nhỏ? | Helpers |
| Gửi email/payment/AI/storage? | Integrations hoặc Service riêng |
| Job chạy nền? | BackgroundJobs |

---

## 33. Kết luận

Backend LMS phải ưu tiên:

1. Đúng nghiệp vụ.
2. Không rò dữ liệu giữa các campus.
3. Không mất audit trail.
4. Không sửa/xóa dữ liệu nhạy cảm tùy tiện.
5. Controller mỏng, Service rõ, Repository sạch.
6. API response nhất quán.
7. Validation đầy đủ.
8. Transaction cho nghiệp vụ quan trọng.
9. Background job cho tác vụ nặng.
10. Code dễ đọc hơn code “ngắn nhưng khó hiểu”.
