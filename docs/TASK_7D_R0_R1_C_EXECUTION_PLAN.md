# PROMPT THỰC THI CHÍNH THỨC V2 — TASK 7D-R0, 7D-R1 VÀ 7D-C

> Tài liệu này là **Master Contract**. Antigravity phải đọc toàn bộ một lần, nhưng chỉ thực thi task được kích hoạt bằng prompt ngắn ở cuối tài liệu. Không được tự chuyển sang task tiếp theo.

## 0. Mục tiêu và giới hạn tuyệt đối

Bạn đang tiếp tục hoàn thiện **Smart Timetable** của dự án AET LMS. Chỉ thực hiện đúng ba task theo thứ tự bắt buộc:

1. **Task 7D-R0** — thống nhất cách tính sĩ số/phòng và readiness thật.
2. **Task 7D-R1** — sửa các lỗi UX nghiệp vụ nguy hiểm cho người dùng non-tech.
3. **Task 7D-C** — bổ sung component test và kiểm tra live UX bằng AcademicStaff.

Không bắt đầu hoặc triển khai bất kỳ task nào khác. Đặc biệt:

- Không làm Task 7E, không tích hợp AI/chat/Ollama.
- Không làm Task 7F.
- Không dọn toàn bộ 340 lỗi ESLint cũ.
- Không sửa các module không liên quan đến Smart Timetable.
- Không sửa lỗi chỉ dành riêng cho SuperAdmin. Nếu phát hiện, ghi `SUPERADMIN_OUT_OF_SCOPE` trong báo cáo rồi bỏ qua, trừ khi lỗi đó cũng ảnh hưởng build/startup/migration/security hoặc role dùng chung.
- Không sửa 20 vi phạm sức chứa lịch sử đã biết: 2 ca của HK1 2026 và 18 ca của HK Large V10.
- Không sửa lịch đã xuất bản, `BuoiHoc`, `DiemDanh` hoặc dữ liệu lịch sử chỉ để làm test xanh.
- Không dùng SuperAdmin để kiểm thử.
- Không đụng R2, PayOS, secret, credential hoặc Git history.
- Không đụng thay đổi có sẵn trong `frontend/src/components/BGH/performance/bghExport.js`.
- Không stage, commit hoặc push trong cả ba task. Chỉ báo cáo danh sách file đề xuất commit sau khi cả ba task PASS và người dùng duyệt.

Nếu yêu cầu kỹ thuật trong prompt mâu thuẫn với schema/code thực tế, không được đoán. Hãy dừng phần đó, đưa bằng chứng từ model/schema/DB hiện tại và đề xuất phương án tối thiểu để người dùng duyệt.

## 0.1. Cách vận hành tối ưu cho Antigravity

Không triển khai cả ba task trong một lượt. Trình tự bắt buộc:

```text
Đọc Master Contract
        ↓
Kích hoạt 7D-R0 Discovery
        ↓
Người dùng duyệt Discovery
        ↓
Triển khai và nghiệm thu 7D-R0
        ↓
Người dùng duyệt R0
        ↓
Triển khai và nghiệm thu 7D-R1
        ↓
Người dùng duyệt R1
        ↓
Thực hiện 7D-C
```

Mục tiêu của cách vận hành này là phát hiện sai nguồn dữ liệu trước khi lỗi lan từ backend sang frontend và test.

### Quy tắc tự chủ

Trong task đang được kích hoạt, Antigravity được tự chủ:

- tìm kiếm code trên toàn repository;
- đọc model, migration, seeder, service, controller, DTO và frontend consumer liên quan;
- chạy build/test/lint an toàn;
- sửa các file đã liệt kê trong kế hoạch triển khai được duyệt;
- xử lý lỗi nhỏ trực tiếp phát sinh từ chính thay đổi của task.

Antigravity phải dừng và xin duyệt nếu:

- cần migration/schema mới;
- cần sửa file/module ngoài danh sách đã báo ở Discovery;
- phát hiện quy tắc trong prompt trái schema/dữ liệu thật;
- có nguy cơ mutate database không mang tên `LMS_TEST_*`;
- cần Publish thật;
- cần xóa hoặc sửa dữ liệu lịch sử;
- cần mở rộng sang SuperAdmin, secrets, R2, PayOS, AI hoặc task khác;
- test cho thấy baseline 30/30 bị suy giảm nhưng chưa xác định được nguyên nhân.

### Quy tắc sử dụng công cụ

- Dùng tìm kiếm repository để lập danh sách đầy đủ consumer trước khi sửa shared policy.
- Dùng terminal cho build/test/DB evidence; không suy luận kết quả từ code inspection.
- Dùng browser thật cho 7D-C; component test không thay thế live browser.
- Được chạy các kiểm tra độc lập song song chỉ khi chúng không mutate chung DB hoặc cùng sửa một file.
- Các test mutate DB phải chạy tuần tự và có safety guard, transaction/cleanup.
- Không để hai tiến trình test cùng thay đổi một fixture/học kỳ.
- Không chạy lặp build toàn dự án sau từng thay đổi nhỏ; chạy targeted test gần nhất trước, build/regression gate ở cuối lát cắt.

---

## 0.2. DISCOVERY GATE — bắt buộc trước khi viết code R0

Ở lần kích hoạt đầu tiên, **chỉ điều tra, chưa sửa code**. Antigravity phải xuất một báo cáo Discovery gồm:

### A. Bản đồ code thực tế

| Chủ đề | Bằng chứng bắt buộc |
|---|---|
| Nguồn sĩ số | File, class, method, query/field đang dùng |
| Trạng thái enrollment hợp lệ | Giá trị thật từ model/seeder/DB |
| Trạng thái học sinh hoạt động | Giá trị thật, không đoán |
| Capacity service hiện có | Interface, implementation và consumer |
| Candidate phòng của solver | File/method |
| Capacity trong conflict check | File/method |
| Capacity khi save/publish | File/method |
| Readiness backend | Endpoint, DTO, service |
| Readiness frontend | File/computed/function và mọi suy luận cục bộ |
| Payload Generate | Nguồn danh sách khóa học và API caller |
| Tất cả đường Publish | Frontend caller → endpoint → service |
| Polling/recovery | Function, interval, điều kiện dừng |
| Error contract | HTTP status, response DTO, code/message hiện có |

### B. Source-of-truth matrix hiện tại và mục tiêu

Antigravity phải điền ma trận bằng tên class/method thật:

| Quy tắc | Nguồn hiện tại | Nguồn mục tiêu duy nhất | Tất cả consumer |
|---|---|---|---|
| Sĩ số yêu cầu | Phải khảo sát | Capacity policy/service | readiness, solver, conflict, save, publish |
| Phòng hợp lệ | Phải khảo sát | Shared room eligibility policy | solver, suggestion, save, conflict, publish |
| GV hợp lệ | Phải khảo sát | Shared teacher eligibility/constraint policy | readiness, solver, edit, publish |
| Học kỳ được xếp | Phải khảo sát | Authenticated academic context | frontend, Generate |
| Trạng thái job | Phải khảo sát | Generation job backend | polling, recovery |
| Lỗi nghiệp vụ | Phải khảo sát | Stable error code/reason | frontend, tests |

