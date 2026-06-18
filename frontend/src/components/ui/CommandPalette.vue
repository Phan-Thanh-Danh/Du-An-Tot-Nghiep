<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'
import * as LucideIcons from 'lucide-vue-next'

const emit = defineEmits(['close'])
const router = useRouter()
const authStore = useAuthStore()
const recentStore = useRecentFavoritesStore()

const query = ref('')
const selectedIndex = ref(0)
const inputRef = ref(null)
const resultsRef = ref(null)

const allCommands = computed(() => {
  const base = [
    { id: 'home', label: 'Về trang chủ', icon: 'House', route: '/', category: 'Điều hướng' },
    { id: 'profile', label: 'Hồ sơ cá nhân', icon: 'UserCircle', route: getProfileRoute(), category: 'Điều hướng' },
    { id: 'notifications', label: 'Trung tâm thông báo', icon: 'Bell', route: getNotifRoute(), category: 'Điều hướng' },
  ]
  const roleRoutes = getRoleRoutes()
  return [...base, ...roleRoutes]
})

const filteredCommands = computed(() => {
  const q = query.value.toLowerCase().trim()
  if (!q) return allCommands.value.slice(0, 12)
  return allCommands.value.filter(
    c => c.label.toLowerCase().includes(q) || c.category.toLowerCase().includes(q)
  ).slice(0, 12)
})

const selectedCommand = computed(() => filteredCommands.value[selectedIndex.value])

function getProfileRoute() {
  if (authStore.hasRole('Principal')) return '/bgh/profile'
  if (authStore.hasRole('Teacher')) return '/teacher/profile'
  if (authStore.hasRole('AcademicStaff')) return '/staff/profile'
  return '/student/profile'
}

function getNotifRoute() {
  if (authStore.hasRole('Principal')) return '/bgh/notifications'
  if (authStore.hasRole('Teacher')) return '/teacher/notifications'
  if (authStore.hasRole('AcademicStaff')) return '/staff/notifications'
  return '/student/notifications'
}

