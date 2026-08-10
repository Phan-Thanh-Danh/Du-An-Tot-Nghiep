import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';

class ParentShell extends StatelessWidget {
  final StatefulNavigationShell navigationShell;

  const ParentShell({super.key, required this.navigationShell});

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
        indicatorColor: AppColors.successLight,
        elevation: 10,
        height: 65,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.dashboard_outlined),
            selectedIcon: Icon(
              Icons.dashboard_rounded,
              color: AppColors.success,
            ),
            label: 'Trang chủ',
          ),
          NavigationDestination(
            icon: Icon(Icons.people_outline_rounded),
            selectedIcon: Icon(Icons.people_rounded, color: AppColors.success),
            label: 'Con em',
          ),
          NavigationDestination(
            icon: Icon(Icons.school_outlined),
            selectedIcon: Icon(Icons.school_rounded, color: AppColors.success),
            label: 'Học tập',
          ),
          NavigationDestination(
            icon: Icon(Icons.account_balance_wallet_outlined),
            selectedIcon: Icon(
              Icons.account_balance_wallet_rounded,
              color: AppColors.success,
            ),
            label: 'Tài chính',
          ),
          NavigationDestination(
            icon: Icon(Icons.admin_panel_settings_outlined),
            selectedIcon: Icon(
              Icons.admin_panel_settings_rounded,
              color: AppColors.success,
            ),
            label: 'Cá nhân',
          ),
        ],
      ),
    );
  }
}
