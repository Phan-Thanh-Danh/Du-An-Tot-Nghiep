import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/core/widgets/app_stat_card.dart';
import 'package:lms_mobile/core/widgets/app_section_header.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentDashboardScreen extends ConsumerWidget {
  const StudentDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final repo = ref.watch(activeStudentRepoProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Row(
          children: [
            FutureBuilder(
              future: repo.getProfile(),
              builder: (context, snapshot) {
                if (snapshot.hasData) {
                  final avatarUrl = snapshot.data!.avatarUrl;
                  return CircleAvatar(
                    radius: 20,
                    backgroundColor: AppColors.primaryLight,
                    backgroundImage: avatarUrl.isEmpty
                        ? null
                        : NetworkImage(avatarUrl),
                    child: avatarUrl.isEmpty
                        ? const Icon(Icons.person, color: AppColors.primary)
                        : null,
                  );
                }
                return const CircleAvatar(
                  radius: 20,
                  backgroundColor: AppColors.border,
                  child: Icon(Icons.person, color: AppColors.textSecondary),
                );
              },
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  FutureBuilder(
                    future: repo.getProfile(),
                    builder: (context, snapshot) {
                      final name = snapshot.data?.fullName ?? 'Sinh viên';
                      return Text(
                        'Xin chào, $name!',
                        style: AppTextStyles.bodyMedium.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                      );
                    },
                  ),
                  Text(
                    'Chào mừng bạn quay lại học tập',
                    style: AppTextStyles.caption,
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications_none_rounded),
            onPressed: () => context.push('/student/notifications'),
          ),
          IconButton(
            icon: const Icon(Icons.logout_rounded, color: AppColors.error),
            onPressed: () {
              ref.read(authProvider.notifier).logout();
              context.go('/login');
            },
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          // In real app, this would refresh the providers
          await Future.delayed(const Duration(milliseconds: 500));
        },
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // KPIs row
              AppSectionHeader(title: 'Tổng quan học tập'),
              GridView.count(
                crossAxisCount: 2,
                crossAxisSpacing: 12,
                mainAxisSpacing: 12,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                childAspectRatio: 1.6,
                children: [
                  FutureBuilder(
                    future: repo.getSemesterGPAs(),
                    builder: (context, snapshot) {
                      final semesters = snapshot.data ?? const [];
                      final latest = semesters.isEmpty ? null : semesters.last;
                      return AppStatCard(
                        title: 'GPA tích lũy',
                        value: latest == null
                            ? '--'
                            : '${formatScore(latest.gpa)} / 10',
                        subtitle: latest == null
                            ? 'Chưa có dữ liệu điểm'
                            : 'Hệ 4.0: ${formatScore(latest.gpa4)}',
                        icon: Icons.star_rounded,
                        iconColor: AppColors.warning,
                        iconBackgroundColor: AppColors.warningLight,
                        onTap: () => context.push('/student/grades'),
                      );
                    },
                  ),
                  FutureBuilder(
                    future: repo.getGradeRecords(),
                    builder: (context, snapshot) {
                      final grades = snapshot.data ?? const [];
                      final credits = grades.fold<int>(
                        0,
                        (sum, grade) => sum + grade.creditCount,
                      );
                      return AppStatCard(
                        title: 'Số tín chỉ',
                        value: '$credits TC',
                        subtitle: grades.isEmpty
                            ? 'Chưa có dữ liệu'
                            : '${grades.length} môn học',
                        icon: Icons.menu_book_rounded,
                        iconColor: AppColors.primary,
                        iconBackgroundColor: AppColors.primaryLight,
                        onTap: () => context.push('/student/courses'),
                      );
                    },
                  ),
                  AppStatCard(
                    title: 'Chuyên cần',
                    value: '--',
                    subtitle: 'Backend chưa có API',
                    icon: Icons.check_circle_rounded,
                    iconColor: AppColors.success,
                    iconBackgroundColor: AppColors.successLight,
                    onTap: () => context.push('/student/attendance'),
                  ),
                  FutureBuilder(
                    future: repo.getTuitionInvoices(),
                    builder: (context, snapshot) {
                      final invoices = snapshot.data ?? const [];
                      final debt = invoices.fold<double>(
                        0,
                        (sum, invoice) =>
                            sum + invoice.amount - invoice.paidAmount,
                      );
                      return AppStatCard(
                        title: 'Học phí còn nợ',
                        value: NumberFormat.compactCurrency(
                          locale: 'vi_VN',
                          symbol: 'đ',
                        ).format(debt),
                        subtitle: invoices.isEmpty
                            ? 'Không có hóa đơn'
                            : '${invoices.length} hóa đơn',
                        icon: Icons.account_balance_wallet_rounded,
                        iconColor: AppColors.error,
                        iconBackgroundColor: AppColors.errorLight,
                        onTap: () => context.push('/student/tuition'),
                      );
                    },
                  ),
                ],
              ),
              const SizedBox(height: 16),

              // Today's classes
              AppSectionHeader(
                title: 'Lịch học hôm nay',
                actionLabel: 'Xem tất cả',
                onActionPressed: () => context.push('/student/schedule'),
              ),
              FutureBuilder(
                future: repo.getScheduleEvents(),
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const AppLoadingSkeleton(itemCount: 1);
                  }
                  final todayEvents = snapshot.data ?? [];
                  if (todayEvents.isEmpty) {
                    return Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Center(
                          child: Text(
                            'Hôm nay không có lịch học',
                            style: AppTextStyles.bodyRegular,
                          ),
                        ),
                      ),
                    );
                  }

                  final event = todayEvents.first;
                  return Card(
                    margin: EdgeInsets.zero,
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: AppColors.primaryLight,
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: const Icon(
                              Icons.class_rounded,
                              color: AppColors.primary,
                            ),
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  event.courseName,
                                  style: AppTextStyles.bodyMedium.copyWith(
                                    fontWeight: FontWeight.w700,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Row(
                                  children: [
                                    const Icon(
                                      Icons.access_time_rounded,
                                      size: 14,
                                      color: AppColors.textSecondary,
                                    ),
                                    const SizedBox(width: 4),
                                    Text(
                                      '${event.startTime} - ${event.endTime}',
                                      style: AppTextStyles.caption,
                                    ),
                                    const SizedBox(width: 12),
                                    const Icon(
                                      Icons.room_rounded,
                                      size: 14,
                                      color: AppColors.textSecondary,
                                    ),
                                    const SizedBox(width: 4),
                                    Text(
                                      event.room,
                                      style: AppTextStyles.caption,
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
              const SizedBox(height: 16),

              // Upcoming assignments
              AppSectionHeader(
                title: 'Bài tập sắp hạn nộp',
                actionLabel: 'Xem tất cả',
                onActionPressed: () => context.push('/student/assignments'),
              ),
              FutureBuilder(
                future: repo.getAssignments(),
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const AppLoadingSkeleton(itemCount: 1);
                  }
                  final assignments = snapshot.data ?? [];
                  final pending = assignments
                      .where((a) => a.status == AssignmentStatus.notSubmitted)
                      .toList();
                  if (pending.isEmpty) {
                    return Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Center(
                          child: Text(
                            'Tuyệt vời! Không có bài tập sắp hạn.',
                            style: AppTextStyles.bodyRegular,
                          ),
                        ),
                      ),
                    );
                  }

                  final assignment = pending.first;
                  final daysLeft = assignment.dueDate
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
                                  assignment.title,
                                  style: AppTextStyles.bodyMedium.copyWith(
                                    fontWeight: FontWeight.w700,
                                  ),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 8,
                                  vertical: 4,
                                ),
                                decoration: BoxDecoration(
                                  color: AppColors.errorLight,
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  'Còn $daysLeft ngày',
                                  style: AppTextStyles.caption.copyWith(
                                    color: AppColors.error,
                                    fontWeight: FontWeight.w700,
                                    fontSize: 10,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 6),
                          Text(
                            assignment.courseName,
                            style: AppTextStyles.caption,
                          ),
                          const SizedBox(height: 12),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Hạn nộp: ${DateFormat('dd/MM/yyyy HH:mm').format(assignment.dueDate)}',
                                style: AppTextStyles.caption.copyWith(
                                  color: AppColors.textSecondary,
                                ),
                              ),
                              TextButton(
                                onPressed: () =>
                                    context.push('/student/assignments'),
                                style: TextButton.styleFrom(
                                  minimumSize: Size.zero,
                                  padding: EdgeInsets.zero,
                                  tapTargetSize:
                                      MaterialTapTargetSize.shrinkWrap,
                                ),
                                child: Text(
                                  'Chi tiết',
                                  style: AppTextStyles.caption.copyWith(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}
