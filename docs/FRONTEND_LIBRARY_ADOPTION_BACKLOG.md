# Frontend Library Adoption Backlog

Tài liệu này ghi lại các thư viện/tool đã được nghiên cứu để dùng trong các phase sau, nhưng **KHÔNG** áp dụng trong mock-first batch hiện tại.

## Danh sách thư viện dự kiến

### 1. `@tanstack/vue-query`
- **Dùng khi:** Bắt đầu nối API thật cho Notification, Applications, Reward/Discipline, Schedule/Attendance.
- **Lý do:** Quản lý server state, cache, refetch, pagination, loading/error state.
- **Lưu ý:** Không dùng trong mock batch vì mock service hiện tại đủ đơn giản.

### 2. `vee-validate` + `zod` + `@vee-validate/zod`
- **Dùng khi:** Form động Đơn từ hoặc form nghiệp vụ bắt đầu có validation phức tạp từ backend schema.
- **Module ưu tiên:** Đơn từ, Kỷ luật, Thời khóa biểu.
- **Lưu ý:** Không dùng trong mock batch nếu custom validation hiện tại đủ ổn.

### 3. `@fullcalendar/vue3`
- **Dùng khi:** Thời khóa biểu cần calendar view chuyên nghiệp hơn grid/list tự thiết kế.
- **Lưu ý:** 
  - Chỉ cân nhắc sau khi custom Liquid Glass schedule grid không đáp ứng.
  - Nếu dùng, **phải** wrap/override style theo Liquid Glass, không để theme mặc định phá UI.

### 4. `@uppy/vue`
- **Dùng khi:** Upload minh chứng cần drag-drop, progress, retry, multi-file UX tốt hơn.
- **Module ưu tiên:** Đơn từ, Khiếu nại kỷ luật.
- **Lưu ý:** Không dùng trong mock batch; hiện dùng upload mock tự style.

### 5. `reka-ui`
- **Dùng khi:** Component UI hiện có thiếu Dialog/Tabs/Popover/Combobox accessible.
- **Lưu ý:**
  - Chỉ dùng primitive unstyled, bọc lại bằng Glass component.
  - Không dùng như design system mới.

### 6. `vue-tippy`
- **Dùng khi:** Cần tooltip/popover nhỏ giải thích trạng thái, SLA, ca học, quyền thao tác.
- **Lưu ý:** Không dùng cho layout chính.

### 7. `chart.js` / `vue-chartjs`
- **Dùng khi:** Report dashboard cần biểu đồ thật sau khi có API dữ liệu thật.
- **Lưu ý:** Không dùng trong mock batch; hiện dùng card/table trước.

## Quy tắc áp dụng

- **Batch hiện tại:** Trong final report phải ghi rõ: `External libraries added: None`.
- **Không cài đặt:** Không được cài bất kỳ thư viện nào ở trên trong batch mock-first.
- **Quy trình thêm mới:** Khi cần dùng thư viện, phải tạo task riêng, có lý do, module áp dụng, ảnh hưởng `package.json`, build result và rollback plan.
- **Ưu tiên:** Ưu tiên component Liquid Glass hiện có trước khi thêm dependency.
