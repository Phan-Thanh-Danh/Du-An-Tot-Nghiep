import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';
import 'package:lms_mobile/features/parent/presentation/widgets/child_switcher.dart';

class ParentGradesScreen extends ConsumerStatefulWidget {
  const ParentGradesScreen({super.key});

  @override
  ConsumerState<ParentGradesScreen> createState() => _ParentGradesScreenState();
}

class _ParentGradesScreenState extends ConsumerState<ParentGradesScreen> {
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
    final activeChild = await ref.read(activeChildProvider.future);
    if (activeChild == null) {
      setState(() => _isLoading = false);
      return;
    }
    final repo = ref.read(activeParentRepoProvider);
    final grades = await repo.getChildGrades(activeChild.id);
    final gpas = await repo.getChildSemesterGPAs(activeChild.id);
    setState(() {
      _allGrades = grades;
      _gpas = gpas;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    // Re-trigger load when selected child switches
    ref.listen(activeChildIdProvider, (previous, next) {
      if (next != null) {
        setState(() => _isLoading = true);
        _loadGrades();
      }
    });

    final activeChildVal = ref.watch(activeChildProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Bảng điểm con em'), centerTitle: true),
      body: Column(
        children: [
          const ChildSwitcher(),
          Expanded(
            child: activeChildVal.when(
              loading: () => const AppLoadingSkeleton(itemCount: 3),
              error: (err, stack) => AppErrorState(message: err.toString()),
              data: (child) {
                if (child == null) {
                  return const AppEmptyState(
                    title: 'Trống',
                    description: 'Vui lòng liên kết tài khoản con em.',
                  );
                }

                if (_isLoading) {
                  return const AppLoadingSkeleton(itemCount: 3);
                }

                final semesters = [
                  'Tất cả',
                  ..._allGrades.map((g) => g.semester).toSet(),
                ];
                final displayedGrades = _selectedSemester == 'Tất cả'
                    ? _allGrades
                    : _allGrades
                          .where((g) => g.semester == _selectedSemester)
                          .toList();

                return SingleChildScrollView(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // GPA History chart
                      if (_gpas.isNotEmpty) ...[
                        Card(
                          margin: EdgeInsets.zero,
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Xu hướng GPA của ${child.fullName}',
                                  style: AppTextStyles.subtitle.copyWith(
                                    fontSize: 16,
                                  ),
                                ),
                                const SizedBox(height: 24),
                                SizedBox(
                                  height: 180,
                                  child: LineChart(
                                    LineChartData(
                                      gridData: const FlGridData(
                                        show: true,
                                        drawVerticalLine: false,
                                      ),
                                      titlesData: FlTitlesData(
                                        show: true,
                                        rightTitles: const AxisTitles(
                                          sideTitles: SideTitles(
                                            showTitles: false,
                                          ),
                                        ),
                                        topTitles: const AxisTitles(
                                          sideTitles: SideTitles(
                                            showTitles: false,
                                          ),
                                        ),
                                        bottomTitles: AxisTitles(
                                          sideTitles: SideTitles(
                                            showTitles: true,
                                            reservedSize: 30,
                                            interval: 1,
                                            getTitlesWidget: (value, meta) {
                                              final idx = value.toInt();
                                              if (idx >= 0 &&
                                                  idx < _gpas.length) {
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
                                                final hk =
                                                    'HK${semester ?? '?'}';
                                                return Padding(
                                                  padding:
                                                      const EdgeInsets.only(
                                                        top: 8.0,
                                                      ),
                                                  child: Text(
                                                    '$hk $years',
                                                    style: AppTextStyles.caption
                                                        .copyWith(fontSize: 9),
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
                                            getTitlesWidget: (value, meta) =>
                                                Text(
                                                  value.toStringAsFixed(1),
                                                  style: AppTextStyles.caption
                                                      .copyWith(fontSize: 10),
                                                ),
                                            reservedSize: 28,
                                          ),
                                        ),
                                      ),
                                      borderData: FlBorderData(
                                        show: true,
                                        border: Border.all(
                                          color: AppColors.border,
                                        ),
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
                                          color: AppColors.success,
                                          barWidth: 3,
                                          dotData: const FlDotData(show: true),
                                          belowBarData: BarAreaData(
                                            show: true,
                                            color: AppColors.success.withValues(
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

                      // Dropdown filter
                      Row(
                        children: [
                          Text('Chọn học kỳ:', style: AppTextStyles.bodyMedium),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 12,
                              ),
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
                                    color: AppColors.success,
                                  ),
                                  isExpanded: true,
                                  items: semesters.map((String sem) {
                                    return DropdownMenuItem<String>(
                                      value: sem,
                                      child: Text(
                                        sem,
                                        style: AppTextStyles.bodyMedium,
                                      ),
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

                      // Grades list
                      ListView.separated(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: displayedGrades.length,
                        separatorBuilder: (context, index) =>
                            const SizedBox(height: 12),
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
                                    mainAxisAlignment:
                                        MainAxisAlignment.spaceBetween,
                                    children: [
                                      Expanded(
                                        child: Text(
                                          grade.courseName,
                                          style: AppTextStyles.bodyMedium
                                              .copyWith(
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
                                            borderRadius: BorderRadius.circular(
                                              8,
                                            ),
                                          ),
                                          child: Text(
                                            grade.letterGrade!,
                                            style: AppTextStyles.caption
                                                .copyWith(
                                                  color:
                                                      (grade.totalScore ?? 0) <
                                                          5
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
                                    mainAxisAlignment:
                                        MainAxisAlignment.spaceBetween,
                                    children: [
                                      _buildScoreCol(
                                        'Quá trình (30%)',
                                        grade.processScore,
                                      ),
                                      _buildScoreCol(
                                        'Giữa kỳ (30%)',
                                        grade.midtermScore,
                                      ),
                                      _buildScoreCol(
                                        'Cuối kỳ (40%)',
                                        grade.finalScore,
                                      ),
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
                );
              },
            ),
          ),
        ],
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
            color: isTotal ? AppColors.success : AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}
