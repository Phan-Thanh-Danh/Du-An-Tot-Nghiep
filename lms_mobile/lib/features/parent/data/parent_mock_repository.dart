import '../models/parent_models.dart';
import 'parent_repository.dart';
import '../../finance/models/tuition_payment.dart';
import '../../student/models/student_models.dart';

class ParentMockRepository implements ParentRepository {
  ParentProfile _profile = const ParentProfile(
    id: 'pr_01',
    fullName: 'Phan Văn Dũng',
    email: 'dung.pv@gmail.com',
    phone: '0903123456',
    avatarUrl:
        'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&q=80&w=200',
    address: '123 Đường Láng, Đống Đa, Hà Nội',
  );

  final List<Child> _children = [
    const Child(
      id: 'std_01',
      fullName: 'Phan Thanh Danh',
      code: 'SV20261102',
      classCode: 'K20-PM01',
      department: 'Công nghệ thông tin',
      major: 'Kỹ thuật phần mềm',
      avatarUrl:
          'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=200',
      maDonVi: 'CAMPUS_HN',
      currentGpa: 8.75,
      attendanceRate: 0.90, // 90%
      tuitionOwed: 1100000,
    ),
    const Child(
      id: 'std_02',
      fullName: 'Phan Minh Ngọc',
      code: 'SV20282405',
      classCode: 'K22-KH02',
      department: 'Khoa học máy tính',
      major: 'Trí tuệ nhân tạo',
      avatarUrl:
          'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&q=80&w=200',
      maDonVi: 'CAMPUS_HN',
      currentGpa: 7.20,
      attendanceRate: 0.98, // 98%
      tuitionOwed: 0,
    ),
  ];

  // Mock data for student 1 (Phan Thanh Danh) - Reused from Student Mock
  final List<GradeRecord> _grades1 = [
    const GradeRecord(
      id: 'g1_01',
      courseCode: 'CS301',
      courseName: 'Lập trình di động với Flutter',
      creditCount: 3,
      processScore: 8.5,
      midtermScore: 8.0,
      totalScore: null,
      semester: 'Học kỳ 1 - 2025-2026',
    ),
    const GradeRecord(
      id: 'g1_02',
      courseCode: 'CS402',
      courseName: 'Phân tích thiết kế hệ thống',
      creditCount: 4,
      processScore: 9.0,
      midtermScore: 7.5,
      finalScore: 8.0,
      totalScore: 8.1,
      letterGrade: 'B+',
      semester: 'Học kỳ 1 - 2025-2026',
    ),
    const GradeRecord(
      id: 'g1_03',
      courseCode: 'CS205',
      courseName: 'Cơ sở dữ liệu nâng cao',
      creditCount: 3,
      processScore: 9.5,
      midtermScore: 9.0,
      finalScore: 9.5,
      totalScore: 9.4,
      letterGrade: 'A',
      semester: 'Học kỳ 1 - 2025-2026',
    ),
  ];
  final List<SemesterGPA> _gpas1 = [
    const SemesterGPA(
      semester: 'Học kỳ 1 - 2025-2026',
      gpa: 8.75,
      gpa4: 3.5,
      creditCount: 10,
    ),
  ];

  // Mock data for student 2 (Phan Minh Ngọc)
  final List<GradeRecord> _grades2 = [
    const GradeRecord(
      id: 'g2_01',
      courseCode: 'AI101',
      courseName: 'Nhập môn Trí tuệ nhân tạo',
      creditCount: 3,
      processScore: 7.0,
      midtermScore: 6.5,
      finalScore: 7.5,
      totalScore: 7.1,
      letterGrade: 'B',
      semester: 'Học kỳ 1 - 2025-2026',
    ),
    const GradeRecord(
      id: 'g2_02',
      courseCode: 'MATH201',
      courseName: 'Đại số tuyến tính',
      creditCount: 3,
      processScore: 6.5,
      midtermScore: 7.0,
      finalScore: 6.0,
      totalScore: 6.3,
      letterGrade: 'C',
      semester: 'Học kỳ 1 - 2025-2026',
    ),
    const GradeRecord(
      id: 'g2_03',
      courseCode: 'CS102',
      courseName: 'Cấu trúc dữ liệu và giải thuật',
      creditCount: 4,
      processScore: 8.0,
      midtermScore: 7.5,
      finalScore: 8.0,
      totalScore: 7.9,
      letterGrade: 'B',
      semester: 'Học kỳ 1 - 2025-2026',
    ),
  ];
  final List<SemesterGPA> _gpas2 = [
    const SemesterGPA(
      semester: 'Học kỳ 1 - 2025-2026',
      gpa: 7.20,
      gpa4: 2.8,
      creditCount: 10,
    ),
  ];

