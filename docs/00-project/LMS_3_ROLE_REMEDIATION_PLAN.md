# Kế hoạch khắc phục nhận xét hội đồng cho BGH, Giảng viên và Sinh viên

> Ngày lập: 13/08/2026  
> Trạng thái: Kế hoạch, chưa triển khai code  
> Mục tiêu: sửa luồng cốt lõi trước, chỉ mở rộng khi có bằng chứng chạy thật

## 1. Phạm vi bắt buộc

### Trong phạm vi

- Vai trò BGH (`Principal`): quản lý nhân sự giảng viên theo cơ sở, xem năng lực/chuyên môn, tải giảng dạy, nhật ký ca dạy, đánh giá và xem cây phạm vi quản lý.
- Vai trò Giảng viên (`Teacher`): xem hồ sơ chuyên môn của chính mình, nguyện vọng, lịch dạy rõ ràng, lớp, điểm danh, bài tập/chấm bài và coi thi.
- Vai trò Sinh viên (`Student`): xem lịch học đã công bố, nộp file bài tập, làm bài thi, nhận kết quả và đánh giá giảng viên.
- Backend, database và test dùng chung chỉ được sửa khi phục vụ trực tiếp ba vai trò trên.

### Ngoài phạm vi đợt này

- Không sửa giao diện Giáo vụ, Super Admin, Phụ huynh hoặc các role khác.
- Không triển khai hoặc kiểm thử xếp lịch tự động, sinh bản nháp, duyệt/publish bản nháp, thuật toán xếp lịch hay benchmark xếp lịch. Toàn bộ chức năng này do thành viên khác phụ trách.
- Yêu cầu “Giáo vụ cùng BGH quản lý giảng viên” được ghi nhận là quy tắc phân quyền mục tiêu. Đợt này chỉ hiện thực và demo phía BGH; giao diện Giáo vụ do kế hoạch role khác xử lý.
- Yêu cầu “Super Admin chỉ xem BGH” không sửa trong đợt này vì thuộc role Super Admin.
- Yêu cầu chỉ quản lý phụ huynh đối với người học dưới 18 tuổi không sửa trong đợt này vì role Phụ huynh nằm ngoài phạm vi. Khi xử lý role Phụ huynh phải dùng ngày sinh thật; `NguoiDung` hiện chưa có trường ngày sinh nên chưa được phép suy đoán tuổi từ `NamNhapHoc`.
- Không bổ sung tính năng chỉ có trong note nhưng không tìm thấy nền tảng tương ứng trong code, trừ phần Quản lý nhân sự được yêu cầu rõ trong đề bài này.
- Không làm AI, dashboard lớn hoặc hiệu ứng giao diện trước khi các luồng cốt lõi đạt tiêu chí nghiệm thu.

## 2. Kết quả kiểm tra code hiện tại

