import 'dart:async';
import 'dart:ui' as ui;

import 'package:file_saver/file_saver.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_buttons.dart';
import '../models/tuition_payment.dart';

Future<bool?> showTuitionPaymentFlow({
  required BuildContext context,
  required String invoiceTitle,
  required double amount,
  required Future<TuitionPayment> Function() createPayment,
  required Future<TuitionPayment> Function(String transactionId) getPayment,
}) async {
  final formatter = NumberFormat.currency(locale: 'vi_VN', symbol: 'đ');
  final confirmed = await showDialog<bool>(
    context: context,
    builder: (dialogContext) => AlertDialog(
      icon: const Icon(Icons.verified_user_outlined, color: AppColors.primary),
      title: const Text('Xác nhận tạo mã thanh toán'),
      content: Text(
        '$invoiceTitle\nSố tiền còn lại: ${formatter.format(amount)}\n\n'
        'Ứng dụng sẽ tạo một giao dịch PayOS mới. Chỉ tiếp tục khi bạn muốn thanh toán hóa đơn này.',
        style: AppTextStyles.bodyRegular.copyWith(height: 1.5),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(dialogContext, false),
          child: const Text('Để sau'),
        ),
        FilledButton(
          onPressed: () => Navigator.pop(dialogContext, true),
          child: const Text('Tạo mã QR'),
        ),
      ],
    ),
  );

  if (confirmed != true || !context.mounted) return false;

  return showModalBottomSheet<bool>(
    context: context,
    isScrollControlled: true,
    useSafeArea: true,
    backgroundColor: AppColors.surface,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
    ),
    builder: (_) => TuitionPaymentSheet(
      createPayment: createPayment,
      getPayment: getPayment,
    ),
  );
}

class TuitionPaymentSheet extends StatefulWidget {
  final Future<TuitionPayment> Function() createPayment;
  final Future<TuitionPayment> Function(String transactionId) getPayment;

  const TuitionPaymentSheet({
    super.key,
    required this.createPayment,
    required this.getPayment,
  });

  @override
  State<TuitionPaymentSheet> createState() => _TuitionPaymentSheetState();
}

