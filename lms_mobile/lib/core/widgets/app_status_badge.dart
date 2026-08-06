import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../theme/app_text_styles.dart';

enum BadgeType { success, warning, error, info, primary }

class AppStatusBadge extends StatelessWidget {
  final String label;
  final BadgeType type;

  const AppStatusBadge({
    super.key,
    required this.label,
    this.type = BadgeType.info,
  });

  @override
  Widget build(BuildContext context) {
    Color bgColor;
    Color textColor;

    switch (type) {
      case BadgeType.success:
        bgColor = AppColors.successLight;
        textColor = AppColors.success;
        break;
      case BadgeType.warning:
        bgColor = AppColors.warningLight;
        textColor = AppColors.warning;
        break;
      case BadgeType.error:
        bgColor = AppColors.errorLight;
        textColor = AppColors.error;
        break;
      case BadgeType.primary:
        bgColor = AppColors.primaryLight;
        textColor = AppColors.primary;
        break;
      case BadgeType.info:
        bgColor = AppColors.infoLight;
        textColor = AppColors.info;
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        label,
        style: AppTextStyles.caption.copyWith(
          color: textColor,
          fontWeight: FontWeight.w600,
          fontSize: 11,
        ),
      ),
    );
  }
}
