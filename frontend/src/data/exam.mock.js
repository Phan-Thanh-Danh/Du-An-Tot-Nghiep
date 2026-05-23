/**
 * exam.mock.js – Mock data cho luồng thi trực tuyến sinh viên
 * FE-only, không gọi API thật.
 */

// ─── Danh sách ca thi ──────────────────────────────────────────────────────
export const mockExams = [
  {
    id: 'exam-toan-001',
    title: 'Thi giữa kỳ Toán rời rạc',
    subject: 'Toán rời rạc',
    subjectCode: 'TOAN201',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-22T07:30:00',
    closeAt: '2026-05-22T09:30:00',
    durationMinutes: 90,
    totalQuestions: 30,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'upcoming',
    statusLabel: 'Sắp diễn ra',
    room: 'P.201',
    teacher: 'TS. Lê Văn Minh',
    attempts: 0,
    maxAttempts: 1,
    riskScore: 18,
  },
  {
    id: 'exam-ctdl-002',
    title: 'Quiz Cấu trúc dữ liệu – Cây & Đồ thị',
    subject: 'Cấu trúc dữ liệu & Giải thuật',
    subjectCode: 'CTDL101',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-21T10:00:00',
    closeAt: '2026-05-21T23:59:00',
    durationMinutes: 15,
    totalQuestions: 12,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'open',
    statusLabel: 'Đang mở',
    room: null,
    teacher: 'ThS. Nguyễn Minh Khoa',
    attempts: 0,
    maxAttempts: 1,
    riskScore: 24,
  },
  {
    id: 'exam-ltw-003',
    title: 'Kiểm tra giữa kỳ Lập trình Web',
    subject: 'Lập trình Web',
    subjectCode: 'LTW301',
    classCode: 'CNTT-K28B',
    openAt: '2026-05-10T08:00:00',
    closeAt: '2026-05-10T10:00:00',
    durationMinutes: 120,
    totalQuestions: 25,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'completed',
    statusLabel: 'Đã hoàn thành',
    room: 'Lab 3',
    teacher: 'TS. Trần Thị Lan',
    attempts: 1,
    maxAttempts: 1,
    riskScore: 12,
    resultId: 'result-ltw-003',
  },
  {
    id: 'exam-mmt-004',
    title: 'Thi cuối kỳ Mạng máy tính',
    subject: 'Mạng máy tính',
    subjectCode: 'MMT401',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-18T13:00:00',
    closeAt: '2026-05-18T15:00:00',
    durationMinutes: 120,
    totalQuestions: 40,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'awaiting_result',
    statusLabel: 'Chờ công bố điểm',
    room: 'Hội trường A',
    teacher: 'PGS. Hoàng Đức Bình',
    attempts: 1,
    maxAttempts: 1,
    riskScore: 5,
  },
]

// ─── Chi tiết bài thi ──────────────────────────────────────────────────────
export const mockExamDetail = {
  id: 'exam-ctdl-002',
  title: 'Quiz Cấu trúc dữ liệu – Cây & Đồ thị',
  subject: 'Cấu trúc dữ liệu & Giải thuật',
  subjectCode: 'CTDL101',
  classCode: 'CNTT-K28A',
  teacher: 'ThS. Nguyễn Minh Khoa',
  openAt: '2026-05-21T10:00:00',
  closeAt: '2026-05-21T23:59:00',
  durationMinutes: 15,
  totalQuestions: 12,
  totalPoints: 10,
  examType: 'multiple_choice',
  examTypeLabel: 'Trắc nghiệm',
  maxAttempts: 1,
  status: 'open',
  rules: [
    'Bạn chỉ được phép làm bài một lần duy nhất.',
    'Không được thoát khỏi chế độ toàn màn hình trong khi làm bài.',
    'Không được chuyển tab hoặc cửa sổ trình duyệt.',
    'Mọi hành động copy/paste sẽ bị ghi nhận là vi phạm.',
    'Bài thi sẽ tự động nộp khi hết thời gian.',
  ],
}

