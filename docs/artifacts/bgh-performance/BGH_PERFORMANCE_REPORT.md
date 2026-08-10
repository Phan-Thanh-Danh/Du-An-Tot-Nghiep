# Báo cáo tối ưu hiệu năng toàn bộ khu vực BGH

Ngày đo: 2026-08-07
Nhánh: `codex/fix-bgh-role`

## 1. Kết luận

- Browser smoke 26/26 route BGH thành công, không có màn báo lỗi và không có console error.
- Cold navigation: P50 **362 ms**, P95 **454 ms**, chậm nhất **662 ms**.
- Warm navigation: P50 **229 ms**, P95 **266 ms**, chậm nhất **530 ms**.
- Tất cả route đều thấp hơn ngân sách bắt buộc 3.000 ms.
- Không thêm mock/fallback business data. Số liệu đến từ SQL Server thật.
- Không thay đổi CSS, layout, màu sắc hay animation BGH. Thay đổi template chỉ gắn listener prefetch lên menu sẵn có và hiển thị tổng phân trang do server trả về.

## 2. Điều kiện kiểm thử chính thức

| Thuộc tính | Giá trị |
|---|---|
| Trình duyệt | Chrome trong Codex in-app browser |
| Frontend | Vite production build, `http://localhost:5174` |
| Backend | ASP.NET Core, `http://localhost:5097` |
| Database | SQL Server `CuongCoder\\SQLEXPRESS`, database `LMS` |
| Network | Localhost, không throttling |
| Cold | Lần điều hướng đầu trong tab/route, trước khi route chunk và SWR data được dùng lại |
| Warm | Điều hướng lại sau khi chunk/cache đã được nạp |
| Dữ liệu | 10.005 sinh viên, 110 giáo viên, 12 lớp, 47 TKB chờ duyệt, 150.062 kết quả điểm |
| Pass/Fail | 128.620 pass, 21.442 fail; 3 ngành, 3 chuyên ngành, 63 môn trong chương trình, 6 học kỳ |
| Tài khoản | Role `Principal`, dữ liệu thật theo auth/campus scope |

Các phép đo browser tính từ lúc bắt đầu điều hướng đến khi nội dung route xuất hiện và không còn trạng thái lỗi. Do chạy localhost, kết quả là baseline máy phát triển; môi trường deploy cần lặp lại với network profile thực tế.

## 3. Inventory route, component, API và số request nghiệp vụ chính

Số request bên dưới là request nghiệp vụ chính trong lượt nạp đầu theo source; request auth/shell dùng chung không tính lặp lại.

| Route | Component | API chính | Request |
|---|---|---|---:|
| `/bgh/dashboard` | `Dashboard.vue` | dashboard; evaluation ranking; pass/fail filters + chart | 4 |
| `/bgh/organizations` | `OrganizationsView.vue` | organizations + tree | 2 |
| `/bgh/users` | `UsersView.vue` | users page; roles; organizations | 3 |
| `/bgh/roles` | `RolesView.vue` | RBAC roles | 1 |
| `/bgh/academic-programs` | `ProgramsView.vue` | training programs | 1 |
| `/bgh/curriculum` | `CurriculumView.vue` | training programs; subjects | 2 |
| `/bgh/academic-terms` | `AcademicTermsView.vue` | terms; cohorts | 2 |
| `/bgh/academic/overview` | `AcademicOverviewView.vue` | academic overview | 1 |
| `/bgh/academic/gpa` | `GPAReportsView.vue` | GPA summary | 1 |
| `/bgh/academic/at-risk` | `AtRiskStudentsView.vue` | at-risk page | 1 |
| `/bgh/academic/at-risk/:id/history` | `StudentHistoryView.vue` | student history/detail | 1 |
| `/bgh/academic/reports` | `AcademicReportsView.vue` | academic reports | 1 |
| `/bgh/academic/pass-fail` | `PassFailRatesView.vue` | hierarchy filters; pass/fail | 2 |
| `/bgh/schedule/pending` | `SchedulePendingView.vue` | schedules page, pending | 1 |
| `/bgh/schedule/conflicts` | `ConflictListView.vue` | schedules page | 1 |
| `/bgh/schedule/published` | `PublishedSchedulesView.vue` | schedules page, approved | 1 |
| `/bgh/schedule/changes` | `ScheduleChangesView.vue` | schedule changes | 1 |
| `/bgh/evaluations` | `EvaluationsView.vue` | evaluations; ranking | 2 |
| `/bgh/evaluations/ranking` | `TeacherRankingView.vue` | ranking | 1 |
| `/bgh/evaluations/detail/:id` | `TeacherEvalDetailsView.vue` | evaluation detail | 1 |
| `/bgh/evaluations/overview` | `EvalOverviewView.vue` | evaluation overview | 1 |
| `/bgh/evaluations/ai-analysis` | `AIFeedbackAnalysisView.vue` | AI analysis | 1 |
| `/bgh/facilities` | `FacilitiesView.vue` | buildings; floors; rooms | 3 |
| `/bgh/audit-logs` | `AuditLogsView.vue` | audit page | 1 |
| `/bgh/profile` | `ProfileView.vue` | current profile | 1 |
| `/bgh/notifications` | shared `NotificationsView.vue` | notifications | 1 |