### C. Impact graph

Với mỗi shared policy dự kiến sửa, liệt kê mọi nhánh downstream. Tối thiểu phải chứng minh đã tìm các nhánh:

```text
Capacity policy
├── Readiness
├── Solver
├── Room suggestion
├── Create/update validation
├── Conflict check
├── Draft validation
└── Publish validation
```

### D. Kế hoạch diff

- File dự kiến sửa.
- File test dự kiến thêm/sửa.
- Root cause cụ thể tương ứng mỗi file.
- Test nào sẽ fail trước sửa.
- Test nào xác minh sau sửa.
- Bất kỳ điểm nào trong Master Contract không khớp code thực tế.

### E. Cổng duyệt Discovery

Sau báo cáo Discovery, dừng và chờ người dùng duyệt. Không được tự sửa code hoặc chuyển sang implementation R0.

---

## 0.3. CHANGE BUDGET — kiểm soát phạm vi thay đổi

- Chỉ sửa file nằm trong kế hoạch diff đã được duyệt.
- Nếu cần thêm file, báo tên file, lý do và tác động trước khi sửa.
- Không format toàn file vì một thay đổi nhỏ.
- Không đổi tên hàng loạt.
- Không nâng dependency/package.
- Không tạo abstraction mới nếu service hiện có có thể chuẩn hóa an toàn.
- Không thêm migration nếu chưa chứng minh code/schema hiện tại không thể đáp ứng.
- Không hard-code ID học kỳ, campus hoặc tổng số khóa vào production.
- `HK1_2027`, campus 14 và 30 khóa chỉ là fixture/evidence của LargeDemo.
- Sau mỗi lát cắt, chạy `git diff --stat`, `git diff --check` và rà file ngoài phạm vi.

---

## 0.4. FAILURE-FIRST EVIDENCE

Với mỗi lỗi được sửa:

1. Tái hiện hành vi sai trên code hiện tại.
2. Ghi bằng chứng trước sửa: test fail, payload sai, query sai hoặc trace rõ ràng.
3. Thêm test regression khiến phiên bản sai thất bại, nếu khả thi.
4. Sửa root cause tối thiểu.
5. Chứng minh test mới PASS.
6. Chạy regression gần nhất.
7. Kiểm tra DB/diff không có tác dụng phụ.

Không bắt buộc viết test trước nếu lỗi chỉ tái hiện được trong live browser, nhưng phải lưu network/state evidence trước và sau.

---

## 0.5. BA TẦNG KIỂM THỬ

### Tầng 1 — Pure business tests

Không cần DB thật, dùng cho:

- thứ tự nguồn sĩ số;
- distinct/dedup policy;
- không fallback thành 0;
- room/teacher eligibility;
- readiness status;
- error code mapping;
- UX state transition thuần.

### Tầng 2 — SQL integration trên `LMS_TEST_*`

Dùng cho:

- enrollment/status/campus query thật;
- fallback class membership;
- capacity và room-slot;
- teacher availability/capacity;
- consistency giữa solver/conflict/publish;
- unauthorized mutation;
- cleanup/integrity.

### Tầng 3 — Live browser LargeDemo

Dùng cho:

- authenticated context thật;
- Generate payload/call count;
- progress và recovery;
- draft grouping;
- publish confirmation;
- thông báo lỗi mà AcademicStaff thực sự nhìn thấy.

Không dùng tầng trên để thay thế tầng dưới khi acceptance criteria yêu cầu cả hai.

---

## 0.6. QUY TẮC CHỐNG FAKE TEST

- Không mock chính function/policy đang được kiểm tra.
- Không mock readiness backend rồi dùng kết quả đó để tuyên bố backend readiness đúng.
- Không chỉ assert HTTP status; negative test phải assert đúng stable `code/reason`.
- Không gây sai học kỳ để nhận một lỗi 409 khác rồi gọi là PASS.
- Sau request bị chặn phải kiểm tra DB không phát sinh mutation.
- Component test phải assert event, payload, API call count và state transition.
- Browser test phải kiểm tra network thật; screenshot đơn lẻ không chứng minh payload.
- Không chỉnh DOM/state bằng DevTools để tạo bằng chứng.
- Không thay expected result để khớp code sai.
- Không catch/skip test để tạo số liệu xanh.
- Test bị skip trong acceptance scope đồng nghĩa gate chưa PASS.

---

## 0.7. FIXTURE VÀ AN TOÀN DATABASE

Ưu tiên test fixture/helper dùng lại được cho các negative scenario thay vì các đoạn SQL rời rạc.

Fixture/helper phải:

- kiểm tra `DB_NAME()` thực tế trước mutation;
- chỉ cho phép `LMS_TEST_*`;
- tạo mã/ID riêng cho test;
- theo dõi toàn bộ record đã tạo;
- không phụ thuộc thứ tự test;
- cleanup trong `finally` hoặc transaction rollback;
- không xóa theo phạm vi rộng;
- kiểm tra cleanup sau test;
- không chứa connection string/secret trong source.

Các test mutate cùng database chạy tuần tự.

---

## 0.8. NHẬT KÝ TIẾN ĐỘ VÀ CHECKPOINT

Antigravity phải duy trì checklist ngắn trong báo cáo làm việc:

```text
[ ] Discovery được duyệt
[ ] Failure evidence thu thập
[ ] Implementation hoàn tất
[ ] Targeted tests PASS
[ ] SQL integration PASS
[ ] Frontend behavior PASS
[ ] Live browser PASS (chỉ 7D-C)
[ ] Cleanup/integrity PASS
[ ] Diff review PASS
```

Không gửi log dài liên tục. Chỉ cập nhật khi hoàn thành một lát cắt, phát hiện blocker hoặc thay đổi kế hoạch.

---

## 1. Trạng thái nền bắt buộc phải bảo toàn

- Branch dự kiến: `main`.
- Baseline commit đã biết: `f1da8f4`.
- Worktree đang có nhiều thay đổi chưa commit từ các task nền trước. Không được reset, checkout, stash, xóa hoặc ghi đè thay đổi của người dùng.
- Docker chạy với `SeedProfile=LargeDemo`.
- Database kiểm thử có quyền ghi phải có tên khớp `LMS_TEST_*` và phải qua `TestDatabaseSafetyGuard` kiểm tra `DB_NAME()` thực tế.
- Tuyệt đối không chạy test có khả năng mutate trên database gốc `LMS`.
- Role kiểm thử chính: **AcademicStaff**, campus 14.
- Học kỳ thật đang được AcademicStaff context chọn để xếp lịch: `HK1_2027`, ID 15, campus 14, `canPrepareSchedule=true`.
- Bộ dữ liệu D0 tại học kỳ này: 30 khóa học, 30 lớp học phần, 409 đăng ký thực, 5 block, mapping 3 tín chỉ thành 3 buổi/tuần, 15 giảng viên đủ capacity cho 90 ca/tuần.
- Live Generate gần nhất đã chứng minh 30/30 khóa xếp được, 0 unassigned, 0 hard conflict, score 97; không được làm suy giảm baseline này.
- Specialized room hiện `NOT_SUPPORTED`; không tự phát minh loại phòng/chuyên dụng trong ba task này.

