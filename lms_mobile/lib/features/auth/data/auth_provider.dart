import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import '../../../core/network/api_client.dart';
import '../../student/data/student_repository.dart';
import '../../student/data/student_remote_repository.dart';
import '../../parent/data/parent_repository.dart';
import '../../parent/data/parent_remote_repository.dart';

enum UserRole { guest, student, parent }

class DemoAccount {
  final String username;
  final String password;
  final String title;
  final String description;
  final String role;

  const DemoAccount({
    required this.username,
    required this.password,
    this.title = '',
    this.description = '',
    this.role = 'student',
  });

  bool get isConfigured => username.isNotEmpty && password.isNotEmpty;
}

const _environmentStudentAccount = DemoAccount(
  username: String.fromEnvironment('DEMO_STUDENT_USERNAME'),
  password: String.fromEnvironment('DEMO_STUDENT_PASSWORD'),
  title: 'Demo Sinh viên',
  description: 'Sinh viên Phát triển phần mềm',
);

const _environmentParentAccount = DemoAccount(
  username: String.fromEnvironment('DEMO_PARENT_USERNAME'),
  password: String.fromEnvironment('DEMO_PARENT_PASSWORD'),
  title: 'Demo Phụ huynh',
  description: 'Theo dõi sinh viên thuộc nhiều chuyên ngành',
  role: 'parent',
);

DemoAccount demoStudentAccount = _environmentStudentAccount;
DemoAccount demoParentAccount = _environmentParentAccount;
List<DemoAccount> demoAccounts = const [];

Future<void> loadDemoAccounts() async {
  final loadedAccounts = <DemoAccount>[];
  try {
    final raw = await rootBundle.loadString(
      'assets/demo/demo_credentials.local.json',
    );
    final json = jsonDecode(raw) as Map<String, dynamic>;

    if (!demoStudentAccount.isConfigured) {
      demoStudentAccount = DemoAccount(
        username: json['DEMO_STUDENT_USERNAME']?.toString() ?? '',
        password: json['DEMO_STUDENT_PASSWORD']?.toString() ?? '',
        title: 'Demo Sinh viên',
        description: 'Sinh viên Phát triển phần mềm',
      );
    }
    if (!demoParentAccount.isConfigured) {
      demoParentAccount = DemoAccount(
        username: json['DEMO_PARENT_USERNAME']?.toString() ?? '',
        password: json['DEMO_PARENT_PASSWORD']?.toString() ?? '',
        title: 'Demo Phụ huynh',
        description: 'Theo dõi sinh viên thuộc nhiều chuyên ngành',
        role: 'parent',
      );
    }

    final accountList = json['DEMO_ACCOUNTS'];
    if (accountList is List) {
      for (final item in accountList) {
        if (item is! Map) continue;
        final account = DemoAccount(
          username: item['username']?.toString() ?? '',
          password: item['password']?.toString() ?? '',
          title: item['title']?.toString() ?? '',
          description: item['description']?.toString() ?? '',
          role: item['role']?.toString() ?? 'student',
        );
        if (account.isConfigured) loadedAccounts.add(account);
      }
    }
  } on FlutterError {
    // File local không bắt buộc trên máy không dùng chế độ demo.
  } on FormatException {
    // Bỏ qua file local sai định dạng để ứng dụng vẫn khởi động bình thường.
  }

  final defaults = [
    demoStudentAccount,
    demoParentAccount,
  ].where((account) => account.isConfigured);
  final uniqueAccounts = <String, DemoAccount>{};
  for (final account in [...defaults, ...loadedAccounts]) {
    uniqueAccounts[account.username.toLowerCase()] = account;
  }
  demoAccounts = uniqueAccounts.values.toList(growable: false);
}

class AuthState {
  final UserRole role;
  final bool isAuthenticated;
  final bool isLoading;
  final String? error;

  const AuthState({
    this.role = UserRole.guest,
    this.isAuthenticated = false,
    this.isLoading = false,
    this.error,
  });

  AuthState copyWith({
    UserRole? role,
    bool? isAuthenticated,
    bool? isLoading,
    String? error,
  }) {
    return AuthState(
      role: role ?? this.role,
      isAuthenticated: isAuthenticated ?? this.isAuthenticated,
      isLoading: isLoading ?? this.isLoading,
      error: error,
    );
  }
}

class AuthNotifier extends StateNotifier<AuthState> {
  final Ref ref;

  AuthNotifier(this.ref) : super(const AuthState());

  Future<bool> login(String username, String password) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final response = await ref
          .read(apiClientProvider)
          .post<Map<String, dynamic>>(
            '/auth/login',
            data: {'usernameOrEmail': username.trim(), 'password': password},
            options: Options(
              sendTimeout: const Duration(seconds: 5),
              receiveTimeout: const Duration(seconds: 10),
            ),
          );
      final data = response.data ?? const <String, dynamic>{};
      final user = data['user'] as Map<String, dynamic>? ?? const {};
      final roleValue = user['role']?.toString().toLowerCase();
      final role = switch (roleValue) {
        'student' || 'hoc_sinh' => UserRole.student,
        'parent' || 'phu_huynh' => UserRole.parent,
        _ => UserRole.guest,
      };
      if (role == UserRole.guest) {
        throw const ApiClientException(
          'Ứng dụng mobile hiện chỉ hỗ trợ tài khoản Sinh viên và Phụ huynh.',
        );
      }
      await ref
          .read(secureStorageProvider)
          .write(key: 'auth_token', value: data['accessToken']?.toString());
      await ref
          .read(secureStorageProvider)
          .write(key: 'refresh_token', value: data['refreshToken']?.toString());
      state = AuthState(role: role, isAuthenticated: true, isLoading: false);
      return true;
    } catch (error) {
      state = AuthState(
        role: UserRole.guest,
        isAuthenticated: false,
        isLoading: false,
        error: ApiClient.readableError(error).message,
      );
      return false;
    }
  }

  Future<void> logout() async {
    state = state.copyWith(isLoading: true);
    await ref.read(secureStorageProvider).delete(key: 'auth_token');
    await ref.read(secureStorageProvider).delete(key: 'refresh_token');
    state = const AuthState();
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref);
});

// Provide overridden repositories based on auth status
final activeStudentRepoProvider = Provider<StudentRepository>((ref) {
  return StudentRemoteRepository(ref.watch(apiClientProvider));
});

final activeParentRepoProvider = Provider<ParentRepository>((ref) {
  return ParentRemoteRepository(ref.watch(apiClientProvider));
});
