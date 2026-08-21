import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:file_picker/file_picker.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/utils/number_formatters.dart';
import 'package:lms_mobile/core/widgets/app_status_badge.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/core/widgets/app_buttons.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentAssignmentsScreen extends ConsumerStatefulWidget {
  const StudentAssignmentsScreen({super.key});

  @override
  ConsumerState<StudentAssignmentsScreen> createState() =>
      _StudentAssignmentsScreenState();
}

class _StudentAssignmentsScreenState
    extends ConsumerState<StudentAssignmentsScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final List<Assignment> _localAssignments = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadAssignments();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadAssignments() async {
    final repo = ref.read(activeStudentRepoProvider);
    final data = await repo.getAssignments();
    setState(() {
      _localAssignments.clear();
      _localAssignments.addAll(data);
      _isLoading = false;
    });
  }

  Future<void> _submitAssignment(String id) async {
    final result = await FilePicker.platform.pickFiles(withData: false);
    final path = result?.files.single.path;
    if (path == null) return;
    try {
      await ref.read(activeStudentRepoProvider).submitAssignment(id, path);
      await _loadAssignments();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Nộp bài tập thành công.'),
          backgroundColor: AppColors.success,
        ),
      );
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(error.toString()),
          backgroundColor: AppColors.error,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Bài tập về nhà'),
        centerTitle: true,
        bottom: TabBar(
          controller: _tabController,
          labelColor: AppColors.primary,
          unselectedLabelColor: AppColors.textSecondary,
          indicatorColor: AppColors.primary,
          tabs: const [
            Tab(text: 'Chưa nộp'),
            Tab(text: 'Đã nộp'),
            Tab(text: 'Quá hạn'),
          ],
        ),
      ),
      body: _isLoading
          ? const AppLoadingSkeleton(itemCount: 3)
          : TabBarView(
              controller: _tabController,
              children: [
                _buildAssignmentList(AssignmentStatus.notSubmitted),
                _buildAssignmentList(AssignmentStatus.submitted),
                _buildAssignmentList(AssignmentStatus.overdue),
              ],
            ),
    );
  }

  Widget _buildAssignmentList(AssignmentStatus status) {
    final list = _localAssignments.where((a) => a.status == status).toList();

    if (list.isEmpty) {
      String title = '';
      String desc = '';
      IconData icon = Icons.check_circle_outline_rounded;

      if (status == AssignmentStatus.notSubmitted) {
        title = 'Hoàn thành tất cả!';
        desc = 'Bạn không có bài tập nào chưa nộp.';
      } else if (status == AssignmentStatus.submitted) {
        title = 'Trống';
        desc = 'Bạn chưa nộp bài tập nào gần đây.';
        icon = Icons.upload_file_rounded;
      } else {
        title = 'Tuyệt vời!';
        desc = 'Không có bài tập nào bị quá hạn.';
        icon = Icons.alarm_on_rounded;
      }

      return AppEmptyState(title: title, description: desc, icon: icon);
    }

    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: list.length,
      separatorBuilder: (context, index) => const SizedBox(height: 16),
      itemBuilder: (context, index) {
        final a = list[index];
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
                        a.title,
                        style: AppTextStyles.bodyMedium.copyWith(
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                        ),
                      ),
                    ),
                    _buildBadge(a),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                  a.courseName,
                  style: AppTextStyles.caption.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 12),
                Text(a.description, style: AppTextStyles.bodyRegular),
                const SizedBox(height: 16),
                const Divider(height: 1),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Hạn nộp:',
                          style: AppTextStyles.caption.copyWith(fontSize: 10),
                        ),
                        Text(
                          DateFormat('dd/MM/yyyy HH:mm').format(a.dueDate),
                          style: AppTextStyles.caption.copyWith(
                            color: a.status == AssignmentStatus.overdue
                                ? AppColors.error
                                : AppColors.textPrimary,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                    if (a.submitDate != null)
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Thời gian nộp:',
                            style: AppTextStyles.caption.copyWith(fontSize: 10),
                          ),
                          Text(
                            DateFormat(
                              'dd/MM/yyyy HH:mm',
                            ).format(a.submitDate!),
                            style: AppTextStyles.caption.copyWith(
                              color: AppColors.success,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    if (a.score != null)
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          Text(
                            'Điểm số:',
                            style: AppTextStyles.caption.copyWith(fontSize: 10),
                          ),
                          Text(
                            '${formatScore(a.score)} / 10',
                            style: AppTextStyles.subtitle.copyWith(
                              color: AppColors.primary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                  ],
                ),
                if (a.status == AssignmentStatus.notSubmitted) ...[
                  const SizedBox(height: 16),
                  AppPrimaryButton(
                    text: 'Chọn tệp và nộp bài',
                    onPressed: () => _submitAssignment(a.id),
                    icon: Icons.upload_file_rounded,
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildBadge(Assignment a) {
    if (a.status == AssignmentStatus.submitted) {
      return const AppStatusBadge(label: 'Đã nộp', type: BadgeType.success);
    } else if (a.status == AssignmentStatus.overdue) {
      return const AppStatusBadge(label: 'Quá hạn', type: BadgeType.error);
    } else {
      return const AppStatusBadge(label: 'Chưa nộp', type: BadgeType.warning);
    }
  }
}