### Git preflight bắt buộc

Trước khi sửa:

```powershell
git branch --show-current
git rev-parse HEAD
git status --short
git diff --name-only
git diff --cached --name-only
```

Ghi lại chính xác file dirty có sẵn. Sau mỗi task phải so lại để phân biệt thay đổi của task với thay đổi tồn tại trước đó.

---

# TASK 7D-R0 — CANONICAL CAPACITY & TRUSTWORTHY READINESS

## 1.1. Các lát cắt thực thi R0

Sau khi Discovery được duyệt, thực hiện tuần tự:

- **R0.1 — Canonical capacity:** chuẩn hóa nguồn và fallback sĩ số; chạy pure tests + SQL integration tương ứng.
- **R0.2 — Room-policy consistency:** nối shared capacity/eligibility vào mọi consumer; chạy consistency tests.
- **R0.3 — Structured readiness:** hoàn thiện DTO/service/error codes và các feasibility check.
- **R0.4 — Frontend consumption:** bỏ suy luận readiness cục bộ, map hoàn toàn theo backend codes.
- **R0.5 — Closure:** chạy positive LargeDemo, toàn bộ negative scenarios, build/regression/diff review.

Không chờ người dùng giữa các lát cắt nếu không có blocker và không vượt Change Budget. Nếu một lát cắt fail, sửa và xác minh nó trước khi đi tiếp; không xây lát cắt sau trên kết quả chưa tin cậy.

## 2. Vấn đề cần sửa

Hiện frontend có thể suy luận sai:

- Có giảng viên không đồng nghĩa giảng viên đủ kỹ năng, đủ rảnh và đủ tải.
- Có phòng không đồng nghĩa phòng đủ sức chứa.
- Có ca học không đồng nghĩa tổng số room-slot đủ cho toàn học kỳ.
- Không có dữ liệu sĩ số không được coi như sĩ số bằng 0 rồi cho PASS.

Nếu solver, readiness, conflict check và publish tính sĩ số/phòng theo các cách khác nhau thì có thể xảy ra tình trạng:

- màn hình báo sẵn sàng nhưng Generate thất bại;
- Generate tạo draft nhưng Publish từ chối;
- một phòng được solver chọn nhưng conflict check hoặc publish lại cho là không hợp lệ;
- lớp không có dữ liệu sĩ số bị coi là hợp lệ giả.

R0 phải tạo ra **một chính sách nghiệp vụ dùng chung**, có nguồn dữ liệu rõ ràng và kết quả readiness có mã máy đọc được.

## 3. R0-A — Rà soát trước khi sửa

Đọc và lập bản đồ luồng hiện tại, tối thiểu gồm:

- Entity/model của `DangKyHocPhan`, học sinh/người dùng, lớp hành chính, lớp học phần, khóa học, phòng, ca học, học kỳ, giảng viên, năng lực môn và nguyện vọng.
- `ICourseCapacityService` / `CourseCapacityService` nếu đã tồn tại.
- `SmartTimetableService`.
- `GeneticTimetableSolver`.
- API/DTO Academic Scheduling Context và readiness hiện có.
- Conflict checking.
- Create/update schedule.
- Draft validation và Publish.
- Frontend `ScheduleManagerView.vue` và nơi đang tự suy luận readiness.

Phải trả lời bằng code/schema thực tế:

1. Trạng thái đăng ký nào được tính là đăng ký hợp lệ?
2. Trạng thái học sinh nào được tính là đang hoạt động?
3. Làm sao xác nhận người đăng ký thuộc đúng campus và đúng vai trò học sinh?
4. Lớp học phần liên kết với khóa học/lớp hành chính theo khóa nào?
5. `SoDaDangKy` có phải dữ liệu nguồn đáng tin hay chỉ là số cache/tổng hợp?

Không tự đoán tên trạng thái hoặc enum. Nếu dự án không có enum rõ ràng, đối chiếu model, seeder và dữ liệu DB thực tế rồi ghi lại quyết định.

## 4. R0-B — Chính sách sĩ số chuẩn duy nhất

Tạo hoặc chuẩn hóa một service/policy dùng chung để trả về tối thiểu:

- `requiredCapacity`.
- `source`.
- `isKnown`.
- mã cảnh báo/lỗi nếu không xác định được.
- số lượng bản ghi hợp lệ/không hợp lệ nếu cần cho audit.

Thứ tự nguồn dữ liệu bắt buộc:

1. Đếm **distinct học sinh hợp lệ** từ đăng ký học phần thật của đúng lớp học phần/khóa học, đúng campus, đúng vai trò và trạng thái hợp lệ.
2. Nếu thực sự không có chuỗi đăng ký học phần khả dụng, fallback sang số học sinh hoạt động thuộc lớp hành chính tương ứng.
3. Nếu vẫn không có, fallback sang sĩ số dự kiến được lưu rõ trong dữ liệu lớp/lớp học phần nếu schema thực sự có trường này.
4. Nếu cả ba nguồn đều không xác định được, trả `unknown` với code `STUDENT_CAPACITY_DATA_MISSING`.

Quy tắc bắt buộc:

- Không dùng số 0 làm fallback im lặng.
- Không đếm trùng một học sinh nhiều lần.
- Không đếm tài khoản không phải học sinh.
- Không đếm học sinh/đăng ký không hoạt động theo chính sách đã xác minh.
- Không trộn campus.
- Không tin mù quáng vào `SoDaDangKy` nếu đó chỉ là dữ liệu tổng hợp có thể lệch; phải chứng minh nếu muốn dùng.
- Không tạo quan hệ “học sinh được gán cố định vào phòng”. Phòng được gán cho khóa học/lớp ở từng slot thời khóa biểu.

## 5. R0-C — Chính sách phòng dùng chung xuyên suốt

Một phòng chỉ eligible khi tối thiểu:

- đúng campus;
- đang hoạt động;
- ca học đang hoạt động;
- không trùng lịch;
- sức chứa `>= requiredCapacity`;
- dữ liệu sức chứa khóa học đã biết.

Chính sách này phải được tái sử dụng hoặc cho kết quả đồng nhất trong tất cả consumer liên quan:

1. Feasibility/readiness trước Generate.
2. Candidate room trong solver.
3. Fitness/ưu tiên phòng vừa đủ.
4. Gợi ý slot/phòng.
5. Create/update schedule.
6. Conflict check.
7. Draft validation.
8. Publish validation.

Không sao chép tám biến thể logic khác nhau. Nếu không thể gom hoàn toàn về một service vì kiến trúc hiện tại, phải có shared policy/helper và test chứng minh mọi consumer cho cùng kết quả.

