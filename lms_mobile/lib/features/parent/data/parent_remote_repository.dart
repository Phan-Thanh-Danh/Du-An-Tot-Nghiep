import '../../../core/network/api_client.dart';
import '../../finance/models/tuition_payment.dart';
import '../../student/models/student_models.dart';
import '../models/parent_models.dart';
import 'parent_repository.dart';

class ParentRemoteRepository implements ParentRepository {
  final ApiClient _api;

  ParentRemoteRepository(this._api);

  Map<String, dynamic> _map(dynamic value) =>
      value is Map<String, dynamic> ? value : <String, dynamic>{};
  List<dynamic> _list(dynamic value) => value is List ? value : const [];
  double _double(dynamic value) =>
      value is num ? value.toDouble() : double.tryParse('$value') ?? 0;
  int _int(dynamic value) =>
      value is num ? value.toInt() : int.tryParse('$value') ?? 0;
  DateTime _date(dynamic value) =>
      DateTime.tryParse(value?.toString() ?? '') ?? DateTime.now();

  Future<dynamic> _getData(String path) async {
    try {
      final response = await _api.get<dynamic>(path);
      return ApiClient.unwrap(response.data);
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<ParentProfile> getProfile() async {
    final json = _map(await _getData('/account/me'));
    return ParentProfile(
      id: json['id']?.toString() ?? '',
      fullName: json['hoTen']?.toString() ?? 'Phụ huynh',
      email: json['email']?.toString() ?? '',
      phone: json['soDienThoai']?.toString() ?? '',
      avatarUrl: '',
      address: '',
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
  Future<List<Child>> getChildren() async {
    final summaries = _list(await _getData('/parent/children'));
    return Future.wait(
      summaries.map((item) async {
        final summary = _map(item);
        final id = summary['id']?.toString() ?? '';
        final detail = _map(await _getData('/parent/children/$id'));
        final grades = await getChildGrades(id);
        final graded = grades
            .where((grade) => grade.totalScore != null)
            .toList();
        final attendance = await getChildAttendance(id);
        final tuition = await getChildTuition(id);
        final present = attendance
            .where((record) => record.status != AttendanceStatus.absent)
            .length;
        final rate = attendance.isEmpty ? 0.0 : present / attendance.length;
        return Child(
          id: id,
          fullName:
              detail['name']?.toString() ?? summary['name']?.toString() ?? '',
          code: id,
          classCode:
              detail['className']?.toString() ??
              summary['className']?.toString() ??
              '',
          department: summary['campus']?.toString() ?? '',
          major: detail['major']?.toString() ?? '',
          avatarUrl: '',
          maDonVi: detail['campus']?.toString() ?? '',
          currentGpa: graded.isEmpty
              ? _double(detail['gpa'])
              : graded.fold<double>(
                      0,
                      (sum, grade) => sum + grade.totalScore!,
                    ) /
                    graded.length,
          attendanceRate: rate,
          tuitionOwed: tuition.fold(
            0,
            (sum, invoice) => sum + invoice.amount - invoice.paidAmount,
          ),
        );
      }),
    );
  }

  @override
  Future<List<GradeRecord>> getChildGrades(String childId) async {
    final data = _list(await _getData('/parent/children/$childId/grades'));
    return data.asMap().entries.map((entry) {
      final json = _map(entry.value);
      final totalScore = json['total'] == null ? null : _double(json['total']);
      return GradeRecord(
        id: '${json['code'] ?? 'grade'}-${entry.key}',
        courseCode: json['code']?.toString() ?? '',
        courseName: json['subject']?.toString() ?? '',
        creditCount: 0,
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
        letterGrade: totalScore == null
            ? null
            : totalScore >= 5
            ? 'Đạt'
            : 'Nợ môn',
        semester: json['semester']?.toString() ?? 'Chưa xác định học kỳ',
      );
    }).toList();
  }

  @override
  Future<List<SemesterGPA>> getChildSemesterGPAs(String childId) async {
    final grades = await getChildGrades(childId);
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
        creditCount: entry.value.fold(
          0,
          (sum, grade) => sum + grade.creditCount,
        ),
      );
    }).toList();
  }

  DateTime _dateForWeekday(int weekday) {
    final now = DateTime.now();
    final monday = DateTime(
      now.year,
      now.month,
      now.day,
    ).subtract(Duration(days: now.weekday - 1));
    return monday.add(Duration(days: (weekday.clamp(2, 8) - 2) % 7));
  }

  @override
  Future<List<ScheduleEvent>> getChildSchedule(String childId) async {
    final data = _list(await _getData('/parent/children/$childId/schedule'));
    return data.asMap().entries.map((entry) {
      final json = _map(entry.value);
      final time = json['time']?.toString() ?? '';
      final timeParts = time.split('-');
      return ScheduleEvent(
        id: 'parent-schedule-${entry.key}',
        courseName: json['subject']?.toString() ?? '',
        room: json['room']?.toString() ?? '',
        teacherName: json['teacher']?.toString() ?? '',
        date: _dateForWeekday(_int(json['day'])),
        startTime: timeParts.first.trim(),
        endTime: timeParts.length > 1 ? timeParts.last.trim() : '',
      );
    }).toList();
  }

  @override
  Future<List<ExamSchedule>> getChildExams(String childId) {
    throw const ApiUnavailableException(
      'Backend chưa có API lịch thi của con em dành cho phụ huynh.',
    );
  }

  @override
  Future<List<AttendanceRecord>> getChildAttendance(String childId) async {
    final data = _list(await _getData('/parent/children/$childId/attendance'));
    return data.asMap().entries.map((entry) {
      final json = _map(entry.value);
      final statusText = json['status']?.toString().toLowerCase() ?? '';
      final status = switch (statusText) {
        'vang' || 'absent' => AttendanceStatus.absent,
        'muon' || 'di_muon' || 'late' => AttendanceStatus.late,
        'co_phep' || 'excused' => AttendanceStatus.excused,
        _ => AttendanceStatus.present,
      };
      return AttendanceRecord(
        id: '${json['courseId'] ?? 'attendance'}-${entry.key}',
        date: _date(json['date']),
        courseName: json['subject']?.toString() ?? '',
        status: status,
        note: json['note']?.toString() ?? '',
      );
    }).toList();
  }

  @override
  Future<List<TuitionInvoice>> getChildTuition(String childId) async {
    final data = _list(await _getData('/parent/children/$childId/invoices'));
    return data.map((item) {
      final json = _map(item);
      final status = json['status']?.toString().toLowerCase() ?? '';
      final paid = _double(json['paidAmount']);
      final amount = _double(json['amount']) - _double(json['discountAmount']);
      return TuitionInvoice(
        id: json['id']?.toString() ?? '',
        termName:
            json['title']?.toString() ??
            json['invoiceCode']?.toString() ??
            'Hóa đơn học phí',
        amount: amount,
        paidAmount: paid,
        dueDate: _date(json['dueDate']),
        status: status.contains('da_thanh_toan') || paid >= amount
            ? InvoiceStatus.paid
            : status.contains('qua_han') || status.contains('quá hạn')
            ? InvoiceStatus.overdue
            : paid > 0
            ? InvoiceStatus.partial
            : InvoiceStatus.unpaid,
      );
    }).toList();
  }

  @override
  Future<TuitionPayment> createChildTuitionPayment(
    String childId,
    String invoiceId,
  ) async {
    try {
      final response = await _api.post<dynamic>(
        '/parent/children/$childId/invoices/$invoiceId/payments',
      );
      return TuitionPayment.fromJson(_map(ApiClient.unwrap(response.data)));
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }

  @override
  Future<TuitionPayment> getChildTuitionPayment(
    String childId,
    String transactionId,
  ) async {
    final data = _map(
      await _getData('/parent/children/$childId/payments/$transactionId'),
    );
    return TuitionPayment.fromJson(data);
  }

  @override
  Future<List<StudentNotification>> getNotifications() async {
    final data = _list(await _getData('/parent/notifications'));
    return data.map((item) {
      final json = _map(item);
      return StudentNotification(
        id: json['id']?.toString() ?? '',
        title: json['title']?.toString() ?? '',
        content: json['content']?.toString() ?? '',
        timestamp: _date(json['createdAt']),
        isRead: json['isRead'] == true,
        type: NotificationType.general,
      );
    }).toList();
  }

  @override
  Future<void> markNotificationAsRead(String id) async {
    try {
      await _api.post<dynamic>('/parent/notifications/$id/read');
    } catch (error) {
      throw ApiClient.readableError(error);
    }
  }
}
