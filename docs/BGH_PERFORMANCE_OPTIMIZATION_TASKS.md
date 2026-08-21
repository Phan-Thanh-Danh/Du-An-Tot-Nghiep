# Kế hoạch tối ưu hiệu năng toàn bộ giao diện BGH

## 1. Mục tiêu

Tài liệu này là checklist triển khai và theo dõi tối ưu hiệu năng cho **toàn bộ khu vực Ban Giám Hiệu (BGH)**, không chỉ riêng Dashboard.

Mục tiêu bắt buộc:

- Nội dung chính của mỗi trang BGH phải xuất hiện trong tối đa **3 giây** ở điều kiện kiểm thử đã thống nhất.
- Dữ liệu nghiệp vụ phải đến từ API và SQL Server thật; không thêm mock hoặc fallback dữ liệu giả.
- Không thay đổi thiết kế, bố cục, màu sắc hoặc hiệu ứng hiện tại chỉ để đạt mục tiêu hiệu năng.
- Không tải toàn bộ tập dữ liệu lớn lên trình duyệt; dữ liệu vẫn đầy đủ thông qua phân trang, cursor, tìm kiếm và trang chi tiết.
- Giữ nguyên role scope và campus scope cho mọi request, cache và prefetch.
- Không thêm Java backend chỉ để tăng tốc. Trong tài liệu này, xử lý song song phía trình duyệt là **JavaScript/Web Worker**.

## 2. Quy ước trạng thái

- `[ ]`: Chưa hoàn thành hoặc chưa có đủ bằng chứng.
- `[x]`: Đã hoàn thành và có bằng chứng build/test/smoke/measurement.
- Không đánh dấu `[x]` chỉ vì đã viết code.
- Khi tick một task, phải bổ sung bằng chứng tại mục **Nhật ký hoàn thành**.
- Nếu task không còn cần thiết, giữ `[ ]` và ghi rõ `Không áp dụng` cùng lý do; không xóa task.

## 3. Phạm vi

### 3.1. Khu vực áp dụng

- Dashboard chiến lược.
- Cơ cấu tổ chức, người dùng và vai trò.
- Học kỳ, chương trình đào tạo và chương trình học.
- Tổng quan học vụ, GPA, Pass/Fail, báo cáo và sinh viên rủi ro.
- Đánh giá giáo viên, xếp hạng và phân tích AI.
- TKB chờ duyệt, TKB đã công bố, xung đột và thay đổi lịch.
- Cơ sở vật chất, nhật ký kiểm toán và hồ sơ BGH.

### 3.2. File được ưu tiên

Theo `docs/BGH_ROLE_EDIT_SCOPE.md`, ưu tiên thay đổi trong:

```text
frontend/src/views/BGH/**
frontend/src/components/BGH/**
frontend/src/services/bghApi.js
Backend/Controllers/BghAcademicController.cs
Backend/Controllers/BghDashboardController.cs
Backend/Controllers/BghEvaluationController.cs
Backend/Controllers/BghFacadeController.cs
Backend/DTOs/Bgh/**
Backend/Services/Bgh/**
Backend.ApiTests/Bgh*Tests.cs
docs/API_CONTRACT.md
```

Các file dùng chung như `frontend/src/router/index.js`, `frontend/src/services/apiClient.js`, `Backend/Program.cs`, `ApplicationDbContext` hoặc migrations chỉ được sửa khi thật sự cần thiết, phải đánh giá ảnh hưởng role khác và tuân theo phạm vi có điều kiện.

## 4. Ngân sách hiệu năng