Phòng vừa đủ chỉ là soft preference; phòng thiếu chỗ là hard constraint.

## 6. R0-D — Readiness có cấu trúc từ backend

Backend phải trả readiness theo từng mã ổn định, không yêu cầu frontend phân tích message. Tối thiểu gồm:

- `COURSES_READY`
- `BLOCKS_READY`
- `CREDIT_MAPPING_READY`
- `TEACHER_SKILL_READY`
- `TEACHER_AVAILABILITY_READY`
- `TEACHER_CAPACITY_READY`
- `ACTIVE_ROOMS_READY`
- `ROOM_CAPACITY_READY`
- `ACTIVE_SHIFTS_READY`
- `TOTAL_ROOM_SLOTS_READY`
- `EXISTING_SCHEDULE_LOCK_READY`

Mỗi mục phải có tối thiểu:

```text
code
status: ready | warning | blocked | unknown
message
action
affectedCount
affectedItems (giới hạn hợp lý, không trả payload khổng lồ)
```

Yêu cầu nghiệp vụ:

- `blocked`: chắc chắn không thể Generate/Publish hợp lệ.
- `warning`: vẫn có thể làm nhưng người dùng cần cân nhắc.
- `unknown`: thiếu dữ liệu để kết luận; không được hiển thị như ready.
- `ready`: đã kiểm tra bằng dữ liệu thật.
- Message/action viết bằng tiếng Việt dễ hiểu, nhưng code phải ổn định cho frontend/test.

Readiness phải xét coverage theo từng khóa học, không chỉ đếm tổng:

- Mỗi khóa có ít nhất một GV eligible.
- Tổng teacher capacity đủ cho tổng số ca yêu cầu.
- Mỗi khóa có ít nhất một phòng đủ sức chứa.
- Tổng room-slot đủ cho toàn bộ ca yêu cầu.
- Mapping tín chỉ tồn tại và hợp lệ.

## 7. R0-E — Frontend dùng readiness thật

Trong Simple Mode:

- Xóa mọi suy luận kiểu `hasTeachers => availability ready` hoặc `hasRooms => room capacity ready`.
- Chỉ render theo code/status từ backend.
- Nếu backend chưa trả được mục nào, hiển thị `Chưa xác định` chứ không PASS.
- Mỗi mục blocked/unknown phải có hướng xử lý dễ hiểu.
- Không hiển thị đường dẫn quản trị mà AcademicStaff không có quyền mở. Nếu không có route hợp lệ, chỉ hướng dẫn bằng câu chữ.
- Nút “Xếp lịch ngay” phải disabled khi có `blocked` hoặc `unknown` mang tính bắt buộc.

## 8. R0-F — Test bắt buộc

### 8.1. Test dương LargeDemo

Trên DB test an toàn được dựng từ LargeDemo và AcademicStaff campus 14:

- Context chọn `HK1_2027`, ID 15.
- Readiness trả đúng các mục bắt buộc.
- Không có mục hard requirement bị `blocked/unknown`.
- Live Generate vẫn HTTP 200.
- 30/30 khóa được xếp.
- 0 unassigned.
- 0 hard conflict.
- Không Publish.
- Cleanup chỉ đúng Job/Draft do test tạo.

### 8.2. Test âm biệt lập

Mỗi trường hợp phải được tạo riêng, chỉ thay đổi biến đang test và rollback/cleanup:

1. Tất cả phòng nhỏ hơn sĩ số một khóa → `ROOM_CAPACITY_READY` blocked; khóa đó không được solver gán vào phòng nhỏ.
2. Tổng room-slot thiếu dù từng khóa riêng lẻ có phòng vừa → `TOTAL_ROOM_SLOTS_READY` blocked.
3. Một khóa không có GV đủ skill → `TEACHER_SKILL_READY` blocked.
4. GV có skill nhưng mọi slot đều unavailable → `TEACHER_AVAILABILITY_READY` blocked.
5. Mỗi khóa có candidate nhưng tổng teacher capacity không đủ → `TEACHER_CAPACITY_READY` blocked.
6. Thiếu mapping tín chỉ → `CREDIT_MAPPING_READY` blocked.
7. Không có enrollment, không có active class students và không có expected size → `STUDENT_CAPACITY_DATA_MISSING`, trạng thái unknown/blocked theo contract; tuyệt đối không PASS bằng capacity 0.
8. Phòng hoặc ca inactive → không được solver/create/update/publish chấp nhận.

### 8.3. Test consistency

Với cùng một khóa học và phòng:

- Solver eligibility.
- Conflict checker.
- Draft validator.
- Publish validator.

phải cùng kết luận về capacity/active/campus. Bổ sung automated test để ngăn regression.

### 8.4. Lệnh xác minh R0

- Backend build: 0 errors.
- Chạy toàn bộ targeted tests Smart Timetable hiện có, không chỉ hai class gần nhất.
- Chạy các test mới của R0.
- Frontend build: PASS.
- Frontend unit tests hiện có: PASS.
- Oxlint trên đúng các file frontend bị sửa: 0 lỗi.
- Không chạy full legacy suite nếu nó nằm ngoài phạm vi và có nguy cơ chạm DB; phải ghi rõ chưa chạy.

## 9. Điều kiện PASS của R0

Chỉ báo `TASK 7D-R0: PASS` khi đồng thời:

- Có một chính sách sĩ số rõ nguồn và không zero-fallback im lặng.
- Các consumer phòng cho kết quả đồng nhất.
- Backend trả structured readiness thật.
- Frontend không còn suy luận readiness giả.
- LargeDemo positive Generate giữ 30/30, 0 hard conflict.
- Tất cả test âm bắt đúng nguyên nhân riêng.
- Build/test trong phạm vi đều PASS.

Nếu thiếu một điều kiện, báo `BLOCKED` hoặc `PARTIAL`, không tự hạ tiêu chuẩn.

Sau khi R0 xong: dừng, xuất báo cáo R0 để người dùng duyệt. Chỉ tiếp tục R1 khi R0 đã PASS hoặc người dùng yêu cầu rõ ràng.

---

# TASK 7D-R1 — NON-TECH UX BUSINESS-CORRECTNESS REPAIR

## 10. Mục tiêu R1

R1 không phải redesign thẩm mỹ. Giữ phong cách và bố cục chính đang có, nhưng sửa mọi hành vi khiến người dùng non-tech:

- tưởng đang xếp toàn học kỳ nhưng request chỉ chứa một lớp;
- thấy “sẵn sàng” dù backend chưa xác nhận;
- Publish qua đường không có đầy đủ transaction/lock/attendance protection;
- thấy thay đổi phòng/GV trên màn hình nhưng DB chưa đổi;
- nhận thông báo khóa sai nguyên nhân;
- nhìn thấy “không xung đột” dù hệ thống chỉ kiểm tra một phần dữ liệu;
- bị treo vô hạn khi tiến trình lỗi.

### Mô hình trạng thái UX bắt buộc

