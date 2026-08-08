import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../finance/models/tuition_payment.dart';
import '../models/student_models.dart';

abstract class StudentRepository {
  Future<StudentProfile> getProfile();
  Future<void> updateProfile(String phone, String email);
  Future<void> changePassword(String currentPassword, String newPassword);
  Future<List<Course>> getCourses();
  Future<List<Lesson>> getCourseLessons(String courseId);
  Future<List<Assignment>> getAssignments();
  Future<void> submitAssignment(String assignmentId, String filePath);
  Future<List<ExamSchedule>> getExamSchedules();
  Future<List<GradeRecord>> getGradeRecords();
  Future<List<SemesterGPA>> getSemesterGPAs();
  Future<List<ScheduleEvent>> getScheduleEvents();
  Future<List<AttendanceRecord>> getAttendanceRecords();
  Future<List<TuitionInvoice>> getTuitionInvoices();
  Future<TuitionPayment> createTuitionPayment(String invoiceId);
  Future<TuitionPayment> getTuitionPayment(String transactionId);
  Future<List<StudentNotification>> getNotifications();
  Future<void> markNotificationAsRead(String id);
}

// Global provider for the repository
final studentRepositoryProvider = Provider<StudentRepository>((ref) {
  // Overridden in bootstrap or mock setup
  throw UnimplementedError(
    'studentRepositoryProvider has not been initialized',
  );
});
