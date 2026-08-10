import 'package:go_router/go_router.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../features/auth/presentation/login_screen.dart';
import '../../features/auth/data/auth_provider.dart';

// Student presentation imports
import '../../features/student/presentation/student_shell.dart';
import '../../features/student/presentation/dashboard/student_dashboard_screen.dart';
import '../../features/student/presentation/courses/student_courses_screen.dart';
import '../../features/student/presentation/courses/student_course_detail_screen.dart';
import '../../features/student/presentation/schedule/student_schedule_screen.dart';
import '../../features/student/presentation/grades/student_grades_screen.dart';
import '../../features/student/presentation/exams/student_exam_schedule_screen.dart';
import '../../features/student/presentation/attendance/student_attendance_screen.dart';
import '../../features/student/presentation/tuition/student_tuition_screen.dart';
import '../../features/student/presentation/assignments/student_assignments_screen.dart';
import '../../features/student/presentation/notifications/student_notifications_screen.dart';
import '../../features/student/presentation/profile/student_profile_screen.dart';

// Parent presentation imports
import '../../features/parent/presentation/parent_shell.dart';
import '../../features/parent/presentation/dashboard/parent_dashboard_screen.dart';
import '../../features/parent/presentation/children/parent_children_screen.dart';
import '../../features/parent/presentation/grades/parent_grades_screen.dart';
import '../../features/parent/presentation/attendance/parent_attendance_screen.dart';
import '../../features/parent/presentation/schedule/parent_schedule_screen.dart';
import '../../features/parent/presentation/exams/parent_exam_schedule_screen.dart';
import '../../features/parent/presentation/finance/parent_tuition_screen.dart';
import '../../features/parent/presentation/notifications/parent_notifications_screen.dart';
import '../../features/parent/presentation/profile/parent_profile_screen.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  late final GoRouter router;
  router = GoRouter(
    initialLocation: '/login',
    redirect: (context, state) {
      final auth = ref.read(authProvider);
      final path = state.matchedLocation;
      final isLogin = path == '/login';

      if (!auth.isAuthenticated) {
        return isLogin ? null : '/login';
      }

      final home = auth.role == UserRole.parent
          ? '/parent/dashboard'
          : '/student/dashboard';
      if (isLogin) return home;
      if (path.startsWith('/student') && auth.role != UserRole.student) {
        return home;
      }
      if (path.startsWith('/parent') && auth.role != UserRole.parent) {
        return home;
      }
      return null;
    },
    routes: [
      GoRoute(path: '/', redirect: (context, state) => '/login'),
      GoRoute(path: '/login', builder: (context, state) => const LoginScreen()),
      // Student Stateful Shell
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) {
          return StudentShell(navigationShell: navigationShell);
        },
        branches: [
          // Branch 0: Dashboard/Home
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/student/dashboard',
                builder: (context, state) => const StudentDashboardScreen(),
              ),
            ],
          ),
          // Branch 1: Enrolled Courses & Lesson details
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/student/courses',
                builder: (context, state) => const StudentCoursesScreen(),
                routes: [
                  GoRoute(
                    path: ':courseId',
                    builder: (context, state) => StudentCourseDetailScreen(
                      courseId: state.pathParameters['courseId'] ?? '',
                    ),
                  ),
                ],
              ),
              GoRoute(
                path: '/student/assignments',
                builder: (context, state) => const StudentAssignmentsScreen(),
              ),
            ],
          ),
          // Branch 2: Class calendar schedule
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/student/schedule',
                builder: (context, state) => const StudentScheduleScreen(),
              ),
            ],
          ),
          // Branch 3: Score transcripts, exams, and attendance
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/student/grades',
                builder: (context, state) => const StudentGradesScreen(),
              ),
              GoRoute(
                path: '/student/exams',
                builder: (context, state) => const StudentExamScheduleScreen(),
              ),
              GoRoute(
                path: '/student/attendance',
                builder: (context, state) => const StudentAttendanceScreen(),
              ),
            ],
          ),
          // Branch 4: Profile, tuition debt, notifications
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/student/profile',
                builder: (context, state) => const StudentProfileScreen(),
              ),
              GoRoute(
                path: '/student/tuition',
                builder: (context, state) => const StudentTuitionScreen(),
              ),
              GoRoute(
                path: '/student/notifications',
                builder: (context, state) => const StudentNotificationsScreen(),
              ),
            ],
          ),
        ],
      ),

      // Parent Stateful Shell
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) {
          return ParentShell(navigationShell: navigationShell);
        },
        branches: [
          // Branch 0: Dashboard/Home
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/parent/dashboard',
                builder: (context, state) => const ParentDashboardScreen(),
              ),
            ],
          ),
          // Branch 1: Connected children profile list
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/parent/children',
                builder: (context, state) => const ParentChildrenScreen(),
              ),
            ],
          ),
          // Branch 2: Child learning grades details, calendar, exams, attendance
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/parent/grades',
                builder: (context, state) => const ParentGradesScreen(),
              ),
              GoRoute(
                path: '/parent/schedule',
                builder: (context, state) => const ParentScheduleScreen(),
              ),
              GoRoute(
                path: '/parent/exams',
                builder: (context, state) => const ParentExamScheduleScreen(),
              ),
              GoRoute(
                path: '/parent/attendance',
                builder: (context, state) => const ParentAttendanceScreen(),
              ),
            ],
          ),
          // Branch 3: Child financial tuition bills
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/parent/tuition',
                builder: (context, state) => const ParentTuitionScreen(),
              ),
            ],
          ),
          // Branch 4: Profile & custom alerts
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/parent/profile',
                builder: (context, state) => const ParentProfileScreen(),
              ),
              GoRoute(
                path: '/parent/notifications',
                builder: (context, state) => const ParentNotificationsScreen(),
              ),
            ],
          ),
        ],
      ),
    ],
  );

  ref.listen<AuthState>(authProvider, (previous, next) => router.refresh());
  ref.onDispose(router.dispose);
  return router;
});
