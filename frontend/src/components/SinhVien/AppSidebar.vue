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
      'lg-sidebar relative flex h-full flex-col overflow-hidden transition-all duration-300 ease-in-out select-none',
      collapsed ? 'w-[72px]' : 'w-[260px]',
    ]"
  >
    <div class="pointer-events-none absolute -left-24 top-24 h-56 w-56 rounded-full bg-cyan-300/20 blur-3xl" />
    <div class="pointer-events-none absolute -bottom-20 right-0 h-60 w-60 rounded-full bg-blue-300/20 blur-3xl" />
    <!-- ──────────── LOGO / BRAND ──────────── -->
    <div
      class="relative flex items-center gap-3 border-b border-white/45 px-4 py-4"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="flex-shrink-0 flex h-10 w-10 items-center justify-center rounded-2xl border border-white/40 bg-gradient-to-br from-blue-700 via-blue-600 to-cyan-500 shadow-lg shadow-blue-500/24">
        <GraduationCap :size="20" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[15px] font-bold leading-tight text-slate-950">EduLMS</p>
          <p class="text-[11px] text-slate-500 leading-tight">Cổng học sinh</p>
        </div>
      </Transition>
    </div>

    <!-- ──────────── MENU SCROLL AREA ──────────── -->
    <nav class="relative flex-1 space-y-1 overflow-y-auto overflow-x-visible px-2 py-3 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in sinhVienMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
    </nav>

    <!-- ──────────── BOTTOM: HELP + LOGOUT ──────────── -->
    <div class="relative space-y-1 border-t border-white/45 px-2 py-3">
      <!-- Help -->
      <button
        :title="collapsed ? 'Trợ giúp' : ''"
        :class="[
          'lg-sidebar-item group flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm text-slate-600 focus:outline-none focus:ring-4 focus:ring-blue-500/20',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Trợ giúp"
      >
        <HelpCircle :size="18" stroke-width="1.8" class="flex-shrink-0 text-slate-400 group-hover:text-slate-600 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Trợ giúp</span>
      </button>

      <!-- Logout -->
      <button
        :title="collapsed ? 'Đăng xuất' : ''"
        :class="[
          'group flex w-full items-center gap-3 rounded-xl border border-transparent px-3 py-2.5 text-sm text-red-600 transition-all duration-200 hover:border-red-100 hover:bg-red-50/80 hover:shadow-sm focus:outline-none focus:ring-4 focus:ring-red-500/15',
          collapsed ? 'justify-center' : '',
        ]"
        aria-label="Đăng xuất"
        @click="logout"
      >
        <LogOut :size="18" stroke-width="1.8" class="flex-shrink-0 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Đăng xuất</span>
      </button>
    </div>

    <!-- ──────────── USER CARD ──────────── -->
    <div
      class="relative border-t border-white/45 p-3"
      :class="collapsed ? 'flex justify-center' : ''"
    >
      <div :class="['lg-nav flex items-center gap-3 rounded-2xl p-2', collapsed ? '' : '']">
        <!-- Avatar -->
        <div class="flex-shrink-0 flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-cyan-500 text-xs font-bold text-white shadow-lg shadow-blue-500/20 ring-2 ring-white/70">
          {{ authStore.initials || mockUser.initials }}
        </div>
        <Transition name="fade-slide">
          <div v-if="!collapsed" class="overflow-hidden min-w-0">
            <p class="text-[13px] font-semibold text-slate-700 truncate leading-tight">
              {{ authStore.displayName || mockUser.name }}
            </p>
            <p class="text-[11px] text-slate-400 truncate leading-tight">
              {{ authStore.user?.email || mockUser.studentId }}
            </p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ──────────── TOGGLE BUTTON ──────────── -->
    <button
      class="absolute -right-3 top-[60px] z-10 flex h-7 w-7 items-center justify-center rounded-full border border-white/70 bg-white/90 text-slate-500 shadow-lg shadow-slate-900/10 backdrop-blur-xl transition-all hover:border-blue-300 hover:text-blue-700 focus:outline-none focus:ring-4 focus:ring-blue-500/20"
      :title="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      :aria-label="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      @click="emit('toggle')"
    >
      <component :is="collapsed ? PanelLeftOpen : PanelLeftClose" :size="13" stroke-width="2" />
    </button>
  </aside>
</template>

<style scoped>
/* Smooth fade + slide cho label text */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-6px);
}

/* Scrollbar nhỏ cho nav */
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
</style>
