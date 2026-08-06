import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentCourseDetailScreen extends ConsumerStatefulWidget {
  final String courseId;

  const StudentCourseDetailScreen({super.key, required this.courseId});

  @override
  ConsumerState<StudentCourseDetailScreen> createState() =>
      _StudentCourseDetailScreenState();
}

class _StudentCourseDetailScreenState
    extends ConsumerState<StudentCourseDetailScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  Lesson? _activeLesson;
  final List<String> _comments = [
    'Bài giảng rất hay và trực quan ạ!',
    'Em gặp lỗi khi chạy lệnh flutter pub get, có ai giúp em với?',
    'Đã hoàn thành bài học này, đang làm bài tập giao diện login.',
  ];
  final _commentController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    _commentController.dispose();
    super.dispose();
  }

  void _setActiveLesson(Lesson lesson) {
    setState(() {
      _activeLesson = lesson;
    });
  }

  void _addComment() {
    if (_commentController.text.trim().isNotEmpty) {
      setState(() {
        _comments.insert(0, _commentController.text.trim());
        _commentController.clear();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final repo = ref.watch(activeStudentRepoProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Chi tiết khóa học')),
      body: FutureBuilder<List<Lesson>>(
        future: repo.getCourseLessons(widget.courseId),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoadingSkeleton(itemCount: 4);
          }
          if (snapshot.hasError) {
            return AppErrorState(message: snapshot.error.toString());
          }
          final lessons = snapshot.data ?? [];
          if (lessons.isEmpty) {
            return const AppEmptyState(
              title: 'Bài học trống',
              description:
                  'Chưa có bài học nào được đăng tải cho khóa học này.',
              icon: Icons.play_lesson_outlined,
            );
          }

          // Default active lesson
          _activeLesson ??= lessons.first;

          return Column(
            children: [
              // Top Player Placeholder
              _buildPlayerPanel(),

              // Tabs header
              Container(
                color: AppColors.surface,
                child: TabBar(
                  controller: _tabController,
                  labelColor: AppColors.primary,
                  unselectedLabelColor: AppColors.textSecondary,
                  indicatorColor: AppColors.primary,
                  tabs: const [
                    Tab(text: 'Danh sách bài học'),
                    Tab(text: 'Thảo luận'),
                  ],
                ),
              ),

              // Tab contents
              Expanded(
                child: TabBarView(
                  controller: _tabController,
                  children: [
                    // Lessons list
                    ListView.separated(
                      padding: const EdgeInsets.all(16),
                      itemCount: lessons.length,
                      separatorBuilder: (context, index) =>
                          const SizedBox(height: 12),
                      itemBuilder: (context, index) {
                        final lesson = lessons[index];
                        final isActive = _activeLesson?.id == lesson.id;
                        return Card(
                          margin: EdgeInsets.zero,
                          color: isActive
                              ? AppColors.primaryLight
                              : AppColors.surface,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                            side: BorderSide(
                              color: isActive
                                  ? AppColors.primary
                                  : AppColors.border,
                              width: isActive ? 1.5 : 1,
                            ),
                          ),
                          child: ListTile(
                            onTap: () => _setActiveLesson(lesson),
                            leading: Icon(
                              lesson.isCompleted
                                  ? Icons.check_circle_rounded
                                  : _getIconForType(lesson.type),
                              color: lesson.isCompleted
                                  ? AppColors.success
                                  : (isActive
                                        ? AppColors.primary
                                        : AppColors.textSecondary),
                            ),
                            title: Text(
                              lesson.title,
                              style: AppTextStyles.bodyMedium.copyWith(
                                fontWeight: isActive
                                    ? FontWeight.bold
                                    : FontWeight.normal,
                                color: isActive
                                    ? AppColors.primary
                                    : AppColors.textPrimary,
                              ),
                            ),
                            subtitle: Text(
                              '${lesson.durationMinutes} phút',
                              style: AppTextStyles.caption,
                            ),
                            trailing: const Icon(
                              Icons.play_circle_outline_rounded,
                              size: 20,
                            ),
                          ),
                        );
                      },
                    ),

                    // Comments discussion
                    Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        children: [
                          Row(
                            children: [
                              Expanded(
                                child: TextField(
                                  controller: _commentController,
                                  decoration: const InputDecoration(
                                    hintText: 'Nhập bình luận của bạn...',
                                    border: OutlineInputBorder(),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 8),
                              IconButton(
                                icon: const Icon(
                                  Icons.send_rounded,
                                  color: AppColors.primary,
                                ),
                                onPressed: _addComment,
                              ),
                            ],
                          ),
                          const SizedBox(height: 16),
                          Expanded(
                            child: ListView.separated(
                              itemCount: _comments.length,
                              separatorBuilder: (context, index) =>
                                  const Divider(color: AppColors.border),
                              itemBuilder: (context, index) {
                                return Row(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const CircleAvatar(
                                      radius: 18,
                                      backgroundColor: AppColors.primaryLight,
                                      child: Icon(
                                        Icons.person,
                                        size: 18,
                                        color: AppColors.primary,
                                      ),
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            index == 0 &&
                                                    _commentController
                                                        .text
                                                        .isEmpty
                                                ? 'Bạn'
                                                : 'Học viên khác',
                                            style: AppTextStyles.bodyMedium
                                                .copyWith(
                                                  fontWeight: FontWeight.bold,
                                                ),
                                          ),
                                          const SizedBox(height: 4),
                                          Text(
                                            _comments[index],
                                            style: AppTextStyles.bodyRegular
                                                .copyWith(
                                                  color: AppColors.textPrimary,
                                                ),
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                );
                              },
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildPlayerPanel() {
    if (_activeLesson == null) return const SizedBox.shrink();

    Widget playerMockup;
    if (_activeLesson!.type == LessonType.video) {
      playerMockup = AspectRatio(
        aspectRatio: 16 / 9,
        child: Container(
          color: Colors.black,
          child: Stack(
            alignment: Alignment.center,
            children: [
              // Mock Video Banner
              const Positioned.fill(
                child: Icon(
                  Icons.smart_display_rounded,
                  color: Colors.white,
                  size: 64,
                ),
              ),
              Positioned(
                bottom: 8,
                left: 16,
                right: 16,
                child: Row(
                  children: [
                    const Icon(Icons.play_arrow_rounded, color: Colors.white),
                    const SizedBox(width: 8),
                    Expanded(
                      child: LinearProgressIndicator(
                        value: 0.35,
                        backgroundColor: Colors.white.withValues(alpha: 0.3),
                        valueColor: const AlwaysStoppedAnimation<Color>(
                          AppColors.primary,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    const Text(
                      '12:45 / 45:00',
                      style: TextStyle(color: Colors.white, fontSize: 10),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      );
    } else if (_activeLesson!.type == LessonType.pdf) {
      playerMockup = Container(
        width: double.infinity,
        constraints: const BoxConstraints(minHeight: 190),
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 22),
        decoration: BoxDecoration(
          color: AppColors.errorLight.withValues(alpha: 0.38),
          border: const Border(bottom: BorderSide(color: AppColors.border)),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(18),
              ),
              child: const Icon(
                Icons.picture_as_pdf_rounded,
                size: 40,
                color: AppColors.error,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Tài liệu: ${_activeLesson!.title}.pdf',
              style: AppTextStyles.bodyMedium.copyWith(
                fontWeight: FontWeight.w700,
              ),
              textAlign: TextAlign.center,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
            const SizedBox(height: 12),
            ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 300),
              child: SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: () {},
                  icon: const Icon(Icons.download_rounded),
                  label: const Text('Tải xuống tài liệu'),
                ),
              ),
            ),
          ],
        ),
      );
    } else {
      playerMockup = Container(
        width: double.infinity,
        height: 180,
        color: AppColors.background,
        padding: const EdgeInsets.all(16),
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(_activeLesson!.title, style: AppTextStyles.subtitle),
              const SizedBox(height: 8),
              Text(_activeLesson!.content, style: AppTextStyles.bodyRegular),
            ],
          ),
        ),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(width: double.infinity, child: playerMockup),
        Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                _activeLesson!.title,
                style: AppTextStyles.subtitle.copyWith(fontSize: 18),
              ),
              const SizedBox(height: 8),
              Text(_activeLesson!.content, style: AppTextStyles.bodyRegular),
            ],
          ),
        ),
        const Divider(height: 1, color: AppColors.border),
      ],
    );
  }

  IconData _getIconForType(LessonType type) {
    switch (type) {
      case LessonType.video:
        return Icons.play_circle_fill_rounded;
      case LessonType.pdf:
        return Icons.picture_as_pdf_rounded;
      case LessonType.text:
        return Icons.description_rounded;
    }
  }
}
