import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';

class StudentShell extends StatelessWidget {
  final StatefulNavigationShell navigationShell;

  const StudentShell({super.key, required this.navigationShell});

  void _onTap(BuildContext context, int index) {
    navigationShell.goBranch(
      index,
      initialLocation: index == navigationShell.currentIndex,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: navigationShell,
      bottomNavigationBar: NavigationBar(
        selectedIndex: navigationShell.currentIndex,
        onDestinationSelected: (index) => _onTap(context, index),
        backgroundColor: AppColors.surface,
        indicatorColor: AppColors.primaryLight,
        elevation: 10,
        height: 65,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.home_outlined),
            selectedIcon: Icon(Icons.home_rounded, color: AppColors.primary),
            label: 'Trang chủ',
          ),
          NavigationDestination(
            icon: Icon(Icons.book_outlined),
            selectedIcon: Icon(Icons.book_rounded, color: AppColors.primary),
            label: 'Học tập',
          ),
          NavigationDestination(
            icon: Icon(Icons.calendar_today_outlined),
            selectedIcon: Icon(
              Icons.calendar_today_rounded,
              color: AppColors.primary,
            ),
            label: 'Lịch học',
          ),
          NavigationDestination(
            icon: Icon(Icons.analytics_outlined),
            selectedIcon: Icon(
              Icons.analytics_rounded,
              color: AppColors.primary,
            ),
            label: 'Kết quả',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline_rounded),
            selectedIcon: Icon(Icons.person_rounded, color: AppColors.primary),
            label: 'Cá nhân',
          ),
        ],
      ),
    );
  }
}
