# API Contract

Tài liệu này phân biệt rõ endpoint **đã có** và endpoint **dự kiến**. Chỉ đánh dấu **đã có** khi đã thấy controller/action trong `Backend/Controllers`.

Base path hiện tại: `/api`

## Response/Error Chung

- Validation lỗi model: `400` với `{ statusCode, message, traceId }`.
- Authentication lỗi: `401` với `{ statusCode, message, traceId }`.
- Authorization lỗi: `403` với `{ statusCode, message, traceId }`.
- Lỗi nghiệp vụ nên dùng `ApiException` và đi qua `ExceptionMiddleware`.

## Auth APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| POST | `/api/auth/login` | Public | Đăng nhập bằng email/password, trả access token, expiresAt, requiresPasswordChange, user. |
| POST | `/api/auth/change-password` | JWT | Đổi mật khẩu người dùng hiện tại. |

### Dự kiến/cần bổ sung

- `POST /api/auth/refresh-token`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

## Users APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/accounts` | Admin/SuperAdmin/CampusAdmin | Endpoint mẫu, chưa phải account management đầy đủ. |

### Dự kiến/cần bổ sung

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `PATCH /api/users/{id}/status`
- `POST /api/users/{id}/reset-password`
- `GET /api/users/me`

## Organizations APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/organizations` | JWT | Danh sách tổ chức theo phạm vi user. |
| GET | `/api/organizations/tree` | JWT | Cây tổ chức theo phạm vi user. |
| GET | `/api/organizations/{id}` | JWT | Chi tiết tổ chức. |
| POST | `/api/organizations` | SuperAdmin | Tạo tổ chức. |
| PUT | `/api/organizations/{id}` | SuperAdmin | Cập nhật tổ chức. |
| DELETE | `/api/organizations/{id}` | SuperAdmin | Xóa mềm tổ chức. |
| DELETE | `/api/organizations/{id}/hard-delete` | SuperAdmin | Xóa cứng nếu không có dữ liệu liên quan. |
| GET | `/api/organizations/{id}/subtree` | JWT | Lấy cây con của tổ chức. |

### Dự kiến/cần bổ sung

- `PATCH /api/organizations/{id}/restore`
- `GET /api/organizations/{id}/users`
- `GET /api/organizations/{id}/courses`

## Courses APIs

### Đã có

Chưa thấy controller course trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/courses`
- `GET /api/courses/{id}`
- `POST /api/courses`
- `PUT /api/courses/{id}`
- `PATCH /api/courses/{id}/publish`
- `GET /api/courses/{id}/chapters`
- `GET /api/students/me/courses`

## Lessons APIs

### Đã có

Chưa thấy controller lesson trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/courses/{courseId}/lessons`
- `GET /api/lessons/{id}`
- `POST /api/courses/{courseId}/lessons`
- `PUT /api/lessons/{id}`
- `PATCH /api/lessons/{id}/progress`
- `POST /api/lessons/{id}/comments`

## Assignments APIs

### Đã có

Chưa thấy controller assignment trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/assignments`
- `GET /api/assignments/{id}`
- `POST /api/courses/{courseId}/assignments`
- `PUT /api/assignments/{id}`
- `PATCH /api/assignments/{id}/publish`
- `PATCH /api/assignments/{id}/close`

## Submissions APIs

### Đã có

Chưa thấy controller submission trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/assignments/{assignmentId}/submissions`
- `GET /api/submissions/{id}`
- `POST /api/assignments/{assignmentId}/submissions`
- `PUT /api/submissions/{id}/grade`
- `GET /api/students/me/submissions`

## Exams/Quiz APIs

### Đã có

Chưa thấy controller exams/quiz trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/exams`
- `GET /api/exams/{id}`
- `POST /api/exams`
- `PUT /api/exams/{id}`
- `GET /api/exams/{id}/questions`
- `POST /api/exams/{id}/start`
- `POST /api/exam-sessions/{sessionId}/submit`
- `GET /api/exam-sessions/{sessionId}/result`

## Attendance APIs

### Đã có

Chưa thấy controller attendance trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/attendance`
- `GET /api/students/me/attendance`
- `POST /api/sessions/{sessionId}/attendance`
- `PUT /api/attendance/{id}`
- `POST /api/attendance-unlock-requests`
- `PUT /api/attendance-unlock-requests/{id}/approve`

## Grades APIs

### Đã có

Chưa thấy controller grades trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/grades`
- `GET /api/students/me/grades`
- `PUT /api/grades/{id}`
- `POST /api/grade-change-requests`
- `PUT /api/grade-change-requests/{id}/approve`
- `GET /api/courses/{courseId}/gradebook`

## Notifications APIs

### Đã có

Chưa thấy controller notification trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/notifications`
- `GET /api/notifications/{id}`
- `POST /api/notifications`
- `PATCH /api/notifications/{id}/read`
- `GET /api/notification-preferences`
- `PUT /api/notification-preferences`

## Reports APIs

### Đã có

Chưa thấy controller reports trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/reports/dashboard`
- `POST /api/reports/export`
- `GET /api/reports/attendance-risk`
- `GET /api/reports/failure-risk`
- `GET /api/reports/classroom-usage`
- `GET /api/audit-logs`

## AI APIs

### Đã có

Chưa thấy controller AI trong repo hiện tại.

### Dự kiến/cần bổ sung

- `POST /api/ai/lessons/{lessonId}/summarize`
- `POST /api/ai/submissions/{submissionId}/plagiarism-check`
- `POST /api/ai/submissions/{submissionId}/grade-suggestion`
- `GET /api/ai/risk/attendance`
- `GET /api/ai/risk/failure`
- `POST /api/ai/evaluations/sentiment`

## Quy Tắc Cập Nhật Contract

- Thêm controller mới thì cập nhật file này trong cùng PR/task.
- Nếu endpoint chỉ là ý tưởng, để ở mục `Dự kiến/cần bổ sung`.
- Không sửa frontend gọi endpoint dự kiến nếu backend chưa có mock/server contract rõ ràng.