Không nhất thiết cài thư viện state machine, nhưng code và test phải phân biệt rõ:

```text
loading-context
checking-readiness
blocked
ready
submitting
generating
completed
recoverable-error
expired-or-invalid
```

Với mỗi trạng thái, xác định:

- thông tin hiển thị;
- action được phép;
- action bị khóa và lý do;
- polling có chạy hay không;
- retry/recovery đi tới trạng thái nào;
- reload trang khôi phục bằng dữ liệu backend nào.

Không để nhiều boolean mâu thuẫn khiến UI vừa loading vừa cho Generate/Publish.

## 11. R1-A — Xác định phạm vi Generate chính xác

Kiểm tra luồng giữa `ClassNavigator`, `ScheduleManagerView` và payload Generate.

Yêu cầu:

- Simple Mode mặc định là **toàn học kỳ được context cho phép**, không phụ thuộc lớp đầu tiên mà navigator tự chọn.
- Nếu người dùng chủ động chọn “một lớp”, UI phải ghi rõ đang xếp một lớp và payload chỉ chứa lớp đó.
- Nếu chọn toàn học kỳ, payload phải chứa đúng toàn bộ 30 khóa của HK1_2027 hoặc sử dụng contract backend biểu diễn rõ toàn học kỳ.
- Không được lấy `courseOptions` đã bị filter theo lớp làm nguồn cho request toàn học kỳ.
- Trước khi gửi, hiển thị câu xác nhận ngắn: học kỳ, campus theo context và số khóa sẽ xếp.
- Chống double click/request trùng bằng loading/disabled và idempotency hiện có nếu backend hỗ trợ.

## 12. R1-B — Một đường Publish chính thức

Rà soát mọi nút/hàm publish liên quan, đặc biệt `publishDraft` và `publishAll()`.

Yêu cầu:

- Smart Timetable draft chỉ Publish qua endpoint chính thức có transaction, validation, rollback-safe swap, 30-minute lock, attendance protection, audit/notification theo kiến trúc hiện hành.
- Không được Publish một draft bằng `Promise.all(scheduleApi.update)` hoặc cập nhật từng dòng rời rạc.
- Nếu màn hình còn cần bulk update cho nghiệp vụ khác, nó phải tách rõ và không xuất hiện như đường Publish Smart Timetable.
- Khi có hard issue, nút Publish disabled ở UI; backend vẫn phải từ chối nếu client bị bypass.
- Hộp xác nhận phải nêu: học kỳ, số khóa/lớp, cảnh báo còn lại, quy tắc chỉnh trong 30 phút và khóa vĩnh viễn sau khi có điểm danh.
- Task này không thực hiện Publish thật trên LargeDemo trừ khi người dùng phê duyệt riêng.

## 13. R1-C — Mã lỗi nghiệp vụ ổn định

Không phân loại bằng `message.includes(...)` hoặc phụ thuộc dấu tiếng Việt.

Frontend phải map theo mã lỗi/structured response ổn định. Tối thiểu phân biệt:

- khóa do quá 30 phút;
- khóa do đã có điểm danh;
- forbidden/cross-campus;
- draft không còn hợp lệ hoặc đã publish;
- hard conflict;
- readiness chưa đạt;
- lỗi mạng/tạm thời.

Nếu backend hiện chỉ trả một message chung cho khóa 30 phút và điểm danh, hãy sửa contract tối thiểu để trả code/reason riêng nhưng giữ tương thích message hiện có nếu consumer khác cần. Không thay đổi thuật toán GA.

## 14. R1-D — Không cập nhật giả trên frontend

Rà soát thao tác “áp dụng phòng gợi ý”, đổi GV và các chỉnh sửa draft.

Yêu cầu:

- Không chỉ sửa label/tên hiển thị trong object frontend.
- Khi cho phép áp dụng, phải gửi đúng ID thực (`maPhong`, `maGiaoVien`, slot liên quan) tới API có validation rồi reload dữ liệu từ server.
- Nếu chưa có API an toàn, chỉ hiển thị dưới dạng “Gợi ý” và không có nút/nhãn gây hiểu là đã lưu.
- Sau save thất bại phải hoàn nguyên hoặc reload server state; không giữ optimistic state giả.
- Không cho chọn GV/phòng inactive, sai campus, thiếu capacity, unavailable, thiếu skill hoặc gây hard conflict.
- Radio/select của option không hợp lệ phải disabled và hiện lý do dễ hiểu.
- Backend phải kiểm tra lại; không tin frontend.

## 15. R1-E — Conflict Check đúng phạm vi

Không được tải 100 bản ghi đầu rồi kết luận “toàn hệ thống không xung đột”.

Yêu cầu:

- Kiểm tra đúng campus từ authenticated context.
- Kiểm tra đúng học kỳ/draft đang xem.
- Kiểm tra toàn bộ item thuộc phạm vi, bằng endpoint backend ưu tiên hoặc phân trang đầy đủ nếu contract buộc phải làm vậy.
- Trong lúc chưa tải/kiểm tra xong, trạng thái là “Đang kiểm tra” hoặc “Chưa kiểm tra”, không phải “Không có xung đột”.
- Hiển thị rõ hard conflict và soft warning.
- Chỉ hard conflict chặn Publish.
- Kết quả phải ghi số lượng item đã kiểm tra và phạm vi kiểm tra.

## 16. R1-F — Draft dễ hiểu và có phân nhóm thật

Mặc định người non-tech chỉ cần thấy:

- Học kỳ đang xếp.
- Đã xếp bao nhiêu trên tổng số khóa.
- Bao nhiêu khóa chưa xếp.
- Có lỗi chặn xuất bản hay không.
- Có cảnh báo nên xem lại hay không.
- Lịch tuần.
- Lý do ngắn gọn vì sao chọn GV/phòng nếu backend có dữ liệu thật.

Yêu cầu:

- “Theo lớp”, “Theo giảng viên”, “Theo phòng” phải group dữ liệu thật, có heading và số lượng; không chỉ đổi thứ tự sort.
- Không dùng ID làm nhãn chính nếu có tên/mã nghiệp vụ.
- Draft ID, fitness, generation, population, crossover và log kỹ thuật đặt trong “Chi tiết kỹ thuật”, đóng mặc định.
- Không hiển thị badge “Đã kiểm tra” chung chung nếu chưa có kết quả backend tương ứng.
- Không hard-code ngày/timeline.
- Empty/loading/error state phải khác nhau rõ ràng.
- Không bịa giải thích AI; Task 7E chưa được phép làm.

## 17. R1-G — Progress có timeout và phục hồi

- Polling có interval hợp lý, timeout tổng và dừng khi component unmount.
- Không tạo nhiều polling loop song song.
- Xử lý 401/403/404/409/5xx và lỗi mạng riêng phù hợp.
- Khi timeout/lỗi mạng, cho phép “Thử lại kiểm tra tiến trình”.
- Nếu backend đã tạo draft, phải có khả năng tải lại draft thay vì yêu cầu Generate lại mù quáng.
- Double click/reload không được tạo job trùng ngoài ý muốn.
- Nội dung mặc định là ngôn ngữ nghiệp vụ; technical details đóng mặc định.

