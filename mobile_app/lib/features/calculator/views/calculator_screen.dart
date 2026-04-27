import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_math_fork/flutter_math.dart';
import 'package:vibration/vibration.dart';

import '../view_models/calculator_provider.dart';

class CalculatorScreen extends ConsumerWidget {
  const CalculatorScreen({super.key});

  void _handlePress(WidgetRef ref, String key) async {
    // Kích hoạt Haptic (Light impact)
    if (await Vibration.hasVibrator() ?? false) {
      Vibration.vibrate(duration: 15, amplitude: 50);
    }
    ref.read(calculatorProvider.notifier).onKeyPress(key);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(calculatorProvider);

    return Scaffold(
      backgroundColor: Colors.black, // Dark Mode chuẩn
      body: SafeArea(
        child: Column(
          children: [
            // Display Zone (Top 35%)
            Expanded(
              flex: 3,
              child: Container(
                width: double.infinity,
                padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
                decoration: const BoxDecoration(
                  color: Color(0xFF1A1A1C),
                  borderRadius: BorderRadius.only(
                    bottomLeft: Radius.circular(24),
                    bottomRight: Radius.circular(24),
                  ),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.end,
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    // LaTeX Expression Display
                    SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      reverse: true,
                      child: Math.tex(
                        state.asLaTeX.isEmpty ? '\\text{ClassWiz-X}' : state.asLaTeX,
                        mathStyle: MathStyle.display,
                        textStyle: TextStyle(
                          fontSize: 36,
                          color: state.expression.isEmpty ? Colors.grey : Colors.white,
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    // Result Display
                    Text(
                      state.result,
                      style: TextStyle(
                        fontSize: 48,
                        fontWeight: FontWeight.w600,
                        color: state.hasError ? Colors.redAccent : Colors.greenAccent,
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // Keypad Zone (Bottom 65%)
            Expanded(
              flex: 5,
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                  children: [
                    // Function Row
                    _buildRow(ref, ['SHIFT', 'ALPHA', 'SETUP', 'ON'],
                        colors: {
                          'SHIFT': Colors.orangeAccent,
                          'ALPHA': Colors.redAccent,
                          'ON': Colors.white
                        },
                        defaultColor: Colors.grey[800]!),
                    
                    // Scientific Row
                    _buildRow(ref, ['sin(', 'cos(', 'tan(', '^'],
                        defaultColor: const Color(0xFF2C2C2E)),

                    // Casio Grid
                    _buildRow(ref, ['7', '8', '9', 'DEL', 'AC'],
                        colors: {
                          'DEL': Colors.deepOrange,
                          'AC': Colors.deepOrange
                        }),
                    _buildRow(ref, ['4', '5', '6', '*', '/']),
                    _buildRow(ref, ['1', '2', '3', '+', '-']),
                    _buildRow(ref, ['0', '.', 'sqrt(', 'Ans', '='],
                        colors: {
                          '=': Colors.blueAccent
                        }),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRow(WidgetRef ref, List<String> keys,
      {Map<String, Color> colors = const {},
      Color defaultColor = const Color(0xFF3A3A3C)}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: keys.map((key) {
        final color = colors[key] ?? defaultColor;
        
        // Phím '=' bự hơn một chút giống Casio
        final isEquals = key == '=';

        return Expanded(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 4.0, vertical: 8.0),
            child: Material(
              color: Colors.transparent,
              child: InkWell(
                onTap: () => _handlePress(ref, key),
                borderRadius: BorderRadius.circular(12),
                child: Ink(
                  height: isEquals ? 70 : 60,
                  decoration: BoxDecoration(
                    color: color,
                    borderRadius: BorderRadius.circular(12),
                    boxShadow: const [
                      BoxShadow(
                        color: Colors.black54,
                        offset: Offset(0, 4),
                        blurRadius: 4,
                      )
                    ],
                  ),
                  child: Center(
                    child: Text(
                      key.replaceAll('(', ''), // Tắt dấu ngoặc khi hiển thị phím
                      style: TextStyle(
                        fontSize: isEquals ? 28 : 20,
                        fontWeight: isEquals ? FontWeight.bold : FontWeight.w500,
                        color: key == 'ON' ? Colors.black : Colors.white,
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        );
      }).toList(),
    );
  }
}
