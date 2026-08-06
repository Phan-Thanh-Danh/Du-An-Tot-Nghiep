import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';
import '../theme/app_colors.dart';
import '../theme/app_text_styles.dart';
import 'app_buttons.dart';

class AppEmptyState extends StatelessWidget {
  final String title;
  final String description;
  final IconData icon;
  final String? actionLabel;
  final VoidCallback? onActionPressed;

  const AppEmptyState({
    super.key,
    required this.title,
    required this.description,
    this.icon = Icons.inbox_outlined,
    this.actionLabel,
    this.onActionPressed,
  });

  @override
  Widget build(BuildContext context) {
    return _ResponsiveStateLayout(
      builder: (compact) => Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Container(
            padding: EdgeInsets.all(compact ? 8 : 16),
            decoration: const BoxDecoration(
              color: AppColors.background,
              shape: BoxShape.circle,
            ),
            child: Icon(
              icon,
              size: compact ? 32 : 64,
              color: AppColors.textSecondary.withValues(alpha: 0.5),
            ),
          ),
          SizedBox(height: compact ? 8 : 20),
          Text(
            title,
            style: AppTextStyles.subtitle.copyWith(fontSize: compact ? 15 : 18),
            textAlign: TextAlign.center,
          ),
          SizedBox(height: compact ? 4 : 8),
          Text(
            description,
            style: compact ? AppTextStyles.caption : AppTextStyles.bodyRegular,
            textAlign: TextAlign.center,
          ),
          if (actionLabel != null && onActionPressed != null) ...[
            SizedBox(height: compact ? 10 : 24),
            AppPrimaryButton(
              text: actionLabel!,
              onPressed: onActionPressed!,
              width: 200,
            ),
          ],
        ],
      ),
    );
  }
}

class AppErrorState extends StatelessWidget {
  final String message;
  final VoidCallback? onRetry;

  const AppErrorState({super.key, required this.message, this.onRetry});

  @override
  Widget build(BuildContext context) {
    return _ResponsiveStateLayout(
      builder: (compact) => Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Icon(
            Icons.error_outline_rounded,
            size: compact ? 34 : 64,
            color: AppColors.error,
          ),
          SizedBox(height: compact ? 8 : 20),
          Text(
            'Đã xảy ra lỗi',
            style: AppTextStyles.subtitle.copyWith(fontSize: compact ? 15 : 18),
            textAlign: TextAlign.center,
          ),
          SizedBox(height: compact ? 4 : 8),
          Text(
            message,
            style: (compact ? AppTextStyles.caption : AppTextStyles.bodyRegular)
                .copyWith(color: AppColors.textSecondary),
            textAlign: TextAlign.center,
          ),
          if (onRetry != null) ...[
            SizedBox(height: compact ? 10 : 24),
            AppPrimaryButton(text: 'Thử lại', onPressed: onRetry!, width: 150),
          ],
        ],
      ),
    );
  }
}

class _ResponsiveStateLayout extends StatelessWidget {
  final Widget Function(bool compact) builder;

  const _ResponsiveStateLayout({required this.builder});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final hasBoundedHeight = constraints.hasBoundedHeight;
        final compact = hasBoundedHeight && constraints.maxHeight < 240;
        final horizontalPadding = compact ? 16.0 : 24.0;
        final verticalPadding = compact ? 8.0 : 24.0;
        final content = builder(compact);

        if (!hasBoundedHeight) {
          return Padding(
            padding: EdgeInsets.symmetric(
              horizontal: horizontalPadding,
              vertical: verticalPadding,
            ),
            child: Center(child: content),
          );
        }

        final minimumContentHeight =
            (constraints.maxHeight - (verticalPadding * 2))
                .clamp(0.0, double.infinity)
                .toDouble();

        return SingleChildScrollView(
          physics: const ClampingScrollPhysics(),
          padding: EdgeInsets.symmetric(
            horizontal: horizontalPadding,
            vertical: verticalPadding,
          ),
          child: ConstrainedBox(
            constraints: BoxConstraints(minHeight: minimumContentHeight),
            child: Center(child: content),
          ),
        );
      },
    );
  }
}

class AppLoadingSkeleton extends StatelessWidget {
  final int itemCount;

  const AppLoadingSkeleton({super.key, this.itemCount = 3});

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      itemCount: itemCount,
      itemBuilder: (context, index) {
        return Shimmer.fromColors(
          baseColor: AppColors.border.withValues(alpha: 0.5),
          highlightColor: AppColors.background,
          child: Container(
            margin: const EdgeInsets.only(bottom: 16),
            height: 100,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: AppColors.border),
            ),
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Container(
                        width: double.infinity,
                        height: 16,
                        color: Colors.white,
                      ),
                      const SizedBox(height: 8),
                      Container(width: 150, height: 12, color: Colors.white),
                    ],
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
