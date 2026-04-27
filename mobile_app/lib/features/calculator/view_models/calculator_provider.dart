import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:math_expressions/math_expressions.dart';

class CalculatorState {
  final String expression;
  final String result;
  final bool hasError;

  const CalculatorState({
    required this.expression,
    required this.result,
    required this.hasError,
  });

  CalculatorState copyWith({
    String? expression,
    String? result,
    bool? hasError,
  }) {
    return CalculatorState(
      expression: expression ?? this.expression,
      result: result ?? this.result,
      hasError: hasError ?? this.hasError,
    );
  }

  // Chuyển đổi biểu thức người dùng nhập thành chuẩn LaTeX cho flutter_math_fork
  String get asLaTeX {
    if (expression.isEmpty) return '';
    String tex = expression;
    tex = tex.replaceAll('*', r' \times ');
    tex = tex.replaceAll('/', r' \div ');
    tex = tex.replaceAll('pi', r'\pi');
    tex = tex.replaceAll('sqrt(', r'\sqrt{');
    // Basic bracket fix for sqrt in MVP
    tex = tex.replaceAll(')', '}');
    return tex;
  }
}

class CalculatorNotifier extends Notifier<CalculatorState> {
  @override
  CalculatorState build() {
    return const CalculatorState(expression: '', result: '', hasError: false);
  }

  void onKeyPress(String key) {
    if (state.hasError) {
      state = const CalculatorState(expression: '', result: '', hasError: false);
    }

    switch (key) {
      case 'AC':
        state = const CalculatorState(expression: '', result: '', hasError: false);
        break;
      case 'DEL':
        if (state.expression.isNotEmpty) {
          state = state.copyWith(
            expression: state.expression.substring(0, state.expression.length - 1),
          );
        }
        break;
      case '=':
        _evaluate();
        break;
      default:
        state = state.copyWith(expression: state.expression + key);
        break;
    }
  }

  void _evaluate() {
    if (state.expression.isEmpty) return;
    try {
      // Trong Phase 1, dùng math_expressions parse thành AST thay vì tự viết Lexer
      Parser p = Parser();
      Expression exp = p.parse(state.expression);
      ContextModel cm = ContextModel();
      double eval = exp.evaluate(EvaluationType.REAL, cm);
      
      String resStr = eval.toString();
      // Remove trailing .0 for cleaner display
      if (resStr.endsWith('.0')) {
        resStr = resStr.substring(0, resStr.length - 2);
      }
      
      state = state.copyWith(result: resStr, hasError: false);
    } catch (e) {
      state = state.copyWith(result: 'Syntax Error', hasError: true);
    }
  }
}

final calculatorProvider = NotifierProvider<CalculatorNotifier, CalculatorState>(() {
  return CalculatorNotifier();
});