| Chỉ số | Cold load | Warm/cache load |
|---|---:|---:|
| App shell BGH hiển thị | <= 800 ms | <= 300 ms |
| Nội dung quan trọng đầu tiên | <= 1.500 ms | <= 500 ms |
| Trang có thể tương tác | <= 2.000 ms | <= 800 ms |
| Toàn bộ nội dung ưu tiên hoàn tất | <= 3.000 ms | <= 1.500 ms |
| API summary P95 | <= 500 ms | <= 150 ms |
| API danh sách trang đầu P95 | <= 800 ms | <= 300 ms |
| API báo cáo/biểu đồ P95 | <= 1.200 ms | <= 500 ms |
| Prefetch nền đồng thời | Tối đa 2 request | Tối đa 2 request |
| Request quan trọng đồng thời | Tối đa 4–6 request | Tối đa 4–6 request |

Điều kiện đo bắt buộc phải ghi lại: trình duyệt, máy, database, số lượng dữ liệu, cold/warm cache, network profile và route.

## 5. Checklist triển khai

### P0 — Governance, inventory và baseline

- [x] **BGH-P0-01** — Đã đọc `README.md`, `AGENTS.md`, `CLAUDE.md` và `docs/BGH_ROLE_EDIT_SCOPE.md`.
- [x] **BGH-P0-02** — Đã kiểm tra source thật và xác nhận các BGH route hiện dùng dynamic `import()` để route-level lazy loading.
- [x] **BGH-P0-03** — Đã có baseline API Dashboard/Pass-Fail trên SQL Server thật.
- [x] **BGH-P0-04** — Lập inventory đầy đủ route BGH → component → API → payload → số request → thời gian P50/P95.
- [x] **BGH-P0-05** — Đo cold load và warm load từng route BGH bằng browser, không chỉ gọi API trực tiếp.
- [x] **BGH-P0-06** — Ghi kích thước JavaScript chunk, JSON payload, số DOM node và thời gian render của từng route.
- [x] **BGH-P0-07** — Xác định 5 route chậm nhất và nguyên nhân chính: network, query, JSON, JavaScript hay render.
- [x] **BGH-P0-08** — Chốt điều kiện kiểm thử chính thức cho tiêu chí tối đa 3 giây.

### P1 — API summary, pagination và giới hạn payload

- [x] **BGH-P1-01** — Thiết kế contract Dashboard summary chỉ trả đúng dữ liệu đang hiển thị.
- [x] **BGH-P1-02** — Dashboard chỉ nhận số lượng giới hạn: TKB, cảnh báo, xếp hạng và audit log gần nhất.
- [x] **BGH-P1-03** — Thêm `page/pageSize` hoặc cursor cho danh sách sinh viên rủi ro.
- [x] **BGH-P1-04** — Thêm phân trang cho người dùng, audit log, đánh giá giáo viên, lịch học và danh sách báo cáo lớn.
- [x] **BGH-P1-05** — Đặt giới hạn `pageSize` tối đa ở Backend để tránh request quá lớn.
- [x] **BGH-P1-06** — Tách API summary và API detail; không dùng endpoint danh sách đầy đủ cho card Dashboard.
- [x] **BGH-P1-07** — DTO chỉ chứa trường FE thật sự sử dụng; không serialize entity hoặc navigation không cần thiết.
- [x] **BGH-P1-08** — API tìm kiếm/lọc thực hiện tại Backend thay vì tải hết về lọc ở FE.
- [x] **BGH-P1-09** — Giữ campus scope trong tất cả query pagination/cursor.
- [x] **BGH-P1-10** — Cập nhật phần `/api/bgh/*` trong `docs/API_CONTRACT.md`.

### P2 — Tối ưu SQL Server và truy vấn EF Core

