<script setup>
import { useRouter } from 'vue-router'
import {
  PanelLeftClose,
  PanelLeftOpen,
  HelpCircle,
  LogOut,
  GraduationCap,
} from 'lucide-vue-next'
import SidebarMenuGroup from '../SinhVien/SidebarMenuGroup.vue'
import SidebarRecentFavorites from '@/components/ui/SidebarRecentFavorites.vue'
import { giangVienMenuGroups, mockTeacher } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'

defineProps({
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])

const router = useRouter()
const authStore = useAuthStore()

function logout() {
  authStore.logout()
  router.replace('/login')
}
</script>

<template>
  <aside
    :class="[
      'lg-sidebar relative flex h-full flex-col transition-all duration-300 ease-in-out select-none',
      collapsed ? 'w-[68px]' : 'w-[240px]',
    ]"
    :style="{
      '--sidebar-glow-1': 'rgba(37, 99, 235, 0.10)',
      '--sidebar-glow-2': 'rgba(8, 145, 178, 0.07)',
      '--sidebar-glow-dark-1': 'rgba(37, 99, 235, 0.07)',
      '--sidebar-glow-dark-2': 'rgba(8, 145, 178, 0.05)',
      '--sidebar-accent': '#2563eb',
      '--sidebar-accent-dark': '#60a5fa',
      '--sidebar-indicator': '#2563eb',
      '--sidebar-focus-ring': 'rgba(37, 99, 235, 0.2)',
      '--active-glow': 'rgba(255, 255, 255, 0.22)',
      '--active-start': '#1d4ed8',
      '--active-mid': '#2563eb',
      '--active-end': '#0891b2',
      '--active-shadow-1': 'rgba(29, 78, 216, 0.20)',
      '--active-shadow-2': 'rgba(8, 145, 178, 0.14)',
    }"
  >
    <!-- ──────────── LOGO / BRAND ──────────── -->
    <div
      class="border-card relative flex flex-shrink-0 items-center gap-2.5 border-b px-3 py-2.5"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="lg-sidebar-brand-mark flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-[var(--radius-lg)]">
        <GraduationCap :size="18" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[13px] font-semibold leading-tight text-heading">EduLMS</p>
          <p class="text-[10px] leading-tight text-muted">Giảng viên</p>
        </div>
      </Transition>
    </div>

    <!-- ──────────── MENU SCROLL AREA ──────────── -->
    <nav class="relative flex-1 space-y-0.5 overflow-y-auto overflow-x-visible px-2 py-2 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in giangVienMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
      <SidebarRecentFavorites :collapsed="collapsed" />
    </nav>

    <!-- ──────────── BOTTOM: HELP + LOGOUT ──────────── -->
    <div class="border-card relative flex-shrink-0 space-y-0.5 border-t px-2 py-2">
      <button
        :title="collapsed ? 'Trợ giúp' : ''"
        :class="[
          'lg-sidebar-item group flex w-full items-center gap-2.5 rounded-[var(--radius-md)] px-2.5 py-1.5 text-[13px] text-label focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Trợ giúp"
      >
        <HelpCircle :size="16" stroke-width="1.8" class="flex-shrink-0 text-muted transition-colors group-hover:text-label" />
        <span v-if="!collapsed" class="text-[13px] font-medium">Trợ giúp</span>
      </button>

      <button
        :title="collapsed ? 'Đăng xuất' : ''"
        :class="[
          'group flex w-full items-center gap-2.5 rounded-[var(--radius-md)] border border-transparent px-2.5 py-1.5 text-[13px] text-[var(--color-danger-text)] transition-all duration-200 hover:border-card hover:bg-[var(--color-danger-bg)] focus:outline-none focus:ring-4 focus:ring-[var(--focus-ring)]',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Đăng xuất"
        @click="logout"
      >
        <LogOut :size="16" stroke-width="1.8" class="flex-shrink-0 transition-colors" />
        <span v-if="!collapsed" class="text-[13px] font-medium">Đăng xuất</span>
      </button>
    </div>

    <!-- ──────────── USER CARD ──────────── -->
    <div
      class="border-card relative flex-shrink-0 border-t p-2"
      :class="collapsed ? 'flex justify-center' : ''"
    >
      <div :class="['lg-nav flex items-center gap-2 rounded-xl p-2', collapsed ? '' : 'w-full']">
        <div class="lg-sidebar-avatar relative flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold ring-1 ring-[var(--border-card)]">
          <span>{{ authStore.initials || mockTeacher.initials }}</span>
          <span class="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full border-2 border-[var(--surface-sidebar)] bg-[var(--sidebar-indicator)]" />
        </div>
        <Transition name="fade-slide">
          <div v-if="!collapsed" class="overflow-hidden min-w-0">
            <p class="truncate text-[12px] font-medium leading-tight text-heading">
              {{ authStore.displayName || 'Giảng Viên Demo' }}
            </p>
            <p class="truncate text-[10px] font-medium leading-tight text-muted">
              {{ authStore.user?.email || mockTeacher.teacherId }}
            </p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ──────────── TOGGLE BUTTON ──────────── -->
    <button
      class="lg-sidebar-toggle absolute -right-3 top-[52px] z-[60] flex h-6 w-6 items-center justify-center rounded-full transition-all focus:outline-none focus:ring-[var(--sidebar-focus-ring)]"
      :title="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      :aria-label="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      @click="emit('toggle')"
    >
      <component :is="collapsed ? PanelLeftOpen : PanelLeftClose" :size="13" stroke-width="2" />
    </button>
  </aside>
</template>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-6px);
}
.scrollbar-thin::-webkit-scrollbar {
  width: 3px;
}
.scrollbar-thin::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
  background: var(--border-default);
  border-radius: 999px;
}

:global(.dark) .scrollbar-thin::-webkit-scrollbar-thumb {
  background: var(--border-default);
  border-radius: 999px;
}
</style>
