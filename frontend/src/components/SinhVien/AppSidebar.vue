<script setup>
import { useRouter } from 'vue-router'
import {
  PanelLeftClose,
  PanelLeftOpen,
  HelpCircle,
  LogOut,
  GraduationCap,
} from 'lucide-vue-next'
import SidebarMenuGroup from './SidebarMenuGroup.vue'
import SidebarRecentFavorites from '@/components/ui/SidebarRecentFavorites.vue'
import { sinhVienMenuGroups, mockUser } from './data/menuData.js'
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
      collapsed ? 'w-[76px]' : 'w-[220px]',
    ]"
    :style="{
      '--sidebar-glow-1': 'rgba(37, 99, 235, 0.18)',
      '--sidebar-glow-2': 'rgba(8, 145, 178, 0.12)',
      '--sidebar-glow-dark-1': 'rgba(37, 99, 235, 0.10)',
      '--sidebar-glow-dark-2': 'rgba(8, 145, 178, 0.07)',
      '--sidebar-accent': '#2563eb',
      '--sidebar-accent-dark': '#60a5fa',
      '--sidebar-indicator': '#2563eb',
      '--sidebar-focus-ring': 'rgba(37, 99, 235, 0.2)',
      '--active-glow': 'rgba(255, 255, 255, 0.38)',
      '--active-start': '#1d4ed8',
      '--active-mid': '#2563eb',
      '--active-end': '#0891b2',
      '--active-shadow-1': 'rgba(29, 78, 216, 0.35)',
      '--active-shadow-2': 'rgba(8, 145, 178, 0.25)',
    }"
  >
    <div class="pointer-events-none absolute -left-24 top-24 h-56 w-56 rounded-full bg-blue-300/20 dark:bg-blue-500/8 blur-3xl" />
    <div class="pointer-events-none absolute -bottom-20 right-0 h-60 w-60 rounded-full bg-cyan-300/20 dark:bg-cyan-500/8 blur-3xl" />

    <!-- ──────────── LOGO / BRAND ──────────── -->
    <div
      class="relative flex items-center gap-2.5 border-b border-white/45 dark:border-white/10 px-3 py-3 flex-shrink-0"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="flex-shrink-0 flex h-8 w-8 items-center justify-center rounded-xl bg-gradient-to-br from-blue-700 via-blue-600 to-cyan-600 shadow-md shadow-blue-200/40 dark:shadow-blue-500/10">
        <GraduationCap :size="18" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[13px] font-bold leading-tight text-heading dark:text-slate-100">EduLMS</p>
          <p class="text-[10px] text-slate-500 dark:text-slate-400 leading-tight">Sinh viên</p>
        </div>
      </Transition>
    </div>

    <!-- ──────────── MENU SCROLL AREA ──────────── -->
    <nav class="relative flex-1 space-y-0.5 overflow-y-auto overflow-x-visible px-2 py-2 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in sinhVienMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
      <SidebarRecentFavorites :collapsed="collapsed" />
    </nav>

    <!-- ──────────── BOTTOM: HELP + LOGOUT ──────────── -->
    <div class="relative space-y-0.5 border-t border-white/45 dark:border-white/10 px-2 py-2 flex-shrink-0">
      <button
        :title="collapsed ? 'Trợ giúp' : ''"
        :class="[
          'lg-sidebar-item group flex w-full items-center gap-2.5 rounded-xl px-2.5 py-1.5 text-sm text-slate-600 dark:text-slate-400 focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Trợ giúp"
      >
        <HelpCircle :size="16" stroke-width="1.8" class="flex-shrink-0 text-slate-400 dark:text-slate-500 group-hover:text-slate-600 dark:group-hover:text-slate-300 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Trợ giúp</span>
      </button>

      <button
        :title="collapsed ? 'Đăng xuất' : ''"
        :class="[
          'group flex w-full items-center gap-2.5 rounded-xl border border-transparent dark:border-transparent px-2.5 py-1.5 text-sm text-red-600 dark:text-red-400 transition-all duration-200 hover:border-red-100 dark:hover:border-red-600/30 hover:bg-red-50/80 dark:hover:bg-red-600/20 hover:shadow-sm focus:outline-none focus:ring-4 focus:ring-red-500/15',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Đăng xuất"
        @click="logout"
      >
        <LogOut :size="16" stroke-width="1.8" class="flex-shrink-0 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Đăng xuất</span>
      </button>
    </div>

    <!-- ──────────── USER CARD ──────────── -->
    <div
      class="relative border-t border-white/45 dark:border-white/10 p-2 flex-shrink-0"
      :class="collapsed ? 'flex justify-center' : ''"
    >
      <div :class="['lg-nav flex items-center gap-2 rounded-xl p-2', collapsed ? '' : 'w-full']">
        <div class="relative flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-cyan-600 dark:from-blue-600/80 dark:to-cyan-600/80 text-xs font-bold text-white shadow-lg shadow-blue-500/20 dark:shadow-blue-500/10 ring-2 ring-white/70 dark:ring-white/20">
          <span>{{ authStore.initials || mockUser.initials }}</span>
          <span class="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full border-2 border-white dark:border-slate-800 bg-emerald-500" />
        </div>
        <Transition name="fade-slide">
          <div v-if="!collapsed" class="overflow-hidden min-w-0">
            <p class="text-[12px] font-semibold text-slate-800 dark:text-slate-100 truncate leading-tight">
              {{ authStore.displayName || 'Sinh Viên Demo' }}
            </p>
            <p class="text-[10px] font-semibold text-slate-500 dark:text-slate-400 truncate leading-tight">
              {{ mockUser.class }}
            </p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ──────────── TOGGLE BUTTON ──────────── -->
    <button
      class="absolute -right-3.5 top-[56px] z-[60] flex h-6 w-6 items-center justify-center rounded-full border border-white/70 dark:border-white/10 bg-white/90 dark:bg-slate-700/70 text-slate-500 dark:text-slate-400 shadow-lg shadow-slate-900/10 dark:shadow-slate-900/40 backdrop-blur-xl transition-all hover:border-[var(--sidebar-accent)]/40 dark:hover:border-[var(--sidebar-accent-dark)]/30 hover:text-[var(--sidebar-accent)] dark:hover:text-[var(--sidebar-accent-dark)] focus:outline-none focus:ring-[var(--sidebar-focus-ring)]"
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
  background: #e2e8f0;
  border-radius: 999px;
}

:global(.dark) .scrollbar-thin::-webkit-scrollbar-thumb {
  background: #475569;
  border-radius: 999px;
}
</style>
