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
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';
import 'package:lms_mobile/features/parent/presentation/widgets/child_switcher.dart';

class ParentDashboardScreen extends ConsumerWidget {
  const ParentDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final parentRepo = ref.watch(activeParentRepoProvider);
    final childState = ref.watch(activeChildProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Row(
          children: [
            FutureBuilder(
              future: parentRepo.getProfile(),
              builder: (context, snapshot) {
                if (snapshot.hasData) {
                  return CircleAvatar(
                    radius: 20,
                    backgroundColor: AppColors.successLight,
                    backgroundImage: snapshot.data!.avatarUrl.isEmpty
                        ? null
                        : NetworkImage(snapshot.data!.avatarUrl),
                    child: snapshot.data!.avatarUrl.isEmpty
                        ? const Icon(Icons.person, color: AppColors.success)
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
                    future: parentRepo.getProfile(),
                    builder: (context, snapshot) {
                      final name = snapshot.data?.fullName ?? 'Phụ huynh';
                      return Text(
                        'Phụ huynh: $name',
                        style: AppTextStyles.bodyMedium.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                      );
                    },
                  ),
                  Text(
                    'Giám sát học tập của con em',
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
            onPressed: () => context.push('/parent/notifications'),
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
      body: Column(
        children: [
          // Child context selector
          const ChildSwitcher(),

          Expanded(
            child: RefreshIndicator(
              onRefresh: () async {
                await Future.delayed(const Duration(milliseconds: 500));
              },
              child: childState.when(
                loading: () => const AppLoadingSkeleton(itemCount: 3),
                error: (err, stack) => AppErrorState(message: err.toString()),
                data: (child) {
                  if (child == null) {
                    return const AppEmptyState(
                      title: 'Chưa liên kết học sinh',
                      description:
                          'Vui lòng liên hệ quản trị viên để liên kết mã học sinh.',
                      icon: Icons.people_outline_rounded,
                    );
                  }

                  return SingleChildScrollView(
                    physics: const AlwaysScrollableScrollPhysics(),
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Child Academic Overview
                        AppSectionHeader(
                          title: 'Tình hình của ${child.fullName}',
                        ),
                        GridView.count(
                          crossAxisCount: 2,
                          crossAxisSpacing: 12,
                          mainAxisSpacing: 12,
                          shrinkWrap: true,
                          physics: const NeverScrollableScrollPhysics(),
                          childAspectRatio: 1.6,
                          children: [
                            AppStatCard(
                              title: 'Điểm trung bình (GPA)',
                              value: '${formatScore(child.currentGpa)} / 10',
                              subtitle: 'Kết quả học tập kỳ 1',
                              icon: Icons.emoji_events_rounded,
                              iconColor: AppColors.warning,
                              iconBackgroundColor: AppColors.warningLight,
                              onTap: () => context.push('/parent/grades'),
                            ),
                            AppStatCard(
                              title: 'Chuyên cần',
                              value: '${(child.attendanceRate * 100).toInt()}%',
                              subtitle: 'Tỷ lệ có mặt tại lớp',
                              icon: Icons.co_present_rounded,
                              iconColor: AppColors.success,
                              iconBackgroundColor: AppColors.successLight,
                              onTap: () => context.push('/parent/attendance'),
                            ),
                            AppStatCard(
                              title: 'Học phí còn nợ',
                              value: child.tuitionOwed > 0
                                  ? '${NumberFormat.compact(locale: 'vi_VN').format(child.tuitionOwed)} đ'
                                  : 'Hoàn thành',
                              subtitle: child.tuitionOwed > 0
                                  ? 'Có hóa đơn chưa nộp'
                                  : 'Không có nợ phí',
                              icon: Icons.account_balance_wallet_rounded,
                              iconColor: child.tuitionOwed > 0
                                  ? AppColors.error
                                  : AppColors.success,
                              iconBackgroundColor: child.tuitionOwed > 0
                                  ? AppColors.errorLight
                                  : AppColors.successLight,
                              onTap: () => context.push('/parent/tuition'),
                            ),
                            AppStatCard(
                              title: 'Học kỳ hiện tại',
                              value: 'Kỳ 1 (25-26)',
                              subtitle: 'Hệ chính quy',
                              icon: Icons.history_edu_rounded,
                              iconColor: AppColors.info,
                              iconBackgroundColor: AppColors.infoLight,
                              onTap: () => context.push('/parent/schedule'),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),

                        // Child's Schedule Today
                        AppSectionHeader(
                          title: 'Lịch học hôm nay',
                          actionLabel: 'Chi tiết lịch học',
                          onActionPressed: () =>
                              context.push('/parent/schedule'),
                        ),
                        FutureBuilder(
                          future: parentRepo.getChildSchedule(child.id),
                          builder: (context, snapshot) {
                            if (snapshot.connectionState ==
                                ConnectionState.waiting) {
                              return const AppLoadingSkeleton(itemCount: 1);
                            }
                            final list = snapshot.data ?? [];
                            if (list.isEmpty) {
                              return Card(
                                child: Padding(
                                  padding: const EdgeInsets.all(16.0),
                                  child: Center(
                                    child: Text(
                                      'Hôm nay con em được nghỉ',
                                      style: AppTextStyles.bodyRegular,
                                    ),
                                  ),
                                ),
                              );
                            }

                            final event = list.first;
                            return Card(
                              margin: EdgeInsets.zero,
                              child: Padding(
                                padding: const EdgeInsets.all(16.0),
                                child: Row(
                                  children: [
                                    Container(
                                      padding: const EdgeInsets.all(12),
                                      decoration: BoxDecoration(
                                        color: AppColors.successLight,
                                        borderRadius: BorderRadius.circular(12),
                                      ),
                                      child: const Icon(
                                        Icons.school_rounded,
                                        color: AppColors.success,
                                      ),
                                    ),
                                    const SizedBox(width: 16),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            event.courseName,
                                            style: AppTextStyles.bodyMedium
                                                .copyWith(
                                                  fontWeight: FontWeight.bold,
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

                        // Critical warnings/alerts
                        if (child.tuitionOwed > 0) ...[
                          Container(
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: AppColors.errorLight,
                              borderRadius: BorderRadius.circular(16),
                              border: Border.all(
                                color: AppColors.error.withValues(alpha: 0.3),
                              ),
                            ),
                            child: Row(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Icon(
                                  Icons.warning_amber_rounded,
                                  color: AppColors.error,
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        'Cảnh báo học phí',
                                        style: AppTextStyles.bodyMedium
                                            .copyWith(
                                              fontWeight: FontWeight.bold,
                                              color: AppColors.error,
                                            ),
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        'Sinh viên ${child.fullName} còn dư nợ học phí học kỳ này. Vui lòng hoàn thành trước hạn chót để tránh ảnh hưởng lịch thi.',
                                        style: AppTextStyles.caption.copyWith(
                                          color: AppColors.error,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(height: 16),
                        ],
                      ],
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}
