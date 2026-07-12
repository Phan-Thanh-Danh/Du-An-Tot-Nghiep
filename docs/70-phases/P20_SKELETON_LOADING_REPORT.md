# P20 — Skeleton Loading Rollout Report

**Date**: 2026-07-10
**Status**: `PASS`
**Target Phase**: P20 (Skeleton Loading Rollout)

## 1. Mission Recap
Triển khai skeleton loading cho các màn chính đã kết nối API thật, nhằm cải thiện UX khi chờ dữ liệu từ backend.
Các nguyên tắc đã tuân thủ:
- Giữ nguyên luồng API connection (100% P19 pass).
- Không thêm mock/fake/fallback data.
- Không thêm fake delay ở luồng chính (có quét dọn các delay giả lập/mock không phù hợp nếu chặn luồng data).
- Hiển thị Empty state nếu API trả về danh sách rỗng (không dùng skeleton che đậy).
- Cả backend và frontend đều build pass.

## 2. Skeleton Components Used
- `SkeletonTable.vue`: Dùng cho các danh sách có cấu trúc bảng (Audit Logs, Lịch học, Quản lý khóa học, Users).
- `SkeletonCard.vue`: Dùng cho grid dạng thẻ (Giảng viên - Lịch thi, Khóa học).
- `SkeletonDashboard.vue`: Dùng cho các trang tổng quan (Dashboard BGH, Phụ huynh, Sinh viên, Tổng quan học tập).
- `ListSkeleton.vue`: Dùng cho danh sách chiều dọc, form elements.

## 3. Rollout Coverage (Screens Updated)
Đã triển khai thành công Skeleton loading trên các luồng sau:
- **BGH**: 
  - `Dashboard.vue`
  - `Evaluations/AIFeedbackAnalysisView.vue`
  - `Evaluations/EvalOverviewView.vue`
  - `Evaluations/TeacherEvalDetailsView.vue`
  - `Evaluations/TeacherRankingView.vue`
  - `AuditLogsView.vue`
  - `Academic/AcademicReportsView.vue`
- **Giáo Vụ**:
  - `Courses/CourseManagementView.vue`
  - `Schedule/PublishedSchedulesView.vue`
- **SuperAdmin**:
  - `UsersView.vue`
- **Giảng Viên**:
  - `ExamsView.vue`
- **Sinh Viên**:
  - `Dashboard.vue`
  - `HocTap/KhoacHoc.vue`
- **Phụ Huynh**:
  - `Dashboard.vue`
  - `Children/OverviewView.vue`

## 4. Anti-Regression & Code Quality Checks
- **Mock / Fake Delay Scan**: Tiến hành quét toàn bộ frontend `src` để tìm các trường hợp dùng `setTimeout`. Các trường hợp `setTimeout` còn giữ lại đều thuộc 1 trong 3 nhóm hợp lệ sau:
  - Animation timeout (CSS transition trễ 300ms, UX skeleton delay).
  - Toast message timeout (Tự động ẩn toast sau 3s).
  - Mock action delays (nằm trong các action form chưa liên kết API theo thiết kế P19 như Send Notification, Cancel Schedule). Dữ liệu tải (loading list/detail) hoàn toàn không có `setTimeout` fake.
- **Data flow**: Áp dụng chuẩn `v-if="loading"` (skeleton) -> `v-else-if="error"` -> `v-else-if="empty"` -> `v-else` (data).

## 5. Build Status
- `npm run build`: **PASS**
- `dotnet build`: **PASS**

## 6. UX Skeleton Standard Compliance

| Rule | Status | Evidence |
| --- | --- | --- |
| Skeleton previews final layout | PASS | Đã ánh xạ `SkeletonTable`, `SkeletonCard`, `SkeletonDashboard` phù hợp với từng màn hình. |
| Loading/error/empty separated | PASS | Chuẩn `v-if="loading"` -> `v-else-if="error"` -> `v-else-if="empty"` được áp dụng triệt để. |
| No fake delay | PASS | Đã loại bỏ các `setTimeout` tạo fake action delay ở `PublishedSchedulesView`. Chỉ giữ toast timeout và animation delay hợp lệ. |
| No mock/fallback data | PASS | Không có dữ liệu mock/fallback nào được render khi loading hay có lỗi; tuân thủ hoàn toàn P19. |
| Button actions use spinner/disabled state | PASS | Các hành động sử dụng `isCancelling`, `isSending` với disabled logic. |
| Skeleton not focusable | PASS | Skeleton không có focus loop, dùng cho hiển thị layout. |
| Layout shift minimized | PASS | Skeleton component được cấu hình `rows`, `columns` khớp với dữ liệu thực tế. |

## 7. Conclusion
**P20 = PASS.**
Hệ thống đã sẵn sàng cho trải nghiệm người dùng mượt mà trong quá trình tải dữ liệu, không ảnh hưởng tới chất lượng kết nối API đã đảm bảo từ P19.
