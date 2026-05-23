/**
 * courseDetail.mock.js
 * Mock data hoàn chỉnh cho trang Chi tiết khóa học (/student/courses/:courseId)
 * Chỉ dùng cho frontend demo, không nối API thật.
 */

export const mockCourse = {
  id: 'CTDL101',
  title: 'Cấu trúc dữ liệu & Giải thuật',
  code: 'CTDL101',
  teacher: 'TS. Nguyễn Minh Khoa',
  semester: 'HK2 2024–2025',
  credits: 3,
  coverGradient: 'from-blue-700 via-blue-600 to-cyan-500',
  description:
    'Môn học cung cấp kiến thức nền tảng về cấu trúc dữ liệu và các thuật toán sắp xếp, tìm kiếm, đệ quy phổ biến trong lập trình.',
}

export const mockStats = [
  { label: 'Tiến độ', value: '72', unit: '%', icon: 'Gauge', tone: 'blue', progress: 72, hint: '9/12 bài đã hoàn thành' },
  { label: 'Bài học', value: '9', unit: '/12', icon: 'BookOpenCheck', tone: 'green', progress: 75, hint: 'Đã hoàn thành 9 bài' },
  { label: 'Bài tập', value: '2', unit: 'mục', icon: 'ClipboardList', tone: 'orange', progress: 40, hint: '1 bài gần đến hạn' },
  { label: 'Tài liệu', value: '18', unit: 'file', icon: 'Files', tone: 'violet', progress: 60, hint: 'PDF, video, quiz' },
]

export const mockLessons = [
  {
    id: 'ch1',
    chapter: 'Chương 1',
    title: 'Ôn tập nền tảng lập trình',
    description: 'Ôn lại kiến thức C/C++, con trỏ, cấp phát động và các khái niệm OOP cơ bản.',
    status: 'completed',
    badge: 'Hoàn thành',
    tone: 'green',
    icon: 'CheckCircle2',
    meta: ['3 bài học', '1 quiz'],
    progress: 100,
    lessons: [
      { id: 'l1-1', title: 'Con trỏ và cấp phát động', duration: '18:24', status: 'completed' },
      { id: 'l1-2', title: 'Struct và Class cơ bản', duration: '22:10', status: 'completed' },
      { id: 'l1-3', title: 'Quiz ôn tập chương 1', duration: '10 câu', status: 'completed', type: 'quiz' },
    ],
  },
  {
    id: 'ch2',
    chapter: 'Chương 2',
    title: 'Cấu trúc dữ liệu tuyến tính',
    description: 'Danh sách liên kết, Stack, Queue và ứng dụng trong các bài toán thực tế.',
    status: 'active',
    badge: 'Đang học',
    tone: 'blue',
    icon: 'ListTree',
    meta: ['4 bài học', '2 tài liệu', '1 bài tập'],
    progress: 50,
    lessons: [
      { id: 'l2-1', title: 'Danh sách liên kết đơn', duration: '26:30', status: 'completed' },
      { id: 'l2-2', title: 'Stack và ứng dụng', duration: '20:45', status: 'active' },
      { id: 'l2-3', title: 'Queue và vòng lặp', duration: '24:00', status: 'locked' },
      { id: 'l2-4', title: 'Bài tập thực hành', duration: '–', status: 'locked', type: 'assignment' },
    ],
  },
  {
    id: 'ch3',
    chapter: 'Chương 3',
    title: 'Cây nhị phân và đồ thị',
    description: 'BST, cây AVL, duyệt cây, DFS, BFS và ứng dụng trong tìm đường.',
    status: 'locked',
    badge: 'Bị khóa',
    tone: 'slate',
    icon: 'Lock',
    meta: ['5 bài học', '2 bài tập'],
    progress: 0,
    lessons: [],
  },
  {
    id: 'ch4',
    chapter: 'Chương 4',
    title: 'Sắp xếp & Tìm kiếm',
    description: 'Bubble Sort, Quick Sort, Merge Sort, Binary Search và phân tích độ phức tạp.',
    status: 'upcoming',
    badge: 'Dự kiến',
    tone: 'amber',
    icon: 'GitBranch',
    meta: ['4 bài học', '1 quiz cuối kỳ'],
    progress: 0,
    lessons: [],
  },
]

