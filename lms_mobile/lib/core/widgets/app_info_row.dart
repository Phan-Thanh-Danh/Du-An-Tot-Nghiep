import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../theme/app_text_styles.dart';

class AppInfoRow extends StatelessWidget {
  final String label;
  final String value;
  final IconData? icon;
  final bool isBoldValue;
  final Color? valueColor;

  const AppInfoRow({
    super.key,
    required this.label,
    required this.value,
    this.icon,
    this.isBoldValue = false,
    this.valueColor,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 10),
      child: Row(
        children: [
          if (icon != null) ...[
            Icon(icon, size: 18, color: AppColors.textSecondary),
            const SizedBox(width: 12),
          ],
          Expanded(
            flex: 2,
            child: Text(
              label,
              style: AppTextStyles.bodyRegular.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            flex: 3,
            child: Text(
              value,
              style: AppTextStyles.bodyMedium.copyWith(
                fontWeight: isBoldValue ? FontWeight.w600 : FontWeight.w500,
                color: valueColor ?? AppColors.textPrimary,
              ),
              textAlign: TextAlign.end,
            ),
          ),
        ],
      ),
    );
  }
}
