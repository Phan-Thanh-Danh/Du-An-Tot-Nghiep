<script setup>
import { ref, computed } from 'vue'
import { Search, Bell, X, Menu } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import AppBreadcrumb from './AppBreadcrumb.vue'
import { mockUser, mockNotifications } from './data/menuData.js'

const emit = defineEmits(['toggle-sidebar'])

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
</script>

<template>
  <!-- Overlay để close popups -->
  <div v-if="notifOpen || userMenuOpen" class="fixed inset-0 z-30" @click="closeAll" />

  <header class="relative z-40 flex h-[60px] items-center gap-4 border-b border-slate-100 bg-white px-5 flex-shrink-0">
    <!-- Mobile: toggle sidebar button -->
    <button
      class="flex lg:hidden items-center justify-center rounded-lg p-2 text-slate-500 hover:bg-slate-100 transition-colors"
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
          'flex items-center gap-2 rounded-xl border bg-slate-50 px-3 py-2 transition-all duration-200',
          searchFocused ? 'border-blue-300 bg-white shadow-sm ring-2 ring-blue-100 w-64' : 'border-slate-200 w-48',
        ]"
      >
        <Search :size="15" class="flex-shrink-0 text-slate-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm..."
          class="w-full bg-transparent text-sm text-slate-700 outline-none placeholder:text-slate-400"
          @focus="searchFocused = true"
          @blur="searchFocused = false"
        />
        <button v-if="searchQuery" @click="searchQuery = ''" class="text-slate-400 hover:text-slate-600">
          <X :size="13" />
        </button>
      </div>
    </div>

    <!-- ── Notification bell ── -->
    <div class="relative">
      <button
        :class="[
          'relative flex h-9 w-9 items-center justify-center rounded-xl transition-colors',
          notifOpen ? 'bg-blue-50 text-blue-600' : 'text-slate-500 hover:bg-slate-100',
        ]"
        @click="toggleNotif"
        aria-label="Thông báo"
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
          class="absolute right-0 top-full mt-2 w-[340px] overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-xl ring-1 ring-slate-100"
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
                'flex gap-3 px-4 py-3.5 cursor-pointer transition-colors hover:bg-slate-50',
                !notif.read ? 'bg-blue-50/40' : '',
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
          'flex items-center gap-2.5 rounded-xl px-2.5 py-1.5 transition-colors',
          userMenuOpen ? 'bg-slate-100' : 'hover:bg-slate-100',
        ]"
        @click="toggleUserMenu"
      >
        <!-- Avatar -->
        <div class="flex h-8 w-8 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 text-white text-xs font-bold shadow-sm">
          {{ mockUser.initials }}
        </div>
        <!-- Name (ẩn trên mobile nhỏ) -->
        <div class="hidden sm:block text-left">
          <p class="text-[13px] font-semibold text-slate-700 leading-tight">{{ mockUser.name }}</p>
          <p class="text-[11px] text-slate-400 leading-tight">{{ mockUser.class }}</p>
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
          class="absolute right-0 top-full mt-2 w-[220px] overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-xl"
        >
          <!-- User info header -->
          <div class="border-b border-slate-100 px-4 py-3">
            <p class="text-sm font-semibold text-slate-800">{{ mockUser.name }}</p>
            <p class="text-xs text-slate-500 mt-0.5">{{ mockUser.email }}</p>
            <span class="mt-1.5 inline-flex items-center gap-1 rounded-full bg-blue-100 px-2 py-0.5 text-[10px] font-semibold text-blue-700">
              <LucideIcons.GraduationCap :size="10" />
              {{ mockUser.campus }}
            </span>
          </div>

          <!-- Menu items -->
          <div class="p-1.5">
            <router-link
              to="/student/profile"
              class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-600 hover:bg-slate-100 transition-colors"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-slate-400" />
              Hồ sơ cá nhân
            </router-link>
            <router-link
              to="/student/tuition"
              class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-600 hover:bg-slate-100 transition-colors"
              @click="closeAll"
            >
              <LucideIcons.CreditCard :size="16" class="text-slate-400" />
              Học phí
            </router-link>
          </div>

          <!-- Logout -->
          <div class="border-t border-slate-100 p-1.5">
            <button class="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-red-500 hover:bg-red-50 transition-colors">
              <LucideIcons.LogOut :size="16" />
              Đăng xuất
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </header>
</template>

<script>
import * as LucideIcons from 'lucide-vue-next'
export default { components: { ...LucideIcons } }
</script>
