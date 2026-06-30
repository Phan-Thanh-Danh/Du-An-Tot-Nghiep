# FE UI Research Notes

Pham vi: ghi nhanh cac pattern UX ben ngoai duoc tham khao truoc khi rework UI 4 module core. Muc tieu la hoc pattern, khong copy aesthetic, khong pha Liquid Glass.

## Sources Read

| Source | Main pattern learned | Ap dung vao EduLMS |
|---|---|---|
| GOV.UK Design System - Error message: https://design-system.service.gov.uk/components/error-message/ | Loi validation can hien canh field co loi, dung message cu the. | Form tao don, compose notification, appeal discipline, attendance unlock request. |
| GOV.UK Design System - Error summary: https://design-system.service.gov.uk/components/error-summary/ | Form dai can co summary loi dau form de user sua nhanh. | Don tu multi-step va admin decision form. |
| GOV.UK Design System - Validation pattern: https://design-system.service.gov.uk/patterns/validation/ | Noi ro sai gi va sua the nao, tranh loi chung chung. | Dynamic request form, evidence upload, admin compose. |
| GOV.UK Design System - Check answers: https://design-system.service.gov.uk/patterns/check-answers/ | Giao dich nhieu buoc nen co buoc xem lai truoc khi submit. | Student request wizard: chon loai don -> dien form -> minh chung -> xem lai. |
| WAI-ARIA APG home: https://www.w3.org/WAI/ARIA/apg/ | Chi dung ARIA khi native HTML khong du; component tu tao phai co keyboard semantics. | Dialog, tabs, split pane, table action rows, status radios. |
| WAI-ARIA table pattern: https://www.w3.org/WAI/ARIA/apg/patterns/table/ | Table tinh nen la table HTML; table co nhieu widget can giam tab stops hoac dung grid pattern. | Admin queue, candidate table, discipline table: giu table semantic nhung action gom gon. |
| WAI-ARIA grid pattern: https://www.w3.org/WAI/ARIA/apg/patterns/grid/ | Grid tuong tac can arrow/Home/End neu lam composite widget. | Week schedule grid chi la card/list, khong gan role grid neu chua implement keyboard navigation. |
| WAI keyboard interface: https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/ | Custom dialog/toolbar/listbox phai thao tac duoc bang keyboard. | Modal tao don, notification compose preview, attendance quick mark. |
| NN/g Dashboards: Making Charts and Graphs Easier to Understand: https://www.nngroup.com/articles/dashboards-preattentive/ | Dashboard can dung vi tri/length/hierarchy de nguoi dung doc nhanh, khong dua qua nhieu so ngang nhau. | KPI cards chi dat 3-5 chi so quan trong; report dung drilldown thay vi nhieu chart trang tri. |
| NN/g Data Visualizations for Dashboards: https://www.nngroup.com/videos/data-visualizations-dashboards/ | Overview can doc nhanh va dung visual encoding de giam tai nhan thuc. | Staff/BGH dashboards dung summary + table/action panel, tranh gradient hero lon. |
| Linear filters docs: https://linear.app/docs/filters | Filter la mot lop thao tac chinh, can quick filters va search ro rang. | Inbox notification, admin application queue, schedule conflict, reward candidates. |
| Linear board layout docs: https://linear.app/docs/board-layout | List/board switch chi nen dung khi workflow co state columns ro. | Don tu admin co the dung queue list + detail panel, khong can board neu chua co drag/drop. |
| Linear concepts/views: https://linear.app/docs/conceptual-model | Views la tap filter dong, khong phai route demo rieng. | Staff/BGH saved filters co the la tab trong cung route, khong tao route moi neu khong can. |

## Patterns To Apply

### 1. Enterprise Action Layout

- Page header gom title, subtitle, primary action, secondary utility actions.
- Bo filter/search nam ngay duoi header trong `GlassPanel` compact.
- Noi dung chinh chia thanh list/table + detail/action panel khi user can xu ly tung item.
- KPI chi dat o tren khi no giup quyet dinh ngay, khong dat de trang tri.

Ap dung:
- `/student/requests`: summary nho + filter + list cards + detail/wizard.
- `/super-admin/approvals/requests`: table queue + sticky detail/action panel.
- `/staff/conflicts`: conflict list + impact panel + action suggestions.

### 2. Fast Filtering

- Filter phai gan voi viec nguoi dung lam: tat ca/chua doc/khan cap, trang thai don, hoc ky, lop, phong, ca.
- Search placeholder phai noi ro search theo gi.
- Badge filter dang active phai co text tieng Viet.

Ap dung:
- Notification inbox: Tat ca, Chua doc, Khan cap, Hoc vu, Tai chinh.
- Attendance history: tu ngay, den ngay, trang thai.
- Reward/Discipline admin: hoc ky, co so, trang thai, loai, search sinh vien.

### 3. Progressive Disclosure

- Khong nhet tat ca fields vao card.
- Card/list item chi hien thong tin dinh danh + trang thai + han/action.
- Detail panel/modal moi hien timeline, evidence, payload readonly.

Ap dung:
- Student request card: title, type, status, ngay nop, han xu ly, action xem.
- Discipline student: list kin dao, detail moi hien ly do cong khai va timeline.
- Certificate card: status PDF + action tai, detail tuyen duong rieng.

### 4. Form Error And Review

- Field loi hien gan field.
- Form nhieu buoc co summary loi va buoc xem lai.
- Khong dung `alert()`/`confirm()` browser.

Ap dung:
- RequestForm: error summary trong `GlassPanel` danger/readable.
- Notification compose: preview nguoi nhan truoc khi gui.
- Discipline appeal: ly do bat buoc co helper text va disabled submit khi thieu.

### 5. Accessible Native First

- Table doc du lieu dung `<table>` trong `TableShell`.
- Radio/checkbox/select/input dung native control duoc style bang token.
- Dialog can close button co `aria-label`, focus target dau tien, overlay click phu hop.
- Week schedule grid khong gan role grid neu chua co keyboard model day du.

### 6. Visual Tone By Module

- Schedule/attendance: ro thoi gian, ca, phong, teacher; mau trang thai nhe.
- Applications: hanh chinh, minh bach trang thai, timeline la trung tam.
- Notifications: split-pane can doi, unread hierarchy, priority ro nhung khong choi.
- Rewards: trang trong, dung amber/award icon lam accent nho.
- Discipline: nghiem tuc, kin dao, it glow, khong expose sensitive details o student list.

## Library Decision

- Khong them library trong phase audit/blueprint.
- `reka-ui` chi can neu code phase can Dialog/Tabs/Select accessible hon native.
- `vee-validate` + `zod` chi can neu RequestForm dynamic validation qua phuc tap voi custom code.
- `@uppy/vue` chua can trong rework mock UI; evidence upload co the dung input file custom.
- `@tanstack/vue-query` de danh cho phase noi API that.
- `@fullcalendar/vue3` chua can; week grid custom phu hop hon Liquid Glass.
- `chart.js/vue-chartjs` chi can neu report yeu cau chart thuc su, khong dung cho card/table.

## How This Preserves Liquid Glass

- Tat ca pattern tren can duoc skin bang `GlassPanel`, `GlassButton`, `GlassBadge`, `GlassInput`, `TableShell`, `LoadingSkeleton`, `ProgressBar`.
- Khong copy mau, typography, shadow tu nguon ngoai.
- Khong them design system moi.
- Khong hardcode white/black/slate cho module rework.
