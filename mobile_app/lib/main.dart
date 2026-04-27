import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'features/calculator/views/calculator_screen.dart';

void main() {
  runApp(
    // Bọc toàn bộ App trong ProviderScope để kích hoạt Riverpod
    const ProviderScope(
      child: ClassWizApp(),
    ),
  );
}

class ClassWizApp extends StatelessWidget {
  const ClassWizApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ClassWiz-X',
      debugShowCheckedModeBanner: false,
      theme: ThemeData.dark().copyWith(
        scaffoldBackgroundColor: Colors.black,
        colorScheme: const ColorScheme.dark(
          primary: Colors.orangeAccent,
          secondary: Colors.blueAccent,
        ),
      ),
      home: const CalculatorScreen(),
    );
  }
}
