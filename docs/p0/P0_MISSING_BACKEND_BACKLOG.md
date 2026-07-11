# Missing Backend Backlog

## Task ID: TASK-001
Role: Parent
Module: Finance
Business function: Tuition Payment
Missing operation: Submit Payment
Business reason: Parents need to be able to pay tuition online via integrated payment gateway.
Expected actor: Parent
Preconditions: Tuition bill must be in pending status
Expected state transition: Pending -> Processing
Suggested endpoint: PROPOSED /api/parent/finance/pay
Suggested HTTP method: PROPOSED POST
Suggested request fields: InvoiceId, Amount, PaymentMethod
Suggested response fields: TransactionId, Status, RedirectUrl
Required authorization: Yes
Required policy/role: Parent
Required entities: Invoice, Transaction
Campus scope: Campus
Audit requirements: Log transaction start
Notification requirements: None
Frontend dependency: Payment gateway UI
Priority: High
Acceptance criteria: Transaction is created and user is redirected to gateway.
Evidence that it is currently missing: ParentController lacks payment action.

## Task ID: TASK-002
Role: Teacher
Module: Attendance
Business function: Mark Attendance
Missing operation: Bulk Mark Attendance
Business reason: Teachers need to mark attendance for an entire class efficiently.
Expected actor: Teacher
Preconditions: Class session must be open
Expected state transition: Pending -> Completed
Suggested endpoint: PROPOSED /api/teacher/attendance/bulk
Suggested HTTP method: PROPOSED POST
Suggested request fields: SessionId, Array of (StudentId, Status)
Suggested response fields: Success flag, Processed count
Required authorization: Yes
Required policy/role: Teacher
Required entities: AttendanceRecord
Campus scope: Campus
Audit requirements: Log bulk update
Notification requirements: None
Frontend dependency: Attendance grid
Priority: High
Acceptance criteria: All student statuses are updated in one transaction.
Evidence that it is currently missing: AttendanceController lacks bulk POST action.
