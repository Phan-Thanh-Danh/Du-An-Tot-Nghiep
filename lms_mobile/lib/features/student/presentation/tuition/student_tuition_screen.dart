import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/widgets/app_states.dart';
import '../../../auth/data/auth_provider.dart';
import '../../../finance/presentation/tuition_payment_sheet.dart';
import '../../../finance/presentation/tuition_widgets.dart';
import '../../models/student_models.dart';

class StudentTuitionScreen extends ConsumerStatefulWidget {
  const StudentTuitionScreen({super.key});

  @override
  ConsumerState<StudentTuitionScreen> createState() =>
      _StudentTuitionScreenState();
}

class _StudentTuitionScreenState extends ConsumerState<StudentTuitionScreen> {
  List<TuitionInvoice> _invoices = [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadTuition();
  }

  Future<void> _loadTuition() async {
    try {
      final data = await ref
          .read(activeStudentRepoProvider)
          .getTuitionInvoices();
      if (!mounted) return;
      setState(() {
        _invoices = data;
        _isLoading = false;
        _error = null;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _error = error.toString();
      });
    }
  }

  Future<void> _pay(TuitionInvoice invoice) async {
    final remaining = (invoice.amount - invoice.paidAmount)
        .clamp(0, double.infinity)
        .toDouble();
    final repository = ref.read(activeStudentRepoProvider);
    await showTuitionPaymentFlow(
      context: context,
      invoiceTitle: invoice.termName,
      amount: remaining,
      createPayment: () => repository.createTuitionPayment(invoice.id),
      getPayment: repository.getTuitionPayment,
    );
    if (mounted) await _loadTuition();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Học phí & Công nợ'),
        centerTitle: true,
        actions: [
          IconButton(
            tooltip: 'Tải lại',
            onPressed: _isLoading ? null : _loadTuition,
            icon: const Icon(Icons.refresh_rounded),
          ),
        ],
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoading) return const AppLoadingSkeleton(itemCount: 3);
    if (_error != null) {
      return AppErrorState(message: _error!, onRetry: _loadTuition);
    }

    final totalAmount = _invoices.fold<double>(0, (sum, i) => sum + i.amount);
    final totalPaid = _invoices.fold<double>(0, (sum, i) => sum + i.paidAmount);
    final totalDebt = (totalAmount - totalPaid)
        .clamp(0, double.infinity)
        .toDouble();

    return RefreshIndicator(
      onRefresh: _loadTuition,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            TuitionSummaryCard(
              eyebrow: 'Tổng quan tài chính sinh viên',
              totalAmount: totalAmount,
              totalPaid: totalPaid,
              totalDebt: totalDebt,
            ),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: Text(
                    'Hóa đơn học phí',
                    style: AppTextStyles.subtitle.copyWith(fontSize: 17),
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 5,
                  ),
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    borderRadius: BorderRadius.circular(99),
                  ),
                  child: Text(
                    '${_invoices.length} khoản',
                    style: AppTextStyles.caption.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if (_invoices.isEmpty)
              const AppEmptyState(
                title: 'Chưa có hóa đơn',
                description: 'Các khoản học phí mới sẽ xuất hiện tại đây.',
              )
            else
              ListView.separated(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: _invoices.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (_, index) {
                  final invoice = _invoices[index];
                  return TuitionInvoiceCard(
                    invoice: invoice,
                    onPay: invoice.status == InvoiceStatus.paid
                        ? null
                        : () => _pay(invoice),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }
}
