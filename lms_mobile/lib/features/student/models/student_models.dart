enum LessonType { video, pdf, text }

enum AssignmentStatus { notSubmitted, submitted, overdue }

enum ExamFormat { multipleChoice, writing, essay, practice }

enum ExamStatus { upcoming, completed }

enum AttendanceStatus { present, absent, late, excused }

enum InvoiceStatus { paid, unpaid, partial, overdue }

enum NotificationType { system, academic, tuition, grade, general }

class Course {
  final String id;
  final String code;
  final String name;
  final String teacherName;
  final int creditCount;
  final double progress; // between 0.0 and 1.0
  final String bannerUrl;

  const Course({
    required this.id,
    required this.code,
    required this.name,
    required this.teacherName,
    required this.creditCount,
    required this.progress,
    required this.bannerUrl,
  });
}

class Lesson {
  final String id;
  final String title;
  final String content;
  final LessonType type;
  final int durationMinutes;
  final String resourceUrl;
  final bool isCompleted;

  const Lesson({
    required this.id,
    required this.title,
    required this.content,
    required this.type,
    required this.durationMinutes,
    required this.resourceUrl,
    required this.isCompleted,
  });
}

class Assignment {
  final String id;
  final String title;
  final String description;
  final String courseName;
  final DateTime dueDate;
  final DateTime? submitDate;
  final double? score;
  final AssignmentStatus status;

  const Assignment({
    required this.id,
    required this.title,
    required this.description,
    required this.courseName,
    required this.dueDate,
    this.submitDate,
    this.score,
    required this.status,
  });
}

class ExamSchedule {
  final String id;
  final String courseName;
  final DateTime examDate;
  final String startTime;
  final String endTime;
  final String room;
  final String seatNumber;
  final ExamFormat format;
  final ExamStatus status;

  const ExamSchedule({
    required this.id,
    required this.courseName,
    required this.examDate,
    required this.startTime,
    required this.endTime,
    required this.room,
    required this.seatNumber,
    required this.format,
    required this.status,
  });
}

class GradeRecord {
  final String id;
  final String courseCode;
  final String courseName;
  final int creditCount;
  final double? processScore;
  final double? midtermScore;
  final double? finalScore;
  final double? totalScore;
  final String? letterGrade;
  final String semester;

  const GradeRecord({
    required this.id,
    required this.courseCode,
    required this.courseName,
    required this.creditCount,
    this.processScore,
    this.midtermScore,
    this.finalScore,
    this.totalScore,
    this.letterGrade,
    required this.semester,
  });
}

class SemesterGPA {
  final String semester;
  final double gpa;
  final double gpa4;
  final int creditCount;

  const SemesterGPA({
    required this.semester,
    required this.gpa,
    required this.gpa4,
    required this.creditCount,
  });
}

class ScheduleEvent {
  final String id;
  final String courseName;
  final String room;
  final String teacherName;
  final DateTime date;
  final String startTime;
  final String endTime;

  const ScheduleEvent({
    required this.id,
    required this.courseName,
    required this.room,
    required this.teacherName,
    required this.date,
    required this.startTime,
    required this.endTime,
  });
}

class AttendanceRecord {
  final String id;
  final DateTime date;
  final String courseName;
  final AttendanceStatus status;
  final String note;

  const AttendanceRecord({
    required this.id,
    required this.date,
    required this.courseName,
    required this.status,
    required this.note,
  });
}

class TuitionInvoice {
  final String id;
  final String termName;
  final double amount;
  final double paidAmount;
  final DateTime dueDate;
  final InvoiceStatus status;
  final DateTime? paymentDate;

  const TuitionInvoice({
    required this.id,
    required this.termName,
    required this.amount,
    required this.paidAmount,
    required this.dueDate,
    required this.status,
    this.paymentDate,
  });
}

class StudentProfile {
  final String id;
  final String code;
  final String fullName;
  final String email;
  final String phone;
  final String avatarUrl;
  final String department;
  final String major;
  final String classCode;
  final String maDonVi;

  const StudentProfile({
    required this.id,
    required this.code,
    required this.fullName,
    required this.email,
    required this.phone,
    required this.avatarUrl,
    required this.department,
    required this.major,
    required this.classCode,
    required this.maDonVi,
  });
}

class StudentNotification {
  final String id;
  final String title;
  final String content;
  final DateTime timestamp;
  final bool isRead;
  final NotificationType type;

  const StudentNotification({
    required this.id,
    required this.title,
    required this.content,
    required this.timestamp,
    required this.isRead,
    required this.type,
  });
}
