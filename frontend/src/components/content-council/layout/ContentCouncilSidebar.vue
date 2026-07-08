<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  BookOpen,
  CircleHelp,
  ClipboardCheck,
  LogOut,
  ChevronLeft,
  ChevronRight,
  User,
  GraduationCap,
  PanelLeftClose,
  PanelLeftOpen
} from 'lucide-vue-next'

const props = defineProps({
  isCollapsed: {
    type: Boolean,
    default: false
  },
  isMobileDrawerOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:isCollapsed', 'update:isMobileDrawerOpen'])

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const menuItems = [
  {
    name: 'Nội dung môn học',
    icon: BookOpen,
    route: 'content-council-subjects',
    path: '/content-council/subjects',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Ngân hàng câu hỏi',
    icon: CircleHelp,
    route: 'content-council-question-bank',
    path: '/content-council/question-bank',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Quiz / Đề kiểm tra',
    icon: ClipboardCheck,
    route: 'content-council-quizzes',
    path: '/content-council/quizzes',
    disabled: false,
    tooltip: ''
  }
]

const toggleCollapse = () => {
  emit('update:isCollapsed', !props.isCollapsed)
}

const closeDrawer = () => {
  emit('update:isMobileDrawerOpen', false)
}

const navigate = (item) => {
  if (item.disabled) return
  router.push(item.path)
  closeDrawer()
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <!-- Sidebar Container -->
  <aside
    :class="[
      'lg-sidebar relative flex h-full flex-col transition-all duration-300 ease-in-out select-none',
      isCollapsed ? 'w-[68px]' : 'w-[240px]',
    ]"
    :style="{
      '--sidebar-glow-1': 'rgba(37, 99, 235, 0.10)',
      '--sidebar-glow-2': 'rgba(59, 130, 246, 0.07)',
      '--sidebar-glow-dark-1': 'rgba(37, 99, 235, 0.07)',
      '--sidebar-glow-dark-2': 'rgba(59, 130, 246, 0.05)',
      '--sidebar-accent': '#2563eb',
      '--sidebar-accent-dark': '#60a5fa',
      '--sidebar-indicator': '#2563eb',
      '--sidebar-focus-ring': 'rgba(37, 99, 235, 0.2)',
      '--active-glow': 'rgba(255, 255, 255, 0.22)',
      '--active-start': '#1d4ed8',
      '--active-mid': '#2563eb',
      '--active-end': '#0891b2',
      '--active-shadow-1': 'rgba(30, 64, 175, 0.20)',
      '--active-shadow-2': 'rgba(37, 99, 235, 0.14)',
    }"
  >
    <!-- LOGO / BRAND -->
    <div
      class="border-card relative flex flex-shrink-0 items-center gap-2.5 border-b px-3 py-2.5"
      :class="isCollapsed ? 'justify-center px-2' : ''"
    >
      <div class="lg-sidebar-brand-mark flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-(--radius-lg)">
        <GraduationCap :size="18" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!isCollapsed" class="overflow-hidden">
          <p class="text-[13px] font-semibold leading-tight text-heading">EduLMS</p>
          <p class="text-[10px] leading-tight text-muted">Quản lý nội dung</p>
        </div>
      </Transition>
    </div>

    <!-- MENU SCROLL AREA -->
    <nav class="relative flex-1 overflow-y-auto overflow-x-visible px-2 py-2 space-y-0.5 scrollbar-thin">
      <button
        v-for="item in menuItems"
        :key="item.name"
        @click="navigate(item)"
        class="lg-sidebar-item group relative flex w-full items-center gap-2.5 rounded-(--radius-md) px-2.5 py-2 text-[13px] text-label transition-all hover:text-heading focus:outline-none focus:ring-(--sidebar-focus-ring)"
        :class="[
          isCollapsed ? 'justify-center' : '',
          route.name === item.route ? 'active' : '',
          item.disabled ? 'opacity-50 cursor-not-allowed hover:bg-transparent' : 'cursor-pointer'
        ]"
        :title="isCollapsed ? (item.tooltip || item.name) : item.tooltip"
      >
        <component 
          :is="item.icon" 
          class="flex-shrink-0 transition-colors"
          :class="route.name === item.route ? 'text-white' : 'text-muted group-hover:text-heading'" 
          :size="18" 
          stroke-width="1.8" 
        />
        <span v-if="!isCollapsed" class="text-[13px] font-medium leading-tight">{{ item.name }}</span>
      </button>
    </nav>

    <!-- BOTTOM: LOGOUT -->
    <div class="border-card relative flex-shrink-0 space-y-0.5 border-t px-2 py-2">
      <button
        :title="isCollapsed ? 'Đăng xuất' : ''"
        :class="[
          'group flex w-full items-center gap-2.5 rounded-(--radius-md) border border-transparent px-2.5 py-1.5 text-[13px] text-(--color-danger-text) transition-all duration-200 hover:border-card hover:bg-(--color-danger-bg) focus:outline-none focus:ring-4 focus:ring-(--focus-ring)',
          isCollapsed ? 'justify-center' : '',
        ]"
        aria-label="Đăng xuất"
        @click="handleLogout"
      >
        <LogOut :size="16" stroke-width="1.8" class="flex-shrink-0 transition-colors" />
        <span v-if="!isCollapsed" class="text-[13px] font-medium">Đăng xuất</span>
      </button>
    </div>

    <!-- USER CARD -->
    <div
      class="border-card relative flex-shrink-0 border-t p-2"
      :class="isCollapsed ? 'flex justify-center' : ''"
    >
      <div :class="['lg-nav flex items-center gap-2 rounded-xl p-2', isCollapsed ? '' : 'w-full']">
        <div class="lg-sidebar-avatar relative flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold ring-1 ring-(--border-card)">
          <span v-if="authStore.initials">{{ authStore.initials }}</span>
          <User v-else :size="16" />
          <span class="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full border-2 border-(--surface-sidebar) bg-(--sidebar-indicator)" />
        </div>
        <Transition name="fade-slide">
          <div v-if="!isCollapsed" class="overflow-hidden min-w-0">
            <p class="truncate text-[12px] font-medium leading-tight text-heading">
              {{ authStore.displayName }}
            </p>
            <p class="truncate text-[10px] leading-tight text-muted">
              Hội đồng nội dung
            </p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- TOGGLE BUTTON -->
    <button
      class="lg-sidebar-toggle absolute -right-3 top-[52px] z-[60] hidden md:flex h-6 w-6 items-center justify-center rounded-full transition-all focus:outline-none focus:ring-(--sidebar-focus-ring)"
      :title="isCollapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      :aria-label="isCollapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      @click="toggleCollapse"
    >
      <component :is="isCollapsed ? PanelLeftOpen : PanelLeftClose" :size="13" stroke-width="2" />
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
}
</style>