function getRoleRoutes() {
  if (authStore.hasRole('Principal')) return [
    { id: 'bgh-dashboard', label: 'Dashboard chiến lược', icon: 'LayoutDashboard', route: '/bgh/dashboard', category: 'BGH' },
    { id: 'bgh-schedule', label: 'Duyệt thời khóa biểu', icon: 'CalendarCheck', route: '/bgh/schedule/pending', category: 'BGH' },
    { id: 'bgh-evaluations', label: 'Đánh giá giảng viên', icon: 'Star', route: '/bgh/evaluations', category: 'BGH' },
    { id: 'bgh-programs', label: 'Ngành & Chuyên ngành', icon: 'BookOpen', route: '/bgh/academic-programs', category: 'BGH' },
    { id: 'bgh-curriculum', label: 'Khung chương trình', icon: 'Library', route: '/bgh/curriculum', category: 'BGH' },
    { id: 'bgh-terms', label: 'Học kỳ & Khóa', icon: 'CalendarDays', route: '/bgh/academic-terms', category: 'BGH' },
    { id: 'bgh-organizations', label: 'Quản lý Đơn vị', icon: 'Building2', route: '/bgh/organizations', category: 'BGH' },
    { id: 'bgh-users', label: 'Quản lý Người dùng', icon: 'Users', route: '/bgh/users', category: 'BGH' },
    { id: 'bgh-roles', label: 'Vai trò & Phân quyền', icon: 'ShieldCheck', route: '/bgh/roles', category: 'BGH' },
    { id: 'bgh-facilities', label: 'Cơ sở vật chất', icon: 'MapPin', route: '/bgh/facilities', category: 'BGH' },
    { id: 'bgh-audit', label: 'Nhật ký kiểm toán', icon: 'History', route: '/bgh/audit-logs', category: 'BGH' },
    { id: 'bgh-profile', label: 'Hồ sơ cá nhân', icon: 'UserCircle', route: '/bgh/profile', category: 'BGH' },
  ]
  if (authStore.hasRole('Teacher')) return [
    { id: 'teacher-dashboard', label: 'Tổng quan giảng dạy', icon: 'LayoutDashboard', route: '/teacher/dashboard', category: 'Giảng viên' },
    { id: 'teacher-classes', label: 'Lớp học của tôi', icon: 'Users', route: '/teacher/classes', category: 'Giảng viên' },
    { id: 'teacher-assignments', label: 'Quản lý bài tập', icon: 'FileText', route: '/teacher/assignments', category: 'Giảng viên' },
    { id: 'teacher-grading', label: 'Chấm bài', icon: 'CheckSquare', route: '/teacher/grading', category: 'Giảng viên' },
    { id: 'teacher-attendance', label: 'Điểm danh', icon: 'ClipboardCheck', route: '/teacher/attendance', category: 'Giảng viên' },
    { id: 'teacher-schedule', label: 'Thời khóa biểu', icon: 'Calendar', route: '/teacher/schedule', category: 'Giảng viên' },
  ]
  if (authStore.hasRole('AcademicStaff')) return [
    { id: 'staff-dashboard', label: 'Tổng quan giáo vụ', icon: 'LayoutDashboard', route: '/staff/dashboard', category: 'Giáo vụ' },
    { id: 'staff-schedule', label: 'Quản lý TKB', icon: 'Calendar', route: '/staff/schedule', category: 'Giáo vụ' },
    { id: 'staff-registrations', label: 'Đợt đăng ký', icon: 'ClipboardList', route: '/staff/registrations', category: 'Giáo vụ' },
    { id: 'staff-requests', label: 'Xử lý đơn từ', icon: 'Inbox', route: '/staff/requests', category: 'Giáo vụ' },
    { id: 'staff-rooms', label: 'Quản lý phòng', icon: 'DoorOpen', route: '/staff/rooms', category: 'Giáo vụ' },
  ]
  return [
    { id: 'student-dashboard', label: 'Dashboard', icon: 'LayoutDashboard', route: '/student/dashboard', category: 'Sinh viên' },
    { id: 'student-courses', label: 'Khóa học', icon: 'BookOpen', route: '/student/courses', category: 'Sinh viên' },
    { id: 'student-assignments', label: 'Bài tập', icon: 'FileText', route: '/student/assignments', category: 'Sinh viên' },
    { id: 'student-exams', label: 'Thi / Kiểm tra', icon: 'ClipboardCheck', route: '/student/exams', category: 'Sinh viên' },
    { id: 'student-grades', label: 'Bảng điểm', icon: 'BarChart3', route: '/student/grades', category: 'Sinh viên' },
    { id: 'student-schedule', label: 'Thời khóa biểu', icon: 'Calendar', route: '/student/schedule', category: 'Sinh viên' },
    { id: 'student-attendance', label: 'Điểm danh', icon: 'ClipboardCheck', route: '/student/attendance', category: 'Sinh viên' },
    { id: 'student-tuition', label: 'Học phí', icon: 'CreditCard', route: '/student/tuition', category: 'Sinh viên' },
    { id: 'student-registrations', label: 'Đăng ký môn', icon: 'ClipboardList', route: '/student/registrations', category: 'Sinh viên' },
    { id: 'student-requests', label: 'Đơn từ', icon: 'Inbox', route: '/student/requests', category: 'Sinh viên' },
  ]
}

function execute(item) {
  if (!item) return
  if (item.route) {
    recentStore.visitPage({ path: item.route, label: item.label, icon: item.icon })
    router.push(item.route)
  }
  emit('close')
}

function onKeydown(e) {
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    selectedIndex.value = (selectedIndex.value + 1) % filteredCommands.value.length
    scrollToSelected()
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    selectedIndex.value = (selectedIndex.value - 1 + filteredCommands.value.length) % filteredCommands.value.length
    scrollToSelected()
  } else if (e.key === 'Enter') {
    e.preventDefault()
    execute(selectedCommand.value)
  }
}

