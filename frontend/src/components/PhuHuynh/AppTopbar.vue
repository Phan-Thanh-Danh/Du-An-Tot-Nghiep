<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { Search, Bell, Menu, Target } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import { useRoute, useRouter } from 'vue-router'
import { mockParentUser } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'
import CommandPalette from '@/components/ui/CommandPalette.vue'
import Breadcrumbs from '@/components/ui/Breadcrumbs.vue'
import MiniCalendar from '@/components/ui/MiniCalendar.vue'

const emit = defineEmits(['toggle-sidebar'])
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const recentStore = useRecentFavoritesStore()

const commandPaletteOpen = ref(false)

// Refs for click outside
const profileMenuRef = ref(null)
const notifMenuRef = ref(null)

// Panels state
const notifOpen = ref(false)
const userMenuOpen = ref(false)

// Parent notifications mock
const mockNotifications = [
  {
    id: 1,
    type: 'alert',
    title: 'Cảnh báo: Con vắng mặt không phép',
    description: 'Học sinh Nguyễn Minh Quân vắng mặt tiết 2 môn Toán ngày 10/06.',
    time: '1 giờ trước',
    read: false,
    icon: 'AlertTriangle',
    color: 'red',
    link: '/parent/learning/attendance'
  },
  {
    id: 2,
    type: 'tuition',
    title: 'Thông báo học phí học kỳ mới',
    description: 'Học phí học kỳ 2 đã được mở cổng đóng phí. Hạn đóng: 20/06/2026.',
    time: '1 ngày trước',
    read: false,
    icon: 'Wallet',
    color: 'yellow',
    link: '/parent/finance/tuition'
  },
  {
    id: 3,
    type: 'grade',
    title: 'Kết quả thi giữa kỳ có cập nhật',
    description: 'Môn Cấu trúc dữ liệu & Giải thuật của con có điểm thi: 8.5.',
    time: '2 ngày trước',
    read: true,
    icon: 'Award',
    color: 'green',
    link: '/parent/learning/grades'
  }
]

const unreadCount = computed(() => mockNotifications.filter((n) => !n.read).length)

const pageTitleMap = {
  '/parent/dashboard': { title: 'Dashboard Phụ huynh', subtitle: 'Tổng quan học tập và tài chính của con' },
  '/parent/children/list': { title: 'Danh sách học sinh', subtitle: 'Danh sách các con đang theo học' },
  '/parent/children/overview': { title: 'Tổng quan học tập', subtitle: 'Kết quả học tập chi tiết của con' },
  '/parent/learning/grades': { title: 'Bảng điểm của con', subtitle: 'Điểm số chi tiết các môn học' },
  '/parent/learning/attendance': { title: 'Tình hình điểm danh', subtitle: 'Lịch sử chuyên cần lớp học' },
  '/parent/learning/schedule': { title: 'Thời khóa biểu của con', subtitle: 'Lịch học và lịch thi hàng tuần' },
  '/parent/learning/alerts': { title: 'Cảnh báo học tập', subtitle: 'Cảnh báo vắng mặt hoặc điểm số kém' },
  '/parent/finance/tuition': { title: 'Công nợ học phí', subtitle: 'Thông tin học phí và công nợ hiện tại' },
  '/parent/finance/payment': { title: 'Thanh toán học phí', subtitle: 'Cổng thanh toán học phí trực tuyến' },
  '/parent/finance/transactions': { title: 'Lịch sử giao dịch', subtitle: 'Nhật ký các đợt thanh toán trước đây' },
  '/parent/finance/invoices': { title: 'Hóa đơn học phí', subtitle: 'Quản lý hóa đơn và chứng từ thanh toán' },
  '/parent/notifications/system': { title: 'Cảnh báo hệ thống', subtitle: 'Các cảnh báo tự động từ hệ thống LMS' },
  '/parent/notifications/history': { title: 'Lịch sử thông báo', subtitle: 'Các tin tức, thông báo gửi đến phụ huynh' },
  '/parent/profile/info': { title: 'Hồ sơ phụ huynh', subtitle: 'Thông tin cá nhân liên lạc' },
  '/parent/profile/access-rights': { title: 'Quyền truy cập được cấp', subtitle: 'Các quyền giám sát thông tin học tập' },
}

const currentMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [path, meta] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(`${path}/`)) return meta
  }
  return { title: 'Trang Phụ huynh', subtitle: 'Không gian giám sát học tập' }
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

function handleGlobalKeydown(e) {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault()
    commandPaletteOpen.value = true
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  document.addEventListener('keydown', handleEscape)
  document.addEventListener('keydown', handleGlobalKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  document.removeEventListener('keydown', handleEscape)
  document.removeEventListener('keydown', handleGlobalKeydown)
})

watch(() => route.fullPath, (path) => {
  userMenuOpen.value = false
  notifOpen.value = false
  const title = currentMeta.value?.title
  if (title && path && !path.startsWith('/login')) {
    const icon = guessIcon(path)
    recentStore.visitPage({ path, label: title, icon })
  }
})

