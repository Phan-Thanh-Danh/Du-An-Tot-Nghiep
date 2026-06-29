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
- `RewardDisciplineController`: schema/options read-only cho nền tảng khen thưởng - kỷ luật `/api/reward-discipline/schema/options`.
- `AdminRewardCampaignsController`: CRUD metadata đợt khen thưởng Top 100 học kỳ, xét ứng viên RD3 và duyệt/điều chỉnh Top 100 RD4 qua `/api/admin/reward-campaigns`.
- `AdminCertificateTemplatesController`: quản lý mẫu bằng khen RD5 qua `/api/admin/certificate-templates`, chỉ SuperAdmin.
- `AdminRewardCertificatesController`: tải PDF bằng khen RD6 qua `/api/admin/rewards/{rewardId}/certificate/download`, có header download an toàn.

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
- `IApplicationReportService`/`ApplicationReportService`: báo cáo tổng quan đơn từ cho admin/học vụ bằng SQL aggregate trong campus scope.
- `IApplicationEvidenceObjectStore`: abstraction lưu file minh chứng; Local chỉ dùng Development/Testing, R2 dùng private object storage.
- `ApplicationFormDataValidator`, `ApplicationReferenceValidator`, `ApplicationEvidenceValidator` và các `IApplicationSubmissionRule`: validate form động, entity liên quan, metadata minh chứng và rule theo loại đơn.
- RD1 khen thưởng - kỷ luật có constants/schema endpoint và EF foundation (`DotKhenThuong`, `MauBangKhen`, mở rộng `KhenThuong`, `HoSoKyLuat`). RD2 bổ sung `IRewardCampaignService`/`RewardCampaignService` để quản lý CRUD metadata đợt Top 100 học kỳ, validate học kỳ/cơ sở/mẫu bằng khen/JSON tiêu chí, chống trùng đợt active và ghi audit create/update/cancel. RD3 bổ sung `IRewardEvaluationService` để xét danh sách ứng viên Top 100, chưa tạo `KhenThuong`. RD4 mở rộng service này cho SuperAdmin điều chỉnh ứng viên, thêm thủ công, sắp xếp, trình duyệt và duyệt chính thức; approve chạy transaction, tạo `KhenThuong`, đánh dấu ứng viên `da_duyet_kt` và ghi audit tối giản. RD5 bổ sung `ICertificateTemplateService` để CRUD/vô hiệu hóa/preview metadata mẫu bằng khen, validate JSON render bằng whitelist field, không nhận HTML/CSS tùy ý. RD6 bổ sung `ICertificateGenerationService` và `ICertificatePdfStorageService` để sinh/lưu/tải PDF bằng khen Top 100, cập nhật metadata PDF trên `KhenThuong`, batch continue-on-item-failure và audit generate/regenerate. RD7 bổ sung `IStudentRewardService` và `StudentRewardsController` để học sinh xem danh sách và tải bằng khen của chính mình, đảm bảo an toàn truy cập (trả `404` nếu không phải của mình) và ẩn các trường dữ liệu nội bộ. Công bố, notification và workflow kỷ luật vẫn tách task sau.

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
- `ApplicationReviewOperate`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`.
- `ApplicationSensitiveDecision`: `SuperAdmin`, `Admin`, `CampusAdmin`, `Principal`.
- `ApplicationProcessingOperate`: `SuperAdmin`, `Admin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`.
- `ApplicationSystemAdmin`: `SuperAdmin`, `Admin`.

Endpoint có role riêng nên dùng `[Authorize(Roles = ...)]` với `AuthRoles`.

## Applications / Đơn Từ

P0-DT1 chuẩn hóa nền cho module đơn từ. P0-DT2 bổ sung core lifecycle phía sinh viên, gồm tạo nháp, cập nhật, nộp, nộp lại khi bị yêu cầu bổ sung và hủy đơn. P0-DT3 bổ sung upload/download/delete minh chứng an toàn cho sinh viên. P0-DT4 bổ sung hàng đợi admin, tiếp nhận, phân công/phân công lại, SLA và admin download minh chứng. P0-DT5 bổ sung yêu cầu bổ sung, duyệt và từ chối. P0-DT6 bổ sung nền xử lý nghiệp vụ sau duyệt, chỉ auto ghi nhận đơn xác nhận và cho phép ghi nhận kết quả thủ công. P0-DT7 bổ sung reporting overview, chuẩn hóa metadata upload/xóa minh chứng, hỗ trợ legacy metadata và audit tối giản cho evidence; notification và side-effect nghiệp vụ thật vẫn để phase sau.

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
- `ApplicationAdminQueueService` xử lý list, summary, detail, assignee lookup, filter, sort, pagination và SLA projection. Query dùng `AsNoTracking`, projection DTO trực tiếp và không load collection lớn. Queue summary dùng một conditional aggregate SQL query cho các bucket và giữ alias `Active = TotalActive`, `WaitingForSupplement = NeedSupplement`.
- `ApplicationCampusScopeService` re-query current user từ DB, xác nhận `hoat_dong`, role DB và campus DB. SuperAdmin/Admin unrestricted; CampusAdmin lấy current campus + descendants; SubCampusAdmin/AcademicStaff/Principal exact campus.
- List filter ngoài scope trả `403`; detail/download ngoài scope trả `404`; assignee không hợp lệ hoặc ngoài scope trả `404`.
- Queue mặc định chỉ gồm `da_nop` và `dang_xem_xet`; `yeu_cau_bo_sung`, `nhap` và terminal chỉ hiện khi filter rõ.
- SLA dùng `DonTu.HanXuLyLuc` với `ApplicationQueue:SlaWarningBeforeHours` mặc định 24, validate 1..168. `yeu_cau_bo_sung` là `paused`; terminal/không deadline là `none`.
- `ApplicationAssignmentService` xử lý receive/assign/reassign trong transaction `Serializable`, dùng `sp_getapplock` resource `ApplicationWorkflow:{applicationId}` và `RowVersion` base64 8 bytes.
- Receive chỉ cho `da_nop` chưa có assignee, chuyển `dang_xem_xet`, gán `NguoiDuyetHienTai`, không đổi `NguoiXuLyCuoi`, `NgayNop`, `HanXuLyLuc`.
- Assign/reassign chỉ cho `da_nop`, `dang_xem_xet`, `yeu_cau_bo_sung`; same-assignee là no-op nhưng vẫn validate `RowVersion`. Reassign cần lý do nội bộ 10..1000 ký tự.
- Workflow history dùng `NhatKyDuyetDon`: `tiep_nhan`, `phan_cong`, `phan_cong_lai`. Snapshot chỉ chứa assignee IDs và flag lý do, không chứa form data, file key hoặc secret.
- Admin detail không expose raw `SnapshotJson`. `ApplicationTimelineMetadataSanitizer` parse snapshot theo allowlist typed metadata (`operation`, assignee IDs, reason/template flags, changed fields, attachment IDs/file count, decision metadata và processing metadata); malformed JSON, root không phải object, key lạ hoặc key nhạy cảm bị chuyển thành `null` hoặc bỏ qua thay vì phản chiếu raw payload.
- Admin evidence download dùng private evidence object store, enforce campus scope từ `DonTu`, chỉ trả file content với filename/content type sanitized. Response không trả `StorageKey`, `TenFileLuu`, `FileHash`, bucket hoặc local path.

P0-DT4.1 hardening:

- Không thay đổi schema hoặc migration; chỉ sửa read model, query, test và tài liệu.
- Summary aggregate được gắn tag `P0-DT4.1 QueueSummaryAggregate` để test bằng `DbCommandInterceptor` xác minh đúng một aggregate command.
- Test verification dùng TRX làm source of truth cho P0-DT4/P0-DT2/P0-DT3/full suite. Repository có GitHub Actions backend build-only (`dotnet restore`, `dotnet build --configuration Release`) vì API integration test cần SQL Server database isolated dạng `LMS_TEST_*` và local evidence storage.

Admin review and decision P0-DT5:

- `ApplicationDecisionService` xử lý `request-supplement`, `approve`, `reject`; controller chỉ bind request và trả `ApiResponseDto<AdminApplicationDetailDto>`.
- `ApplicationDecisionPermissionEvaluator` là nguồn duy nhất cho rule quyền của decision service và `AdminApplicationAllowedActionsDto`. Allowed actions gồm `CanRequestSupplement`, `CanApprove`, `CanReject` bên cạnh receive/assign/download.
- Role matrix: SuperAdmin/Admin/CampusAdmin có đủ ba action; SubCampusAdmin chỉ request supplement; AcademicStaff chỉ request supplement khi đơn đang giao cho chính mình; Principal chỉ approve/reject trong exact campus; Teacher/Student/Parent không có quyền.
- Mọi action DT5 yêu cầu `DonTu.NguoiDuyetHienTai != null`; đơn chưa tiếp nhận/phân công trả `409`. Campus ngoài scope trả `404`. Role/campus/status actor luôn re-query từ DB qua `ApplicationCampusScopeService`.
- Transition duy nhất: `dang_xem_xet -> yeu_cau_bo_sung`, `dang_xem_xet -> da_duyet`, `dang_xem_xet -> tu_choi`. Terminal state không đổi và đi qua `ApplicationStateMachine.EnsureTransitionAllowed`.
- Tất cả mutation dùng transaction `Serializable`, `sp_getapplock` resource `ApplicationWorkflow:{applicationId}`, RowVersion base64 8 byte và `SaveChangesAsync` một lần. Stale rowversion, deadlock, SQL timeout, lock timeout hoặc concurrency exception được map về `409`.
- Request supplement giữ assignee/deadline, set `NoiDungYeuCauBoSung`, clear `LyDoTuChoi`, set `TrangThaiXuLyNghiepVu = chua_xu_ly`; SLA hiển thị `paused` và resubmit của sinh viên reset deadline theo template.
- Approve chạy `ApplicationApprovalPreconditionValidator` sau khi đã giữ workflow lock: validate exact template đang gắn với đơn, legacy template safe resolution, template JSON, form JSON, related references, submission rule và evidence requirement. Approve set `NgayDuyet`, `NguoiXuLyCuoi`, clear current assignee/reject/supplement và set `TrangThaiXuLyNghiepVu = cho_xu_ly`; không thực thi side effect nghiệp vụ.
- Reject lưu `LyDoTuChoi`, clear current assignee/supplement, không set `NgayDuyet`, set `TrangThaiXuLyNghiepVu = chua_xu_ly`.
- Mỗi decision thêm đúng một `NhatKyDuyetDon` public cho student và một `NhatKyKiemToan` trong cùng transaction. Audit old/new value chỉ chứa status, assignee, last processor và business processing status; không lưu full form, notes, file key/hash hoặc secret.
- Timeline snapshot decision chỉ chứa metadata tối thiểu (`decision`, `previousAssigneeId`, `processorId`) và được sanitize trước khi trả admin detail. Student detail chỉ thấy public note, `LyDoTuChoi`, `NoiDungYeuCauBoSung`; không thấy internal note, audit hoặc raw snapshot.
Post-approval processing P0-DT6:

- `ApplicationPostApprovalProcessingService` xử lý `process` và `record-processing-result`; controller chỉ bind request và trả `ApiResponseDto<AdminApplicationDetailDto>`.
- `ApplicationProcessingPermissionEvaluator` là nguồn rule quyền xử lý sau duyệt và cập nhật `AdminApplicationAllowedActionsDto` với `CanProcessAutomatically`, `CanRecordProcessingResult`.
- Role matrix: SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff được thao tác trong campus scope; Principal/Teacher/Student/Parent không có quyền.
- `ApplicationProcessingStateMachine` quản lý transition nghiệp vụ: `cho_xu_ly -> da_ghi_nhan/xu_ly_thanh_cong/xu_ly_that_bai/can_xu_ly_thu_cong`, `can_xu_ly_thu_cong -> da_ghi_nhan/xu_ly_thanh_cong/xu_ly_that_bai`, `xu_ly_that_bai -> da_ghi_nhan/xu_ly_thanh_cong/can_xu_ly_thu_cong`. Terminal là `da_ghi_nhan`, `xu_ly_thanh_cong`.
- Auto handler hiện chỉ có `ConfirmationApplicationPostApprovalHandler` cho `xac_nhan`: parse form an toàn, ghi envelope vào `KetQuaXuLyJson`, cập nhật `NhatKyTuDong` latest attempt và set `TrangThaiXuLyNghiepVu = da_ghi_nhan`. Handler không tạo chứng chỉ, không gọi service điểm/điểm danh/tài khoản/cơ sở/ngành/lớp.
- Loại đơn chưa có handler tự động chuyển `can_xu_ly_thu_cong` với handler fallback `manual_required_fallback`.
- Ghi nhận kết quả thủ công dùng `ApplicationProcessingResultSanitizer`: result object/null, tối đa 16KB, depth 5, 50 properties, array 100 item, cấm key nhạy cảm như password/token/secret/storage key/file hash/local path/bucket/connection string và cấm script-like text.
- Mọi mutation DT6 dùng transaction `Serializable`, `sp_getapplock` resource `ApplicationWorkflow:{applicationId}`, validate `RowVersion` base64 8 byte và `SaveChangesAsync` một lần. Stale rowversion, deadlock, SQL timeout, lock timeout hoặc concurrency exception được map về `409`.
- `/process` idempotent no-op cho `da_ghi_nhan`, `xu_ly_thanh_cong`, `can_xu_ly_thu_cong` nếu RowVersion đúng; no-op không tạo timeline/audit mới. `chua_xu_ly` trả `409`; đơn chưa `da_duyet` trả `400`.
- Mỗi mutation thực sự thêm một `NhatKyDuyetDon` public `xu_ly_nghiep_vu` và một `NhatKyKiemToan` `xu_ly_nghiep_vu` trong cùng transaction. Audit old/new value chỉ chứa trạng thái, trạng thái xử lý nghiệp vụ, processor cuối và flag có kết quả xử lý.
- Student list/detail chỉ expose `TrangThaiXuLyNghiepVu`, `TenTrangThaiXuLyNghiepVu`; không expose `KetQuaXuLyJson`, `NhatKyTuDong`, internal note hoặc raw snapshot.

- Known limitations P0-DT6: không escalation, không workflow nhiều cấp, không bulk decision, không frontend và chưa có side-effect nghiệp vụ thật cho các loại đơn ngoài `xac_nhan`. Notification trạng thái đơn được tích hợp riêng ở P0-DT8.

Reporting and audit closure P0-DT7:

- `GET /api/admin/applications/reports/overview` nằm trong `AdminApplicationsController`, dùng `ApplicationQueueRead` và `ApplicationReportService`.
- Report service luôn re-query actor qua `ApplicationCampusScopeService`; SuperAdmin/Admin global, CampusAdmin gồm descendants, SubCampusAdmin/AcademicStaff/Principal exact campus. Campus filter ngoài scope trả `403`.
- Filter hỗ trợ alias Việt/Anh: `maDonVi`/`campusId`, `loaiDon`/`type`, `trangThai`/`status`, `trangThaiXuLyNghiepVu`/`processingStatus`, `nguoiDuyetHienTai`/`assigneeId`, `nguoiXuLyCuoi`/`processorId`, `tuNgayNop`/`submittedFrom`, `denNgayNop`/`submittedTo`. Alias khác giá trị trả `400`; ID phải > 0; enum được canonicalize case-insensitive.
- Summary metrics gồm tổng đơn, pending review, waiting supplement, overdue, due soon, approved/rejected/cancelled và các trạng thái xử lý nghiệp vụ. `overdue`/`dueSoon` chỉ tính `da_nop`, `dang_xem_xet` có deadline, không tính supplement/terminal/draft.
- Breakdown status/processing/type luôn trả đủ bucket nghiệp vụ kể cả count 0; campus breakdown chỉ trả campus có dữ liệu trong scope và order theo tên/id.
- Approval/rejection rate dùng denominator `approved + rejected`, làm tròn 2 chữ số; nếu denominator 0 thì rate 0. Average review hours dùng `NgayNop` tới latest timeline `phe_duyet`/`tu_choi`, fallback `NgayDuyet`, loại record thiếu hoặc đảo thời gian.
- Query strategy: `AsNoTracking`, conditional aggregate cho summary tag `P0-DT7 ReportSummary`, group query theo status/processing/type/campus và aggregate review duration bằng `EF.Functions.DateDiffMinute`. Không parse form/result JSON, không `AsEnumerable` trước aggregate, không N+1 campus.
- Evidence future timeline metadata dùng `{ operation: "upload_evidence", fileCount }` và `{ operation: "delete_evidence", attachmentId }`; `ApplicationTimelineMetadataSanitizer` vẫn map legacy `{ attachmentAction: "upload", count }` và `{ attachmentAction: "delete", maTep }` sang metadata typed. Future format được ưu tiên nếu có cả hai.
- Upload/delete evidence thêm `NhatKyKiemToan` trong cùng DB transaction với attachment metadata, `DonTu.NgayCapNhat` và hidden timeline. Audit old/new chỉ chứa `activeFileCount`, `totalSizeBytes`; `MoTa` phân biệt upload/delete; không chứa filename, `StorageKey`, `TenFileLuu`, `FileHash`, path, bucket, bytes hoặc full entity.
- Known limitations P0-DT7: không export Excel/PDF, không dashboard chart, không trend analytics, không business-hour SLA, không post-approval duration analytics, không advanced audit search và không frontend.

Application notification integration P0-DT8:

- `IApplicationNotificationService`/`ApplicationNotificationService` là side-effect layer nội bộ giữa module Đơn từ và `NotificationService`. Controller không tự build payload notification.
- Các event đã gửi thông báo cho học sinh: nộp đơn, tiếp nhận/phân công xử lý, yêu cầu bổ sung, duyệt, từ chối, hủy, xử lý nghiệp vụ đã ghi nhận/thành công/thất bại/cần xử lý thủ công.
- Notification dùng `ThongBao.LoaiThongBao = system`, `LoaiDoiTuongLienKet = DonTu`, `MaDoiTuongLienKet = MaDonTu`, `DuongDan = /student/applications/{id}` và Editor.js JSON hợp lệ có metadata tối thiểu `eventType`, `maDonTu`, `loaiDon`, `trangThaiMoi`, `tieuDe`.
- Dedup nhẹ theo student + linked `DonTu` + title event để không gửi trùng cùng một transition trong các flow idempotent/service retry.
- Notification là kết quả phụ: lỗi từ Notification Center bị catch/log warning trong `ApplicationNotificationService` và không rollback trạng thái đơn, timeline hoặc audit nghiệp vụ chính.
- Payload student không chứa `GhiChuNoiBo`, raw `SnapshotJson`, `KetQuaXuLyJson`, `NhatKyTuDong`, raw form data, storage key, file hash hoặc stack trace.
- Known limitations P0-DT8: chưa gửi email/push/SMS, chưa gửi phụ huynh/admin assignee, chưa reminder/SLA notification, chưa template notification riêng cho Đơn từ và chưa frontend integration.

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
- Timeline evidence upload/delete là hidden với student, nhưng admin detail thấy metadata typed đã sanitize. Audit evidence chỉ giữ số file active/tổng dung lượng trước-sau để phục vụ closure mà không lộ metadata lưu trữ.
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

## Notification Center Core

P0-NT-Core tách notification thành hai lớp:

- `ThongBao`: nội dung chung của một lần gửi, gồm loại, mức độ, phạm vi, Editor.js JSON/text, đối tượng liên kết, trạng thái gửi và audit metadata.
- `ThongBaoNguoiNhan`: trạng thái từng người nhận, gồm `DaDoc`, `DocLuc`, `DaAn`, `AnLuc`, `NhanLuc`.

`NotificationService` là facade chính cho API user/admin và các module backend khác. Các method legacy `CreateSystemNotificationAsync`, `SendToUsersAsync`, `SendToClassAsync`, `SendToCourseAsync`, `SendToCampusAsync` vẫn được giữ để P0-5/P0-7/P0-9 tiếp tục compile/runtime. Admin create dùng transaction có execution strategy, resolve recipient server-side và rollback nếu insert recipient/log lỗi.

P0-DT8 dùng facade này để gửi notification trạng thái đơn từ cho học sinh. `SystemNotificationRequest.DuongDan` được map vào `ThongBao.DuongDan` để inbox có link về chi tiết đơn.

Authorization:

- SuperAdmin quản lý toàn hệ thống.
- Admin/CampusAdmin/AcademicStaff chỉ quản lý notification trong campus scope hiện tại; CampusAdmin bao gồm descendants.
- User thường chỉ xem/đọc/ẩn notification của chính mình.

Editor.js validation ở mức MVP: JSON phải là object, `blocks` nếu có phải là array, không nhận `data:image`/base64 trong JSON, extract text từ paragraph/header/list/quote/table và sinh summary khi thiếu.

Audit/logging: admin create/cancel ghi `NhatKyKiemToan`; mỗi recipient gửi ghi `NhatKyThongBao` kênh `thong_bao_day`. Không ghi full content JSON lớn vào audit.

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