- [x] **BGH-P2-01** — Kiểm tra mọi query BGH read-only sử dụng `AsNoTracking()` khi phù hợp.
- [x] **BGH-P2-02** — Loại bỏ N+1 query và correlated subquery tốn kém ở các báo cáo BGH.
- [x] **BGH-P2-03** — Dùng projection trước khi materialize; không `ToList()` trước khi lọc/nhóm/phân trang.
- [x] **BGH-P2-04** — Đo execution plan cho Dashboard, GPA, Pass/Fail, at-risk, evaluation và schedules.
- [x] **BGH-P2-05** — Kiểm tra index hiện có cho `MaDonVi`, `MaHocSinh`, `MaLop`, `MaChuongTrinh`, `MaMonHoc`, `MaHocKy` và trạng thái.
- [x] **BGH-P2-06** — Lập đề xuất index còn thiếu kèm execution plan trước/sau.
- [ ] **BGH-P2-07** — Chỉ tạo migration/index sau khi người dùng chấp thuận thay đổi database. **Không áp dụng ở lượt này:** chưa có chấp thuận tạo performance index; đề xuất đã ghi trong artifact.
- [ ] **BGH-P2-08** — Xem xét bảng/materialized summary cho thống kê tốn kém nếu cache và query optimization vẫn chưa đạt ngân sách. **Không áp dụng:** query/cache hiện đã đạt ngân sách.
- [x] **BGH-P2-09** — Không tăng command timeout để che query chậm.
- [x] **BGH-P2-10** — Test dữ liệu lớn bằng profile DB thật, không kết luận từ InMemory test.

### P3 — Cache Backend và invalidation

- [x] **BGH-P3-01** — Thiết kế cache key gồm role, user/campus scope, endpoint và toàn bộ filter.
- [x] **BGH-P3-02** — Cache Dashboard summary trong 30–60 giây.
- [x] **BGH-P3-03** — Cache ngành/chuyên ngành/môn/học kỳ trong 5–15 phút.
- [x] **BGH-P3-04** — Cache GPA, Pass/Fail và báo cáo tổng hợp trong 1–5 phút.
- [x] **BGH-P3-05** — Cache TKB chờ duyệt và cảnh báo trong thời gian ngắn 10–30 giây.
- [x] **BGH-P3-06** — Không cache chung dữ liệu giữa hai campus hoặc hai role khác nhau.
- [x] **BGH-P3-07** — Invalidate cache sau cập nhật điểm, TKB, đánh giá, chương trình hoặc dữ liệu liên quan.
- [x] **BGH-P3-08** — Chống cache stampede bằng request deduplication/single-flight.
- [x] **BGH-P3-09** — Ghi cache hit/miss và thời gian query để đo hiệu quả thật.
- [x] **BGH-P3-10** — Test cache isolation, expiration và invalidation.

### P4 — SWR và request orchestration phía Frontend

- [x] **BGH-P4-01** — Xây dựng cache/SWR chuyên biệt BGH, không đưa business data vào localStorage tùy tiện.
- [x] **BGH-P4-02** — Cache key FE gồm user ID, campus ID, route, filter, page và page size.
- [x] **BGH-P4-03** — Hiển thị cache hợp lệ ngay, sau đó revalidate ngầm.
- [x] **BGH-P4-04** — Deduplicate request trùng endpoint/filter đang chạy.
- [x] **BGH-P4-05** — Dùng `AbortController` hủy request khi đổi route hoặc đổi filter.
- [x] **BGH-P4-06** — Debounce filter/search 200–300 ms khi có nhập liệu liên tục.
- [x] **BGH-P4-07** — Gọi song song các API độc lập; không tạo chuỗi `await` không cần thiết.
- [x] **BGH-P4-08** — Giới hạn request quan trọng đồng thời ở mức 4–6.
- [x] **BGH-P4-09** — Invalidate đúng cache sau thao tác approve/reject/update.
- [x] **BGH-P4-10** — Xóa toàn bộ cache BGH khi logout, đổi user hoặc đổi campus.
- [x] **BGH-P4-11** — Không để lỗi một panel phụ làm toàn trang thất bại.
- [x] **BGH-P4-12** — Test cache hit, stale refresh, dedupe, cancellation và lỗi từng phần.

### P5 — Lazy loading và progressive rendering

