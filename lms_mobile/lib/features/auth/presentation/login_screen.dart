import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:lms_mobile/core/network/api_client.dart';
import 'package:lms_mobile/core/theme/app_colors.dart';
import 'package:lms_mobile/core/theme/app_text_styles.dart';
import 'package:lms_mobile/core/widgets/app_buttons.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _handleLogin() async {
    if (_formKey.currentState!.validate()) {
      final success = await ref
          .read(authProvider.notifier)
          .login(_usernameController.text, _passwordController.text);
      if (success && mounted) {
        final role = ref.read(authProvider).role;
        context.go(
          role == UserRole.parent ? '/parent/dashboard' : '/student/dashboard',
        );
      }
    }
  }

  Future<void> _handleDemoLogin(DemoAccount account) async {
    _fillDemoAccount(account);
    await _handleLogin();
  }

  void _fillDemoAccount(DemoAccount account) {
    _usernameController.text = account.username;
    _passwordController.text = account.password;
    setState(() => _obscurePassword = false);
  }

  Future<void> _copyDemoValue(String label, String value) async {
    await Clipboard.setData(ClipboardData(text: value));
    if (!mounted) return;
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text('Đã sao chép $label')));
  }

  Widget _buildDemoAccountCard({
    required DemoAccount account,
    required bool isLoading,
  }) {
    final isParent = account.role.toLowerCase() == 'parent';
    final title = account.title.isEmpty
        ? (isParent ? 'Demo Phụ huynh' : 'Demo Sinh viên')
        : account.title;
    final icon = isParent
        ? Icons.family_restroom_outlined
        : Icons.school_outlined;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.primaryLight,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.primary.withValues(alpha: 0.2)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: AppColors.primary),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  title,
                  style: AppTextStyles.bodyRegular.copyWith(
                    fontWeight: FontWeight.w700,
                    color: AppColors.primary,
                  ),
                ),
              ),
            ],
          ),
          if (account.description.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(account.description, style: AppTextStyles.caption),
          ],
          const SizedBox(height: 10),
          Text('Tài khoản', style: AppTextStyles.caption),
          Row(
            children: [
              Expanded(child: SelectableText(account.username)),
              IconButton(
                tooltip: 'Sao chép tài khoản',
                visualDensity: VisualDensity.compact,
                onPressed: () => _copyDemoValue('tài khoản', account.username),
                icon: const Icon(Icons.copy_rounded, size: 19),
              ),
            ],
          ),
          Text('Mật khẩu', style: AppTextStyles.caption),
          Row(
            children: [
              Expanded(child: SelectableText(account.password)),
              IconButton(
                tooltip: 'Sao chép mật khẩu',
                visualDensity: VisualDensity.compact,
                onPressed: () => _copyDemoValue('mật khẩu', account.password),
                icon: const Icon(Icons.copy_rounded, size: 19),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: isLoading ? null : () => _fillDemoAccount(account),
                  child: const Text('Điền vào form'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: FilledButton(
                  onPressed: isLoading ? null : () => _handleDemoLogin(account),
                  child: const Text('Đăng nhập ngay'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const SizedBox(height: 40),
                // Logo & Header
                Center(
                  child: Column(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppColors.primaryLight,
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.school_rounded,
                          size: 48,
                          color: AppColors.primary,
                        ),
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'LMS Portal',
                        style: AppTextStyles.display.copyWith(
                          fontSize: 26,
                          color: AppColors.primary,
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Hệ thống quản lý học tập thông minh',
                        style: AppTextStyles.bodyRegular,
                        textAlign: TextAlign.center,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 48),
                Text(
                  'Đăng nhập',
                  style: AppTextStyles.title.copyWith(fontSize: 22),
                ),
                const SizedBox(height: 8),
                Text(
                  'Vui lòng đăng nhập để tiếp tục sử dụng',
                  style: AppTextStyles.bodyRegular,
                ),
                const SizedBox(height: 6),
                Text('Máy chủ: $apiBaseUrl', style: AppTextStyles.caption),
                const SizedBox(height: 24),
                // Input Fields
                TextFormField(
                  controller: _usernameController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(
                    labelText: 'Tên đăng nhập / Email',
                    hintText: 'Nhập email của bạn',
                    prefixIcon: Icon(Icons.email_outlined, size: 20),
                  ),
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) {
                      return 'Vui lòng nhập tên đăng nhập';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _passwordController,
                  obscureText: _obscurePassword,
                  decoration: InputDecoration(
                    labelText: 'Mật khẩu',
                    hintText: 'Nhập mật khẩu của bạn',
                    prefixIcon: const Icon(
                      Icons.lock_outline_rounded,
                      size: 20,
                    ),
                    suffixIcon: IconButton(
                      icon: Icon(
                        _obscurePassword
                            ? Icons.visibility_off_outlined
                            : Icons.visibility_outlined,
                        size: 20,
                      ),
                      onPressed: () {
                        setState(() {
                          _obscurePassword = !_obscurePassword;
                        });
                      },
                    ),
                  ),
                  validator: (value) {
                    if (value == null || value.isEmpty) {
                      return 'Vui lòng nhập mật khẩu';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 24),
                // Submit Button
                AppPrimaryButton(
                  text: 'Đăng nhập',
                  onPressed: _handleLogin,
                  isLoading: authState.isLoading,
                ),
                if (demoAccounts.isNotEmpty) ...[
                  const SizedBox(height: 20),
                  Row(
                    children: [
                      const Expanded(child: Divider()),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 12),
                        child: Text(
                          'Đăng nhập nhanh để demo',
                          style: AppTextStyles.caption,
                        ),
                      ),
                      const Expanded(child: Divider()),
                    ],
                  ),
                  const SizedBox(height: 12),
                  ...demoAccounts.indexed.map((entry) {
                    final (index, account) = entry;
                    return Padding(
                      padding: EdgeInsets.only(top: index == 0 ? 0 : 12),
                      child: _buildDemoAccountCard(
                        account: account,
                        isLoading: authState.isLoading,
                      ),
                    );
                  }),
                ],
                if (authState.error != null) ...[
                  const SizedBox(height: 16),
                  Text(
                    authState.error!,
                    style: AppTextStyles.bodyRegular.copyWith(
                      color: AppColors.error,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
