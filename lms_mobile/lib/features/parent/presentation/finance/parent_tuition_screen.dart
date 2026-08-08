import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/widgets/app_states.dart';
import '../../../auth/data/auth_provider.dart';
import '../../../finance/presentation/tuition_payment_sheet.dart';
import '../../../finance/presentation/tuition_widgets.dart';
import '../../../student/models/student_models.dart';
import '../../data/active_child_provider.dart';
import '../../models/parent_models.dart';
import '../widgets/child_switcher.dart';

class ParentTuitionScreen extends ConsumerStatefulWidget {
  const ParentTuitionScreen({super.key});

  @override
  ConsumerState<ParentTuitionScreen> createState() =>
      _ParentTuitionScreenState();
}

class _ParentTuitionScreenState extends ConsumerState<ParentTuitionScreen> {
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
      final child = await ref.read(activeChildProvider.future);
      if (child == null) {
        if (!mounted) return;
        setState(() {
          _invoices = [];
          _isLoading = false;
          _error = null;
        });
        return;
      }
      final data = await ref
          .read(activeParentRepoProvider)
          .getChildTuition(child.id);
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

  Future<void> _pay(Child child, TuitionInvoice invoice) async {
    final remaining = (invoice.amount - invoice.paidAmount)
        .clamp(0, double.infinity)
        .toDouble();
    final repository = ref.read(activeParentRepoProvider);
    await showTuitionPaymentFlow(
      context: context,
      invoiceTitle: '${invoice.termName} · ${child.fullName}',
      amount: remaining,
      createPayment: () =>
          repository.createChildTuitionPayment(child.id, invoice.id),
      getPayment: (transactionId) =>
          repository.getChildTuitionPayment(child.id, transactionId),
    );
    if (mounted) await _loadTuition();
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(activeChildIdProvider, (previous, next) {
      if (next != previous) {
        setState(() => _isLoading = true);
        _loadTuition();
      }
    });

    final activeChild = ref.watch(activeChildProvider);
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
      body: Column(
        children: [
          const ChildSwitcher(),
          Expanded(
            child: activeChild.when(
              loading: () => const AppLoadingSkeleton(itemCount: 3),
              error: (error, _) => AppErrorState(message: error.toString()),
              data: (child) => _buildBody(child),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBody(Child? child) {
    if (child == null) {
      return const AppEmptyState(
        title: 'Chưa có hồ sơ sinh viên',
        description: 'Vui lòng liên kết tài khoản con em để xem học phí.',
      );
    }
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
              eyebrow: 'Tài chính của ${child.fullName}',
              totalAmount: totalAmount,
              totalPaid: totalPaid,
              totalDebt: totalDebt,
            ),
            const SizedBox(height: 14),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: AppColors.primaryLight,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: AppColors.primary.withValues(alpha: 0.18),
                ),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(
                    Icons.account_tree_outlined,
                    color: AppColors.primary,
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'QR gộp cho nhiều con em',
                          style: AppTextStyles.bodyMedium.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Mobile đã dành sẵn luồng này. Backend cần endpoint gộp nhiều hóa đơn thành một giao dịch PayOS trước khi có thể bật nút tạo QR.',
                          style: AppTextStyles.caption.copyWith(height: 1.4),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: Text(
                    'Các khoản phải thu',
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
                description:
                    'Các khoản học phí của sinh viên sẽ xuất hiện tại đây.',
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
                        : () => _pay(child, invoice),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }
}
