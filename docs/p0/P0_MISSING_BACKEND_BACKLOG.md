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

