import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_status_badge.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentAttendanceScreen extends ConsumerWidget {
  const StudentAttendanceScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final repo = ref.watch(activeStudentRepoProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Điểm danh chuyên cần'),
        centerTitle: true,
      ),
      body: FutureBuilder<List<AttendanceRecord>>(
        future: repo.getAttendanceRecords(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoadingSkeleton(itemCount: 4);
          }
          if (snapshot.hasError) {
            return AppErrorState(message: snapshot.error.toString());
          }
          final records = snapshot.data ?? [];
          if (records.isEmpty) {
            return const AppEmptyState(
              title: 'Không có dữ liệu',
              description: 'Chưa ghi nhận thông tin chuyên cần nào của bạn.',
              icon: Icons.checklist_rounded,
            );
          }

          // Calculate stats
          final presentCount = records
              .where((r) => r.status == AttendanceStatus.present)
              .length;
          final absentCount = records
              .where((r) => r.status == AttendanceStatus.absent)
              .length;
          final lateCount = records
              .where((r) => r.status == AttendanceStatus.late)
              .length;
          final excusedCount = records
              .where((r) => r.status == AttendanceStatus.excused)
              .length;
          final totalCount = records.length;
          final attendanceRate = totalCount > 0
              ? ((presentCount + excusedCount + lateCount) / totalCount * 100)
                    .toInt()
              : 100;

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Stats Card
                Card(
                  margin: EdgeInsets.zero,
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Tỷ lệ chuyên cần',
                              style: AppTextStyles.bodyMedium.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            Text(
                              '$attendanceRate%',
                              style: AppTextStyles.subtitle.copyWith(
                                color: attendanceRate >= 80
                                    ? AppColors.success
                                    : AppColors.error,
                                fontSize: 20,
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        const Divider(height: 1),
                        const SizedBox(height: 16),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildStatItem(
                              'Có mặt',
                              presentCount,
                              AppColors.success,
                            ),
                            _buildStatItem(
                              'Muộn',
                              lateCount,
                              AppColors.warning,
                            ),
                            _buildStatItem(
                              'Có phép',
                              excusedCount,
                              AppColors.info,
                            ),
                            _buildStatItem(
                              'Vắng mặt',
                              absentCount,
                              AppColors.error,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Record List Title
                Text(
                  'Lịch sử điểm danh',
                  style: AppTextStyles.subtitle.copyWith(fontSize: 16),
                ),
                const SizedBox(height: 12),

                // Record list
                ListView.separated(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: records.length,
                  separatorBuilder: (context, index) =>
                      const SizedBox(height: 12),
                  itemBuilder: (context, index) {
                    final r = records[index];
                    return Card(
                      margin: EdgeInsets.zero,
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    r.courseName,
                                    style: AppTextStyles.bodyMedium.copyWith(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 15,
                                    ),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    DateFormat('dd/MM/yyyy').format(r.date),
                                    style: AppTextStyles.caption,
                                  ),
                                  if (r.note.isNotEmpty) ...[
                                    const SizedBox(height: 6),
                                    Text(
                                      'Ghi chú: ${r.note}',
                                      style: AppTextStyles.caption.copyWith(
                                        fontStyle: FontStyle.italic,
                                        color: AppColors.textSecondary,
                                      ),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                            const SizedBox(width: 12),
                            _buildStatusBadge(r.status),
                          ],
                        ),
                      ),
                    );
                  },
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildStatItem(String label, int count, Color color) {
    return Column(
      children: [
        Text(label, style: AppTextStyles.caption),
        const SizedBox(height: 6),
        Text(
          count.toString(),
          style: AppTextStyles.subtitle.copyWith(
            color: color,
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }

  Widget _buildStatusBadge(AttendanceStatus status) {
    switch (status) {
      case AttendanceStatus.present:
        return const AppStatusBadge(label: 'Có mặt', type: BadgeType.success);
      case AttendanceStatus.late:
        return const AppStatusBadge(label: 'Đi muộn', type: BadgeType.warning);
      case AttendanceStatus.excused:
        return const AppStatusBadge(label: 'Có phép', type: BadgeType.info);
      case AttendanceStatus.absent:
        return const AppStatusBadge(label: 'Vắng học', type: BadgeType.error);
    }
  }
}
