import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

class ChildSwitcher extends ConsumerWidget {
  const ChildSwitcher({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final repo = ref.watch(activeParentRepoProvider);
    final activeId = ref.watch(activeChildIdProvider);

    return FutureBuilder(
      future: repo.getChildren(),
      builder: (context, snapshot) {
        if (!snapshot.hasData) return const SizedBox.shrink();
        final list = snapshot.data ?? [];
        if (list.length <= 1) return const SizedBox.shrink();

        // Ensure activeId is set initially
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (activeId == null && list.isNotEmpty) {
            ref.read(activeChildIdProvider.notifier).state = list.first.id;
          }
        });

        return Container(
          height: 112,
          color: AppColors.surface,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            itemCount: list.length,
            itemBuilder: (context, index) {
              final child = list[index];
              final isSelected = child.id == (activeId ?? list.first.id);

              return Padding(
                padding: const EdgeInsets.only(right: 16.0),
                child: InkWell(
                  onTap: () {
                    ref.read(activeChildIdProvider.notifier).state = child.id;
                  },
                  borderRadius: BorderRadius.circular(12),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 200),
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 8,
                    ),
                    decoration: BoxDecoration(
                      color: isSelected
                          ? AppColors.successLight
                          : Colors.transparent,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(
                        color: isSelected
                            ? AppColors.success
                            : AppColors.border,
                        width: isSelected ? 1.5 : 1,
                      ),
                    ),
                    child: Row(
                      children: [
                        CircleAvatar(
                          radius: 20,
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
                        const SizedBox(width: 12),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text(
                              child.fullName,
                              style: AppTextStyles.bodyMedium.copyWith(
                                fontWeight: isSelected
                                    ? FontWeight.bold
                                    : FontWeight.w500,
                                color: isSelected
                                    ? AppColors.success
                                    : AppColors.textPrimary,
                              ),
                            ),
                            Text(
                              child.classCode,
                              style: AppTextStyles.caption.copyWith(
                                fontSize: 10,
                              ),
                            ),
                            SizedBox(
                              width: 150,
                              child: Text(
                                child.major,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: AppTextStyles.caption.copyWith(
                                  fontSize: 10,
                                  color: AppColors.primary,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              );
            },
          ),
        );
      },
    );
  }
}