  static DateTime _getTodayWithOffset(int offsetDays) {
    final now = DateTime.now();
    return DateTime(
      now.year,
      now.month,
      now.day,
    ).add(Duration(days: offsetDays));
  }

  // Schedules
  late final Map<String, List<ScheduleEvent>> _schedules = {
    'std_01': [
      ScheduleEvent(
        id: 's1_01',
        courseName: 'Lập trình di động với Flutter',
        room: 'Phòng Lab 302',
        teacherName: 'ThS. Nguyễn Văn A',
        date: _getTodayWithOffset(0),
        startTime: '08:00',
        endTime: '11:30',
      ),
      ScheduleEvent(
        id: 's1_02',
        courseName: 'Phân tích thiết kế hệ thống',
        room: 'Phòng 105',
        teacherName: 'TS. Lê Thị B',
        date: _getTodayWithOffset(1),
        startTime: '13:30',
        endTime: '17:00',
      ),
    ],
    'std_02': [
      ScheduleEvent(
        id: 's2_01',
        courseName: 'Nhập môn Trí tuệ nhân tạo',
        room: 'Phòng 403',
        teacherName: 'PGS. TS. Trần Đức D',
        date: _getTodayWithOffset(0),
        startTime: '13:30',
        endTime: '17:00',
      ),
      ScheduleEvent(
        id: 's2_02',
        courseName: 'Cấu trúc dữ liệu và giải thuật',
        room: 'Phòng Lab 202',
        teacherName: 'ThS. Hoàng Văn E',
        date: _getTodayWithOffset(1),
        startTime: '08:00',
        endTime: '11:30',
      ),
      ScheduleEvent(
        id: 's2_03',
        courseName: 'Đại số tuyến tính',
        room: 'Phòng 304',
        teacherName: 'TS. Nguyễn Thị F',
        date: _getTodayWithOffset(2),
        startTime: '08:00',
        endTime: '10:00',
      ),
    ],
  };

  // Exams
  late final Map<String, List<ExamSchedule>> _exams = {
    'std_01': [
      ExamSchedule(
        id: 'e1_01',
        courseName: 'Lập trình di động với Flutter',
        examDate: _getTodayWithOffset(7),
        startTime: '08:00',
        endTime: '10:00',
        room: 'Phòng Lab 302',
        seatNumber: 'B-12',
        format: ExamFormat.practice,
        status: ExamStatus.upcoming,
      ),
      ExamSchedule(
        id: 'e1_02',
        courseName: 'Phân tích thiết kế hệ thống',
        examDate: _getTodayWithOffset(9),
        startTime: '13:30',
        endTime: '15:30',
        room: 'Phòng Lý thuyết 105',
        seatNumber: 'A-45',
        format: ExamFormat.writing,
        status: ExamStatus.upcoming,
      ),
    ],
    'std_02': [
      ExamSchedule(
        id: 'e2_01',
        courseName: 'Nhập môn Trí tuệ nhân tạo',
        examDate: _getTodayWithOffset(6),
        startTime: '13:30',
        endTime: '15:30',
        room: 'Phòng 403',
        seatNumber: 'C-22',
        format: ExamFormat.writing,
        status: ExamStatus.upcoming,
      ),
      ExamSchedule(
        id: 'e2_02',
        courseName: 'Cấu trúc dữ liệu và giải thuật',
        examDate: _getTodayWithOffset(8),
        startTime: '08:00',
        endTime: '10:00',
        room: 'Phòng Máy 202',
        seatNumber: 'A-10',
        format: ExamFormat.practice,
        status: ExamStatus.upcoming,
      ),
    ],
  };

