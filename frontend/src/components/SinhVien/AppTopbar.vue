<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { Search, Bell, X, Menu, Target, Sparkles } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import { useRoute, useRouter } from 'vue-router'
import { mockUser, mockNotifications } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'

const emit = defineEmits(['toggle-sidebar'])
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Refs for click outside
const profileMenuRef = ref(null)
const notifMenuRef = ref(null)

// Search
const searchQuery = ref('')
const searchFocused = ref(false)

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

function notifColorClass(color) {
  const map = {
    red: 'bg-red-100 text-red-600',
    green: 'bg-green-100 text-green-600',
    blue: 'bg-blue-100 text-blue-600',
    yellow: 'bg-yellow-100 text-yellow-600',
  }
  return map[color] || 'bg-slate-100 text-slate-600'
}

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Bell
}

function logout() {
  authStore.logout()
  closeAll()
  router.replace('/login')
}
</script>

<template>
  <header class="lg-topbar sticky top-3 z-40 mx-3 mt-3 flex h-16 flex-shrink-0 items-center gap-3 rounded-[26px] px-4 sm:mx-4 sm:mt-4 sm:gap-4 sm:px-5">
    <!-- Mobile toggle -->
    <button
      class="lg-icon-button flex p-2 text-slate-500 hover:text-blue-700 lg:hidden"
      aria-label="Mở menu"
      @click="emit('toggle-sidebar')"
    >
      <Menu :size="20" />
    </button>

    <!-- Page title -->
    <div class="hidden min-w-0 flex-1 sm:block">
      <div class="flex items-center gap-2 text-[10px] font-bold uppercase tracking-wider text-slate-500">
        <Sparkles :size="12" class="text-cyan-600" />
        Student workspace
      </div>
      <div class="mt-0.5 flex min-w-0 items-baseline gap-2">
        <h1 class="truncate text-lg font-bold leading-tight text-slate-950">{{ currentMeta.title }}</h1>
        <span class="hidden text-xs font-medium text-slate-500 lg:inline">{{ currentMeta.subtitle }}</span>
      </div>
    </div>

    <!-- Search -->
    <div class="relative hidden items-center md:flex">
      <div
        :class="[
          'lg-control flex h-10 items-center gap-2 px-3 transition-all duration-200',
          searchFocused ? 'w-80' : 'w-64',
        ]"
      >
        <Search :size="14" class="flex-shrink-0 text-slate-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm..."
          class="w-full bg-transparent text-sm text-slate-700 outline-none placeholder:text-slate-400"
          aria-label="Tìm kiếm"
          @focus="searchFocused = true"
          @blur="searchFocused = false"
        />
      </div>
    </div>

    <button
      class="lg-button-secondary hidden h-9 px-3 text-xs font-bold text-slate-700 xl:inline-flex"
      aria-label="Bật chế độ tập trung"
    >
      <Target :size="14" />
      Tập trung
    </button>

    <!-- Notification -->
    <div ref="notifMenuRef" class="relative">
      <button
        :class="[
          'lg-icon-button relative h-9 w-9 border border-white/50 bg-white/45 text-slate-500 shadow-sm backdrop-blur-xl focus:outline-none',
          notifOpen ? 'bg-blue-50 text-blue-700 shadow-md' : 'hover:text-blue-700',
        ]"
        aria-label="Thông báo"
        @click.stop="toggleNotif"
      >
        <Bell :size="17" />
        <span
          v-if="unreadCount > 0"
          class="absolute -right-0.5 -top-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-red-500 text-[8px] font-bold text-white ring-2 ring-white"
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
          class="absolute right-0 top-full z-[100] mt-3 w-[320px] origin-top-right overflow-hidden rounded-[24px] border border-white/60 bg-white/80 p-1 shadow-[0_20px_50px_rgba(0,0,0,0.15)] backdrop-blur-2xl"
          @click.stop
        >
          <div class="flex items-center justify-between border-b border-slate-100/50 px-4 py-3">
            <h3 class="text-sm font-bold text-slate-800">Thông báo</h3>
            <span v-if="unreadCount" class="rounded-full bg-blue-100 px-2 py-0.5 text-[10px] font-bold text-blue-700">
              {{ unreadCount }} mới
            </span>
          </div>
          <div class="max-h-[320px] overflow-y-auto divide-y divide-slate-50/50">
            <div
              v-for="notif in mockNotifications"
              :key="notif.id"
              class="flex cursor-pointer gap-3 px-4 py-3 transition-all hover:bg-white/60 active:scale-[0.98]"
              @click="closeAll"
            >
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-[10px]', notifColorClass(notif.color)]">
                <component :is="getIcon(notif.icon)" :size="14" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-[12px] font-bold leading-tight text-slate-800">{{ notif.title }}</p>
                <p class="mt-0.5 line-clamp-2 text-[11px] font-medium leading-snug text-slate-500">{{ notif.description }}</p>
                <p class="mt-1.5 text-[10px] font-bold text-slate-400">{{ notif.time }}</p>
              </div>
            </div>
          </div>
          <div class="border-t border-slate-100/50 px-4 py-2.5 text-center bg-slate-50/30">
            <button class="text-[11px] font-bold text-blue-600 hover:text-blue-700">Xem tất cả thông báo</button>
          </div>
        </div>
      </Transition>
    </div>

    <!-- Profile menu -->
    <div ref="profileMenuRef" class="relative">
      <button
        :class="[
          'flex items-center gap-2 rounded-xl border border-white/45 bg-white/45 p-1 transition-all duration-200 focus:outline-none ring-offset-2 focus:ring-2 focus:ring-blue-500/20',
          userMenuOpen ? 'bg-white shadow-md' : 'hover:bg-white/70',
        ]"
        aria-haspopup="menu"
        :aria-expanded="userMenuOpen"
        aria-label="Mở hồ sơ"
        @click.stop="toggleUserMenu"
      >
        <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-blue-600 to-cyan-500 text-[10px] font-bold text-white shadow-sm ring-1 ring-white/70">
          {{ authStore.initials || mockUser.initials }}
        </div>
        <div class="hidden pr-1.5 text-left sm:block">
          <p class="text-[12px] font-bold leading-tight text-slate-800">{{ authStore.displayName || mockUser.name }}</p>
          <p class="text-[10px] font-medium text-slate-400">{{ authStore.role || 'Sinh viên' }}</p>
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
          class="absolute right-0 top-full z-[100] mt-3 w-[240px] origin-top-right overflow-hidden rounded-[24px] border border-white/60 bg-white/80 p-1.5 shadow-[0_20px_50px_rgba(0,0,0,0.15)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <div class="border-b border-slate-100/50 px-4 py-3.5 bg-white/40">
            <p class="text-[13px] font-bold text-slate-800">{{ authStore.displayName || mockUser.name }}</p>
            <p class="mt-0.5 truncate text-[11px] font-medium text-slate-500">{{ authStore.user?.email || mockUser.email }}</p>
            <span class="mt-2.5 inline-flex items-center gap-1.5 rounded-full bg-blue-50 px-2.5 py-1 text-[9px] font-bold text-blue-700 border border-blue-100 shadow-sm">
              <LucideIcons.GraduationCap :size="11" />
              Cơ sở {{ authStore.user?.campusId || mockUser.campus }}
            </span>
          </div>

          <div class="p-1 space-y-0.5">
            <router-link
              to="/student/profile"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-slate-600 transition-all hover:bg-white hover:text-blue-700 hover:shadow-sm"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-slate-400" />
              Hồ sơ cá nhân
            </router-link>
            <router-link
              to="/student/tuition"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-slate-600 transition-all hover:bg-white hover:text-blue-700 hover:shadow-sm"
              @click="closeAll"
            >
              <LucideIcons.CreditCard :size="16" class="text-slate-400" />
              Học phí
            </router-link>
          </div>

          <div class="border-t border-slate-100/50 p-1">
            <button
              class="flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-red-600 transition-all hover:bg-red-50 hover:text-red-700"
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

