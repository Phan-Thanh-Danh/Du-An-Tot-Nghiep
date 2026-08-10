import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../finance/models/tuition_payment.dart';
import '../models/parent_models.dart';
import '../../student/models/student_models.dart';

abstract class ParentRepository {
  Future<ParentProfile> getProfile();
  Future<void> updateProfile(String phone, String email);
  Future<void> changePassword(String currentPassword, String newPassword);
  Future<List<Child>> getChildren();
  Future<List<GradeRecord>> getChildGrades(String childId);
  Future<List<SemesterGPA>> getChildSemesterGPAs(String childId);
  Future<List<ScheduleEvent>> getChildSchedule(String childId);
  Future<List<ExamSchedule>> getChildExams(String childId);
  Future<List<AttendanceRecord>> getChildAttendance(String childId);
  Future<List<TuitionInvoice>> getChildTuition(String childId);
  Future<TuitionPayment> createChildTuitionPayment(
    String childId,
    String invoiceId,
  );
  Future<TuitionPayment> getChildTuitionPayment(
    String childId,
    String transactionId,
  );
  Future<List<StudentNotification>> getNotifications();
  Future<void> markNotificationAsRead(String id);
}

// Global provider for parent repository
final parentRepositoryProvider = Provider<ParentRepository>((ref) {
  throw UnimplementedError('parentRepositoryProvider has not been initialized');
});