export const mockCurrentLesson = {
  id: 'l2-2',
  chapterId: 'ch2',
  chapterTitle: 'Chương 2: Cấu trúc dữ liệu tuyến tính',
  title: 'Stack và ứng dụng trong lập trình',
  duration: '20:45',
  watchedSeconds: 743, // ~12 phút
  totalSeconds: 1245, // 20:45
  videoThumb: null, // null = show placeholder
  documentTitle: 'Slide bài giảng Chương 2.pdf',
  documentPages: 34,
  documentCurrentPage: 12,
}

export const mockQuizQuestions = [
  {
    id: 'q1',
    question: 'Cấu trúc dữ liệu nào hoạt động theo nguyên tắc LIFO (Last In First Out)?',
    options: ['Queue', 'Stack', 'Linked List', 'Tree'],
    correctIndex: 1,
  },
  {
    id: 'q2',
    question: 'Độ phức tạp thời gian trung bình của thuật toán Quick Sort là?',
    options: ['O(n)', 'O(n log n)', 'O(n²)', 'O(log n)'],
    correctIndex: 1,
  },
]

export const mockComments = [
  {
    id: 'c1',
    author: 'Trần Văn An',
    initials: 'TA',
    avatarColor: 'from-blue-500 to-cyan-400',
    content: 'Thầy ơi, phần push/pop trong Stack có thể giải thích thêm về trường hợp stack overflow không ạ?',
    time: '2 giờ trước',
    likes: 4,
    replies: [
      {
        id: 'r1',
        author: 'TS. Nguyễn Minh Khoa',
        initials: 'GV',
        avatarColor: 'from-violet-600 to-indigo-500',
        content: 'Stack overflow xảy ra khi đệ quy quá sâu hoặc cấp phát stack vượt giới hạn. Mình sẽ bổ sung slide vào buổi sau nhé.',
        time: '1 giờ trước',
        isTeacher: true,
      },
    ],
  },
  {
    id: 'c2',
    author: 'Lê Thị Bích',
    initials: 'LB',
    avatarColor: 'from-emerald-500 to-teal-400',
    content: 'Bài giảng rất dễ hiểu. Mình đã làm bài tập và pass được 3/4 test case rồi ạ!',
    time: '5 giờ trước',
    likes: 7,
    replies: [],
  },
]

export const mockTimeline = [
  {
    id: 't1',
    title: 'Tiếp tục bài đang học',
    description: 'Stack và ứng dụng – Chương 2',
    time: 'Đang học',
    tone: 'blue',
    icon: 'Play',
  },
  {
    id: 't2',
    title: 'Hoàn thành Quiz chương 2',
    description: 'Nộp trước 31/05/2025',
    time: 'Sắp đến hạn',
    tone: 'orange',
    icon: 'ClipboardList',
  },
  {
    id: 't3',
    title: 'Mở khóa Chương 3',
    description: 'Hoàn thành 100% chương 2 để tiếp tục',
    time: 'Điều kiện tiên quyết',
    tone: 'slate',
    icon: 'Lock',
  },
  {
    id: 't4',
    title: 'Thi cuối kỳ',
    description: 'Dự kiến tuần 16 – phòng thi A201',
    time: 'Dự kiến 20/06',
    tone: 'violet',
    icon: 'GraduationCap',
  },
]

export const mockAISummary = {
  points: [
    'Stack là cấu trúc LIFO – phần tử cuối vào sẽ được lấy ra đầu tiên.',
    'Hai thao tác cơ bản: push (thêm vào đỉnh) và pop (lấy khỏi đỉnh), đều O(1).',
    'Ứng dụng phổ biến: kiểm tra dấu ngoặc, đảo ngược chuỗi, thực thi đệ quy.',
    'Cần phân biệt stack cấp phát tĩnh (array-based) và cấp phát động (linked list-based).',
    'Bài tiếp theo sẽ học về Queue – cấu trúc FIFO bổ sung cho Stack.',
  ],
}
