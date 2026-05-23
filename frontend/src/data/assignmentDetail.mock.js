/**
 * assignmentDetail.mock.js
 * Mock data cho trang Chi tiết & Nộp bài (/student/assignments/:assignmentId)
 */

export const mockAssignment = {
  id: 'BT-CTDL-02',
  title: 'Bài tập thực hành: Cài đặt Stack bằng Linked List',
  courseCode: 'CTDL101',
  courseTitle: 'Cấu trúc dữ liệu & Giải thuật',
  teacher: 'TS. Nguyễn Minh Khoa',
  class: 'CNTT K28A',
  assignedAt: '10/05/2025',
  deadline: '2025-05-31T23:59:00',
  deadlineDisplay: '23:59 – 31/05/2025',
  status: 'submitted',
  statusLabel: 'Đã nộp',
  description: `Sinh viên cài đặt cấu trúc Stack sử dụng Linked List trong C++. Bài làm phải bao gồm:\n\n1. Định nghĩa Node và lớp Stack với các thao tác: push(), pop(), peek(), isEmpty(), size().\n2. Demo chương trình chuyển đổi biểu thức trung tố sang hậu tố (Infix → Postfix) sử dụng Stack.\n3. File báo cáo PDF giải thích thuật toán, độ phức tạp và kết quả kiểm thử.\n4. Đặt tên file theo format: MSSV_HoTen_BT02.zip`,
}

export const mockSubmissionRules = {
  allowedFormats: ['.zip', '.rar', '.pdf', '.docx'],
  maxSizeMB: 20,
  maxAttempts: 3,
  currentAttempt: 2,
  canSubmit: true,
  note: 'File .zip phải chứa toàn bộ source code và file báo cáo PDF. Không nộp file .exe.',
}

export const mockAttachments = [
  { id: 'a1', name: 'Đề bài BT02 – Stack.pdf', size: '234 KB', type: 'pdf' },
  { id: 'a2', name: 'Template báo cáo.docx', size: '45 KB', type: 'docx' },
  { id: 'a3', name: 'Test cases mẫu.zip', size: '12 KB', type: 'zip' },
]

export const mockSubmissions = [
  {
    id: 's1', attempt: 1,
    submittedAt: '15/05/2025 – 10:32', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'BT2101_NguyenVanA_BT02.zip', fileSize: '2.3 MB',
    note: 'Lần nộp đầu tiên.', isLatest: false,
  },
  {
    id: 's2', attempt: 2,
    submittedAt: '18/05/2025 – 22:14', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'BT2101_NguyenVanA_BT02_v2.zip', fileSize: '2.6 MB',
    note: 'Cập nhật phần báo cáo.', isLatest: true,
  },
]

export const mockPlagiarismResult = {
  status: 'safe',
  percentage: 12,
  checkedAt: '18/05/2025 – 22:20',
  detail: 'Tỷ lệ trùng lặp trong ngưỡng cho phép. Bài làm được xác nhận hợp lệ.',
}

export const mockFeedback = {
  graded: true,
  score: 8.5,
  maxScore: 10,
  gradedAt: '20/05/2025',
  gradedBy: 'TS. Nguyễn Minh Khoa',
  comment: 'Bài làm đúng yêu cầu, code rõ ràng và có chú thích tốt. Phần Infix→Postfix cần giải thích thuật toán rõ hơn trong báo cáo. Các test case cơ bản đều pass.',
  aiSuggestion: 'Bài làm đúng cấu trúc. Cần bổ sung phân tích độ phức tạp O(n) và xử lý trường hợp biên (stack rỗng, ký tự đặc biệt).',
  rubric: [
    { label: 'Cài đặt Stack (push/pop/peek)', score: 3.0, max: 3.0 },
    { label: 'Demo Infix → Postfix', score: 2.5, max: 3.0 },
    { label: 'Báo cáo & giải thích', score: 2.0, max: 3.0 },
    { label: 'Phong cách code & comment', score: 1.0, max: 1.0 },
  ],
}
