// ============================================================
// Shared Mock Data for Parent Portal (Học tập & Học sinh & Tài chính & Thông báo & Cá nhân)
// ============================================================

export const parentProfile = {
  name: 'Phạm Thị Mẹ Học Sinh',
  dob: '15/10/1980',
  cccd: '001180009876',
  phone: '0987654321',
  email: 'parent@mock.local',
  permanentAddress: 'Số 12, Đường Láng, Đống Đa, Hà Nội',
  temporaryAddress: 'Chung cư EcoGreen, Nguyễn Xiển, Thanh Xuân, Hà Nội'
};

export const childrenData = [
  {
    id: 1,
    name: 'Nguyễn Minh Quân',
    studentId: 'SV2024001',
    class: 'CNTT K26A',
    major: 'Kỹ thuật Phần mềm',
    avatarInitials: 'MQ',
    gpa: 8.4,
    gpaTrendText: '+0.3 so với kỳ trước',
    attendanceRate: 96,
    absences: 2,
    absencesDetail: '1 phép, 1 không phép',
    lateCount: 2,
    presentCount: 48,
    
    // Xu hướng điểm số các tháng gần đây (để vẽ biểu đồ)
    gpaTrend: [
      { month: 'T12/2025', gpa: 7.9 },
      { month: 'T01/2026', gpa: 8.0 },
      { month: 'T02/2026', gpa: 8.1 },
      { month: 'T03/2026', gpa: 8.2 },
      { month: 'T04/2026', gpa: 8.4 }
    ],

    // Danh sách học kỳ
    semesters: [
      'Học kỳ 1 - 2025-2026',
      'Học kỳ 2 - 2024-2025'
    ],

    // Bảng điểm chi tiết
    grades: {
      'Học kỳ 1 - 2025-2026': [
        { code: 'SWE302', name: 'Cấu trúc dữ liệu & Giải thuật', attendance: 10, quiz: 8.5, midterm: 8.0, final: 8.5, average: 8.4, status: 'Passed', isPublished: true },
        { code: 'WEB204', name: 'Lập trình Web nâng cao', attendance: 10, quiz: 9.0, midterm: 8.5, final: 8.0, average: 8.3, status: 'Passed', isPublished: true },
        { code: 'DBA301', name: 'Cơ sở dữ liệu phân tán', attendance: 9.0, quiz: 7.5, midterm: 8.0, final: 7.0, average: 7.5, status: 'Passed', isPublished: true },
        { code: 'MAT102', name: 'Toán cao cấp', attendance: 7.0, quiz: 4.5, midterm: 5.0, final: 5.5, average: 5.3, status: 'Passed', isPublished: true },
        { code: 'PRN211', name: 'Lập trình C#', attendance: 10, quiz: 8.0, midterm: 4.5, final: null, average: null, status: 'In Progress', isPublished: false } // Chưa duyệt công bố
      ],
      'Học kỳ 2 - 2024-2025': [
        { code: 'PRF192', name: 'Nhập môn lập trình C', attendance: 10, quiz: 8.0, midterm: 7.5, final: 8.0, average: 7.9, status: 'Passed', isPublished: true },
        { code: 'MAE101', name: 'Đại số tuyến tính', attendance: 9.0, quiz: 6.0, midterm: 5.5, final: 4.0, average: 4.8, status: 'Failed', isPublished: true },
        { code: 'ENG111', name: 'Tiếng Anh sơ cấp', attendance: 10, quiz: 9.5, midterm: 9.0, final: 8.5, average: 8.9, status: 'Passed', isPublished: true }
      ]
    },

    // Điểm danh chi tiết
    attendanceHistory: [
      { date: '10/06/2026', subject: 'Toán cao cấp', shift: 'Ca 2 (09:30 - 11:30)', status: 'Vắng không phép', note: 'Sinh viên vắng không lý do' },
      { date: '09/06/2026', subject: 'Cấu trúc dữ liệu & Giải thuật', shift: 'Ca 1 (07:30 - 09:30)', status: 'Có mặt', note: '' },
      { date: '08/06/2026', subject: 'Lập trình Web nâng cao', shift: 'Ca 3 (12:30 - 14:30)', status: 'Đi muộn', note: 'Muộn 15 phút' },
      { date: '07/06/2026', subject: 'Cơ sở dữ liệu phân tán', shift: 'Ca 2 (09:30 - 11:30)', status: 'Có mặt', note: '' },
      { date: '05/06/2026', subject: 'Toán cao cấp', shift: 'Ca 2 (09:30 - 11:30)', status: 'Vắng có phép', note: 'Có đơn xin nghỉ phép ốm' },
      { date: '04/06/2026', subject: 'Lập trình Web nâng cao', shift: 'Ca 3 (12:30 - 14:30)', status: 'Có mặt', note: '' },
      { date: '03/06/2026', subject: 'Cấu trúc dữ liệu & Giải thuật', shift: 'Ca 1 (07:30 - 09:30)', status: 'Có mặt', note: '' },
      { date: '02/06/2026', subject: 'Cơ sở dữ liệu phân tán', shift: 'Ca 2 (09:30 - 11:30)', status: 'Có mặt', note: '' }
    ],

    // Thời khóa biểu tuần này
    schedule: [
      { day: 'Thứ 2', date: '08/06/2026', subject: 'Lập trình Web nâng cao', shift: 'Ca 3 (12:30 - 14:30)', room: 'Phòng 402 - Nhà Alpha', teacher: 'ThS. Nguyễn Văn A', isExam: false },
      { day: 'Thứ 3', date: '09/06/2026', subject: 'Cấu trúc dữ liệu & Giải thuật', shift: 'Ca 1 (07:30 - 09:30)', room: 'Phòng 201 - Nhà Beta', teacher: 'TS. Trần Thị B', isExam: false },
      { day: 'Thứ 4', date: '10/06/2026', subject: 'Toán cao cấp (Kiểm Tra Giữa Kỳ)', shift: 'Ca 2 (09:30 - 11:30)', room: 'Phòng 105 - Nhà Gamma', teacher: 'ThS. Lê Văn C', isExam: true },
      { day: 'Thứ 5', date: '11/06/2026', subject: 'Cơ sở dữ liệu phân tán', shift: 'Ca 2 (09:30 - 11:30)', room: 'Học Online (Google Meet)', teacher: 'TS. Phạm Văn D', isExam: false, link: 'https://meet.google.com/abc-defg-hij' },
      { day: 'Thứ 6', date: '12/06/2026', subject: 'Lập trình C#', shift: 'Ca 4 (14:45 - 16:45)', room: 'Phòng Lab 3 - Nhà Epsilon', teacher: 'ThS. Đỗ Thị E', isExam: false },
      { day: 'Thứ 7', date: '13/06/2026', subject: 'Không có lịch học', shift: '-', room: '-', teacher: '-', isExam: false },
      { day: 'Chủ Nhật', date: '14/06/2026', subject: 'Không có lịch học', shift: '-', room: '-', teacher: '-', isExam: false }
    ],

    // Cảnh báo học tập
    warnings: [
      { id: 1, type: 'danger', subject: 'Toán cao cấp', reason: 'Vắng học quá 20% số buổi quy định (Đã vắng 3 buổi)', advice: 'Học sinh cần làm bản cam kết và liên hệ ngay với Giảng viên chủ nhiệm để nộp bù bài tập.', date: '10/06/2026', confirmed: false },
      { id: 2, type: 'warning', subject: 'Lập trình C#', reason: 'Điểm kiểm tra định kỳ lý thuyết dưới trung bình: 4.5', advice: 'Cần tăng cường ôn tập thực hành nhóm và tự học trên LMS.', date: '07/06/2026', confirmed: false }
    ],

    // Dữ liệu tài chính
    totalTuition: 18450000,
    paidTuition: 8000000,
    balanceTuition: 10450000,
    deadlineTuition: '05/06/2026',
    isOverdue: true,

    feeItems: [
      { name: 'Học phí kì 1 - Lập trình Web nâng cao', amount: 3500000, status: 'Đã nộp' },
      { name: 'Học phí kì 1 - Cấu trúc dữ liệu & Giải thuật', amount: 3500000, status: 'Đã nộp' },
      { name: 'Học phí kì 1 - Cơ sở dữ liệu phân tán', amount: 3500000, status: 'Chưa nộp' },
      { name: 'Học phí kì 1 - Toán cao cấp', amount: 3500000, status: 'Chưa nộp' },
      { name: 'Học phí kì 1 - Lập trình C#', amount: 3500000, status: 'Chưa nộp' },
      { name: 'Phí cơ sở vật chất học kỳ', amount: 1000000, status: 'Đã nộp' },
      { name: 'Bảo hiểm y tế tự nguyện (1 năm)', amount: 450000, status: 'Chưa nộp' }
    ],

    transactions: [
      { code: 'TX890123', date: '15/05/2026', amount: 8000000, method: 'Ví điện tử Momo', status: 'Thành công' },
      { code: 'TX890001', date: '05/05/2026', amount: 3500000, method: 'Chuyển khoản VietQR', status: 'Đang xử lý' }
    ],

    invoices: [
      { id: 'INV-2026-001', transactionCode: 'TX890123', date: '15/05/2026', amount: 8000000, status: 'Đã phát hành' }
    ],

    // Dữ liệu thông báo
    systemNotifications: [
      { id: 101, title: 'Cảnh báo vắng học không phép', content: 'Học sinh Nguyễn Minh Quân đã vắng mặt không phép tiết 2 môn Toán cao cấp ngày 10/06/2026.', date: '10/06/2026', read: false, type: 'danger' },
      { id: 102, title: 'Cảnh báo kết quả kiểm tra', content: 'Học sinh Nguyễn Minh Quân đạt điểm 4.5 dưới trung bình trong bài kiểm tra định kỳ lý thuyết môn Lập trình C# ngày 07/06/2026.', date: '07/06/2026', read: false, type: 'warning' },
      { id: 103, title: 'Nhắc nhở đóng học phí trễ hạn', content: 'Học phí kì 1 năm học 2025-2026 của học sinh Nguyễn Minh Quân đã quá hạn nộp ngày 05/06/2026. Số tiền chưa nộp là 10.450.000đ. Vui lòng thanh toán trực tuyến.', date: '05/06/2026', read: false, type: 'danger' }
    ],

    schoolNotices: [
      { id: 201, title: 'Thông báo lịch nghỉ lễ tổng kết năm học', content: 'Ban Giám hiệu nhà trường trân trọng thông báo lịch nghỉ lễ tổng kết năm học của toàn thể học sinh sinh viên bắt đầu từ ngày 15/06/2026 đến hết ngày 20/06/2026.', date: '08/06/2026', read: false, category: 'Nhà trường' },
      { id: 202, title: 'Kế hoạch tổ chức Hội thao mùa hè 2026', content: 'Hội thao sinh viên mùa hè năm học 2025-2026 dự kiến diễn ra từ ngày 25/06/2026 tại Sân vận động cơ sở chính. Kính mời phụ huynh khuyến khích con tham gia đăng ký.', date: '04/06/2026', read: true, category: 'Nhà trường' }
    ],

    teacherMessages: [
      { id: 301, sender: 'TS. Trần Thị B (Giảng viên cấu trúc dữ liệu)', content: 'Chào phụ huynh, em Quân tuần này học tập rất chăm chú, làm bài lab đạt điểm tối đa. Rất mong gia đình tiếp tục khích lệ em phát huy.', date: '09/06/2026', read: false, category: 'Giảng viên' },
      { id: 302, sender: 'ThS. Lê Văn C (Giảng viên toán cao cấp)', content: 'Em Quân vắng học không phép 1 buổi tuần này và điểm danh muộn 1 buổi. Đề nghị phụ huynh nhắc nhở em chú ý giờ giấc lên lớp hơn.', date: '05/06/2026', read: true, category: 'Giảng viên' }
    ],

    // ── QUYỀN TRUY CẬP ĐƯỢC CẤP (NEW) ────────────────────
    accessRights: [
      { name: 'Xem bảng điểm & Kết quả học tập', code: 'GRADES', active: true, desc: 'Cho phép xem điểm chuyên cần, quiz, thi giữa kỳ, cuối kỳ.' },
      { name: 'Xem điểm danh & Lịch sử chuyên cần', code: 'ATTENDANCE', active: true, desc: 'Cho phép theo dõi tỷ lệ đi học, xin phép nghỉ và các buổi vắng.' },
      { name: 'Xem thông tin học phí & Công nợ', code: 'FINANCE', active: true, desc: 'Cho phép theo dõi học phí cần đóng, hóa đơn và lịch sử đóng tiền.' },
      { name: 'Xem chi tiết bài làm quiz / đề thi trên LMS', code: 'QUIZ_DETAILS', active: false, desc: 'Cho phép xem từng câu trả lời đúng/sai của con em trong các bài kiểm tra trực tuyến.' }
    ]
  },
  {
    id: 2,
    name: 'Nguyễn Khánh Linh',
    studentId: 'SV2024045',
    class: 'CNTT K26B',
    major: 'An toàn thông tin',
    avatarInitials: 'KL',
    gpa: 9.1,
    gpaTrendText: '+0.1 so với kỳ trước',
    attendanceRate: 100,
    absences: 0,
    absencesDetail: 'Đi đầy đủ',
    lateCount: 0,
    presentCount: 36,

    gpaTrend: [
      { month: 'T12/2025', gpa: 8.8 },
      { month: 'T01/2026', gpa: 8.9 },
      { month: 'T02/2026', gpa: 9.0 },
      { month: 'T03/2026', gpa: 9.0 },
      { month: 'T04/2026', gpa: 9.1 }
    ],

    semesters: [
      'Học kỳ 1 - 2025-2026',
      'Học kỳ 2 - 2024-2025'
    ],

    grades: {
      'Học kỳ 1 - 2025-2026': [
        { code: 'MOB306', name: 'Phát triển ứng dụng di động', attendance: 10, quiz: 9.5, midterm: 9.0, final: 9.5, average: 9.4, status: 'Passed', isPublished: true },
        { code: 'NET202', name: 'An toàn thông tin mạng', attendance: 10, quiz: 8.5, midterm: 9.0, final: 8.5, average: 8.7, status: 'Passed', isPublished: true },
        { code: 'ENG201', name: 'Tiếng Anh chuyên ngành CNTT', attendance: 10, quiz: 9.0, midterm: 9.5, final: 9.0, average: 9.2, status: 'Passed', isPublished: true }
      ],
      'Học kỳ 2 - 2024-2025': [
        { code: 'COS101', name: 'Kiến trúc máy tính', attendance: 10, quiz: 8.5, midterm: 8.0, final: 8.5, average: 8.4, status: 'Passed', isPublished: true },
        { code: 'MAT101', name: 'Toán rời rạc', attendance: 10, quiz: 9.0, midterm: 9.0, final: 9.5, average: 9.3, status: 'Passed', isPublished: true }
      ]
    },

    attendanceHistory: [
      { date: '09/06/2026', subject: 'Phát triển ứng dụng di động', shift: 'Ca 2 (09:30 - 11:30)', status: 'Có mặt', note: '' },
      { date: '08/06/2026', subject: 'An toàn thông tin mạng', shift: 'Ca 1 (07:30 - 09:30)', status: 'Có mặt', note: '' },
      { date: '05/06/2026', subject: 'Tiếng Anh chuyên ngành CNTT', shift: 'Ca 4 (14:45 - 16:45)', status: 'Có mặt', note: '' },
      { date: '04/06/2026', subject: 'Phát triển ứng dụng di động', shift: 'Ca 2 (09:30 - 11:30)', status: 'Có mặt', note: '' },
      { date: '02/06/2026', subject: 'An toàn thông tin mạng', shift: 'Ca 1 (07:30 - 09:30)', status: 'Có mặt', note: '' }
    ],

    schedule: [
      { day: 'Thứ 2', date: '08/06/2026', subject: 'An toàn thông tin mạng', shift: 'Ca 1 (07:30 - 09:30)', room: 'Phòng 301 - Nhà Alpha', teacher: 'TS. Nguyễn Hoàng G', isExam: false },
      { day: 'Thứ 3', date: '09/06/2026', subject: 'Phát triển ứng dụng di động', shift: 'Ca 2 (09:30 - 11:30)', room: 'Phòng Lab 2 - Nhà Beta', teacher: 'ThS. Vũ Thị H', isExam: false },
      { day: 'Thứ 4', date: '10/06/2026', subject: 'Không có lịch học', shift: '-', room: '-', teacher: '-', isExam: false },
      { day: 'Thứ 5', date: '11/06/2026', subject: 'Tiếng Anh chuyên ngành CNTT (Thi Cuối Kỳ)', shift: 'Ca 4 (14:45 - 16:45)', room: 'Hội trường Lớn - Nhà Delta', teacher: 'TS. Nguyễn Hoàng G', isExam: true },
      { day: 'Thứ 6', date: '12/06/2026', subject: 'Phát triển ứng dụng di động', shift: 'Ca 2 (09:30 - 11:30)', room: 'Phòng Lab 2 - Nhà Beta', teacher: 'ThS. Vũ Thị H', isExam: false },
      { day: 'Thứ 7', date: '13/06/2026', subject: 'Không có lịch học', shift: '-', room: '-', teacher: '-', isExam: false },
      { day: 'Chủ Nhật', date: '14/06/2026', subject: 'Không có lịch học', shift: '-', room: '-', teacher: '-', isExam: false }
    ],

    warnings: [],

    // Dữ liệu tài chính
    totalTuition: 11500000,
    paidTuition: 11500000,
    balanceTuition: 0,
    deadlineTuition: '15/06/2026',
    isOverdue: false,

    feeItems: [
      { name: 'Học phí kì 1 - Phát triển ứng dụng di động', amount: 3500000, status: 'Đã nộp' },
      { name: 'Học phí kì 1 - An toàn thông tin mạng', amount: 3500000, status: 'Đã nộp' },
      { name: 'Học phí kì 1 - Tiếng Anh chuyên ngành CNTT', amount: 3500000, status: 'Đã nộp' },
      { name: 'Phí cơ sở vật chất học kỳ', amount: 1000000, status: 'Đã nộp' }
    ],

    transactions: [
      { code: 'TX900234', date: '25/05/2026', amount: 11500000, method: 'Chuyển khoản VietQR', status: 'Thành công' }
    ],

    invoices: [
      { id: 'INV-2026-045', transactionCode: 'TX900234', date: '25/05/2026', amount: 11500000, status: 'Đã phát hành' }
    ],

    // Dữ liệu thông báo
    systemNotifications: [
      { id: 104, title: 'Công bố điểm thi mới', content: 'Điểm thi môn Phát triển ứng dụng di động học kỳ 1 của học sinh Nguyễn Khánh Linh đã được công bố trên hệ thống: 9.4 (Average).', date: '09/06/2026', read: false, type: 'info' }
    ],

    schoolNotices: [
      { id: 203, title: 'Thông báo lịch nghỉ lễ tổng kết năm học', content: 'Ban Giám hiệu nhà trường trân trọng thông báo lịch nghỉ lễ tổng kết năm học của toàn thể học sinh sinh viên bắt đầu từ ngày 15/06/2026 đến hết ngày 20/06/2026.', date: '08/06/2026', read: false, category: 'Nhà trường' }
    ],

    teacherMessages: [
      { id: 303, sender: 'ThS. Vũ Thị H (Giảng viên ứng dụng di động)', content: 'Học sinh Khánh Linh hoàn thành xuất sắc đồ án cuối khóa di động và có tinh thần hỗ trợ các bạn trong lớp. Kết quả bài thi đạt điểm xuất sắc.', date: '08/06/2026', read: false, category: 'Giảng viên' }
    ],

    // ── QUYỀN TRUY CẬP ĐƯỢC CẤP (NEW) ────────────────────
    accessRights: [
      { name: 'Xem bảng điểm & Kết quả học tập', code: 'GRADES', active: true, desc: 'Cho phép xem điểm chuyên cần, quiz, thi giữa kỳ, cuối kỳ.' },
      { name: 'Xem điểm danh & Lịch sử chuyên cần', code: 'ATTENDANCE', active: true, desc: 'Cho phép theo dõi tỷ lệ đi học, xin phép nghỉ và các buổi vắng.' },
      { name: 'Xem thông tin học phí & Công nợ', code: 'FINANCE', active: true, desc: 'Cho phép theo dõi học phí cần đóng, hóa đơn và lịch sử đóng tiền.' },
      { name: 'Xem chi tiết bài làm quiz / đề thi trên LMS', code: 'QUIZ_DETAILS', active: true, desc: 'Cho phép xem từng câu trả lời đúng/sai của con em trong các bài kiểm tra trực tuyến.' }
    ]
  }
];

// Helper to get active child from local storage or URL query
export function getActiveChild(routeQueryId) {
  const storedId = localStorage.getItem('parent_active_student_id');
  const activeId = Number(routeQueryId) || Number(storedId) || 1;
  return childrenData.find(c => c.id === activeId) || childrenData[0];
}

// Helper to save active child ID
export function setActiveChildId(id) {
  localStorage.setItem('parent_active_student_id', id);
}
