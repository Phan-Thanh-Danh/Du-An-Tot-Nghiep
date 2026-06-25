# Backend Architecture

## Stack Backend

- ASP.NET Core `net10.0`
- EF Core `10.0.x`
- SQL Server
- JWT Bearer Authentication
- OpenAPI trong môi trường Development
- CORS policy `FrontendDev` cho Vite dev server, có thể cấu hình bằng `Cors:AllowedOrigins`
- Controller-Service-DTO pattern

## Cấu Trúc Thư Mục Hiện Tại

```text
Backend/
├── Constants/
├── Controllers/
├── Data/
├── DTOs/
├── Exceptions/
├── Helpers/
├── Middlewares/
├── Migrations/
├── Models/
├── Services/
├── Backend.csproj
└── Program.cs
```

## Controllers

Đã có:

- `AuthController`: `POST /api/auth/login`, `POST /api/auth/change-password`.
- `OrganizationsController`: CRUD tổ chức/cây tổ chức.
- `AccountManagementExampleController`: endpoint mẫu `/api/admin/accounts`.
- `CoursesController`: CRUD khóa học `/api/courses` theo môn học, giảng viên, lớp hành chính, học kỳ và cơ sở.
- `BuildingsController`: CRUD tòa nhà `/api/master-data/buildings` theo scope `MaDonVi`.
- `FloorsController`: CRUD tầng/lầu `/api/master-data/floors` và danh sách tầng theo tòa nhà.
- `RoomsController`: CRUD phòng học `/api/master-data/rooms` theo cấu trúc `DonVi -> ToaNha -> Tang -> PhongHoc`.
- `ApplicationSchemaController`: schema/mẫu đơn read-only cho module đơn từ.
- `StudentApplicationsController`: vòng đời đơn từ phía sinh viên `/api/student/applications`.

Quy ước:
- Controller chỉ nhận request, gọi service, trả DTO/result.
- Không viết business logic dài trong controller.
- Endpoint protected dùng `[Authorize]`.

## Services

Đã có:

- `IAuthService`/`AuthService`: login, đổi mật khẩu, audit login/password.
- `IOrganizationService`/`OrganizationService`: cây tổ chức, validate parent, scope theo campus, xóa mềm/xóa cứng.
- `ICourseService`/`CourseService`: quản lý khóa học MVP theo lớp hành chính, validate cơ sở/môn/giảng viên/học kỳ, chống trùng và ghi audit.
- `IBuildingService`/`BuildingService`: quản lý tòa nhà, validate đơn vị, mã tòa nhà và xóa mềm.
- `IFloorService`/`FloorService`: quản lý tầng/lầu, validate tòa nhà, thứ tự tầng và xóa mềm.
- `IRoomService`/`RoomService`: quản lý phòng học, validate đơn vị, tòa nhà, tầng, mã phòng, loại/trạng thái phòng và xóa mềm.
- `IApplicationSchemaService`/`ApplicationSchemaService`: expose constants/template active của đơn từ.
- `IStudentApplicationService`/`StudentApplicationService`: tạo nháp, danh sách/chi tiết của sinh viên, cập nhật, nộp, nộp lại và hủy đơn.
- `IApplicationEvidenceService`/`ApplicationEvidenceService`: upload, download và soft delete minh chứng đơn từ của sinh viên.
- `IApplicationEvidenceObjectStore`: abstraction lưu file minh chứng; Local chỉ dùng Development/Testing, R2 dùng private object storage.
- `ApplicationFormDataValidator`, `ApplicationReferenceValidator`, `ApplicationEvidenceValidator` và các `IApplicationSubmissionRule`: validate form động, entity liên quan, metadata minh chứng và rule theo loại đơn.

Quy ước:
- Service xử lý nghiệp vụ và gọi `ApplicationDbContext`.
- Lỗi nghiệp vụ throw `ApiException`.
- Mapping entity -> DTO có thể đặt trong service nếu module nhỏ; module lớn nên cân nhắc mapper/helper nội bộ.

## DTOs

Đã có:

- `DTOs/Auth`: login request/response, current user context, change password.
- `DTOs/Organizations`: create/update/response/tree DTO.