  // Attendance
  late final Map<String, List<AttendanceRecord>> _attendance = {
    'std_01': [
      AttendanceRecord(
        id: 'a1_01',
        date: DateTime.now().subtract(const Duration(days: 1)),
        courseName: 'Lập trình di động với Flutter',
        status: AttendanceStatus.present,
        note: 'Đúng giờ',
      ),
      AttendanceRecord(
        id: 'a1_02',
        date: DateTime.now().subtract(const Duration(days: 2)),
        courseName: 'Phân tích thiết kế hệ thống',
        status: AttendanceStatus.present,
        note: 'Đúng giờ',
      ),
      AttendanceRecord(
        id: 'a1_03',
        date: DateTime.now().subtract(const Duration(days: 7)),
        courseName: 'Lập trình di động với Flutter',
        status: AttendanceStatus.absent,
        note: 'Nghỉ học không phép',
      ),
    ],
    'std_02': [
      AttendanceRecord(
        id: 'a2_01',
        date: DateTime.now().subtract(const Duration(days: 1)),
        courseName: 'Nhập môn Trí tuệ nhân tạo',
        status: AttendanceStatus.present,
        note: 'Đúng giờ',
      ),
      AttendanceRecord(
        id: 'a2_02',
        date: DateTime.now().subtract(const Duration(days: 2)),
        courseName: 'Cấu trúc dữ liệu và giải thuật',
        status: AttendanceStatus.present,
        note: 'Đúng giờ',
      ),
      AttendanceRecord(
        id: 'a2_03',
        date: DateTime.now().subtract(const Duration(days: 3)),
        courseName: 'Đại số tuyến tính',
        status: AttendanceStatus.present,
        note: 'Đúng giờ',
      ),
    ],
  };

  // Tuition
  final Map<String, List<TuitionInvoice>> _tuition = {
    'std_01': [
      TuitionInvoice(
        id: 't1_01',
        termName: 'Học kỳ 1 - Năm học 2025-2026',
        amount: 15400000,
        paidAmount: 15400000,
        dueDate: DateTime.now().subtract(const Duration(days: 20)),
        status: InvoiceStatus.paid,
        paymentDate: DateTime.now().subtract(const Duration(days: 22)),
      ),
      TuitionInvoice(
        id: 't1_02',
        termName: 'Học phí bổ sung - Học kỳ 1 2025-2026',
        amount: 2100000,
        paidAmount: 1000000,
        dueDate: DateTime.now().add(const Duration(days: 5)),
        status: InvoiceStatus.partial,
      ),
    ],
    'std_02': [
      TuitionInvoice(
        id: 't2_01',
        termName: 'Học kỳ 1 - Năm học 2025-2026',
        amount: 16500000,
        paidAmount: 16500000,
        dueDate: DateTime.now().subtract(const Duration(days: 20)),
        status: InvoiceStatus.paid,
        paymentDate: DateTime.now().subtract(const Duration(days: 21)),
      ),
    ],
  };

