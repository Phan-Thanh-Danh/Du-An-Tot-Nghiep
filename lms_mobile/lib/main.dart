import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'app/lms_mobile_app.dart';
import 'features/auth/data/auth_provider.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize Vietnamese date time symbols for table_calendar
  await initializeDateFormatting('vi_VN', null);
  await loadDemoAccounts();

  runApp(const ProviderScope(child: LmsMobileApp()));
}
