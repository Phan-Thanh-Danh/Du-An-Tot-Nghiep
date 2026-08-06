import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/features/finance/models/tuition_payment.dart';
import 'package:lms_mobile/features/finance/presentation/tuition_payment_sheet.dart';

void main() {
  test('score formatter keeps at most two decimal digits', () {
    expect(formatScore(8.755555), '8.76');
    expect(formatScore(8.70), '8.7');
    expect(formatScore(8.0), '8');
    expect(formatScore(null), '-');
  });

  test('maps PayOS payment response and recognizes paid status', () {
    final payment = TuitionPayment.fromJson({
      'maGiaoDich': 101,
      'maHoaDon': 202,
      'provider': 'payos',
      'amount': 9500000,
      'maThamChieuNoiBo': 'LMS-202-101',
      'noiDungChuyenKhoan': 'LMS 202 101',
      'qrPayload': '000201010212',
      'checkoutUrl': 'https://pay.payos.vn/web/demo',
      'trangThai': 'thanh_cong',
    });

    expect(payment.transactionId, '101');
    expect(payment.amount, 9500000);
    expect(payment.isPaid, isTrue);
    expect(payment.isTerminal, isTrue);
    expect(payment.hasQr, isTrue);
  });

  test('unknown backend payment status stays pending', () {
    final payment = TuitionPayment.fromJson({
      'maGiaoDich': 1,
      'maHoaDon': 2,
      'trangThai': 'dang_xu_ly',
    });

    expect(payment.status, TuitionPaymentStatus.pending);
    expect(payment.isTerminal, isFalse);
  });

  testWidgets('payment flow asks confirmation before creating transaction', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    var createCalls = 0;
    final paidPayment = TuitionPayment.fromJson({
      'maGiaoDich': 101,
      'maHoaDon': 202,
      'provider': 'payos',
      'amount': 9500000,
      'maThamChieuNoiBo': 'LMS-202-101',
      'noiDungChuyenKhoan': 'LMS 202 101',
      'qrPayload': '000201010212',
      'checkoutUrl': 'https://pay.payos.vn/web/demo',
      'trangThai': 'da_thanh_toan',
    });

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: Builder(
            builder: (context) => FilledButton(
              onPressed: () => showTuitionPaymentFlow(
                context: context,
                invoiceTitle: 'Học phí HK2 2026',
                amount: 9500000,
                createPayment: () async {
                  createCalls++;
                  return paidPayment;
                },
                getPayment: (_) async => paidPayment,
              ),
              child: const Text('Thanh toán'),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Thanh toán'));
    await tester.pumpAndSettle();
    expect(createCalls, 0);
    expect(find.text('Xác nhận tạo mã thanh toán'), findsOneWidget);

    await tester.tap(find.text('Tạo mã QR'));
    await tester.pumpAndSettle();
    expect(createCalls, 1);
    expect(find.text('Đã thanh toán'), findsOneWidget);
    expect(find.text('Lưu ảnh QR'), findsOneWidget);
    expect(find.text('Mở PayOS'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}