### Khả năng phục hồi sau reload

Phải kiểm tra luồng:

1. Bắt đầu Generate.
2. Reload khi job đang chạy.
3. Tải lại job hiện tại từ backend theo authenticated context/term.
4. Tiếp tục theo dõi đúng job.
5. Mở đúng draft khi hoàn tất.
6. Không tạo job thứ hai.

Nếu backend hiện không có khả năng truy vấn job đang chạy phù hợp, không tự tạo endpoint lớn ngoài Change Budget. Báo blocker/đề xuất tối thiểu để duyệt; tuyệt đối không tự Generate lại sau reload.

## 17.1. Khả năng sử dụng cơ bản cho người non-tech

Đây là acceptance criteria UX, không phải task thiết kế giao diện mới:

- Loading phải có câu chữ, không chỉ spinner.
- Trạng thái/lỗi không phân biệt chỉ bằng màu sắc.
- Mỗi nút disabled quan trọng phải có lý do gần nút hoặc trong readiness.
- Lỗi phải chỉ ra dữ liệu nào cần xử lý.
- Modal xác nhận có tiêu đề/hành động rõ và không làm mất focus bất thường.
- Luồng chính có thể thao tác bằng bàn phím ở mức cơ bản.
- Dùng nhãn chữ nhất quán: `Sẵn sàng`, `Cần xem lại`, `Chưa thể xếp`, `Chưa xác định`.
- Không dùng thuật ngữ GA/fitness/population trong luồng mặc định.

## 18. R1-H — Các màn hình phụ liên quan

Chỉ sửa khi liên quan trực tiếp đến luồng Smart Timetable:

- `TeacherAssignmentView`: option không eligible phải disabled và có lý do.
- `RoomManagementView`: bỏ hard-code campus mặc định nếu nó ảnh hưởng luồng; lấy campus từ context.
- `ShiftManagementView`: tránh hai toggle/action trùng nhau và không nuốt lỗi im lặng.
- `StaffPublishedSchedulesView`: không ưu tiên hiển thị Draft ID/fitness vô nghĩa cho người dùng thường; không hiển thị tab thay đổi rỗng như có dữ liệu.
- Các màn hình Block/Credit Mapping/Shift chỉ cần validation và mô tả ảnh hưởng đến readiness; không redesign toàn module.

Không mở rộng thành cuộc đại tu giao diện toàn hệ thống.

## 19. Điều kiện PASS của R1

Chỉ báo `TASK 7D-R1: PASS` khi:

- Generate toàn học kỳ chứng minh gửi đủ 30 khóa, không phụ thuộc lớp auto-selected.
- Readiness frontend chỉ dùng dữ liệu thật của R0.
- Smart draft chỉ còn một đường Publish chính thức.
- Attendance lock và 30-minute lock được phân biệt bằng code/reason.
- Không còn thao tác phòng/GV “lưu giả” ở client.
- Không thể chọn option không eligible.
- Conflict Check bao phủ đúng toàn bộ draft/term/campus.
- Draft grouping là grouping thật và non-tech đọc được.
- Polling có timeout, cleanup và recovery.
- Build/lint/test trong phạm vi PASS.

Sau khi R1 xong: dừng, xuất báo cáo R1 để người dùng duyệt. Chỉ tiếp tục 7D-C khi R1 đã PASS hoặc người dùng yêu cầu rõ ràng.

---

# TASK 7D-C — COMPONENT TEST & LIVE UX VERIFICATION

## 20. Mục tiêu 7D-C

7D-C không thêm tính năng lớn và không che lỗi bằng cách sửa test assertion. Nó phải chứng minh các hành vi của R0 và R1 hoạt động thật bằng:

1. Component/unit tests có ý nghĩa.
2. API/network payload evidence.
3. Live browser trên Docker LargeDemo bằng AcademicStaff campus 14.
4. Kiểm tra tác dụng phụ và cleanup.

## 21. C-A — Component tests bắt buộc

Bổ sung test theo component/composable/service phù hợp, tối thiểu bao phủ:

1. Simple Mode là mặc định.
2. Campus lấy từ authenticated context; người dùng thường không thấy campus override.
3. Draft ID và tham số GA không nằm trong luồng đơn giản.
4. Toàn học kỳ gửi đúng 30 khóa hoặc contract toàn-term tương đương đã được backend xác minh.
5. Chủ động chọn một lớp chỉ gửi khóa của lớp đó và UI ghi rõ phạm vi.
6. Double click chỉ tạo một Generate request.
7. Có readiness `blocked` bắt buộc thì nút Generate disabled.
8. Readiness `unknown` không được render thành ready.
9. Hard conflict chặn Publish.
10. Soft warning không tự động chặn Publish nhưng phải xuất hiện trong confirm.
11. Smart draft gọi đúng publish endpoint; không gọi per-row `update`.
12. Attendance lock và 30-minute lock hiện hai hướng dẫn phù hợp khác nhau theo error code.
13. 403/cross-campus có thông báo an toàn, không đề nghị đổi campus thủ công.
14. Chuyển Theo lớp/GV/Phòng tạo group thật.
15. Option GV/phòng không eligible bị disabled.
16. Áp dụng gợi ý thành công reload state từ backend; thất bại không giữ state giả.
17. Polling dừng khi success/failure/unmount/timeout.
18. Sau timeout có thể retry progress mà không tạo Generate job mới.
19. Conflict Check hiển thị “chưa/đang kiểm tra” trước khi hoàn tất.
20. Kết quả conflict ghi đúng campus, term/draft và số item đã kiểm tra.

Không chỉ snapshot HTML. Phải assert event, payload, API call count, state transition và điều kiện disabled/enabled.

## 22. C-B — Live browser UX bằng AcademicStaff

Khởi động hệ thống local bằng Docker LargeDemo. Dùng tài khoản AcademicStaff campus 14; không dùng SuperAdmin.

Luồng dương bắt buộc:

1. Đăng nhập AcademicStaff.
2. Mở Smart Timetable theo đường người dùng thật.
3. Xác minh context tự chọn `HK1_2027`, campus 14 và không cho override campus trong Simple Mode.
4. Xác minh readiness lấy từ API, hiển thị bằng ngôn ngữ dễ hiểu.
5. Chọn toàn học kỳ.
6. Trước khi Generate, xác minh màn hình ghi 30 khóa.
7. Bấm “Xếp lịch ngay” một lần; kiểm tra network chỉ có một request Generate.
8. Theo dõi progress đến hoàn tất.
9. Mở draft và xác minh 30/30 khóa, 0 unassigned, 0 hard conflict.
10. Kiểm tra ba chế độ Theo lớp/Theo giảng viên/Theo phòng thực sự thay đổi nhóm.
11. Kiểm tra phần kỹ thuật đóng mặc định.
12. Mở xác nhận Publish để kiểm tra nội dung nhưng **không xác nhận Publish**.
13. Đóng hộp thoại; đảm bảo không có TKB/BuoiHoc published mới.
14. Chạy kịch bản phục hồi: Generate một draft test khác hoặc job fixture phù hợp, reload khi đang xử lý, xác minh UI tìm lại đúng job và không tạo request Generate thứ hai.