| Nhóm | Đã có trong code | Vấn đề phải xử lý |
| --- | --- | --- |
| BGH - người dùng | `/bgh/users`, `GET /api/bgh/users` có phân trang và scope theo cơ sở | BGH chỉ xem. `UsersView.vue` chỉ cho `SuperAdmin/Admin` sửa; BGH không quản lý được giảng viên. Nút nhập Excel chỉ `setTimeout` rồi báo thành công, không tải file lên backend. |
| BGH - phân quyền | Có `VaiTro`, `PhanQuyenNguoiDung`, cây đơn vị và danh sách vai trò | Dữ liệu quyền hiện chỉ là role gán cho user; chưa có mô hình permission/action theo cây. `RolesView.vue` là bảng phẳng, BGH chỉ đọc và route `/bgh/roles` còn không nằm trong menu BGH. |
| Hồ sơ giảng viên | Có `GiaoVienMonHoc`, `GiaoVienChuyenNganh`, mức phù hợp, số năm kinh nghiệm, số lần đã dạy, môn/chuyên môn chính | `NguoiDung` chỉ có thông tin tài khoản. Chưa có học vị, bằng cấp, chứng chỉ, chuyên môn mô tả hoặc hồ sơ nhân sự tổng hợp. |
| Tải và chất lượng giảng dạy | Có `TeacherAcademicWorkloadService`, `KhoaHoc`, `ThoiKhoaBieu`, `BuoiHoc`, điểm danh và `DanhGiaGiaoVien` | Chưa có API BGH gom thành một hồ sơ: số lớp, số ca, dạy thay, buổi hủy, điểm danh đúng hạn, phản hồi sinh viên và lịch sử theo học kỳ. Không được gọi điểm đánh giá sinh viên là toàn bộ “hiệu suất giảng viên”. |
| Lịch Giảng viên/Sinh viên | Có `TeacherScheduleController`, `StudentScheduleController`, hai màn lịch và API theo học kỳ/ngày | Cần chứng minh cùng một lịch sau công bố xuất hiện nhất quán ở BGH, Giảng viên và Sinh viên. Chưa có bài smoke xuyên ba role cho luồng này. |
| Thi và chấm thi | Có start/autosave/submit, chấm trắc nghiệm, Teacher results, SignalR/WebRTC proctoring | Chỉ tìm thấy test trực tiếp cho `QuizGradingService`; chưa có integration/load test cho nhiều sinh viên thi cùng lúc. `PhienThiHocSinh` chưa có row version. Autosave và submit có nguy cơ ghi đè khi đến đồng thời. |
| Màn coi thi | Có API ca thi, thí sinh, vi phạm, biên bản và SignalR client | `teacherApi.js` vẫn gán mặc định thí sinh “present”, tự suy diễn trạng thái stream và trả `logs: []`. Phải bỏ suy diễn trước khi demo. |
| Chứng minh tải file | Sinh viên có API nộp một file bài tập thật; đơn từ có API tải minh chứng | Nút Excel ở BGH là fake success. Chứng minh file trong đợt này phải dùng luồng nộp bài thật hoặc minh chứng đơn từ. |

## 3. Thứ tự ưu tiên

Không bắt đầu P1 khi P0 chưa đạt cổng nghiệm thu. Không bắt đầu P2/P3 chỉ để có thêm màn hình demo.

### P0 - Dọn sai lệch và khóa claim sai

1. Bỏ fake success tại màn nhập Excel của BGH.
   - Hoặc nối API thật, hoặc ẩn/disable nút kèm thông báo “chưa hỗ trợ”.
   - Không giữ giao diện báo “đã nhập thành công” khi backend không nhận file.
2. Bỏ dữ liệu suy diễn trong coi thi.
   - Không mặc định mọi thí sinh là `present`.
   - Không tự gán `streaming/stopped/waiting` nếu backend/SignalR chưa xác nhận.
   - Không hiển thị log rỗng như dữ liệu thật.
3. Tạo bộ dữ liệu demo an toàn, nhỏ và có thể reset cho ba role.

**Cổng P0:** không còn toast thành công giả; không còn status coi thi suy diễn; dữ liệu demo của ba role có thể reset an toàn.

### P1 - Chức năng cơ bản và cốt lõi

#### P1.1. Quản lý nhân sự giảng viên cho BGH

Tạo module “Nhân sự giảng viên” riêng thay vì dùng màn “Quản lý người dùng” chung.

Chức năng tối thiểu:

- Danh sách chỉ gồm giảng viên trong cơ sở/phạm vi BGH.
- Tìm kiếm, lọc theo đơn vị, chuyên ngành, môn có thể dạy, trạng thái và học kỳ.
- Xem chi tiết hồ sơ:
  - thông tin tài khoản;
  - học vị/trình độ và chứng chỉ có minh chứng;
  - chuyên ngành chính/phụ;
  - môn được phép dạy, mức phù hợp, kinh nghiệm, số lần đã dạy;
  - số lớp và số ca/tuần trong học kỳ;
  - nguyện vọng ca dạy;
  - đánh giá sinh viên, có cỡ mẫu và học kỳ;
  - nhật ký ca dạy.
- BGH được tạo/cập nhật/khóa giảng viên trong cơ sở của mình; không được tạo hoặc gán `SuperAdmin`, `Admin`, `Principal`.
- Mọi thay đổi hồ sơ, chuyên môn, trạng thái và phân công đều ghi audit với người thực hiện, thời điểm, giá trị cũ/mới và lý do.

Nhật ký ca dạy phải lấy từ dữ liệu thật:

- Ca được phân công từ `KhoaHoc` + `ThoiKhoaBieu` + `BuoiHoc`.
- Phân biệt giảng viên chính và giảng viên dạy thay.
- Trạng thái buổi học, hủy/dời/đổi phòng, thời điểm mở/gửi/khóa điểm danh.
- KPI chỉ tính từ dữ liệu có nguồn: tổng ca, đã diễn ra, bị hủy, dạy thay, điểm danh gửi đúng hạn/trễ/chưa gửi.
- Không tự kết luận “dạy tốt/kém” chỉ từ một chỉ số. Màn hình phải hiển thị nguồn và kỳ đo của từng chỉ số.

#### P1.2. Cây phạm vi quản lý và phân quyền

Giai đoạn đầu chỉ hiển thị dữ liệu quyền đang có thật:

```text
Đơn vị/cơ sở
└── Vai trò
    └── Người dùng
        ├── Vai trò chính
        └── Vai trò bổ sung
```

- Cây lấy từ `DonVi`, `VaiTro`, `PhanQuyenNguoiDung`, không hardcode danh sách người dùng.
- Mỗi node hiển thị phạm vi BGH được xem/quản lý.
- BGH chỉ được thao tác trên node Giảng viên thuộc cơ sở của mình.
- Quyền hệ thống chi tiết theo action/resource là hạng mục P3 vì database hiện chưa có mô hình permission. Không dựng cây checkbox giả trước khi có bảng và API thật.

Ma trận quyền mục tiêu trong phạm vi đợt này:

| Hành động | BGH | Giảng viên | Sinh viên |
| --- | --- | --- | --- |
| Xem danh sách giảng viên trong cơ sở | Có | Không | Không |
| Tạo/sửa/khóa hồ sơ giảng viên | Có, theo scope | Chỉ đề nghị sửa hồ sơ của mình | Không |
| Sửa chuyên môn/môn được dạy | Có, ghi audit | Không | Không |
| Xem lịch dạy cá nhân | Có khi xem hồ sơ | Có | Không |
| Xem lịch học cá nhân | Không | Không | Có |
| Xem lịch đã công bố | Có | Có | Có |
| Xem đánh giá giảng viên | Có, dữ liệu tổng hợp | Chỉ kết quả của mình nếu được công bố | Gửi đánh giá của mình |

#### P1.3. Ba luồng cốt lõi cần nghiệm thu

| Role | Luồng phải chạy trước tính năng lớn |
| --- | --- |
| BGH | Đăng nhập -> Nhân sự giảng viên -> xem năng lực/tải/ca dạy -> xem audit |
| Giảng viên | Đăng nhập -> lịch dạy -> lớp -> điểm danh -> xem bài nộp/chấm -> coi thi |
| Sinh viên | Đăng nhập -> khóa học -> nộp file -> lịch học -> làm thi -> xem kết quả |

### P2 - Độ tin cậy và hiệu suất

#### P2.1. Thi/chấm thi đồng thời

Sửa độ an toàn dữ liệu trước khi test tải:

- Thêm optimistic concurrency hoặc câu lệnh update có điều kiện cho phiên thi.
- Autosave mang version/timestamp; request cũ không được ghi đè câu trả lời mới.
- Submit idempotent: gửi lặp do mạng chỉ tạo một kết quả.
- Start đồng thời phải trả lại phiên đang hoạt động hoặc lỗi nghiệp vụ rõ, không để unique constraint thành lỗi 500.
- Không nuốt lỗi chấm tự động rồi vẫn coi là hoàn tất. Phải có trạng thái/chẩn đoán và khả năng retry an toàn.
- Tách phép đo HTTP/SQL khỏi phép đo tải media WebRTC. WebRTC P2P của nhiều sinh viên không được đánh đồng với tải API thi.

Kịch bản load test tối thiểu:

| Giai đoạn | Tải | Hành vi |
| --- | --- | --- |
| Baseline | 1 sinh viên | start -> lấy câu hỏi -> autosave -> submit -> đọc kết quả |
| Nhỏ | 25 sinh viên đồng thời | start trong 30 giây, autosave mỗi 15 giây, submit rải đều |
| Trung bình | 50-100 sinh viên | autosave đồng thời và spike submit cuối giờ |
| Stress | 200 sinh viên hoặc tới khi vượt ngưỡng | xác định điểm bão hòa, không dùng để claim production |

Tiêu chí dự kiến cần xác nhận bằng môi trường demo thật:

- Không mất câu trả lời, không có phiên trùng, không chấm hai lần.
- HTTP 5xx dưới 1% trong tải mục tiêu.
- p95 start dưới 2 giây, autosave dưới 1 giây, submit dưới 3 giây.
- Sau test, đối chiếu số sinh viên bắt đầu/nộp/kết quả bằng query SQL.
- Ghi CPU, RAM, SQL connection, request rate và SignalR connection; không chỉ ghi “PASS”.

### P3 - Tính năng lớn sau khi lõi ổn định

- Permission catalog theo resource/action và cây checkbox có API/database thật.
- Phân tích hiệu suất giảng viên nâng cao, chỉ khi thống nhất công thức nghiệp vụ và nguồn dữ liệu.
- Dashboard xu hướng dài hạn, cảnh báo, AI feedback.
- Import hàng loạt `.xlsx` nếu thật sự cần; phải có preview, validate từng dòng, dry-run, báo lỗi và transaction. Không thêm dependency chỉ để phục vụ demo.
- Kiến trúc giám sát thi quy mô lớn (giới hạn số luồng xem đồng thời hoặc SFU) sau khi đo P2P hiện tại.

## 4. Các file dự kiến khi triển khai

Danh sách này là định hướng để chia việc; phải kiểm tra lại worktree trước mỗi phase.

| Hạng mục | File/thư mục dự kiến |
| --- | --- |
| Hồ sơ nhân sự giảng viên | `Backend/Models/HoSoChuyenMonGiaoVien.cs` (dự kiến), `Backend/DTOs/TeacherPersonnel/`, `Backend/Services/TeacherPersonnel/`, `Backend/Controllers/BghTeacherPersonnelController.cs` (dự kiến), `ApplicationDbContext.cs`, migration có chủ đích |
| BGH nhân sự | `frontend/src/views/BGH/HumanResources/` (dự kiến), `frontend/src/services/bghPersonnelApi.js` (dự kiến), `frontend/src/components/BGH/data/menuData.js`, `frontend/src/router/index.js` |
| Cây phạm vi | `Backend/Services/Rbac/`, DTO tree dự kiến, `frontend/src/views/BGH/RolesView.vue` hoặc view mới sau khi review |
| Lịch cá nhân hiện có | Chỉ đọc/hiển thị ca dạy và ca học tại `frontend/src/views/GiangVien/TeachingScheduleView.vue`, `frontend/src/views/Student/ScheduleView.vue` cùng API tương ứng; không sửa luồng xếp lịch tự động |
| Thi đồng thời | `Backend/Models/PhienThiHocSinh.cs`, `ApplicationDbContext.cs`, `Backend/Services/Exam/ExamService.cs`, `Backend/Services/QuizAttempts/QuizAttemptService.cs`, migration và API tests |
| Coi thi | `frontend/src/services/teacherApi.js`, `frontend/src/services/examProctoringHub.js`, các view `frontend/src/views/GiangVien/Proctoring*` và `Student/ExamTakeView.vue` |
| Nộp file thật | `Backend/Controllers/StudentAssignmentsController.cs`, storage service, `frontend/src/views/Student/AssignmentDetailView.vue`, view bài nộp của Giảng viên |
| Kiểm thử | `Backend.ApiTests/` cho nhân sự và concurrency thi; script load trong `docs/artifacts/<task>/` |
| Hợp đồng/tài liệu | `docs/API_CONTRACT.md`, báo cáo nghiệm thu phase và evidence JSON/ảnh |

Không sửa đồng thời tất cả file trên. Mỗi PR chỉ làm một lát cắt dọc có API, UI, test và evidence.

## 5. Cách chứng minh, không nói suông

### Bộ bằng chứng bắt buộc

Mỗi luồng demo phải có đủ:

1. Ảnh màn hình có role, thời gian và ID dữ liệu thật.
2. Network/API response lưu `.json`, che token và dữ liệu nhạy cảm.
3. Query SQL đối chiếu trước/sau đối với thao tác ghi.
4. Audit log cho thao tác BGH.
5. Kết quả test tự động và môi trường chạy.

### Chứng minh nộp file

Dùng luồng đã có thật thay vì giả lập:

