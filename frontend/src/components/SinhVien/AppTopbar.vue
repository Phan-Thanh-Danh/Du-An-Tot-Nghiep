<script setup>
import { ref, computed } from 'vue'
import { Search, Bell, X, Menu } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import AppBreadcrumb from './AppBreadcrumb.vue'
import { mockUser, mockNotifications } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'

const emit = defineEmits(['toggle-sidebar'])
const router = useRouter()
const authStore = useAuthStore()

// Search
const searchQuery = ref('')
const searchFocused = ref(false)

// Notification panel
const notifOpen = ref(false)
const unreadCount = computed(() => mockNotifications.filter((n) => !n.read).length)

// User menu
const userMenuOpen = ref(false)

function toggleNotif() {
  notifOpen.value = !notifOpen.value
  userMenuOpen.value = false
}

function toggleUserMenu() {
  userMenuOpen.value = !userMenuOpen.value
  notifOpen.value = false
}

// Close popups on outside click
function closeAll() {
  notifOpen.value = false
  userMenuOpen.value = false
}

// Get notification icon color class
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
  userMenuOpen.value = false
  router.replace('/login')
}
</script>

<template>
  <!-- Overlay để close popups -->
  <div v-if="notifOpen || userMenuOpen" class="fixed inset-0 z-30" @click="closeAll" />

  <header class="lg-topbar relative z-40 flex h-[64px] flex-shrink-0 items-center gap-4 px-5">
    <!-- Mobile: toggle sidebar button -->
    <button
      class="lg-icon-button flex p-2 text-slate-500 hover:text-blue-700 lg:hidden"
      aria-label="Mở menu"
      @click="emit('toggle-sidebar')"
    >
      <Menu :size="20" />
    </button>

    <!-- Breadcrumb -->
    <div class="hidden sm:flex flex-1 min-w-0">
      <AppBreadcrumb />
    </div>

    <!-- Spacer on mobile -->
    <div class="flex-1 sm:hidden" />

    <!-- ── Search ── -->
    <div class="relative hidden md:flex items-center">
      <div
        :class="[
          'lg-control flex items-center gap-2 px-3 py-2 transition-all duration-200',
          searchFocused ? 'w-64' : 'w-48',
        ]"
      >
        <Search :size="15" class="flex-shrink-0 text-slate-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm..."
          class="w-full bg-transparent text-sm text-slate-700 outline-none placeholder:text-slate-400"
          aria-label="Tìm kiếm"
          @focus="searchFocused = true"
          @blur="searchFocused = false"
        />
        <button
          v-if="searchQuery"
          class="lg-icon-button p-0.5 text-slate-400 hover:text-slate-600"
          aria-label="Xóa tìm kiếm"
          @click="searchQuery = ''"
        >
          <X :size="13" />
        </button>
      </div>
    </div>

    <!-- ── Notification bell ── -->
    <div class="relative">
      <button
        :class="[
          'lg-icon-button relative h-10 w-10 border border-white/50 bg-white/45 text-slate-500 shadow-sm backdrop-blur-xl focus:outline-none focus:ring-4 focus:ring-blue-500/20',
          notifOpen ? 'bg-blue-50/90 text-blue-700 shadow-md' : 'hover:text-blue-700',
        ]"
        aria-label="Thông báo"
        @click="toggleNotif"
      >
        <Bell :size="18" stroke-width="1.8" />
        <!-- Unread badge -->
        <span
          v-if="unreadCount > 0"
          class="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[9px] font-bold text-white ring-2 ring-white"
        >
          {{ unreadCount > 9 ? '9+' : unreadCount }}
        </span>
      </button>

      <!-- Notification Dropdown -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-1 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-1 scale-95"
      >
        <div
          v-if="notifOpen"
          class="lg-glass-strong absolute right-0 top-full mt-2 w-[340px] overflow-hidden rounded-[24px]"
        >
          <!-- Header -->
          <div class="flex items-center justify-between border-b border-slate-100 px-4 py-3">
            <h3 class="text-sm font-semibold text-slate-800">Thông báo</h3>
            <span v-if="unreadCount" class="rounded-full bg-blue-100 px-2 py-0.5 text-[11px] font-semibold text-blue-700">
              {{ unreadCount }} mới
            </span>
          </div>

          <!-- Notification list -->
          <div class="max-h-[360px] overflow-y-auto divide-y divide-slate-50">
            <div
              v-for="notif in mockNotifications"
              :key="notif.id"
              :class="[
                'flex cursor-pointer gap-3 px-4 py-3.5 transition-colors hover:bg-white/70',
                !notif.read ? 'bg-blue-50/50' : '',
              ]"
            >
              <!-- Icon -->
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs', notifColorClass(notif.color)]">
                <component :is="getIcon(notif.icon)" :size="15" />
              </div>
              <!-- Content -->
              <div class="min-w-0 flex-1">
                <div class="flex items-start justify-between gap-2">
                  <p :class="['text-[13px] leading-snug', !notif.read ? 'font-semibold text-slate-800' : 'font-medium text-slate-700']">
                    {{ notif.title }}
                  </p>
                  <span v-if="!notif.read" class="mt-1 h-1.5 w-1.5 flex-shrink-0 rounded-full bg-blue-500" />
                </div>
                <p class="mt-0.5 text-[12px] text-slate-500 leading-snug">{{ notif.description }}</p>
                <p class="mt-1 text-[11px] text-slate-400">{{ notif.time }}</p>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="border-t border-slate-100 px-4 py-2.5">
            <button class="text-xs font-medium text-blue-600 hover:text-blue-700 transition-colors">
              Xem tất cả thông báo →
            </button>
          </div>
        </div>
      </Transition>
    </div>

    <!-- ── User avatar / menu ── -->
    <div class="relative">
      <button
        :class="[
          'flex items-center gap-2.5 rounded-2xl border border-white/45 bg-white/45 px-2.5 py-1.5 shadow-sm backdrop-blur-xl transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-blue-500/20',
          userMenuOpen ? 'bg-white/85 shadow-md' : 'hover:bg-white/70',
        ]"
        aria-label="Mở menu tài khoản"
        @click="toggleUserMenu"
      >
        <!-- Avatar -->
        <div class="flex h-8 w-8 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-cyan-500 text-xs font-bold text-white shadow-sm ring-2 ring-white/70">
          {{ authStore.initials || mockUser.initials }}
        </div>
        <!-- Name (ẩn trên mobile nhỏ) -->
        <div class="hidden sm:block text-left">
          <p class="text-[13px] font-semibold text-slate-700 leading-tight">{{ authStore.displayName || mockUser.name }}</p>
          <p class="text-[11px] text-slate-400 leading-tight">{{ authStore.role || mockUser.class }}</p>
        </div>
        <LucideIcons.ChevronDown :size="14" class="hidden sm:block text-slate-400" />
      </button>

      <!-- User Dropdown -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-1 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-1 scale-95"
      >
        <div
          v-if="userMenuOpen"
          class="lg-glass-strong absolute right-0 top-full mt-2 w-[230px] overflow-hidden rounded-[24px]"
        >
          <!-- User info header -->
          <div class="border-b border-slate-100 px-4 py-3">
            <p class="text-sm font-semibold text-slate-800">{{ authStore.displayName || mockUser.name }}</p>
            <p class="text-xs text-slate-500 mt-0.5">{{ authStore.user?.email || mockUser.email }}</p>
            <span class="mt-1.5 inline-flex items-center gap-1 rounded-full bg-blue-100 px-2 py-0.5 text-[10px] font-semibold text-blue-700">
              <LucideIcons.GraduationCap :size="10" />
              Cơ sở {{ authStore.user?.campusId || mockUser.campus }}
            </span>
          </div>

          <!-- Menu items -->
          <div class="p-1.5">
            <router-link
              to="/student/profile"
              class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-600 transition-colors hover:bg-white/70 hover:text-slate-950"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-slate-400" />
              Hồ sơ cá nhân
            </router-link>
            <router-link
              to="/student/tuition"
              class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-600 transition-colors hover:bg-white/70 hover:text-slate-950"
              @click="closeAll"
            >
              <LucideIcons.CreditCard :size="16" class="text-slate-400" />
              Học phí
            </router-link>
          </div>

          <!-- Logout -->
          <div class="border-t border-slate-100 p-1.5">
            <button
              class="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-red-600 transition-colors hover:bg-red-50"
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
