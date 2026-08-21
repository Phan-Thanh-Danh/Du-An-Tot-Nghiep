import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_status_badge.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';
import 'package:lms_mobile/features/parent/presentation/widgets/child_switcher.dart';

class ParentExamScheduleScreen extends ConsumerStatefulWidget {
  const ParentExamScheduleScreen({super.key});

  @override
  ConsumerState<ParentExamScheduleScreen> createState() =>
      _ParentExamScheduleScreenState();
}

class _ParentExamScheduleScreenState
    extends ConsumerState<ParentExamScheduleScreen> {
  List<ExamSchedule> _exams = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadExams();
  }

  Future<void> _loadExams() async {
    final activeChild = await ref.read(activeChildProvider.future);
    if (activeChild == null) {
      setState(() => _isLoading = false);
      return;
    }
    final repo = ref.read(activeParentRepoProvider);
    final data = await repo.getChildExams(activeChild.id);
    setState(() {
      _exams = data;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(activeChildIdProvider, (previous, next) {
      if (next != null) {
        setState(() => _isLoading = true);
        _loadExams();
      }
    });

    final activeChildVal = ref.watch(activeChildProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Lịch thi của con'), centerTitle: true),
      body: Column(
        children: [
          const ChildSwitcher(),
          Expanded(
            child: activeChildVal.when(
              loading: () => const AppLoadingSkeleton(itemCount: 3),
              error: (err, stack) => AppErrorState(message: err.toString()),
              data: (child) {
                if (child == null) {
                  return const AppEmptyState(
                    title: 'Trống',
                    description: 'Vui lòng liên kết tài khoản con em.',
                  );
                }

                if (_isLoading) {
                  return const AppLoadingSkeleton(itemCount: 3);
                }

                _exams.sort((a, b) => a.examDate.compareTo(b.examDate));

                if (_exams.isEmpty) {
                  return const AppEmptyState(
                    title: 'Chưa có lịch thi',
                    description:
                        'Nhà trường chưa cập nhật ca thi mới cho học sinh này.',
                    icon: Icons.calendar_month_rounded,
                  );
                }

                return ListView.separated(
                  padding: const EdgeInsets.all(16),
                  itemCount: _exams.length,
                  separatorBuilder: (context, index) =>
                      const SizedBox(height: 16),
                  itemBuilder: (context, index) {
                    final exam = _exams[index];
                    final isUpcoming = exam.status == ExamStatus.upcoming;
                    final daysLeft = exam.examDate
                        .difference(DateTime.now())
                        .inDays;

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
                                    label: 'Đã thi xong',
                                    type: BadgeType.success,
                                  )
                                else
                                  const AppStatusBadge(
                                    label: 'Lên lịch',
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
                              'Số báo danh / Ghế',
                              exam.seatNumber,
                            ),
                            _buildDetailRow(
                              Icons.description_outlined,
                              'Hình thức',
                              _getFormatLabel(exam.format),
                              valueColor: AppColors.success,
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                );
              },
            ),
          ),
        ],
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