Quy ước:
- Không expose entity trực tiếp ra API.
- Request DTO không chứa field hệ thống như password hash, audit fields nếu không cần.
- Response DTO dùng tên API dễ hiểu, không ép frontend biết tên cột DB.

## EF Core DbContext

`ApplicationDbContext`:

- Khai báo `DbSet` cho toàn bộ entity nghiệp vụ.
- Cấu hình table, key, column, index, check constraint và relationship.
- Dùng SQL Server qua `UseSqlServer`.

`Program.cs` hiện gọi `context.Database.MigrateAsync()` khi app khởi động. Khi deploy thật, cần cân nhắc quy trình migration có kiểm soát.

## Middleware

Đã có:

- `ExceptionMiddleware`: chuẩn hóa lỗi.
- `JwtMiddleware`: đọc JWT/current user context.
- `FirstLoginMiddleware`: xử lý trạng thái đăng nhập lần đầu.
- `CampusScopeMiddleware`: hỗ trợ scope theo cơ sở.

Thứ tự hiện tại trong `Program.cs`: exception -> HTTPS -> migration/seed -> CORS -> authentication -> JWT -> first-login -> campus-scope -> authorization -> controllers.

## JWT Authentication

- Cấu hình trong `Program.cs` bằng `JwtBearerDefaults.AuthenticationScheme`.
- Token sinh bởi `JwtHelper`.
- Claim types tùy biến nằm trong `CustomClaimTypes`.
- Login map role/status từ DB sang API role/status.

Không hardcode JWT secret trong code. Dùng configuration/user secrets/environment variables.

## Authorization Policies

Policy hiện có:

- `AdminOnly`: `Admin`, `SuperAdmin`.
- `AcademicOperations`: `Admin`, `SuperAdmin`, `AcademicStaff`, `CampusAdmin`.
- `Reports`: `Admin`, `SuperAdmin`, `Principal`, `CampusAdmin`.
- `ApplicationStudent`: `Student`.
- `ApplicationOperations`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`.
- `ApplicationQueueRead`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`, `Principal`.
- `ApplicationReceive`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`.
- `ApplicationAssignmentManage`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`.
- `ApplicationSensitiveDecision`: `SuperAdmin`, `Admin`, `CampusAdmin`, `Principal`.
- `ApplicationSystemAdmin`: `SuperAdmin`, `Admin`.

Endpoint có role riêng nên dùng `[Authorize(Roles = ...)]` với `AuthRoles`.

## Applications / Đơn Từ

P0-DT1 chuẩn hóa nền cho module đơn từ. P0-DT2 bổ sung core lifecycle phía sinh viên, gồm tạo nháp, cập nhật, nộp, nộp lại khi bị yêu cầu bổ sung và hủy đơn. P0-DT3 bổ sung upload/download/delete minh chứng an toàn cho sinh viên. P0-DT4 bổ sung hàng đợi admin, tiếp nhận, phân công/phân công lại, SLA và admin download minh chứng. Các luồng duyệt/từ chối, notification và xử lý nghiệp vụ sau duyệt vẫn để phase sau.

Schema foundation:

- `DonTu`: giữ cột legacy và bổ sung `MaDonVi`, `MaMauDon`, `TieuDe`, `TrangThaiXuLyNghiepVu`, `NguoiXuLyCuoi`, `NoiDungYeuCauBoSung`, `KetQuaXuLyJson`, các mốc thời gian xử lý và `RowVersion`.
- `MauDonTu`: mẫu đơn versioned, JSON config có check `ISJSON`, unique `LoaiDon + PhienBan`, filtered unique cho một active template mỗi loại.
- `TepDinhKemDonTu`: metadata minh chứng, lưu `StorageKey` thay vì public URL, soft delete, FK `NoAction`.
- `NhatKyDuyetDon`: log trạng thái mở rộng, tách `GhiChuCongKhai` và `GhiChuNoiBo`, hỗ trợ `NguonThucHien = user/system`.

State machine dùng `ApplicationStateMachine`:

- Hợp lệ: `nhap -> da_nop/da_huy`, `da_nop -> dang_xem_xet/da_huy`, `dang_xem_xet -> yeu_cau_bo_sung/da_duyet/tu_choi`, `yeu_cau_bo_sung -> dang_xem_xet/da_huy`.
- Terminal: `da_duyet`, `tu_choi`, `da_huy`.
- Không nhận trạng thái tùy ý từ frontend.

Campus/security rule cho service workflow sau này:

- Student chỉ truy cập đơn có `MaHocSinh = CurrentUser.UserId`.
- Student endpoint re-query `NguoiDung`, chỉ chấp nhận role DB `hoc_sinh` và trạng thái `hoat_dong`; không tin claim/token cho dữ liệu hệ thống.
- SuperAdmin/Admin xem toàn hệ thống.
- CampusAdmin xem campus và campus con.
- SubCampusAdmin/AcademicStaff/Principal xem campus hiện tại.
- Không tin frontend truyền `MaHocSinh`, `MaDonVi`, `TrangThai`, `NguoiDuyetHienTai`.
- `CampusScopeMiddleware` chỉ đọc route/query/header, nên không đủ cho các endpoint đơn từ không truyền `maDonVi`; service phải tự scope bằng `CurrentUserContext`.

Student lifecycle P0-DT2:

- `POST /api/student/applications` tạo `DonTu` trạng thái `nhap`, gắn active `MauDonTu`, `MaHocSinh`, `MaDonVi`, `NguoiTao`, `NgayTao` và log `tao_nhap`.
- `PUT /api/student/applications/{id}` chỉ cho `nhap` hoặc `yeu_cau_bo_sung`, dùng `RowVersion` base64 để chống lost update; no-op không tạo log.
- `POST /api/student/applications/{id}/submit` chuyển `nhap -> da_nop`, set `NgayNop`, `HanXuLyLuc` theo SLA template và log public `nop`.
- `POST /api/student/applications/{id}/resubmit` chuyển `yeu_cau_bo_sung -> dang_xem_xet`, giữ `NgayNop` ban đầu, clear nội dung yêu cầu bổ sung và log public `nop_lai`.
- `POST /api/student/applications/{id}/cancel` chỉ cho trạng thái còn hủy được, set `da_huy`, không hard delete.
- Các thao tác ghi dùng transaction cho `DonTu` và `NhatKyDuyetDon`; stale `RowVersion` hoặc race condition trả `409`.

Admin queue and assignment P0-DT4:

- `AdminApplicationsController` chỉ bind route/query/body và trả `ApiResponseDto` hoặc file stream; business logic nằm trong service.
- `ApplicationAdminQueueService` xử lý list, summary, detail, assignee lookup, filter, sort, pagination và SLA projection. Query dùng `AsNoTracking`, projection DTO trực tiếp và không load collection lớn.
- `ApplicationCampusScopeService` re-query current user từ DB, xác nhận `hoat_dong`, role DB và campus DB. SuperAdmin/Admin unrestricted; CampusAdmin lấy current campus + descendants; SubCampusAdmin/AcademicStaff/Principal exact campus.
- List filter ngoài scope trả `403`; detail/download ngoài scope trả `404`; assignee không hợp lệ hoặc ngoài scope trả `404`.
- Queue mặc định chỉ gồm `da_nop` và `dang_xem_xet`; `yeu_cau_bo_sung`, `nhap` và terminal chỉ hiện khi filter rõ.
- SLA dùng `DonTu.HanXuLyLuc` với `ApplicationQueue:SlaWarningBeforeHours` mặc định 24, validate 1..168. `yeu_cau_bo_sung` là `paused`; terminal/không deadline là `none`.
- `ApplicationAssignmentService` xử lý receive/assign/reassign trong transaction `Serializable`, dùng `sp_getapplock` resource `ApplicationWorkflow:{applicationId}` và `RowVersion` base64 8 bytes.
- Receive chỉ cho `da_nop` chưa có assignee, chuyển `dang_xem_xet`, gán `NguoiDuyetHienTai`, không đổi `NguoiXuLyCuoi`, `NgayNop`, `HanXuLyLuc`.
- Assign/reassign chỉ cho `da_nop`, `dang_xem_xet`, `yeu_cau_bo_sung`; same-assignee là no-op nhưng vẫn validate `RowVersion`. Reassign cần lý do nội bộ 10..1000 ký tự.
- Workflow history dùng `NhatKyDuyetDon`: `tiep_nhan`, `phan_cong`, `phan_cong_lai`. Snapshot chỉ chứa assignee IDs và flag lý do, không chứa form data, file key hoặc secret.
- Admin evidence download dùng private evidence object store, enforce campus scope từ `DonTu`, chỉ trả file content với filename/content type sanitized. Response không trả `StorageKey`, `TenFileLuu`, `FileHash`, bucket hoặc local path.

