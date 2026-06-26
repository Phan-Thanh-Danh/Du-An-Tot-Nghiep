<script setup>
import { useRouter } from 'vue-router'
import {
  PanelLeftClose,
  PanelLeftOpen,
  HelpCircle,
  LogOut,
  BookOpen,
} from 'lucide-vue-next'
import SidebarMenuGroup from '../SinhVien/SidebarMenuGroup.vue'
import SidebarRecentFavorites from '@/components/ui/SidebarRecentFavorites.vue'
import { contentBoardMenuGroups, mockContentBoard } from './data/menuData.js'
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
      collapsed ? 'w-[68px]' : 'w-[260px]',
    ]"
    :style="{
      '--sidebar-glow-1': 'rgba(8, 145, 178, 0.10)',
      '--sidebar-glow-2': 'rgba(6, 182, 212, 0.07)',
      '--sidebar-glow-dark-1': 'rgba(8, 145, 178, 0.07)',
      '--sidebar-glow-dark-2': 'rgba(6, 182, 212, 0.05)',
      '--sidebar-accent': '#0891b2',
      '--sidebar-accent-dark': '#22d3ee',
      '--sidebar-indicator': '#0891b2',
      '--sidebar-focus-ring': 'rgba(8, 145, 178, 0.2)',
      '--active-glow': 'rgba(255, 255, 255, 0.22)',
      '--active-start': '#0e7490',
      '--active-mid': '#0891b2',
      '--active-end': '#06b6d4',
      '--active-shadow-1': 'rgba(8, 145, 178, 0.20)',
      '--active-shadow-2': 'rgba(6, 182, 212, 0.14)',
    }"
  >
    <!-- LOGO / BRAND -->
    <div
      class="border-card relative flex flex-shrink-0 items-center gap-2.5 border-b px-3 py-2.5"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="lg-sidebar-brand-mark flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-[var(--radius-lg)]">
        <BookOpen :size="18" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[13px] font-semibold leading-tight text-heading">EduLMS</p>
          <p class="text-[10px] leading-tight text-muted">Hội đồng QL Nội dung</p>
        </div>
      </Transition>
    </div>

    <!-- MENU SCROLL AREA -->
    <nav class="relative flex-1 overflow-y-auto overflow-x-visible px-2 py-2 space-y-0.5 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in contentBoardMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
      <SidebarRecentFavorites :collapsed="collapsed" />
    </nav>

    <!-- BOTTOM: HELP + LOGOUT -->
    <div class="flex-shrink-0 border-t border-card px-2 py-2 space-y-0.5">
      <button
        class="sidebar-item group relative flex w-full items-center gap-2.5 rounded-[var(--radius-md)] px-3 py-2 text-sm transition-all duration-200"
        :class="collapsed ? 'justify-center' : ''"
      >
        <HelpCircle :size="18" class="shrink-0 text-muted group-hover:text-heading transition-colors" />
        <Transition name="fade-slide">
          <span v-if="!collapsed" class="text-[13px] font-medium text-muted group-hover:text-heading">Trợ giúp</span>
        </Transition>
      </button>
      <button
        @click="logout"
        class="sidebar-item group relative flex w-full items-center gap-2.5 rounded-[var(--radius-md)] px-3 py-2 text-sm transition-all duration-200"
        :class="collapsed ? 'justify-center' : ''"
      >
        <LogOut :size="18" class="shrink-0 text-muted group-hover:text-danger transition-colors" />
        <Transition name="fade-slide">
          <span v-if="!collapsed" class="text-[13px] font-medium text-muted group-hover:text-danger">Đăng xuất</span>
        </Transition>
      </button>
    </div>

    <!-- COLLAPSE BUTTON -->
    <button
      @click="emit('toggle')"
      class="absolute bottom-2 -right-3 z-10 flex h-6 w-6 items-center justify-center rounded-full border border-card bg-[var(--lg-surface-glass-strong)] shadow-sm text-muted hover:text-heading transition-all"
    >
      <PanelLeftClose v-if="!collapsed" :size="14" />
      <PanelLeftOpen v-else :size="14" />
    </button>
  </aside>
</template>
