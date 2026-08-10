import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/parent_models.dart';
import 'package:lms_mobile/features/auth/data/auth_provider.dart';

// Keeps track of the selected child ID
final activeChildIdProvider = StateProvider<String?>((ref) => null);

// Fetches the active child profile
final activeChildProvider = FutureProvider<Child?>((ref) async {
  final repo = ref.watch(activeParentRepoProvider);
  final children = await repo.getChildren();
  if (children.isEmpty) return null;

  final selectedId = ref.watch(activeChildIdProvider);
  if (selectedId == null) {
    // Default to the first child
    return children.first;
  }

  return children.firstWhere(
    (c) => c.id == selectedId,
    orElse: () => children.first,
  );
});
