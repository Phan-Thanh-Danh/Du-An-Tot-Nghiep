import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';

class ParentChildrenScreen extends ConsumerWidget {
  const ParentChildrenScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final parentRepo = ref.watch(activeParentRepoProvider);
    final activeId = ref.watch(activeChildIdProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Con em liên kết'), centerTitle: true),
      body: FutureBuilder(
        future: parentRepo.getChildren(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoadingSkeleton(itemCount: 2);
          }
          if (snapshot.hasError) {
            return AppErrorState(message: snapshot.error.toString());
          }
          final list = snapshot.data ?? [];
          if (list.isEmpty) {
            return const AppEmptyState(
              title: 'Không tìm thấy dữ liệu con em',
              description:
                  'Chưa có tài khoản học sinh nào được liên kết với số điện thoại của bạn.',
              icon: Icons.people_outline_rounded,
            );
          }

          return ListView.separated(
            padding: const EdgeInsets.all(16),
            itemCount: list.length,
            separatorBuilder: (context, index) => const SizedBox(height: 16),
            itemBuilder: (context, index) {
              final child = list[index];
              final isCurrentActive = child.id == activeId;

              return Card(
                margin: EdgeInsets.zero,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                  side: BorderSide(
                    color: isCurrentActive
                        ? AppColors.success
                        : AppColors.border,
                    width: isCurrentActive ? 1.5 : 1,
                  ),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    children: [
                      Row(
                        children: [
                          CircleAvatar(
                            radius: 32,
                            backgroundColor: AppColors.successLight,
                            backgroundImage: child.avatarUrl.isEmpty
                                ? null
                                : NetworkImage(child.avatarUrl),
                            child: child.avatarUrl.isEmpty
                                ? const Icon(
                                    Icons.person,
                                    color: AppColors.success,
                                  )
                                : null,
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  child.fullName,
                                  style: AppTextStyles.subtitle.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  'Mã SV: ${child.code} • Lớp: ${child.classCode}',
                                  style: AppTextStyles.caption.copyWith(
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                                const SizedBox(height: 2),
                                Text(
                                  'Chuyên ngành: ${child.major.isEmpty ? 'Chưa cập nhật' : child.major}',
                                  style: AppTextStyles.caption.copyWith(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                                const SizedBox(height: 2),
                                Text(
                                  'Cơ sở: ${child.department}',
                                  style: AppTextStyles.caption,
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      const Divider(height: 1),
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          Expanded(
                            child: _buildMetricCol(
                              'Điểm GPA',
                              formatScore(child.currentGpa),
                              AppColors.warning,
                            ),
                          ),
                          Container(
                            width: 1,
                            height: 30,
                            color: AppColors.border,
                          ),
                          Expanded(
                            child: _buildMetricCol(
                              'Chuyên cần',
                              '${(child.attendanceRate * 100).toInt()}%',
                              AppColors.success,
                            ),
                          ),
                          Container(
                            width: 1,
                            height: 30,
                            color: AppColors.border,
                          ),
                          Expanded(
                            child: _buildMetricCol(
                              'Nợ học phí',
                              child.tuitionOwed > 0
                                  ? '${NumberFormat.compact(locale: 'vi_VN').format(child.tuitionOwed)}đ'
                                  : '0đ',
                              child.tuitionOwed > 0
                                  ? AppColors.error
                                  : AppColors.textPrimary,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      SizedBox(
                        width: double.infinity,
                        child: OutlinedButton(
                          onPressed: () {
                            ref.read(activeChildIdProvider.notifier).state =
                                child.id;
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(
                                  'Đã chọn giám sát học sinh: ${child.fullName}',
                                ),
                                backgroundColor: AppColors.success,
                                duration: const Duration(seconds: 1),
                              ),
                            );
                            context.go('/parent/dashboard');
                          },
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppColors.success,
                            side: const BorderSide(color: AppColors.success),
                          ),
                          child: Text(
                            isCurrentActive
                                ? 'Đang theo dõi'
                                : 'Chọn theo dõi con em',
                          ),
                        ),
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

  Widget _buildMetricCol(String label, String value, Color color) {
    return Column(
      children: [
        Text(label, style: AppTextStyles.caption),
        const SizedBox(height: 4),
        Text(
          value,
          style: AppTextStyles.bodyMedium.copyWith(
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
      ],
    );
  }
}