function guessIcon(path) {
  if (path.includes('dashboard')) return 'LayoutDashboard'
  if (path.includes('grades')) return 'Award'
  if (path.includes('attendance')) return 'UserCheck'
  if (path.includes('schedule')) return 'Calendar'
  if (path.includes('alerts')) return 'AlertTriangle'
  if (path.includes('tuition')) return 'BadgeAlert'
  if (path.includes('payment')) return 'CreditCard'
  if (path.includes('transactions')) return 'History'
  if (path.includes('invoices')) return 'FileText'
  if (path.includes('system')) return 'ShieldAlert'
  if (path.includes('history')) return 'MessageSquare'
  if (path.includes('info')) return 'UserCircle'
  if (path.includes('access-rights')) return 'ShieldCheck'
  return 'Circle'
}

function notifColorClass(color) {
  const map = {
    red: 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]',
    green: 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]',
    blue: 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]',
    yellow: 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]',
  }
  return map[color] || 'surface-input text-label'
}

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Bell
}

const logout = () => {
  authStore.logout()
  closeAll()
  router.replace('/login')
}
</script>

<template>
  <header class="lg-topbar absolute top-0 left-0 right-0 z-50 mx-2 mt-2 flex h-12 flex-shrink-0 items-center gap-2 overflow-visible rounded-[var(--radius-xl)] px-3 sm:mx-3 sm:mt-2 sm:gap-2.5">
    <!-- Mobile toggle -->
    <button
      class="lg-icon-button flex h-8 w-8 text-muted hover:text-link lg:hidden"
      aria-label="Mở menu"
      @click="emit('toggle-sidebar')"
    >
      <Menu :size="20" />
    </button>

    <!-- Page title + Breadcrumbs -->
    <div class="hidden min-w-0 flex-1 sm:block">
      <Breadcrumbs class="mb-0.5" />
      <div class="flex min-w-0 items-baseline gap-2">
        <h1 class="truncate text-base font-bold leading-tight text-heading">{{ currentMeta.title }}</h1>
        <span class="hidden text-xs font-medium text-muted lg:inline">{{ currentMeta.subtitle }}</span>
      </div>
    </div>

    <!-- Command palette trigger (Cmd+K) -->
    <button
      class="surface-input hidden h-8 items-center gap-1.5 rounded-[var(--radius-md)] border border-card px-2.5 text-[10px] font-semibold text-label shadow-sm transition-all hover:bg-[var(--surface-input-focus)] hover:text-heading focus:outline-none md:inline-flex"
      aria-label="Mở command palette"
      @click="commandPaletteOpen = true"
    >
      <Search :size="14" />
      <span class="hidden sm:inline">Tìm kiếm</span>
      <kbd class="border-card surface-card ml-1 hidden rounded-md border px-1.5 py-0.5 text-[9px] font-semibold text-muted xl:inline-block">⌘K</kbd>
    </button>

    <!-- Focus Mode button -->
    <button
      class="lg-button-secondary hidden h-8 px-2.5 text-[10px] font-semibold text-label xl:inline-flex"
      aria-label="Bật chế độ tập trung"
    >
      <Target :size="12" />
      Tập trung
    </button>

    <MiniCalendar />
    <ThemeToggle />

    <!-- Notification -->
    <div ref="notifMenuRef" class="relative">
      <button
        :class="[
          'lg-icon-button surface-input relative h-8 w-8 border border-card text-label shadow-sm focus:outline-none',
          notifOpen ? 'bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm' : 'hover:text-link',
        ]"
        aria-label="Thông báo"
        aria-haspopup="menu"
        :aria-expanded="notifOpen"
        @click.stop="toggleNotif"
      >
        <Bell :size="15" />
        <span
          v-if="unreadCount > 0"
          class="absolute -right-0.5 -top-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-orange-600 text-[8px] font-bold text-inverse ring-2 ring-[var(--surface-topbar)]"
        >
          {{ unreadCount }}
        </span>
      </button>

      <!-- Dropdown -->
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
          class="surface-dropdown absolute right-0 top-[calc(100%+0.55rem)] z-[80] w-80 origin-top-right overflow-hidden rounded-[var(--radius-xl)] border border-card p-1 shadow-[var(--lg-shadow-md)]"
          role="menu"
          @click.stop
        >
          <div class="border-card flex items-center justify-between border-b px-3 py-2.5">
            <h3 class="text-sm font-semibold text-heading">Thông báo</h3>
            <span v-if="unreadCount" class="rounded-full bg-orange-100 dark:bg-orange-900/20 px-2 py-0.5 text-[10px] font-semibold text-orange-600">
              {{ unreadCount }} mới
            </span>
          </div>
          <div class="max-h-[320px] divide-y divide-[var(--border-card)] overflow-y-auto" role="none">
            <div
              v-for="notif in mockNotifications"
              :key="notif.id"
              class="flex cursor-pointer gap-3 px-3 py-2.5 transition-all hover:bg-[var(--surface-card-hover)] active:scale-[0.98]"
              role="menuitem"
              tabindex="0"
              @click="() => { closeAll(); if(notif.link) router.push(notif.link); }"
            >
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-[10px]', notifColorClass(notif.color)]">
                <component :is="getIcon(notif.icon)" :size="14" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-medium leading-tight text-heading">{{ notif.title }}</p>
                <p class="mt-0.5 line-clamp-2 text-[11px] font-medium leading-snug text-body">{{ notif.description }}</p>
                <p class="mt-1.5 text-[10px] font-semibold text-muted">{{ notif.time }}</p>
              </div>
            </div>
          </div>
          <div class="border-card surface-card border-t px-3 py-2 text-center">
            <button class="text-[11px] font-bold text-orange-600 hover:text-heading" @click="router.push('/parent/notifications/history'); closeAll()">Xem tất cả thông báo</button>
          </div>
        </div>
      </Transition>
    </div>

    <!-- Profile menu -->
    <div ref="profileMenuRef" class="relative">
      <button
        :class="[
          'surface-input flex items-center gap-2 rounded-[var(--radius-md)] border border-card p-1.5 pr-2.5 transition-all duration-200 focus:outline-none ring-offset-2 focus:ring-2 focus:ring-[var(--focus-ring)]',
          userMenuOpen ? 'bg-[var(--surface-input-focus)] shadow-sm' : 'hover:bg-[var(--surface-input-focus)]',
        ]"
        aria-haspopup="menu"
        :aria-expanded="userMenuOpen"
        aria-label="Mở hồ sơ"
        @click.stop="toggleUserMenu"
      >
        <div class="app-topbar-avatar flex h-8 w-8 items-center justify-center overflow-hidden rounded-full text-[10px] font-bold text-inverse shadow-sm ring-1 ring-[var(--border-card)] bg-orange-100 text-orange-700">
          <img v-if="authStore.user?.avatar" :src="authStore.user.avatar" class="h-full w-full object-cover" />
          <span v-else>{{ authStore.initials || mockParentUser.initials }}</span>
        </div>
        <div class="hidden pr-1.5 text-left sm:block">
          <p class="text-[12px] font-semibold leading-tight text-heading">{{ authStore.displayName || mockParentUser.name }}</p>
          <p class="text-[10px] font-medium capitalize text-muted">Phụ huynh</p>
        </div>
      </button>

      <!-- Dropdown -->
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
          class="surface-dropdown absolute right-0 top-[calc(100%+0.55rem)] z-[80] w-72 origin-top-right overflow-hidden rounded-[var(--radius-xl)] border border-card p-1.5 shadow-[var(--lg-shadow-md)]"
          role="menu"
          @click.stop
        >
          <div class="border-card surface-card border-b px-3 py-3">
            <p class="text-[13px] font-semibold text-heading">{{ authStore.displayName || mockParentUser.name }}</p>
            <p class="mt-0.5 truncate text-[11px] font-medium text-muted">{{ authStore.user?.email || mockParentUser.email }}</p>
            <span class="mt-2 inline-flex items-center gap-1.5 rounded-full border border-card bg-orange-50 dark:bg-orange-950/20 px-2.5 py-1 text-[10px] font-semibold text-orange-600 shadow-sm">
              <LucideIcons.User :size="11" />
              {{ mockParentUser.campus }}
            </span>
          </div>

          <div class="p-1 space-y-0.5">
            <router-link
              to="/parent/profile/info"
              class="flex items-center gap-2.5 rounded-[var(--radius-md)] px-3 py-2 text-[12px] font-semibold text-label transition-all hover:bg-[var(--surface-card-hover)] hover:text-orange-600"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-muted" />
              Hồ sơ cá nhân
            </router-link>
            <router-link
              to="/parent/finance/tuition"
              class="flex items-center gap-2.5 rounded-[var(--radius-md)] px-3 py-2 text-[12px] font-semibold text-label transition-all hover:bg-[var(--surface-card-hover)] hover:text-orange-600"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.CreditCard :size="16" class="text-muted" />
              Công nợ học phí
            </router-link>
          </div>

          <div class="border-card border-t p-1">
            <button
              class="flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-semibold text-red-600 dark:text-red-400 transition-all hover:bg-red-50 dark:hover:bg-red-600/20 hover:text-red-700 dark:hover:text-red-300"
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

  <CommandPalette v-if="commandPaletteOpen" @close="commandPaletteOpen = false" />
</template>

<style scoped>
.app-topbar-avatar {
  background: linear-gradient(
    135deg,
    var(--active-start, #c2410c),
    var(--active-mid, #ea580c) 55%,
    var(--active-end, #fb923c)
  );
}
</style>
