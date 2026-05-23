<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { Search, Bell, Menu, Target, Sparkles } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import { useRoute, useRouter } from 'vue-router'
import { mockUser, mockNotifications } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'

const emit = defineEmits(['toggle-sidebar'])
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Refs for click outside
const profileMenuRef = ref(null)
const notifMenuRef = ref(null)

// Search
const searchQuery = ref('')
// Panels state
const notifOpen = ref(false)
const userMenuOpen = ref(false)
const unreadCount = computed(() => mockNotifications.filter((n) => !n.read).length)

const pageTitleMap = {
  '/student/dashboard': { title: 'Dashboard', subtitle: 'Tổng quan học tập hôm nay' },
  '/student/courses': { title: 'Khóa học', subtitle: 'Tiếp tục bài học và theo dõi tiến độ' },
  '/student/assignments': { title: 'Bài tập', subtitle: 'Deadline, bài nộp và phản hồi' },
  '/student/exams': { title: 'Thi / Kiểm tra', subtitle: 'Bài thi đang mở và kết quả đã công bố' },
  '/student/grades': { title: 'Bảng điểm', subtitle: 'GPA, điểm môn và trạng thái học phần' },
  '/student/schedule': { title: 'Lịch học', subtitle: 'Buổi học, phòng học và giảng viên' },
  '/student/attendance': { title: 'Điểm danh', subtitle: 'Chuyên cần và cảnh báo vắng học' },
  '/student/registrations': { title: 'Đăng ký', subtitle: 'Đợt đăng ký và lớp học phần' },
  '/student/tuition': { title: 'Tài chính', subtitle: 'Học phí, hóa đơn và thanh toán' },
  '/student/support-tickets': { title: 'Hỗ trợ', subtitle: 'Ticket và phản hồi từ nhà trường' },
  '/student/requests': { title: 'Đơn từ', subtitle: 'Yêu cầu học vụ đang xử lý' },
  '/student/evaluations': { title: 'Đánh giá', subtitle: 'Khảo sát chất lượng giảng dạy' },
  '/student/profile': { title: 'Cá nhân', subtitle: 'Hồ sơ và cài đặt tài khoản' },
  '/student/parent-links': { title: 'Phụ huynh', subtitle: 'Quyền truy cập thông tin học tập' },
}

const currentMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [path, meta] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(`${path}/`)) return meta
  }
  return { title: 'Trang học sinh', subtitle: 'Không gian học tập cá nhân' }
})

function toggleNotif() {
  notifOpen.value = !notifOpen.value
  userMenuOpen.value = false
}

function toggleUserMenu() {
  userMenuOpen.value = !userMenuOpen.value
  notifOpen.value = false
}

function closeAll() {
  notifOpen.value = false
  userMenuOpen.value = false
}

// Click outside logic
function handleClickOutside(event) {
  if (profileMenuRef.value && !profileMenuRef.value.contains(event.target)) {
    userMenuOpen.value = false
  }
  if (notifMenuRef.value && !notifMenuRef.value.contains(event.target)) {
    notifOpen.value = false
  }
}

function handleEscape(event) {
  if (event.key === 'Escape') closeAll()
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  document.addEventListener('keydown', handleEscape)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  document.removeEventListener('keydown', handleEscape)
})

watch(() => route.fullPath, () => {
  userMenuOpen.value = false
  notifOpen.value = false
})

function notifColorClass(color) {
  const map = {
    red: 'bg-red-100 dark:bg-red-600/25 text-red-600 dark:text-red-300',
    green: 'bg-green-100 dark:bg-green-600/25 text-green-600 dark:text-green-300',
    blue: 'bg-blue-100 dark:bg-blue-600/25 text-blue-600 dark:text-blue-300',
    yellow: 'bg-yellow-100 dark:bg-yellow-600/25 text-yellow-600 dark:text-yellow-300',
  }
  return map[color] || 'bg-slate-100 dark:bg-slate-600/25 text-slate-600 dark:text-slate-300'
}

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Bell
}

const logout = () => {
  authStore.logout()
  closeAll()
  router.replace('/login')
}

// ── Role-aware logic ──────────────────────────────────────
const profileLink = computed(() => {
  if (authStore.hasRole('Principal')) return '/bgh/profile'
  if (authStore.hasRole('Teacher')) return '/teacher/profile'
  if (authStore.hasRole('AcademicStaff')) return '/staff/profile'
  return '/student/profile'
})

const isStudent = computed(() => authStore.hasRole('Student'))

const workspaceName = computed(() => {
  if (authStore.hasRole('Principal')) return 'Management workspace'
  if (authStore.hasRole('Teacher')) return 'Faculty workspace'
  if (authStore.hasRole('AcademicStaff')) return 'Staff workspace'
  return 'Student workspace'
})
</script>

