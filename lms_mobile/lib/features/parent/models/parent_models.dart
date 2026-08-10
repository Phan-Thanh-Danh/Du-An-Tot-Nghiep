class Child {
  final String id;
  final String fullName;
  final String code;
  final String classCode;
  final String department;
  final String major;
  final String avatarUrl;
  final String maDonVi;
  final double currentGpa;
  final double attendanceRate; // e.g. 0.95 (95%)
  final double tuitionOwed;

  const Child({
    required this.id,
    required this.fullName,
    required this.code,
    required this.classCode,
    required this.department,
    required this.major,
    required this.avatarUrl,
    required this.maDonVi,
    required this.currentGpa,
    required this.attendanceRate,
    required this.tuitionOwed,
  });
}

class ParentProfile {
  final String id;
  final String fullName;
  final String email;
  final String phone;
  final String avatarUrl;
  final String address;

  const ParentProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.phone,
    required this.avatarUrl,
    required this.address,
  });
}