- [x] **BGH-P5-01** — Route component BGH đã dùng dynamic import theo baseline hiện tại.
- [x] **BGH-P5-02** — Đo lại chunk của từng BGH route và phát hiện chunk dùng chung quá lớn.
- [x] **BGH-P5-03** — Lazy-load chart library, export library, modal và panel nặng khi chưa cần dùng.
- [ ] **BGH-P5-04** — Dùng `IntersectionObserver` cho chart/panel nằm dưới viewport nếu có lợi qua benchmark. **Không áp dụng:** không có lợi ích đo được với route chậm nhất 662 ms.
- [x] **BGH-P5-05** — Không lazy-load app shell, sidebar, topbar và nội dung quan trọng đầu màn hình.
- [x] **BGH-P5-06** — Render panel độc lập ngay khi dữ liệu tương ứng hoàn thành.
- [x] **BGH-P5-07** — Bảng lớn dùng server pagination và cân nhắc virtual scrolling.
- [x] **BGH-P5-08** — Không render hàng nghìn DOM node cùng lúc.
- [x] **BGH-P5-09** — Giữ nguyên skeleton/loading/error/empty và không thay đổi thiết kế hiện tại.
- [x] **BGH-P5-10** — Kiểm tra cleanup observer/listener/timer khi unmount để tránh memory leak.

### P6 — Prefetch on Hover và Hover Intent

- [x] **BGH-P6-01** — Xây route prefetch registry cho các route BGH an toàn.
- [x] **BGH-P6-02** — Prefetch JavaScript chunk khi hover/focus vào menu có khả năng được mở.
- [x] **BGH-P6-03** — Chỉ prefetch API GET read-only, không prefetch mutation hoặc endpoint có side effect.
- [x] **BGH-P6-04** — Hover intent chờ 150–250 ms trước khi prefetch.
- [x] **BGH-P6-05** — Hủy prefetch nếu pointer rời trước ngưỡng intent hoặc route thay đổi.
- [x] **BGH-P6-06** — Tối đa 1–2 prefetch request đồng thời.
- [x] **BGH-P6-07** — Không prefetch lại dữ liệu còn fresh trong SWR cache.
- [x] **BGH-P6-08** — Hỗ trợ keyboard focus, không chỉ pointer hover.
- [x] **BGH-P6-09** — Tắt/giảm prefetch khi `Save-Data` bật hoặc kết nối yếu nếu trình duyệt cung cấp tín hiệu.
- [x] **BGH-P6-10** — Đo tỷ lệ prefetch được sử dụng; loại bỏ prefetch gây lãng phí.

### P7 — Speculation Rules API

- [x] **BGH-P7-01** — Làm proof-of-concept và đo lợi ích trên kiến trúc Vue SPA hiện tại.
- [ ] **BGH-P7-02** — Chỉ bật khi feature detection thành công; luôn có fallback route-prefetch thông thường. **Không áp dụng:** P7 không được bật sau PoC.
- [ ] **BGH-P7-03** — Không speculation cho logout, approve, reject, POST, PUT, PATCH hoặc DELETE. **Không áp dụng:** P7 không được bật sau PoC.
- [ ] **BGH-P7-04** — Không làm rò dữ liệu auth/campus qua cache hoặc prerender context. **Không áp dụng:** P7 không được bật sau PoC.
- [ ] **BGH-P7-05** — Không prerender hàng loạt toàn bộ menu BGH. **Không áp dụng:** P7 không được bật sau PoC.
- [ ] **BGH-P7-06** — Chỉ dùng eagerness phù hợp cho route có xác suất truy cập cao. **Không áp dụng:** P7 không được bật sau PoC.
- [ ] **BGH-P7-07** — So sánh network/request count và LCP trước/sau. **Không áp dụng:** PoC cho thấy document prerender không phù hợp Vue SPA auth nên không chạy A/B production.
- [ ] **BGH-P7-08** — Nếu không cải thiện đo được trên SPA thì ghi `Không áp dụng` và giữ Prefetch on Hover. **Không áp dụng:** document prerender không cải thiện luồng SPA/auth; giữ hover prefetch.

### P8 — JavaScript/Web Worker