  final List<StudentNotification> _notifications = [
    StudentNotification(
      id: 'pn_01',
      title: 'Nhắc nhở đóng học phí bổ sung cho Phan Thanh Danh',
      content:
          'Số tiền học phí còn thiếu: 1,100,000đ. Hạn chót đóng học phí bổ sung học kỳ 1 diễn ra vào ngày 13/07/2026.',
      timestamp: DateTime.now().subtract(const Duration(hours: 1)),
      isRead: false,
      type: NotificationType.tuition,
    ),
    StudentNotification(
      id: 'pn_02',
      title: 'Cảnh báo vắng học của Phan Thanh Danh',
      content:
          'Học sinh Phan Thanh Danh đã nghỉ học không phép môn Lập trình di động với Flutter ngày 01/07/2026. Kính báo phụ huynh lưu ý.',
      timestamp: DateTime.now().subtract(const Duration(days: 2)),
      isRead: false,
      type: NotificationType.academic,
    ),
    StudentNotification(
      id: 'pn_03',
      title: 'Công bố điểm thi của Phan Minh Ngọc',
      content:
          'Học sinh Phan Minh Ngọc đã có điểm thi môn Đại số tuyến tính. Điểm tổng kết đạt 6.3 (Điểm chữ: C).',
      timestamp: DateTime.now().subtract(const Duration(days: 3)),
      isRead: true,
      type: NotificationType.grade,
    ),
    StudentNotification(
      id: 'pn_04',
      title: 'Đăng ký tham gia họp phụ huynh cuối kỳ',
      content:
          'Nhà trường thông báo tổ chức buổi họp phụ huynh cuối kỳ vào 8h30 sáng Chủ Nhật ngày 19/07/2026 tại hội trường lớn.',
      timestamp: DateTime.now().subtract(const Duration(days: 4)),
      isRead: true,
      type: NotificationType.general,
    ),
    StudentNotification(
      id: 'pn_05',
      title: 'Công bố điểm thi của Phan Thanh Danh',
      content:
          'Học sinh Phan Thanh Danh đạt 9.4 môn Cơ sở dữ liệu nâng cao (Điểm chữ: A). Chúc mừng học sinh đạt kết quả tốt.',
      timestamp: DateTime.now().subtract(const Duration(days: 5)),
      isRead: true,
      type: NotificationType.grade,
    ),
  ];

  @override
  Future<ParentProfile> getProfile() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _profile;
  }

  @override
  Future<void> updateProfile(String phone, String email) async {
    await Future.delayed(const Duration(milliseconds: 500));
    _profile = ParentProfile(
      id: _profile.id,
      fullName: _profile.fullName,
      email: email,
      phone: phone,
      avatarUrl: _profile.avatarUrl,
      address: _profile.address,
    );
  }

  @override
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    await Future.delayed(const Duration(milliseconds: 500));
  }

  @override
  Future<List<Child>> getChildren() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _children;
  }

  @override
  Future<List<GradeRecord>> getChildGrades(String childId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return childId == 'std_01' ? _grades1 : _grades2;
  }

  @override
  Future<List<SemesterGPA>> getChildSemesterGPAs(String childId) async {
    await Future.delayed(const Duration(milliseconds: 400));
    return childId == 'std_01' ? _gpas1 : _gpas2;
  }

  @override
  Future<List<ScheduleEvent>> getChildSchedule(String childId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _schedules[childId] ?? [];
  }

  @override
  Future<List<ExamSchedule>> getChildExams(String childId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _exams[childId] ?? [];
  }

  @override
  Future<List<AttendanceRecord>> getChildAttendance(String childId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _attendance[childId] ?? [];
  }

  @override
  Future<List<TuitionInvoice>> getChildTuition(String childId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _tuition[childId] ?? [];
  }

  @override
  Future<TuitionPayment> createChildTuitionPayment(
    String childId,
    String invoiceId,
  ) {
    throw UnsupportedError('Mock repository không tạo giao dịch thanh toán.');
  }

  @override
  Future<TuitionPayment> getChildTuitionPayment(
    String childId,
    String transactionId,
  ) {
    throw UnsupportedError('Mock repository không có trạng thái thanh toán.');
  }

  @override
  Future<List<StudentNotification>> getNotifications() async {
    await Future.delayed(const Duration(milliseconds: 400));
    return _notifications;
  }

  @override
  Future<void> markNotificationAsRead(String id) async {
    final idx = _notifications.indexWhere((n) => n.id == id);
    if (idx != -1) {
      final old = _notifications[idx];
      _notifications[idx] = StudentNotification(
        id: old.id,
        title: old.title,
        content: old.content,
        timestamp: old.timestamp,
        isRead: true,
        type: old.type,
      );
    }
  }
}
