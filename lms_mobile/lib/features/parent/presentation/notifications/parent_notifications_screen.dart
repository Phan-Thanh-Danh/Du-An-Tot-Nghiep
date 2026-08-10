import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class ParentNotificationsScreen extends ConsumerStatefulWidget {
  const ParentNotificationsScreen({super.key});

  @override
  ConsumerState<ParentNotificationsScreen> createState() =>
      _ParentNotificationsScreenState();
}

class _ParentNotificationsScreenState
    extends ConsumerState<ParentNotificationsScreen> {
  final List<StudentNotification> _localNotifications = [];
  bool _isLoading = true;
  String? _error;
  String _activeFilter = 'Tất cả';

  @override
  void initState() {
    super.initState();
    _loadNotifications();
  }

  Future<void> _loadNotifications() async {
    try {
      final data = await ref.read(activeParentRepoProvider).getNotifications();
      if (!mounted) return;
      setState(() {
        _localNotifications
          ..clear()
          ..addAll(data);
        _isLoading = false;
        _error = null;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _error = error.toString();
      });
    }
  }

  Future<void> _markAsRead(String id) async {
    try {
      await ref.read(activeParentRepoProvider).markNotificationAsRead(id);
      if (!mounted) return;
      setState(() {
        final idx = _localNotifications.indexWhere((n) => n.id == id);
        if (idx != -1) {
          final old = _localNotifications[idx];
          _localNotifications[idx] = StudentNotification(
            id: old.id,
            title: old.title,
            content: old.content,
            timestamp: old.timestamp,
            isRead: true,
            type: old.type,
          );
        }
      });
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

  Future<void> _markAllAsRead() async {
    for (final notification in _localNotifications.where(
      (item) => !item.isRead,
    )) {
      await _markAsRead(notification.id);
      if (!mounted) return;
    }
  }

  List<StudentNotification> _getFilteredNotifications() {
    if (_activeFilter == 'Tất cả') {
      return _localNotifications;
    }
    NotificationType type;
    switch (_activeFilter) {
      case 'Học tập':
        type = NotificationType.academic;
        break;
      case 'Học phí':
        type = NotificationType.tuition;
        break;
      case 'Điểm số':
        type = NotificationType.grade;
        break;
      default:
        return _localNotifications;
    }
    return _localNotifications.where((n) => n.type == type).toList();
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Thông báo từ nhà trường')),
        body: const AppLoadingSkeleton(itemCount: 4),
      );
    }

    if (_error != null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Thông báo từ nhà trường')),
        body: AppErrorState(message: _error!, onRetry: _loadNotifications),
      );
    }

    final filteredList = _getFilteredNotifications();
    final hasUnread = _localNotifications.any((n) => !n.isRead);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Thông báo từ nhà trường'),
        centerTitle: true,
        actions: [
          if (hasUnread)
            IconButton(
              icon: const Icon(
                Icons.mark_chat_read_rounded,
                color: AppColors.success,
              ),
              tooltip: 'Đọc tất cả',
              onPressed: _markAllAsRead,
            ),
        ],
      ),
      body: Column(
        children: [
          // Filter list
          Container(
            height: 50,
            color: AppColors.surface,
            child: ListView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              children: [
                _buildFilterChip('Tất cả'),
                _buildFilterChip('Học tập'),
                _buildFilterChip('Học phí'),
                _buildFilterChip('Điểm số'),
              ],
            ),
          ),
          const SizedBox(height: 8),

          // Notifications list
          Expanded(
            child: filteredList.isEmpty
                ? const AppEmptyState(
                    title: 'Không có cảnh báo mới',
                    description:
                        'Hệ thống chưa ghi nhận thông báo mới nào dành cho phụ huynh.',
                    icon: Icons.notifications_off_rounded,
                  )
                : ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: filteredList.length,
                    separatorBuilder: (context, index) =>
                        const SizedBox(height: 12),
                    itemBuilder: (context, index) {
                      final n = filteredList[index];
                      return Card(
                        margin: EdgeInsets.zero,
                        color: n.isRead
                            ? AppColors.surface
                            : AppColors.successLight.withValues(alpha: 0.4),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                          side: BorderSide(
                            color: n.isRead
                                ? AppColors.border
                                : AppColors.success.withValues(alpha: 0.3),
                            width: n.isRead ? 1 : 1.5,
                          ),
                        ),
                        child: InkWell(
                          onTap: () {
                            if (!n.isRead) {
                              _markAsRead(n.id);
                            }
                            _showNotificationDetail(n);
                          },
                          borderRadius: BorderRadius.circular(12),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Row(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Container(
                                  padding: const EdgeInsets.all(8),
                                  decoration: BoxDecoration(
                                    color: _getIconBgColor(n.type),
                                    shape: BoxShape.circle,
                                  ),
                                  child: Icon(
                                    _getIconData(n.type),
                                    color: _getIconColor(n.type),
                                    size: 20,
                                  ),
                                ),
                                const SizedBox(width: 16),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Row(
                                        mainAxisAlignment:
                                            MainAxisAlignment.spaceBetween,
                                        children: [
                                          Expanded(
                                            child: Text(
                                              n.title,
                                              style: AppTextStyles.bodyMedium
                                                  .copyWith(
                                                    fontWeight: n.isRead
                                                        ? FontWeight.w600
                                                        : FontWeight.bold,
                                                    fontSize: 14,
                                                  ),
                                            ),
                                          ),
                                          if (!n.isRead)
                                            Container(
                                              width: 8,
                                              height: 8,
                                              decoration: const BoxDecoration(
                                                color: AppColors.success,
                                                shape: BoxShape.circle,
                                              ),
                                            ),
                                        ],
                                      ),
                                      const SizedBox(height: 6),
                                      Text(
                                        n.content,
                                        style: AppTextStyles.bodyRegular
                                            .copyWith(fontSize: 12),
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      const SizedBox(height: 8),
                                      Text(
                                        _formatTime(n.timestamp),
                                        style: AppTextStyles.caption.copyWith(
                                          fontSize: 10,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChip(String label) {
    final isActive = _activeFilter == label;
    return Padding(
      padding: const EdgeInsets.only(right: 8.0),
      child: ChoiceChip(
        label: Text(label),
        selected: isActive,
        onSelected: (selected) {
          if (selected) {
            setState(() {
              _activeFilter = label;
            });
          }
        },
        selectedColor: AppColors.successLight,
        backgroundColor: AppColors.background,
        labelStyle: TextStyle(
          color: isActive ? AppColors.success : AppColors.textSecondary,
          fontWeight: isActive ? FontWeight.bold : FontWeight.normal,
          fontSize: 12,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(
            color: isActive ? AppColors.success : AppColors.border,
          ),
        ),
      ),
    );
  }

  void _showNotificationDetail(StudentNotification n) {
    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(20),
          ),
          title: Text(n.title, style: AppTextStyles.subtitle),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                _formatTime(n.timestamp),
                style: AppTextStyles.caption.copyWith(
                  fontStyle: FontStyle.italic,
                ),
              ),
              const SizedBox(height: 12),
              Text(
                n.content,
                style: AppTextStyles.bodyRegular.copyWith(
                  color: AppColors.textPrimary,
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Đóng'),
            ),
          ],
        );
      },
    );
  }

  String _formatTime(DateTime dt) {
    final diff = DateTime.now().difference(dt);
    if (diff.inMinutes < 60) {
      return '${diff.inMinutes} phút trước';
    } else if (diff.inHours < 24) {
      return '${diff.inHours} giờ trước';
    } else {
      return DateFormat('dd/MM/yyyy HH:mm').format(dt);
    }
  }

  IconData _getIconData(NotificationType type) {
    switch (type) {
      case NotificationType.system:
        return Icons.settings_suggest_rounded;
      case NotificationType.academic:
        return Icons.school_rounded;
      case NotificationType.tuition:
        return Icons.payment_rounded;
      case NotificationType.grade:
        return Icons.grade_rounded;
      case NotificationType.general:
        return Icons.info_outline_rounded;
    }
  }

  Color _getIconColor(NotificationType type) {
    switch (type) {
      case NotificationType.system:
        return AppColors.textSecondary;
      case NotificationType.academic:
        return AppColors.success;
      case NotificationType.tuition:
        return AppColors.error;
      case NotificationType.grade:
        return AppColors.warning;
      case NotificationType.general:
        return AppColors.info;
    }
  }

  Color _getIconBgColor(NotificationType type) {
    switch (type) {
      case NotificationType.system:
        return AppColors.border.withValues(alpha: 0.5);
      case NotificationType.academic:
        return AppColors.successLight;
      case NotificationType.tuition:
        return AppColors.errorLight;
      case NotificationType.grade:
        return AppColors.warningLight;
      case NotificationType.general:
        return AppColors.infoLight;
    }
  }
}