Thu thập evidence cần thiết:

- Screenshot các mốc chính.
- Request payload Generate đã redacted token/credential.
- Response readiness/progress/draft chỉ giữ field cần chứng minh, không lộ secret.
- Số Job/Draft trước và sau.
- Network trace tối thiểu chứng minh call count/payload/status; phải xóa hoặc che token, cookie và credential trước khi lưu evidence.

## 23. C-C — Live negative UX verification

Dùng DB test `LMS_TEST_*`, transaction hoặc fixture có cleanup. Không phá LargeDemo chính.

Chạy riêng từng tình huống:

- phòng không đủ sức chứa;
- tổng room-slot thiếu;
- thiếu skill GV;
- GV unavailable;
- tổng teacher capacity thiếu;
- thiếu credit mapping;
- không xác định được sĩ số;
- hard conflict;
- 30-minute lock;
- attendance lock;
- 403 cross-campus bằng AcademicStaff campus 14 truy cập campus khác.

Mỗi tình huống phải xác minh:

1. Backend trả đúng code/status.
2. Frontend hiển thị đúng nguyên nhân bằng tiếng Việt dễ hiểu.
3. Có hành động khắc phục phù hợp và role hiện tại có thể hiểu/thực hiện.
4. Generate hoặc Publish bị chặn đúng tầng.
5. Không có mutation trái phép.

Không được làm sai học kỳ để vô tình nhận một lỗi 409 khác rồi gọi là test PASS.

## 24. C-D — Kiểm tra cleanup và toàn vẹn

Sau test:

- Không có TKB/BuoiHoc published mới từ 7D-C.
- Xóa đúng Job/Draft do 7D-C tạo, không xóa baseline cũ.
- Không còn dữ liệu fixture âm.
- Không có orphan `BuoiHoc`/`DiemDanh`.
- Không có duplicate Job/Draft/notification do double click hoặc retry.
- Dữ liệu lịch sử và 20 anomaly lịch sử không bị sửa.
- `bghExport.js` không bị đụng bởi task.
- Không có screenshot, TRX, log, token, `.env` hoặc artifact test được đưa vào danh sách file nguồn đề xuất commit.

## 25. C-E — Lệnh kiểm chứng cuối

- Backend build: PASS, 0 errors.
- Frontend build: PASS.
- Toàn bộ targeted Smart Timetable backend tests hiện có + test R0: PASS, 0 skip trong phạm vi cần DB vì DB test đã được cấu hình.
- Frontend tests cũ và component tests mới: PASS.
- Oxlint trên toàn bộ file frontend bị thay đổi bởi R0/R1/C: PASS.
- ESLint toàn repo có thể vẫn có nợ cũ; báo số liệu nhưng không sửa lan ngoài phạm vi.
- Không chạy test mutation nào nếu guard không xác nhận DB thực là `LMS_TEST_*`.

## 26. Điều kiện PASS của 7D-C

Chỉ báo `TASK 7D-C: PASS` khi:

- Component tests kiểm tra hành vi/payload, không chỉ render.
- Live AcademicStaff flow hoàn tất Generate 30/30 và xem draft dễ hiểu.
- Không dùng SuperAdmin.
- Không Publish.
- Toàn bộ negative scenario trả đúng lỗi cần test.
- Cleanup và integrity đều sạch.
- Build/tests/lint trong phạm vi PASS.

Nếu không chạy được live browser, không được thay thế bằng lời mô tả hoặc component test rồi báo PASS. Hãy báo `BLOCKED: LIVE_UX_NOT_VERIFIED`.

---

# 27. Quy tắc xử lý lỗi trong cả ba task

Khi phát hiện lỗi:

1. Tái hiện và ghi bằng chứng.
2. Xác định lỗi thuộc R0, R1 hay C.
3. Xác định root cause, không sửa triệu chứng bằng hard-code.
4. Viết test fail trước hoặc test tái hiện phù hợp khi khả thi.
5. Sửa phạm vi tối thiểu.
6. Chạy lại test mục tiêu.
7. Chạy regression trong phạm vi Smart Timetable.
8. Kiểm tra diff để loại thay đổi ngoài phạm vi.

Không được:

- đổi expected result để hợp với code sai;
- catch rồi nuốt lỗi để test xanh;
- hard-code ID 15, campus 14 hoặc số 30 vào business logic production;
- dùng dữ liệu seed giả làm logic production;
- bỏ validation backend vì frontend đã disable;
- dùng message text làm API contract;
- xóa dữ liệu cũ hàng loạt để cleanup;
- tuyên bố PASS chỉ vì build thành công.

---

# 28. Định dạng báo cáo bắt buộc sau mỗi task

```text
TASK: 7D-R0 | 7D-R1 | 7D-C
VERDICT: PASS | PARTIAL | BLOCKED | FAIL

STARTING BRANCH / HEAD:
WORKTREE PREFLIGHT:
FILES CHANGED BY THIS TASK:
PRE-EXISTING DIRTY FILES PRESERVED:

ROOT CAUSES CONFIRMED:
- ...

FIXES IMPLEMENTED:
- ...

BUSINESS RULE EVIDENCE:
- ...

TESTS:
- Test suite/name:
- Passed / Failed / Skipped:
- DB actually used:
- Safety guard result:

LIVE ACADEMICSTAFF EVIDENCE:
- Role/campus:
- Term:
- Generate payload scope:
- Assigned/unassigned/hard conflicts:
- Publish performed: NO

DATA CLEANUP / INTEGRITY:
- ...

BACKEND BUILD:
FRONTEND BUILD:
FRONTEND TESTS:
OXLINT CHANGED FILES:

SUPERADMIN_OUT_OF_SCOPE:
SECRETS/R2/PAYOS TOUCHED: NO
BGH_EXPORT TOUCHED BY TASK: NO
STAGED / COMMIT / PUSH: NO / NO / NO

REMAINING RISKS WITHIN THIS TASK:
- ...

FINAL PASS-GATE JUSTIFICATION:
- Dẫn từng điều kiện PASS và bằng chứng tương ứng.
```

Không gộp ba task thành một câu “overall PASS” khi một task chưa đủ evidence. Mỗi task phải có verdict độc lập.

---

# 29. Kết quả cuối cùng mong đợi

Sau khi hoàn thành đúng ba task:

- Backend và frontend dùng cùng một sự thật về sĩ số, phòng và readiness.
- Người dùng AcademicStaff có thể bấm xếp toàn học kỳ mà không vô tình chỉ xếp một lớp.
- Người dùng không bị đánh lừa bởi readiness/conflict/save giả.
- Smart Timetable chỉ Publish qua đường an toàn chính thức.
- Draft dễ đọc theo lớp/GV/phòng và không lộ kỹ thuật mặc định.
- Component test và live test chứng minh hành vi thật.
- Không dùng SuperAdmin, không làm AI, không Publish, không commit/push và không mở rộng sang task khác.

