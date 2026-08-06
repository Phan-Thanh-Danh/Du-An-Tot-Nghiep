# P0 Missing Backend Backlog

## CAP-PAY-002: Parent initiates payment
- **Role**: Parent
- **Status**: PARTIAL
- **Gap Details**: Endpoint exists but implementation is incomplete or mocked.
- **Missing Evidence**: 
- **Proposed Endpoints**: Needs implementation for Parent initiates payment
- **Acceptance Criteria**: 
  - Create transaction and link invoice.
  - Idempotency to prevent duplicate payments.
  - Gateway checkout or payment intent implementation.
  - Callback/webhook verification.
  - Update success/failure status.
  - Do not mark the invoice as paid until gateway confirmation.

## CAP-STF-007: AcademicStaff books room
- **Role**: AcademicStaff
- **Status**: PARTIAL
- **Gap Details**: Endpoint exists but implementation is incomplete or mocked.
- **Missing Evidence**: 
- **Proposed Endpoints**: Needs implementation for AcademicStaff books room
- **Acceptance Criteria**: Must fully implement business logic for AcademicStaff books room with proper EF Core queries and role authorization.

## GIAOVU-001: View student list per class (future feature)
- **Role**: AcademicStaff
- **Status**: NOT STARTED (chưa nằm trong scope hiện tại)
- **Gap Details**: `classApi.getStudents()` từng là stub "đang phát triển" và đã được xóa (dead code, không view nào gọi). Hiện tại ClassManagementView chỉ hiển thị sĩ số (`siSo/siSoToiDa`); không có UI lẫn BE endpoint cho "xem danh sách học sinh trong lớp".
- **Missing Evidence**: 
- **Proposed Endpoints**: `GET /api/admin/classes/{maLop}/students` (dự kiến) + UI: click vào lớp → xem danh sách học sinh.
- **Acceptance Criteria**: GiaoVu mở lớp → thấy danh sách học sinh (mã, họ tên, mã lớp hành chính); phân trang khi lớp đông; role AcademicOperations.

