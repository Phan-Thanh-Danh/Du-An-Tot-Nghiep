import '../models/student_models.dart';
import '../../finance/models/tuition_payment.dart';
import 'student_repository.dart';

class StudentMockRepository implements StudentRepository {
  StudentProfile _profile = const StudentProfile(
    id: 'std_01',
    code: 'SV20261102',
    fullName: 'Phan Thanh Danh',
    email: 'danh.pt.26@lms.edu.vn',
    phone: '0987654321',
    avatarUrl:
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=200',
    department: 'Công nghệ thông tin',
    major: 'Kỹ thuật phần mềm',
    classCode: 'K20-PM01',
    maDonVi: 'CAMPUS_HN',
  );

  final List<Course> _courses = [
    const Course(
      id: 'c_01',
      code: 'CS301',
      name: 'Lập trình di động với Flutter',
      teacherName: 'ThS. Nguyễn Văn A',
      creditCount: 3,
      progress: 0.75,
      bannerUrl:
          'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&q=80&w=400',
    ),
    const Course(
      id: 'c_02',
      code: 'CS402',
      name: 'Phân tích thiết kế hệ thống',
      teacherName: 'TS. Lê Thị B',
      creditCount: 4,
      progress: 0.40,
      bannerUrl:
          'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&q=80&w=400',
    ),
    const Course(
      id: 'c_03',
      code: 'CS205',
      name: 'Cơ sở dữ liệu nâng cao',
      teacherName: 'ThS. Trần Văn C',
      creditCount: 3,
      progress: 0.90,
      bannerUrl:
          'https://images.unsplash.com/photo-1544383835-bda2bc66a55d?auto=format&fit=crop&q=80&w=400',
    ),
    const Course(
      id: 'c_04',
      code: 'ENG102',
      name: 'Tiếng Anh chuyên ngành CNTT',
      teacherName: 'Cô Emma Watson',
      creditCount: 2,
      progress: 0.15,
      bannerUrl:
          'https://images.unsplash.com/photo-1434030216411-0b793f4b4173?auto=format&fit=crop&q=80&w=400',
    ),
  ];

  final Map<String, List<Lesson>> _lessons = {
    'c_01': [
      const Lesson(
        id: 'l_01_01',
        title: 'Giới thiệu về Flutter & Cài đặt môi trường',
        content:
            'Tổng quan về Flutter framework, Dart language và cách cấu hình SDK, Android Studio, Xcode.',
        type: LessonType.video,
        durationMinutes: 45,
        resourceUrl: 'https://www.w3schools.com/html/mov_bbb.mp4',
        isCompleted: true,
      ),
      const Lesson(
        id: 'l_01_02',
        title: 'Widget cơ bản và Layout trong Flutter',
        content:
            'Học về Stateless/Stateful Widget, Container, Row, Column, Stack, ListView.',
        type: LessonType.video,
        durationMinutes: 60,
        resourceUrl: 'https://www.w3schools.com/html/mov_bbb.mp4',
        isCompleted: true,
      ),
      const Lesson(
        id: 'l_01_03',
        title: 'Tài liệu hướng dẫn quản lý State với Riverpod',
        content:
            'Tài liệu chi tiết về Provider, StateProvider, NotifierProvider và thực hành cơ bản.',
        type: LessonType.pdf,
        durationMinutes: 30,
        resourceUrl:
            'https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf',
        isCompleted: true,
      ),
      const Lesson(
        id: 'l_01_04',
        title: 'Xử lý bất đồng bộ & kết nối API bằng Dio',
        content:
            'Tìm hiểu Future, Stream, cấu hình Dio client, interceptors, và parse JSON.',
        type: LessonType.video,
        durationMinutes: 90,
        resourceUrl: 'https://www.w3schools.com/html/mov_bbb.mp4',
        isCompleted: false,
      ),
      const Lesson(
        id: 'l_01_05',
        title: 'Kiểm tra lý thuyết: Riverpod & Networking',
        content:
            'Bài kiểm tra trắc nghiệm 15 câu về quản lý trạng thái và gọi API.',
        type: LessonType.text,
        durationMinutes: 15,
        resourceUrl: '',
        isCompleted: false,
      ),
    ],
    'c_02': [
      const Lesson(
        id: 'l_02_01',
        title: 'Khái niệm về Phân tích & Thiết kế hệ thống',
        content:
            'Vòng đời phát triển phần mềm (SDLC), mô hình Agile/Scrum và vai trò BA.',
        type: LessonType.video,
        durationMinutes: 50,
        resourceUrl: 'https://www.w3schools.com/html/mov_bbb.mp4',
        isCompleted: true,
      ),
      const Lesson(
        id: 'l_02_02',
        title: 'Thiết kế Usecase Diagram',
        content:
            'Cách xác định Actor, Use Case, mối quan hệ Include, Extend và vẽ biểu đồ.',
        type: LessonType.pdf,
        durationMinutes: 40,
        resourceUrl:
            'https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf',
        isCompleted: false,
      ),
    ],
  };

