import 'package:intl/intl.dart';
import 'package:dio/dio.dart';

import '../../../core/network/api_client.dart';
import '../../finance/models/tuition_payment.dart';
import '../models/student_models.dart';
import 'student_repository.dart';

class StudentRemoteRepository implements StudentRepository {
  final ApiClient _api;

  StudentRemoteRepository(this._api);

  Future<dynamic> _getData(String path, {Map<String, dynamic>? query}) async {
    try {
      final response = await _api.get<dynamic>(path, queryParameters: query);
      return ApiClient.unwrap(response.data);
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  Map<String, dynamic> _map(dynamic value) =>
      value is Map<String, dynamic> ? value : <String, dynamic>{};

  List<dynamic> _list(dynamic value) => value is List ? value : const [];

  double _double(dynamic value) =>
      value is num ? value.toDouble() : double.tryParse('$value') ?? 0;

  int _int(dynamic value) =>
      value is num ? value.toInt() : int.tryParse('$value') ?? 0;

  DateTime _date(dynamic value, {DateTime? fallback}) =>
      DateTime.tryParse(value?.toString() ?? '') ?? fallback ?? DateTime.now();

  @override
  Future<StudentProfile> getProfile() async {
    final account = _map(await _getData('/account/me'));
    return StudentProfile(
      id: account['id']?.toString() ?? '',
      code: account['id']?.toString() ?? '',
      fullName: account['hoTen']?.toString() ?? 'Sinh viên',
      email: account['email']?.toString() ?? '',
      phone: account['soDienThoai']?.toString() ?? '',
      avatarUrl: '',
      department: account['campus']?.toString() ?? '',
      major: account['major']?.toString() ?? '',
      classCode: account['className']?.toString() ?? '',
      maDonVi: '',
    );
  }

  @override
  Future<void> updateProfile(String phone, String email) async {
    try {
      await _api.put<dynamic>(
        '/account/profile',
        data: {'email': email.trim(), 'soDienThoai': phone.trim()},
      );
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    try {
      await _api.put<dynamic>(
        '/account/change-password',
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
          'confirmPassword': newPassword,
        },
      );
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<List<Course>> getCourses() async {
    final data = _list(await _getData('/student/courses'));
    return data.map((item) {
      final json = _map(item);
      return Course(
        id: json['id']?.toString() ?? '',
        code: json['code']?.toString() ?? '',
        name: json['name']?.toString() ?? '',
        teacherName: json['lecturer']?.toString() ?? 'Chưa phân công',
        creditCount: 0,
        progress: (_double(json['progress']) / 100).clamp(0, 1),
        bannerUrl: '',
      );
    }).toList();
  }

  @override
  Future<List<Lesson>> getCourseLessons(String courseId) async {
    final data = _map(await _getData('/student/courses/$courseId'));
    final chapters = _list(data['lessons']);
    return chapters.expand((chapterItem) {
      final chapter = _map(chapterItem);
      return _list(chapter['lessons']).map((lessonItem) {
        final json = _map(lessonItem);
        final type = switch (json['type']?.toString().toLowerCase()) {
          'video' => LessonType.video,
          'pdf' || 'document' => LessonType.pdf,
          _ => LessonType.text,
        };
        final durationText = json['duration']?.toString() ?? '';
        final minutes =
            int.tryParse(
              RegExp(r'\d+').firstMatch(durationText)?.group(0) ?? '',
            ) ??
            0;
        final status = json['status']?.toString().toLowerCase() ?? '';
        return Lesson(
          id: json['id']?.toString() ?? '',
          title: json['title']?.toString() ?? '',
          content: chapter['description']?.toString() ?? '',
          type: type,
          durationMinutes: minutes,
          resourceUrl: json['url']?.toString() ?? '',
          isCompleted: status == 'completed' || status == 'hoan_thanh',
        );
      });
    }).toList();
  }

  @override
  Future<List<Assignment>> getAssignments() async {
    final data = _list(await _getData('/student/assignments'));
    return data.map((item) {
      final json = _map(item);
      final statusText = json['status']?.toString().toLowerCase() ?? '';
      final dueDate =
          DateFormat(
            'dd/MM/yyyy',
          ).tryParse(json['deadline']?.toString() ?? '') ??
          DateTime.now();
      return Assignment(
        id: json['id']?.toString() ?? '',
        title: json['title']?.toString() ?? '',
        description: '',
        courseName: json['course']?.toString() ?? '',
        dueDate: dueDate,
        status: statusText.contains('đã nộp')
            ? AssignmentStatus.submitted
            : statusText.contains('quá hạn')
            ? AssignmentStatus.overdue
            : AssignmentStatus.notSubmitted,
      );
    }).toList();
  }

  @override
  Future<void> submitAssignment(String assignmentId, String filePath) async {
    try {
      final fileName = filePath.split(RegExp(r'[/\\]')).last;
      await _api.post<dynamic>(
        '/student/assignments/$assignmentId/submit',
        data: FormData.fromMap({
          'file': await MultipartFile.fromFile(filePath, filename: fileName),
        }),
      );
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<List<ExamSchedule>> getExamSchedules() async {
    final data = _list(await _getData('/exam/student/list'));
    return data.map((item) {
      final json = _map(item);
      final openAt = _date(json['openAt']);
      final closeAt = _date(json['closeAt'], fallback: openAt);
      final type = json['examTypeLabel']?.toString().toLowerCase() ?? '';
      final accessStatus = json['accessStatus']?.toString().toLowerCase() ?? '';
      final hasResult = json['resultId']?.toString().isNotEmpty == true;
      final usedAttempts = _int(json['usedAttempts']);

      final ExamFormat format;
      if (type.contains('trắc') || type.contains('multiple')) {
        format = ExamFormat.multipleChoice;
      } else if (type.contains('tự luận') || type.contains('writing')) {
        format = ExamFormat.writing;
      } else if (type.contains('thực hành') || type.contains('practice')) {
        format = ExamFormat.practice;
      } else {
        format = ExamFormat.essay;
      }

      return ExamSchedule(
        id: json['id']?.toString() ?? json['maDeKiemTra']?.toString() ?? '',
        courseName: json['subject']?.toString().isNotEmpty == true
            ? json['subject'].toString()
            : json['title']?.toString() ?? '',
        examDate: openAt,
        startTime: DateFormat('HH:mm').format(openAt),
        endTime: DateFormat('HH:mm').format(closeAt),
        room: '',
        seatNumber: '',
        format: format,
        status:
            hasResult || usedAttempts > 0 || accessStatus.contains('completed')
            ? ExamStatus.completed
            : ExamStatus.upcoming,
      );
    }).toList();
  }

  Future<List<GradeRecord>> _loadGrades() async {
    final data = _map(await _getData('/student/grades'));
    return _list(data['subjects']).asMap().entries.map((entry) {
      final json = _map(entry.value);
      final totalScore = _double(json['gpa']);
      final backendGrade = json['letterGrade']?.toString().trim() ?? '';
      return GradeRecord(
        id: '${json['code'] ?? 'grade'}-${entry.key}',
        courseCode: json['code']?.toString() ?? '',
        courseName: json['name']?.toString() ?? '',
        creditCount: _int(json['credits']),
        processScore: json['processScore'] == null
            ? null
            : _double(json['processScore']),
        midtermScore: json['midtermScore'] == null
            ? null
            : _double(json['midtermScore']),
        finalScore: json['finalScore'] == null
            ? null
            : _double(json['finalScore']),
        totalScore: totalScore,
        letterGrade: backendGrade.isNotEmpty
            ? backendGrade
            : totalScore >= 5
            ? 'Đạt'
            : 'Nợ môn',
        semester: json['semester']?.toString() ?? 'Chưa xác định học kỳ',
      );
    }).toList();
  }

  @override
  Future<List<GradeRecord>> getGradeRecords() => _loadGrades();

  @override
  Future<List<SemesterGPA>> getSemesterGPAs() async {
    final grades = await _loadGrades();
    final grouped = <String, List<GradeRecord>>{};
    for (final grade in grades) {
      grouped.putIfAbsent(grade.semester, () => []).add(grade);
    }
    return grouped.entries.map((entry) {
      final valid = entry.value.where((g) => g.totalScore != null).toList();
      final gpa = valid.isEmpty
          ? 0.0
          : valid.fold<double>(0, (sum, g) => sum + g.totalScore!) /
                valid.length;
      return SemesterGPA(
        semester: entry.key,
        gpa: gpa,
        gpa4: (gpa / 10 * 4).clamp(0, 4),
        creditCount: entry.value.fold(0, (sum, g) => sum + g.creditCount),
      );
    }).toList();
  }

  @override
  Future<List<ScheduleEvent>> getScheduleEvents() async {
    final now = DateTime.now();
    final data = _map(
      await _getData(
        '/student/schedule',
        query: {
          'ngayTu': DateTime(
            now.year,
            now.month,
            now.day,
          ).subtract(const Duration(days: 14)).toIso8601String(),
          'ngayDen': DateTime(
            now.year,
            now.month,
            now.day,
          ).add(const Duration(days: 30)).toIso8601String(),
          'pageSize': 100,
        },
      ),
    );
    return _list(data['items']).map((item) {
      final json = _map(item);
      return ScheduleEvent(
        id: json['maBuoiHoc']?.toString() ?? '',
        courseName:
            json['tenMonHoc']?.toString() ??
            json['tieuDeKhoaHoc']?.toString() ??
            '',
        room:
            json['maCodePhong']?.toString() ??
            json['tenPhong']?.toString() ??
            '',
        teacherName: json['tenGiaoVienDayThay']?.toString().isNotEmpty == true
            ? json['tenGiaoVienDayThay'].toString()
            : json['tenGiaoVien']?.toString() ?? '',
        date: _date(json['ngayHoc']),
        startTime: json['gioBatDau']?.toString() ?? '',
        endTime: json['gioKetThuc']?.toString() ?? '',
      );
    }).toList();
  }

  @override
  Future<List<AttendanceRecord>> getAttendanceRecords() async {
    final page = _map(
      await _getData('/student/attendance', query: {'pageSize': 1000}),
    );
    return _list(page['items']).map((item) {
      final json = _map(item);
      final value = json['trangThai']?.toString().toLowerCase() ?? '';
      final AttendanceStatus status;
      if (value.contains('có phép') || value.contains('co_phep')) {
        status = AttendanceStatus.excused;
      } else if (value.contains('muộn') || value.contains('di_muon')) {
        status = AttendanceStatus.late;
      } else if (value.contains('vắng') || value.contains('vang')) {
        status = AttendanceStatus.absent;
      } else {
        status = AttendanceStatus.present;
      }
      final start = json['gioBatDau']?.toString() ?? '';
      final end = json['gioKetThuc']?.toString() ?? '';
      final shift = json['tenCa']?.toString() ?? '';
      return AttendanceRecord(
        id: json['maDiemDanh']?.toString() ?? '',
        date: _date(json['ngayHoc']),
        courseName:
            json['tenMonHoc']?.toString() ??
            json['tieuDeKhoaHoc']?.toString() ??
            '',
        status: status,
        note: [
          shift,
          if (start.isNotEmpty || end.isNotEmpty) '$start - $end',
        ].where((part) => part.isNotEmpty).join(' · '),
      );
    }).toList();
  }

  @override
  Future<List<TuitionInvoice>> getTuitionInvoices() async {
    final data = _list(await _getData('/student/tuition/invoices'));
    return data.map((item) {
      final json = _map(item);
      final status = json['trangThai']?.toString().toLowerCase() ?? '';
      return TuitionInvoice(
        id: json['maHoaDon']?.toString() ?? '',
        termName:
            json['hocKy']?.toString() ??
            json['maHoaDonCode']?.toString() ??
            'Hóa đơn học phí',
        amount: _double(json['soTienPhaiDong'] ?? json['soTien']),
        paidAmount: _double(json['daThanhToan']),
        dueDate: _date(json['hanThanhToan']),
        status: status.contains('da_thanh_toan') || status.contains('đã')
            ? InvoiceStatus.paid
            : status.contains('qua_han') || status.contains('quá hạn')
            ? InvoiceStatus.overdue
            : _double(json['daThanhToan']) > 0
            ? InvoiceStatus.partial
            : InvoiceStatus.unpaid,
      );
    }).toList();
  }

  @override
  Future<TuitionPayment> createTuitionPayment(String invoiceId) async {
    try {
      final response = await _api.post<dynamic>(
        '/student/tuition/invoices/$invoiceId/payments',
        data: const {'provider': 'payos'},
      );
      return TuitionPayment.fromJson(_map(ApiClient.unwrap(response.data)));
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<TuitionPayment> getTuitionPayment(String transactionId) async {
    final data = _map(
      await _getData('/student/tuition/payments/$transactionId'),
    );
    return TuitionPayment.fromJson(data);
  }

  @override
  Future<List<StudentNotification>> getNotifications() async {
    final page = _map(
      await _getData('/notifications', query: {'pageSize': 100}),
    );
    return _list(page['items']).map((item) {
      final json = _map(item);
      return StudentNotification(
        id: json['maThongBao']?.toString() ?? '',
        title: json['tieuDe']?.toString() ?? '',
        content:
            json['tomTatNoiDung']?.toString() ??
            json['tomTat']?.toString() ??
            '',
        timestamp: _date(json['nhanLuc'] ?? json['ngayTao']),
        isRead: json['daDoc'] == true,
        type: _notificationType(json['loaiThongBao']?.toString()),
      );
    }).toList();
  }

  NotificationType _notificationType(String? value) {
    final text = value?.toLowerCase() ?? '';
    if (text.contains('học phí') || text.contains('tuition')) {
      return NotificationType.tuition;
    }
    if (text.contains('điểm') || text.contains('grade')) {
      return NotificationType.grade;
    }
    if (text.contains('học') || text.contains('academic')) {
      return NotificationType.academic;
    }
    return NotificationType.general;
  }

  @override
  Future<void> markNotificationAsRead(String id) async {
    try {
      await _api.patch<dynamic>('/notifications/$id/read');
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }
}