function scrollToSelected() {
  nextTick(() => {
    const el = resultsRef.value?.querySelector('[data-selected]')
    el?.scrollIntoView({ block: 'nearest' })
  })
}

watch(query, () => { selectedIndex.value = 0 })

onMounted(() => {
  nextTick(() => inputRef.value?.focus())
})

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Circle
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-[200] flex items-start justify-center pt-[12vh]" @click.self="emit('close')">
      <div class="fixed inset-0 bg-slate-900/30 backdrop-blur-sm" @click="emit('close')" />
      <div
        class="relative w-full max-w-[580px] overflow-hidden rounded-[24px] border border-white/60 dark:border-white/10 bg-white/92 dark:bg-slate-900/90 shadow-[0_30px_80px_rgba(15,23,42,0.25)] dark:shadow-[0_30px_80px_rgba(2,6,23,0.5)] backdrop-blur-2xl"
        role="dialog"
        aria-label="Command palette"
        @keydown="onKeydown"
      >
        <div class="flex items-center gap-3 border-b border-slate-100/50 dark:border-white/10 px-4 py-3">
          <LucideIcons.Search :size="16" class="flex-shrink-0 text-slate-400 dark:text-slate-500" />
          <input
            ref="inputRef"
            v-model="query"
            type="text"
            placeholder="Tìm kiếm trang, tính năng..."
            class="w-full bg-transparent text-sm text-slate-800 dark:text-slate-100 outline-none placeholder:text-slate-400 dark:placeholder:text-slate-500"
            @keydown="onKeydown"
          />
          <kbd class="flex-shrink-0 hidden rounded-md border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-1.5 py-0.5 text-[10px] font-bold text-slate-400 dark:text-slate-500 sm:inline-block">ESC</kbd>
        </div>

        <div ref="resultsRef" class="max-h-[360px] overflow-y-auto p-2 space-y-0.5">
          <div v-if="filteredCommands.length === 0" class="flex flex-col items-center py-10 text-center">
            <LucideIcons.SearchX :size="28" class="text-slate-300 dark:text-slate-600" />
            <p class="mt-2 text-sm font-semibold text-slate-500 dark:text-slate-400">Không tìm thấy kết quả</p>
            <p class="text-xs text-slate-400 dark:text-slate-500">Thử tìm kiếm với từ khóa khác</p>
          </div>

          <div v-for="(item, i) in filteredCommands" :key="item.id">
            <button
              :data-selected="i === selectedIndex ? '' : undefined"
              class="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition-all duration-150 focus:outline-none"
              :class="i === selectedIndex
                ? 'bg-blue-50 dark:bg-blue-600/20 text-blue-700 dark:text-blue-300 shadow-sm'
                : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-white/5'"
              @click="execute(item)"
              @mouseenter="selectedIndex = i"
            >
              <component :is="getIcon(item.icon)" :size="16" class="flex-shrink-0"
                :class="i === selectedIndex ? 'text-blue-600 dark:text-blue-400' : 'text-slate-400 dark:text-slate-500'" />
              <span class="flex-1 truncate font-medium">{{ item.label }}</span>
              <span class="flex-shrink-0 text-[10px] font-semibold text-slate-400 dark:text-slate-500">{{ item.category }}</span>
            </button>
          </div>
        </div>

        <div class="flex items-center gap-4 border-t border-slate-100/50 dark:border-white/10 px-4 py-2.5 text-[10px] text-slate-400 dark:text-slate-500">
          <span class="flex items-center gap-1"><kbd class="rounded border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-1.5 py-0.5 font-bold">↑↓</kbd> Di chuyển</span>
          <span class="flex items-center gap-1"><kbd class="rounded border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-1.5 py-0.5 font-bold">↵</kbd> Chọn</span>
          <span class="flex items-center gap-1"><kbd class="rounded border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-1.5 py-0.5 font-bold">ESC</kbd> Đóng</span>
        </div>
      </div>
    </div>
  </Teleport>
</template>
