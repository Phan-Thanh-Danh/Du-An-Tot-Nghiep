import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_status_badge.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentExamScheduleScreen extends ConsumerWidget {
  const StudentExamScheduleScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final repo = ref.watch(activeStudentRepoProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Lịch thi học kỳ'), centerTitle: true),
      body: FutureBuilder<List<ExamSchedule>>(
        future: repo.getExamSchedules(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoadingSkeleton(itemCount: 3);
          }
          if (snapshot.hasError) {
            return AppErrorState(message: snapshot.error.toString());
          }
          final exams = snapshot.data ?? [];
          if (exams.isEmpty) {
            return const AppEmptyState(
              title: 'Lịch thi trống',
              description: 'Chưa cập nhật lịch thi mới cho học kỳ này.',
              icon: Icons.calendar_month_rounded,
            );
          }

          // Sort exams by date
          exams.sort((a, b) => a.examDate.compareTo(b.examDate));

          return ListView.separated(
            padding: const EdgeInsets.all(16),
            itemCount: exams.length,
            separatorBuilder: (context, index) => const SizedBox(height: 16),
            itemBuilder: (context, index) {
              final exam = exams[index];
              final isUpcoming = exam.status == ExamStatus.upcoming;
              final daysLeft = exam.examDate.difference(DateTime.now()).inDays;

              return Card(
                margin: EdgeInsets.zero,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Expanded(
                            child: Text(
                              exam.courseName,
                              style: AppTextStyles.bodyMedium.copyWith(
                                fontWeight: FontWeight.bold,
                                fontSize: 16,
                              ),
                            ),
                          ),
                          if (isUpcoming && daysLeft >= 0)
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: AppColors.warningLight,
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Text(
                                'Còn $daysLeft ngày',
                                style: AppTextStyles.caption.copyWith(
                                  color: AppColors.warning,
                                  fontWeight: FontWeight.bold,
                                  fontSize: 10,
                                ),
                              ),
                            )
                          else if (!isUpcoming)
                            const AppStatusBadge(
                              label: 'Đã hoàn thành',
                              type: BadgeType.success,
                            )
                          else
                            const AppStatusBadge(
                              label: 'Sắp thi',
                              type: BadgeType.info,
                            ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      const Divider(height: 1),
                      const SizedBox(height: 12),
                      _buildDetailRow(
                        Icons.calendar_today_rounded,
                        'Ngày thi',
                        DateFormat('dd/MM/yyyy').format(exam.examDate),
                      ),
                      _buildDetailRow(
                        Icons.access_time_rounded,
                        'Giờ thi',
                        '${exam.startTime} - ${exam.endTime}',
                      ),
                      _buildDetailRow(
                        Icons.room_rounded,
                        'Phòng thi',
                        exam.room,
                      ),
                      _buildDetailRow(
                        Icons.event_seat_rounded,
                        'Số báo danh / Số ghế',
                        exam.seatNumber,
                      ),
                      _buildDetailRow(
                        Icons.description_outlined,
                        'Hình thức thi',
                        _getFormatLabel(exam.format),
                        valueColor: AppColors.primary,
                      ),
                    ],
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }

  Widget _buildDetailRow(
    IconData icon,
    String label,
    String value, {
    Color? valueColor,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6.0),
      child: Row(
        children: [
          Icon(icon, size: 16, color: AppColors.textSecondary),
          const SizedBox(width: 10),
          Text(
            '$label:',
            style: AppTextStyles.bodyRegular.copyWith(fontSize: 13),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              value,
              style: AppTextStyles.bodyMedium.copyWith(
                fontWeight: FontWeight.w600,
                fontSize: 13,
                color: valueColor ?? AppColors.textPrimary,
              ),
              textAlign: TextAlign.end,
              softWrap: true,
            ),
          ),
        ],
      ),
    );
  }

  String _getFormatLabel(ExamFormat format) {
    switch (format) {
      case ExamFormat.multipleChoice:
        return 'Trắc nghiệm';
      case ExamFormat.writing:
        return 'Tự luận';
      case ExamFormat.essay:
        return 'Tiểu luận';
      case ExamFormat.practice:
        return 'Thực hành máy';
    }
  }
}
