enum TuitionPaymentStatus { pending, paid, failed, cancelled, expired }

class TuitionPayment {
  final String transactionId;
  final String invoiceId;
  final String provider;
  final double amount;
  final String reference;
  final String transferContent;
  final String? qrUrl;
  final String? checkoutUrl;
  final String? qrPayload;
  final TuitionPaymentStatus status;

  const TuitionPayment({
    required this.transactionId,
    required this.invoiceId,
    required this.provider,
    required this.amount,
    required this.reference,
    required this.transferContent,
    required this.status,
    this.qrUrl,
    this.checkoutUrl,
    this.qrPayload,
  });

  bool get isPaid => status == TuitionPaymentStatus.paid;
  bool get isTerminal => switch (status) {
    TuitionPaymentStatus.pending => false,
    _ => true,
  };

  bool get hasQr =>
      (qrPayload?.trim().isNotEmpty ?? false) ||
      (qrUrl?.trim().isNotEmpty ?? false);

  factory TuitionPayment.fromJson(Map<String, dynamic> json) {
    String text(String key) => json[key]?.toString().trim() ?? '';
    double number(String key) => json[key] is num
        ? (json[key] as num).toDouble()
        : double.tryParse(text(key)) ?? 0;

    final rawStatus = text('trangThai').toLowerCase();
    final status = switch (rawStatus) {
      'paid' ||
      'success' ||
      'succeeded' ||
      'thanh_cong' ||
      'da_thanh_toan' => TuitionPaymentStatus.paid,
      'failed' || 'that_bai' || 'loi' => TuitionPaymentStatus.failed,
      'cancelled' || 'canceled' || 'da_huy' => TuitionPaymentStatus.cancelled,
      'expired' || 'het_han' => TuitionPaymentStatus.expired,
      _ => TuitionPaymentStatus.pending,
    };

    String? optional(String key) {
      final value = text(key);
      return value.isEmpty ? null : value;
    }

    return TuitionPayment(
      transactionId: text('maGiaoDich'),
      invoiceId: text('maHoaDon'),
      provider: text('provider'),
      amount: number('amount'),
      reference: text('maThamChieuNoiBo'),
      transferContent: text('noiDungChuyenKhoan'),
      qrUrl: optional('qrUrl'),
      checkoutUrl: optional('checkoutUrl'),
      qrPayload: optional('qrPayload'),
      status: status,
    );
  }
}