## 4. Kết quả browser từng route

| Route | Cold ms | Warm ms | DOM baseline (node) | Kết quả |
|---|---:|---:|---:|---|
| Dashboard | 662 | 530 | 925 | PASS |
| Organizations | 374 | 234 | 544 | PASS |
| Users | 366 | 249 | 754 | PASS |
| Roles | 357 | 222 | 609 | PASS |
| Academic programs | 364 | 231 | 645 | PASS |
| Curriculum | 377 | 235 | 536 | PASS |
| Academic terms | 350 | 228 | 677 | PASS |
| Academic overview | 350 | 227 | 535 | PASS |
| GPA | 351 | 228 | 540 | PASS |
| At-risk | 338 | 233 | 540 | PASS |
| Student history | 366 | 240 | 100 dòng semantic mới nhất | PASS |
| Academic reports | 362 | 230 | 652 | PASS |
| Pass/Fail | 355 | 249 | 1.446 | PASS |
| Schedule pending | 454 | 266 | 1.948 | PASS |
| Schedule conflicts | 375 | 247 | 904 | PASS |
| Schedule published | 375 | 236 | 673 | PASS |
| Schedule changes | 337 | 221 | 546 | PASS |
| Evaluations | 337 | 238 | 1.656 | PASS |
| Teacher ranking | 437 | 257 | 1.347 | PASS |
| Evaluation detail | 416 | 254 | 106 dòng semantic mới nhất | PASS |
| Evaluation overview | 348 | 228 | 556 | PASS |
| AI analysis | 338 | 223 | 618 | PASS |
| Facilities | 338 | 225 | 535 | PASS |
| Audit logs | 369 | 244 | 865 | PASS |
| Profile | 371 | 229 | 607 | PASS |
| Notifications | 352 | 229 | 598 | PASS |

Năm route cold chậm nhất: Dashboard 662 ms, TKB chờ duyệt 454 ms, xếp hạng 437 ms, chi tiết đánh giá 416 ms và curriculum 377 ms. Nguyên nhân còn lại chủ yếu là tải route chunk và render DOM; API warm đều nhỏ hơn ngân sách.

## 5. Benchmark API thật

Các số P95 là chuỗi gọi lặp sau lần cold; đơn vị ms.

| API | Cold | Warm P50 | Warm P95 | Payload raw byte |
|---|---:|---:|---:|---:|
| Dashboard summary | 346 | 6,5 | 13,6 | 1.555 trước khi thêm top-5 risk |
| Evaluation ranking | 44 | 8,3 | 22,6 | 1.863 |
| Evaluation overview | 131 | 7,1 | 8,2 | 885 |
| AI analysis | 89 | 6,2 | 7,0 | 488 |
| Academic overview | 704,8 | 6,8 | 9,6 | 1.694 |
| GPA | 309,7 | 2,6 | 3,6 | 824 |
| At-risk page | 419,7 | 11,9 | 14,5 | phân trang |
| Academic reports | 61 | 1,7 | 2,8 | 216 |
| Pass/Fail filters | 259,6 | 2,4 | 2,7 | 9.232 |
| Pass/Fail chart | 990,7 | 2,3 | 3,0 | 3.445 |
| Schedule changes | 53 | 1,8 | 2,1 | nhỏ |
| Schedules page | 88,7 | 2,0 | 2,7 | 24.262 |
| Users page | 60 | 2,5 | 2,8 | 4.402 |
| Audit page | 42 | 2,0 | 2,4 | 7.647 |

Cache backend sau benchmark: 12 hit, 6 miss, 6 factory execution, 6 key theo dõi; hit-rate **66,7%**. Cache sử dụng single-flight nên các miss đồng thời không chạy lại cùng factory.

## 6. Payload, compression và bundle

| Hạng mục | Raw | Brotli | Giảm |
|---|---:|---:|---:|
| Schedules | 24.262 B | 4.746 B | 80,4% |
| Pass/Fail filters | 9.232 B | 2.435 B | 73,6% |
| Audit logs | 7.790 B | 1.623 B | 79,2% |

Vite production build:

| Chunk | Raw | Gzip |
|---|---:|---:|
| BGH Dashboard | 33,27 kB | 9,20 kB |
| Layout BGH | 13,02 kB | 4,59 kB |
| At-risk | 16,78 kB | 5,20 kB |
| Pass/Fail | 18,56 kB | 5,39 kB |
| XLSX export | 282,67 kB | 94,18 kB |
| Shared vendor | 1.128,90 kB | 297,18 kB |

XLSX đã được chuyển sang dynamic import và chỉ tải khi người dùng bấm export. Shared vendor là chunk toàn ứng dụng, không phải payload riêng của từng BGH route; route-level dynamic import vẫn giữ nguyên.