  final List<Assignment> _assignments = [
    Assignment(
      id: 'a_01',
      title: 'Thiết kế giao diện login & dashboard app',
      description:
          'Sử dụng các widget đã học để thiết kế màn hình Login và Dashboard của app LMS. Nộp link github.',
      courseName: 'Lập trình di động với Flutter',
      dueDate: DateTime.now().add(const Duration(days: 2)),
      status: AssignmentStatus.notSubmitted,
    ),
    Assignment(
      id: 'a_02',
      title: 'Phân tích biểu đồ Use Case cho website bán hàng',
      description:
          'Xác định ít nhất 5 actor, 15 usecase và vẽ sơ đồ kèm đặc tả chi tiết 3 usecase chính.',
      courseName: 'Phân tích thiết kế hệ thống',
      dueDate: DateTime.now().subtract(const Duration(days: 1)),
      submitDate: DateTime.now().subtract(const Duration(days: 1, hours: 2)),
      score: 8.5,
      status: AssignmentStatus.submitted,
    ),
    Assignment(
      id: 'a_03',
      title: 'Bài tập SQL nâng cao & Tối ưu câu lệnh',
      description:
          'Viết các câu lệnh Query phức tạp sử dụng CTE, Window Functions và phân tích Execution Plan.',
      courseName: 'Cơ sở dữ liệu nâng cao',
      dueDate: DateTime.now().subtract(const Duration(days: 5)),
      status: AssignmentStatus.overdue,
    ),
  ];

  final List<ExamSchedule> _exams = [
    ExamSchedule(
      id: 'e_01',
      courseName: 'Lập trình di động với Flutter',
      examDate: DateTime.now().add(const Duration(days: 7)),
      startTime: '08:00',
      endTime: '10:00',
      room: 'Phòng Lab 302',
      seatNumber: 'B-12',
      format: ExamFormat.practice,
      status: ExamStatus.upcoming,
    ),
    ExamSchedule(
      id: 'e_02',
      courseName: 'Phân tích thiết kế hệ thống',
      examDate: DateTime.now().add(const Duration(days: 9)),
      startTime: '13:30',
      endTime: '15:30',
      room: 'Phòng Lý thuyết 105',
      seatNumber: 'A-45',
      format: ExamFormat.writing,
      status: ExamStatus.upcoming,
    ),
    ExamSchedule(
      id: 'e_03',
      courseName: 'Cơ sở dữ liệu nâng cao',
      examDate: DateTime.now().subtract(const Duration(days: 15)),
      startTime: '09:30',
      endTime: '11:00',
      room: 'Phòng Máy 204',
      seatNumber: 'C-08',
      format: ExamFormat.multipleChoice,
      status: ExamStatus.completed,
    ),
  ];