// ─── Pre-flight checks ─────────────────────────────────────────────────────
export const mockPreflightChecks = [
  { id: 'browser', label: 'Trình duyệt hỗ trợ', description: 'Chrome/Edge ≥ 110', status: 'pass', detail: 'Chrome 124 – Đạt yêu cầu', icon: 'Globe' },
  { id: 'fullscreen', label: 'Chế độ toàn màn hình', description: 'Fullscreen API khả dụng', status: 'pass', detail: 'Fullscreen API khả dụng', icon: 'Maximize' },
  { id: 'screen_share', label: 'Chia sẻ màn hình', description: 'Quyền Screen Capture', status: 'warning', detail: 'Chưa cấp quyền – Cần cho phép trước khi vào thi', icon: 'MonitorCheck' },
  { id: 'network', label: 'Kết nối mạng', description: 'Độ trễ < 200ms', status: 'pass', detail: 'Độ trễ: 42ms – Ổn định', icon: 'Wifi' },
  { id: 'extensions', label: 'Tiện ích bị cấm', description: 'Không có extension gian lận', status: 'pass', detail: 'Không phát hiện tiện ích bị cấm', icon: 'Puzzle' },
  { id: 'vm', label: 'Máy ảo / Remote Desktop', description: 'Không phát hiện VM', status: 'pass', detail: 'Không phát hiện VM hoặc RDP', icon: 'Server' },
  { id: 'ai_tools', label: 'Công cụ AI / Dịch thuật', description: 'Không có tab AI', status: 'pass', detail: 'Không phát hiện công cụ bị cấm', icon: 'Bot' },
]

export const mockPreflightChecksHighRisk = [
  { id: 'browser', label: 'Trình duyệt hỗ trợ', description: 'Chrome/Edge ≥ 110', status: 'pass', detail: 'Chrome 124 – Đạt yêu cầu', icon: 'Globe' },
  { id: 'fullscreen', label: 'Chế độ toàn màn hình', description: 'Fullscreen API khả dụng', status: 'fail', detail: 'Không thể kích hoạt fullscreen – Kiểm tra cài đặt', icon: 'Maximize' },
  { id: 'screen_share', label: 'Chia sẻ màn hình', description: 'Quyền Screen Capture', status: 'fail', detail: 'Quyền bị từ chối – Vui lòng cấp quyền để tiếp tục', icon: 'MonitorCheck' },
  { id: 'network', label: 'Kết nối mạng', description: 'Độ trễ < 200ms', status: 'warning', detail: 'Độ trễ: 185ms – Không ổn định', icon: 'Wifi' },
  { id: 'extensions', label: 'Tiện ích bị cấm', description: 'Không có extension gian lận', status: 'fail', detail: 'Phát hiện: "AI Answer Helper" – Hãy gỡ cài đặt', icon: 'Puzzle' },
  { id: 'vm', label: 'Máy ảo / Remote Desktop', description: 'Không phát hiện VM', status: 'pass', detail: 'Không phát hiện VM hoặc RDP', icon: 'Server' },
  { id: 'ai_tools', label: 'Công cụ AI / Dịch thuật', description: 'Không có tab AI', status: 'warning', detail: 'Phát hiện tab ChatGPT – Hãy đóng trước khi thi', icon: 'Bot' },
]

