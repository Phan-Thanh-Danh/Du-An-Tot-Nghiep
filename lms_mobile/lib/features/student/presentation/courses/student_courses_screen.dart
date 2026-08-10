import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/progress_bar.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

class StudentCoursesScreen extends ConsumerWidget {
  const StudentCoursesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final repo = ref.watch(activeStudentRepoProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Khóa học của tôi'), centerTitle: true),
      body: FutureBuilder(
        future: repo.getCourses(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoadingSkeleton(itemCount: 4);
          }
          if (snapshot.hasError) {
            return AppErrorState(message: snapshot.error.toString());
          }
          final courses = snapshot.data ?? [];
          if (courses.isEmpty) {
            return const AppEmptyState(
              title: 'Không tìm thấy khóa học',
              description:
                  'Hiện tại bạn chưa được phân lớp hoặc tham gia khóa học nào.',
              icon: Icons.menu_book_rounded,
            );
          }

          return ListView.separated(
            padding: const EdgeInsets.all(16),
            itemCount: courses.length,
            separatorBuilder: (context, index) => const SizedBox(height: 16),
            itemBuilder: (context, index) {
              final course = courses[index];
              return Card(
                clipBehavior: Clip.antiAlias,
                margin: EdgeInsets.zero,
                child: InkWell(
                  onTap: () => context.push('/student/courses/${course.id}'),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Banner image
                      Container(
                        height: 120,
                        width: double.infinity,
                        decoration: BoxDecoration(
                          color: AppColors.primary,
                          image: course.bannerUrl.isEmpty
                              ? null
                              : DecorationImage(
                                  image: NetworkImage(course.bannerUrl),
                                  fit: BoxFit.cover,
                                ),
                        ),
                        child: Container(
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              begin: Alignment.topCenter,
                              end: Alignment.bottomCenter,
                              colors: [
                                Colors.transparent,
                                Colors.black.withValues(alpha: 0.7),
                              ],
                            ),
                          ),
                          padding: const EdgeInsets.all(16),
                          alignment: Alignment.bottomLeft,
                          child: Text(
                            course.code,
                            style: AppTextStyles.subtitle.copyWith(
                              color: Colors.white,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ),
                      // Details
                      Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              course.name,
                              style: AppTextStyles.bodyMedium.copyWith(
                                fontWeight: FontWeight.bold,
                                fontSize: 16,
                              ),
                            ),
                            const SizedBox(height: 6),
                            Row(
                              children: [
                                const Icon(
                                  Icons.person_outline_rounded,
                                  size: 16,
                                  color: AppColors.textSecondary,
                                ),
                                const SizedBox(width: 6),
                                Text(
                                  course.teacherName,
                                  style: AppTextStyles.caption,
                                ),
                                const Spacer(),
                                const Icon(
                                  Icons.star_outline_rounded,
                                  size: 16,
                                  color: AppColors.textSecondary,
                                ),
                                const SizedBox(width: 4),
                                Text(
                                  '${course.creditCount} Tín chỉ',
                                  style: AppTextStyles.caption.copyWith(
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 16),
                            ProgressBar(progress: course.progress),
                          ],
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
}
