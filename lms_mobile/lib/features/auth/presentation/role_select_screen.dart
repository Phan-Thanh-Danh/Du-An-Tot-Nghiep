import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

class RoleSelectScreen extends ConsumerWidget {
  const RoleSelectScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                'Chọn Cổng Truy Cập',
                style: AppTextStyles.display.copyWith(
                  fontSize: 24,
                  fontWeight: FontWeight.w800,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                'Vui lòng chọn vai trò để truy cập cổng dịch vụ tương ứng',
                style: AppTextStyles.bodyRegular,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 40),
              // Student Role Card
              _buildRoleCard(
                context: context,
                ref: ref,
                title: 'Cổng Sinh Viên',
                description:
                    'Xem lịch học, tham gia khóa học, nộp bài tập, theo dõi điểm số và chuyên cần.',
                icon: Icons.person_pin_rounded,
                color: AppColors.primary,
                role: UserRole.student,
                route: '/student/dashboard',
              ),
              const SizedBox(height: 20),
              // Parent Role Card
              _buildRoleCard(
                context: context,
                ref: ref,
                title: 'Cổng Phụ Huynh',
                description:
                    'Theo dõi tiến độ học tập, chuyên cần, lịch thi, nhận thông báo và đóng học phí cho con.',
                icon: Icons.family_restroom_rounded,
                color: AppColors.success,
                role: UserRole.parent,
                route: '/parent/dashboard',
              ),
              const SizedBox(height: 40),
              // Back to login
              TextButton(
                onPressed: () {
                  ref.read(authProvider.notifier).logout();
                  context.go('/login');
                },
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.arrow_back_rounded, size: 18),
                    const SizedBox(width: 8),
                    Text(
                      'Quay lại đăng nhập',
                      style: AppTextStyles.bodyMedium.copyWith(
                        color: AppColors.primary,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildRoleCard({
    required BuildContext context,
    required WidgetRef ref,
    required String title,
    required String description,
    required IconData icon,
    required Color color,
    required UserRole role,
    required String route,
  }) {
    return Card(
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: AppColors.border, width: 1.5),
      ),
      child: InkWell(
        onTap: () {
          context.go('/login');
        },
        borderRadius: BorderRadius.circular(20),
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.1),
                  shape: BoxShape.circle,
                ),
                child: Icon(icon, size: 32, color: color),
              ),
              const SizedBox(width: 20),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: AppTextStyles.subtitle.copyWith(
                        color: AppColors.textPrimary,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      description,
                      style: AppTextStyles.caption.copyWith(
                        color: AppColors.textSecondary,
                        height: 1.4,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
