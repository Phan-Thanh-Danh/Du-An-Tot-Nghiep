import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:table_calendar/table_calendar.dart';
import 'package:intl/intl.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/parent/data/active_child_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';
import 'package:lms_mobile/features/parent/presentation/widgets/child_switcher.dart';

class ParentScheduleScreen extends ConsumerStatefulWidget {
  const ParentScheduleScreen({super.key});

  @override
  ConsumerState<ParentScheduleScreen> createState() =>
      _ParentScheduleScreenState();
}

class _ParentScheduleScreenState extends ConsumerState<ParentScheduleScreen> {
  CalendarFormat _calendarFormat = CalendarFormat.week;
  DateTime _focusedDay = DateTime.now();
  DateTime? _selectedDay;
  List<ScheduleEvent> _allEvents = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _selectedDay = _focusedDay;
    _loadEvents();
  }

  Future<void> _loadEvents() async {
    final activeChild = await ref.read(activeChildProvider.future);
    if (activeChild == null) {
      setState(() => _isLoading = false);
      return;
    }
    final repo = ref.read(activeParentRepoProvider);
    final events = await repo.getChildSchedule(activeChild.id);
    setState(() {
      _allEvents = events;
      _isLoading = false;
    });
  }

  List<ScheduleEvent> _getEventsForDay(DateTime day) {
    return _allEvents.where((e) {
      return e.date.year == day.year &&
          e.date.month == day.month &&
          e.date.day == day.day;
    }).toList();
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(activeChildIdProvider, (previous, next) {
      if (next != null) {
        setState(() => _isLoading = true);
        _loadEvents();
      }
    });

    final activeChildVal = ref.watch(activeChildProvider);

    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Thời khóa biểu của con')),
        body: const AppLoadingSkeleton(itemCount: 2),
      );
    }

    final dailyEvents = _getEventsForDay(_selectedDay ?? _focusedDay);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Thời khóa biểu của con'),
        centerTitle: true,
      ),
      body: Column(
        children: [
          const ChildSwitcher(),
          activeChildVal.when(
            loading: () =>
                const Expanded(child: AppLoadingSkeleton(itemCount: 1)),
            error: (err, stack) =>
                Expanded(child: AppErrorState(message: err.toString())),
            data: (child) {
              if (child == null) {
                return const Expanded(
                  child: AppEmptyState(
                    title: 'Trống',
                    description: 'Vui lòng liên kết tài khoản con em.',
                  ),
                );
              }

              return Expanded(
                child: Column(
                  children: [
                    // Calendar
                    Container(
                      margin: const EdgeInsets.fromLTRB(12, 10, 12, 0),
                      padding: const EdgeInsets.only(bottom: 10),
                      clipBehavior: Clip.antiAlias,
                      decoration: BoxDecoration(
                        color: AppColors.surface,
                        borderRadius: BorderRadius.circular(20),
                        border: Border.all(color: AppColors.border),
                        boxShadow: [
                          BoxShadow(
                            color: AppColors.cardShadow.withValues(alpha: 0.05),
                            blurRadius: 18,
                            offset: const Offset(0, 8),
                          ),
                        ],
                      ),
                      child: TableCalendar<ScheduleEvent>(
                        locale: 'vi_VN',
                        firstDay: DateTime.now().subtract(
                          const Duration(days: 30),
                        ),
                        lastDay: DateTime.now().add(const Duration(days: 30)),
                        focusedDay: _focusedDay,
                        calendarFormat: _calendarFormat,
                        selectedDayPredicate: (day) =>
                            isSameDay(_selectedDay, day),
                        onDaySelected: (selectedDay, focusedDay) {
                          if (!isSameDay(_selectedDay, selectedDay)) {
                            setState(() {
                              _selectedDay = selectedDay;
                              _focusedDay = focusedDay;
                            });
                          }
                        },
                        onFormatChanged: (format) {
                          if (_calendarFormat != format) {
                            setState(() {
                              _calendarFormat = format;
                            });
                          }
                        },
                        onPageChanged: (focusedDay) {
                          _focusedDay = focusedDay;
                        },
                        eventLoader: _getEventsForDay,
                        calendarBuilders: CalendarBuilders<ScheduleEvent>(
                          markerBuilder: (context, day, events) {
                            if (events.isEmpty) return null;
                            final isSelected = isSameDay(_selectedDay, day);
                            return SizedBox(
                              width: 44,
                              height: 44,
                              child: AnimatedAlign(
                                duration: const Duration(milliseconds: 320),
                                curve: Curves.easeOutCubic,
                                alignment: isSelected
                                    ? Alignment.center
                                    : Alignment.bottomCenter,
                                child: AnimatedContainer(
                                  duration: const Duration(milliseconds: 320),
                                  curve: Curves.easeOutCubic,
                                  width: isSelected ? 42 : 6,
                                  height: isSelected ? 42 : 6,
                                  decoration: BoxDecoration(
                                    color: isSelected
                                        ? AppColors.successLight
                                        : AppColors.success,
                                    shape: BoxShape.circle,
                                    border: isSelected
                                        ? Border.all(
                                            color: AppColors.success.withValues(
                                              alpha: 0.42,
                                            ),
                                            width: 1.2,
                                          )
                                        : null,
                                    boxShadow: [
                                      BoxShadow(
                                        color: AppColors.success.withValues(
                                          alpha: isSelected ? 0.14 : 0.22,
                                        ),
                                        blurRadius: isSelected ? 10 : 4,
                                        offset: Offset(0, isSelected ? 4 : 1),
                                      ),
                                    ],
                                  ),
                                  child: AnimatedSwitcher(
                                    duration: const Duration(milliseconds: 180),
                                    child: isSelected
                                        ? Center(
                                            key: const ValueKey('selected-day'),
                                            child: Text(
                                              '${day.day}',
                                              style: const TextStyle(
                                                color: Color(0xFF166534),
                                                fontSize: 16,
                                                fontWeight: FontWeight.w700,
                                              ),
                                            ),
                                          )
                                        : const SizedBox.shrink(
                                            key: ValueKey('event-dot'),
                                          ),
                                  ),
                                ),
                              ),
                            );
                          },
                        ),
                        calendarStyle: const CalendarStyle(
                          markerSize: 6,
                          markersAlignment: Alignment.center,
                          markerMargin: EdgeInsets.zero,
                          todayDecoration: BoxDecoration(
                            color: AppColors.successLight,
                            shape: BoxShape.circle,
                          ),
                          todayTextStyle: TextStyle(
                            color: AppColors.success,
                            fontWeight: FontWeight.bold,
                          ),
                          selectedDecoration: BoxDecoration(
                            color: AppColors.successLight,
                            shape: BoxShape.circle,
                          ),
                          selectedTextStyle: TextStyle(
                            color: Color(0xFF166534),
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        headerStyle: HeaderStyle(
                          formatButtonVisible: true,
                          formatButtonDecoration: BoxDecoration(
                            border: Border.all(color: AppColors.border),
                            borderRadius: BorderRadius.circular(12),
                          ),
                          formatButtonPadding: const EdgeInsets.symmetric(
                            horizontal: 10,
                            vertical: 4,
                          ),
                          formatButtonTextStyle: AppTextStyles.caption.copyWith(
                            color: AppColors.success,
                            fontWeight: FontWeight.bold,
                          ),
                          titleCentered: true,
                          titleTextStyle: AppTextStyles.subtitle,
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),

                    // Title
                    Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16.0,
                        vertical: 8.0,
                      ),
                      child: Row(
                        children: [
                          const Icon(
                            Icons.list_alt_rounded,
                            size: 18,
                            color: AppColors.success,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            'Lịch học ngày ${DateFormat('dd/MM/yyyy').format(_selectedDay ?? _focusedDay)}',
                            style: AppTextStyles.bodyMedium.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),

                    // List
                    Expanded(
                      child: dailyEvents.isEmpty
                          ? const AppEmptyState(
                              title: 'Không có lớp học',
                              description:
                                  'Con em được nghỉ hoặc chưa có lịch học ngày này.',
                              icon: Icons.coffee_rounded,
                            )
                          : ListView.separated(
                              padding: const EdgeInsets.all(16),
                              itemCount: dailyEvents.length,
                              separatorBuilder: (context, index) =>
                                  const SizedBox(height: 12),
                              itemBuilder: (context, index) {
                                final event = dailyEvents[index];
                                return Card(
                                  margin: EdgeInsets.zero,
                                  child: Padding(
                                    padding: const EdgeInsets.all(16.0),
                                    child: Row(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Container(
                                          padding: const EdgeInsets.all(10),
                                          decoration: BoxDecoration(
                                            color: AppColors.successLight,
                                            borderRadius: BorderRadius.circular(
                                              12,
                                            ),
                                          ),
                                          child: Column(
                                            children: [
                                              const Icon(
                                                Icons.access_time_rounded,
                                                size: 18,
                                                color: AppColors.success,
                                              ),
                                              const SizedBox(height: 4),
                                              Text(
                                                event.startTime,
                                                style: AppTextStyles.caption
                                                    .copyWith(
                                                      color: AppColors.success,
                                                      fontWeight:
                                                          FontWeight.bold,
                                                    ),
                                              ),
                                            ],
                                          ),
                                        ),
                                        const SizedBox(width: 16),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                event.courseName,
                                                style: AppTextStyles.bodyMedium
                                                    .copyWith(
                                                      fontWeight:
                                                          FontWeight.bold,
                                                      fontSize: 15,
                                                    ),
                                              ),
                                              const SizedBox(height: 6),
                                              Row(
                                                children: [
                                                  const Icon(
                                                    Icons
                                                        .person_outline_rounded,
                                                    size: 14,
                                                    color:
                                                        AppColors.textSecondary,
                                                  ),
                                                  const SizedBox(width: 4),
                                                  Text(
                                                    event.teacherName,
                                                    style:
                                                        AppTextStyles.caption,
                                                  ),
                                                ],
                                              ),
                                              const SizedBox(height: 4),
                                              Row(
                                                children: [
                                                  const Icon(
                                                    Icons.room_rounded,
                                                    size: 14,
                                                    color:
                                                        AppColors.textSecondary,
                                                  ),
                                                  const SizedBox(width: 4),
                                                  Text(
                                                    event.room,
                                                    style: AppTextStyles.caption
                                                        .copyWith(
                                                          fontWeight:
                                                              FontWeight.bold,
                                                        ),
                                                  ),
                                                ],
                                              ),
                                            ],
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                );
                              },
                            ),
                    ),
                  ],
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}
