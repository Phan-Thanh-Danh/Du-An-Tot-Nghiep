import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lms_mobile/app/lms_mobile_app.dart';
import 'package:lms_mobile/app/router/app_router.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  setUpAll(loadDemoAccounts);

  testWidgets('App login screen smoke test', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    await tester.pumpWidget(const ProviderScope(child: LmsMobileApp()));

    // Verify that the login text is found.
    expect(find.text('LMS Portal'), findsOneWidget);
    expect(find.text('Đăng nhập'), findsNWidgets(2));
    expect(find.text('Demo Sinh viên'), findsOneWidget);
    expect(find.text(demoStudentAccount.username), findsOneWidget);
    expect(find.text(demoStudentAccount.password), findsWidgets);
    expect(find.text('Demo Phụ huynh đa chuyên ngành'), findsOneWidget);
    expect(find.text(demoParentAccount.username), findsOneWidget);
    expect(find.text(demoParentAccount.password), findsWidgets);
  });

  testWidgets('Protected route redirects unauthenticated user to login', (
    WidgetTester tester,
  ) async {
    final container = ProviderContainer();
    addTearDown(container.dispose);

    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const LmsMobileApp(),
      ),
    );
    container.read(appRouterProvider).go('/student/dashboard');
    await tester.pumpAndSettle();

    expect(find.text('LMS Portal'), findsOneWidget);
    expect(container.read(appRouterProvider).state.matchedLocation, '/login');
  });

  testWidgets('Login screen has no overflow at 390x844', (
    WidgetTester tester,
  ) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    await tester.pumpWidget(const ProviderScope(child: LmsMobileApp()));
    await tester.pumpAndSettle();

    expect(tester.takeException(), isNull);
  });

  testWidgets('Empty state has no overflow in a 107px-high area', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: SizedBox(
            width: 477.3,
            height: 107,
            child: AppEmptyState(
              title: 'Không có dữ liệu',
              description: 'Chưa có nội dung để hiển thị trong mục này.',
              actionLabel: 'Tải lại',
              onActionPressed: () {},
            ),
          ),
        ),
      ),
    );

    expect(tester.takeException(), isNull);
  });

  testWidgets('Error state has no overflow in a 107px-high area', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: SizedBox(
            width: 477.3,
            height: 107,
            child: AppErrorState(
              message: 'Không thể tải dữ liệu lúc này.',
              onRetry: () {},
            ),
          ),
        ),
      ),
    );

    expect(tester.takeException(), isNull);
  });
}