  final List<GradeRecord> _grades = [
    const GradeRecord(
      id: 'g_01',
      courseCode: 'CS301',
      courseName: 'Lập trình di động với Flutter',
      creditCount: 3,
      processScore: 8.5,
      midtermScore: 8.0,
      finalScore: null,
      totalScore: null,
      semester: 'Học kỳ 1 - 2025-2026',
    ),
    const GradeRecord(
      id: 'g_02',
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
      id: 'g_03',
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
    const GradeRecord(
      id: 'g_04',
      courseCode: 'CS101',
      courseName: 'Nhập môn lập trình C/C++',
      creditCount: 3,
      processScore: 8.0,
      midtermScore: 7.0,
      finalScore: 7.5,
      totalScore: 7.5,
      letterGrade: 'B',
      semester: 'Học kỳ 2 - 2024-2025',
    ),
    const GradeRecord(
      id: 'g_05',
      courseCode: 'MATH101',
      courseName: 'Giải tích 1',
      creditCount: 3,
      processScore: 7.0,
      midtermScore: 6.5,
      finalScore: 7.0,
      totalScore: 6.8,
      letterGrade: 'C+',
      semester: 'Học kỳ 2 - 2024-2025',
    ),
    const GradeRecord(
      id: 'g_06',
      courseCode: 'ENG101',
      courseName: 'Tiếng Anh cơ bản',
      creditCount: 3,
      processScore: 9.0,
      midtermScore: 9.5,
      finalScore: 8.5,
      totalScore: 8.9,
      letterGrade: 'A',
      semester: 'Học kỳ 2 - 2024-2025',
    ),
  ];

  final List<SemesterGPA> _gpas = [
    const SemesterGPA(
      semester: 'Học kỳ 2 - 2024-2025',
      gpa: 7.73,
      gpa4: 3.1,
      creditCount: 9,
    ),
    const SemesterGPA(
      semester: 'Học kỳ 1 - 2025-2026',
      gpa: 8.75,
      gpa4: 3.5,
      creditCount: 10,
    ),
  ];

  late final List<ScheduleEvent> _schedule = [
    ScheduleEvent(
      id: 's_01',
      courseName: 'Lập trình di động với Flutter',
      room: 'Phòng Lab 302',
      teacherName: 'ThS. Nguyễn Văn A',
      date: _getTodayWithOffset(0), // Today
      startTime: '08:00',
      endTime: '11:30',
    ),
    ScheduleEvent(
      id: 's_02',
      courseName: 'Phân tích thiết kế hệ thống',
      room: 'Phòng 105',
      teacherName: 'TS. Lê Thị B',
      date: _getTodayWithOffset(1), // Tomorrow
      startTime: '13:30',
      endTime: '17:00',
    ),
    ScheduleEvent(
      id: 's_03',
      courseName: 'Cơ sở dữ liệu nâng cao',
      room: 'Phòng Lab 204',
      teacherName: 'ThS. Trần Văn C',
      date: _getTodayWithOffset(2), // Day after tomorrow
      startTime: '08:00',
      endTime: '11:30',
    ),
    ScheduleEvent(
      id: 's_04',
      courseName: 'Tiếng Anh chuyên ngành CNTT',
      room: 'Phòng 301',
      teacherName: 'Cô Emma Watson',
      date: _getTodayWithOffset(3),
      startTime: '10:00',
      endTime: '11:30',
    ),
    ScheduleEvent(
      id: 's_05',
      courseName: 'Lập trình di động với Flutter',
      room: 'Phòng Lab 302',
      teacherName: 'ThS. Nguyễn Văn A',
      date: _getTodayWithOffset(4),
      startTime: '08:00',
      endTime: '11:30',
    ),
  ];

  late final List<AttendanceRecord> _attendance = [
    AttendanceRecord(
      id: 'att_01',
      date: DateTime.now().subtract(const Duration(days: 1)),
      courseName: 'Lập trình di động với Flutter',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
    AttendanceRecord(
      id: 'att_02',
      date: DateTime.now().subtract(const Duration(days: 2)),
      courseName: 'Phân tích thiết kế hệ thống',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
    AttendanceRecord(
      id: 'att_03',
      date: DateTime.now().subtract(const Duration(days: 3)),
      courseName: 'Cơ sở dữ liệu nâng cao',
      status: AttendanceStatus.late,
      note: 'Đi muộn 15p',
    ),
    AttendanceRecord(
      id: 'att_04',
      date: DateTime.now().subtract(const Duration(days: 4)),
      courseName: 'Tiếng Anh chuyên ngành CNTT',
      status: AttendanceStatus.present,
      note: 'Tích cực phát biểu',
    ),
    AttendanceRecord(
      id: 'att_05',
      date: DateTime.now().subtract(const Duration(days: 7)),
      courseName: 'Lập trình di động với Flutter',
      status: AttendanceStatus.absent,
      note: 'Nghỉ học không phép',
    ),
    AttendanceRecord(
      id: 'att_06',
      date: DateTime.now().subtract(const Duration(days: 8)),
      courseName: 'Phân tích thiết kế hệ thống',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
    AttendanceRecord(
      id: 'att_07',
      date: DateTime.now().subtract(const Duration(days: 9)),
      courseName: 'Cơ sở dữ liệu nâng cao',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
    AttendanceRecord(
      id: 'att_08',
      date: DateTime.now().subtract(const Duration(days: 10)),
      courseName: 'Tiếng Anh chuyên ngành CNTT',
      status: AttendanceStatus.excused,
      note: 'Nghỉ phép có lò đơn',
    ),
    AttendanceRecord(
      id: 'att_09',
      date: DateTime.now().subtract(const Duration(days: 14)),
      courseName: 'Lập trình di động với Flutter',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
    AttendanceRecord(
      id: 'att_10',
      date: DateTime.now().subtract(const Duration(days: 15)),
      courseName: 'Phân tích thiết kế hệ thống',
      status: AttendanceStatus.present,
      note: 'Đúng giờ',
    ),
  ];

  final List<TuitionInvoice> _tuition = [
    TuitionInvoice(
      id: 'inv_01',
      termName: 'Học kỳ 1 - Năm học 2025-2026',
      amount: 15400000,
      paidAmount: 15400000,
      dueDate: DateTime.now().subtract(const Duration(days: 20)),
      status: InvoiceStatus.paid,
      paymentDate: DateTime.now().subtract(const Duration(days: 22)),
    ),
    TuitionInvoice(
      id: 'inv_02',
      termName: 'Học phí bổ sung - Học kỳ 1 2025-2026',
      amount: 2100000,
      paidAmount: 1000000,
      dueDate: DateTime.now().add(const Duration(days: 5)),
      status: InvoiceStatus.partial,
    ),
    TuitionInvoice(
      id: 'inv_03',
      termName: 'Lệ phí thi chứng chỉ anh văn đầu ra',
      amount: 450000,
      paidAmount: 0,
      dueDate: DateTime.now().add(const Duration(days: 15)),
      status: InvoiceStatus.unpaid,
    ),
    TuitionInvoice(
      id: 'inv_04',
      termName: 'Học kỳ 2 - Năm học 2024-2025',
      amount: 14800000,
      paidAmount: 14800000,
      dueDate: DateTime.now().subtract(const Duration(days: 150)),
      status: InvoiceStatus.paid,
      paymentDate: DateTime.now().subtract(const Duration(days: 155)),
    ),
  ];

  final List<StudentNotification> _notifications = [
    StudentNotification(
      id: 'n_01',
      title: 'Cảnh báo hạn nộp học phí bổ sung kỳ 1',
      content:
          'Nhắc nhở hạn chót hoàn thành học phí bổ sung học kỳ 1 (2025-2026) vào ngày 13/07/2026. Số tiền còn nợ: 1,100,000đ. Quá hạn tài khoản có thể bị khóa đăng ký môn.',
      timestamp: DateTime.now().subtract(const Duration(hours: 2)),
      isRead: false,
      type: NotificationType.tuition,
    ),
    StudentNotification(
      id: 'n_02',
      title: 'Đã cập nhật điểm thi Cơ sở dữ liệu nâng cao',
      content:
          'Giảng viên đã công bố điểm cuối kỳ cho môn học Cơ sở dữ liệu nâng cao. Điểm cuối kỳ của bạn là 9.5. Vui lòng kiểm tra lại bảng điểm.',
      timestamp: DateTime.now().subtract(const Duration(days: 1)),
      isRead: false,
      type: NotificationType.grade,
    ),
    StudentNotification(
      id: 'n_03',
      title: 'Thông báo lịch thi học kỳ 1',
      content:
          'Lịch thi học kỳ 1 chính thức năm học 2025-2026 đã được cập nhật trên app. Môn thi đầu tiên: Lập trình di động với Flutter diễn ra vào ngày 15/07/2026.',
      timestamp: DateTime.now().subtract(const Duration(days: 2)),
      isRead: true,
      type: NotificationType.academic,
    ),
    StudentNotification(
      id: 'n_04',
      title: 'Thông báo bảo trì hệ thống LMS',
      content:
          'Hệ thống LMS sẽ tạm đóng để bảo trì định kỳ từ 0h00 đến 4h00 sáng ngày 10/07/2026. Rất xin lỗi vì sự bất tiện này.',
      timestamp: DateTime.now().subtract(const Duration(days: 3)),
      isRead: true,
      type: NotificationType.system,
    ),
    StudentNotification(
      id: 'n_05',
      title: 'Giao bài tập mới: Lập trình di động với Flutter',
      content:
          'Giảng viên Nguyễn Văn A đã đăng tải bài tập mới "Thiết kế giao diện login & dashboard app". Hạn nộp: 10/07/2026.',
      timestamp: DateTime.now().subtract(const Duration(days: 4)),
      isRead: true,
      type: NotificationType.academic,
    ),
    StudentNotification(
      id: 'n_06',
      title: 'Thay đổi phòng học môn Phân tích thiết kế hệ thống',
      content:
          'Môn học Phân tích thiết kế hệ thống vào ngày mai sẽ chuyển từ phòng học 201 sang phòng học 105. Thời gian không đổi.',
      timestamp: DateTime.now().subtract(const Duration(days: 5)),
      isRead: true,
      type: NotificationType.academic,
    ),
    StudentNotification(
      id: 'n_07',
      title: 'Thông báo đăng ký câu lạc bộ tin học',
      content:
          'Câu lạc bộ Tin Học (IT Club) thông báo tuyển thành viên mới kỳ hè. Đăng ký online trước 15/07/2026.',
      timestamp: DateTime.now().subtract(const Duration(days: 7)),
      isRead: true,
      type: NotificationType.general,
    ),
    StudentNotification(
      id: 'n_08',
      title: 'Cảnh báo vắng học môn Flutter',
      content:
          'Hệ thống ghi nhận bạn đã vắng 1 buổi học môn Lập trình di động với Flutter ngày 01/07/2026. Số buổi vắng tối đa cho phép: 3 buổi.',
      timestamp: DateTime.now().subtract(const Duration(days: 8)),
      isRead: true,
      type: NotificationType.academic,
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

  @override
  Future<StudentProfile> getProfile() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _profile;
  }

  @override
  Future<void> updateProfile(String phone, String email) async {
    await Future.delayed(const Duration(milliseconds: 500));
    _profile = StudentProfile(
      id: _profile.id,
      code: _profile.code,
      fullName: _profile.fullName,
      email: email,
      phone: phone,
      avatarUrl: _profile.avatarUrl,
      department: _profile.department,
      major: _profile.major,
      classCode: _profile.classCode,
      maDonVi: _profile.maDonVi,
    );
  }

  @override
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    await Future.delayed(const Duration(milliseconds: 500));
    // Mock change success
  }

  @override
  Future<List<Course>> getCourses() async {
    await Future.delayed(const Duration(milliseconds: 600));
    return _courses;
  }

  @override
  Future<List<Lesson>> getCourseLessons(String courseId) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _lessons[courseId] ?? [];
  }

  @override
  Future<List<Assignment>> getAssignments() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _assignments;
  }

  @override
  Future<void> submitAssignment(String assignmentId, String filePath) async {
    throw UnsupportedError('Mock repository không hỗ trợ nộp bài thật.');
  }

  @override
  Future<List<ExamSchedule>> getExamSchedules() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _exams;
  }

  @override
  Future<List<GradeRecord>> getGradeRecords() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _grades;
  }

  @override
  Future<List<SemesterGPA>> getSemesterGPAs() async {
    await Future.delayed(const Duration(milliseconds: 400));
    return _gpas;
  }

  @override
  Future<List<ScheduleEvent>> getScheduleEvents() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _schedule;
  }

  @override
  Future<List<AttendanceRecord>> getAttendanceRecords() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return _attendance;
  }

  @override
  Future<List<TuitionInvoice>> getTuitionInvoices() async {
    await Future.delayed(const Duration(milliseconds: 600));
    return _tuition;
  }

  @override
  Future<TuitionPayment> createTuitionPayment(String invoiceId) {
    throw UnsupportedError('Mock repository không tạo giao dịch thanh toán.');
  }

  @override
  Future<TuitionPayment> getTuitionPayment(String transactionId) {
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