// ─── Câu hỏi ──────────────────────────────────────────────────────────────
export const mockQuestions = [
  {
    id: 'q1', order: 1, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Cây nhị phân tìm kiếm (BST) có tính chất nào sau đây?',
    choices: [
      { id: 'a', label: 'A', text: 'Nút trái luôn lớn hơn nút cha' },
      { id: 'b', label: 'B', text: 'Nút phải luôn nhỏ hơn nút cha' },
      { id: 'c', label: 'C', text: 'Nút trái nhỏ hơn nút cha, nút phải lớn hơn nút cha' },
      { id: 'd', label: 'D', text: 'Tất cả nút có cùng giá trị' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q2', order: 2, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Độ phức tạp thời gian trung bình của phép tìm kiếm trong BST cân bằng là?',
    choices: [
      { id: 'a', label: 'A', text: 'O(1)' },
      { id: 'b', label: 'B', text: 'O(log n)' },
      { id: 'c', label: 'C', text: 'O(n)' },
      { id: 'd', label: 'D', text: 'O(n log n)' },
    ],
    answer: 'b', flagged: false,
  },
  {
    id: 'q3', order: 3, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Trong duyệt cây theo thứ tự Inorder (LNR), thứ tự duyệt là?',
    choices: [
      { id: 'a', label: 'A', text: 'Gốc → Trái → Phải' },
      { id: 'b', label: 'B', text: 'Trái → Gốc → Phải' },
      { id: 'c', label: 'C', text: 'Trái → Phải → Gốc' },
      { id: 'd', label: 'D', text: 'Phải → Gốc → Trái' },
    ],
    answer: 'b', flagged: true,
  },
  {
    id: 'q4', order: 4, type: 'multiple_choice', typeLabel: 'Chọn nhiều đáp án', points: 1.5,
    content: 'Những cấu trúc dữ liệu nào có thể biểu diễn đồ thị? (Chọn tất cả đáp án đúng)',
    choices: [
      { id: 'a', label: 'A', text: 'Ma trận kề (Adjacency Matrix)' },
      { id: 'b', label: 'B', text: 'Danh sách kề (Adjacency List)' },
      { id: 'c', label: 'C', text: 'Ngăn xếp (Stack)' },
      { id: 'd', label: 'D', text: 'Danh sách cạnh (Edge List)' },
    ],
    answer: [], flagged: false,
  },
  {
    id: 'q5', order: 5, type: 'multiple_choice', typeLabel: 'Chọn nhiều đáp án', points: 1.5,
    content: 'Thuật toán nào dùng để duyệt đồ thị? (Chọn tất cả đáp án đúng)',
    choices: [
      { id: 'a', label: 'A', text: 'BFS (Breadth-First Search)' },
      { id: 'b', label: 'B', text: 'DFS (Depth-First Search)' },
      { id: 'c', label: 'C', text: 'Binary Search' },
      { id: 'd', label: 'D', text: 'Dijkstra' },
    ],
    answer: ['a', 'b'], flagged: true,
  },
  {
    id: 'q6', order: 6, type: 'short_answer', typeLabel: 'Trả lời ngắn', points: 1.0,
    content: 'Viết tên thuật toán sắp xếp có độ phức tạp trung bình O(n log n) và được cài đặt bằng đệ quy phân chia đôi mảng.',
    answer: '', flagged: false,
  },
  {
    id: 'q7', order: 7, type: 'short_answer', typeLabel: 'Trả lời ngắn', points: 1.0,
    content: 'Cho cây nhị phân với 7 nút, chiều cao tối thiểu của cây là bao nhiêu?',
    answer: '', flagged: false,
  },
  {
    id: 'q8', order: 8, type: 'essay', typeLabel: 'Tự luận', points: 3.0,
    content: 'Phân tích và so sánh BFS và Dijkstra:\n1. Trình bày ý tưởng chính.\n2. Nêu độ phức tạp thời gian và không gian.\n3. Khi nào dùng BFS, khi nào dùng Dijkstra?\n4. Cho ví dụ minh họa.',
    answer: '', flagged: false,
  },
  {
    id: 'q9', order: 9, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Cây AVL là cây BST tự cân bằng. Điều kiện cân bằng của cây AVL là?',
    choices: [
      { id: 'a', label: 'A', text: 'Chênh lệch chiều cao cây con ≤ 2' },
      { id: 'b', label: 'B', text: 'Chênh lệch chiều cao cây con ≤ 1' },
      { id: 'c', label: 'C', text: 'Tất cả lá cùng chiều cao' },
      { id: 'd', label: 'D', text: 'Số nút cây con trái và phải bằng nhau' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q10', order: 10, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Thuật toán Prim và Kruskal đều dùng để tìm?',
    choices: [
      { id: 'a', label: 'A', text: 'Đường đi ngắn nhất' },
      { id: 'b', label: 'B', text: 'Cây khung nhỏ nhất (Minimum Spanning Tree)' },
      { id: 'c', label: 'C', text: 'Chu trình Euler' },
      { id: 'd', label: 'D', text: 'Thành phần liên thông mạnh' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q11', order: 11, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Trong đồ thị có hướng, sắp xếp tô-pô áp dụng được khi?',
    choices: [
      { id: 'a', label: 'A', text: 'Đồ thị có chu trình' },
      { id: 'b', label: 'B', text: 'Đồ thị không có chu trình (DAG)' },
      { id: 'c', label: 'C', text: 'Đồ thị vô hướng' },
      { id: 'd', label: 'D', text: 'Đồ thị đầy đủ' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q12', order: 12, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Hash table giải quyết xung đột bằng phương pháp "Chaining" bằng cách?',
    choices: [
      { id: 'a', label: 'A', text: 'Tìm ô kế tiếp còn trống trong bảng' },
      { id: 'b', label: 'B', text: 'Sử dụng danh sách liên kết tại mỗi ô hash' },
      { id: 'c', label: 'C', text: 'Tăng kích thước bảng khi xảy ra xung đột' },
      { id: 'd', label: 'D', text: 'Xóa phần tử cũ và chèn phần tử mới' },
    ],
    answer: null, flagged: false,
  },
]

// ─── Violation logs ────────────────────────────────────────────────────────
export const mockViolationLogs = [
  { id: 'v1', type: 'tab_switch', label: 'Chuyển tab trình duyệt', description: 'Phát hiện sinh viên chuyển sang tab khác', severity: 'high', severityLabel: 'Cao', occurredAt: '10:04:23', count: 1 },
  { id: 'v2', type: 'copy_paste', label: 'Sao chép / Dán nội dung', description: 'Phát hiện hành động Ctrl+C hoặc Ctrl+V', severity: 'medium', severityLabel: 'Trung bình', occurredAt: '10:06:11', count: 2 },
  { id: 'v3', type: 'fullscreen_exit', label: 'Thoát toàn màn hình', description: 'Sinh viên nhấn Escape hoặc chuyển cửa sổ', severity: 'high', severityLabel: 'Cao', occurredAt: '10:07:45', count: 1 },
]

// ─── Kết quả bài thi ──────────────────────────────────────────────────────
export const mockExamResult = {
  id: 'result-ltw-003',
  examId: 'exam-ltw-003',
  examTitle: 'Kiểm tra giữa kỳ Lập trình Web',
  subject: 'Lập trình Web',
  subjectCode: 'LTW301',
  studentId: 'SV001234',
  studentName: 'Nguyễn Văn An',
  startedAt: '2026-05-10T08:00:00',
  submittedAt: '2026-05-10T09:42:00',
  durationActual: 102,
  durationAllowed: 120,
  totalQuestions: 25,
  answeredQuestions: 25,
  correctAnswers: 22,
  incorrectAnswers: 2,
  skippedAnswers: 1,
  score: 8.8,
  maxScore: 10,
  grade: 'A',
  status: 'graded',
  statusLabel: 'Đã có điểm',
  essayStatus: 'graded',
  teacherComment: 'Bài làm tốt, nắm vững kiến thức HTML/CSS và JavaScript cơ bản. Phần REST API cần bổ sung thêm hiểu biết về status code.',
  aiSuggestion: 'Sinh viên đạt trên 88% câu trắc nghiệm. Cần ôn thêm HTTP Methods và RESTful API conventions cho bài cuối kỳ.',
  violations: [
    { type: 'copy_paste', label: 'Sao chép / Dán', severity: 'low', severityLabel: 'Thấp', occurredAt: '08:23:10', count: 1 },
  ],
  scoreBreakdown: [
    { section: 'HTML & CSS cơ bản', correct: 8, total: 8, points: 3.2 },
    { section: 'JavaScript & DOM', correct: 7, total: 8, points: 2.8 },
    { section: 'REST API & Fetch', correct: 5, total: 7, points: 2.0 },
    { section: 'Tự luận – Thiết kế giao diện', score: 0.8, maxScore: 2.0, status: 'graded' },
  ],
}
