String formatScore(num? value) {
  if (value == null) return '-';
  final number = value.toDouble();
  if (!number.isFinite) return '-';

  return number
      .toStringAsFixed(2)
      .replaceFirst(RegExp(r'0+$'), '')
      .replaceFirst(RegExp(r'\.$'), '');
}
