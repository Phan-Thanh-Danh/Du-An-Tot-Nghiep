import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_status_badge.dart';
import '../../student/models/student_models.dart';

class TuitionSummaryCard extends StatelessWidget {
  final String eyebrow;
  final double totalAmount;
  final double totalPaid;
  final double totalDebt;

  const TuitionSummaryCard({
    super.key,
    required this.eyebrow,
    required this.totalAmount,
    required this.totalPaid,
    required this.totalDebt,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.currency(locale: 'vi_VN', symbol: 'đ');
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [AppColors.primaryDark, AppColors.primary, AppColors.info],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withValues(alpha: 0.24),
            blurRadius: 24,
            offset: const Offset(0, 12),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 42,
                height: 42,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.18),
                  borderRadius: BorderRadius.circular(13),
                ),
                child: const Icon(
                  Icons.account_balance_wallet_outlined,
                  color: Colors.white,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  eyebrow,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: AppTextStyles.bodyMedium.copyWith(color: Colors.white),
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          Text(
            'Còn phải thanh toán',
            style: AppTextStyles.caption.copyWith(color: Colors.white70),
          ),
          const SizedBox(height: 4),
          FittedBox(
            fit: BoxFit.scaleDown,
            alignment: Alignment.centerLeft,
            child: Text(
              currency.format(totalDebt),
              style: AppTextStyles.display.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
          const SizedBox(height: 18),
          Row(
            children: [
              Expanded(
                child: _SummaryMetric(
                  label: 'Tổng hóa đơn',
                  value: currency.format(totalAmount),
                ),
              ),
              Container(width: 1, height: 34, color: Colors.white24),
              Expanded(
                child: _SummaryMetric(
                  label: 'Đã thanh toán',
                  value: currency.format(totalPaid),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _SummaryMetric extends StatelessWidget {
  final String label;
  final String value;

  const _SummaryMetric({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: AppTextStyles.caption.copyWith(color: Colors.white70),
          ),
          const SizedBox(height: 3),
          FittedBox(
            fit: BoxFit.scaleDown,
            alignment: Alignment.centerLeft,
            child: Text(
              value,
              style: AppTextStyles.bodyMedium.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class TuitionInvoiceCard extends StatelessWidget {
  final TuitionInvoice invoice;
  final VoidCallback? onPay;

  const TuitionInvoiceCard({super.key, required this.invoice, this.onPay});

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.currency(locale: 'vi_VN', symbol: 'đ');
    final remaining = (invoice.amount - invoice.paidAmount).clamp(
      0,
      double.infinity,
    );
    final isPaid = invoice.status == InvoiceStatus.paid;

    return Card(
      margin: EdgeInsets.zero,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 42,
                  height: 42,
                  decoration: BoxDecoration(
                    color: isPaid
                        ? AppColors.successLight
                        : AppColors.primaryLight,
                    borderRadius: BorderRadius.circular(13),
                  ),
                  child: Icon(
                    isPaid
                        ? Icons.receipt_long_rounded
                        : Icons.payments_outlined,
                    color: isPaid ? AppColors.success : AppColors.primary,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        invoice.termName,
                        style: AppTextStyles.bodyMedium.copyWith(
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Hạn ${DateFormat('dd/MM/yyyy').format(invoice.dueDate)}',
                        style: AppTextStyles.caption,
                      ),
                    ],
                  ),
                ),
                _statusBadge(invoice.status),
              ],
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(13),
              decoration: BoxDecoration(
                color: AppColors.background,
                borderRadius: BorderRadius.circular(14),
              ),
              child: Column(
                children: [
                  _AmountRow(
                    label: 'Tổng phải đóng',
                    value: currency.format(invoice.amount),
                  ),
                  const SizedBox(height: 8),
                  _AmountRow(
                    label: 'Đã thanh toán',
                    value: currency.format(invoice.paidAmount),
                    color: AppColors.success,
                  ),
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 9),
                    child: Divider(height: 1),
                  ),
                  _AmountRow(
                    label: 'Còn lại',
                    value: currency.format(remaining),
                    color: remaining > 0 ? AppColors.error : AppColors.success,
                    emphasized: true,
                  ),
                ],
              ),
            ),
            if (!isPaid && onPay != null) ...[
              const SizedBox(height: 14),
              SizedBox(
                width: double.infinity,
                child: FilledButton.icon(
                  onPressed: onPay,
                  icon: const Icon(Icons.qr_code_2_rounded),
                  label: const Text('Tạo QR thanh toán'),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _statusBadge(InvoiceStatus status) => switch (status) {
    InvoiceStatus.paid => const AppStatusBadge(
      label: 'Đã đóng',
      type: BadgeType.success,
    ),
    InvoiceStatus.partial => const AppStatusBadge(
      label: 'Một phần',
      type: BadgeType.warning,
    ),
    InvoiceStatus.unpaid => const AppStatusBadge(
      label: 'Chưa đóng',
      type: BadgeType.error,
    ),
    InvoiceStatus.overdue => const AppStatusBadge(
      label: 'Quá hạn',
      type: BadgeType.error,
    ),
  };
}

class _AmountRow extends StatelessWidget {
  final String label;
  final String value;
  final Color? color;
  final bool emphasized;

  const _AmountRow({
    required this.label,
    required this.value,
    this.color,
    this.emphasized = false,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(child: Text(label, style: AppTextStyles.bodyRegular)),
        const SizedBox(width: 12),
        Text(
          value,
          style: AppTextStyles.bodyMedium.copyWith(
            color: color ?? AppColors.textPrimary,
            fontWeight: emphasized ? FontWeight.w800 : FontWeight.w600,
          ),
        ),
      ],
    );
  }
}