class _TuitionPaymentSheetState extends State<TuitionPaymentSheet> {
  final _qrKey = GlobalKey();
  final _currency = NumberFormat.currency(locale: 'vi_VN', symbol: 'đ');
  Timer? _pollTimer;
  TuitionPayment? _payment;
  String? _error;
  bool _isLoading = true;
  bool _isPolling = false;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _createPayment();
  }

  @override
  void dispose() {
    _pollTimer?.cancel();
    super.dispose();
  }

  Future<void> _createPayment() async {
    _pollTimer?.cancel();
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final payment = await widget.createPayment();
      if (!mounted) return;
      setState(() {
        _payment = payment;
        _isLoading = false;
      });
      _startPolling();
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _error = error.toString();
      });
    }
  }

  void _startPolling() {
    final payment = _payment;
    if (payment == null || payment.isTerminal) return;
    _pollTimer = Timer.periodic(const Duration(seconds: 5), (_) => _poll());
  }

  Future<void> _poll() async {
    final payment = _payment;
    if (_isPolling || payment == null || payment.isTerminal) return;
    _isPolling = true;
    try {
      final updated = await widget.getPayment(payment.transactionId);
      if (!mounted) return;
      setState(() => _payment = updated);
      if (updated.isTerminal) _pollTimer?.cancel();
    } catch (_) {
      // Giữ QR hiện tại; lần polling sau sẽ thử lại.
    } finally {
      _isPolling = false;
    }
  }

  Future<void> _copyTransferContent() async {
    final content = _payment?.transferContent ?? '';
    if (content.isEmpty) return;
    await Clipboard.setData(ClipboardData(text: content));
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Đã sao chép nội dung chuyển khoản.')),
    );
  }

  Future<void> _saveQr() async {
    if (_isSaving) return;
    setState(() => _isSaving = true);
    try {
      await WidgetsBinding.instance.endOfFrame;
      final boundary =
          _qrKey.currentContext?.findRenderObject() as RenderRepaintBoundary?;
      if (boundary == null) throw StateError('Không tìm thấy mã QR.');
      final image = await boundary.toImage(pixelRatio: 3);
      final byteData = await image.toByteData(format: ui.ImageByteFormat.png);
      final bytes = byteData?.buffer.asUint8List();
      if (bytes == null) throw StateError('Không thể tạo ảnh QR.');

      final payment = _payment!;
      final fileName =
          'LMS-QR-${payment.reference.isEmpty ? payment.transactionId : payment.reference}';
      if (!kIsWeb && defaultTargetPlatform == TargetPlatform.linux) {
        await FileSaver.instance.saveFile(
          name: fileName,
          bytes: bytes,
          fileExtension: 'png',
          mimeType: MimeType.png,
        );
      } else {
        await FileSaver.instance.saveAs(
          name: fileName,
          bytes: bytes,
          fileExtension: 'png',
          mimeType: MimeType.png,
        );
      }
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Đã lưu ảnh mã QR.')));
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text('Không thể lưu mã QR: $error')));
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _openCheckout() async {
    final rawUrl = _payment?.checkoutUrl;
    final uri = rawUrl == null ? null : Uri.tryParse(rawUrl);
    if (uri == null ||
        !await launchUrl(uri, mode: LaunchMode.externalApplication)) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Không thể mở trang thanh toán PayOS.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return FractionallySizedBox(
      heightFactor: 0.92,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 20),
        child: Column(
          children: [
            Container(
              width: 44,
              height: 5,
              decoration: BoxDecoration(
                color: AppColors.border,
                borderRadius: BorderRadius.circular(99),
              ),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Container(
                  width: 44,
                  height: 44,
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: const Icon(Icons.qr_code_2, color: AppColors.primary),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Thanh toán học phí', style: AppTextStyles.title),
                      Text(
                        'Quét QR hoặc mở PayOS để chọn ngân hàng',
                        style: AppTextStyles.caption,
                      ),
                    ],
                  ),
                ),
                IconButton(
                  tooltip: 'Đóng',
                  onPressed: () => Navigator.pop(context, _payment?.isPaid),
                  icon: const Icon(Icons.close),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(child: _buildContent()),
          ],
        ),
      ),
    );
  }

  Widget _buildContent() {
    if (_isLoading) {
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 16),
            Text('Đang tạo mã thanh toán an toàn...'),
          ],
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 52, color: AppColors.error),
            const SizedBox(height: 12),
            Text(_error!, textAlign: TextAlign.center),
            const SizedBox(height: 20),
            AppPrimaryButton(
              text: 'Thử tạo lại',
              icon: Icons.refresh,
              onPressed: _createPayment,
            ),
          ],
        ),
      );
    }

    final payment = _payment!;
    return SingleChildScrollView(
      child: Column(
        children: [
          _PaymentStatusBanner(payment: payment),
          const SizedBox(height: 16),
          if (payment.hasQr)
            RepaintBoundary(
              key: _qrKey,
              child: Container(
                width: 252,
                height: 252,
                padding: const EdgeInsets.all(14),
                color: Colors.white,
                child: payment.qrPayload?.isNotEmpty == true
                    ? QrImageView(
                        data: payment.qrPayload!,
                        version: QrVersions.auto,
                        backgroundColor: Colors.white,
                      )
                    : Image.network(
                        payment.qrUrl!,
                        fit: BoxFit.contain,
                        errorBuilder: (_, _, _) => const Center(
                          child: Icon(Icons.broken_image_outlined, size: 48),
                        ),
                      ),
              ),
            )
          else
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: AppColors.warningLight,
                borderRadius: BorderRadius.circular(16),
              ),
              child: const Text(
                'Backend chưa trả QR. Bạn vẫn có thể mở trang PayOS bên dưới.',
                textAlign: TextAlign.center,
              ),
            ),
          const SizedBox(height: 14),
          Text(
            _currency.format(payment.amount),
            style: AppTextStyles.display.copyWith(
              color: AppColors.primary,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 16),
          _PaymentDetail(
            label: 'Mã tham chiếu',
            value: payment.reference.isEmpty
                ? 'Đang cập nhật'
                : payment.reference,
          ),
          _PaymentDetail(
            label: 'Nội dung chuyển khoản',
            value: payment.transferContent.isEmpty
                ? 'Đang cập nhật'
                : payment.transferContent,
            trailing: payment.transferContent.isEmpty
                ? null
                : IconButton(
                    tooltip: 'Sao chép',
                    onPressed: _copyTransferContent,
                    icon: const Icon(Icons.copy_rounded, size: 20),
                  ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: payment.hasQr && !_isSaving ? _saveQr : null,
                  icon: _isSaving
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Icon(Icons.download_rounded),
                  label: const Text('Lưu ảnh QR'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FilledButton.icon(
                  onPressed: payment.checkoutUrl == null ? null : _openCheckout,
                  icon: const Icon(Icons.open_in_new_rounded),
                  label: const Text('Mở PayOS'),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            payment.isTerminal
                ? 'Trạng thái đã được xác nhận từ Backend.'
                : 'Ứng dụng tự kiểm tra trạng thái mỗi 5 giây. Không đóng màn hình nếu bạn muốn theo dõi ngay.',
            style: AppTextStyles.caption,
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

class _PaymentStatusBanner extends StatelessWidget {
  final TuitionPayment payment;

  const _PaymentStatusBanner({required this.payment});

  @override
  Widget build(BuildContext context) {
    final (label, color, background, icon) = switch (payment.status) {
      TuitionPaymentStatus.paid => (
        'Đã thanh toán',
        AppColors.success,
        AppColors.successLight,
        Icons.check_circle,
      ),
      TuitionPaymentStatus.failed => (
        'Giao dịch thất bại',
        AppColors.error,
        AppColors.errorLight,
        Icons.error,
      ),
      TuitionPaymentStatus.cancelled => (
        'Đã hủy',
        AppColors.error,
        AppColors.errorLight,
        Icons.cancel,
      ),
      TuitionPaymentStatus.expired => (
        'Mã đã hết hạn',
        AppColors.warning,
        AppColors.warningLight,
        Icons.timer_off,
      ),
      TuitionPaymentStatus.pending => (
        'Đang chờ thanh toán',
        AppColors.primary,
        AppColors.primaryLight,
        Icons.schedule,
      ),
    };

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, color: color, size: 20),
          const SizedBox(width: 8),
          Text(
            label,
            style: AppTextStyles.bodyMedium.copyWith(
              color: color,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }
}

class _PaymentDetail extends StatelessWidget {
  final String label;
  final String value;
  final Widget? trailing;

  const _PaymentDetail({
    required this.label,
    required this.value,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.fromLTRB(14, 10, 8, 10),
      decoration: BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: AppTextStyles.caption),
                const SizedBox(height: 3),
                SelectableText(
                  value,
                  style: AppTextStyles.bodyMedium.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
          ?trailing,
        ],
      ),
    );
  }
}