<template>
  <header class="lg-topbar sticky top-3 z-20 mx-3 mt-3 flex h-16 flex-shrink-0 items-center gap-3 overflow-visible rounded-[26px] px-4 sm:mx-4 sm:mt-4 sm:gap-4 sm:px-5">
    <!-- Mobile toggle -->
    <button
      class="lg-icon-button flex p-2 text-slate-500 dark:text-slate-400 hover:text-blue-700 dark:hover:text-blue-300 lg:hidden"
      aria-label="Mở menu"
      @click="emit('toggle-sidebar')"
    >
      <Menu :size="20" />
    </button>

    <!-- Page title -->
    <div class="hidden min-w-0 flex-1 sm:block">
      <div class="flex items-center gap-2 text-[10px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400">
        <Sparkles :size="12" class="text-indigo-500 dark:text-indigo-400" v-if="authStore.hasRole('Principal')" />
        <Sparkles :size="12" class="text-cyan-600 dark:text-cyan-400" v-else />
        {{ workspaceName }}
      </div>
      <div class="mt-0.5 flex min-w-0 items-baseline gap-2">
        <h1 class="truncate text-lg font-bold leading-tight text-slate-950 dark:text-slate-100">{{ currentMeta.title }}</h1>
        <span class="hidden text-xs font-medium text-slate-500 dark:text-slate-400 lg:inline">{{ currentMeta.subtitle }}</span>
      </div>
    </div>

    <!-- Search -->
    <div class="relative hidden items-center md:flex">
      <div
        class="lg-control flex h-10 w-64 items-center gap-2 px-3 transition-colors duration-200 xl:w-72"
      >
        <Search :size="14" class="flex-shrink-0 text-slate-400 dark:text-slate-500" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm..."
          class="w-full bg-transparent text-sm text-slate-700 dark:text-slate-200 outline-none placeholder:text-slate-400 dark:placeholder:text-slate-500"
          aria-label="Tìm kiếm"
        />
      </div>
    </div>

    <button
      class="lg-button-secondary hidden h-9 px-3 text-xs font-bold text-slate-700 dark:text-slate-300 xl:inline-flex"
      aria-label="Bật chế độ tập trung"
    >
      <Target :size="14" />
      Tập trung
    </button>

    <ThemeToggle />

    <!-- Notification -->
    <div ref="notifMenuRef" class="relative">
      <button
        :class="[
          'lg-icon-button relative h-9 w-9 border border-white/50 dark:border-white/10 bg-white/45 dark:bg-slate-700/40 text-slate-500 dark:text-slate-400 shadow-sm backdrop-blur-xl focus:outline-none',
          notifOpen ? 'bg-blue-50 dark:bg-blue-600/25 text-blue-700 dark:text-blue-300 shadow-md' : 'hover:text-blue-700 dark:hover:text-blue-300',
        ]"
        aria-label="Thông báo"
        aria-haspopup="menu"
        :aria-expanded="notifOpen"
        @click.stop="toggleNotif"
      >
        <Bell :size="17" />
        <span
          v-if="unreadCount > 0"
          class="absolute -right-0.5 -top-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-red-500 text-[8px] font-bold text-white ring-2 ring-white dark:ring-slate-800"
        >
          {{ unreadCount }}
        </span>
      </button>

      <!-- Dropdown đổ xuống ngay tại đây -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-2 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-2 scale-95"
      >
        <div
          v-if="notifOpen"
          class="absolute right-0 top-[calc(100%+0.75rem)] z-[80] w-80 origin-top-right overflow-hidden rounded-[24px] border border-white/60 dark:border-white/10 bg-white/88 dark:bg-slate-800/80 p-1 shadow-[0_20px_50px_rgba(0,0,0,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <div class="flex items-center justify-between border-b border-slate-100/50 dark:border-white/10 px-4 py-3">
            <h3 class="text-sm font-bold text-slate-800 dark:text-slate-100">Thông báo</h3>
            <span v-if="unreadCount" class="rounded-full bg-blue-100 dark:bg-blue-600/25 px-2 py-0.5 text-[10px] font-bold text-blue-700 dark:text-blue-300">
              {{ unreadCount }} mới
            </span>
          </div>
          <div class="max-h-[320px] divide-y divide-slate-50/50 dark:divide-white/5 overflow-y-auto" role="none">
            <div
              v-for="notif in mockNotifications"
              :key="notif.id"
              class="flex cursor-pointer gap-3 px-4 py-3 transition-all hover:bg-white/60 dark:hover:bg-white/5 active:scale-[0.98]"
              role="menuitem"
              tabindex="0"
              @click="closeAll"
            >
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-[10px]', notifColorClass(notif.color)]">
                <component :is="getIcon(notif.icon)" :size="14" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-semibold leading-tight text-slate-800 dark:text-slate-100">{{ notif.title }}</p>
                <p class="mt-0.5 line-clamp-2 text-[11px] font-medium leading-snug text-slate-500 dark:text-slate-400">{{ notif.description }}</p>
                <p class="mt-1.5 text-[10px] font-semibold text-slate-400 dark:text-slate-500">{{ notif.time }}</p>
              </div>
            </div>
          </div>
          <div class="border-t border-slate-100/50 dark:border-white/10 px-4 py-2.5 text-center bg-slate-50/30 dark:bg-slate-700/20">
            <button class="text-[11px] font-bold text-blue-600 dark:text-blue-300 hover:text-blue-700 dark:hover:text-blue-200">Xem tất cả thông báo</button>
          </div>
        </div>
      </Transition>
    </div>

    <!-- Profile menu -->
    <div ref="profileMenuRef" class="relative">
      <button
        :class="[
          'flex items-center gap-2 rounded-xl border border-white/45 dark:border-white/10 bg-white/45 dark:bg-slate-700/40 p-1 transition-all duration-200 focus:outline-none ring-offset-2 focus:ring-2 focus:ring-blue-500/20',
          userMenuOpen ? 'bg-white dark:bg-slate-700/60 shadow-md' : 'hover:bg-white/70 dark:hover:bg-slate-700/60',
        ]"
        aria-haspopup="menu"
        :aria-expanded="userMenuOpen"
        aria-label="Mở hồ sơ"
        @click.stop="toggleUserMenu"
      >
        <div :class="['flex h-8 w-8 items-center justify-center rounded-full text-[10px] font-bold text-white shadow-sm ring-1 ring-white/70 dark:ring-white/20 overflow-hidden bg-gradient-to-br',
          authStore.hasRole('Principal') ? 'from-indigo-600 to-purple-600' : 'from-blue-600 to-cyan-500']">
          <img v-if="authStore.user?.avatar" :src="authStore.user.avatar" class="h-full w-full object-cover" />
          <span v-else>{{ authStore.initials || mockUser.initials }}</span>
        </div>
        <div class="hidden pr-1.5 text-left sm:block">
          <p class="text-[12px] font-bold leading-tight text-slate-800 dark:text-slate-100">{{ authStore.displayName || mockUser.name }}</p>
          <p class="text-[10px] font-medium text-slate-400 dark:text-slate-500 capitalize">{{ (authStore.role || 'Sinh viên').toLowerCase() }}</p>
        </div>
      </button>

      <!-- Dropdown đổ xuống ngay tại đây -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-2 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-2 scale-95"
      >
        <div
          v-if="userMenuOpen"
          class="absolute right-0 top-[calc(100%+0.75rem)] z-[80] w-72 origin-top-right overflow-hidden rounded-[24px] border border-white/60 dark:border-white/10 bg-white/88 dark:bg-slate-800/80 p-1.5 shadow-[0_20px_50px_rgba(0,0,0,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <div class="border-b border-slate-100/50 dark:border-white/10 px-4 py-3.5 bg-white/40 dark:bg-slate-700/30">
            <p class="text-[13px] font-bold text-slate-800 dark:text-slate-100">{{ authStore.displayName || mockUser.name }}</p>
            <p class="mt-0.5 truncate text-[11px] font-medium text-slate-500 dark:text-slate-400">{{ authStore.user?.email || mockUser.email }}</p>
            <span class="mt-2.5 inline-flex items-center gap-1.5 rounded-full bg-blue-50 dark:bg-blue-600/25 px-2.5 py-1 text-[10px] font-semibold text-blue-700 dark:text-blue-300 border border-blue-100 dark:border-blue-500/30 shadow-sm">
              <LucideIcons.GraduationCap :size="11" />
              Cơ sở {{ authStore.user?.campusId || mockUser.campus }}
            </span>
          </div>

          <div class="p-1 space-y-0.5">
            <router-link
              :to="profileLink"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-slate-600 dark:text-slate-400 transition-all hover:bg-white dark:hover:bg-white/5 hover:text-blue-700 dark:hover:text-blue-300 hover:shadow-sm"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-slate-400 dark:text-slate-500" />
              Hồ sơ cá nhân
            </router-link>
            <router-link
              v-if="isStudent"
              to="/student/tuition"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-slate-600 dark:text-slate-400 transition-all hover:bg-white dark:hover:bg-white/5 hover:text-blue-700 dark:hover:text-blue-300 hover:shadow-sm"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.CreditCard :size="16" class="text-slate-400 dark:text-slate-500" />
              Học phí
            </router-link>
          </div>

          <div class="border-t border-slate-100/50 dark:border-white/10 p-1">
            <button
              class="flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-red-600 dark:text-red-400 transition-all hover:bg-red-50 dark:hover:bg-red-600/20 hover:text-red-700 dark:hover:text-red-300"
              role="menuitem"
              @click="logout"
            >
              <LucideIcons.LogOut :size="16" />
              Đăng xuất
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </header>
</template>