---

# 30. PROMPT KÍCH HOẠT NGẮN DÀNH CHO ANTIGRAVITY

Không dán lại toàn bộ Master Contract nếu Antigravity đang ở cùng workspace và đã đọc file này. Dùng đúng từng prompt dưới đây.

## Prompt 1 — Nạp Master Contract và chạy Discovery R0

```text
Đọc TOÀN BỘ file TASK_7D_R0_R1_C_EXECUTION_PLAN.md và coi đó là Master Contract bắt buộc.

Chỉ thực hiện DISCOVERY GATE cho Task 7D-R0. Chưa sửa bất kỳ file nào, chưa chạy test có mutation, chưa stage/commit/push và không chuyển sang implementation.

Hãy dùng khả năng tìm kiếm toàn repository để lập:
1. Bản đồ code thực tế.
2. Source-of-truth matrix.
3. Impact graph đầy đủ consumer.
4. Root cause có file/method cụ thể.
5. Kế hoạch diff + test failure-first.
6. Điểm nào trong Master Contract không khớp schema/code/dữ liệu thật.

Giữ nguyên mọi thay đổi dirty có sẵn, loại bghExport.js khỏi phạm vi, không dùng SuperAdmin và không đụng AI/R2/PayOS/secrets.

Kết thúc bằng verdict DISCOVERY_READY hoặc BLOCKED rồi DỪNG để tôi duyệt.
```

## Prompt 2 — Triển khai R0 sau khi Discovery được duyệt

Chỉ gửi prompt này sau khi người dùng đã xem và duyệt Discovery:

```text
Discovery Task 7D-R0 đã được duyệt. Tiếp tục thực hiện CHỈ Task 7D-R0 theo Master Contract TASK_7D_R0_R1_C_EXECUTION_PLAN.md và đúng kế hoạch diff đã duyệt.

Thực hiện tuần tự R0.1 → R0.5. Sau mỗi lát cắt chạy targeted test gần nhất; chỉ chạy regression/build đầy đủ trong phạm vi ở closure. Test mutate DB phải qua TestDatabaseSafetyGuard và chỉ dùng DB_NAME() khớp LMS_TEST_*.

Bắt buộc chứng minh:
- capacity source không zero-fallback im lặng;
- mọi consumer dùng cùng policy/kết luận;
- structured readiness backend;
- frontend bỏ suy luận readiness giả;
- positive LargeDemo giữ 30/30, 0 unassigned, 0 hard conflict;
- từng negative scenario trả đúng stable code và không mutation trái phép.

Không Publish, không dùng SuperAdmin, không sửa dữ liệu lịch sử, không làm R1/C/7E, không stage/commit/push. Nếu vượt Change Budget hoặc cần schema/migration mới, dừng xin duyệt.

Xuất báo cáo đúng mẫu của Master Contract và DỪNG.
```

## Prompt 3 — Triển khai R1 sau khi R0 PASS

```text
Task 7D-R0 đã được người dùng duyệt PASS. Đọc lại các phần 0, 10–19, 27–30 trong TASK_7D_R0_R1_C_EXECUTION_PLAN.md và thực hiện CHỈ Task 7D-R1.

Trước khi sửa, tái hiện và ghi failure-first evidence cho các lỗi R1; sau đó thực hiện trong Change Budget. Tập trung vào tính đúng nghiệp vụ, không redesign thẩm mỹ.

Bắt buộc chứng minh:
- toàn học kỳ gửi đúng phạm vi 30 khóa, không bị lớp auto-selected thu hẹp;
- chỉ một đường Publish Smart Timetable an toàn;
- error code phân biệt attendance/30 phút/403/hard conflict;
- không có save giả phòng/GV ở client;
- option không eligible bị khóa và backend vẫn validate;
- conflict check đúng toàn term/draft/campus;
- grouping thật theo lớp/GV/phòng;
- UX state rõ ràng, polling timeout/cleanup/retry và reload không tạo job trùng;
- readiness chỉ dùng kết quả R0.

Chạy targeted tests/build/Oxlint trong phạm vi. Không Publish, không dùng SuperAdmin, không làm 7D-C/7E, không stage/commit/push. Xuất báo cáo R1 và DỪNG.
```

## Prompt 4 — Thực hiện 7D-C sau khi R1 PASS

```text
Task 7D-R1 đã được người dùng duyệt PASS. Đọc lại các phần 0, 20–30 trong TASK_7D_R0_R1_C_EXECUTION_PLAN.md và thực hiện CHỈ Task 7D-C.

Không thêm tính năng lớn. Bổ sung component tests kiểm tra event/payload/call count/state transition; chạy SQL integration trên LMS_TEST_*; sau đó kiểm tra live browser trên Docker LargeDemo bằng AcademicStaff campus 14.

Live flow phải chứng minh context HK1_2027, Generate toàn học kỳ đúng 30 khóa, chỉ một request, progress/reload recovery, draft 30/30, 0 unassigned, 0 hard conflict, grouping thật và publish confirmation đúng. Chỉ mở hộp xác nhận, KHÔNG Publish.

Chạy riêng các negative UX scenarios và assert đúng stable error code; không dùng một lỗi 409 khác để giả PASS. Thu thập network evidence đã che token/cookie/credential. Cleanup đúng Job/Draft/fixture do task tạo và xác minh DB integrity.

Không dùng SuperAdmin, không làm 7E, không sửa nợ toàn dự án, không stage/commit/push. Nếu live browser không chạy được, verdict phải là BLOCKED: LIVE_UX_NOT_VERIFIED.

Xuất báo cáo 7D-C theo Master Contract và DỪNG.
```

## Prompt xử lý khi agent báo cáo

Sau mỗi báo cáo, người dùng nên yêu cầu agent không làm tiếp trong lúc báo cáo đang được duyệt:

```text
Dừng tại đây. Không sửa thêm, không tự chuyển task, không stage/commit/push. Giữ nguyên workspace để tôi rà báo cáo và diff.
```

## Cách tối ưu tốc độ mà vẫn an toàn

- Chỉ gửi Prompt 1 một lần để agent hiểu toàn repository trước khi sửa.
- Sau khi duyệt, gửi Prompt 2/3/4 thay vì dán lại toàn bộ tài liệu.
- Cho agent chạy song song các kiểm tra read-only độc lập; test ghi DB vẫn tuần tự.
- Targeted test sau từng lát cắt; build/regression rộng ở cuối task.
- Không yêu cầu agent chụp mọi màn hình; chỉ các mốc acceptance bắt buộc.
- Không yêu cầu báo cáo log từng lệnh; chỉ cần command, exit code, test count và evidence liên quan.
- Khi agent phát hiện blocker thật, xử lý blocker trước; không bắt tiếp tục để đạt số lượng checklist.