## 7. SQL/EF Core và index

- Read query BGH dùng projection và `AsNoTracking()` khi phù hợp; lọc/nhóm/phân trang thực hiện trước materialize.
- At-risk được chuyển từ correlated aggregation sang tập group thống kê, có filter và server pagination.
- Dashboard chỉ query các aggregate/card cần hiển thị và top-5 cảnh báo; không gọi danh sách at-risk đầy đủ.
- Pass/Fail đi đúng chuỗi dữ liệu thật: `NganhDaoTao -> ChuyenNganh -> MonHocTrongChuongTrinh -> HocKy -> DiemSo`.
- Đã kiểm tra index/DMV cho các khóa `MaDonVi`, `MaHocSinh`, `MaLop`, `MaChuongTrinh`, `MaMonHoc`, `MaHocKy`, trạng thái. `DiemSo` đã có index `(MaHocSinh, MaHocKy)`, `(MaDonVi, MaHocKy)` và unique theo sinh viên/môn/học kỳ.
- Đề xuất cần duyệt trước khi triển khai: index phủ cho các filter `DiemSo.MaDonVi` và `NguoiDung(MaDonVi, VaiTroChinh)` kèm INCLUDE đúng projection. Không tạo migration index vì checklist yêu cầu người dùng chấp thuận thay đổi database trước.
- Không tăng command timeout. Materialized summary không cần thiết vì cold/warm đã đạt ngân sách.

## 8. Cache, SWR và invalidation

- Backend key gồm role, user, campus, endpoint và filter; TTL theo nhóm dữ liệu từ 15 giây đến 15 phút.
- Middleware xóa prefix cache BGH sau mutation thành công, bao phủ điểm, TKB, đánh giá, chương trình và dữ liệu liên quan.
- Frontend SWR key gồm auth/campus scope và toàn bộ path/query; không lưu business data vào localStorage.
- Có fresh/stale refresh, request dedupe, `AbortController`, concurrency 6 request chính và 2 prefetch.
- Hover intent 180 ms prefetch cả route chunk và GET read-only; hủy khi rời menu/đổi route; hỗ trợ focus bàn phím; bỏ qua khi Save-Data/2G.
- Metrics gồm attempt, skipped, used và use-rate. Unit test xác nhận prefetched response được request foreground dùng lại.

## 9. Quyết định không áp dụng sau benchmark

- **Speculation Rules API:** không bật. BGH là Vue SPA auth; prerender URL document sẽ tạo thêm document request nhưng API/chunk vẫn do SPA điều phối, có rủi ro lãng phí và context auth. Hover-intent prefetch đã đưa P95 xuống 454 ms cold.
- **Web Worker:** không bật. Payload lớn nhất khoảng 24 kB, DOM lớn nhất 1.948 node và không ghi nhận long task làm route vượt 3 giây; structured-clone/worker lifecycle sẽ tăng độ phức tạp mà không có lợi ích đo được.
- **IntersectionObserver/virtual scrolling:** chưa cần. Danh sách lớn đã server pagination; route chậm nhất 662 ms và DOM thấp hơn 2.000 node.
- **Lazy image:** khu vực BGH không có ảnh nội dung lớn dưới viewport; icon là SVG/component.
- **ETag/public cache:** dữ liệu BGH có auth/campus scope, dùng private application cache + SWR; không public-cache response nhạy cảm.

## 10. Sửa sai lệch schema notification

Database đánh dấu migration notification cũ đã áp dụng nhưng thiếu bảng `ThongBaoNguoiNhan` và thiếu các cột entity đang map trong `ThongBao`. Hai migration repair dùng SQL idempotent đã:

- tạo bảng/index/FK người nhận và backfill legacy notification;
- bổ sung cột/index/FK bị thiếu mà không xóa dữ liệu;
- đưa `/bgh/notifications` từ lỗi query về empty/real state hợp lệ, warm load 294 ms khi kiểm tra riêng.

## 11. Lệnh và bằng chứng kiểm thử

```powershell
dotnet build Backend/Backend.csproj --no-restore
dotnet test Backend.ApiTests/Backend.ApiTests.csproj --filter FullyQualifiedName~Bgh
npx.cmd vitest run src/components/BGH/performance/__tests__/bghDataClient.spec.js src/components/BGH/performance/__tests__/bghRoutePrefetch.spec.js
npx.cmd eslint <toàn bộ file BGH thay đổi>
npx.cmd oxlint <toàn bộ file BGH thay đổi>
npm.cmd run build
```

- Unit test SWR/prefetch: 10/10 pass.
- Browser smoke: 26/26 pass, không có 401/403/404/500, console log cuối 0 lỗi.
- Mock scan các runtime BGH không thấy `ENABLE_MOCK_API`, fallback business data, fake/random/demo data.
- File `Backend/appsettings.Development.json` là thay đổi cục bộ của người dùng và không thuộc commit này.
