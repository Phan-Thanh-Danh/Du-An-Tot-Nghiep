import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentGradesScreen extends ConsumerStatefulWidget {
  const StudentGradesScreen({super.key});

  @override
  ConsumerState<StudentGradesScreen> createState() =>
      _StudentGradesScreenState();
}

class _StudentGradesScreenState extends ConsumerState<StudentGradesScreen> {
  String _selectedSemester = 'Tất cả';
  List<GradeRecord> _allGrades = [];
  List<SemesterGPA> _gpas = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadGrades();
  }

  Future<void> _loadGrades() async {
    final repo = ref.read(activeStudentRepoProvider);
    final grades = await repo.getGradeRecords();
    final gpas = await repo.getSemesterGPAs();
    setState(() {
      _allGrades = grades;
      _gpas = gpas;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Bảng điểm cá nhân')),
        body: const AppLoadingSkeleton(itemCount: 4),
      );
    }

    final semesters = ['Tất cả', ..._allGrades.map((g) => g.semester).toSet()];
    final displayedGrades = _selectedSemester == 'Tất cả'
        ? _allGrades
        : _allGrades.where((g) => g.semester == _selectedSemester).toList();

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Bảng điểm cá nhân'), centerTitle: true),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Chart Section
            if (_gpas.isNotEmpty) ...[
              Card(
                margin: EdgeInsets.zero,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Xu hướng GPA',
                        style: AppTextStyles.subtitle.copyWith(fontSize: 16),
                      ),
                      const SizedBox(height: 24),
                      SizedBox(
                        height: 180,
                        child: LineChart(
                          LineChartData(
                            gridData: FlGridData(
                              show: true,
                              drawVerticalLine: false,
                              getDrawingHorizontalLine: (value) => FlLine(
                                color: AppColors.border,
                                strokeWidth: 1,
                              ),
                            ),
                            titlesData: FlTitlesData(
                              show: true,
                              rightTitles: const AxisTitles(
                                sideTitles: SideTitles(showTitles: false),
                              ),
                              topTitles: const AxisTitles(
                                sideTitles: SideTitles(showTitles: false),
                              ),
                              bottomTitles: AxisTitles(
                                sideTitles: SideTitles(
                                  showTitles: true,
                                  reservedSize: 30,
                                  interval: 1,
                                  getTitlesWidget: (value, meta) {
                                    final idx = value.toInt();
                                    if (idx >= 0 && idx < _gpas.length) {
                                      final raw = _gpas[idx].semester;
                                      final semester = RegExp(
                                        r'(?:Học kỳ|HK)\s*(\d+)',
                                        caseSensitive: false,
                                      ).firstMatch(raw)?.group(1);
                                      final yearMatch = RegExp(
                                        r'(\d{4})\D+(\d{4})',
                                      ).firstMatch(raw);
                                      final years = yearMatch == null
                                          ? ''
                                          : '${yearMatch.group(1)!.substring(2)}-${yearMatch.group(2)!.substring(2)}';
                                      final hk = 'HK${semester ?? '?'}';
                                      return Padding(
                                        padding: const EdgeInsets.only(
                                          top: 8.0,
                                        ),
                                        child: Text(
                                          '$hk $years',
                                          style: AppTextStyles.caption.copyWith(
                                            fontSize: 9,
                                          ),
                                        ),
                                      );
                                    }
                                    return const Text('');
                                  },
                                ),
                              ),
                              leftTitles: AxisTitles(
                                sideTitles: SideTitles(
                                  showTitles: true,
                                  interval: 1,
                                  getTitlesWidget: (value, meta) {
                                    return Text(
                                      value.toStringAsFixed(1),
                                      style: AppTextStyles.caption.copyWith(
                                        fontSize: 10,
                                      ),
                                    );
                                  },
                                  reservedSize: 28,
                                ),
                              ),
                            ),
                            borderData: FlBorderData(
                              show: true,
                              border: Border.all(color: AppColors.border),
                            ),
                            minX: 0,
                            maxX: (_gpas.length - 1).toDouble(),
                            minY: 0,
                            maxY: 10,
                            lineBarsData: [
                              LineChartBarData(
                                spots: List.generate(
                                  _gpas.length,
                                  (index) => FlSpot(
                                    index.toDouble(),
                                    _gpas[index].gpa,
                                  ),
                                ),
                                isCurved: true,
                                color: AppColors.primary,
                                barWidth: 3,
                                isStrokeCapRound: true,
                                dotData: const FlDotData(show: true),
                                belowBarData: BarAreaData(
                                  show: true,
                                  color: AppColors.primary.withValues(
                                    alpha: 0.1,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
            ],

            // Semester Selector
            Row(
              children: [
                Text('Chọn học kỳ:', style: AppTextStyles.bodyMedium),
                const SizedBox(width: 12),
                Expanded(
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.border),
                    ),
                    child: DropdownButtonHideUnderline(
                      child: DropdownButton<String>(
                        value: _selectedSemester,
                        icon: const Icon(
                          Icons.arrow_drop_down,
                          color: AppColors.primary,
                        ),
                        isExpanded: true,
                        items: semesters.map((String sem) {
                          return DropdownMenuItem<String>(
                            value: sem,
                            child: Text(sem, style: AppTextStyles.bodyMedium),
                          );
                        }).toList(),
                        onChanged: (newValue) {
                          if (newValue != null) {
                            setState(() {
                              _selectedSemester = newValue;
                            });
                          }
                        },
                      ),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Subject grades list
            ListView.separated(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: displayedGrades.length,
              separatorBuilder: (context, index) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final grade = displayedGrades[index];
                return Card(
                  margin: EdgeInsets.zero,
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Expanded(
                              child: Text(
                                grade.courseName,
                                style: AppTextStyles.bodyMedium.copyWith(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 15,
                                ),
                              ),
                            ),
                            if (grade.letterGrade != null)
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 10,
                                  vertical: 4,
                                ),
                                decoration: BoxDecoration(
                                  color: (grade.totalScore ?? 0) < 5
                                      ? AppColors.errorLight
                                      : AppColors.successLight,
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  grade.letterGrade!,
                                  style: AppTextStyles.caption.copyWith(
                                    color: (grade.totalScore ?? 0) < 5
                                        ? AppColors.error
                                        : AppColors.success,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                          ],
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Mã môn: ${grade.courseCode}  •  Số TC: ${grade.creditCount}',
                          style: AppTextStyles.caption,
                        ),
                        const SizedBox(height: 12),
                        const Divider(height: 1),
                        const SizedBox(height: 12),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildScoreCol(
                              'Quá trình (30%)',
                              grade.processScore,
                            ),
                            _buildScoreCol('Giữa kỳ (30%)', grade.midtermScore),
                            _buildScoreCol('Cuối kỳ (40%)', grade.finalScore),
                            _buildScoreCol(
                              'Tổng kết',
                              grade.totalScore,
                              isTotal: true,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildScoreCol(String label, double? score, {bool isTotal = false}) {
    return Column(
      crossAxisAlignment: isTotal
          ? CrossAxisAlignment.end
          : CrossAxisAlignment.start,
      children: [
        Text(label, style: AppTextStyles.caption.copyWith(fontSize: 10)),
        const SizedBox(height: 4),
        Text(
          formatScore(score),
          style: AppTextStyles.bodyMedium.copyWith(
            fontWeight: isTotal ? FontWeight.bold : FontWeight.w500,
            fontSize: isTotal ? 16 : 13,
            color: isTotal ? AppColors.primary : AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}
