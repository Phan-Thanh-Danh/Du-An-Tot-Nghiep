import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_info_row.dart';
import 'package:lms_mobile/core/widgets/app_buttons.dart';
import 'package:lms_mobile/core/widgets/app_states.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';
import 'package:lms_mobile/features/student/models/student_models.dart';

class StudentProfileScreen extends ConsumerStatefulWidget {
  const StudentProfileScreen({super.key});

  @override
  ConsumerState<StudentProfileScreen> createState() =>
      _StudentProfileScreenState();
}

class _StudentProfileScreenState extends ConsumerState<StudentProfileScreen> {
  StudentProfile? _profile;
  bool _isLoading = true;
  bool _isEditing = false;
  String? _error;

  final _phoneController = TextEditingController();
  final _emailController = TextEditingController();

  final _currentPwdController = TextEditingController();
  final _newPwdController = TextEditingController();
  final _confirmPwdController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  @override
  void dispose() {
    _phoneController.dispose();
    _emailController.dispose();
    _currentPwdController.dispose();
    _newPwdController.dispose();
    _confirmPwdController.dispose();
    super.dispose();
  }

  Future<void> _loadProfile() async {
    try {
      final p = await ref.read(activeStudentRepoProvider).getProfile();
      if (!mounted) return;
      setState(() {
        _profile = p;
        _phoneController.text = p.phone;
        _emailController.text = p.email;
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

  void _saveProfile() async {
    setState(() {
      _isLoading = true;
    });
    final repo = ref.read(activeStudentRepoProvider);
    try {
      await repo.updateProfile(_phoneController.text, _emailController.text);
      final updated = await repo.getProfile();
      if (!mounted) return;
      setState(() {
        _profile = updated;
        _isEditing = false;
        _isLoading = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cập nhật thông tin thành công.'),
          backgroundColor: AppColors.success,
        ),
      );
    } catch (error) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(error.toString()),
          backgroundColor: AppColors.error,
        ),
      );
    }
  }

  void _showChangePasswordDialog() {
    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(20),
          ),
          title: const Text('Đổi mật khẩu'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: _currentPwdController,
                  obscureText: true,
                  decoration: const InputDecoration(
                    labelText: 'Mật khẩu hiện tại',
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _newPwdController,
                  obscureText: true,
                  decoration: const InputDecoration(labelText: 'Mật khẩu mới'),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _confirmPwdController,
                  obscureText: true,
                  decoration: const InputDecoration(
                    labelText: 'Xác nhận mật khẩu mới',
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Hủy'),
            ),
            ElevatedButton(
              onPressed: () {
                if (_newPwdController.text != _confirmPwdController.text) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Mật khẩu mới không khớp!'),
                      backgroundColor: AppColors.error,
                    ),
                  );
                  return;
                }
                Navigator.pop(context);
                _changePassword();
              },
              child: const Text('Thay đổi'),
            ),
          ],
        );
      },
    );
  }

  void _changePassword() async {
    final repo = ref.read(activeStudentRepoProvider);
    try {
      await repo.changePassword(
        _currentPwdController.text,
        _newPwdController.text,
      );
      _currentPwdController.clear();
      _newPwdController.clear();
      _confirmPwdController.clear();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Đổi mật khẩu thành công.'),
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
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Thông tin cá nhân')),
        body: const AppLoadingSkeleton(itemCount: 3),
      );
    }

    if (_error != null || _profile == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Thông tin cá nhân')),
        body: AppErrorState(
          message: _error ?? 'Không tải được hồ sơ.',
          onRetry: _loadProfile,
        ),
      );
    }

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Hồ sơ học sinh'),
        centerTitle: true,
        actions: [
          IconButton(
            icon: Icon(
              _isEditing ? Icons.save_rounded : Icons.edit_rounded,
              color: AppColors.primary,
            ),
            onPressed: () {
              if (_isEditing) {
                _saveProfile();
              } else {
                setState(() {
                  _isEditing = true;
                });
              }
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            // Profile Header Card
            Card(
              margin: EdgeInsets.zero,
              child: Padding(
                padding: const EdgeInsets.all(20.0),
                child: Column(
                  children: [
                    CircleAvatar(
                      radius: 50,
                      backgroundColor: AppColors.primaryLight,
                      backgroundImage: _profile!.avatarUrl.isEmpty
                          ? null
                          : NetworkImage(_profile!.avatarUrl),
                      child: _profile!.avatarUrl.isEmpty
                          ? const Icon(Icons.person, color: AppColors.primary)
                          : null,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      _profile!.fullName,
                      style: AppTextStyles.subtitle.copyWith(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'SBD / Mã SV: ${_profile!.code}',
                      style: AppTextStyles.caption.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppColors.primary,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'Phân hiệu: ${_profile!.maDonVi == 'CAMPUS_HN' ? 'Hà Nội' : 'Hồ Chí Minh'}',
                      style: AppTextStyles.caption,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Academic Info Card
            Card(
              margin: EdgeInsets.zero,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16.0,
                  vertical: 8.0,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: const EdgeInsets.only(top: 8.0, bottom: 4.0),
                      child: Text(
                        'Thông tin học vụ',
                        style: AppTextStyles.bodyMedium.copyWith(
                          fontWeight: FontWeight.bold,
                          color: AppColors.primary,
                        ),
                      ),
                    ),
                    const Divider(color: AppColors.border),
                    AppInfoRow(
                      label: 'Khoa / Viện',
                      value: _profile!.department,
                      icon: Icons.corporate_fare_rounded,
                    ),
                    AppInfoRow(
                      label: 'Ngành học',
                      value: _profile!.major,
                      icon: Icons.workspace_premium_rounded,
                    ),
                    AppInfoRow(
                      label: 'Lớp sinh hoạt',
                      value: _profile!.classCode,
                      icon: Icons.people_rounded,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Personal Contact Card
            Card(
              margin: EdgeInsets.zero,
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Thông tin liên hệ',
                      style: AppTextStyles.bodyMedium.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppColors.primary,
                      ),
                    ),
                    const Divider(color: AppColors.border),
                    const SizedBox(height: 12),
                    if (_isEditing) ...[
                      TextField(
                        controller: _phoneController,
                        keyboardType: TextInputType.phone,
                        decoration: const InputDecoration(
                          labelText: 'Số điện thoại',
                          prefixIcon: Icon(Icons.phone_rounded),
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextField(
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        decoration: const InputDecoration(
                          labelText: 'Email liên hệ',
                          prefixIcon: Icon(Icons.email_rounded),
                        ),
                      ),
                    ] else ...[
                      AppInfoRow(
                        label: 'Số điện thoại',
                        value: _profile!.phone,
                        icon: Icons.phone_rounded,
                      ),
                      AppInfoRow(
                        label: 'Email liên hệ',
                        value: _profile!.email,
                        icon: Icons.email_rounded,
                      ),
                    ],
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Settings & Actions
            AppSecondaryButton(
              text: 'Đổi mật khẩu',
              onPressed: _showChangePasswordDialog,
              icon: Icons.lock_reset_rounded,
            ),
            const SizedBox(height: 12),
            AppSecondaryButton(
              text: 'Đăng xuất tài khoản',
              onPressed: () {
                ref.read(authProvider.notifier).logout();
                context.go('/login');
              },
              icon: Icons.logout_rounded,
            ),
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }
}