1. Sinh viên chọn một file `.pdf` hợp lệ, tên cố định cho buổi demo.
2. Network ghi nhận `POST /api/student/assignments/{id}/submit` thành công.
3. Refresh trang vẫn thấy lần nộp từ database/storage.
4. Giảng viên mở đúng bài nộp và tải file xuống.
5. So sánh SHA-256 file gốc và file tải xuống; hai hash phải giống nhau.
6. Thử file sai định dạng/quá dung lượng và lưu bằng chứng bị chặn.

Không dùng nút nhập Excel BGH để demo trước khi có endpoint thật.

### Chứng minh thi nhiều người

- Xuất report gồm cấu hình máy, số user, duration, p50/p95/p99, error rate, CPU/RAM/SQL và số bản ghi sau test.
- Giữ file request/response mẫu đã ẩn token.
- Có test nộp lặp, autosave đến trễ, mất kết nối và reconnect.
- Nếu chưa đạt ngưỡng thì nói đúng điểm bão hòa, không dùng từ “production-ready”.

## 6. Definition of Done toàn đợt

- Chỉ ba role BGH/Giảng viên/Sinh viên có thay đổi giao diện.
- Không có mock/fallback/fake success trong các luồng được demo.
- BGH quản lý được giảng viên theo scope, có hồ sơ năng lực và nhật ký ca dạy thật.
- Cây quản lý phản ánh dữ liệu `DonVi`/role/user thật; không có checkbox permission giả.
- File sinh viên tải lên được Giảng viên tải xuống và đối chiếu hash.
- Thi có integration test, concurrency test và load report; không chỉ có test chấm điểm đơn vị.
- Backend build/test, frontend build/test/lint và Docker rebuild theo `AGENTS.md` đều đạt.
- Evidence không chứa token, mật khẩu, connection string hoặc dữ liệu cá nhân nhạy cảm.

## 7. Project map rút gọn đã đối chiếu

Luồng dữ liệu chính:

```text
BGH / Giảng viên / Sinh viên
        -> Vue Router + role layout
        -> service API theo module
        -> ASP.NET Controller
        -> Service nghiệp vụ
        -> EF Core / SQL Server
        -> Audit + response theo scope
```

```json
{
  "purpose": "LMS / Academic Management System",
  "scope": ["Principal", "Teacher", "Student"],
  "stack": {
    "backend": "ASP.NET Core net10.0 + EF Core 10 + SQL Server + JWT + SignalR",
    "frontend": "Vue 3 + Vite + Pinia + Vue Router + Tailwind",
    "tests": "NUnit API tests + Vitest"
  },
  "modules": [
    {
      "name": "BGH",
      "boundaries": ["UI", "API", "Domain"],
      "key_files": [
        "frontend/src/views/BGH",
        "Backend/Controllers/BghFacadeController.cs",
        "Backend/Controllers/BghEvaluationController.cs"
      ]
    },
    {
      "name": "Teacher",
      "boundaries": ["UI", "API", "Domain"],
      "key_files": [
        "frontend/src/views/GiangVien",
        "Backend/Controllers/TeacherScheduleController.cs",
        "Backend/Controllers/TeacherClassesController.cs"
      ]
    },
    {
      "name": "Student",
      "boundaries": ["UI", "API", "Domain"],
      "key_files": [
        "frontend/src/views/Student",
        "frontend/src/views/SinhVien",
        "Backend/Controllers/StudentScheduleController.cs",
        "Backend/Controllers/StudentAssignmentsController.cs"
      ]
    },
    {
      "name": "Exam",
      "boundaries": ["UI", "API", "Domain", "Data", "Infrastructure"],
      "entities": ["CaThi", "PhienThiHocSinh", "NhatKyViPhamThi", "BienBanThi"]
    },
    {
      "name": "TeacherPersonnel",
      "status": "can_bo_sung",
      "reuses": ["NguoiDung", "GiaoVienMonHoc", "GiaoVienChuyenNganh", "KhoaHoc", "BuoiHoc", "DanhGiaGiaoVien"]
    }
  ],
  "terms": {
    "NguoiDung": "User",
    "GiaoVienMonHoc": "Teacher subject capability",
    "GiaoVienChuyenNganh": "Teacher specialization",
    "KhoaHoc": "Course offering",
    "ThoiKhoaBieu": "Weekly timetable",
    "BuoiHoc": "Teaching session",
    "PhienThiHocSinh": "Student exam attempt"
  }
}
```