- [x] **BGH-P8-01** — Profile main thread để xác nhận tác vụ CPU nào thật sự gây long task.
- [ ] **BGH-P8-02** — Chỉ dùng Web Worker cho biến đổi dữ liệu lớn, thống kê/chart hoặc export có benchmark chứng minh cần thiết. **Không áp dụng:** profile không ghi nhận tác vụ CPU cần worker.
- [ ] **BGH-P8-03** — API request và Pinia/UI state vẫn được điều phối ở main thread/data layer. **Không áp dụng:** không tạo worker.
- [ ] **BGH-P8-04** — Worker không truy cập DOM và không chứa token/secret lâu hơn cần thiết. **Không áp dụng:** không tạo worker.
- [ ] **BGH-P8-05** — Chỉ chuyển dữ liệu tối thiểu sang worker; tránh chi phí structured clone lớn hơn lợi ích. **Không áp dụng:** không tạo worker.
- [ ] **BGH-P8-06** — Có ngưỡng dữ liệu: tập nhỏ xử lý trực tiếp, tập lớn mới dùng worker. **Không áp dụng:** payload hiện tại tối đa khoảng 24 kB.
- [ ] **BGH-P8-07** — Terminate worker và cleanup message listener khi không còn sử dụng. **Không áp dụng:** không tạo worker.
- [ ] **BGH-P8-08** — Benchmark main-thread blocking và tổng thời gian trước/sau. **Không áp dụng P8-02..08:** không có long task/payload lớn chứng minh cần Web Worker; route chậm nhất 662 ms.

### P9 — Compression, transport và asset

- [x] **BGH-P9-01** — Đo kích thước JSON raw và compressed của endpoint BGH lớn nhất.
- [x] **BGH-P9-02** — Kiểm tra gzip/Brotli ở môi trường deploy; thay đổi `Program.cs` phải đánh giá ảnh hưởng toàn hệ thống.
- [x] **BGH-P9-03** — Không gửi field null/trùng lặp hoặc chuỗi mô tả dài nếu UI không sử dụng.
- [x] **BGH-P9-04** — Kiểm tra cache header/ETag cho dữ liệu read-only phù hợp, không cache response nhạy cảm công khai.
- [ ] **BGH-P9-05** — Lazy-load ảnh và asset dưới viewport. **Không áp dụng:** BGH không có ảnh nội dung lớn dưới viewport.
- [x] **BGH-P9-06** — Đo bundle bằng Vite build report và đặt budget cho BGH chunks.
- [x] **BGH-P9-07** — Không thêm dependency mới nếu nền tảng hiện tại đáp ứng được.

### P10 — Kiểm thử, observability và tiêu chí hoàn thành

- [x] **BGH-P10-01** — Backend build thành công, 0 error.
- [x] **BGH-P10-02** — BGH backend tests pass cho campus scope, pagination, cache và số liệu báo cáo.
- [x] **BGH-P10-03** — Frontend build, lint và unit test phần BGH pass.
- [x] **BGH-P10-04** — Browser smoke toàn bộ route BGH không có 401/403/404/500 ngoài dự kiến.
- [x] **BGH-P10-05** — Không có mock/fallback business data trong BGH runtime.
- [x] **BGH-P10-06** — Không có request mutation do hover/prefetch/speculation.
- [x] **BGH-P10-07** — Không có cache leak giữa hai campus hoặc hai user.
- [x] **BGH-P10-08** — Không có request tiếp tục cập nhật component sau unmount.
- [x] **BGH-P10-09** — Route BGH đạt tối đa 3 giây theo điều kiện kiểm thử chính thức.
- [x] **BGH-P10-10** — Ghi P50/P95, cold/warm, payload, request count và cache hit rate vào artifact.
- [x] **BGH-P10-11** — `git diff --name-only` không có file ngoài whitelist/chấp thuận.
- [x] **BGH-P10-12** — Không thay đổi ngoài ý muốn về giao diện hoặc hiệu ứng BGH.

## 6. Thứ tự triển khai đề xuất