Template validation:

- `ApplicationTemplateValidator` parse bằng `JsonDocument`, giới hạn size/depth, yêu cầu `root.fields`.
- Field `key` không rỗng/không trùng; `type` và `relatedEntity` thuộc whitelist.
- Không cho cấu hình chứa script/html/sql/query động.

Storage evidence:

- P0-DT1 chỉ tạo schema; P0-DT2 validate metadata minh chứng active đã có trong `TepDinhKemDonTu` hoặc legacy `UrlBangChung`.
- P0-DT3 thêm endpoint student upload/download/delete minh chứng qua `StudentApplicationEvidenceController`.
- Allowlist upload: PDF, JPEG, PNG, WEBP. Backend kiểm tra extension, declared content type, magic bytes, kích thước và SHA-256 hash.
- Hard cap: 5 file, 10MB/file, 25MB/tổng; effective cap lấy min giữa hard cap và cấu hình template đang gắn với đơn.
- Upload/delete chỉ cho trạng thái `nhap` hoặc `yeu_cau_bo_sung`; download cho mọi trạng thái nếu sinh viên sở hữu đơn, metadata chưa xóa và object tồn tại.
- Upload/delete dùng `RowVersion`, transaction `Serializable` và `sp_getapplock` resource `ApplicationEvidence:{applicationId}` để chống race condition.
- File upload được ghi object store trước DB transaction; nếu DB mutation thất bại thì service cleanup object đã upload. Delete soft-delete metadata trước để revoke API access, sau đó physical delete best-effort.
- Response/API không trả `StorageKey`, file hash, tên file lưu hoặc public URL. Download set `Content-Disposition: attachment`, `X-Content-Type-Options: nosniff` và `Cache-Control: private, no-store`.
- Local object store bị chặn trong Production và yêu cầu root isolated khi test/dev. Production dùng R2 private object store.
- Không dùng nguyên `StorageController` hiện tại cho minh chứng Student vì controller đó phục vụ nội dung học tập chung và allow ZIP/SVG/public URL.

## Error Handling

- Model validation trả `400` với `statusCode`, `message`, `traceId`.
- JWT challenge trả `401`.
- Forbidden trả `403`.
- Business error nên dùng `ApiException`.
- Không trả stack trace cho client.

## Validation

Hiện validation chủ yếu nằm trong service và data annotations/ApiBehaviorOptions. Khi thêm API:

- Validate input trong DTO/service.
- Kiểm tra quyền truy cập theo role và `MaDonVi`.
- Kiểm tra foreign key tồn tại trước khi tạo/cập nhật.
- Không tin dữ liệu từ frontend.

## Logging/Audit

Đã có bảng/model audit:

- `NhatKyKiemToan`
- `NhatKyThayDoiDiem`
- `NhatKyThongBao`
- `NhatKyDuyetDon`

`AuthService` đã ghi audit cho login/password. Các module nhạy cảm như điểm, tài chính, quyền, tổ chức nên ghi audit khi triển khai.

## Testing Recommendation

Ưu tiên:

1. Unit test service cho Auth và Organizations.
2. Integration test controller với database test hoặc test container SQL Server nếu phù hợp.
3. Test authorization cho role/campus scope.
4. Test migration/schema khi thay đổi database.
5. Test lỗi validation và response shape.

Lệnh verify cơ bản:

```powershell
cd Backend
dotnet restore
dotnet build
```