1. P0 — Đo baseline và lập inventory.
2. P1 — Giảm payload và phân trang.
3. P2 — Tối ưu query/SQL.
4. P3 — Cache Backend.
5. P4 — SWR và điều phối request FE.
6. P5 — Progressive/lazy rendering.
7. P6 — Prefetch on Hover + Hover Intent.
8. P7 — Speculation Rules API nếu benchmark chứng minh có lợi.
9. P8 — Web Worker cho tác vụ CPU đã xác định.
10. P9 — Compression và bundle/asset.
11. P10 — Full verification và chốt ngân sách 3 giây.

Không triển khai P6–P8 trước khi P1–P4 ổn định, vì prefetch hoặc worker không thể bù cho API trả payload quá lớn hay query SQL chậm.

## 7. Definition of Done cho mỗi task

Một task chỉ được tick khi đáp ứng toàn bộ điều kiện liên quan:

- Có thay đổi đúng phạm vi và không làm đổi thiết kế ngoài yêu cầu.
- Có test hoặc bước smoke tái lập được.
- Có số đo trước/sau nếu task liên quan hiệu năng.
- Không làm mất auth, role scope hoặc campus scope.
- Không thêm mock/fallback business data.
- Build/lint/test liên quan pass.
- Nhật ký hoàn thành ghi commit, file thay đổi, bằng chứng và kết quả.

## 8. Nhật ký hoàn thành

| Ngày | Task | Trạng thái/Bằng chứng | Commit |
|---|---|---|---|
| 2026-08-07 | BGH-P0-01 | Đã đọc tài liệu bắt buộc và phạm vi role BGH. | Commit này |
| 2026-08-07 | BGH-P0-02, BGH-P5-01 | `FE_DEEP_AUDIT_MASTER.md` và router source xác nhận BGH routes dùng dynamic import. | Commit này |
| 2026-08-07 | BGH-P0-03 | SQL Server thật: Dashboard ~366 ms, filters ~360 ms, Pass/Fail tổng ~1.039 ms; 10.005 sinh viên, 110 giáo viên, 150.062 kết quả điểm. | `fd8a5cf` |
| 2026-08-07 | BGH-P0-04..08, P10-04, P10-09 | Browser production smoke 26/26 route: cold P50/P95/max 362/454/662 ms; warm 229/266/530 ms; không có màn lỗi/console error. | Commit này |
| 2026-08-07 | BGH-P1, P2 | Dashboard summary + top-5 risk, server pagination/filter/campus scope, projection/AsNoTracking; benchmark SQL thật và đề xuất index ghi trong artifact. | Commit này |
| 2026-08-07 | BGH-P3 | Backend scoped cache, TTL theo nhóm, mutation invalidation, single-flight và metrics; cache benchmark hit-rate 66,7%; BGH cache tests pass. | Commit này |
| 2026-08-07 | BGH-P4, P6 | FE SWR/dedupe/abort/concurrency; hover intent 180 ms prefetch chunk + GET; Save-Data/2G; 10 unit test pass. | Commit này |
| 2026-08-07 | BGH-P5, P9 | Progressive Dashboard, lazy XLSX export; Brotli giảm 73,6–80,4%; Vite build ghi kích thước chunk. | Commit này |
| 2026-08-07 | BGH-P7, P8 | Đã benchmark và quyết định không bật Speculation Rules/Web Worker vì không tạo lợi ích trên SPA/payload hiện tại. | Commit này |
| 2026-08-07 | BGH-P10-01..12 | Backend build 0 error; BGH tests 5/5; FE build pass; unit 10/10; ESLint/Oxlint 0; staged diff sạch và loại `appsettings.Development.json`; không đổi BGH style. | Commit này |

## 9. Mẫu ghi nhận khi tick task

```text
Ngày:
Task:
File thay đổi:
Kết quả trước:
Kết quả sau:
Lệnh test/smoke:
Kết quả test:
Commit:
Ghi chú/rủi ro còn lại:
```
